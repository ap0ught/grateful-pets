using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Cysharp.Text;
using Platform.IO;
using Qud.UI;
using XRL;
using XRL.UI;

namespace Qud.API;

public static class SavesAPI
{
	private static List<string> _SavedGamePaths;

	private static readonly string[] InfoFileNames = new string[2] { "Primary.json", "Primary.sav.json" };

	public static List<string> SavedGamePaths
	{
		get
		{
			List<string> list = _SavedGamePaths;
			if (list == null)
			{
				list = new List<string>
				{
					DataManager.SyncedPath("Saves"),
					DataManager.SavePath("Saves")
				};
				_SavedGamePaths = list;
			}
			return list;
		}
	}

	private static long GetDirectorySize(string p)
	{
		IEnumerable<string> enumerable = Platform.IO.Directory.EnumerateFiles(p);
		long num = 0L;
		foreach (string item in enumerable)
		{
			Platform.IO.FileInfo fileInfo = new Platform.IO.FileInfo(item);
			num += fileInfo.Length;
		}
		return num;
	}

	public static async Task<SaveGameInfo> ReadSaveJson(string dirPath, string filePath)
	{
		SaveGameJSON json = null;
		try
		{
			json = await Platform.IO.File.ReadAllJsonAsync<SaveGameJSON>(filePath);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Loading save json " + filePath, x);
		}
		if (json == null)
		{
			return new SaveGameInfo
			{
				Name = "&RCorrupt info file",
				Size = "Total size: " + GetDirectorySize(dirPath) / 1000000 + "mb",
				Info = "",
				Directory = dirPath
			};
		}
		SaveGameInfo saveGameInfo = new SaveGameInfo
		{
			json = json,
			Directory = dirPath,
			Size = "Total size: " + GetDirectorySize(dirPath) / 1000000 + "mb",
			ID = json.ID,
			Version = json.GameVersion,
			Name = json.Name,
			Description = $"Level {json.Level} {json.GenoSubType} [{json.GameMode}]",
			Info = $"{json.Location}, {json.InGameTime} turn {json.Turn}",
			SaveTime = json.SaveTime,
			ModsEnabled = json.ModsEnabled
		};
		if (json.SaveVersion < 395 || json.SaveVersion > 409)
		{
			saveGameInfo.Name = "{{R|Older Version (" + json.GameVersion + ")}} " + saveGameInfo.Name;
		}
		return saveGameInfo;
	}

