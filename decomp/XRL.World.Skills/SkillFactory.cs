using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;

namespace XRL.World.Skills;

[Serializable]
[HasModSensitiveStaticCache]
public class SkillFactory : IPart
{
	public Dictionary<string, SkillEntry> SkillList = new Dictionary<string, SkillEntry>();

	public Dictionary<string, SkillEntry> SkillByClass = new Dictionary<string, SkillEntry>();

	public Dictionary<string, PowerEntry> PowersByClass = new Dictionary<string, PowerEntry>();

	public Dictionary<string, List<IBaseSkillEntry>> EntriesByClass = new Dictionary<string, List<IBaseSkillEntry>>();

	[NonSerialized]
	[ModSensitiveStaticCache(false)]
	private static List<SkillEntry> Skills;

	[NonSerialized]
	[ModSensitiveStaticCache(false)]
	private static List<PowerEntry> Powers;

	[NonSerialized]
	private static readonly List<SkillEntry> ReadOnlySkills = new List<SkillEntry>();

	[NonSerialized]
	private static readonly List<PowerEntry> ReadOnlyPowers = new List<PowerEntry>();

	[ModSensitiveStaticCache(false)]
	private static SkillFactory _Factory;

	protected Dictionary<string, Action<XmlDataHelper>> rootNode;

	protected Dictionary<string, Action<XmlDataHelper>> skillsNodeChildren;

	protected Dictionary<string, Action<XmlDataHelper>> skillNodeChildren;

	protected SkillEntry NewSkill;

	public static SkillFactory Factory
	{
		get
		{
			if (_Factory == null)
			{
				_Factory = new SkillFactory();
				Loading.LoadTask("Loading Skills.xml", _Factory.LoadSkills);
			}
			return _Factory;
		}
	}

	public SkillEntry GetSkill(string Name)
	{
		if (Name == null)
		{
			throw new Exception("cannot retrieve skill with null name");
		}
		if (SkillList.TryGetValue(Name, out var value))
		{
			return value;
		}
		throw new Exception("no such skill: " + Name);
	}

	public SkillEntry GetSkillIfExists(string Name)
	{
		if (Name != null && SkillList.TryGetValue(Name, out var value))
		{
			return value;
		}
		return null;
	}

	public IBaseSkillEntry GetFirstEntry(string Class, IBaseSkillEntry Default = null)
	{
		if (!EntriesByClass.TryGetValue(Class, out var value))
		{
			return Default;
		}
		return value.First();
	}

	public bool TryGetFirstEntry(string Class, out IBaseSkillEntry Entry)
	{
		if (!EntriesByClass.TryGetValue(Class, out var value))
		{
			Entry = null;
			return false;
		}
		Entry = value.First();
		return true;
	}

	public static List<PowerEntry> GetUnknownPowersFor(GameObject Object, bool ReadOnly = true)
	{
		List<PowerEntry> list = (ReadOnly ? ReadOnlyPowers : new List<PowerEntry>());
		list.Clear();
		foreach (PowerEntry power in GetPowers())
		{
			if (!Object.HasPart(power.Class))
			{
				list.Add(power);
			}
		}
		return list;
	}

	public static List<SkillEntry> GetSkillPool(GameObject Object = null, Predicate<SkillEntry> Filter = null, int MinCost = 0, int MaxCost = int.MaxValue, bool CheckRequirements = false, bool IncludeInitiatory = false, bool ReadOnly = true)
	{
		List<SkillEntry> list = (ReadOnly ? ReadOnlySkills : new List<SkillEntry>());
		list.Clear();
		foreach (SkillEntry skill in GetSkills())
		{
			if ((Object == null || (!Object.HasPart(skill.Class) && (!CheckRequirements || skill.MeetsRequirements(Object)))) && !skill.ExcludeFromPool && skill.Cost <= MaxCost && skill.Cost >= MinCost && (!skill.Initiatory || IncludeInitiatory) && (Filter == null || Filter(skill)))
			{
				list.Add(skill);
			}
		}
		return list;
	}

	public static List<PowerEntry> GetPowerPool(GameObject Object = null, Predicate<PowerEntry> Filter = null, int MinCost = 0, int MaxCost = int.MaxValue, bool CheckRequirements = false, bool IncludeInitiatory = false, bool ReadOnly = true)
	{
		List<PowerEntry> list = (ReadOnly ? ReadOnlyPowers : new List<PowerEntry>());
		list.Clear();
		foreach (PowerEntry power in GetPowers())
		{
			if ((Object == null || (!Object.HasPart(power.Class) && (!CheckRequirements || power.MeetsRequirements(Object)))) && !power.ExcludeFromPool && power.Cost <= MaxCost && power.Cost >= MinCost && (!power.IsSkillInitiatory || (IncludeInitiatory && Object != null && power.MeetsRequirements(Object))) && (Filter == null || Filter(power)))
			{
				list.Add(power);
			}
		}
		return list;
	}

