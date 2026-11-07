using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace XRL;

[HasModSensitiveStaticCache]
public static class SubtypeFactory
{
	[ModSensitiveStaticCache(false)]
	private static List<SubtypeClass> _Classes;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, SubtypeClass> _ClassesByID;

	[ModSensitiveStaticCache(false)]
	private static List<SubtypeEntry> _Subtypes;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, SubtypeEntry> _SubtypesByName;

	public static List<SubtypeClass> Classes
	{
		get
		{
			if (_Classes == null)
			{
				Init();
			}
			return _Classes;
		}
	}

	public static Dictionary<string, SubtypeClass> ClassesByID
	{
		get
		{
			if (_ClassesByID == null)
			{
				Init();
			}
			return _ClassesByID;
		}
	}

	public static List<SubtypeEntry> Subtypes
	{
		get
		{
			if (_Subtypes == null)
			{
				Init();
			}
			return _Subtypes;
		}
	}

	public static Dictionary<string, SubtypeEntry> SubtypesByName
	{
		get
		{
			if (_SubtypesByName == null)
			{
				Init();
			}
			return _SubtypesByName;
		}
	}

	public static SubtypeEntry GetSubtypeEntry(string Name)
	{
		TryGetSubtypeEntry(Name, out var Entry);
		return Entry;
	}

	public static bool TryGetSubtypeEntry(string Name, out SubtypeEntry Entry)
	{
		if (Name != null && SubtypesByName.TryGetValue(Name, out Entry))
		{
			return true;
		}
		Entry = null;
		return false;
	}

	public static SubtypeClass GetSubtypeClass(string ID)
	{
		TryGetSubtypeClass(ID, out var Class);
		return Class;
	}

	public static SubtypeClass GetSubtypeClass(GenotypeEntry Genotype)
	{
		TryGetSubtypeClass(Genotype, out var Class);
		return Class;
	}

	public static bool TryGetSubtypeClass(string ID, out SubtypeClass Class)
	{
		if (ID != null && ClassesByID.TryGetValue(ID, out Class))
		{
			return true;
		}
		Class = null;
		return false;
	}

	public static bool TryGetSubtypeClass(GenotypeEntry Genotype, out SubtypeClass Class)
	{
		return TryGetSubtypeClass(Genotype?.Subtypes, out Class);
	}

