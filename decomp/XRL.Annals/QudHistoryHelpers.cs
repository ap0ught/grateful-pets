using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using XRL.Language;
using XRL.Rules;
using XRL.World;

namespace XRL.Annals;

public static class QudHistoryHelpers
{
	public static HistoricEntitySnapshot GetRegionalizationParametersSnapshot(History history)
	{
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals("regionalizationParameters")).GetRandomElement().GetSnapshotAtYear(history.currentYear);
	}

	public static string GetNewRegion(History history, string currentRegion)
	{
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("type").Equals("region") && !entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals(currentRegion)).GetRandomElement().GetSnapshotAtYear(history.currentYear)
			.GetProperty("name");
	}

	public static List<string> GetLocationsInRegion(History history, string region)
	{
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals(region)).GetRandomElement().GetSnapshotAtYear(history.currentYear)
			.GetList("locations");
	}

	public static List<string> GetLocationsInRegionByPeriod(History history, string region, int period)
	{
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals(region) && int.Parse(entity.GetSnapshotAtYear(entity.lastYear).GetProperty("period")) <= period).GetRandomElement().GetSnapshotAtYear(history.currentYear)
			.GetList("locations");
	}

	public static string GetRandomLocationInRegion(History history, string region)
	{
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals(region)).GetRandomElement().GetSnapshotAtYear(history.currentYear)
			.GetList("locations")
			.GetRandomElement();
	}

	public static string GetRandomLocationInRegionByPeriod(History history, string region, int period)
	{
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals(region) && int.Parse(entity.GetSnapshotAtYear(entity.lastYear).GetProperty("period")) <= period).GetRandomElement().GetSnapshotAtYear(history.currentYear)
			.GetList("locations")
			.GetRandomElement();
	}

	public static string GetNewLocationInRegion(History history, string region, string currentLocation)
	{
		HistoricEntityList entitiesByDelegate = history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("type").Equals("location") && entity.GetSnapshotAtYear(entity.lastYear).GetProperty("region").Equals(region) && !entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals(currentLocation));
		if (entitiesByDelegate.Count > 0)
		{
			return entitiesByDelegate.GetRandomElement().GetSnapshotAtYear(history.currentYear).GetProperty("name");
		}
		HistoricEntity historicEntity = history.CreateEntity(history.currentYear);
		historicEntity.ApplyEvent(new InitializeLocation(region, 0));
		return historicEntity.GetCurrentSnapshot().GetProperty("name");
	}

	public static string GetNewLocationInRegionByPeriod(History history, string region, string currentLocation, int period)
	{
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("type").Equals("location") && entity.GetSnapshotAtYear(entity.lastYear).GetProperty("region").Equals(region) && !entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals(currentLocation) && int.Parse(entity.GetSnapshotAtYear(entity.lastYear).GetProperty("period")) <= period).GetRandomElement().GetSnapshotAtYear(history.currentYear)
			.GetProperty("name");
	}

	public static string GetRegionNewName(History history, string region)
	{
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals(region)).GetRandomElement().GetSnapshotAtYear(history.currentYear)
			.GetProperty("newName");
	}

	public static string GetRegionNameRoot(History history, string region)
	{
		return history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("name").Equals(region)).GetRandomElement().GetSnapshotAtYear(history.currentYear)
			.GetProperty("nameRoot");
	}

	public static string GetNewFaction(HistoricEntity entity)
	{
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string name = Factions.GetRandomOldFaction().Name;
		while (snapshotAtYear.GetList("likedFactions").Contains(name) || snapshotAtYear.GetList("hatedFactions").Contains(name))
		{
			name = Factions.GetRandomOldFaction().Name;
		}
		return name;
	}

	private static void DoBrightnessCycle(List<char> pattern, SolidColor color1, SolidColor color2)
	{
		int num = 2 * ((!color1.DarkTone.HasValue) ? 1 : 2) * ((!color1.LightTone.HasValue) ? 1 : 2);
		int num2 = 2 * ((!color2.DarkTone.HasValue) ? 1 : 2) * ((!color2.LightTone.HasValue) ? 1 : 2);
		int num3 = num + num2;
		int num4 = Math.Max(num, num2);
		int num5 = Stat.Random(0, num4 - 1);
		int num6 = ((num4 >= 6) ? 3 : 2);
		int num7 = ((num4 >= 6) ? 1 : 2);
		for (int i = 0; i < num3; i++)
		{
			SolidColor solidColor = ((i % 2 == 0) ? color1 : color2);
			if (num5 <= num7 && solidColor.DarkTone.HasValue)
			{
				pattern.Add(solidColor.DarkTone.Value);
			}
			else if (num5 >= num6 && solidColor.LightTone.HasValue)
			{
				pattern.Add(solidColor.LightTone.Value);
			}
			else
			{
				pattern.Add(solidColor.Foreground.Value);
			}
			num5 = (num5 + 1) % num4;
		}
	}

	private static void DoHighlighting(string text, List<char> pattern, SolidColor color, char basic, int highlightChance = 25, int toneChance = 50, int darkToneChance = 50, int maxPatternLength = 10)
	{
		if (text != null && text.Length < maxPatternLength)
		{
			maxPatternLength = text.Length;
		}
		bool flag = false;
		int i = 0;
		for (int num = maxPatternLength - 1; i < num; i++)
		{
			if (highlightChance.in100())
			{
				char c = '\0';
				if ((color.DarkTone.HasValue || color.LightTone.HasValue) && toneChance.in100())
				{
					if (color.DarkTone.HasValue && darkToneChance.in100())
					{
						c = color.DarkTone.Value;
					}
					else if (color.LightTone.HasValue)
					{
						c = color.LightTone.Value;
					}
				}
				if (c == '\0')
				{
					c = color.Foreground ?? 'y';
				}
				pattern.Add(c);
				i++;
				flag = true;
			}
			else
			{
				pattern.Add(basic);
			}
		}
		if (!flag)
		{
			pattern.Clear();
		}
		pattern.Add(basic);
	}

	private static void DoHighlighting(string text, List<char> pattern, SolidColor color, SolidColor basic, int highlightChance = 25, int toneChance = 50, int darkToneChance = 50, int maxPatternLength = 10)
	{
		DoHighlighting(text, pattern, color, basic.Foreground.Value, highlightChance, toneChance, darkToneChance, maxPatternLength);
	}

	private static string ComposePattern(string text, List<char> pattern, string patternType)
	{
		return pattern.Count switch
		{
			0 => text, 
			1 => "{{" + pattern[0] + "|" + text + "}}", 
			_ => "{{" + string.Join("-", pattern.Select((char c) => char.ToString(c)).ToArray()) + " " + patternType + "|" + text + "}}", 
		};
	}

	public static string Ansify(string Name, string Article = null)
	{
		MarkupShaders.CheckInit();
		List<SolidColor> list = new List<SolidColor>(16);
		foreach (SolidColor pickerColor in MarkupShaders.PickerColors)
		{
			if ((!pickerColor.Background.HasValue || pickerColor.Background == 'k') && pickerColor.Foreground.HasValue && pickerColor.Foreground != 'o' && pickerColor.Foreground != 'O')
			{
				list.Add(pickerColor);
			}
		}
		List<SolidColor> list2 = new List<SolidColor>();
		list2.Add(list.GetRandomElement());
		int num = 0;
		SolidColor randomElement;
		do
		{
			randomElement = list.GetRandomElement();
		}
		while (list2.Contains(randomElement) && ++num < 100);
		list2.Add(randomElement);
		num = 0;
		do
		{
			randomElement = list.GetRandomElement();
		}
		while (list2.Contains(randomElement) && ++num < 100);
		list2.Add(randomElement);
		string text = Name;
		int num2 = Stat.Random(0, 100);
		List<char> list3 = new List<char>();
		string patternType = "sequence";
		if (num2 < 20)
		{
			DoBrightnessCycle(list3, list2[0], list2[1]);
		}
		else if (num2 < 40)
		{
			DoHighlighting(Name, list3, list2[0], 'Y');
		}
		else if (num2 < 600)
		{
			DoHighlighting(Name, list3, list2[0], list2[1]);
		}
		else
		{
			text = "";
			int num3 = Stat.Random(0, 100);
			string[] array = Name.Split(' ');
			int i = 0;
			for (int num4 = array.Length; i < num4; i++)
			{
				string text2 = array[i];
				switch (text2)
				{
				case "the":
				case "The":
				case "of":
					text = text + "{{Y|" + text2 + "}}";
					break;
				default:
				{
					num3 = (50.in100() ? (num3 - 5) : (num3 + 5));
					if (num3 < 20)
					{
						DoBrightnessCycle(list3, list2[0], list2[1]);
						text += ComposePattern(text2, list3, patternType);
						list3.Clear();
						break;
					}
					if (num3 < 40)
					{
						DoHighlighting(text2, list3, list2[1], 'Y');
						text += ComposePattern(text2, list3, patternType);
						list3.Clear();
						break;
					}
					if (num3 < 55)
					{
						DoHighlighting(text2, list3, list2[1], list2[0]);
						text += ComposePattern(text2, list3, patternType);
						list3.Clear();
						break;
					}
					if (num3 < 70)
					{
						DoHighlighting(text2, list3, list2[2], list2[0]);
						text += ComposePattern(text2, list3, patternType);
						list3.Clear();
						break;
					}
					SolidColor solidColor = (50.in100() ? list2[0] : list2[1]);
					char c = '\0';
					if ((solidColor.DarkTone.HasValue || solidColor.LightTone.HasValue) && 50.in100())
					{
						if (solidColor.DarkTone.HasValue && 50.in100())
						{
							c = solidColor.DarkTone.Value;
						}
						else if (solidColor.LightTone.HasValue)
						{
							c = solidColor.LightTone.Value;
						}
					}
					if (c == '\0')
					{
						c = solidColor.Foreground ?? 'y';
					}
					text = text + "{{" + c + "|" + text2 + "}}";
					break;
				}
				}
				if (i < num4 - 1)
				{
					text += " ";
				}
			}
		}
		string text3 = ComposePattern(text, list3, patternType);
		if (!Article.IsNullOrEmpty())
		{
			text3 = Article + " " + text3;
		}
		return text3;
	}

	public static string NameItem(string obj, History history, HistoricEntity entity)
	{
		string wordRoot;
		do
		{
			wordRoot = Grammar.GetWordRoot(obj);
			wordRoot += HistoricStringExpander.ExpandString("<spice.items.suffixes.!random>", null, history);
		}
		while (history.GetEntitiesWherePropertyEquals("name", wordRoot).Count > 0);
		return Grammar.InitCap(wordRoot);
	}

	public static string NameItemNounRoot(string obj, History history, HistoricEntity entity)
	{
		string wordRoot;
		do
		{
			if (50.in100())
			{
				wordRoot = Grammar.GetWordRoot(obj);
				wordRoot += HistoricStringExpander.ExpandString("<spice.items.suffixes.!random>", null, history);
				continue;
			}
			wordRoot = Grammar.GetRandomMeaningfulWord(obj);
			if (50.in100())
			{
				wordRoot = HistoricStringExpander.ExpandString("<spice.items.blessing.!random>", null, history) + " of " + wordRoot;
				if (50.in100())
				{
					wordRoot = "the " + wordRoot;
				}
			}
			else
			{
				wordRoot = Grammar.MakePossessive(wordRoot) + " " + HistoricStringExpander.ExpandString("<spice.items.blessing.!random>", null, history);
			}
			if (wordRoot == null)
			{
				wordRoot = obj;
			}
		}
		while (history.GetEntitiesWherePropertyEquals("name", wordRoot).Count > 0);
		return Grammar.InitCap(wordRoot);
	}

	public static string NameItemAdjRoot(string obj, History history, HistoricEntity entity)
	{
		string wordRoot;
		do
		{
			if (50.in100())
			{
				wordRoot = Grammar.GetWordRoot(obj);
				wordRoot += HistoricStringExpander.ExpandString("<spice.items.suffixes.!random>", null, history);
				continue;
			}
			wordRoot = Grammar.GetRandomMeaningfulWord(obj);
			if (50.in100())
			{
				wordRoot = wordRoot + " " + HistoricStringExpander.ExpandString("<spice.items.blessing.!random>", null, history);
				if (50.in100())
				{
					wordRoot = "the " + wordRoot;
				}
			}
			if (wordRoot == null)
			{
				wordRoot = obj;
			}
		}
		while (history.GetEntitiesWherePropertyEquals("name", wordRoot).Count > 0);
		return Grammar.InitCap(wordRoot);
	}

	public static string GenerateLocationName(string seed, History history)
	{
		string text = seed;
		int num = 50;
		while (num > 0)
		{
			text = Stat.Random(0, 3) switch
			{
				0 => Grammar.InitCap(Grammar.GetWordRoot(seed)) + "abad", 
				1 => Grammar.InitCap(Grammar.GetWordRoot(seed)) + "grad", 
				2 => Grammar.InitCap(Grammar.GetWordRoot(seed)) + "platz", 
				_ => Grammar.GetRandomMeaningfulWord(seed) + " City", 
			};
			num--;
			if (history.GetEntitiesWherePropertyEquals("name", text).Count <= 0)
			{
				break;
			}
		}
		return text;
	}

	public static string ListAllCognomen(HistoricEntitySnapshot snapSultan)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = new List<string>(snapSultan.GetList("cognomen"));
		if (list.Count == 0)
		{
			list.Add("the Untitled");
		}
		for (int i = 0; i < list.Count; i++)
		{
			stringBuilder.Append(", " + list[i]);
		}
		stringBuilder.Append(",");
		return stringBuilder.ToString();
	}

	public static HistoricEntityList GetSultans(History History)
	{
		return History.GetEntitiesWherePropertyEquals("type", "sultan");
	}

	public static HistoricEntity GetPeriodSultan(History History, string Period)
	{
		return History.GetEntitiesWherePropertyEquals("type", "sultan").GetEntitiesWherePropertyEquals("period", Period).First();
	}

	public static HistoricEntity GetPeriodSultan(History History, int Period)
	{
		return GetPeriodSultan(History, Period.ToString());
	}

	public static HistoricEntity GetRandomSultan(History History)
	{
		return GetSultans(History).GetRandomElement();
	}

	public static string GetRandomCognomen(HistoricEntitySnapshot snapSultan)
	{
		return snapSultan.GetList("cognomen").GetRandomElement() ?? "the Untitled";
	}

	public static string GetRandomCognomen(History History)
	{
		return GetRandomCognomen(GetRandomSultan(History).GetCurrentSnapshot());
	}

	public static string GetSultanateEra(HistoricEntitySnapshot snapSultan)
	{
		switch (int.Parse(snapSultan.GetProperty("period")))
		{
		case 1:
		case 2:
			return "EarlySultanate";
		case 4:
		case 5:
			return "LateSultanate";
		case 3:
			if (If.CoinFlip())
			{
				return "EarlySultanate";
			}
			return "LateSultanate";
		default:
			return "LateSultanate";
		}
	}

	public static string GetMaskName(HistoricEntitySnapshot snapSultan)
	{
		return int.Parse(snapSultan.GetProperty("period")) switch
		{
			1 => "Kesil Face", 
			2 => "Shemesh Face", 
			3 => "Earth Face", 
			4 => "Levant Face", 
			5 => "Olive Face", 
			_ => "Nil Face", 
		};
	}

	public static void RenameLocation(string oldLocation, string newLocation, History history)
	{
		history.GetEntitiesWherePropertyEquals("name", oldLocation).ForEach((HistoricEntity entity) =>
		{
			entity.ApplyEvent(new SetEntityProperty("name", newLocation));
		});
		history.GetEntitiesWithListPropertyThatContains("locations", oldLocation).ForEach((HistoricEntity entity) =>
		{
			entity.ApplyEvent(new RemoveEntityListItem("locations", oldLocation));
			entity.ApplyEvent(new AddEntityListItem("locations", newLocation));
		});
	}

	public static string GenerateSultanateYearName()
	{
		return HistoricStringExpander.ExpandString("Year of the <spice.adjectives.!random.capitalize> <spice.nouns.!random.capitalize>");
	}

	public static Dictionary<string, string> BuildContextFromObjectTextFragments(string blueprintName)
	{
		if (!GameObjectFactory.Factory.Blueprints.ContainsKey(blueprintName))
		{
			return null;
		}
		if (GameObjectFactory.Factory.Blueprints[blueprintName].xTags == null)
		{
			return null;
		}
		if (!GameObjectFactory.Factory.Blueprints[blueprintName].xTags.ContainsKey("TextFragments"))
		{
			return null;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string key in GameObjectFactory.Factory.Blueprints[blueprintName].xTags["TextFragments"].Keys)
		{
			dictionary.Add("*" + key + "*", GameObjectFactory.Factory.Blueprints[blueprintName].xTags["TextFragments"][key].Split(',').GetRandomElement());
		}
		return dictionary;
	}

	public static Dictionary<string, string> BuildContextFromMemberTextFragments(HistoricEntity Village, bool Throw = false)
	{
		string entityProperty = Village.GetEntityProperty("baseFactionMember", -1L);
		if (entityProperty.IsNullOrEmpty())
		{
			if (Throw)
			{
				throw new Exception("Village '" + Village.Name + "' is missing baseFactionMember property.");
			}
			return null;
		}
		Dictionary<string, string> dictionary = BuildContextFromObjectTextFragments(entityProperty);
		if (Throw && dictionary.IsNullOrEmpty())
		{
			throw new Exception("Village '" + Village.Name + "' member '" + entityProperty + "' is missing text fragments.");
		}
		return dictionary;
	}

	public static string GetRandomProperty(HistoricEntitySnapshot entitySnapshot, string defaultValue, params string[] args)
	{
		if (args.Length == 0)
		{
			return defaultValue;
		}
		List<string> list = new List<string>();
		for (int i = 0; i < args.Length; i++)
		{
			if (entitySnapshot.hasProperty(args[i]))
			{
				list.Add(entitySnapshot.GetProperty(args[i]));
			}
			else
			{
				if (!entitySnapshot.listProperties.ContainsKey(args[i]))
				{
					continue;
				}
				foreach (string item in entitySnapshot.GetList(args[i]))
				{
					list.Add(item);
				}
			}
		}
		if (list.Count == 0)
		{
			return defaultValue;
		}
		return list.GetRandomElement();
	}

	public static List<int> GetSpreadOfSultanYears(int yearsInSultanate, int numSultans)
	{
		List<int> list = new List<int>();
		yearsInSultanate = (int)((float)yearsInSultanate * Stat.Random(0.8f, 1.2f));
		int num = yearsInSultanate - numSultans * 200 + numSultans - 1;
		List<int> list2 = new List<int>();
		while (list2.Count < numSultans - 1)
		{
			int item = Stat.Random(1, num);
			if (!list2.Contains(item))
			{
				list2.Add(item);
			}
		}
		list2.Sort();
		for (int i = 0; i < numSultans; i++)
		{
			if (i == 0)
			{
				list.Add(list2[i] - 1);
			}
			else if (i == numSultans - 1)
			{
				list.Add(num - list2[i - 1]);
			}
			else
			{
				list.Add(list2[i] - list2[i - 1] - 1);
			}
		}
		list.Sort();
		return list;
	}

	public static string ConvertGospelToSultanateCalendarEra(string gospel, long endOfSultanateDate)
	{
		if (!gospel.Contains("%"))
		{
			return gospel;
		}
		Match match = Regex.Match(gospel, "\\%(.*?)\\%");
		if (match.Success)
		{
			return Regex.Replace(gospel, "\\%.*?\\%", ConvertYearToSultanateCalendarEra(long.Parse(match.Groups[1].Value), endOfSultanateDate));
		}
		return gospel;
	}

	public static string ConvertYearToSultanateCalendarEra(long year, long endOfSultanateDate)
	{
		if (endOfSultanateDate > year)
		{
			return endOfSultanateDate - year + " BR";
		}
		return year - endOfSultanateDate + 1 + " AR";
	}

	public static int GetReshephGospelXP(int numGospels)
	{
		return numGospels switch
		{
			1 => 250, 
			2 => 500, 
			3 => 1000, 
			4 => 2000, 
			5 => 3000, 
			6 => 4000, 
			7 => 5000, 
			8 => 6000, 
			9 => 7500, 
			10 => 10000, 
			11 => 12500, 
			12 => 15000, 
			13 => 20000, 
			14 => 25000, 
			15 => 30000, 
			_ => 40000, 
		};
	}

	public static void ExtractArticle(ref string Name, out string Article)
	{
		Article = null;
		if (Name.StartsWith("the ") || Name.StartsWith("The "))
		{
			Name = Name.Substring(4);
			Article = "the";
		}
	}
}
