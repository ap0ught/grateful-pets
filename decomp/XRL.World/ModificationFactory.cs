using System;
using System.Collections.Generic;
using System.Xml;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Tinkering;

namespace XRL.World;

[HasModSensitiveStaticCache]
public class ModificationFactory
{
	[ModSensitiveStaticCache(false)]
	private static List<ModEntry> _ModList;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, List<ModEntry>> _ModTable;

	[ModSensitiveStaticCache(false)]
	public static Dictionary<string, ModEntry> _ModsByPart;

	[NonSerialized]
	private static List<int> rarityCodes;

	[NonSerialized]
	private static List<ModEntry> modsEligible = new List<ModEntry>(32);

	[NonSerialized]
	private static Dictionary<ModEntry, int> modDist = new Dictionary<ModEntry, int>(32);

	[NonSerialized]
	private static Dictionary<int, int> rarityCodeWeights = new Dictionary<int, int>(4);

	public static List<ModEntry> ModList
	{
		get
		{
			CheckInit();
			return _ModList;
		}
	}

	public static Dictionary<string, List<ModEntry>> ModTable
	{
		get
		{
			CheckInit();
			return _ModTable;
		}
	}

	public static Dictionary<string, ModEntry> ModsByPart
	{
		get
		{
			CheckInit();
			return _ModsByPart;
		}
	}

	public static void CheckInit()
	{
		if (_ModTable == null)
		{
			Loading.LoadTask("Loading Mods.xml", LoadMods);
		}
		if (rarityCodes != null)
		{
			return;
		}
		rarityCodes = new List<int>();
		foreach (ModEntry mod in ModList)
		{
			if (!rarityCodes.Contains(mod.Rarity))
			{
				rarityCodes.Add(mod.Rarity);
			}
		}
	}

