using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using Cysharp.Text;
using HarmonyLib;
using Kobold;
using Platform.IO;
using Qud.UI;
using RoslynCSharp.Compiler;
using Steamworks;
using UnityEngine;
using XRL.Collections;
using XRL.Rules;
using XRL.UI;

namespace XRL;

[HasModSensitiveStaticCache]
public class ModInfo : IComparable<ModInfo>
{
	public const uint DEP_WEIGHT_REQUIRED = 10000000u;

	public const uint DEP_WEIGHT_OPTIONAL = 1000u;

	public const uint DEP_WEIGHT_HINT = 1u;

	/// <summary>The system location of this mod's folder.</summary>
	public string Path;

	/// <summary>A unique identifier for this mod.</summary>
	/// <seealso cref="M:XRL.ModManager.RegisterMod(XRL.ModInfo)" />
	public string ID = "";

	/// <summary>The load origin of this mod, e.g. the local mods folder or steam workshop.</summary>
	public ModSource Source;

	/// <inheritdoc cref="F:XRL.ModInfo.Path" />
	public DirectoryInfo Directory;

	/// <summary>The final compiled assembly of all <see cref="!:ScriptFiles" />.</summary>
	public Assembly Assembly;

	/// <summary>A harmony instance using <see cref="F:XRL.ModInfo.ID" />.</summary>
	public Harmony Harmony;

	/// <summary>A list of all visible files within <see cref="!:FileDirectories" />.</summary>
	public Rack<ModFile> Files = new Rack<ModFile>();

	/// <summary>All resolved dependencies mapped with weights.</summary>
	public Dictionary<ModInfo, uint> Dependencies = new Dictionary<ModInfo, uint>();

	/// <summary>The load order of this mod, lower values will be prioritized over higher ones.</summary>
	/// <seealso cref="M:XRL.ModInfo.CompareTo(XRL.ModInfo)" />
	public int LoadOrder = int.MinValue;

	public bool Active;

	/// <value><c>true</c> if this mod contains any <see cref="!:ScriptFiles" />; otherwise, <c>false</c>.</value>
	public bool IsScripting;

	/// <value><c>true</c> if this mod is missing a required dependency; otherwise, <c>false</c>.</value>
	public bool IsMissingDependency;

	/// <value><c>true</c> if this mod is has a remote version greater than its local manifest; otherwise, <c>false</c>.</value>
	public bool HasUpdate;

	/// <summary>The total size in bytes of all visible files within <see cref="F:XRL.ModInfo.Directory" />.</summary>
	public long Size;

	/// <summary>Version available for update.</summary>
	public Version RemoteVersion;

	/// <summary>User configured state which is stored in ModSettings.json.</summary>
	/// <seealso cref="M:XRL.ModInfo.LoadSettings" />
	public ModSettings Settings;

	/// <summary>A manifest.json read from the mod's root <see cref="F:XRL.ModInfo.Directory" />.</summary>
	/// <seealso cref="M:XRL.ModInfo.ReadConfigurations" />
	public ModManifest Manifest = new ModManifest();

	/// <summary>A workshop.json read and written to the mod's root <see cref="F:XRL.ModInfo.Directory" />.</summary>
	/// <seealso cref="M:XRL.ModInfo.ReadConfigurations" />
	public SteamWorkshopInfo WorkshopInfo;

	/// <summary>A modconfig.json read from the mod's root <see cref="F:XRL.ModInfo.Directory" />.</summary>
	/// <remarks>This might be obsoleted in the future with a per-file texture configuration like <c>"cute_snapjaw.png.cfg"</c>.</remarks>
	/// <seealso cref="M:XRL.ModInfo.ReadConfigurations" />
	public TextureConfiguration TextureConfiguration = new TextureConfiguration();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<string, Sprite> spriteByPath = new Dictionary<string, Sprite>();

	/// <summary>
	/// Gets or sets a value that determines if this mod is enabled.
	/// </summary>
	/// <value>
	/// <c>true</c> if the mod has been approved, enabled, and successfully compiled; otherwise, <c>false</c>.
	/// </value>
	public bool IsEnabled
	{
		get
		{
			return State == ModState.Enabled;
		}
		set
		{
			Settings.Enabled = value;
		}
	}