	public static bool HasSavedGameInfo()
	{
		foreach (string savedGamePath in SavedGamePaths)
		{
			if (!Platform.IO.Directory.Exists(savedGamePath))
			{
				continue;
			}
			EnumerateDirectoriesResult enumerateDirectoriesResult = Folder.EnumerateDirectories(savedGamePath);
			if (!enumerateDirectoriesResult.WasSuccessful())
			{
				return false;
			}
			string[] directories = enumerateDirectoriesResult.directories;
			foreach (string text in directories)
			{
				string[] infoFileNames = InfoFileNames;
				foreach (string text2 in infoFileNames)
				{
					if (Platform.IO.File.Exists(Platform.IO.Path.Combine(text, text2)))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private static void FatalSaveError(Exception e, string path)
	{
		MetricsManager.LogError("Error checking for save files", e);
		using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		if (e is UnauthorizedAccessException || e is IOException)
		{
			utf16ValueStringBuilder.AppendLine("There was a permission error while trying to access your save directory.");
			utf16ValueStringBuilder.AppendLine();
			utf16ValueStringBuilder.AppendLine(ColorUtility.EscapeFormatting(e.Message));
		}
		else
		{
			utf16ValueStringBuilder.AppendLine("There was an error while trying to access your save directory.");
			utf16ValueStringBuilder.AppendLine();
			utf16ValueStringBuilder.Append("Directory: ");
			utf16ValueStringBuilder.AppendLine(ColorUtility.EscapeFormatting(path));
		}
		utf16ValueStringBuilder.AppendLine();
		utf16ValueStringBuilder.AppendLine("Caves of Qud will exit now since we cannot save games. Please check your directory’s permissions.");
		Popup.WaitNewPopupMessage(utf16ValueStringBuilder.ToString(), new List<QudMenuItem>
		{
			new QudMenuItem
			{
				text = "Quit",
				hotkey = "Accept,Cancel",
				command = "Cancel"
			}
		}, null, null, "Error reading save location.");
		GameManager.Instance.uiQueue.queueTask(GameManager.Instance.Quit);
	}

	public static async Task<List<SaveGameInfo>> GetSavedGameInfo()
	{
		List<SaveGameInfo> result = new List<SaveGameInfo>();
		foreach (string path in SavedGamePaths)
		{
			try
			{
				if (!Platform.IO.Directory.Exists(path))
				{
					continue;
				}
				EnumerateDirectoriesResult enumerateDirectoriesResult = await Folder.EnumerateDirectoriesAsync(path);
				enumerateDirectoriesResult.LogErrorIfFailed();
				string[] directories = enumerateDirectoriesResult.directories;
				int num = 0;
				while (true)
				{
					if (num < directories.Length)
					{
						SaveGameInfo saveGameInfo = await GetDirectoryInfo(directories[num]);
						if (saveGameInfo != null)
						{
							result.Add(saveGameInfo);
						}
						num++;
						continue;
					}
					goto end_IL_004c;
				}
				end_IL_004c:;
			}
			catch (Exception e)
			{
				FatalSaveError(e, path);
			}
		}
		result.Sort(SortGameByDate);
		return result;
	}

	private static async Task<SaveGameInfo> GetDirectoryInfo(string dirPath)
	{
		_ = 1;
		try
		{
			if (Platform.IO.Path.GetFileNameWithoutExtension(dirPath).EqualsNoCase("mods") || Platform.IO.Path.GetFileNameWithoutExtension(dirPath).EqualsNoCase("textures"))
			{
				return null;
			}
			string[] infoFileNames = InfoFileNames;
			foreach (string text in infoFileNames)
			{
				string path = Platform.IO.Path.Combine(dirPath, text);
				if (await Platform.IO.File.ExistsAsync(path))
				{
					return await ReadSaveJson(dirPath, path);
				}
			}
			if (!Platform.IO.Directory.EnumerateFiles(dirPath).Any((string file) => !file.EndsWith("Cache.db")))
			{
				try
				{
					Platform.IO.Directory.Delete(dirPath);
				}
				catch (Exception message)
				{
					MetricsManager.LogWarning(message);
				}
			}
			else
			{
				MetricsManager.LogWarning("Weird save directory with no .json file present: " + DataManager.SanitizePathForDisplay(dirPath));
			}
		}
		catch (ThreadInterruptedException ex)
		{
			throw ex;
		}
		catch (Exception ex2)
		{
			MetricsManager.LogWarning(ex2.ToString());
		}
		return null;
	}

	private static int SortGameByDate(SaveGameInfo I1, SaveGameInfo I2)
	{
		try
		{
			if (string.IsNullOrEmpty(I1.SaveTime) || !I1.SaveTime.Contains(" at "))
			{
				return 1;
			}
			if (string.IsNullOrEmpty(I2.SaveTime) || !I2.SaveTime.Contains(" at "))
			{
				return -1;
			}
			string text = I1.SaveTime.Substring(0, I1.SaveTime.IndexOf(" at "));
			string text2 = I1.SaveTime.Substring(I1.SaveTime.IndexOf(" at ") + 4);
			string text3 = I2.SaveTime.Substring(0, I2.SaveTime.IndexOf(" at "));
			string text4 = I2.SaveTime.Substring(I2.SaveTime.IndexOf(" at ") + 4);
			DateTime value = DateTime.Parse(text + " " + text2);
			return DateTime.Parse(text3 + " " + text4).CompareTo(value);
		}
		catch
		{
			return 0;
		}
	}
}