	[ModSensitiveCacheInit]
	public static void Init()
	{
		_Classes = new List<SubtypeClass>();
		_ClassesByID = new Dictionary<string, SubtypeClass>();
		_Subtypes = new List<SubtypeEntry>();
		_SubtypesByName = new Dictionary<string, SubtypeEntry>();
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Subtypes"))
		{
			try
			{
				item.WhitespaceHandling = WhitespaceHandling.None;
				while (item.Read())
				{
					if (item.Name == "subtypes")
					{
						LoadSubtypesNode(item, bMod: true);
					}
					if (item.NodeType == XmlNodeType.EndElement && item.Name == "subtypes")
					{
						break;
					}
				}
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.modInfo, message);
			}
		}
	}

	public static void LoadSubtypesNode(XmlTextReader Reader, bool bMod = false)
	{
		SubtypeClass subtypeClass = null;
		SubtypeCategory subtypeCategory = null;
		while (Reader.Read())
		{
			if (Reader.Name == "class")
			{
				if (Reader.NodeType == XmlNodeType.Element)
				{
					string attribute = Reader.GetAttribute("ID");
					subtypeClass = null;
					for (int i = 0; i < Classes.Count; i++)
					{
						if (Classes[i].ID == attribute)
						{
							subtypeClass = Classes[i];
							subtypeCategory = Classes[i].Categories[0];
							break;
						}
					}
					if (subtypeClass == null)
					{
						subtypeClass = new SubtypeClass();
						subtypeClass.ID = Reader.GetAttribute("ID");
						Classes.Add(subtypeClass);
						ClassesByID.Add(subtypeClass.ID, subtypeClass);
						subtypeClass.Categories.Add(new SubtypeCategory());
						subtypeCategory = subtypeClass.Categories[0];
					}
					subtypeClass.ChargenTitle = Reader.GetAttribute("ChargenTitle") ?? subtypeClass.ChargenTitle;
					subtypeClass.SingluarTitle = Reader.GetAttribute("SingularTitle") ?? subtypeClass.SingluarTitle ?? subtypeClass.ChargenTitle;
					subtypeClass.StatBoxDisplay = Reader.GetAttribute("StatBoxDisplay") ?? subtypeClass.StatBoxDisplay;
				}
				if (Reader.NodeType == XmlNodeType.EndElement)
				{
					subtypeClass = null;
				}
			}
			else if (Reader.Name == "category")
			{
				if (Reader.NodeType == XmlNodeType.Element)
				{
					string attribute2 = Reader.GetAttribute("Name");
					subtypeCategory = null;
					for (int j = 0; j < subtypeClass.Categories.Count; j++)
					{
						if (subtypeClass.Categories[j].Name == attribute2)
						{
							subtypeCategory = subtypeClass.Categories[j];
							break;
						}
					}
					if (subtypeCategory == null)
					{
						subtypeCategory = new SubtypeCategory();
						subtypeCategory.Name = Reader.GetAttribute("Name") ?? "";
						subtypeCategory.DisplayName = Reader.GetAttribute("DisplayName");
						subtypeClass.Categories.Add(subtypeCategory);
					}
				}
				if (Reader.NodeType == XmlNodeType.EndElement)
				{
					subtypeCategory = subtypeClass.Categories[0];
				}
			}
			else if (Reader.Name == "subtype")
			{
				LoadSubtypeNode(subtypeClass, subtypeCategory, Reader, bMod);
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "subtypes")
			{
				break;
			}
		}
	}

	public static void LoadSubtypeNode(SubtypeClass currentClass, SubtypeCategory currentCategory, XmlTextReader Reader, bool bMod)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute[0] == '-')
		{
			attribute = attribute.Substring(1);
			if (!SubtypesByName.ContainsKey(attribute))
			{
				return;
			}
			SubtypeEntry item = SubtypesByName[attribute];
			SubtypesByName.Remove(attribute);
			Subtypes.Remove(item);
			for (int i = 0; i < Classes.Count; i++)
			{
				for (int j = 0; j < Classes[i].Categories.Count; j++)
				{
					if (Classes[i].Categories[j].Subtypes.Contains(item))
					{
						Classes[i].Categories[j].Subtypes.Remove(item);
					}
				}
			}
			return;
		}
		SubtypeEntry subtypeEntry = new SubtypeEntry();
		subtypeEntry.Name = attribute;
		subtypeEntry.DisplayName = Reader.GetAttribute("DisplayName");
		if (string.IsNullOrEmpty(subtypeEntry.DisplayName))
		{
			subtypeEntry.DisplayName = subtypeEntry.Name;
		}
		subtypeEntry.Class = Reader.GetAttribute("Class");
		subtypeEntry.Gear = Reader.GetAttribute("Gear");
		subtypeEntry.Tile = Reader.GetAttribute("Tile");
		subtypeEntry.BodyObject = Reader.GetAttribute("BodyObject");
		subtypeEntry.DetailColor = Reader.GetAttribute("DetailColor");
		subtypeEntry.StartingLocation = Reader.GetAttribute("StartingLocation");
		subtypeEntry.Constructor = Reader.GetAttribute("Constructor");
		subtypeEntry.BaseMPGain = Reader.GetAttribute("BaseMPGain");
		subtypeEntry.BaseSPGain = Reader.GetAttribute("BaseSPGain");
		subtypeEntry.BaseHPGain = Reader.GetAttribute("BaseHPGain");
		subtypeEntry.Species = Reader.GetAttribute("Species");
		subtypeEntry.CyberneticsLicensePoints = GetAttributeInt(Reader.GetAttribute("CyberneticsLicensePoints"));
		subtypeEntry.Constructor = Reader.GetAttribute("Constructor");
		string attribute2 = Reader.GetAttribute("Skills");
		if (!string.IsNullOrEmpty(attribute2))
		{
			Debug.LogWarning(Reader.BaseURI + " uses Skills attribute at line " + Reader.LineNumber + ", should be ported to skills element");
			string[] array = attribute2.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				string item2 = CompatManager.ProcessSkill(array[k]);
				if (!subtypeEntry.Skills.Contains(item2))
				{
					subtypeEntry.Skills.Add(item2);
				}
			}
		}
		string attribute3 = Reader.GetAttribute("Reputation");
		if (!string.IsNullOrEmpty(attribute3))
		{
			Debug.LogWarning(Reader.BaseURI + " uses Reputation attribute at line " + Reader.LineNumber + ", should be ported to reputations element");
			string[] array = attribute3.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				string[] array2 = array[k].Split(':');
				if (array2.Length == 2)
				{
					try
					{
						int value = Convert.ToInt32(array2[1]);
						SubtypeReputation subtypeReputation = new SubtypeReputation();
						subtypeReputation.With = array2[0];
						subtypeReputation.Value = value;
						subtypeEntry.Reputations.Add(subtypeReputation);
					}
					catch
					{
					}
				}
			}
		}
		string attribute4 = Reader.GetAttribute("SaveModifierVs");
		if (!string.IsNullOrEmpty(attribute4))
		{
			MetricsManager.LogError(Reader.BaseURI + " uses deprecated SaveModifierVs attribute at line " + Reader.LineNumber + ", should be ported to savemodifiers element");
			try
			{
				int attributeInt = GetAttributeInt(Reader.GetAttribute("SaveModifierAmount"));
				SubtypeSaveModifier subtypeSaveModifier = new SubtypeSaveModifier();
				subtypeSaveModifier.Vs = attribute4;
				subtypeSaveModifier.Amount = attributeInt;
				subtypeEntry.SaveModifiers.Add(subtypeSaveModifier);
			}
			catch
			{
			}
		}
		while (Reader.Read())
		{
			if (Reader.Name == "stat")
			{
				LoadStatNode(subtypeEntry, Reader, bMod);
			}
			else if (Reader.Name == "skills")
			{
				LoadSkillsNode(subtypeEntry, Reader, bMod);
			}
			else if (Reader.Name == "reputations")
			{
				LoadReputationsNode(subtypeEntry, Reader, bMod);
			}
			else if (Reader.Name == "savemodifiers")
			{
				LoadSaveModifiersNode(subtypeEntry, Reader, bMod);
			}
			else if (Reader.Name == "extrainfo")
			{
				LoadExtraInfoNode(subtypeEntry, Reader, bMod);
			}
			else if (Reader.Name == "removeextrainfo")
			{
				LoadRemoveExtraInfoNode(subtypeEntry, Reader, bMod);
			}
			else if (Reader.Name == "chargeninfo")
			{
				Debug.LogWarning(Reader.BaseURI + " uses chargeninfo element at line " + Reader.LineNumber + ", should be ported to extrainfo elements");
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "subtype")
			{
				break;
			}
		}
		if (SubtypesByName.ContainsKey(subtypeEntry.Name))
		{
			SubtypesByName[subtypeEntry.Name].MergeWith(subtypeEntry);
			if (currentCategory != null && currentCategory != SubtypesByName[subtypeEntry.Name].Category)
			{
				SubtypesByName[subtypeEntry.Name].Category.Subtypes.Remove(SubtypesByName[subtypeEntry.Name]);
				currentCategory.Subtypes.Add(SubtypesByName[subtypeEntry.Name]);
				SubtypesByName[subtypeEntry.Name].Category = currentCategory;
			}
		}
		else
		{
			SubtypesByName[subtypeEntry.Name] = subtypeEntry;
			Subtypes.Add(subtypeEntry);
			subtypeEntry.Category = currentCategory;
			subtypeEntry.SubtypeClass = currentClass;
			currentCategory.Subtypes.Add(subtypeEntry);
		}
	}

	public static int GetAttributeInt(string Attribute, int Default = -999)
	{
		if (string.IsNullOrEmpty(Attribute))
		{
			return Default;
		}
		return int.Parse(Attribute);
	}

	public static void LoadStatNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		SubtypeStat subtypeStat = new SubtypeStat();
		subtypeStat.Name = Reader.GetAttribute("Name");
		subtypeStat.Minimum = GetAttributeInt(Reader.GetAttribute("Minimum"));
		subtypeStat.Maximum = GetAttributeInt(Reader.GetAttribute("Maximum"));
		subtypeStat.Bonus = GetAttributeInt(Reader.GetAttribute("Bonus"));
		NewSubtype.Stats[subtypeStat.Name] = subtypeStat;
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || (!(Reader.Name == "") && !(Reader.Name == "stat"))))
			{
			}
		}
	}

	public static void LoadSkillsNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "skill")
			{
				LoadSkillNode(NewSubtype, Reader, bMod);
			}
			else if (Reader.Name == "removeskill")
			{
				LoadRemoveSkillNode(NewSubtype, Reader, bMod);
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "skills")
			{
				break;
			}
		}
	}

	public static void LoadSkillNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		string Skill = Reader.GetAttribute("Name");
		CompatManager.ProcessSkill(ref Skill);
		if (!NewSubtype.Skills.Contains(Skill))
		{
			NewSubtype.Skills.Add(Skill);
		}
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || (!(Reader.Name == "") && !(Reader.Name == "skill"))))
			{
			}
		}
	}

	public static void LoadRemoveSkillNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		string Skill = Reader.GetAttribute("Name");
		CompatManager.ProcessSkill(ref Skill);
		if (!NewSubtype.RemoveSkills.Contains(Skill))
		{
			NewSubtype.RemoveSkills.Add(Skill);
		}
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || (!(Reader.Name == "") && !(Reader.Name == "removeskill"))))
			{
			}
		}
	}

	public static void LoadExtraInfoNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		Reader.Read();
		string value = Reader.Value;
		if (!NewSubtype.ExtraInfo.Contains(value))
		{
			NewSubtype.ExtraInfo.Add(value);
		}
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || !(Reader.Name == "extrainfo")))
			{
			}
		}
	}

	public static void LoadRemoveExtraInfoNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		Reader.Read();
		string value = Reader.Value;
		if (!NewSubtype.RemoveExtraInfo.Contains(value))
		{
			NewSubtype.RemoveExtraInfo.Add(value);
		}
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || !(Reader.Name == "removeextrainfo")))
			{
			}
		}
	}

	public static void LoadReputationsNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "reputation")
			{
				LoadReputationNode(NewSubtype, Reader, bMod);
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "reputations")
			{
				break;
			}
		}
	}

	public static void LoadReputationNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		SubtypeReputation subtypeReputation = new SubtypeReputation();
		subtypeReputation.With = Reader.GetAttribute("With");
		subtypeReputation.Value = GetAttributeInt(Reader.GetAttribute("Value"));
		NewSubtype.Reputations.Add(subtypeReputation);
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || (!(Reader.Name == "") && !(Reader.Name == "reputation"))))
			{
			}
		}
	}

	public static void LoadSaveModifiersNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "savemodifier")
			{
				LoadSaveModifierNode(NewSubtype, Reader, bMod);
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "savemodifiers")
			{
				break;
			}
		}
	}

	public static void LoadSaveModifierNode(SubtypeEntry NewSubtype, XmlTextReader Reader, bool bMod)
	{
		SubtypeSaveModifier subtypeSaveModifier = new SubtypeSaveModifier();
		subtypeSaveModifier.Vs = Reader.GetAttribute("Vs");
		subtypeSaveModifier.Amount = GetAttributeInt(Reader.GetAttribute("Amount"));
		NewSubtype.SaveModifiers.Add(subtypeSaveModifier);
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || (!(Reader.Name == "") && !(Reader.Name == "savemodifier"))))
			{
			}
		}
	}
}