	[Obsolete]
	public bool IsApproved => true;

	/// <summary>
	/// An enum representation of the mod's current state: lacks approval, failed to compile, enabled, disabled.
	/// </summary>
	public ModState State
	{
		get
		{
			if (IsScripting && !Options.AllowCSMods)
			{
				return ModState.Disabled;
			}
			if (!Settings.Enabled)
			{
				return ModState.Disabled;
			}
			if (IsMissingDependency)
			{
				return ModState.MissingDependency;
			}
			if (Settings.Failed)
			{
				return ModState.Failed;
			}
			return ModState.Enabled;
		}
	}

	/// <summary><see cref="P:XRL.ModInfo.DisplayTitle" /> stripped of ANSI formatting.</summary>
	public string DisplayTitleStripped => ConsoleLib.Console.ColorUtility.StripFormatting(DisplayTitle);

	/// <summary>
	/// A display title for the <see cref="T:Qud.UI.ModManagerUI" />.
	/// </summary>
	public string DisplayTitle => Manifest.Title.Coalesce(ID);

	public ModInfo(string Path, string ID = null, ModSource Source = ModSource.Unknown, bool Initialize = false)
	{
		Directory = new DirectoryInfo(Path);
		this.Path = Directory.FullName;
		this.ID = ID;
		this.Source = Source;
		if (Initialize)
		{
			this.Initialize();
		}
	}

	/// <summary>
	/// Initialize the mod info instance based on configuration and user settings.
	/// </summary>
	public void Initialize()
	{
		if (Directory.Exists)
		{
			ReadConfigurations();
			LoadSettings();
		}
	}

