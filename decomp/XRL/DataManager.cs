using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using LaundryBear;
using Mono.Data.Sqlite;
using Platform.IO;
using UnityEngine;
using XRL.Core;

namespace XRL;

[HasModSensitiveStaticCache]
[HasGameBasedStaticCache]
public static class DataManager
{
	public struct CacheOperation : IDisposable
	{
		private SqliteCommand _Command;

		private Latcher.Lock Lock;

		public SqliteCommand Command => _Command ?? (_Command = RequireCacheConnection().CreateCommand());

		public CacheOperation(Latcher.Lock Lock)
		{
			_Command = null;
			this.Lock = Lock;
		}

		public void Dispose()
		{
			if (_Command != null)
			{
				_Command.Dispose();
				_Command = null;
			}
			Lock.Release();
		}
	}

	public struct CacheTransaction(Latcher.Lock Lock) : IDisposable
	{
		private List<SqliteCommand> _Commands = new List<SqliteCommand>();

		private SqliteTransaction _Transaction = RequireCacheConnection().BeginTransaction();

		private Latcher.Lock Lock = Lock;

		private bool _Committed = false;

		private bool _Rolledback = false;

		public SqliteCommand AddCommand()
		{
			if (_Committed || _Rolledback)
			{
				throw new SqliteException("Cannot add additional commands to a committed or rolled back transaction.");
			}
			SqliteCommand sqliteCommand = Connection.CreateCommand();
			sqliteCommand.Transaction = _Transaction;
			_Commands.Add(sqliteCommand);
			return sqliteCommand;
		}

		public void CommitTransaction()
		{
			if (_Committed || _Rolledback)
			{
				throw new SqliteException("Cannot commit a transaction that has already been committed or roleld back.");
			}
			_Transaction.Commit();
			_Committed = true;
		}

		public void Rollback()
		{
			if (_Committed || _Rolledback)
			{
				throw new SqliteException("Cannot roll back a transaction that has already been committed or roleld back.");
			}
			_Transaction.Rollback();
			_Rolledback = true;
		}

		public void Dispose()
		{
			if (!_Committed && !_Rolledback)
			{
				CommitTransaction();
			}
			_Transaction.Dispose();
			_Transaction = null;
			for (int num = _Commands.Count - 1; num >= 0; num--)
			{
				SqliteCommand sqliteCommand = _Commands[num];
				_Commands.RemoveAt(num);
				sqliteCommand.Dispose();
			}
			_Commands = null;
			Lock.Release();
		}
	}

	public static Dictionary<string, string> contents = new Dictionary<string, string>();

	private static SqliteConnection Connection;

	private static Latcher ConnectionLock = new Latcher();

	public const string STEAM_PATH_REGEX = "^(.+)[/\\\\]steamapps[/\\\\]";

	private static Dictionary<string, List<DataFile>> XMLFilesByRoot = new Dictionary<string, List<DataFile>>(StringComparer.OrdinalIgnoreCase);

	private static bool XMLCacheInitialized;

	[GameBasedCacheInit]
	public static void ResetCacheConnection()
	{
		if (Connection != null)
		{
			Connection.Close();
			Connection.Dispose();
			Connection = null;
		}
	}

	public static void CloseCacheConnection()
	{
		if (Connection != null)
		{
			Connection.Close();
		}
	}

	public static bool HasMigration(string Migration)
	{
		using SqliteCommand sqliteCommand = Connection.CreateCommand();
		sqliteCommand.CommandText = "SELECT Migration FROM Migrations WHERE Migration = @Migration";
		sqliteCommand.Parameters.AddWithValue("Migration", Migration);
		using SqliteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
		return sqliteDataReader.HasRows;
	}

	public static SqliteCommand CreateMigrationUpgradeTransactionCommand(string Migration, string CommandRaw)
	{
		SqliteCommand sqliteCommand = Connection.CreateCommand();
		sqliteCommand.CommandText = "BEGIN; " + CommandRaw + "; INSERT INTO Migrations (Migration) VALUES (@Migration); COMMIT;";
		sqliteCommand.Parameters.AddWithValue("Migration", Migration);
		return sqliteCommand;
	}

