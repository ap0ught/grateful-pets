using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Platform.IO;
using SimpleJSON;

namespace XRL.UI;

public class NameValueBag
{
	private string fileName;

	public Dictionary<string, string> Bag;

	public NameValueBag(string Path)
	{
		fileName = Path;
		Bag = new Dictionary<string, string>();
	}

	public string GetValue(string Name, string Default = null)
	{
		if (Bag == null)
		{
			Load();
		}
		if (Bag.TryGetValue(Name, out var value))
		{
			return value;
		}
		return Default;
	}

	public void SetValue(string Name, string Value, bool FlushToFile = true)
	{
		Bag[Name] = Value;
		if (FlushToFile)
		{
			Flush().Wait();
		}
	}

	public void Load()
	{
		Bag = new Dictionary<string, string>();
		if (!File.Exists(fileName))
		{
			return;
		}
		try
		{
			foreach (KeyValuePair<string, JSONNode> item in JSON.Parse(File.ReadAllText(fileName)) as JSONClass)
			{
				SetValue(item.Key, item.Value, FlushToFile: false);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Failed to parse " + fileName + "!", x);
		}
	}

	public async Task Flush()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{\n");
		int num = 0;
		foreach (string key in Bag.Keys)
		{
			if (num != 0)
			{
				stringBuilder.Append(",\n");
			}
			num++;
			stringBuilder.Append("\"" + key + "\":\"" + Bag[key] + "\"");
		}
		stringBuilder.Append("\n}");
		(await Blob.WriteAllTextAsync(fileName, stringBuilder.ToString())).LogIfErrored();
	}
}
