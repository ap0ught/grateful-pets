using System.Collections.Generic;
using Newtonsoft.Json;
using Platform.IO;

namespace XRL;

public static class BuildLibrary
{
	public static List<BuildEntry> _BuildEntries;

	public static List<BuildEntry> BuildEntries
	{
		get
		{
			if (_BuildEntries == null)
			{
				Init();
			}
			return _BuildEntries;
		}
	}

	public static void Init()
	{
		if (_BuildEntries == null)
		{
			string path = DataManager.SyncedPath("BuildLibrary.json");
			_BuildEntries = new List<BuildEntry>();
			if (File.Exists(path))
			{
				_BuildEntries = JsonConvert.DeserializeObject<List<BuildEntry>>(File.ReadAllText(path));
			}
		}
	}

	private static void Save()
	{
		if (BuildEntries != null)
		{
			File.WriteAllText(DataManager.SyncedPath("BuildLibrary.json"), JsonConvert.SerializeObject(BuildEntries));
		}
	}

	public static bool AddBuild(string Code, string Name, string Tile = null, string Foreground = null, string Detail = null)
	{
		if (string.IsNullOrEmpty(Name))
		{
			return false;
		}
		if (BuildEntries == null)
		{
			Init();
		}
		if (HasBuild(Code))
		{
			return false;
		}
		BuildEntry buildEntry = new BuildEntry();
		buildEntry.Code = Code;
		buildEntry.Name = Name;
		buildEntry.Tile = Tile;
		buildEntry.Foreground = Foreground;
		buildEntry.Detail = Detail;
		BuildEntries.Add(buildEntry);
		Save();
		return true;
	}

	public static BuildEntry GetBuild(string Code)
	{
		if (BuildEntries == null)
		{
			Init();
		}
		for (int i = 0; i < BuildEntries.Count; i++)
		{
			if (BuildEntries[i].Code == Code)
			{
				return BuildEntries[i];
			}
		}
		return null;
	}

	public static bool HasBuild(string Code)
	{
		if (BuildEntries == null)
		{
			Init();
		}
		for (int i = 0; i < BuildEntries.Count; i++)
		{
			if (BuildEntries[i].Code == Code)
			{
				return true;
			}
		}
		return false;
	}

	public static void UpdateBuild(BuildEntry Info)
	{
		if (BuildEntries == null)
		{
			Init();
		}
		for (int i = 0; i < BuildEntries.Count; i++)
		{
			if (BuildEntries[i].Code == Info.Code)
			{
				BuildEntries[i].Name = Info.Name;
				BuildEntries[i].Tile = Info.Tile;
				BuildEntries[i].Foreground = Info.Foreground;
				BuildEntries[i].Detail = Info.Detail;
				Save();
				break;
			}
		}
	}

	public static void DeleteBuild(string Code)
	{
		if (BuildEntries == null)
		{
			Init();
		}
		for (int i = 0; i < BuildEntries.Count; i++)
		{
			if (BuildEntries[i].Code == Code)
			{
				BuildEntries.RemoveAt(i);
				Save();
				break;
			}
		}
	}
}