	public static void CheckSqlMigrations(bool initialCreate)
	{
		if (initialCreate)
		{
			using (SqliteCommand sqliteCommand = CreateMigrationUpgradeTransactionCommand("FrozenZone_FrozenTick", "\r\n                    CREATE TABLE IF NOT EXISTS 'FrozenZone' (\r\n                        'ID' INTEGER PRIMARY KEY AUTOINCREMENT,\r\n                        'ZoneID' TEXT UNIQUE NOT NULL,\r\n                        'FrozenTick' INTEGER,\r\n                        'Data' BLOB NOT NULL\r\n                    );\r\n                    CREATE UNIQUE INDEX IF NOT EXISTS ZoneIDIndex ON FrozenZone (ZoneID);\r\n                "))
			{
				sqliteCommand.ExecuteNonQuery();
				MetricsManager.LogInfo("Creating Cache.db with FrozenZone_FrozenTick");
				return;
			}
		}
		if (!HasMigration("FrozenZone_FrozenTick"))
		{
			using (SqliteCommand sqliteCommand2 = CreateMigrationUpgradeTransactionCommand("FrozenZone_FrozenTick", "ALTER TABLE 'FrozenZone' ADD 'FrozenTick' INTEGER"))
			{
				sqliteCommand2.ExecuteNonQuery();
				MetricsManager.LogInfo("Upgrading Cache.db with FrozenZone_FrozenTick");
			}
		}
	}

	public static SqliteConnection RequireCacheConnection()
	{
		if (Connection == null)
		{
			string cacheDirectory = (The.Game ?? throw new NullReferenceException("Game is not initialized.")).GetCacheDirectory("Cache.db");
			bool initialCreate = !Platform.IO.File.Exists(cacheDirectory);
			Connection = new SqliteConnection("Data Source=" + cacheDirectory);
			Connection.Open();
			using (SqliteCommand sqliteCommand = Connection.CreateCommand())
			{
				sqliteCommand.CommandText = "BEGIN;CREATE TABLE IF NOT EXISTS 'Migrations' (   'ID' INTEGER PRIMARY KEY AUTOINCREMENT,   'Migration' TEXT UNIQUE NOT NULL );CREATE UNIQUE INDEX IF NOT EXISTS MigrationIndex ON Migrations (Migration); COMMIT;";
				sqliteCommand.ExecuteNonQuery();
			}
			CheckSqlMigrations(initialCreate);
		}
		else if (Connection.State == ConnectionState.Closed)
		{
			Connection.Open();
		}
		return Connection;
	}

	public static CacheOperation StartCacheOperation(bool Exclusive = false)
	{
		if (Exclusive)
		{
			return new CacheOperation(ConnectionLock.TakeWriteLock().Normalize());
		}
		return new CacheOperation(ConnectionLock.TakeReadLock().Normalize());
	}

	public static async Task<CacheOperation> StartCacheOperationAsync(bool Exclusive = false)
	{
		if (Exclusive)
		{
			return new CacheOperation((await ConnectionLock.TakeWriteLockAsync()).Normalize());
		}
		return new CacheOperation((await ConnectionLock.TakeReadLockAsync()).Normalize());
	}

	public static CacheTransaction StartCacheTransaction(bool Exclusive = false)
	{
		if (Exclusive)
		{
			return new CacheTransaction(ConnectionLock.TakeWriteLock().Normalize());
		}
		return new CacheTransaction(ConnectionLock.TakeReadLock().Normalize());
	}

	public static void Shutdown()
	{
		ResetCacheConnection();
	}