	protected SkillFactory()
	{
		rootNode = new Dictionary<string, Action<XmlDataHelper>> { { "skills", HandleSkillsNode } };
		skillsNodeChildren = new Dictionary<string, Action<XmlDataHelper>> { { "skill", HandleSkillNode } };
		skillNodeChildren = new Dictionary<string, Action<XmlDataHelper>> { { "power", HandlePowerNode } };
	}

	private void LoadSkills()
	{
		SkillList = new Dictionary<string, SkillEntry>();
		SkillByClass = new Dictionary<string, SkillEntry>();
		PowersByClass = new Dictionary<string, PowerEntry>();
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Skills"))
		{
			try
			{
				item.HandleNodes(rootNode);
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.modInfo, message);
			}
		}
		foreach (KeyValuePair<string, SkillEntry> skill in SkillList)
		{
			if (!SkillByClass.ContainsKey(skill.Value.Class))
			{
				SkillByClass.Add(skill.Value.Class, skill.Value);
			}
			foreach (KeyValuePair<string, PowerEntry> power in skill.Value.Powers)
			{
				if (power.Value.Class != null && !PowersByClass.ContainsKey(power.Value.Class))
				{
					PowersByClass.Add(power.Value.Class, power.Value);
				}
			}
		}
		EntriesByClass = new Dictionary<string, List<IBaseSkillEntry>>(SkillByClass.Count + PowersByClass.Count);
		foreach (KeyValuePair<string, SkillEntry> item2 in SkillByClass)
		{
			if (!EntriesByClass.TryGetValue(item2.Key, out var value))
			{
				value = (EntriesByClass[item2.Key] = new List<IBaseSkillEntry>(1));
			}
			value.Add(item2.Value);
		}
		foreach (KeyValuePair<string, PowerEntry> item3 in PowersByClass)
		{
			if (!EntriesByClass.TryGetValue(item3.Key, out var value2))
			{
				value2 = (EntriesByClass[item3.Key] = new List<IBaseSkillEntry>(1));
			}
			value2.Add(item3.Value);
		}
	}

	public void HandleSkillsNode(XmlDataHelper xml)
	{
		xml.HandleNodes(skillsNodeChildren);
	}

	public void HandleSkillNode(XmlDataHelper xml)
	{
		string text = xml.ParseAttribute<string>("Name", null, required: true);
		if (text.StartsWith('-'))
		{
			text = text.TrimStart('-');
			MetricsManager.LogPotentialModError(xml.modInfo, DataManager.SanitizePathForDisplay(xml.BaseURI) + ":" + xml.LineNumber + ": Entry removal discontinued, set Hidden attribute instead.");
		}
		if (!SkillList.TryGetValue(text, out NewSkill))
		{
			NewSkill = new SkillEntry
			{
				Name = text,
				Cost = -999
			};
			SkillList.Add(text, NewSkill);
		}
		NewSkill.HandleXMLNode(xml);
		xml.HandleNodes(skillNodeChildren);
		NewSkill = null;
	}

	public void HandlePowerNode(XmlDataHelper xml)
	{
		string text = xml.ParseAttribute<string>("Name", null, required: true);
		if (text.StartsWith('-'))
		{
			text = text.TrimStart('-');
			MetricsManager.LogPotentialModError(xml.modInfo, DataManager.SanitizePathForDisplay(xml.BaseURI) + ":" + xml.LineNumber + ": Entry removal discontinued, set Hidden attribute instead.");
		}
		if (!NewSkill.Powers.TryGetValue(text, out var value))
		{
			value = new PowerEntry
			{
				Name = text,
				Cost = -999,
				ParentSkill = NewSkill,
				Hidden = NewSkill.Hidden,
				ExcludeFromPool = NewSkill.ExcludeFromPool
			};
			if (NewSkill.Initiatory)
			{
				value.Requires = NewSkill.PowerList.LastOrDefault()?.Class;
			}
			NewSkill.Add(value);
		}
		value.HandleXMLNode(xml);
		xml.DoneWithElement();
	}

	public static List<SkillEntry> GetSkills()
	{
		if (Skills == null)
		{
			Skills = new List<SkillEntry>();
			foreach (KeyValuePair<string, SkillEntry> skill in Factory.SkillList)
			{
				if (!string.IsNullOrEmpty(skill.Value.Class))
				{
					Skills.Add(skill.Value);
				}
			}
		}
		return Skills;
	}

	public static List<PowerEntry> GetPowers()
	{
		if (Powers == null)
		{
			Powers = new List<PowerEntry>();
			foreach (SkillEntry value in Factory.SkillList.Values)
			{
				foreach (PowerEntry value2 in value.Powers.Values)
				{
					if (!string.IsNullOrEmpty(value2.Class))
					{
						Powers.Add(value2);
					}
				}
			}
		}
		return Powers;
	}

	public static string GetRandomPowerClass()
	{
		return GetPowers().GetRandomElement()?.Class;
	}

	public static string GetSkillOrPowerName(string ClassName)
	{
		if (Factory.SkillByClass.TryGetValue(ClassName, out var value))
		{
			return value.Name;
		}
		if (Factory.PowersByClass.TryGetValue(ClassName, out var value2))
		{
			return value2.Name;
		}
		return ClassName;
	}
}
