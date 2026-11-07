using System;
using System.Collections.Generic;
using System.Text;
using XRL.CharacterCreation;
using XRL.Language;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;
using XRL.World.Skills;

namespace XRL;

[Serializable]
public class SubtypeEntry
{
	public SubtypeCategory Category;

	public SubtypeClass SubtypeClass;

	public string Name;

	public string DisplayName;

	public int RandomWeight = 1;

	public int CyberneticsLicensePoints;

	public string Gear;

	public string Class;

	public string Tile;

	public string DetailColor;

	public string StartingLocation;

	public string BodyObject;

	public string BaseMPGain;

	public string BaseHPGain;

	public string BaseSPGain;

	public string Species;

	public string Constructor;

	public Dictionary<string, SubtypeStat> Stats = new Dictionary<string, SubtypeStat>();

	public List<string> Skills = new List<string>(8);

	public List<string> RemoveSkills = new List<string>();

	public List<SubtypeReputation> Reputations = new List<SubtypeReputation>();

	public List<SubtypeSaveModifier> SaveModifiers = new List<SubtypeSaveModifier>();

	public List<string> ExtraInfo = new List<string>();

	public List<string> RemoveExtraInfo = new List<string>();

	public bool supportsCybernetics => CyberneticsLicensePoints > 0;

	public SubtypeClass GetClass()
	{
		SubtypeClass value = null;
		SubtypeFactory.ClassesByID.TryGetValue(Class, out value);
		return value;
	}

	public int GetStatBonus(string ID)
	{
		if (Stats.ContainsKey(ID))
		{
			return Stats[ID].Bonus;
		}
		return 0;
	}

	public void MergeWith(SubtypeEntry newEntry)
	{
		if (!string.IsNullOrEmpty(newEntry.Name))
		{
			Name = newEntry.Name;
		}
		if (!string.IsNullOrEmpty(newEntry.DisplayName))
		{
			DisplayName = newEntry.DisplayName;
		}
		if (!string.IsNullOrEmpty(newEntry.Gear))
		{
			Gear = newEntry.Gear;
		}
		if (!string.IsNullOrEmpty(newEntry.Class))
		{
			Class = newEntry.Class;
		}
		if (!string.IsNullOrEmpty(newEntry.Tile))
		{
			Tile = newEntry.Tile;
		}
		if (!string.IsNullOrEmpty(newEntry.BodyObject))
		{
			BodyObject = newEntry.BodyObject;
		}
		if (!string.IsNullOrEmpty(newEntry.DetailColor))
		{
			DetailColor = newEntry.DetailColor;
		}
		if (!string.IsNullOrEmpty(newEntry.StartingLocation))
		{
			StartingLocation = newEntry.StartingLocation;
		}
		if (!string.IsNullOrEmpty(newEntry.Constructor))
		{
			Constructor = newEntry.Constructor;
		}
		if (!string.IsNullOrEmpty(newEntry.BaseMPGain))
		{
			BaseMPGain = newEntry.BaseMPGain;
		}
		if (!string.IsNullOrEmpty(newEntry.BaseHPGain))
		{
			BaseHPGain = newEntry.BaseHPGain;
		}
		if (!string.IsNullOrEmpty(newEntry.BaseSPGain))
		{
			BaseSPGain = newEntry.BaseSPGain;
		}
		if (!string.IsNullOrEmpty(newEntry.Species))
		{
			Species = newEntry.Species;
		}
		if (newEntry.CyberneticsLicensePoints != -999)
		{
			CyberneticsLicensePoints = newEntry.CyberneticsLicensePoints;
		}
		foreach (string skill in newEntry.Skills)
		{
			if (!Skills.Contains(skill))
			{
				Skills.Add(skill);
			}
		}
		foreach (string removeSkill in newEntry.RemoveSkills)
		{
			if (!RemoveSkills.Contains(removeSkill))
			{
				RemoveSkills.Add(removeSkill);
			}
		}
		foreach (SubtypeStat value2 in newEntry.Stats.Values)
		{
			if (Stats.TryGetValue(value2.Name, out var value))
			{
				value.MergeWith(value2);
			}
			else
			{
				Stats[value2.Name] = value2;
			}
		}
		foreach (string item in newEntry.ExtraInfo)
		{
			if (!ExtraInfo.Contains(item))
			{
				ExtraInfo.Add(item);
			}
		}
		foreach (string item2 in newEntry.RemoveExtraInfo)
		{
			if (!RemoveExtraInfo.Contains(item2))
			{
				RemoveExtraInfo.Add(item2);
			}
		}
		foreach (SubtypeReputation reputation in newEntry.Reputations)
		{
			bool flag = false;
			foreach (SubtypeReputation reputation2 in Reputations)
			{
				if (reputation2.With == reputation.With)
				{
					reputation2.Value += reputation.Value;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Reputations.Add(reputation);
			}
		}
		foreach (SubtypeSaveModifier saveModifier in newEntry.SaveModifiers)
		{
			bool flag2 = false;
			foreach (SubtypeSaveModifier saveModifier2 in SaveModifiers)
			{
				if (saveModifier2.Vs == saveModifier.Vs)
				{
					saveModifier2.Amount += saveModifier.Amount;
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				SaveModifiers.Add(saveModifier);
			}
		}
	}

	public BaseMutation CreateInstance()
	{
		if (string.IsNullOrEmpty(Class))
		{
			return null;
		}
		Type type = ModManager.ResolveType("XRL.CharacterGeneration." + Class);
		if (string.IsNullOrEmpty(Constructor))
		{
			return (BaseMutation)Activator.CreateInstance(type);
		}
		object[] args = Constructor.Split(',');
		return (BaseMutation)Activator.CreateInstance(type, args);
	}

	public string GetFlatChargenInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in GetChargenInfo())
		{
			stringBuilder.Append(item);
			stringBuilder.Append('\n');
		}
		return stringBuilder.ToString();
	}

