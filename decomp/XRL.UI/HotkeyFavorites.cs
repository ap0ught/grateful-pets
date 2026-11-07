using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;

namespace XRL.UI;

public static class HotkeyFavorites
{
	public static Dictionary<string, List<int>> commandToKey;

	public static Dictionary<string, List<int>> LoadForUpgrade()
	{
		Load();
		return commandToKey;
	}

	private static void Load()
	{
		commandToKey = new Dictionary<string, List<int>>();
		try
		{
			string path = DataManager.SavePath("HotkeyFavorites.json");
			if (!File.Exists(path))
			{
				return;
			}
			string text = "";
			using (StreamReader streamReader = new StreamReader(path))
			{
				text = streamReader.ReadToEnd();
			}
			if (!(text != ""))
			{
				return;
			}
			foreach (KeyValuePair<string, JSONNode> item in JSON.Parse(text) as JSONClass)
			{
				string key = item.Key;
				string text2 = item.Value;
				List<int> list = new List<int>();
				if (!string.IsNullOrEmpty(text2))
				{
					string[] array = text2.Split(',');
					foreach (string value in array)
					{
						list.Add(Convert.ToInt32(value));
					}
				}
				commandToKey.Add(key, list);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("HotkeyFavorites::Load", x);
		}
	}

	private static void Save()
	{
		try
		{
			string path = DataManager.SavePath("HotkeyFavorites.json");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("{");
			int num = 0;
			foreach (KeyValuePair<string, List<int>> item in commandToKey)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				for (int i = 0; i < item.Value.Count; i++)
				{
					stringBuilder2.Append(item.Value[i]);
					if (i != item.Value.Count - 1)
					{
						stringBuilder2.Append(",");
					}
				}
				stringBuilder.Append(" \"" + item.Key + "\":\"" + stringBuilder2?.ToString() + "\"");
				if (num != commandToKey.Keys.Count - 1)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.AppendLine();
				num++;
			}
			stringBuilder.AppendLine("}");
			File.WriteAllText(path, stringBuilder.ToString());
		}
		catch (Exception x)
		{
			MetricsManager.LogError("HotkeyFavorites::Save", x);
		}
	}

	public static List<int> GetFavorites(string command)
	{
		if (commandToKey == null)
		{
			Load();
		}
		if (commandToKey.TryGetValue(command, out var value))
		{
			return value;
		}
		return null;
	}

	public static void AddFavorite(string command, int key)
	{
		if (commandToKey == null)
		{
			Load();
		}
		if (commandToKey.TryGetValue(command, out var value))
		{
			value.Insert(0, key);
		}
		else
		{
			List<int> list = new List<int>();
			commandToKey.Add(command, list);
			list.Add(key);
		}
		Save();
	}
}
