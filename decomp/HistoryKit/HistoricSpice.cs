using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Text;
using Newtonsoft.Json.Linq;
using XRL;
using XRL.Collections;
using XRL.UI;

namespace HistoryKit;

[HasModSensitiveStaticCache]
public static class HistoricSpice
{
	[ModSensitiveStaticCache(false)]
	private static JObject _root;

	[ModSensitiveStaticCache(false)]
	public static Dictionary<string, JContainer> _roots;

	public static JObject root
	{
		get
		{
			CheckInit();
			return _root;
		}
	}

	public static Dictionary<string, JContainer> roots
	{
		get
		{
			CheckInit();
			return _roots;
		}
	}

	public static void CheckInit()
	{
		if (_roots == null)
		{
			Loading.LoadTask("Loading HistorySpice.json", Init);
		}
	}

	private static void Init()
	{
		if (_roots != null)
		{
			return;
		}
		JObject jObject = JObject.Parse(File.ReadAllText(DataManager.FilePath("HistorySpice.json")));
		JsonMergeSettings settings = new JsonMergeSettings
		{
			MergeArrayHandling = MergeArrayHandling.Union,
			MergeNullValueHandling = MergeNullValueHandling.Ignore,
			PropertyNameComparison = StringComparison.Ordinal
		};
		foreach (ModInfo activeMod in ModManager.ActiveMods)
		{
			foreach (ModFile file in activeMod.Files)
			{
				if (file.Type == ModFileType.JSON && file.Name == "historyspice.json")
				{
					try
					{
						JObject content = JObject.Parse(File.ReadAllText(file.OriginalName));
						jObject.Merge(content, settings);
					}
					catch (Exception message)
					{
						MetricsManager.LogModError(activeMod, message);
					}
				}
			}
		}
		JObject jObject2 = (JObject)jObject["spice"];
		Dictionary<string, JContainer> dictionary = new Dictionary<string, JContainer>();
		_root = jObject2;
		_roots = dictionary;
		foreach (JProperty item2 in jObject2.Properties())
		{
			dictionary.Add(item2.Name, (JContainer)item2.Value);
		}
		RingDeque<string> ringDeque = new RingDeque<string>();
		ringDeque.Enqueue("spice");
		foreach (var (item, container) in dictionary)
		{
			ringDeque.Enqueue(item);
			ResolveRelativeLinks(ringDeque, container);
			ringDeque.Eject();
		}
	}

	private static void ResolveRelativeLinks(RingDeque<string> Stack, JContainer Container)
	{
		foreach (JToken item in Container.Children())
		{
			if (item.Type != JTokenType.String)
			{
				continue;
			}
			string text = (string?)item;
			Match match = Regex.Match(text, "<\\^.*?>");
			if (!match.Success)
			{
				continue;
			}
			using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
			using Utf16ValueStringBuilder utf16ValueStringBuilder2 = ZString.CreateStringBuilder();
			utf16ValueStringBuilder.Append(text);
			while (!match.Value.IsNullOrEmpty())
			{
				string value = match.Value;
				int num = value.IndexOf("^.", StringComparison.Ordinal);
				if (num == -1)
				{
					match = match.NextMatch();
					continue;
				}
				int num2 = 1;
				int num3 = num - 1;
				while (num3 >= 0 && value[num3] == '^')
				{
					num3--;
					num2++;
				}
				utf16ValueStringBuilder2.Clear();
				utf16ValueStringBuilder2.Append('<');
				int i = 0;
				for (int num4 = Stack.Count - num2; i < num4; i++)
				{
					utf16ValueStringBuilder2.Append(Stack[i]);
					utf16ValueStringBuilder2.Append('.');
				}
				utf16ValueStringBuilder2.Append(value, num + 2, value.Length - 3);
				int num5 = utf16ValueStringBuilder.Length - text.Length;
				utf16ValueStringBuilder.Remove(match.Index + num5, match.Length);
				utf16ValueStringBuilder.Insert(match.Index + num5, utf16ValueStringBuilder2.AsSpan(), 1);
				match = match.NextMatch();
			}
			((JValue)item).Value = utf16ValueStringBuilder.ToString();
		}
		if (!(Container is JObject jObject))
		{
			return;
		}
		foreach (JProperty item2 in jObject.Properties())
		{
			if (item2.Value is JContainer container)
			{
				Stack.Enqueue(item2.Name);
				ResolveRelativeLinks(Stack, container);
				Stack.Eject();
			}
		}
	}
}