	public List<string> GetChargenInfo(GenotypeEntry genotype = null)
	{
		List<string> list = new List<string>(16);
		if (Stats.Count > 0)
		{
			if (Stats.Count > 1)
			{
				List<string> list2 = new List<string>(Stats.Keys);
				Statistic.SortStatistics(list2);
				foreach (string item in list2)
				{
					ProcessChargenInfoStat(list, item, Stats[item]);
				}
			}
			else
			{
				foreach (KeyValuePair<string, SubtypeStat> stat in Stats)
				{
					ProcessChargenInfoStat(list, stat.Key, stat.Value);
				}
			}
		}
		foreach (SubtypeSaveModifier saveModifier in SaveModifiers)
		{
			if (saveModifier.Amount != 0 && saveModifier.Amount != -999)
			{
				string saveBonusDescription = SavingThrows.GetSaveBonusDescription(saveModifier.Amount, saveModifier.Vs);
				if (saveBonusDescription != null)
				{
					list.Add("{{c|ù}} " + saveBonusDescription);
				}
			}
		}
		List<string> list3 = new List<string>(Skills.Count);
		foreach (string skill in Skills)
		{
			if (!RemoveSkills.Contains(skill) && (genotype == null || !genotype.RemoveSkills.Contains(skill)))
			{
				list3.Add(skill);
			}
		}
		List<string> list4 = new List<string>(list3.Count);
		List<string> list5 = new List<string>(list3.Count);
		List<string> list6 = new List<string>(list3.Count);
		List<string> list7 = new List<string>(list3.Count);
		List<string> searchSkills = new List<string>();
		Dictionary<string, SkillEntry> dictionary = new Dictionary<string, SkillEntry>(list3.Count);
		PowerEntry value2;
		SkillEntry value;
		foreach (string item2 in list3)
		{
			if (SkillFactory.Factory.SkillByClass.TryGetValue(item2, out value))
			{
				dictionary[item2] = value;
				list4.Add(item2);
			}
			else if (SkillFactory.Factory.PowersByClass.TryGetValue(item2, out value2))
			{
				list5.Add(item2);
			}
		}
		foreach (string item3 in list5)
		{
			if (!SkillFactory.Factory.PowersByClass.TryGetValue(item3, out value2))
			{
				continue;
			}
			if (dictionary.TryGetValue(value2.ParentSkill.Class, out value))
			{
				if (value2.Cost != 0)
				{
					list6.Add(item3);
				}
			}
			else
			{
				list7.Add(item3);
			}
		}
		foreach (string item4 in list4)
		{
			value = dictionary[item4];
			list.Add("{{c|ù}} " + value.Name);
			foreach (string item5 in list6)
			{
				if (SkillFactory.Factory.PowersByClass.TryGetValue(item5, out value2) && value2.ParentSkill == value)
				{
					list.Add("  {{C|ù}} " + value2.Name);
				}
			}
		}
		foreach (string item6 in list7)
		{
			if (SkillFactory.Factory.PowersByClass.TryGetValue(item6, out value2))
			{
				string text = DisambiguateSkill(searchSkills, value2);
				list.Add("{{c|ù}} " + text);
			}
		}
		if (!string.IsNullOrEmpty(Class) && Activator.CreateInstance(ModManager.ResolveType("XRL.CharacterCreation." + Class)) is ICustomChargenClass customChargenClass)
		{
			foreach (string item7 in customChargenClass.GetChargenInfo())
			{
				if (!string.IsNullOrEmpty(item7))
				{
					list.Add("{{c|ù}} " + item7);
				}
			}
		}
		foreach (SubtypeReputation reputation in Reputations)
		{
			if (reputation.Value != 0 && reputation.Value != -999)
			{
				Faction ifExists = Factions.GetIfExists(reputation.With);
				if (ifExists != null)
				{
					list.Add("{{c|ù}} " + reputation.Value.Signed() + " reputation with " + ifExists.GetFormattedName());
				}
			}
		}
		foreach (string item8 in ExtraInfo)
		{
			if (!RemoveExtraInfo.Contains(item8))
			{
				list.Add("{{c|ù}} " + item8);
			}
		}
		return list;
	}