	private static void LoadMods()
	{
		_ModList = new List<ModEntry>(128);
		_ModTable = new Dictionary<string, List<ModEntry>>(32);
		_ModsByPart = new Dictionary<string, ModEntry>(128);
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Mods"))
		{
			try
			{
				item.WhitespaceHandling = WhitespaceHandling.None;
				while (item.Read())
				{
					if (item.Name == "mods")
					{
						LoadModsNode(item, isPrimary: false);
					}
				}
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.modInfo, message);
			}
		}
		foreach (ModEntry mod in _ModList)
		{
			if (mod.Tables.IsNullOrEmpty())
			{
				continue;
			}
			string[] array = mod.Tables.Split(',');
			foreach (string key in array)
			{
				if (!_ModTable.ContainsKey(key))
				{
					_ModTable.Add(key, new List<ModEntry>());
				}
				_ModTable[key].Add(mod);
			}
		}
	}

	public static void LoadModsNode(XmlTextReader Reader, bool isPrimary = true)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "mod")
			{
				LoadModNode(Reader, isPrimary);
			}
		}
	}

	public static void LoadModNode(XmlTextReader Reader, bool isPrimary = true)
	{
		string attribute = Reader.GetAttribute("Part");
		if (!_ModsByPart.TryGetValue(attribute, out var value))
		{
			value = new ModEntry
			{
				Part = attribute
			};
			_ModsByPart.Add(attribute, value);
			_ModList.Add(value);
		}
		string attribute2 = Reader.GetAttribute("MinTier");
		if (!attribute2.IsNullOrEmpty())
		{
			value.MinTier = Convert.ToInt32(attribute2);
		}
		attribute2 = Reader.GetAttribute("MaxTier");
		if (!attribute2.IsNullOrEmpty())
		{
			value.MaxTier = Convert.ToInt32(attribute2);
		}
		attribute2 = Reader.GetAttribute("NativeTier");
		if (!attribute2.IsNullOrEmpty())
		{
			value.NativeTier = Convert.ToInt32(attribute2);
		}
		attribute2 = Reader.GetAttribute("TinkerTier");
		if (!attribute2.IsNullOrEmpty())
		{
			value.TinkerTier = Convert.ToInt32(attribute2);
		}
		attribute2 = Reader.GetAttribute("Value");
		if (!attribute2.IsNullOrEmpty())
		{
			value.Value = double.Parse(attribute2);
		}
		attribute2 = Reader.GetAttribute("Description");
		if (attribute2 != null)
		{
			value.Description = attribute2;
		}
		attribute2 = Reader.GetAttribute("TinkerDisplayName");
		if (attribute2 != null)
		{
			value.TinkerDisplayName = attribute2;
		}
		attribute2 = Reader.GetAttribute("TinkerIngredient");
		if (attribute2 != null)
		{
			value.TinkerIngredient = attribute2;
		}
		attribute2 = Reader.GetAttribute("Tables");
		if (attribute2 != null)
		{
			value.Tables = attribute2;
		}
		attribute2 = Reader.GetAttribute("TinkerAllowed");
		if (!attribute2.IsNullOrEmpty())
		{
			value.TinkerAllowed = !attribute2.EqualsNoCase("false");
		}
		attribute2 = Reader.GetAttribute("BonusAllowed");
		if (!attribute2.IsNullOrEmpty())
		{
			value.BonusAllowed = !attribute2.EqualsNoCase("false");
		}
		attribute2 = Reader.GetAttribute("CanAutoTinker");
		if (!attribute2.IsNullOrEmpty())
		{
			value.CanAutoTinker = !attribute2.EqualsNoCase("false");
		}
		attribute2 = Reader.GetAttribute("NoSparkingQuest");
		if (!attribute2.IsNullOrEmpty())
		{
			value.NoSparkingQuest = attribute2.EqualsNoCase("true");
		}
		attribute2 = Reader.GetAttribute("Rarity");
		if (!attribute2.IsNullOrEmpty())
		{
			value.Rarity = getRarityCode(attribute2);
		}
		attribute2 = Reader.GetAttribute("TinkerCategory");
		if (attribute2 != null)
		{
			value.TinkerCategory = attribute2;
		}
		if (Reader.NodeType != XmlNodeType.EndElement && !Reader.IsEmptyElement)
		{
			while (Reader.Read() && (Reader.NodeType != XmlNodeType.EndElement || (!(Reader.Name == "") && !(Reader.Name == "mod"))))
			{
			}
		}
	}

	public static int getRarityCode(string rarity)
	{
		return rarity switch
		{
			"C" => 0, 
			"U" => 1, 
			"R" => 2, 
			"R2" => 3, 
			"R3" => 4, 
			_ => throw new Exception("Unknown rarity " + rarity), 
		};
	}

	public static int getBaseRarityWeight(int rarityCode)
	{
		return rarityCode switch
		{
			0 => 100000, 
			1 => 40000, 
			2 => 10500, 
			3 => 1500, 
			4 => 150, 
			_ => throw new Exception("unknown rarity code " + rarityCode), 
		};
	}

	public static int getTierRarityWeight(int modNativeTier, int itemTier)
	{
		int num = 100;
		if (itemTier < modNativeTier)
		{
			num /= (modNativeTier - itemTier) * 5;
		}
		return num;
	}

	private static int fuzzTier(int tier)
	{
		while (true)
		{
			int num = Stat.Random(1, 100);
			if (num <= 10 && tier > 1)
			{
				tier--;
				continue;
			}
			if (num > 20 || tier >= 8)
			{
				break;
			}
			tier++;
		}
		return tier;
	}

	public static int ApplyModifications(GameObject GO, GameObjectBlueprint Blueprint, int BonusModChance, int SetModNumber, string Context = null)
	{
		int num = 0;
		CheckInit();
		if (BonusModChance <= -999 && SetModNumber <= 0)
		{
			return num;
		}
		try
		{
			int num2 = 3;
			int result;
			if (GO.HasIntProperty("BaseModChance"))
			{
				num2 = GO.GetIntProperty("BaseModChance");
			}
			else if (int.TryParse(GO.GetTag("BaseModChance"), out result))
			{
				num2 = result;
			}
			int num3 = num2 + BonusModChance;
			if (num3 <= 0 && (!The.Core.CheatMaxMod || BonusModChance <= -999) && SetModNumber <= 0)
			{
				return num;
			}
			string tag = Blueprint.GetTag("Mods");
			if (tag.IsNullOrEmpty())
			{
				return num;
			}
			List<string> list = tag.CachedCommaExpansion();
			int Tier = 0;
			modDist.Clear();
			modsEligible.Clear();
			rarityCodeWeights.Clear();
			if (XRLCore.Core.CheatMaxMod)
			{
				num3 = 100;
			}
			for (int i = 0; i < 3; i++)
			{
				bool flag = SetModNumber > 0;
				if (!flag && !num3.in100())
				{
					continue;
				}
				if (flag)
				{
					SetModNumber--;
				}
				if (Tier == 0)
				{
					Tier = 1;
					string tag2 = Blueprint.GetTag("TechTier");
					if (!tag2.IsNullOrEmpty())
					{
						Tier = Convert.ToInt32(tag2);
					}
					else
					{
						string tag3 = Blueprint.GetTag("Tier");
						if (!tag3.IsNullOrEmpty())
						{
							Tier = Convert.ToInt32(tag3);
						}
					}
					XRL.World.Capabilities.Tier.Constrain(ref Tier);
				}
				modDist.Clear();
				modsEligible.Clear();
				rarityCodeWeights.Clear();
				int num4 = 0;
				foreach (string item in list)
				{
					if (!_ModTable.TryGetValue(item, out var value))
					{
						continue;
					}
					foreach (ModEntry item2 in value)
					{
						if (Tier >= item2.MinTier && Tier <= item2.MaxTier && (item2.CanAutoTinker || Context != "Tinkering" || (flag && item2.BonusAllowed)) && !modsEligible.Contains(item2) && ItemModding.ModificationApplicable(item2.Part, GO))
						{
							modsEligible.Add(item2);
						}
					}
				}
				foreach (ModEntry item3 in modsEligible)
				{
					if (!rarityCodeWeights.ContainsKey(item3.Rarity))
					{
						rarityCodeWeights.Add(item3.Rarity, 0);
					}
					rarityCodeWeights[item3.Rarity] += getTierRarityWeight(item3.NativeTier, Tier);
				}
				foreach (ModEntry item4 in modsEligible)
				{
					int baseWeight = getBaseRarityWeight(item4.Rarity) * getTierRarityWeight(item4.NativeTier, Tier) / rarityCodeWeights[item4.Rarity];
					baseWeight = GetModRarityWeightEvent.GetFor(GO, item4, baseWeight);
					if (baseWeight > 0)
					{
						modDist.Add(item4, baseWeight);
						num4 += baseWeight;
					}
				}
				if (modDist.Count <= 0)
				{
					break;
				}
				int num5 = 0;
				foreach (int rarityCode in rarityCodes)
				{
					if (!rarityCodeWeights.ContainsKey(rarityCode))
					{
						num5 += getBaseRarityWeight(rarityCode);
					}
				}
				if (num5 == 0 || Stat.Random(1, num4 + num5) <= num4)
				{
					ModEntry randomElement = modDist.GetRandomElement();
					if (randomElement != null && randomElement.Part != null && !GO.HasPart(randomElement.Part) && ItemModding.ApplyModification(GO, randomElement.Part, fuzzTier(Tier)))
					{
						num++;
					}
				}
			}
		}
		catch (Exception ex)
		{
			XRLCore.LogError(ex);
			MetricsManager.LogException("ApplyModification", ex);
		}
		finally
		{
			modDist.Clear();
			modsEligible.Clear();
			rarityCodeWeights.Clear();
		}
		return num;
	}
}
