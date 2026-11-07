using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Text;
using Genkit;
using HarmonyLib;
using Newtonsoft.Json;
using Platform.IO;
using RoslynCSharp;
using RoslynCSharp.Compiler;
using Steamworks;
using UnityEngine;
using XRL.Collections;
using XRL.Core;
using XRL.UI;
using XRL.World;

namespace XRL;

[HasModSensitiveStaticCache]
public static class ModManager
{
	public static Rack<ModInfo> Mods = new Rack<ModInfo>();

	public static Rack<ModInfo> ActiveMods = new Rack<ModInfo>();

	public static Rack<Module> ActiveModules = new Rack<Module>(Assembly.GetExecutingAssembly().GetModules());

	public static Dictionary<string, ModInfo> ModMap = new Dictionary<string, ModInfo>();

	public static Dictionary<string, ModSettings> ModSettingsMap = new Dictionary<string, ModSettings>();

	public static Dictionary<Assembly, ModInfo> ModAssemblyMap = new Dictionary<Assembly, ModInfo>();

	public static Dictionary<ulong, ModInfo> ModWorkshopMap = new Dictionary<ulong, ModInfo>();

	/// <summary>
	/// Get enabled script mod assemblies in priority order.
	/// </summary>
	public static Rack<Assembly> ModAssemblies = new Rack<Assembly>();

	public static bool Initialized = false;

	public static JsonSerializer JsonSerializer = new JsonSerializer
	{
		Formatting = Formatting.Indented,
		NullValueHandling = NullValueHandling.Ignore
	};

	[ModSensitiveStaticCache(true)]
	private static Dictionary<Type, List<Type>> _typesWithAttribute = new Dictionary<Type, List<Type>>();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<(Type, Type), List<FieldInfo>> _fieldsWithAttribute = new Dictionary<(Type, Type), List<FieldInfo>>();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<(Type, Type), List<MethodInfo>> _methodsWithAttribute = new Dictionary<(Type, Type), List<MethodInfo>>();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<Type, List<Type>> _assignableTypes = new Dictionary<Type, List<Type>>();

	[ModSensitiveStaticCache(true)]
	private static Dictionary<string, Type> _typeResolutions = new Dictionary<string, Type>();

	public static Dictionary<Type, string> typeNames = new Dictionary<Type, string>();

	private static Harmony harmony = new Harmony("com.freeholdgames.cavesofqud");

	public const BindingFlags ATTRIBUTE_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	[Obsolete("Use ModManager.Initialized")]
	public static bool Compiled => Initialized;

	public static string MarketingPostfix => XRLGame.MarketingPostfix;

	public static System.Version MarketingVersion => XRLGame.MarketingVersion;

	public static System.Version CoreVersion => XRLGame.CoreVersion;

	/// <summary>
	/// Yields every type in <see cref="P:XRL.ModManager.ActiveAssemblies" />.
	/// </summary>
	public static IEnumerable<Type> ActiveTypes
	{
		get
		{
			foreach (Module activeModule in ActiveModules)
			{
				Type[] types = activeModule.GetTypes();
				for (int i = 0; i < types.Length; i++)
				{
					yield return types[i];
				}
			}
		}
	}

	/// <summary>
	/// Yields the current executing assembly followed by any enabled script mod assemblies in priority order.
	/// </summary>
	public static IEnumerable<Assembly> ActiveAssemblies
	{
		get
		{
			yield return Assembly.GetExecutingAssembly();
			foreach (Assembly modAssembly in ModAssemblies)
			{
				yield return modAssembly;
			}
		}
	}