	public void ResolveDependencies()
	{
		Dictionary<string, ModInfo> modMap = ModManager.ModMap;
		string value;
		string key;
		if (!Manifest.Dependencies.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, string> dependency in Manifest.Dependencies)
			{
				dependency.Deconstruct(out value, out key);
				string text = value;
				string text2 = key;
				if (modMap.TryGetValue(text, out var value2) && value2.Manifest.Version.EqualsSemantic(text2))
				{
					Logger.buildLog.Info("[Required] " + ID + " -> " + text + " (" + text2 + "): OK");
					if (!Dependencies.TryGetValue(value2, out var value3) || value3 < 10000000)
					{
						Dependencies[value2] = 10000000u;
					}
				}
				else
				{
					Logger.buildLog.Info("[Required] " + ID + " -> " + text + " (" + text2 + "): MISSING");
					IsMissingDependency = true;
				}
			}
		}
		if (!Manifest.Directories.IsNullOrEmpty())
		{
			ModDirectory[] directories = Manifest.Directories;
			foreach (ModDirectory modDirectory in directories)
			{
				if (modDirectory.Dependencies.IsNullOrEmpty())
				{
					continue;
				}
				foreach (KeyValuePair<string, string> dependency2 in modDirectory.Dependencies)
				{
					dependency2.Deconstruct(out key, out value);
					string text3 = key;
					string text4 = value;
					if (modMap.TryGetValue(text3, out var value4) && value4.Manifest.Version.EqualsSemantic(text4))
					{
						Logger.buildLog.Info("[Optional] " + ID + " -> " + text3 + " (" + text4 + "): OK");
						if (!Dependencies.TryGetValue(value4, out var value5) || value5 < 1000)
						{
							Dependencies[value4] = 1000u;
						}
					}
					else
					{
						Logger.buildLog.Info("[Optional] " + ID + " -> " + text3 + " (" + text4 + "): MISSING");
					}
				}
			}
		}
		string[] loadBefore;
		if (!Manifest.LoadBefore.IsNullOrEmpty())
		{
			loadBefore = Manifest.LoadBefore;
			foreach (string key2 in loadBefore)
			{
				if (modMap.TryGetValue(key2, out var value6) && (!value6.Dependencies.TryGetValue(this, out var value7) || value7 < 1))
				{
					value6.Dependencies[this] = 1u;
				}
			}
		}
		if (Manifest.LoadAfter.IsNullOrEmpty())
		{
			return;
		}
		loadBefore = Manifest.LoadAfter;
		foreach (string key3 in loadBefore)
		{
			if (modMap.TryGetValue(key3, out var value8) && (!Dependencies.TryGetValue(value8, out var value9) || value9 < 1))
			{
				Dependencies[value8] = 1u;
			}
		}
	}

	public void AssertDependencies()
	{
		foreach (var (modInfo2, num2) in Dependencies)
		{
			if (!modInfo2.Active && num2 >= 10000000)
			{
				Logger.buildLog.Error("Required dependency '" + modInfo2.ID + "' is not loaded");
				IsMissingDependency = true;
			}
		}
	}

	/// <summary>
	/// Read mod root configuration files: manifest.json, config.json, workshop.json, modconfig.json.
	/// </summary>
	public void ReadConfigurations()
	{
		foreach (FileInfo item in Directory.EnumerateFiles())
		{
			try
			{
				ReadConfiguration(item);
			}
			catch (Exception msg)
			{
				Error(msg);
			}
		}
		ID = Regex.Replace(Manifest.ID ?? ID, "[^\\w ]", "");
	}

	private void ReadConfiguration(FileInfo File)
	{
		switch (File.Name.ToLower())
		{
		case "manifest.json":
			ModManager.JsonSerializer.Populate(File.FullName, Manifest);
			if (Manifest.LoadOrder.HasValue)
			{
				Warn("Mod defining manual load order, please convert it to use the Dependencies field.");
			}
			break;
		case "config.json":
		{
			ModManifest modManifest = ModManager.JsonSerializer.Deserialize<ModManifest>(File.FullName);
			if (Manifest.ID == null)
			{
				Manifest.ID = modManifest.ID;
			}
			Warn("Mod using config.json, please convert to manifest.json and check out https://wiki.cavesofqud.com/wiki/Modding:Mod_Configuration for other options to set");
			break;
		}
		case "workshop.json":
			WorkshopInfo = ModManager.JsonSerializer.Deserialize<SteamWorkshopInfo>(File.FullName);
			if (Manifest.Tags == null)
			{
				Manifest.Tags = WorkshopInfo.Tags;
			}
			if (Manifest.PreviewImage == null)
			{
				Manifest.PreviewImage = WorkshopInfo.ImagePath;
			}
			if (Manifest.Title == null && WorkshopInfo.Title != null)
			{
				Manifest.Title = ConsoleLib.Console.ColorUtility.EscapeFormatting(WorkshopInfo.Title);
			}
			break;
		case "modconfig.json":
			ModManager.JsonSerializer.Populate(File.FullName, TextureConfiguration);
			break;
		}
	}

	/// <summary>
	/// Load settings by ID from <see cref="F:XRL.ModManager.ModSettingsMap" />.
	/// </summary>
	public void LoadSettings()
	{
		if (!ModManager.ModSettingsMap.TryGetValue(ID, out Settings))
		{
			Settings = new ModSettings();
			ModManager.ModSettingsMap[ID] = Settings;
		}
		Settings.Title = DisplayTitle;
	}

	/// <summary>
	/// Check for script and XML files within the mod directory and sort them into their respective lists.
	/// </summary>
	public void InitializeFiles()
	{
		IsScripting = false;
		string fullName = Directory.FullName;
		if (Manifest.Directories.IsNullOrEmpty())
		{
			Logger.buildLog.Info("Loading path: " + Platform.IO.Path.DirectorySeparatorChar);
			InitializeFiles(Directory);
			return;
		}
		Rack<DirectoryInfo> rack = new Rack<DirectoryInfo>();
		Version version = XRLGame.MarketingVersion;
		Version version2 = XRLGame.CoreVersion;
		Dictionary<string, ModInfo> modMap = ModManager.ModMap;
		ModDirectory[] directories = Manifest.Directories;
		foreach (ModDirectory modDirectory in directories)
		{
			if (modDirectory.Paths.IsNullOrEmpty() || !version.EqualsSemantic(modDirectory.Version) || !version2.EqualsSemantic(modDirectory.Build))
			{
				continue;
			}
			GameOption.RequiresSpec options = modDirectory.Options;
			if (options != null && !options.RequirementsMet)
			{
				continue;
			}
			string value;
			string key;
			if (!modDirectory.Dependencies.IsNullOrEmpty())
			{
				bool flag = false;
				foreach (KeyValuePair<string, string> dependency in modDirectory.Dependencies)
				{
					dependency.Deconstruct(out value, out key);
					string key2 = value;
					string text = key;
					if (!modMap.TryGetValue(key2, out var value2) || !value2.Manifest.Version.EqualsSemantic(text) || !value2.IsEnabled || value2.LoadOrder > LoadOrder)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			if (!modDirectory.Exclusions.IsNullOrEmpty())
			{
				bool flag2 = false;
				foreach (KeyValuePair<string, string> exclusion in modDirectory.Exclusions)
				{
					exclusion.Deconstruct(out key, out value);
					string key3 = key;
					string text2 = value;
					if (modMap.TryGetValue(key3, out var value3) && value3.Manifest.Version.EqualsSemantic(text2) && value3.IsEnabled)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					continue;
				}
			}
			string[] paths = modDirectory.Paths;
			foreach (string path in paths)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(Platform.IO.Path.Join(Path, path));
				string fullName2 = directoryInfo.FullName;
				if (!directoryInfo.Exists || !fullName2.StartsWith(fullName))
				{
					continue;
				}
				bool flag3 = true;
				foreach (DirectoryInfo item in rack)
				{
					if (fullName2.StartsWith(item.FullName))
					{
						flag3 = false;
						break;
					}
				}
				if (!flag3)
				{
					continue;
				}
				rack.Add(directoryInfo);
				for (int num = rack.Count - 2; num >= 0; num--)
				{
					if (rack[num].FullName.StartsWith(fullName2))
					{
						rack.RemoveAt(num);
					}
				}
			}
		}
		foreach (DirectoryInfo item2 in rack)
		{
			string text3 = Platform.IO.Path.GetRelativePath(fullName, item2.FullName);
			if (text3 == ".")
			{
				text3 = Platform.IO.Path.DirectorySeparatorChar.GetString();
			}
			Logger.buildLog.Info("Loading path: " + text3);
			InitializeFiles(item2);
		}
	}

	private void InitializeFiles(DirectoryInfo Directory)
	{
		foreach (FileSystemInfo item in Directory.EnumerateFileSystemInfos())
		{
			if ((item.Attributes & FileAttributes.Hidden) > FileAttributes.None)
			{
				continue;
			}
			if ((item.Attributes & FileAttributes.Directory) > FileAttributes.None)
			{
				InitializeFiles((DirectoryInfo)item);
				continue;
			}
			FileInfo fileInfo = (FileInfo)item;
			ModFile modFile = new ModFile(this, fileInfo);
			Size += fileInfo.Length;
			Files.Add(modFile);
			if (!IsScripting && modFile.Type == ModFileType.CSharp)
			{
				IsScripting = true;
			}
		}
	}

	/// <summary>
	/// Display a popup to the user with failure information and ask to retry.
	/// Clears <see cref="F:XRL.ModSettings.Failed" /> on success or no errors.
	/// </summary>
	public void ConfirmFailure()
	{
		int count = Settings.Errors.Count;
		if (count > 0)
		{
			LogToClipboard();
			string title = DisplayTitle + " - {{R|Errors}}";
			string text = string.Join("\n", Settings.Errors.Take(3));
			if (count > 3)
			{
				text = text + "\n(... {{R|+" + (count - 3) + "}} more)";
			}
			text = text + "\n\nAutomatically on your clipboard should you wish to forward it to " + (Manifest.Author ?? "the mod author") + ".";
			List<QudMenuItem> list = new List<QudMenuItem>(PopupMessage.CancelButton);
			list.Add(new QudMenuItem
			{
				text = "{{W|[R]}} {{y|Retry}}",
				command = "retry",
				hotkey = "R"
			});
			if (WorkshopInfo != null)
			{
				list.Add(new QudMenuItem
				{
					text = "{{W|[W]}} {{y|Workshop}}",
					command = "workshop",
					hotkey = "W"
				});
			}
			Popup.WaitNewPopupMessage(text, list, (QudMenuItem i) =>
			{
				if (i.command == "workshop")
				{
					WorkshopInfo?.OpenWorkshopPage();
				}
				else if (i.command == "retry")
				{
					Settings.Failed = false;
				}
			}, null, title);
		}
		else
		{
			Settings.Failed = false;
		}
	}

	/// <summary>
	/// Display a popup to the user with dependency information
	/// Clears <see cref="F:XRL.ModSettings.Failed" /> on success or no errors.
	/// </summary>
	public void ConfirmDependencies()
	{
		Utf16ValueStringBuilder SB = ZString.CreateStringBuilder();
		SB.AppendMultiple(DisplayTitle, " is missing one or more dependencies.");
		string value;
		string key;
		if (!Manifest.Dependencies.IsNullOrEmpty())
		{
			SB.Compound("{{W|=== Required ===}}", "\n\n");
			foreach (KeyValuePair<string, string> dependency in Manifest.Dependencies)
			{
				dependency.Deconstruct(out value, out key);
				string iD = value;
				string version = key;
				AppendDependencyConfirmation(ref SB, iD, version);
			}
		}
		if (!Manifest.Directories.IsNullOrEmpty())
		{
			bool flag = true;
			ModDirectory[] directories = Manifest.Directories;
			foreach (ModDirectory modDirectory in directories)
			{
				if (modDirectory.Dependencies.IsNullOrEmpty())
				{
					continue;
				}
				if (flag)
				{
					SB.Compound("{{W|=== Optional ===}}", "\n\n");
					flag = false;
				}
				foreach (KeyValuePair<string, string> dependency2 in modDirectory.Dependencies)
				{
					dependency2.Deconstruct(out key, out value);
					string iD2 = key;
					string version2 = value;
					AppendDependencyConfirmation(ref SB, iD2, version2);
				}
			}
		}
		if (SB.Length > 0)
		{
			Popup.WaitNewPopupMessage(SB.ToString(), null, null, null, "{{W|Dependencies}}");
		}
	}

	public void ConfirmUpdate()
	{
		if (RemoteVersion == Version.Zero)
		{
			return;
		}
		if (Settings != null)
		{
			Settings.UpdateVersion = RemoteVersion;
			ModManager.WriteModSettings();
		}
		bool canUpdate = Source == ModSource.Steam;
		Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		utf16ValueStringBuilder.Append(DisplayTitle);
		utf16ValueStringBuilder.Append($" has a new version available: {RemoteVersion}.");
		if (canUpdate)
		{
			utf16ValueStringBuilder.Append("\n\nDo you want to download it?");
		}
		if (utf16ValueStringBuilder.Length <= 0)
		{
			return;
		}
		Popup.WaitNewPopupMessage(utf16ValueStringBuilder.ToString(), canUpdate ? PopupMessage.YesNoButton : null, (QudMenuItem i) =>
		{
			if (canUpdate && i.command == "Yes")
			{
				GameManager.Instance.StartCoroutine(DownloadUpdate());
			}
		}, null, "{{W|Update Available}}");
	}

	public IEnumerator DownloadUpdate()
	{
		if (Source != ModSource.Steam || WorkshopInfo == null)
		{
			yield break;
		}
		PublishedFileId_t id = new PublishedFileId_t(WorkshopInfo.WorkshopId);
		try
		{
			Directory.Delete();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Exception deleting workshop directory", x);
			yield break;
		}
		if (!SteamUGC.DownloadItem(id, bHighPriority: true))
		{
			yield break;
		}
		using (Loading.StartTask("Updating " + DisplayTitleStripped + "..."))
		{
			uint flags = 48u;
			while ((SteamUGC.GetItemState(id) & flags) != 0)
			{
				yield return new WaitForSeconds(1f);
			}
			ReadConfigurations();
			HasUpdate = false;
		}
	}

	private void AppendDependencyConfirmation(ref Utf16ValueStringBuilder SB, string ID, string Version)
	{
		string value2;
		string value3;
		if (ModManager.ModMap.TryGetValue(ID, out var value))
		{
			if (value.Manifest.Version.EqualsSemantic(Version))
			{
				if (!value.Active)
				{
					value2 = "Invalid";
					value3 = "R";
				}
				else
				{
					value2 = "OK";
					value3 = "g";
				}
			}
			else
			{
				value2 = "Version mismatch";
				value3 = "W";
			}
		}
		else
		{
			value2 = "Missing";
			value3 = "R";
		}
		SB.AppendMultiple("\n{{", value3, "|", ID);
		SB.AppendMultiple(" (", ConsoleLib.Console.ColorUtility.EscapeFormatting(Version), "): ", value2, "}}");
	}

	/// <summary>
	/// Write mod error and warning messages to the clipboard.
	/// </summary>
	public void LogToClipboard()
	{
		StringBuilder stringBuilder = Strings.SB.Clear().Append("=== ").Append(DisplayTitleStripped);
		if (!Manifest.Version.IsZero())
		{
			stringBuilder.Append(" ").Append(Manifest.Version.ToString());
		}
		stringBuilder.Append(" Errors ===\n");
		if (Settings.Errors.Any())
		{
			stringBuilder.AppendRange(Settings.Errors, "\n");
		}
		else
		{
			stringBuilder.Append("None");
		}
		stringBuilder.Append("\n== Warnings ==\n");
		if (Settings.Warnings.Any())
		{
			stringBuilder.AppendRange(Settings.Warnings, "\n");
		}
		else
		{
			stringBuilder.Append("None");
		}
		ClipboardHelper.SetClipboardData(stringBuilder.ToString());
	}

	public Sprite GetDefaultSprite()
	{
		return SpriteManager.GetUnitySprite("UI/Achievements/coq.png");
	}

	/// <summary>
	/// Get unity sprite of image defined in manifest.json or workshop.json.
	/// </summary>
	public Sprite GetSprite()
	{
		string text = Manifest.PreviewImage ?? WorkshopInfo?.ImagePath;
		if (string.IsNullOrEmpty(text))
		{
			return GetDefaultSprite();
		}
		string text2 = Platform.IO.Path.Combine(Path, text);
		if (!text2.StartsWith(Path))
		{
			return GetDefaultSprite();
		}
		Sprite value = null;
		if (spriteByPath.TryGetValue(text2, out value))
		{
			return value;
		}
		Texture2D texture2D = null;
		if (File.Exists(text2))
		{
			byte[] data = File.ReadAllBytes(text2);
			texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
			texture2D.LoadImage(data);
			texture2D.filterMode = UnityEngine.FilterMode.Trilinear;
		}
		if (texture2D != null)
		{
			value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
		}
		spriteByPath.Add(text2, value);
		return value;
	}

	public bool TryBuildAssembly(RoslynCSharpCompiler Service, Rack<string> PathList, out Assembly Assembly)
	{
		PathList.Clear();
		foreach (ModFile file in Files)
		{
			if (file.Type == ModFileType.CSharp)
			{
				PathList.Add(file.OriginalName);
			}
		}
		string[] array = PathList.ToArray();
		string arg = ((array.Length == 1) ? "file" : "files");
		Logger.buildLog.Info($"Compiling {array.Length} {arg}...");
		Service.OutputName = ID;
		CompilationResult compilationResult = Service.CompileFromFiles(array);
		if (compilationResult.Success)
		{
			Logger.buildLog.Info("Success :)");
			this.Assembly = (Assembly = compilationResult.OutputAssembly);
			Settings.Failed = false;
			if (compilationResult.OutputFile.IsNullOrEmpty())
			{
				Service.ReferenceAssemblies.Add(AssemblyReference.FromImage(compilationResult.OutputAssemblyImage));
			}
			else
			{
				Service.ReferenceAssemblies.Add(AssemblyReference.FromNameOrFile(compilationResult.OutputFile));
				Logger.buildLog.Info("Location: " + compilationResult.OutputFile);
			}
		}
		else
		{
			Logger.buildLog.Info("Failure :(");
			Assembly = null;
			Settings.Failed = true;
		}
		if (compilationResult.ErrorCount > 0)
		{
			Logger.buildLog.Info("== COMPILER ERRORS ==");
			CompilationError[] errors = compilationResult.Errors;
			foreach (CompilationError compilationError in errors)
			{
				if (compilationError.IsError)
				{
					string text = DataManager.SanitizePathForDisplay(compilationError.ToString());
					Logger.buildLog.Error(text);
					Error(text);
				}
			}
		}
		if (compilationResult.WarningCount > 0)
		{
			Logger.buildLog.Info("== COMPILER WARNINGS ==");
			CompilationError[] errors = compilationResult.Errors;
			foreach (CompilationError compilationError2 in errors)
			{
				if (compilationError2.IsWarning)
				{
					string text2 = DataManager.SanitizePathForDisplay(compilationError2.ToString());
					Logger.buildLog.Info(text2);
					Warn(text2);
				}
			}
		}
		ApplyHarmonyPatches();
		return compilationResult.Success;
	}

	/// <summary>
	/// Log a warning with mod context.
	/// </summary>
	/// <remarks>Also added to <see cref="F:XRL.ModSettings.Warnings" />.</remarks>
	public void Warn(object msg)
	{
		MetricsManager.LogModWarning(this, msg);
	}

	/// <summary>
	/// Log an error with mod context.
	/// </summary>
	/// <remarks>Also added to <see cref="F:XRL.ModSettings.Errors" />.</remarks>
	public void Error(object msg)
	{
		MetricsManager.LogModError(this, msg);
	}

	/// <summary>
	/// Apply harmony patches from this mod's assembly, if it has any.
	/// </summary>
	/// <remarks>Uses <see cref="F:XRL.ModInfo.ID" /> as the ID for the <see cref="F:XRL.ModInfo.Harmony" /> instance.</remarks>
	public void ApplyHarmonyPatches()
	{
		try
		{
			if (!(Assembly == null) && Assembly.GetTypes().Any((Type x) => x.IsDefined(typeof(HarmonyAttribute), inherit: true)))
			{
				Logger.buildLog.Info("Applying Harmony patches...");
				Harmony = Harmony ?? new Harmony(ID);
				Harmony.PatchAll(Assembly);
				Logger.buildLog.Info("Success :)");
			}
		}
		catch (Exception ex)
		{
			Error("Exception applying harmony patches: " + ex);
			Logger.buildLog.Info("Failure :(");
		}
	}

	/// <summary>
	/// Unapply harmony patches using this mod's <see cref="F:XRL.ModInfo.ID" />.
	/// </summary>
	public void UnapplyHarmonyPatches()
	{
		try
		{
			if (Harmony.GetPatchedMethods().Any())
			{
				Logger.buildLog.Info("Unapplying Harmony patches...");
				Harmony.UnpatchAll(Harmony.Id);
			}
		}
		catch (Exception ex)
		{
			Error("Exception unapplying harmony patches: " + ex);
		}
	}

	/// <summary>
	/// Initialize a new <see cref="T:XRL.SteamWorkshopInfo" /> instance with <paramref name="PublishedFileId" />.
	/// </summary>
	public void InitializeWorkshopInfo(ulong PublishedFileId)
	{
		WorkshopInfo = new SteamWorkshopInfo();
		WorkshopInfo.WorkshopId = PublishedFileId;
		SaveWorkshopInfo();
	}

	public void SaveWorkshopInfo()
	{
		if (WorkshopInfo != null)
		{
			ModManager.JsonSerializer.Serialize(Platform.IO.Path.Combine(Path, "workshop.json"), WorkshopInfo);
		}
	}

	public int CompareTo(ModInfo Other)
	{
		int num = LoadOrder.CompareTo(Other.LoadOrder);
		if (num != 0)
		{
			return num;
		}
		num = Manifest.LoadOrder.GetValueOrDefault().CompareTo(Other.Manifest.LoadOrder.GetValueOrDefault());
		if (num != 0)
		{
			return num;
		}
		return string.Compare(ID, Other.ID, StringComparison.Ordinal);
	}

	public override string ToString()
	{
		return ID;
	}
}
