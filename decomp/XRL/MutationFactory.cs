using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace XRL;

[HasModSensitiveStaticCache]
public static class MutationFactory
{
	[ModSensitiveStaticCache(false)]
	private static List<MutationCategory> _Categories = null;

	private static Dictionary<string, MutationCategory> _CategoriesByName = null;

	private static Dictionary<string, MutationEntry> _MutationsByName = null;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, List<MutationEntry>> _MutationsByClass = null;

	[ModSensitiveStaticCache(false)]
	public static List<string> StatsUsedByMutations = new List<string>(1);

	[ModSensitiveStaticCache(false)]
	public static MutationCategory Morphotypes;

	public static Dictionary<string, Action<XmlDataHelper>> XMLNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "mutations", HandleMutationsNode },
		{ "category", HandleCategoryNode }
	};

	private static bool FilePoolExclude;

	private static bool FileHidden;

	public static MutationCategory _currentParsingCategory;

	private static bool CategoryPoolExclude;

	private static bool CategoryHidden;

	private static bool CategoryDefect;

	public static Dictionary<string, Action<XmlDataHelper>> MutationSubnodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "leveltext", HandleLevelTextNode },
		{ "description", HandleDescriptionNode }
	};

	private static MutationEntry currentMutation;

	[Obsolete("Use GetCategories() instead of Categories")]
	public static List<MutationCategory> Categories
	{
		get
		{
			CheckInit();
			return _Categories;
		}
	}

	[Obsolete("Use GetCategories() instead of CategoriesByName")]
	public static Dictionary<string, MutationCategory> CategoriesByName
	{
		get
		{
			CheckInit();
			return _CategoriesByName;
		}
	}

	[Obsolete("Use HasMutation(string name) or GetMutationEntryByName(string name)")]
	public static Dictionary<string, MutationEntry> MutationsByName
	{
		get
		{
			CheckInit();
			return _MutationsByName;
		}
	}

	public static void CheckInit()
	{
		if (_Categories == null)
		{
			try
			{
				Loading.LoadTask("Loading Mutations.xml", Init);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Error loading mutations.", x);
			}
		}
	}

	public static IEnumerable<MutationEntry> AllMutationEntries()
	{
		return _MutationsByName.Values;
	}

	public static bool HasMutation(string name)
	{
		CheckInit();
		return _MutationsByName.ContainsKey(name);
	}

	public static MutationEntry GetMutationEntryByName(string name)
	{
		CheckInit();
		_MutationsByName.TryGetValue(name, out var value);
		return value;
	}

	public static List<MutationCategory> GetCategories()
	{
		CheckInit();
		return _Categories;
	}

	public static List<MutationEntry> GetMutationEntries(BaseMutation mutation)
	{
		return GetMutationEntries(mutation.GetType().Name);
	}

	public static List<MutationEntry> GetMutationEntries(string Class)
	{
		CheckInit();
		if (!_MutationsByClass.TryGetValue(Class, out var value))
		{
			return null;
		}
		return value;
	}

	public static bool TryGetMutationEntry(BaseMutation Mutation, out MutationEntry Entry)
	{
		return TryGetMutationEntry(Mutation.GetMutationEntry()?.Name, Mutation.Name, out Entry);
	}

	public static bool TryGetMutationEntry(string Spec, out MutationEntry Entry)
	{
		return TryGetMutationEntry(Spec, Spec, out Entry);
	}

	public static bool TryGetMutationEntry(string Name, string Class, out MutationEntry Entry)
	{
		CheckInit();
		if (Name != null && _MutationsByName.TryGetValue(Name, out Entry))
		{
			return true;
		}
		if (Class != null && _MutationsByClass.TryGetValue(Class, out var value))
		{
			Entry = value[0];
			return true;
		}
		Entry = null;
		return false;
	}

	public static MutationEntry GetMorphotype(GameObject Object)
	{
		TryGetMorphotype(Object, out var Morphotype);
		return Morphotype;
	}

	public static bool TryGetMorphotype(GameObject Object, out MutationEntry Morphotype)
	{
		CheckInit();
		if (Morphotypes != null)
		{
			string tagOrStringProperty = Object.GetTagOrStringProperty("MutationLevel");
			if (!tagOrStringProperty.IsNullOrEmpty() && _MutationsByName.TryGetValue(tagOrStringProperty, out Morphotype) && Morphotype.Category == Morphotypes)
			{
				return true;
			}
			foreach (MutationEntry entry in Morphotypes.Entries)
			{
				if (!entry.Class.IsNullOrEmpty() && Object.HasPart(entry.Class))
				{
					Morphotype = entry;
					return true;
				}
			}
		}
		Morphotype = null;
		return false;
	}

	private static void AddFromCategoryRespectingPrerelease(List<MutationEntry> list, MutationCategory category, bool IgnoreExclude = false)
	{
		CheckInit();
		bool enablePrereleaseContent = Options.EnablePrereleaseContent;
		foreach (MutationEntry entry in category.Entries)
		{
			if ((!entry.Prerelease || enablePrereleaseContent) && (!entry.ExcludeFromPool || IgnoreExclude))
			{
				list.Add(entry);
			}
		}
	}

	public static List<MutationEntry> GetMutationsOfCategory(string Categories, bool IgnoreExclude = false)
	{
		CheckInit();
		List<MutationEntry> list = new List<MutationEntry>(128);
		if (Categories.Contains(","))
		{
			string[] array = Categories.Split(',');
			foreach (string key in array)
			{
				AddFromCategoryRespectingPrerelease(list, _CategoriesByName[key], IgnoreExclude);
			}
		}
		else
		{
			AddFromCategoryRespectingPrerelease(list, _CategoriesByName[Categories], IgnoreExclude);
		}
		return list;
	}

	public static BaseMutation GetRandomMutation(string Categories)
	{
		CheckInit();
		return GetMutationsOfCategory(Categories).GetRandomElement()?.CreateInstance();
	}

	private static void Init()
	{
		_Categories = new List<MutationCategory>(5);
		_CategoriesByName = new Dictionary<string, MutationCategory>(5);
		_MutationsByName = new Dictionary<string, MutationEntry>(128);
		_MutationsByClass = new Dictionary<string, List<MutationEntry>>(128);
		StatsUsedByMutations = new List<string>(1);
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Mutations"))
		{
			item.HandleNodes(XMLNodes);
		}
		for (int i = 0; i < _Categories.Count; i++)
		{
			MutationCategory mutationCategory = _Categories[i];
			if (!string.IsNullOrEmpty(mutationCategory.Stat) && !StatsUsedByMutations.Contains(mutationCategory.Stat))
			{
				StatsUsedByMutations.Add(mutationCategory.Stat);
			}
			mutationCategory.Entries.Sort((MutationEntry a, MutationEntry b) => a.GetDisplayName().CompareTo(b.GetDisplayName()));
			for (int num = 0; num < mutationCategory.Entries.Count; num++)
			{
				MutationEntry mutationEntry = mutationCategory.Entries[num];
				_MutationsByName[mutationEntry.Name] = mutationEntry;
				if (!string.IsNullOrEmpty(mutationEntry.Class))
				{
					if (!_MutationsByClass.ContainsKey(mutationEntry.Class))
					{
						_MutationsByClass.Add(mutationEntry.Class, new List<MutationEntry>(1));
					}
					_MutationsByClass[mutationEntry.Class].Add(mutationEntry);
				}
				if (!string.IsNullOrEmpty(mutationEntry.Stat) && !StatsUsedByMutations.Contains(mutationEntry.Stat))
				{
					StatsUsedByMutations.Add(mutationEntry.Stat);
				}
			}
		}
		_CategoriesByName.TryGetValue("Morphotypes", out Morphotypes);
	}

	public static MutationEntry CreateMutationEntryForMutation(BaseMutation Mutation)
	{
		MutationEntry mutationEntry = new MutationEntry
		{
			Name = (string.IsNullOrEmpty(Mutation._DisplayName) ? Mutation.GetType().Name : Mutation._DisplayName),
			Class = Mutation.GetType().Name,
			Hidden = true,
			ExcludeFromPool = true
		};
		string key = Mutation._Type ?? "Physical";
		if (_CategoriesByName.TryGetValue(key, out var value))
		{
			mutationEntry.Category = value;
			value.Add(mutationEntry);
		}
		_MutationsByClass.TryAdd(mutationEntry.Class, new List<MutationEntry> { mutationEntry });
		_MutationsByName.TryAdd(mutationEntry.Name, mutationEntry);
		return mutationEntry;
	}

	public static void HandleMutationsNode(XmlDataHelper xml)
	{
		FilePoolExclude = !xml.ParseAttribute("IncludeInMutatePool", defaultValue: true);
		FilePoolExclude = xml.ParseAttribute("ExcludeFromPool", FilePoolExclude);
		FileHidden = xml.ParseAttribute("Hidden", defaultValue: false);
		xml.HandleNodes(XMLNodes);
	}

	public static void HandleCategoryNode(XmlDataHelper xml)
	{
		string text = xml.ParseAttribute<string>("Name", null, required: true);
		if (text.StartsWith('-'))
		{
			text = text.TrimStart('-');
			MetricsManager.LogPotentialModError(xml.modInfo, DataManager.SanitizePathForDisplay(xml.BaseURI) + ":" + xml.LineNumber + ": Entry removal discontinued, set Hidden attribute instead.");
		}
		if (!_CategoriesByName.TryGetValue(text, out var value))
		{
			value = new MutationCategory(text);
			_CategoriesByName.Add(text, value);
			_Categories.Add(value);
		}
		_currentParsingCategory = value;
		CategoryPoolExclude = !xml.ParseAttribute("IncludeInMutatePool", !FilePoolExclude);
		CategoryPoolExclude = xml.ParseAttribute("ExcludeFromPool", CategoryPoolExclude);
		CategoryHidden = xml.ParseAttribute("Hidden", FileHidden);
		CategoryDefect = xml.ParseAttribute("Defect", defaultValue: false);
		value.DisplayName = xml.ParseAttribute("DisplayName", value.DisplayName);
		value.Help = xml.ParseAttribute("Help", value.Help);
		value.Stat = xml.ParseAttribute("Stat", value.Stat);
		value.Property = xml.ParseAttribute("Property", value.Property);
		value.ForceProperty = xml.ParseAttribute("ForceProperty", value.ForceProperty);
		value.Foreground = xml.ParseAttribute("Foreground", value.Foreground);
		value.Detail = xml.ParseAttribute("Detail", value.Detail);
		xml.HandleNodes(new Dictionary<string, Action<XmlDataHelper>> { { "mutation", HandleMutationNode } });
		_currentParsingCategory = null;
	}

	public static void HandleMutationNode(XmlDataHelper xml)
	{
		string name = xml.ParseAttribute<string>("Name", null, required: true);
		currentMutation = _currentParsingCategory.Entries.Find((MutationEntry x) => x.Name == name);
		bool flag = currentMutation == null;
		if (currentMutation == null)
		{
			currentMutation = new MutationEntry
			{
				Name = name,
				Defect = CategoryDefect,
				Hidden = CategoryHidden,
				ExcludeFromPool = CategoryPoolExclude,
				Foreground = _currentParsingCategory.Foreground,
				Detail = _currentParsingCategory.Detail
			};
		}
		currentMutation.HandleXMLNode(xml);
		currentMutation.Category = _currentParsingCategory;
		MutationEntry mutationEntry = currentMutation;
		if (mutationEntry.Type == null)
		{
			mutationEntry.Type = _currentParsingCategory?.Name;
		}
		if (flag)
		{
			if (currentMutation.Prerelease && !Options.EnablePrereleaseContent)
			{
				currentMutation.Hidden = true;
				currentMutation.ExcludeFromPool = true;
			}
			_currentParsingCategory.Entries.Add(currentMutation);
		}
		xml.HandleNodes(MutationSubnodes);
		currentMutation = null;
	}

	public static void HandleLevelTextNode(XmlDataHelper xml)
	{
		Templates.LoadTemplateFromExternal("Mutation." + currentMutation.Class + ".LevelText", xml);
	}

	public static void HandleDescriptionNode(XmlDataHelper xml)
	{
		Templates.LoadTemplateFromExternal("Mutation." + currentMutation.Class + ".Description", xml);
	}
}
