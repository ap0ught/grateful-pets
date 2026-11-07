using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XRL;

[HasModSensitiveStaticCache]
public static class GenotypeFactory
{
	[ModSensitiveStaticCache(false)]
	private static List<GenotypeEntry> _Genotypes = null;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, GenotypeEntry> _GenotypesByName = null;

	[ModSensitiveStaticCache(false)]
	private static GenotypeEntry DefaultEntry = null;

	private static Dictionary<string, Action<XmlDataHelper>> Nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "genotypes", HandleNodes },
		{ "genotype", HandleGenotypeNode }
	};

	public static List<GenotypeEntry> Genotypes
	{
		get
		{
			if (_Genotypes == null)
			{
				Init();
			}
			return _Genotypes;
		}
	}

	public static Dictionary<string, GenotypeEntry> GenotypesByName
	{
		get
		{
			if (_GenotypesByName == null)
			{
				Init();
			}
			return _GenotypesByName;
		}
	}

	public static GenotypeEntry GetGenotypeEntry(string Name)
	{
		TryGetGenotypeEntry(Name, out var Entry);
		return Entry;
	}

	public static GenotypeEntry RequireGenotypeEntry(string Name)
	{
		if (!TryGetGenotypeEntry(Name, out var Entry))
		{
			return DefaultEntry;
		}
		return Entry;
	}

	public static bool TryGetGenotypeEntry(string Name, out GenotypeEntry Entry)
	{
		if (Name != null && GenotypesByName.TryGetValue(Name, out Entry))
		{
			return true;
		}
		Entry = null;
		return false;
	}

	private static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(Nodes);
	}

	[ModSensitiveCacheInit]
	public static void Init()
	{
		_Genotypes = new List<GenotypeEntry>();
		_GenotypesByName = new Dictionary<string, GenotypeEntry>();
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Genotypes"))
		{
			HandleNodes(item);
		}
		if (!_GenotypesByName.TryGetValue("Mutated Human", out DefaultEntry))
		{
			DefaultEntry = _Genotypes.FirstOrDefault();
		}
	}

	private static void HandleGenotypeNode(XmlDataHelper Reader)
	{
		GenotypeEntry genotypeEntry = LoadGenotypeNode(Reader, Reader.modInfo != null);
		if (genotypeEntry.Name[0] == '-')
		{
			if (_GenotypesByName.ContainsKey(genotypeEntry.Name.Substring(1)))
			{
				GenotypeEntry item = _GenotypesByName[genotypeEntry.Name.Substring(1)];
				_GenotypesByName.Remove(genotypeEntry.Name.Substring(1));
				_Genotypes.Remove(item);
			}
		}
		else if (_GenotypesByName.ContainsKey(genotypeEntry.Name))
		{
			_GenotypesByName[genotypeEntry.Name].MergeWith(genotypeEntry);
		}
		else
		{
			_GenotypesByName.Add(genotypeEntry.Name, genotypeEntry);
			_Genotypes.Add(genotypeEntry);
		}
	}

	public static GenotypeEntry LoadGenotypeNode(XmlDataHelper Reader, bool bMod)
	{
		GenotypeEntry NewGenotype = new GenotypeEntry();
		NewGenotype.Name = Reader.GetAttribute("Name");
		NewGenotype.DisplayName = Reader.GetAttribute("DisplayName");
		NewGenotype.AllowedMutationCategories = Reader.GetAttribute("AllowedMutationCategories");
		NewGenotype.CharacterBuilderModules = Reader.GetAttribute("CharacterBuilderModules");
		NewGenotype.BodyTypes = Reader.GetAttribute("BodyTypes");
		NewGenotype.RestrictedGender = Reader.GetAttribute("RestrictedGender");
		NewGenotype.Subtypes = Reader.GetAttribute("Subtypes");
		NewGenotype.Class = Reader.GetAttribute("Class");
		NewGenotype.Tile = Reader.GetAttribute("Tile");
		NewGenotype.Gear = Reader.GetAttribute("Gear");
		NewGenotype.BaseHPGain = Reader.GetAttribute("BaseHPGain");
		NewGenotype.BaseMPGain = Reader.GetAttribute("BaseMPGain");
		NewGenotype.BaseSPGain = Reader.GetAttribute("BaseSPGain");
		NewGenotype.DetailColor = Reader.GetAttribute("DetailColor");
		NewGenotype.BodyObject = Reader.GetAttribute("BodyObject");
		NewGenotype.StartingLocation = Reader.GetAttribute("StartingLocation");
		NewGenotype.Species = Reader.GetAttribute("Species");
		NewGenotype.Constructor = Reader.GetAttribute("Constructor");
		NewGenotype.IsMutant = Reader.GetAttributeBool("IsMutant", defaultValue: false);
		NewGenotype.IsTrueKin = Reader.GetAttributeBool("IsTrueKin", defaultValue: false);
		NewGenotype.MutationPoints = Reader.GetAttributeInt("MutationPoints", -999);
		NewGenotype.CyberneticsLicensePoints = Reader.GetAttributeInt("CyberneticsLicensePoints", -999);
		NewGenotype.StatPoints = Reader.GetAttributeInt("StatPoints", -999);
		NewGenotype.RandomWeight = Reader.GetAttributeInt("RandomWeight", -999);
		string attribute = Reader.GetAttribute("Skills");
		if (!string.IsNullOrEmpty(attribute))
		{
			Debug.LogWarning(Reader.BaseURI + " uses Skills attribute at line " + Reader.LineNumber + ", should be ported to skills element");
			string[] array = attribute.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string item = CompatManager.ProcessSkill(array[i]);
				if (!NewGenotype.Skills.Contains(item))
				{
					NewGenotype.Skills.Add(item);
				}
			}
		}
		string attribute2 = Reader.GetAttribute("Reputation");
		if (!string.IsNullOrEmpty(attribute2))
		{
			Debug.LogWarning(Reader.BaseURI + " uses Reputation attribute at line " + Reader.LineNumber + ", should be ported to reputations element");
			string[] array = attribute2.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(':');
				if (array2.Length == 2)
				{
					try
					{
						int value = Convert.ToInt32(array2[1]);
						GenotypeReputation genotypeReputation = new GenotypeReputation();
						genotypeReputation.With = array2[0];
						genotypeReputation.Value = value;
						NewGenotype.Reputations.Add(genotypeReputation);
					}
					catch
					{
					}
				}
			}
		}
		Dictionary<string, Action<XmlDataHelper>> subNodes = null;
		subNodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{
				"stat",
				(XmlDataHelper _) =>
				{
					LoadStatNode(NewGenotype, Reader, bMod);
				}
			},
			{
				"skills",
				(XmlDataHelper _) =>
				{
					Reader.HandleNodes(subNodes);
				}
			},
			{
				"skill",
				(XmlDataHelper _) =>
				{
					LoadSkillNode(NewGenotype, Reader, bMod);
				}
			},
			{
				"removeskill",
				(XmlDataHelper _) =>
				{
					LoadRemoveSkillNode(NewGenotype, Reader, bMod);
				}
			},
			{
				"reputations",
				(XmlDataHelper _) =>
				{
					Reader.HandleNodes(subNodes);
				}
			},
			{
				"reputation",
				(XmlDataHelper _) =>
				{
					LoadReputationNode(NewGenotype, Reader, bMod);
				}
			},
			{
				"savemodifiers",
				(XmlDataHelper _) =>
				{
					Reader.HandleNodes(subNodes);
				}
			},
			{
				"savemodifier",
				(XmlDataHelper _) =>
				{
					LoadSaveModifierNode(NewGenotype, Reader, bMod);
				}
			},
			{
				"chargeninfo",
				(XmlDataHelper _) =>
				{
					LoadChargenInfoNode(NewGenotype, Reader, bMod);
				}
			},
			{
				"extrainfo",
				(XmlDataHelper _) =>
				{
					LoadExtraInfoNode(NewGenotype, Reader, bMod);
				}
			},
			{
				"removeextrainfo",
				(XmlDataHelper _) =>
				{
					LoadRemoveExtraInfoNode(NewGenotype, Reader, bMod);
				}
			}
		};
		Reader.HandleNodes(subNodes);
		return NewGenotype;
	}

	public static void LoadStatNode(GenotypeEntry NewGenotype, XmlDataHelper Reader, bool bMod)
	{
		GenotypeStat genotypeStat = new GenotypeStat();
		genotypeStat.Name = Reader.GetAttribute("Name");
		genotypeStat.ChargenDescription = Reader.GetAttribute("ChargenDescription");
		genotypeStat.Minimum = Reader.GetAttributeInt("Minimum", -999);
		genotypeStat.Maximum = Reader.GetAttributeInt("Maximum", -999);
		genotypeStat.Bonus = Reader.GetAttributeInt("Bonus", -999);
		NewGenotype.Stats[genotypeStat.Name] = genotypeStat;
		Reader.DoneWithElement();
	}

	public static void LoadSkillNode(GenotypeEntry NewGenotype, XmlDataHelper Reader, bool bMod)
	{
		string text = Reader.ParseAttribute<string>("Name", null, required: true);
		string newSkill = CompatManager.GetNewSkill(text);
		if (newSkill != null)
		{
			Reader.ParseWarning("skill \"" + text + "\" was renamed \"" + newSkill + "\"");
			text = newSkill;
		}
		if (!NewGenotype.Skills.Contains(text))
		{
			NewGenotype.Skills.Add(text);
		}
		Reader.DoneWithElement();
	}

	public static void LoadRemoveSkillNode(GenotypeEntry NewGenotype, XmlDataHelper Reader, bool bMod)
	{
		string Skill = Reader.GetAttribute("Name");
		CompatManager.ProcessSkill(ref Skill);
		if (!NewGenotype.RemoveSkills.Contains(Skill))
		{
			NewGenotype.RemoveSkills.Add(Skill);
		}
		Reader.DoneWithElement();
	}

	public static void LoadReputationNode(GenotypeEntry NewGenotype, XmlDataHelper Reader, bool bMod)
	{
		GenotypeReputation genotypeReputation = new GenotypeReputation();
		genotypeReputation.With = Reader.GetAttribute("With");
		genotypeReputation.Value = Reader.GetAttributeInt("Value", -999);
		NewGenotype.Reputations.Add(genotypeReputation);
		Reader.DoneWithElement();
	}

	public static void LoadSaveModifierNode(GenotypeEntry NewGenotype, XmlDataHelper Reader, bool bMod)
	{
		GenotypeSaveModifier genotypeSaveModifier = new GenotypeSaveModifier();
		genotypeSaveModifier.Vs = Reader.GetAttribute("Vs");
		genotypeSaveModifier.Amount = Reader.GetAttributeInt("Amount", -999);
		NewGenotype.SaveModifiers.Add(genotypeSaveModifier);
		Reader.DoneWithElement();
	}

	public static void LoadChargenInfoNode(GenotypeEntry NewGenotype, XmlDataHelper Reader, bool bMod)
	{
		Debug.LogWarning(DataManager.SanitizePathForDisplay(Reader.BaseURI) + " uses chargeninfo element at line " + Reader.LineNumber + ", should be ported to extrainfo elements");
		Reader.Read();
		string[] array = Reader.ReadString().Split('\n');
		foreach (string text in array)
		{
			NewGenotype.ExtraInfo.AddIfNot(text.Replace("ù", "").Trim(), string.IsNullOrEmpty);
		}
		Reader.DoneWithElement();
	}

	public static void LoadExtraInfoNode(GenotypeEntry NewGenotype, XmlDataHelper Reader, bool bMod)
	{
		Reader.Read();
		NewGenotype.ExtraInfo.AddIfNot(Reader.ReadString(), NewGenotype.ExtraInfo.Contains);
		Reader.DoneWithElement();
	}

	public static void LoadRemoveExtraInfoNode(GenotypeEntry NewGenotype, XmlDataHelper Reader, bool bMod)
	{
		Reader.Read();
		NewGenotype.RemoveExtraInfo.AddIfNot(Reader.ReadString(), NewGenotype.RemoveExtraInfo.Contains);
		Reader.DoneWithElement();
	}
}