	public static async Task PerformFileLocationChangesAsAppropriate()
	{
		ServiceLocator.GetServiceOrThrow<PlatformService>();
		UnityEngine.Debug.Log("Performing File Location Copies...");
		await copyIfSrcExistsAndDstDoesnt(FilePath("UserPrefs.json"), SyncedPath("UserPrefs.json"));
		UnityEngine.Debug.Log("Performing File Location Upgrades...");
		await copyIfSrcExistsAndDstDoesnt(SavePath("UserPrefs.json"), SyncedPath("UserPrefs.json"));
		await copyIfSrcExistsAndDstDoesnt(SavePath("Achievements.json"), SyncedPath("Achievements.json"));
		await copyIfSrcExistsAndDstDoesnt(SavePath("HighScores.json"), SyncedPath("HighScores.json"));
		await copyIfSrcExistsAndDstDoesnt(SavePath("PlayerOptions.json"), SyncedPath("PlayerOptions.json"));
		await copyIfSrcExistsAndDstDoesnt(SavePath("ModSettings.json"), SyncedPath("ModSettings.json"));
		await copyIfSrcExistsAndDstDoesnt(SavePath("BuildLibrary.json"), SyncedPath("BuildLibrary.json"));
		string[] files = (await Folder.EnumerateFilesShallowAsync(SavePath(string.Empty))).files;
		foreach (string text in files)
		{
			if (text.EndsWith("Keymap.json") || text.EndsWith("Keymap2.json"))
			{
				string dstPath = SyncedPath(Platform.IO.Path.GetFileName(text));
				await copyIfSrcExistsAndDstDoesnt(text, dstPath);
			}
		}
		static async Task copyIfSrcExistsAndDstDoesnt(string srcPath, string text2)
		{
			if (await Platform.IO.File.ExistsAsync(srcPath) && !(await Platform.IO.File.ExistsAsync(text2)))
			{
				PlatformIOResult platformIOResult = await Blob.CopyAsync(srcPath, text2, overwrite: false);
				if (platformIOResult.WasSuccessful())
				{
					UnityEngine.Debug.Log("File Upgrade: Copying \"" + srcPath + "\" to \"" + text2 + "\". Not deleting old file in case some old mods still use the original locations.");
				}
				else
				{
					UnityEngine.Debug.LogWarning(platformIOResult.ToString());
				}
			}
		}
	}

