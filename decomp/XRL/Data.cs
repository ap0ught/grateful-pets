using System.Collections.Generic;
using System.IO;
using SimpleJSON;

namespace XRL;

public static class Data
{
	private static Dictionary<string, string> TextData;

	private static void CheckInit()
	{
		if (TextData != null)
		{
			return;
		}
		TextData = new Dictionary<string, string>();
		using StreamReader streamReader = DataManager.GetStreamingAssetsStreamReader("Text.txt");
		foreach (KeyValuePair<string, JSONNode> item in JSON.Parse(streamReader.ReadToEnd()) as JSONClass)
		{
			TextData.Add(item.Key, item.Value);
		}
	}

	public static bool HasText(string Name)
	{
		CheckInit();
		return TextData.ContainsKey(Name);
	}

	public static string GetText(string Name)
	{
		CheckInit();
		return TextData[Name];
	}

	public static bool TryGetText(string Name, out string Result)
	{
		CheckInit();
		return TextData.TryGetValue(Name, out Result);
	}
}