	private string DisambiguateSkill(List<string> SearchSkills, PowerEntry powerEntry)
	{
		string text = powerEntry.Name;
		SearchSkills.Clear();
		SearchSkills.Add(text);
		if (text.EndsWith("s"))
		{
			SearchSkills.Add(text.Substring(0, text.Length - 1));
		}
		else
		{
			SearchSkills.Add(Grammar.Pluralize(text));
		}
		foreach (PowerEntry value in SkillFactory.Factory.PowersByClass.Values)
		{
			if (value != powerEntry && SearchSkills.Contains(value.Name))
			{
				text = text + " (" + powerEntry.ParentSkill.Name + ")";
				break;
			}
		}
		return text;
	}

	private void ProcessChargenInfoStat(List<string> info, string StatName, SubtypeStat Stat)
	{
		string statDisplayName = Statistic.GetStatDisplayName(StatName);
		if (Stat.Minimum != -999)
		{
			info.Add("{{c|ù}} minimum " + Stat.Minimum + " " + statDisplayName);
		}
		if (Stat.Maximum != -999)
		{
			info.Add("{{c|ù}} maximum " + Stat.Minimum + " " + statDisplayName);
		}
		if (Stat.Bonus != -999 && Stat.Bonus != 0)
		{
			info.Add("{{c|ù}} " + Stat.Bonus.Signed() + " " + statDisplayName);
		}
	}

	public void AddSkills(GameObject Object, ICollection<string> Exclude = null)
	{
		foreach (string skill in Skills)
		{
			if (!RemoveSkills.Contains(skill) && (Exclude == null || !Exclude.Contains(skill)))
			{
				Object.AddSkill(skill);
			}
		}
	}
}