	private static RoslynCSharpCompiler GetCompilerService()
	{
		RoslynCSharpCompiler roslynCompilerService = ScriptDomain.CreateDomain("ModsDomain").RoslynCompilerService;
		roslynCompilerService.GenerateInMemory = !Options.OutputModAssembly;
		roslynCompilerService.OutputDirectory = DataManager.SavePath("ModAssemblies");
		roslynCompilerService.OutputPDBExtension = ".pdb";
		roslynCompilerService.GenerateSymbols = !roslynCompilerService.GenerateInMemory;
		foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies().Where(MainAssemblyPredicate))
		{
			roslynCompilerService.ReferenceAssemblies.Add(AssemblyReference.FromAssembly(item));
		}
		System.Version marketingVersion = MarketingVersion;
		System.Version coreVersion = CoreVersion;
		string text = $"VERSION_{marketingVersion.Major}_{marketingVersion.Minor}";
		string text2 = $"BUILD_{coreVersion.Major}_{coreVersion.Minor}_{coreVersion.Build}";
		roslynCompilerService.DefineSymbols.Add(text);
		roslynCompilerService.DefineSymbols.Add(text2);
		Logger.buildLog.Info("Defined symbol: " + text);
		Logger.buildLog.Info("Defined symbol: " + text2);
		return roslynCompilerService;
	}

	public static void CheckXRLConflicts()
	{
		ModInfo item = new ModInfo(DataManager.FilePath(), "Base");
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		Dictionary<string, object> dictionary = new Dictionary<string, object>(ModAssemblies.Count * 32);
		foreach (ModInfo activeMod in ActiveMods)
		{
			if ((object)activeMod.Assembly == null)
			{
				continue;
			}
			Type[] exportedTypes = activeMod.Assembly.GetExportedTypes();
			for (int i = 0; i < exportedTypes.Length; i++)
			{
				string fullName = exportedTypes[i].FullName;
				if (!fullName.StartsWith("XRL."))
				{
					continue;
				}
				if (!dictionary.TryGetValue(fullName, out var value))
				{
					if ((object)executingAssembly.GetType(fullName) != null)
					{
						dictionary[fullName] = new Rack<ModInfo> { item, activeMod };
					}
					else
					{
						dictionary[fullName] = activeMod;
					}
				}
				else if (value is Rack<ModInfo> rack)
				{
					rack.Add(activeMod);
				}
				else
				{
					dictionary[fullName] = new Rack<ModInfo>
					{
						(ModInfo)value,
						activeMod
					};
				}
			}
		}
		HashSet<ModInfo> hashSet = new HashSet<ModInfo>();
		bool flag = false;
		using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		foreach (var (value2, obj2) in dictionary)
		{
			if (!(obj2 is Rack<ModInfo> { Count: >1 } rack2))
			{
				continue;
			}
			if (!flag)
			{
				Logger.buildLog.Error("==== TYPE CONFLICTS DETECTED ====");
				flag = true;
			}
			utf16ValueStringBuilder.Append(value2);
			utf16ValueStringBuilder.Append(" is defined in: ");
			int j = 0;
			for (int count = rack2.Count; j < count; j++)
			{
				hashSet.Add(rack2[j]);
				if (j != 0)
				{
					utf16ValueStringBuilder.Append(", ");
				}
				string displayTitleStripped = rack2[j].DisplayTitleStripped;
				bool flag2 = displayTitleStripped.Contains(',');
				if (flag2)
				{
					utf16ValueStringBuilder.Append('\'');
				}
				utf16ValueStringBuilder.Append(displayTitleStripped);
				if (flag2)
				{
					utf16ValueStringBuilder.Append('\'');
				}
			}
			Logger.buildLog.Info(utf16ValueStringBuilder.ToString());
			utf16ValueStringBuilder.Clear();
		}
		if (!flag)
		{
			return;
		}
		hashSet.Remove(item);
		foreach (ModInfo item2 in hashSet)
		{
			if (utf16ValueStringBuilder.Length != 0)
			{
				utf16ValueStringBuilder.Append(", ");
			}
			utf16ValueStringBuilder.Append(item2.DisplayTitleStripped);
		}
		utf16ValueStringBuilder.Insert(0, "Type conflicts detected in mods: ");
		utf16ValueStringBuilder.Append(".\nThis can lead to unexpected casting errors and null references for types that are resolved via their name in XML like parts, skills, and mutations.");
		utf16ValueStringBuilder.Append("\nIt's strongly recommended to rename your type to be unique.");
		utf16ValueStringBuilder.Append("\nSee build log for details of all conflicting types.");
		MetricsManager.LogModError(null, utf16ValueStringBuilder.ToString());
	}

	/// <summary>
	/// Register mod in the manager if a mod with that ID has not already been registered.
	/// </summary>
	/// <returns><c>true</c> if the mod was successfully registered; otherwise, <c>false</c>.</returns>
	public static bool RegisterMod(ModInfo Mod)
	{
		if (ModMap.ContainsKey(Mod.ID))
		{
			Mod.Warn("A mod with the ID \"" + Mod.ID + "\" already exists in " + DataManager.SanitizePathForDisplay(ModMap[Mod.ID].Path) + ", skipping.");
			return false;
		}
		ModMap[Mod.ID] = Mod;
		Mods.Add(Mod);
		if (Mod.WorkshopInfo != null && Mod.WorkshopInfo.WorkshopId != 0L)
		{
			ModWorkshopMap[Mod.WorkshopInfo.WorkshopId] = Mod;
		}
		return true;
	}

	public static bool DoesModDefineType(Type T)
	{
		return ModAssemblies.Contains(T.Assembly);
	}

	public static bool DoesModDefineType(string TypeID)
	{
		if (_typeResolutions.TryGetValue(TypeID, out var value))
		{
			return ModAssemblies.Contains(value.Assembly);
		}
		return ModAssemblies.Any((Assembly x) => x.GetType(TypeID) != null);
	}

	/// <summary>
	/// Attempt to find a type within loaded assemblies.
	/// Mods are searched first according to priority, followed by the main game assembly.
	/// </summary>
	/// <inheritdoc cref="M:XRL.ModManager.ResolveType(System.String,System.String,System.Boolean,System.Boolean,System.Boolean)" />
	public static Type ResolveType(string TypeID, bool IgnoreCase = false, bool ThrowOnError = false, bool Cache = true)
	{
		return ResolveType(null, TypeID, IgnoreCase, ThrowOnError, Cache);
	}

	/// <summary>
	/// Attempt to find a type within loaded assemblies.
	/// Mods are searched first according to priority, followed by the main game assembly.
	/// If not found as Namespace.TypeID it will look for just TypeID also.
	/// </summary>
	/// <param name="Namespace">The default namespace to look in.</param>
	/// <param name="TypeID">The full name of the type, including namespaces.</param>
	/// <param name="IgnoreCase"><c>true</c> to ignore the case of the type name; otherwise, <c>false</c>.</param>
	/// <param name="ThrowOnError">Throw exception if invalid <see cref="!:TypeID" /> is specified.</param>
	/// <param name="Cache">Store the resolved type for this <see cref="!:TypeID" />.</param>
	/// <returns><see cref="T:System.Type" /> if one was found; otherwise, <c>null</c>.</returns>
	public static Type ResolveType(string Namespace, string TypeID, bool IgnoreCase = false, bool ThrowOnError = false, bool Cache = true)
	{
		if (TypeID.IsNullOrEmpty())
		{
			return null;
		}
		string text = ((Namespace != null) ? (Namespace + "." + TypeID) : TypeID);
		if (_typeResolutions.TryGetValue(text, out var value))
		{
			return value;
		}
		value = Type.GetType(text, throwOnError: false, IgnoreCase);
		if ((object)value == null)
		{
			foreach (Assembly modAssembly in ModAssemblies)
			{
				value = modAssembly.GetType(text, throwOnError: false, IgnoreCase);
				if ((object)value != null)
				{
					break;
				}
			}
			if (ThrowOnError && (object)value == null)
			{
				throw new TypeLoadException("Could not resolve type '" + text + "' from active assemblies.");
			}
		}
		if (!IgnoreCase & Cache)
		{
			_typeResolutions[text] = value;
		}
		return value;
	}

	public static string ResolveTypeName(Type T)
	{
		if (typeNames.TryGetValue(T, out var value))
		{
			return value;
		}
		typeNames.Add(T, T.Name);
		return typeNames[T];
	}

	public static void ResetModSensitiveStaticCaches()
	{
		Type typeFromHandle = typeof(ModSensitiveStaticCacheAttribute);
		Type typeFromHandle2 = typeof(ModSensitiveCacheInitAttribute);
		Type typeFromHandle3 = typeof(HasModSensitiveStaticCacheAttribute);
		foreach (FieldInfo item in GetFieldsWithAttribute(typeFromHandle, typeFromHandle3, Cache: false))
		{
			if (item.IsStatic)
			{
				try
				{
					bool flag = item.FieldType.IsValueType || item.GetCustomAttribute<ModSensitiveStaticCacheAttribute>().CreateEmptyInstance;
					item.SetValue(null, flag ? Activator.CreateInstance(item.FieldType) : null);
				}
				catch (Exception arg)
				{
					MetricsManager.LogAssemblyError(item, $"Error initializing {item.DeclaringType.FullName}.{item.Name}: {arg}");
				}
			}
		}
		foreach (MethodInfo item2 in GetMethodsWithAttribute(typeFromHandle2, typeFromHandle3, Cache: false))
		{
			try
			{
				item2.Invoke(null, new object[0]);
			}
			catch (Exception arg2)
			{
				MetricsManager.LogAssemblyError(item2, $"Error invoking {item2.DeclaringType.FullName}.{item2.Name}: {arg2}");
			}
		}
		XRLCore.Core?.ReloadUIViews();
	}

	public static void CallAfterGameLoaded()
	{
		foreach (MethodInfo item in GetMethodsWithAttribute(typeof(CallAfterGameLoadedAttribute), typeof(HasCallAfterGameLoadedAttribute)))
		{
			try
			{
				item.Invoke(null, Array.Empty<object>());
			}
			catch (Exception arg)
			{
				MetricsManager.LogAssemblyError(item, $"Error invoking {item.DeclaringType.FullName}.{item.Name}: {arg}");
			}
		}
	}

	internal static bool MainAssemblyPredicate(Assembly assembly)
	{
		if (assembly.IsDynamic)
		{
			return false;
		}
		if (assembly.Location.IsNullOrEmpty())
		{
			return false;
		}
		if (assembly.Location.Contains("ModAssemblies"))
		{
			return false;
		}
		if (assembly.Location.Contains("UIElements"))
		{
			return false;
		}
		if (assembly.Location.Contains("UnityEditor."))
		{
			return false;
		}
		if (assembly.FullName.Contains("ExCSS"))
		{
			return false;
		}
		return true;
	}

	private static void BuildMods()
	{
		try
		{
			Options.LoadModOptionDefaults();
		}
		catch (Exception message)
		{
			MetricsManager.LogError(message);
		}
		Logger.buildLog.Info("==== BUILDING MODS ====");
		RoslynCSharpCompiler compilerService = GetCompilerService();
		Rack<string> pathList = new Rack<string>();
		foreach (ModInfo mod in Mods)
		{
			Logger.buildLog.Info("=== " + mod.DisplayTitleStripped.ToUpper() + " ===");
			mod.InitializeFiles();
			mod.AssertDependencies();
			if (!mod.IsEnabled)
			{
				Logger.buildLog.Info($"Skipping, state: {mod.State}");
				continue;
			}
			try
			{
				if (mod.IsScripting && mod.TryBuildAssembly(compilerService, pathList, out var Assembly))
				{
					ModAssemblies.Add(Assembly);
					ModAssemblyMap[Assembly] = mod;
					ActiveModules.AddRange((IReadOnlyList<Module>)Assembly.GetModules());
				}
			}
			catch (Exception ex)
			{
				mod.Error("Exception compiling mod assembly: " + ex);
				mod.Settings.Failed = true;
			}
			if (mod.IsEnabled)
			{
				mod.Active = true;
				ActiveMods.Add(mod);
				string text = "MOD_" + mod.ID.ToUpperInvariant().Replace(" ", "_");
				compilerService.DefineSymbols.Add(text);
				Logger.buildLog.Info("Defined symbol: " + text);
			}
		}
		MinEvent.ResetEvents();
	}

	private static void PrintLoadOrder()
	{
		Logger.buildLog.Info("==== FINAL LOAD ORDER ====");
		for (int i = 0; i < ActiveMods.Count; i++)
		{
			Logger.buildLog.Info($"{i + 1}: {ActiveMods[i].ID}");
		}
	}

	private static bool OnlySupportsEmbeddedMods()
	{
		return false;
	}

	public static void RefreshModDirectory(string Path, bool Create = false, ModSource Source = ModSource.Local)
	{
		if (Source != ModSource.Embedded && OnlySupportsEmbeddedMods())
		{
			return;
		}
		MetricsManager.LogInfo("RefreshModDirectory " + ((Path != null) ? Path : "(null)"));
		Platform.IO.DirectoryInfo directoryInfo = (Create ? Platform.IO.Directory.CreateDirectory(Path) : new Platform.IO.DirectoryInfo(Path));
		if (!directoryInfo.Exists)
		{
			return;
		}
		foreach (Platform.IO.DirectoryInfo item in directoryInfo.EnumerateDirectories())
		{
			if ((item.Attributes & Platform.IO.FileAttributes.Hidden) <= Platform.IO.FileAttributes.None)
			{
				try
				{
					RegisterMod(new ModInfo(item.FullName, item.Name, Source, Initialize: true));
				}
				catch (Exception x)
				{
					MetricsManager.LogError("Exception reading local mod directory " + item.Name, x);
				}
			}
		}
	}

	private static void RefreshWorkshopSubscriptions()
	{
		if (!PlatformManager.IsInitialized())
		{
			MetricsManager.LogInfo("Skipping workshop subscription info because steam isn't connected");
			return;
		}
		PublishedFileId_t[] array = new PublishedFileId_t[4096];
		uint subscribedItems = SteamUGC.GetSubscribedItems(array, 4096u);
		MetricsManager.LogInfo("Subscribed workshop items: " + subscribedItems);
		for (int i = 0; i < subscribedItems; i++)
		{
			try
			{
				if (SteamUGC.GetItemInstallInfo(array[i], out var _, out var pchFolder, 4096u, out var _))
				{
					if (!System.IO.Directory.Exists(pchFolder))
					{
						MetricsManager.LogError("Mod directory does not exist: " + pchFolder);
					}
					else
					{
						RegisterMod(new ModInfo(pchFolder, array[i].ToString(), ModSource.Steam, Initialize: true));
					}
				}
			}
			catch (Exception x)
			{
				PublishedFileId_t publishedFileId_t = array[i];
				MetricsManager.LogError("Exception reading workshop mod subscription " + publishedFileId_t.ToString(), x);
			}
		}
	}

	/// <summary>
	/// Read current mod settings from ModSettings.json.
	/// </summary>
	/// <param name="Reload">Force reload from disk even if current state in memory (used for undo).</param>
	public static void ReadModSettings(bool Repopulate = false)
	{
		try
		{
			ModSettingsMap.Clear();
			string text = DataManager.LocalPath("ModSettings.json");
			if (Platform.IO.File.Exists(text))
			{
				JsonSerializer.Populate(text, ModSettingsMap);
			}
			if (!Repopulate)
			{
				return;
			}
			foreach (ModInfo mod in Mods)
			{
				mod.LoadSettings();
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Failed reading ModSettings.json", x);
		}
	}

	/// <summary>
	/// Write current mod settings to ModSettings.json.
	/// </summary>
	public static void WriteModSettings()
	{
		if (ModSettingsMap != null)
		{
			string file = DataManager.LocalPath("ModSettings.json");
			JsonSerializer.Serialize(file, ModSettingsMap);
		}
	}

	public static void Init()
	{
		if (Initialized || !Application.isPlaying)
		{
			return;
		}
		if (Options.Bag == null)
		{
			MetricsManager.LogError("Mod initialization executed early, options not initialized.");
			return;
		}
		ReadModSettings();
		try
		{
			RefreshModDirectory(DataManager.DLCPath(), Create: false, ModSource.Embedded);
		}
		catch (Exception message)
		{
			MetricsManager.LogError(message);
		}
		try
		{
			RefreshModDirectory(DataManager.EmbeddedModsPath(), Create: false, ModSource.Embedded);
		}
		catch (Exception message2)
		{
			MetricsManager.LogError(message2);
		}
		if (Options.EnableMods)
		{
			try
			{
				RefreshModDirectory(DataManager.SavePath("Mods"), Create: true);
			}
			catch (Exception message3)
			{
				MetricsManager.LogError(message3);
			}
			try
			{
				RefreshModDirectory(DataManager.LocalPath("Mods"), Create: true);
			}
			catch (Exception message4)
			{
				MetricsManager.LogError(message4);
			}
			try
			{
				RefreshWorkshopSubscriptions();
			}
			catch (Exception message5)
			{
				MetricsManager.LogError(message5);
			}
		}
		try
		{
			CheckUpdates();
		}
		catch (Exception message6)
		{
			MetricsManager.LogError(message6);
		}
		ResolveAllDependencies();
		DetermineLoadOrderFast();
		BuildMods();
		CheckXRLConflicts();
		PrintLoadOrder();
		Initialized = true;
		ResetModSensitiveStaticCaches();
	}

	private static void CheckUpdates()
	{
		if (!PlatformManager.IsInitialized())
		{
			return;
		}
		PublishedFileId_t[] publishedFileIDs = GetPublishedFileIDs();
		UGCQueryHandle_t query = SteamUGC.CreateQueryUGCDetailsRequest(publishedFileIDs, (uint)publishedFileIDs.Length);
		SteamUGC.SetReturnOnlyIDs(query, bReturnOnlyIDs: true);
		SteamUGC.SetReturnKeyValueTags(query, bReturnKeyValueTags: true);
		SteamUGC.SetAllowCachedResponse(query, 300u);
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(query);
		CallResult<SteamUGCQueryCompleted_t> callResult = CallResult<SteamUGCQueryCompleted_t>.Create();
		PlatformManager.Steam.Disposables.Add(callResult);
		callResult.Set(hAPICall, (SteamUGCQueryCompleted_t args, bool _) =>
		{
			try
			{
				if (args.m_eResult == EResult.k_EResultOK)
				{
					uint num = 0u;
					for (uint unNumResultsReturned = args.m_unNumResultsReturned; num < unNumResultsReturned; num++)
					{
						SteamUGC.GetQueryUGCResult(query, num, out var pDetails);
						SteamUGC.GetQueryUGCKeyValueTag(query, num, "manifest_version", out var pchValue, 32u);
						if (!pchValue.IsNullOrEmpty() && ModWorkshopMap.TryGetValue((ulong)pDetails.m_nPublishedFileId, out var value) && Version.TryParse(pchValue, out var Version))
						{
							value.RemoteVersion = Version;
							value.HasUpdate = value.RemoteVersion > value.Manifest.Version;
						}
					}
				}
			}
			finally
			{
				SteamUGC.ReleaseQueryUGCRequest(query);
				PlatformManager.Steam.Disposables.Remove(callResult);
				callResult.Dispose();
			}
		});
	}

	public static void ResolveAllDependencies()
	{
		Logger.buildLog.Info("==== RESOLVING DEPENDENCIES ====");
		foreach (ModInfo mod in Mods)
		{
			mod.ResolveDependencies();
		}
	}

	public static void DetermineLoadOrderFast()
	{
		int Order = 1;
		Mods.Sort();
		try
		{
			foreach (ModInfo mod in Mods)
			{
				Visit(mod, ref Order);
			}
			Mods.Sort();
		}
		catch (Exception)
		{
			foreach (ModInfo mod2 in Mods)
			{
				mod2.LoadOrder = int.MinValue;
			}
			DetermineLoadOrder();
		}
		static void Visit(ModInfo Mod, ref int reference)
		{
			if (Mod.LoadOrder == int.MinValue)
			{
				Mod.LoadOrder = int.MaxValue;
				foreach (KeyValuePair<ModInfo, uint> dependency in Mod.Dependencies)
				{
					dependency.Deconstruct(out var key, out var _);
					Visit(key, ref reference);
				}
				Mod.LoadOrder = reference++;
			}
			else if (Mod.LoadOrder == int.MaxValue)
			{
				throw new Exception("Cycle detected.");
			}
		}
	}

	public static void DetermineLoadOrder()
	{
		WeightedDigraph weightedDigraph = new WeightedDigraph(Mods.Count);
		Mods.Sort();
		int i = 0;
		for (int count = Mods.Count; i < count; i++)
		{
			foreach (var (item, weight) in Mods[i].Dependencies)
			{
				int to = Mods.IndexOf(item);
				weightedDigraph.AddArc(i, to, weight);
			}
		}
		try
		{
			foreach (RingDeque<int> item2 in weightedDigraph.YieldCycles(10000000u))
			{
				ModInfo modInfo2 = Mods[item2.First];
				int highlight = item2.Last;
				modInfo2.Error("Circular mod dependency: " + string.Join(" -> ", item2.Select((int num3) => (num3 != highlight) ? Mods[num3].ID : ("*" + Mods[num3].ID + "*"))));
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Graph cycle detection", x);
		}
		weightedDigraph.Resolve();
		int num2 = 0;
		for (int count2 = Mods.Count; num2 < count2; num2++)
		{
			Mods[num2].LoadOrder = weightedDigraph.GetTarget(num2) + 1;
		}
		Mods.Sort();
	}

	/// <summary>
	/// Get the mod associated with the calling assembly.
	/// </summary>
	/// <returns>The <see cref="T:XRL.ModInfo" /> if one associated with that assembly exists; otherwise, <c>null</c>.</returns>
	public static ModInfo GetMod()
	{
		Assembly callingAssembly = Assembly.GetCallingAssembly();
		if (ModAssemblyMap.TryGetValue(callingAssembly, out var value))
		{
			return value;
		}
		return null;
	}

	/// <summary>
	/// Get the mod associated with specified ID.
	/// </summary>
	/// <returns>The <see cref="T:XRL.ModInfo" /> if one by that ID exists; otherwise, <c>null</c>.</returns>
	public static ModInfo GetMod(string ID)
	{
		if (ID == null)
		{
			return null;
		}
		if (ModMap.TryGetValue(ID, out var value))
		{
			return value;
		}
		return null;
	}

	/// <summary>
	/// Get the mod associated with specified workshop ID.
	/// </summary>
	/// <returns>The <see cref="T:XRL.ModInfo" /> if one by that workshop ID exists; otherwise, <c>null</c>.</returns>
	public static ModInfo GetMod(ulong WorkshopID)
	{
		if (WorkshopID == 0)
		{
			return null;
		}
		if (ModWorkshopMap.TryGetValue(WorkshopID, out var value))
		{
			return value;
		}
		return null;
	}

	/// <summary>
	/// Get the mod associated with specified assembly.
	/// </summary>
	/// <returns>The <see cref="T:XRL.ModInfo" /> if one defines that assembly; otherwise, <c>null</c>.</returns>
	public static ModInfo GetMod(Assembly Assembly)
	{
		if (Assembly == null)
		{
			return null;
		}
		foreach (ModInfo mod in Mods)
		{
			if (!(mod.Assembly != Assembly))
			{
				return mod;
			}
		}
		return null;
	}

	/// <summary>
	/// Get the mod associated with a string that may be either
	/// an ID or a string specification of a workshop ID.
	/// </summary>
	/// <returns>The <see cref="T:XRL.ModInfo" /> if one retrievable by either form of ID exists; otherwise, <c>null</c>.</returns>
	public static ModInfo GetModBySpec(string Spec)
	{
		try
		{
			ModInfo mod = GetMod(Convert.ToUInt64(Spec));
			if (mod != null)
			{
				return mod;
			}
		}
		catch
		{
		}
		return GetMod(Spec);
	}

	/// <summary>
	/// Retrieves whether a mod is loaded that is associated with
	/// a string that may be either an ID or a string specification
	/// of a workshop ID.
	/// </summary>
	public static bool ModLoadedBySpec(string Spec)
	{
		return GetModBySpec(Spec) != null;
	}

	/// <summary>Get the first mod encountered in the currently executing stack.</summary>
	public static bool TryGetCallingMod(out ModInfo Mod, out StackFrame Frame)
	{
		return TryGetStackMod(new StackTrace(1), out Mod, out Frame);
	}

	/// <summary>Get the first mod encountered in the specified exception's stack.</summary>
	public static bool TryGetStackMod(Exception Exception, out ModInfo Mod, out StackFrame Frame)
	{
		return TryGetStackMod(new StackTrace(Exception), out Mod, out Frame);
	}

	/// <summary>Get the first mod encountered in the specified stack.</summary>
	/// <remarks>Mostly for logging purposes when the ModInfo isn't readily accessible.</remarks>
	public static bool TryGetStackMod(StackTrace Trace, out ModInfo Mod, out StackFrame Frame)
	{
		try
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			StackFrame[] frames = Trace.GetFrames();
			foreach (StackFrame stackFrame in frames)
			{
				Assembly assembly = stackFrame.GetMethod().DeclaringType.Assembly;
				if (!(assembly == executingAssembly) && ModAssemblyMap.TryGetValue(assembly, out var value))
				{
					Mod = value;
					Frame = stackFrame;
					return true;
				}
			}
		}
		catch
		{
		}
		Mod = null;
		Frame = null;
		return false;
	}

	public static string GetModTitle(string ID)
	{
		ModInfo mod = GetMod(ID);
		if (mod != null)
		{
			return mod.DisplayTitle;
		}
		if (ModSettingsMap.TryGetValue(ID, out var value) && !value.Title.IsNullOrEmpty())
		{
			return value.Title;
		}
		return ID;
	}

	public static void LogRunningMods()
	{
		string text = "Enabled mods: ";
		text = ((Mods != null && Mods.Count != 0) ? (text + string.Join(", ", from m in Mods
			where m.IsEnabled
			select m.DisplayTitleStripped)) : (text + "None"));
		MetricsManager.LogInfo(text);
	}

	public static IEnumerable<string> GetRunningMods()
	{
		if (ActiveMods == null)
		{
			yield break;
		}
		foreach (ModInfo activeMod in ActiveMods)
		{
			yield return activeMod.ID;
		}
	}

	public static List<string> GetAvailableMods()
	{
		List<string> result = new List<string>();
		ForEachMod((ModInfo mod) =>
		{
			result.Add(mod.ID);
		}, IncludeDisabled: true);
		return result;
	}

	public static bool IsScriptingUndetermined()
	{
		if (Options.Bag == null)
		{
			return false;
		}
		if (!Options.Bag.GetValue("OptionAllowCSMods").IsNullOrEmpty())
		{
			return false;
		}
		foreach (ModInfo mod in Mods)
		{
			if (mod.IsScripting)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAnyModFailed()
	{
		foreach (ModInfo mod in Mods)
		{
			if (mod.State == ModState.Failed)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAnyModMissingDependency()
	{
		foreach (ModInfo mod in Mods)
		{
			if (mod.State == ModState.MissingDependency)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasActiveScripts()
	{
		foreach (ModInfo mod in Mods)
		{
			if (mod.Active && mod.IsScripting)
			{
				return true;
			}
		}
		return false;
	}

	public static void ForEachMod(Action<ModInfo> ModAction, bool IncludeDisabled = false)
	{
		foreach (ModInfo mod in Mods)
		{
			if (mod.Active || IncludeDisabled)
			{
				try
				{
					ModAction(mod);
				}
				catch (Exception ex)
				{
					mod.Error(DataManager.SanitizePathForDisplay(ex.ToString()));
				}
			}
		}
	}

	public static void ForEveryFile(Action<string, ModInfo> FileAction, bool IncludeDisabled = false)
	{
		ForEachMod((ModInfo mod) =>
		{
			if (!mod.Directory.Exists)
			{
				return;
			}
			foreach (Platform.IO.FileInfo item in mod.Directory.EnumerateFiles())
			{
				FileAction(item.FullName, mod);
			}
		}, IncludeDisabled);
	}

	public static void ForEveryFileRecursive(Action<string, ModInfo> FileAction, string SearchPattern = "*.*", bool IncludeDisabled = false)
	{
		ForEachMod((ModInfo mod) =>
		{
			if (!mod.Directory.Exists)
			{
				return;
			}
			foreach (string item in Platform.IO.Directory.EnumerateFiles(mod.Path, SearchPattern, Platform.IO.SearchOption.AllDirectories))
			{
				FileAction(item, mod);
			}
		}, IncludeDisabled);
	}

	public static void ForEachFile(string FileName, Action<string> FileAction, bool IncludeDisabled = false, bool Recursive = false)
	{
		ForEachFile(FileName, (string f, ModInfo i) =>
		{
			FileAction(f);
		}, IncludeDisabled, Recursive);
	}

	public static void ForEachFile(string FileName, Action<string, ModInfo> FileAction, bool IncludeDisabled = false, bool Recursive = false)
	{
		string text = FileName.ToLower();
		foreach (ModInfo mod in Mods)
		{
			if (!mod.Active && !IncludeDisabled)
			{
				continue;
			}
			foreach (ModFile file in mod.Files)
			{
				if (!((Recursive ? file.Name : file.RelativeName) == text))
				{
					continue;
				}
				try
				{
					FileAction(file.OriginalName, mod);
					if (!Recursive)
					{
						break;
					}
				}
				catch (Exception ex)
				{
					mod.Error(DataManager.SanitizePathForDisplay(mod.Path + "/" + FileName + ": " + ex.ToString()));
				}
			}
		}
	}

	public static void ForEachFileIn(string Subdirectory, Action<string, ModInfo> FileAction, bool bIncludeBase = false, bool bIncludeDisabled = false)
	{
		if (bIncludeBase)
		{
			_ForEachFileIn(DataManager.FilePath(Subdirectory), FileAction, null);
		}
		foreach (ModInfo mod in Mods)
		{
			if (bIncludeDisabled || mod.Active)
			{
				_ForEachFileIn(Platform.IO.Path.Combine(mod.Path, Subdirectory), FileAction, mod);
			}
		}
	}

	private static void _ForEachFileIn(string Subdirectory, Action<string, ModInfo> FileAction, ModInfo mod)
	{
		if (!Platform.IO.Directory.Exists(Subdirectory))
		{
			return;
		}
		foreach (string item in Platform.IO.Directory.EnumerateFiles(Subdirectory))
		{
			try
			{
				FileAction(item, mod);
			}
			catch (Exception ex)
			{
				mod.Error(DataManager.SanitizePathForDisplay(Subdirectory + ": " + ex.ToString()));
			}
		}
		string[] directories = Platform.IO.Directory.GetDirectories(Subdirectory);
		for (int i = 0; i < directories.Length; i++)
		{
			_ForEachFileIn(directories[i], FileAction, mod);
		}
	}

	public static object CreateInstance(string className)
	{
		Type type = ResolveType(className);
		if (type == null)
		{
			throw new TypeLoadException("No class with name \"" + className + "\" could be found. A full name including namespaces is required.");
		}
		return Activator.CreateInstance(type);
	}

	public static T CreateInstance<T>(string className) where T : class
	{
		return CreateInstance(className) as T;
	}

	public static T CreateInstance<T>(Type type) where T : class
	{
		return Activator.CreateInstance(type) as T;
	}

	public static IEnumerable<Type> GetClassesWithAttribute(Type attributeToSearchFor, Type classFilterAttribute = null)
	{
		List<Type> list = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.Contains("Assembly-CSharp"))
			{
				list.AddRange(from m in assembly.GetTypes().Concat(assembly.GetTypes().SelectMany((Type type) => type.GetNestedTypes()))
					where m.IsDefined(attributeToSearchFor, inherit: false) && m.IsClass
					select m);
			}
		}
		return list;
	}

	public static List<T> GetInstancesWithAttribute<T>(Type attributeType) where T : class
	{
		List<T> list = new List<T>();
		foreach (Type item2 in GetTypesWithAttribute(attributeType))
		{
			if (Activator.CreateInstance(item2) is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Type> GetTypesWithAttribute(Type AttributeType, bool Cache = true)
	{
		if (Cache && _typesWithAttribute.TryGetValue(AttributeType, out var value))
		{
			return value;
		}
		value = new List<Type>(32);
		foreach (Type activeType in ActiveTypes)
		{
			if (activeType.IsDefined(AttributeType, inherit: true))
			{
				value.Add(activeType);
			}
		}
		if (Cache)
		{
			_typesWithAttribute[AttributeType] = value;
			value.TrimExcess();
		}
		return value;
	}

	public static List<MethodInfo> GetMethodsWithAttribute(Type AttributeType, Type ClassFilterType = null, bool Cache = true)
	{
		if (_methodsWithAttribute.TryGetValue((AttributeType, ClassFilterType), out var value))
		{
			return value;
		}
		value = new List<MethodInfo>(128);
		IEnumerable<Type> enumerable;
		if (!(ClassFilterType == null))
		{
			IEnumerable<Type> typesWithAttribute = GetTypesWithAttribute(ClassFilterType, Cache);
			enumerable = typesWithAttribute;
		}
		else
		{
			enumerable = ActiveTypes;
		}
		foreach (Type item in enumerable)
		{
			MethodInfo[] methods = item.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.IsDefined(AttributeType, inherit: false))
				{
					value.Add(methodInfo);
				}
			}
		}
		if (Cache)
		{
			_methodsWithAttribute.Add((AttributeType, ClassFilterType), value);
			value.TrimExcess();
		}
		return value;
	}

	public static List<FieldInfo> GetFieldsWithAttribute(Type AttributeType, Type ClassFilterType = null, bool Cache = true)
	{
		if (Cache && _fieldsWithAttribute.TryGetValue((AttributeType, ClassFilterType), out var value))
		{
			return value;
		}
		value = new List<FieldInfo>(128);
		IEnumerable<Type> enumerable;
		if (!(ClassFilterType == null))
		{
			IEnumerable<Type> typesWithAttribute = GetTypesWithAttribute(ClassFilterType, Cache);
			enumerable = typesWithAttribute;
		}
		else
		{
			enumerable = ActiveTypes;
		}
		foreach (Type item in enumerable)
		{
			FieldInfo[] fields = item.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.IsDefined(AttributeType, inherit: false))
				{
					value.Add(fieldInfo);
				}
			}
		}
		if (Cache)
		{
			_fieldsWithAttribute.Add((AttributeType, ClassFilterType), value);
			value.TrimExcess();
		}
		return value;
	}

	public static List<Type> GetTypesAssignableFrom(Type AssignableType, bool Cache = true)
	{
		if (Cache && _assignableTypes.TryGetValue(AssignableType, out var value))
		{
			return value;
		}
		value = new List<Type>(64);
		GetTypesAssignableFrom(AssignableType, value);
		if (Cache)
		{
			_assignableTypes.Add(AssignableType, value);
			value.TrimExcess();
		}
		return value;
	}

	public static void GetTypesAssignableFrom(Type AssignableType, List<Type> Result)
	{
		Result.Clear();
		foreach (Type activeType in ActiveTypes)
		{
			if (AssignableType.IsAssignableFrom(activeType))
			{
				Result.Add(activeType);
			}
		}
	}

	public static PublishedFileId_t[] GetPublishedFileIDs()
	{
		uint num = 0u;
		foreach (ModInfo mod in Mods)
		{
			if (mod.WorkshopInfo != null && mod.WorkshopInfo.WorkshopId != 0L)
			{
				num++;
			}
		}
		PublishedFileId_t[] array = new PublishedFileId_t[num];
		int num2 = 0;
		foreach (ModInfo mod2 in Mods)
		{
			if (mod2.WorkshopInfo != null && mod2.WorkshopInfo.WorkshopId != 0L)
			{
				array[num2++] = new PublishedFileId_t(mod2.WorkshopInfo.WorkshopId);
			}
		}
		return array;
	}
}
