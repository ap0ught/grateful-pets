using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.World;

namespace XRL;

public class WishSearcher
{
	public static int WishResultSort(WishResult x, WishResult y)
	{
		if (x == y)
		{
			return 0;
		}
		if (x == null)
		{
			return 1;
		}
		if (y == null)
		{
			return -1;
		}
		int num = x.Distance.CompareTo(y.Distance);
		if (num != 0)
		{
			return num;
		}
		int num2 = x.NegativeMarks.CompareTo(y.NegativeMarks);
		if (num2 != 0)
		{
			return num2;
		}
		return x.AddOrder.CompareTo(y.AddOrder);
	}

	public static WishResult SearchForWish(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		list.Add(SearchForZone(Search));
		list.Add(SearchForBlueprint(Search));
		list.Add(SearchForQuest(Search));
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	public static WishResult SearchForZone(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		string text = Search.ToLower();
		text = text.Replace("zone:", "");
		foreach (string key in WorldFactory.Factory.ZoneDisplayToID.Keys)
		{
			WishResult wishResult = new WishResult();
			wishResult.Distance = Grammar.LevenshteinDistance(text, key);
			wishResult.Result = WorldFactory.Factory.ZoneDisplayToID[key];
			wishResult.Type = WishResultType.Zone;
			wishResult.AddOrder = list.Count;
			list.Add(wishResult);
		}
		list.Sort(WishResultSort);
		if (list.Count == 0)
		{
			return null;
		}
		return list[0];
	}

	public static WishResult SearchForCrayonBlueprint(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		string text = Search.ToLower();
		text = text.Replace("object:", "");
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!blueprint.HasPart("Physics") || blueprint.HasTag("BaseObject") || blueprint.Name.StartsWith("Base", StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			int num = Grammar.LevenshteinDistance(text, blueprint.Name);
			int num2 = 99999;
			string text2 = null;
			string partParameter = blueprint.GetPartParameter<string>("Render", "DisplayName");
			if (!string.IsNullOrEmpty(partParameter))
			{
				text2 = partParameter.ToLower();
				num2 = Grammar.LevenshteinDistance(text, text2);
			}
			if (text2 != null && !text2.StartsWith("["))
			{
				WishResult wishResult = new WishResult();
				if (num < num2)
				{
					wishResult.Distance = num;
				}
				else
				{
					wishResult.Distance = num2;
				}
				wishResult.Result = blueprint.Name;
				wishResult.Type = WishResultType.Blueprint;
				wishResult.AddOrder = list.Count;
				list.Add(wishResult);
			}
		}
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	public static WishResult SearchForBlueprint(string Search)
	{
		if (Search.Contains("object:"))
		{
			Search = Search.Replace("object:", "");
		}
		else if (Search.Contains("Object:"))
		{
			Search = Search.Replace("Object:", "");
		}
		else if (Search.Contains("OBJECT:"))
		{
			Search = Search.Replace("OBJECT:", "");
		}
		WishResult wishResult = null;
		if (GameObjectFactory.Factory.Blueprints.ContainsKey(Search))
		{
			wishResult = new WishResult();
			wishResult.Distance = 0;
			wishResult.Result = Search;
			wishResult.Type = WishResultType.Blueprint;
			return wishResult;
		}
		int num = int.MaxValue;
		string s = Search.ToLower();
		List<WishResult> list = new List<WishResult>();
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!blueprint.HasPart("Physics"))
			{
				continue;
			}
			int num2 = Math.Min(Grammar.LevenshteinDistance(s, blueprint.CachedNameLC), Grammar.LevenshteinDistance(s, blueprint.CachedDisplayNameStrippedLC));
			if (num2 > 10)
			{
				continue;
			}
			if (num2 > 0)
			{
				num2 = Math.Min(num2, Grammar.LevenshteinDistance(Search, blueprint.Name));
			}
			if (num2 > 0)
			{
				num2 = Math.Min(num2, Grammar.LevenshteinDistance(Search, blueprint.CachedDisplayNameStripped, caseInsensitive: false));
			}
			if (num2 <= num)
			{
				wishResult = new WishResult();
				wishResult.Distance = num2;
				wishResult.Result = blueprint.Name;
				wishResult.Type = WishResultType.Blueprint;
				if (!blueprint.GetPartParameter("Physics", "IsReal", Default: false))
				{
					wishResult.NegativeMarks++;
				}
				if (blueprint.Name.StartsWith("Base", StringComparison.InvariantCultureIgnoreCase))
				{
					wishResult.NegativeMarks++;
				}
				if (blueprint.HasTag("BaseObject"))
				{
					wishResult.NegativeMarks++;
				}
				if (!blueprint.HasPart("Render"))
				{
					wishResult.NegativeMarks++;
				}
				if (num2 < num)
				{
					list.Clear();
					num = num2;
				}
				else
				{
					wishResult.AddOrder = list.Count;
				}
				list.Add(wishResult);
			}
		}
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	public static WishResult SearchForQuest(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		string text = Search.ToLower();
		text = text.Replace("quest:", "");
		foreach (string key in QuestLoader.Loader.QuestsByID.Keys)
		{
			WishResult wishResult = new WishResult();
			wishResult.Type = WishResultType.Quest;
			wishResult.Result = key;
			wishResult.Distance = Grammar.LevenshteinDistance(text, key);
			list.Add(wishResult);
		}
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	public static WishResult SearchForEffect(string Search)
	{
		List<WishResult> list = new List<WishResult>();
		list.Sort(WishResultSort);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}
}
