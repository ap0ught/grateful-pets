using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace XRL;

[Serializable]
public class GenotypeEntry
{
	public string Name;

	public string DisplayName;

	public int MutationPoints;

	public int CyberneticsLicensePoints;

	public int StatPoints;

	public int RandomWeight;

	public string CharacterBuilderModules;

	public string BodyTypes;

	public string RestrictedGender;

	public string Subtypes;

	public string Class;

	public string Tile;

	public string Gear;

	public string DetailColor;

	public string BodyObject;

	public string BaseHPGain;

	public string BaseSPGain;

	public string BaseMPGain;

	public string StartingLocation;

	public string Species = "human";

	public bool IsMutant;

	public bool IsTrueKin;

	public string _AllowedMutationCategories;

	public List<string> _AllowedMutationCategoriesList;

	public string Constructor;

	public Dictionary<string, GenotypeStat> Stats = new Dictionary<string, GenotypeStat>();

	public List<string> Skills = new List<string>();

	public List<string> RemoveSkills = new List<string>();

	public List<GenotypeReputation> Reputations = new List<GenotypeReputation>();

	public List<GenotypeSaveModifier> SaveModifiers = new List<GenotypeSaveModifier>();

	public List<string> ExtraInfo = new List<string>();

	public List<string> RemoveExtraInfo = new List<string>();

	public bool supportsMutations
	{
		get
		{
			if (!AllowedMutationCategoriesList.IsNullOrEmpty())
			{
				return MutationPoints > 0;
			}
			return false;
		}
	}

	public bool supportsCybernetics => CyberneticsLicensePoints > 0;

	public string AllowedMutationCategories
	{
		get
		{
			return _AllowedMutationCategories;
		}
		set
		{
			_AllowedMutationCategories = value;
			_AllowedMutationCategoriesList = null;
		}
	}

	public List<string> AllowedMutationCategoriesList
	{
		get
		{
			if (_AllowedMutationCategoriesList == null)
			{
				_AllowedMutationCategoriesList = new List<string>();
				if (!AllowedMutationCategories.IsNullOrEmpty())
				{
					string[] array = AllowedMutationCategories.Split(',');
					foreach (string text in array)
					{
						if (text == "*")
						{
							_AllowedMutationCategoriesList.AddRange(from x in MutationFactory.GetCategories()
								select x.Name);
						}
						else if (text.Length > 1 && text.StartsWith("-"))
						{
							_AllowedMutationCategoriesList.Remove(text.Substring(1));
						}
						else
						{
							_AllowedMutationCategoriesList.Add(text);
						}
					}
				}
			}
			return _AllowedMutationCategoriesList;
		}
	}

	public string GetBodyDisplayName()
	{
		return GameObjectFactory.Factory.GetBlueprintIfExists(BodyObject)?.GetTag("BodyDisplayName", BodyObject) ?? BodyObject;
	}

	public void MergeWith(GenotypeEntry newEntry)
	{
		if (!string.IsNullOrEmpty(newEntry.Name))
		{
			Name = newEntry.Name;
		}
		if (!string.IsNullOrEmpty(newEntry.DisplayName))
		{
			DisplayName = newEntry.DisplayName;
		}
		if (!string.IsNullOrEmpty(newEntry.AllowedMutationCategories))
		{
			AllowedMutationCategories = newEntry.AllowedMutationCategories;
		}
		if (!string.IsNullOrEmpty(newEntry.CharacterBuilderModules))
		{
			CharacterBuilderModules = newEntry.CharacterBuilderModules;
		}
		if (!string.IsNullOrEmpty(newEntry.BodyTypes))
		{
			BodyTypes = newEntry.BodyTypes;
		}
		if (!string.IsNullOrEmpty(newEntry.RestrictedGender))
		{
			RestrictedGender = newEntry.RestrictedGender;
		}
		if (!string.IsNullOrEmpty(newEntry.Subtypes))
		{
			Subtypes = newEntry.Subtypes;
		}
		if (!string.IsNullOrEmpty(newEntry.Class))
		{
			Class = newEntry.Class;
		}
		if (!string.IsNullOrEmpty(newEntry.BodyObject))
		{
			BodyObject = newEntry.BodyObject;
		}
		if (!string.IsNullOrEmpty(newEntry.Tile))
		{
			Tile = newEntry.Tile;
		}
		if (!string.IsNullOrEmpty(newEntry.BaseHPGain))
		{
			BaseHPGain = newEntry.BaseHPGain;
		}
		if (!string.IsNullOrEmpty(newEntry.BaseSPGain))
		{
			BaseSPGain = newEntry.BaseSPGain;
		}
		if (!string.IsNullOrEmpty(newEntry.BaseMPGain))
		{
			BaseMPGain = newEntry.BaseMPGain;
		}
		if (!string.IsNullOrEmpty(newEntry.Gear))
		{
			Gear = newEntry.Gear;
		}
		if (!string.IsNullOrEmpty(newEntry.DetailColor))
		{
			DetailColor = newEntry.DetailColor;
		}
		if (!string.IsNullOrEmpty(newEntry.StartingLocation))
		{
			StartingLocation = newEntry.StartingLocation;
		}
		if (!string.IsNullOrEmpty(newEntry.Species))
		{
			Species = newEntry.Species;
		}
		if (!string.IsNullOrEmpty(newEntry.Constructor))
		{
			Constructor = newEntry.Constructor;
		}
		if (newEntry.IsMutant)
		{
			IsMutant = true;
		}
		if (newEntry.IsTrueKin)
		{
			IsTrueKin = true;
		}
		if (newEntry.MutationPoints != -999)
		{
			MutationPoints = newEntry.MutationPoints;
		}
		if (newEntry.CyberneticsLicensePoints != -999)
		{
			CyberneticsLicensePoints = newEntry.CyberneticsLicensePoints;
		}
		if (newEntry.StatPoints != -999)
		{
			StatPoints = newEntry.StatPoints;
		}
		if (newEntry.RandomWeight != -999)
		{
			RandomWeight = newEntry.RandomWeight;
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
		foreach (GenotypeStat value2 in newEntry.Stats.Values)
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
		foreach (GenotypeReputation reputation in newEntry.Reputations)
		{
			Reputations.Add(reputation);
		}
		foreach (GenotypeSaveModifier saveModifier in newEntry.SaveModifiers)
		{
			SaveModifiers.Add(saveModifier);
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

	public List<string> GetChargenInfo()
	{
		List<string> list = new List<string>(ExtraInfo.Count - RemoveExtraInfo.Count);
		foreach (string item in ExtraInfo)
		{
			if (!RemoveExtraInfo.Contains(item))
			{
				list.Add("{{c|ù}} " + item);
			}
		}
		return list;
	}

	public string GetFlatChargenInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in GetChargenInfo())
		{
			stringBuilder.Append(item).Append("\n");
		}
		return stringBuilder.ToString();
	}
}