	public static void preloadContents(string path)
	{
		try
		{
			string key = Platform.IO.Path.GetFileName(path).ToLower();
			if (!contents.ContainsKey(key))
			{
				string text = Platform.IO.Path.Combine(Platform.IO.Path.Combine(Application.streamingAssetsPath, "Base"), path);
				WWW wWW = new WWW(text);
				while (!wWW.isDone)
				{
					Thread.Sleep(0);
				}
				contents.Add(key, wWW.text);
				MetricsManager.LogInfo("loaded " + text + " length " + wWW.text.Length);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("error loading " + path, x);
		}
	}

	public static Stream GenerateStreamFromString(string s)
	{
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(s);
		streamWriter.Flush();
		memoryStream.Position = 0L;
		return memoryStream;
	}

	public static XmlDataHelper GetXMLStream(string FileName, ModInfo modInfo)
	{
		string key = Platform.IO.Path.GetFileName(FileName).ToLower();
		if (contents.ContainsKey(key))
		{
			return new XmlDataHelper(GenerateStreamFromString(contents[key]), modInfo);
		}
		return new XmlDataHelper(FilePath(FileName), modInfo);
	}

	public static XmlTextReader GetStreamingAssetsXMLStream(string FileName)
	{
		string key = Platform.IO.Path.GetFileName(FileName).ToLower();
		if (contents.ContainsKey(key))
		{
			return new XmlTextReader(GenerateStreamFromString(contents[key]));
		}
		return new XmlTextReader(FilePath(FileName));
	}

	public static StreamReader GetStreamingAssetsStreamReader(string FileName)
	{
		string key = Platform.IO.Path.GetFileName(FileName).ToLower();
		if (contents.ContainsKey(key))
		{
			return new StreamReader(GenerateStreamFromString(contents[key]));
		}
		return new StreamReader(FilePath(FileName));
	}

	public static string EmbeddedModsPath()
	{
		if (string.IsNullOrWhiteSpace(XRLCore.EmbeddedModsPath))
		{
			XRLCore.EmbeddedModsPath = Platform.IO.Path.Combine(Application.streamingAssetsPath, "Mods");
		}
		return XRLCore.EmbeddedModsPath;
	}

	public static string OSXDLCPath()
	{
		if (string.IsNullOrWhiteSpace(XRLCore.OSXDLCPath))
		{
			XRLCore.OSXDLCPath = Platform.IO.Path.Combine(Application.dataPath, "../../CoQ_Data/StreamingAssets/DLC");
		}
		return XRLCore.OSXDLCPath;
	}

	public static string OSXDLCPath(string file)
	{
		if (string.IsNullOrWhiteSpace(XRLCore.OSXDLCPath))
		{
			XRLCore.OSXDLCPath = Platform.IO.Path.Combine(Application.dataPath, "../../CoQ_Data/StreamingAssets/DLC");
		}
		return Platform.IO.Path.Combine(XRLCore.OSXDLCPath, file);
	}

	public static string DLCPath()
	{
		if (string.IsNullOrWhiteSpace(XRLCore.DLCPath))
		{
			XRLCore.DLCPath = Platform.IO.Path.Combine(Application.streamingAssetsPath, "DLC");
		}
		return XRLCore.DLCPath;
	}

	public static string DLCPath(string file)
	{
		if (string.IsNullOrWhiteSpace(XRLCore.DLCPath))
		{
			XRLCore.DLCPath = Platform.IO.Path.Combine(Application.streamingAssetsPath, "DLC");
		}
		return Platform.IO.Path.Combine(XRLCore.DLCPath, file);
	}

	public static string FilePath()
	{
		if (string.IsNullOrWhiteSpace(XRLCore.DataPath))
		{
			XRLCore.DataPath = Platform.IO.Path.Combine(Application.streamingAssetsPath, "Base");
		}
		return XRLCore.DataPath;
	}

	public static string FilePath(string FileName)
	{
		if (string.IsNullOrWhiteSpace(XRLCore.DataPath))
		{
			XRLCore.DataPath = Platform.IO.Path.Combine(Application.streamingAssetsPath, "Base");
		}
		return Platform.IO.Path.Combine(XRLCore.DataPath, FileName);
	}

	/// <returns> A read/write-able path that can be used with System.IO </returns>
	/// <remarks> If you want to write portable I/O, prefer Platform.Path.RW, which must be used with Platform.IO or IStorage</remarks>
	public static string SavePath(string FileName)
	{
		return Platform.IO.Path.Combine(XRLCore.SavePath, FileName);
	}

	/// <returns> A read/write-able path that can be used with System.IO </returns>
	/// <remarks> If you want to write portable I/O, prefer Platform.Path.RWLocal, which must be used with Platform.IO or IStorage</remarks>
	public static string LocalPath(string FileName)
	{
		return Platform.IO.Path.Combine(XRLCore.LocalPath, FileName);
	}

	/// <returns> A read/write-able path that is saved(/synced) to the cloud that can be used with System.IO </returns>
	/// <remarks> If you want to write portable I/O, consider Platform.Path.RWSynced, which must be used with Platform.IO or IStorage</remarks>
	public static string SyncedPath(string FileName)
	{
		return Platform.IO.Path.Combine(XRLCore.SyncedPath, FileName);
	}

	[Conditional("DEBUGGING_IO")]
	private static void LogIfDebuggingIO(string msg)
	{
		UnityEngine.Debug.Log(msg);
	}

	public static string SanitizePathForDisplay(string Source)
	{
		string text = SavePath("");
		if (text.IsNullOrEmpty())
		{
			return "<...>";
		}
		string text2 = Source.Replace(text, "<...>/CavesOfQud");
		text = SavePath("").Replace("/", "\\");
		return Regex.Replace(text2.Replace(text, "<...>"), "^(.+)[/\\\\]steamapps[/\\\\]", "<...>/steamapps/").Replace('\\', '/');
	}

	[ModSensitiveCacheInit]
	private static void ClearXMLCache()
	{
		if (XMLCacheInitialized && GameManager.AwakeComplete)
		{
			XMLFilesByRoot.Clear();
			XMLCacheInitialized = false;
		}
	}

	private static void CheckXMLInitialize()
	{
		if (XMLCacheInitialized)
		{
			return;
		}
		if (XMLFilesByRoot.IsNullOrEmpty())
		{
			System.IO.FileInfo[] files = new System.IO.DirectoryInfo(FilePath("")).GetFiles("*.xml", System.IO.SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				CacheFile(files[i].FullName);
			}
			SortXMLRoots();
		}
		if (!ModManager.Initialized)
		{
			return;
		}
		foreach (ModInfo activeMod in ModManager.ActiveMods)
		{
			foreach (ModFile file in activeMod.Files)
			{
				if (file.Type == ModFileType.XML)
				{
					CacheFile(file.OriginalName, activeMod);
				}
			}
		}
		SortXMLRoots();
		PrintXMLRoots();
		XMLCacheInitialized = true;
	}

	private static void SortXMLRoots()
	{
		foreach (KeyValuePair<string, List<DataFile>> item in XMLFilesByRoot)
		{
			item.Value.Sort();
		}
	}

	private static void PrintXMLRoots()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"Found {XMLFilesByRoot.Count} XML roots;");
		foreach (var (value, list2) in XMLFilesByRoot)
		{
			int num = 0;
			int num2 = 0;
			foreach (DataFile item in list2)
			{
				if (item.IsBase)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
			stringBuilder.Append("\n - ").AppendUpper(value).Append(": ");
			if (num != 0)
			{
				stringBuilder.AppendThings(num, "base file", "base files");
			}
			if (num2 != 0)
			{
				if (num != 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.AppendThings(num2, "mod file", "mod files");
			}
		}
		MetricsManager.LogInfo(stringBuilder.ToString());
	}

	private static void CacheFile(string Path, ModInfo Mod = null)
	{
		try
		{
			using XmlReader xmlReader = XmlReader.Create(Path);
			while (xmlReader.Read())
			{
				if (!xmlReader.Name.IsNullOrEmpty() && !(xmlReader.Name == "xml"))
				{
					if (!XMLFilesByRoot.TryGetValue(xmlReader.Name, out var value))
					{
						value = (XMLFilesByRoot[xmlReader.Name] = new List<DataFile>());
					}
					int priority = (int.TryParse(xmlReader.GetAttribute("LoadPriority"), out var result) ? result : 0);
					value.Add(new DataFile
					{
						Path = Path,
						Mod = Mod,
						Priority = priority
					});
					break;
				}
			}
		}
		catch (Exception ex)
		{
			MetricsManager.LogPotentialModError(Mod, Path + ": " + ex.Message + "\n" + ex.StackTrace.ToString());
		}
	}

	public static void DeleteSaveDirectory(string Path)
	{
		if (Path.IsNullOrEmpty())
		{
			return;
		}
		using (StartCacheOperation(Exclusive: true))
		{
			ResetCacheConnection();
			int num = 0;
			while (true)
			{
				try
				{
					Platform.IO.Directory.Delete(Path);
					break;
				}
				catch (Exception x)
				{
					if (num++ < 20)
					{
						Thread.Sleep(50);
						continue;
					}
					MetricsManager.LogException("Error deleting saved game", x);
					break;
				}
			}
		}
	}

	/// <summary>Get list of XML files with root element within both base and mod directories.</summary>
	public static List<DataFile> GetXMLFilesWithRoot(string Root)
	{
		CheckXMLInitialize();
		return XMLFilesByRoot.GetValue(Root) ?? new List<DataFile>();
	}

	/// <summary>Yield XmlDataHelper streams for each file in <see cref="M:XRL.DataManager.GetXMLFilesWithRoot(System.String)" />.</summary>
	/// <remarks>Stream is disposed on next enumeration.</remarks>
	public static IEnumerable<XmlDataHelper> YieldXMLStreamsWithRoot(string Root, bool IncludeBase = true, bool IncludeMods = true)
	{
		foreach (DataFile item in GetXMLFilesWithRoot(Root))
		{
			if ((IncludeBase || !item.IsBase) && (IncludeMods || !item.IsMod))
			{
				using XmlDataHelper stream = GetXMLStream(item, item.Mod);
				yield return stream;
			}
		}
	}

	public static void Open(string Path)
	{
		Path = System.IO.Path.GetFullPath(Path);
		try
		{
			if (Process.Start("open", "\"" + Path + "\"") != null)
			{
				return;
			}
		}
		catch (Exception)
		{
		}
		try
		{
			if (Process.Start("xdg-open", "\"" + Path + "\"") != null)
			{
				return;
			}
		}
		catch (Exception)
		{
		}
		try
		{
			Process.Start("explorer", "\"" + Path + "\"");
		}
		catch (Exception)
		{
		}
	}
}
