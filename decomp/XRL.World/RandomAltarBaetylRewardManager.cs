using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World;

[HasModSensitiveStaticCache]
public static class RandomAltarBaetylRewardManager
{
	public static Dictionary<string, RandomAltarBaetylReward> ByName = new Dictionary<string, RandomAltarBaetylReward>();

	private static readonly Dictionary<string, Action<XmlDataHelper>> _outerNodes = new Dictionary<string, Action<XmlDataHelper>> { { "sparkingbaetyls", HandleInnerNode } };

	private static readonly Dictionary<string, Action<XmlDataHelper>> _innerNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "rewards", HandleInnerNode },
		{ "reward", HandleRewardNode }
	};

	private static List<RandomAltarBaetylReward> Work = new List<RandomAltarBaetylReward>();

	public static void CheckInit()
	{
		if (ByName.Count == 0)
		{
			Init();
		}
	}

	[ModSensitiveCacheInit]
	private static void Init()
	{
		ByName.Clear();
		Loading.LoadTask("Loading SparkingBaetyls.xml", () =>
		{
			foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("sparkingbaetyls"))
			{
				item.HandleNodes(_outerNodes);
			}
		});
	}

	public static void HandleInnerNode(XmlDataHelper xml)
	{
		xml.HandleNodes(_innerNodes);
	}

	public static void HandleRewardNode(XmlDataHelper Reader)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute.IsNullOrEmpty())
		{
			throw new Exception(Reader.Name + " tag had missing or empty Name attribute");
		}
		RandomAltarBaetylReward value = null;
		if (Reader.GetAttribute("Load") != "Merge" || !ByName.TryGetValue(attribute, out value))
		{
			value = new RandomAltarBaetylReward();
			value.Name = attribute;
			ByName[attribute] = value;
		}
		if (Reader.HasAttribute("Description"))
		{
			value.Description = Reader.GetAttribute("Description");
		}
		else if (value.Description.IsNullOrEmpty())
		{
			throw new Exception(Reader.Name + " tag had missing or empty Description attribute");
		}
		if (Reader.HasAttribute("Chance"))
		{
			try
			{
				value.Chance = Convert.ToInt32(Reader.GetAttribute("Chance"));
			}
			catch
			{
				throw new Exception(Reader.Name + " tag had invalid Chance attribute");
			}
		}
		if (Reader.HasAttribute("Weight"))
		{
			try
			{
				value.Weight = Convert.ToInt32(Reader.GetAttribute("Weight"));
			}
			catch
			{
				throw new Exception(Reader.Name + " tag had invalid Weight attribute");
			}
		}
		if (Reader.HasAttribute("Item"))
		{
			value.Item = Reader.GetAttribute("Item");
		}
		if (Reader.HasAttribute("ItemMod"))
		{
			value.ItemMod = Reader.GetAttribute("ItemMod");
		}
		if (Reader.HasAttribute("ItemBonusModChance"))
		{
			value.ItemBonusModChance = Reader.GetAttribute("ItemBonusModChance");
		}
		if (Reader.HasAttribute("ItemSetModNumber"))
		{
			value.ItemSetModNumber = Reader.GetAttribute("ItemSetModNumber");
		}
		if (Reader.HasAttribute("ItemMinorBestowalChance"))
		{
			value.ItemMinorBestowalChance = Reader.GetAttribute("ItemMinorBestowalChance");
		}
		if (Reader.HasAttribute("ItemElementBestowalChance"))
		{
			value.ItemElementBestowalChance = Reader.GetAttribute("ItemElementBestowalChance");
		}
		if (Reader.HasAttribute("AttributePoints"))
		{
			value.AttributePoints = Reader.GetAttribute("AttributePoints");
		}
		if (Reader.HasAttribute("MutationPoints"))
		{
			value.MutationPoints = Reader.GetAttribute("MutationPoints");
		}
		if (Reader.HasAttribute("SkillPoints"))
		{
			value.SkillPoints = Reader.GetAttribute("SkillPoints");
		}
		if (Reader.HasAttribute("ExperiencePoints"))
		{
			value.ExperiencePoints = Reader.GetAttribute("ExperiencePoints");
		}
		if (Reader.HasAttribute("LicensePoints"))
		{
			value.LicensePoints = Reader.GetAttribute("LicensePoints");
		}
		if (Reader.HasAttribute("Reputation"))
		{
			value.Reputation = Reader.GetAttribute("Reputation");
		}
		if (Reader.HasAttribute("ReputationFaction"))
		{
			value.ReputationFaction = Reader.GetAttribute("ReputationFaction");
		}
		if (Reader.HasAttribute("OnlyIfSkill"))
		{
			value.OnlyIfSkill = Reader.GetAttribute("OnlyIfSkill");
		}
		if (Reader.HasAttribute("OnlyIfPart"))
		{
			value.OnlyIfPart = Reader.GetAttribute("OnlyIfPart");
		}
		if (Reader.HasAttribute("OnlyIfEffect"))
		{
			value.OnlyIfEffect = Reader.GetAttribute("OnlyIfEffect");
		}
		if (Reader.HasAttribute("MutantOnly"))
		{
			try
			{
				value.MutantOnly = Convert.ToBoolean(Reader.GetAttribute("MutantOnly"));
			}
			catch
			{
				throw new Exception(Reader.Name + " tag had invalid MutantOnly attribute");
			}
		}
		if (Reader.HasAttribute("TrueKinOnly"))
		{
			try
			{
				value.TrueKinOnly = Convert.ToBoolean(Reader.GetAttribute("TrueKinOnly"));
			}
			catch
			{
				throw new Exception(Reader.Name + " tag had invalid TrueKinOnly attribute");
			}
		}
		Reader.DoneWithElement();
	}

	public static RandomAltarBaetylReward GetReward(string Name)
	{
		CheckInit();
		if (Name != null && ByName.TryGetValue(Name, out var value))
		{
			return value;
		}
		return null;
	}

	public static void GetRewardPool(GameObject Subject, List<RandomAltarBaetylReward> Store)
	{
		if (!GameObject.Validate(ref Subject))
		{
			return;
		}
		CheckInit();
		foreach (RandomAltarBaetylReward value in ByName.Values)
		{
			if (value.Weight > 0 && (!value.MutantOnly || Subject.IsMutant()) && (!value.TrueKinOnly || Subject.IsTrueKin()) && (value.OnlyIfSkill.IsNullOrEmpty() || Subject.HasSkill(value.OnlyIfSkill)) && (value.OnlyIfPart.IsNullOrEmpty() || Subject.HasPart(value.OnlyIfPart)) && (value.OnlyIfEffect.IsNullOrEmpty() || Subject.HasEffect(value.OnlyIfEffect)) && value.Chance.in100())
			{
				for (int i = 0; i < value.Weight; i++)
				{
					Store.Add(value);
				}
			}
		}
	}

	public static List<RandomAltarBaetylReward> GetRewardPool(GameObject Subject)
	{
		List<RandomAltarBaetylReward> list = new List<RandomAltarBaetylReward>();
		GetRewardPool(Subject, list);
		return list;
	}

	public static RandomAltarBaetylReward GetRandomReward(GameObject Subject)
	{
		Work.Clear();
		GetRewardPool(Subject, Work);
		return Work.GetRandomElement();
	}
}
