using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HistoryKit;
using Newtonsoft.Json.Linq;
using Qud.API;
using XRL.Annals;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World;

/// <summary>
/// Base game object
/// </summary>
[HasModSensitiveStaticCache]
public static class RelicGenerator
{
	public static List<string> Types = new List<string>();

	public static Dictionary<string, string> TypeMap = new Dictionary<string, string>();

	private static readonly Dictionary<string, Action<XmlDataHelper>> _outerNodes = new Dictionary<string, Action<XmlDataHelper>> { { "relics", HandleInnerNode } };

	private static readonly Dictionary<string, Action<XmlDataHelper>> _innerNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "relictypes", HandleInnerNode },
		{ "relictypemappings", HandleInnerNode },
		{ "relictype", HandleRelicTypeNode },
		{ "removerelictype", HandleRemoveRelicTypeNode },
		{ "relictypemapping", HandleRelicTypeMappingNode },
		{ "removerelictypemapping", HandleRemoveRelicTypeMappingNode }
	};

	private static Dictionary<string, string> RelicNameContext = new Dictionary<string, string>();

	public static void CheckInit()
	{
		if (Types.Count == 0)
		{
			Init();
		}
	}

	[ModSensitiveCacheInit]
	public static void Init()
	{
		Types.Clear();
		TypeMap.Clear();
		Loading.LoadTask("Loading Relics.xml", () =>
		{
			foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("relics"))
			{
				item.HandleNodes(_outerNodes);
			}
		});
	}

	public static void HandleInnerNode(XmlDataHelper xml)
	{
		xml.HandleNodes(_innerNodes);
	}

	public static void HandleRelicTypeNode(XmlDataHelper Reader)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute.IsNullOrEmpty())
		{
			throw new Exception(Reader.Name + " tag had missing or empty Name attribute");
		}
		if (!Types.Contains(attribute))
		{
			Types.Add(attribute);
		}
		Reader.DoneWithElement();
	}

	public static void HandleRemoveRelicTypeNode(XmlDataHelper Reader)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute.IsNullOrEmpty())
		{
			throw new Exception(Reader.Name + " tag had missing or empty Name attribute");
		}
		Types.Remove(attribute);
		Reader.DoneWithElement();
	}

	public static void HandleRelicTypeMappingNode(XmlDataHelper Reader)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute.IsNullOrEmpty())
		{
			throw new Exception(Reader.Name + " tag had missing or empty Name attribute");
		}
		string attribute2 = Reader.GetAttribute("Type");
		if (attribute2.IsNullOrEmpty())
		{
			throw new Exception(Reader.Name + " tag had missing or empty Type attribute");
		}
		if (!Types.Contains(attribute2))
		{
			throw new Exception(Reader.Name + " tag had unknown Type \"" + attribute2 + "\"");
		}
		TypeMap[attribute] = attribute2;
		Reader.DoneWithElement();
	}

	public static void HandleRemoveRelicTypeMappingNode(XmlDataHelper Reader)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute.IsNullOrEmpty())
		{
			throw new Exception(Reader.Name + " tag had missing or empty Name attribute");
		}
		TypeMap.Remove(attribute);
		Reader.DoneWithElement();
	}

	public static List<string> GetTypes()
	{
		CheckInit();
		return Types;
	}

	public static string GenerateRelicName(string Type, HistoricEntitySnapshot SnapRegion, string Element, out string Article)
	{
		Article = null;
		if (SnapRegion != null)
		{
			return GenerateRelicNameByRegion(Type, SnapRegion, Element, out Article);
		}
		RelicNameContext["*element*"] = Element;
		RelicNameContext["*itemType*"] = Type;
		string Name = HistoricStringExpander.ExpandString("<spice.history.relics.names.!random>", null, null, RelicNameContext);
		if (Name.Contains("*personNounPossessive*"))
		{
			string text = "<spice.personNouns.!random>";
			string text2 = HistoricStringExpander.ExpandString(text);
			Name = ((!(text2 == text)) ? Name.Replace("*personNounPossessive*", Grammar.MakePossessive(text2)) : Name.Replace("*personNounPossessive*", "*creatureNamePossessive*"));
		}
		if (Name.Contains("*creatureNamePossessive*"))
		{
			Name = Name.Replace("*creatureNamePossessive*", Grammar.MakePossessive(NameMaker.MakeName(EncountersAPI.GetACreature(), null, null, null, null, null, null, null, null, null, "Relic", null, null, FailureOkay: false, SpecialFaildown: true)));
		}
		QudHistoryHelpers.ExtractArticle(ref Name, out Article);
		return QudHistoryHelpers.Ansify(Grammar.MakeTitleCase(Name));
	}

	public static string GenerateRelicNameByRegion(string Type, HistoricEntitySnapshot SnapRegion, string Element, out string Article)
	{
		History sultanHistory = The.Game.sultanHistory;
		string phrase = HistoricStringExpander.ExpandString("<spice.elements." + Element + ".adjectives.!random> " + HistoricStringExpander.ExpandString("<spice.itemTypes." + Type + ".!random>", null, sultanHistory) + " of " + SnapRegion.GetProperty("newName"), null, sultanHistory);
		Article = "the";
		return QudHistoryHelpers.Ansify(Grammar.MakeTitleCase(phrase));
	}

	public static GameObject GenerateSpindleNegotiationRelic(string ItemType, string SparedFaction, string BetrayedFaction, string PlayerNameAndAppositive, int Tier = 4)
	{
		List<string> adjectives = new List<string> { HistoricStringExpander.ExpandString("<spice.elements.!random>") };
		Dictionary<string, List<string>> listProperties = new Dictionary<string, List<string>>
		{
			{
				"likedFactions",
				new List<string> { SparedFaction }
			},
			{
				"hatedFactions",
				new List<string> { BetrayedFaction }
			}
		};
		return GenerateRelic(ItemType, Tier, null, adjectives, listProperties, null, null, PlayerNameAndAppositive);
	}

	public static GameObject GenerateRelic(int Tier, bool RandomName = false)
	{
		return GenerateRelic(The.Game.sultanHistory.entities.GetRandomElement().GetCurrentSnapshot(), Tier, null, RandomName);
	}

	public static GameObject GenerateRelic(HistoricEntitySnapshot Snapshot, int Tier, string Type = null, bool RandomName = false)
	{
		List<string> list = new List<string>();
		list.AddRange(Snapshot.properties.Values);
		foreach (List<string> value in Snapshot.listProperties.Values)
		{
			list.AddRange(value);
		}
		if (Type == null)
		{
			Type = Snapshot.GetProperty("itemType");
		}
		return GenerateRelic(Type, Tier, Snapshot.GetProperty("type").Equals("region") ? Snapshot : null, list, Snapshot.listProperties, RandomName ? null : Snapshot.Name, RandomName ? null : Snapshot.GetProperty("article", null));
	}

	public static GameObject GenerateRelic(HistoricEntitySnapshot Snapshot, string Type = null, bool RandomName = false)
	{
		List<string> list = new List<string>();
		list.AddRange(Snapshot.properties.Values);
		foreach (List<string> value in Snapshot.listProperties.Values)
		{
			list.AddRange(value);
		}
		if (Type == null)
		{
			Type = Snapshot.GetProperty("itemType");
		}
		int relicTierFromPeriod = GetRelicTierFromPeriod(int.Parse(Snapshot.GetProperty("period")));
		return GenerateRelic(Type, relicTierFromPeriod, Snapshot.GetProperty("type").Equals("region") ? Snapshot : null, list, Snapshot.listProperties, RandomName ? null : Snapshot.GetProperty("Name", null), RandomName ? null : Snapshot.GetProperty("article", null));
	}

	public static int GetRelicTierFromPeriod(int Period)
	{
		switch (Period)
		{
		case 5:
			if (!If.CoinFlip())
			{
				return 2;
			}
			return 1;
		case 4:
			if (!If.CoinFlip())
			{
				return 4;
			}
			return 3;
		case 3:
			if (!If.CoinFlip())
			{
				return 6;
			}
			return 5;
		case 2:
			return 7;
		case 1:
			return 8;
		default:
			return 4;
		}
	}

	public static int GetPeriodFromRelicTier(int Tier)
	{
		switch (Tier)
		{
		case 0:
		case 1:
		case 2:
			return 5;
		case 3:
		case 4:
			return 4;
		case 5:
		case 6:
			return 3;
		case 7:
			return 2;
		case 8:
			return 1;
		default:
			return 4;
		}
	}

	public static int GetAttributeBonusFromRelicTier(int Tier)
	{
		if (Tier <= 4)
		{
			if (!If.CoinFlip())
			{
				return 2;
			}
			return 1;
		}
		if (!If.CoinFlip())
		{
			return 4;
		}
		return 3;
	}

	public static int GetResistanceBonusFromRelicTier(int Tier)
	{
		int num = Stat.Random(20, 30);
		if (Tier >= 4)
		{
			num += (Tier - 4) * 20;
		}
		return num;
	}

	public static int GetMoveSpeedBonusFromRelicTier(int Tier)
	{
		return -(8 + (Tier - 1) * 3 + Stat.Random(1, 4));
	}

	public static string TranslateAdjective(string Input)
	{
		Input = Input.ToLower();
		if (Input == "none")
		{
			return Input;
		}
		foreach (JProperty item in ((JObject)HistoricSpice.root["elements"]).Properties())
		{
			if (Input.EqualsNoCase(item.Name))
			{
				return Input;
			}
			foreach (JToken item2 in item.Value["adjectives"].Children())
			{
				if (Input.EqualsNoCase((string?)item2))
				{
					return item.Name.ToLower();
				}
			}
		}
		return null;
	}

	public static bool GiveRandomMeleeWeaponBestowal(GameObject Object, int Tier = 1, bool Standard = false, bool ShowInShortDescription = false)
	{
		MeleeWeapon part = Object.GetPart<MeleeWeapon>();
		if (part == null)
		{
			return false;
		}
		switch (Stat.Random(0, 4))
		{
		case 0:
			if (part.AdjustDamageDieSize(2))
			{
				if (ShowInShortDescription)
				{
					Object.RequirePart<NoteImprovedDamageDieSize>().Amount += 2;
				}
				return true;
			}
			break;
		case 1:
			if (part.AdjustDamage(1))
			{
				if (ShowInShortDescription)
				{
					Object.RequirePart<NoteImprovedDamageRoll>().Amount++;
				}
				return true;
			}
			break;
		case 2:
			if (part.AdjustBonusCap(3))
			{
				if (ShowInShortDescription)
				{
					Object.RequirePart<NoteImprovedBonusCap>().Amount += 3;
				}
				return true;
			}
			break;
		case 3:
			part.HitBonus += 2;
			return true;
		}
		part.PenBonus++;
		if (ShowInShortDescription)
		{
			Object.RequirePart<NoteImprovedPenetrationBonus>().Amount++;
		}
		return true;
	}

	public static bool GiveRandomMissileWeaponBestowal(GameObject Object, int Tier = 1, bool Standard = false, bool ShowInShortDescription = false)
	{
		MissileWeapon part = Object.GetPart<MissileWeapon>();
		if (part == null)
		{
			return false;
		}
		int num = Stat.Random(0, 7);
		if (num == 0 && !part.NoWildfire)
		{
			part.NoWildfire = true;
			return true;
		}
		switch (num)
		{
		case 1:
		{
			NoteImprovedRateOfFire noteImprovedRateOfFire = (ShowInShortDescription ? Object.RequirePart<NoteImprovedRateOfFire>() : null);
			if (part.AmmoPerAction == part.ShotsPerAction)
			{
				part.AmmoPerAction++;
				if (noteImprovedRateOfFire != null)
				{
					noteImprovedRateOfFire.AmmoAmount++;
				}
			}
			part.ShotsPerAction++;
			if (noteImprovedRateOfFire != null)
			{
				noteImprovedRateOfFire.ShotsAmount++;
			}
			MagazineAmmoLoader part2 = Object.GetPart<MagazineAmmoLoader>();
			if (part2 != null && part2.MaxAmmo < part.AmmoPerAction)
			{
				if (noteImprovedRateOfFire != null)
				{
					noteImprovedRateOfFire.AmmoCapacityAmount += part.AmmoPerAction - part2.MaxAmmo;
				}
				part2.MaxAmmo = part.AmmoPerAction;
			}
			return true;
		}
		case 2:
			Object.RequirePart<MissilePerformance>().PenetrationModifier++;
			if (ShowInShortDescription)
			{
				Object.RequirePart<MissilePerformance>().ShowInShortDescription = true;
			}
			return true;
		case 3:
			Object.RequirePart<MissilePerformance>().DamageDieModifier += 2;
			if (ShowInShortDescription)
			{
				Object.RequirePart<MissilePerformance>().ShowInShortDescription = true;
			}
			return true;
		case 4:
			Object.RequirePart<MissilePerformance>().DamageModifier++;
			if (ShowInShortDescription)
			{
				Object.RequirePart<MissilePerformance>().ShowInShortDescription = true;
			}
			return true;
		case 5:
			Object.RequirePart<MissilePerformance>().PenetrateCreatures = true;
			if (ShowInShortDescription)
			{
				Object.RequirePart<MissilePerformance>().ShowInShortDescription = true;
			}
			return true;
		case 6:
			if (!MissileWeapon.IsVorpal(Object))
			{
				Object.RequirePart<MissilePerformance>().WantAddAttribute("Vorpal");
				if (ShowInShortDescription)
				{
					Object.RequirePart<MissilePerformance>().ShowInShortDescription = true;
				}
				return true;
			}
			break;
		}
		if (part.WeaponAccuracy > 0)
		{
			part.WeaponAccuracy = Math.Max(part.WeaponAccuracy - Stat.Random(5, 10), 0);
		}
		else
		{
			part.AimVarianceBonus += Stat.Random(2, 6);
		}
		if (ShowInShortDescription)
		{
			Object.RequirePart<NoteImprovedAccuracy>();
		}
		return true;
	}

	public static bool GiveRandomArmorBestowal(GameObject Object, int Tier = 1, bool Standard = false, bool ShowInShortDescription = false)
	{
		Armor part = Object.GetPart<Armor>();
		if (part == null)
		{
			return false;
		}
		switch (Stat.Random(0, 14))
		{
		case 0:
			part.DV++;
			if (ShowInShortDescription)
			{
				Object.RequirePart<NoteImprovedDodgeValue>().Amount++;
			}
			break;
		case 1:
			part.AV++;
			if (ShowInShortDescription)
			{
				Object.RequirePart<NoteImprovedArmorValue>().Amount++;
			}
			break;
		case 2:
			part.MA += 2;
			break;
		case 3:
			part.Acid += 10;
			break;
		case 4:
			part.Cold += 10;
			break;
		case 5:
			part.Heat += 10;
			break;
		case 6:
			part.Elec += 10;
			break;
		case 7:
			part.Strength++;
			break;
		case 8:
			part.Agility++;
			break;
		case 9:
			part.Toughness++;
			break;
		case 10:
			part.Intelligence++;
			break;
		case 11:
			part.Willpower++;
			break;
		case 12:
			part.Ego++;
			break;
		case 13:
			part.ToHit += 2;
			break;
		case 14:
			if (part.SpeedPenalty > 0)
			{
				int num = Math.Max(part.SpeedPenalty - Stat.Random(5, 10), 0);
				if (ShowInShortDescription)
				{
					Object.RequirePart<NoteImprovedArmorValue>().Amount += num - part.SpeedPenalty;
				}
				part.SpeedPenalty = num;
			}
			else
			{
				part.SpeedBonus += Stat.Random(1, 5);
			}
			break;
		}
		return true;
	}

	public static bool GiveRandomShieldBestowal(GameObject Object, int Tier = 1, bool Standard = false, bool ShowInShortDescription = false)
	{
		Shield part = Object.GetPart<Shield>();
		if (part == null)
		{
			return false;
		}
		switch ((!Standard) ? Stat.Random(0, 3) : 0)
		{
		case 0:
			if (!Object.HasPart<ModImprovedBlock>())
			{
				Object.AddPart(new ModImprovedBlock(Tier));
				return true;
			}
			if (!Standard)
			{
				break;
			}
			if (part.AV < 1)
			{
				if (ShowInShortDescription)
				{
					Object.RequirePart<NoteImprovedArmorValue>().Amount += 1 - part.AV;
				}
				part.AV = 1;
			}
			return true;
		case 1:
			if (part.DV >= 0)
			{
				break;
			}
			part.DV++;
			if (ShowInShortDescription)
			{
				Object.RequirePart<NoteImprovedDodgeValue>().Amount++;
			}
			if (part.DV < 0 && 50.in100())
			{
				part.DV++;
				if (ShowInShortDescription)
				{
					Object.RequirePart<NoteImprovedDodgeValue>().Amount++;
				}
			}
			return true;
		case 2:
			if (part.SpeedPenalty > 0)
			{
				int num = Math.Max(part.SpeedPenalty - Stat.Random(5, 10), 0);
				if (ShowInShortDescription)
				{
					Object.RequirePart<NoteImprovedArmorValue>().Amount += num - part.SpeedPenalty;
				}
				part.SpeedPenalty = num;
				return true;
			}
			break;
		}
		part.AV++;
		if (ShowInShortDescription)
		{
			Object.RequirePart<NoteImprovedArmorValue>().Amount++;
		}
		return true;
	}

	public static bool GiveRandomImplantBestowal(GameObject Object, int Tier = 1, bool Standard = false, bool ShowInShortDescription = false)
	{
		Object.ModIntProperty("CyberneticRejectionSyndromeModifier", -Stat.Random(1, Tier * 2), RemoveIfZero: true);
		CyberneticsBaseItem part = Object.GetPart<CyberneticsBaseItem>();
		if (part == null)
		{
			return false;
		}
		int num = Stat.Random(0, 5);
		if (num == 0 && part.Cost > 1)
		{
			NoteReducedLicensePoints noteReducedLicensePoints = (ShowInShortDescription ? Object.RequirePart<NoteReducedLicensePoints>() : null);
			part.Cost--;
			if (noteReducedLicensePoints != null)
			{
				noteReducedLicensePoints.Amount++;
			}
			if (part.Cost > 2 && Tier >= 4)
			{
				part.Cost--;
				if (noteReducedLicensePoints != null)
				{
					noteReducedLicensePoints.Amount++;
				}
				if (part.Cost > 3 && Tier >= 8)
				{
					part.Cost--;
					if (noteReducedLicensePoints != null)
					{
						noteReducedLicensePoints.Amount++;
					}
				}
			}
			return true;
		}
		if (num <= 1 && !part.Slots.HasDelimitedSubstring(',', "Hand") && (part.Slots.HasDelimitedSubstring(',', "Hands") || part.Slots.HasDelimitedSubstring(',', "Arm")) && !Object.HasTag("CyberneticsModifiesAnatomy") && !Object.HasTag("CyberneticsUsesEqSlot"))
		{
			part.Slots += ",Hand";
			if (ShowInShortDescription)
			{
				Object.RequirePart<NoteExpandedSlots>().AddSlot("Hand");
			}
			return true;
		}
		if (num <= 1 && !part.Slots.HasDelimitedSubstring(',', "Arm") && (part.Slots.HasDelimitedSubstring(',', "Body") || part.Slots.HasDelimitedSubstring(',', "Back") || part.Slots.HasDelimitedSubstring(',', "Head") || part.Slots.HasDelimitedSubstring(',', "Face")) && !Object.HasTag("CyberneticsModifiesAnatomy") && !Object.HasTag("CyberneticsUsesEqSlot"))
		{
			part.Slots += ",Arm";
			if (ShowInShortDescription)
			{
				Object.RequirePart<NoteExpandedSlots>().AddSlot("Arm");
			}
			return true;
		}
		if (num <= 1 && !part.Slots.HasDelimitedSubstring(',', "Face") && (part.Slots.HasDelimitedSubstring(',', "Head") || part.Slots.HasDelimitedSubstring(',', "Arm") || part.Slots.HasDelimitedSubstring(',', "Back")) && !Object.HasTag("CyberneticsModifiesAnatomy") && !Object.HasTag("CyberneticsUsesEqSlot"))
		{
			part.Slots += ",Face";
			if (ShowInShortDescription)
			{
				Object.RequirePart<NoteExpandedSlots>().AddSlot("Face");
			}
			return true;
		}
		if (num <= 1 && !part.Slots.HasDelimitedSubstring(',', "Tail") && (part.Slots.HasDelimitedSubstring(',', "Hands") || part.Slots.HasDelimitedSubstring(',', "Hand") || part.Slots.HasDelimitedSubstring(',', "Arm") || part.Slots.HasDelimitedSubstring(',', "Back")) && !Object.HasTag("CyberneticsModifiesAnatomy") && !Object.HasTag("CyberneticsUsesEqSlot"))
		{
			part.Slots += ",Tail";
			if (ShowInShortDescription)
			{
				Object.RequirePart<NoteExpandedSlots>().AddSlot("Tail");
			}
			return true;
		}
		if (num <= 2 && !Object.WantEvent(SingletonEvent<GetAvailableComputePowerEvent>.ID, GetAvailableComputePowerEvent.CascadeLevel))
		{
			ComputeNode computeNode = new ComputeNode();
			computeNode.ChargeUse = 0;
			computeNode.IsPowerLoadSensitive = false;
			computeNode.WorksOn(AdjacentCellContents: false, Carrier: false, CellContents: false, Enclosed: false, Equipper: false, Holder: false, Implantee: true);
			computeNode.Power = Stat.Random(2, (Tier + 2) * 2);
			Object.AddPart(computeNode);
			SyncImplantPart(Object, computeNode);
			return true;
		}
		if (num <= 3 && AddImplantAttributeBoost(Object, Statistic.Attributes.GetRandomElement(), Tier, Minor: true))
		{
			return true;
		}
		if (num <= 4 && !Object.HasPart<CyberneticsSingleSkillsoft>() && (part.Slots.HasDelimitedSubstring(',', "Head") || part.Slots.HasDelimitedSubstring(',', "Face") || part.Slots.HasDelimitedSubstring(',', "Arm") || part.Slots.HasDelimitedSubstring(',', "Back") || part.Slots.HasDelimitedSubstring(',', "Body")))
		{
			CyberneticsSingleSkillsoft cyberneticsSingleSkillsoft = new CyberneticsSingleSkillsoft();
			cyberneticsSingleSkillsoft.AddOn = true;
			Object.AddPart(cyberneticsSingleSkillsoft);
			SyncImplantPart(Object, cyberneticsSingleSkillsoft);
			return true;
		}
		if (Object.Weight > 0)
		{
			int weight = Object.Physics._Weight;
			if (weight > 0)
			{
				int i = 0;
				for (int num2 = 1 + Tier / 4; i < num2; i++)
				{
					if (Object.Physics._Weight >= 8)
					{
						Object.Physics._Weight /= 2;
					}
					else if (Object.Physics._Weight > 0)
					{
						Object.Physics._Weight--;
					}
				}
				if (ShowInShortDescription)
				{
					Object.RequirePart<NoteReducedWeight>().Amount += weight - Object.Physics.Weight;
				}
				return true;
			}
		}
		return false;
	}

	private static GameObject GenerateBaseRelic(ref string Type, int Tier, bool AllowAlternate = false)
	{
		CheckInit();
		bool flag = 20.in100();
		if (Type.IsNullOrEmpty() || Type == "unknown")
		{
			Type = Types.GetRandomElement();
		}
		else if (!Types.Contains(Type))
		{
			if (TypeMap.TryGetValue(Type, out var value))
			{
				Type = value;
			}
			else if (Type.Any(char.IsUpper))
			{
				string key = Type.ToLower();
				if (TypeMap.TryGetValue(key, out var value2))
				{
					Type = value2;
				}
			}
		}
		if (Type == "Curio")
		{
			Type = "Artifact";
		}
		XRL.World.Capabilities.Tier.Constrain(ref Tier);
		string text = null;
		string text2 = "BaseRelic_" + Type;
		string text3 = text2 + Tier;
		if (flag)
		{
			string text4 = text3 + "th";
			if (PopulationManager.TableExists(text4))
			{
				PopulationResult populationResult = PopulationManager.RollOneFrom(text4);
				if (populationResult != null)
				{
					text = populationResult.Blueprint;
				}
			}
		}
		if (text == null && PopulationManager.TableExists(text3))
		{
			PopulationResult populationResult2 = PopulationManager.RollOneFrom(text3);
			if (populationResult2 != null)
			{
				text = populationResult2.Blueprint;
			}
		}
		if (text == null && PopulationManager.TableExists(text2))
		{
			PopulationResult populationResult3 = PopulationManager.RollOneFrom(text2);
			if (populationResult3 != null)
			{
				text = populationResult3.Blueprint;
			}
		}
		bool flag2 = false;
		if (text == null)
		{
			int num = Tier;
			List<string> list = new List<string>(8);
			string value3 = "BaseTier" + Type + num;
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (blueprint.Name.StartsWith(value3))
				{
					list.Add(blueprint.Name);
				}
			}
			if (list.Count > 0)
			{
				text = list.GetRandomElement();
				flag2 = true;
			}
		}
		if (text == null)
		{
			MetricsManager.LogError("Unknown relic type: " + Type + ", tier: " + Tier);
			Type = null;
			return GenerateBaseRelic(ref Type, Tier, AllowAlternate);
		}
		GameObject gameObject = GameObject.CreateUnmodified(text);
		if (!flag2)
		{
			gameObject.SetStringProperty("Mods", "None");
		}
		gameObject.SetImportant(flag: true);
		return gameObject;
	}

	public static string GetType(GameObject Object)
	{
		string inventoryCategory = Object.GetInventoryCategory();
		switch (inventoryCategory)
		{
		case "Cybernetic Implants":
			return "Implant";
		case "Food":
			return "Food";
		case "Books":
			return "Book";
		default:
		{
			Armor part = Object.GetPart<Armor>();
			if (part != null)
			{
				string wornOn = part.WornOn;
				switch (wornOn)
				{
				case "Head":
				case "Face":
				case "Arm":
				case "Feet":
				case "Hands":
					return wornOn;
				case "Floating Nearby":
					return "Floating";
				default:
					return "Body";
				}
			}
			if (inventoryCategory == "Shields")
			{
				return "Shield";
			}
			MissileWeapon part2 = Object.GetPart<MissileWeapon>();
			if (part2 != null)
			{
				switch (part2.Skill)
				{
				case "Rifle":
				case "Bow":
					return "Rifle";
				case "Pistol":
					return "Pistol";
				}
			}
			MeleeWeapon part3 = Object.GetPart<MeleeWeapon>();
			if (part3 != null && !part3.IsImprovisedWeapon())
			{
				switch (part3.Skill)
				{
				case "Axe":
					return "Axe";
				case "ShortBlades":
					return "ShortBlade";
				case "LongBlades":
					return "LongBlade";
				case "Cudgel":
					return "Cudgel";
				}
			}
			if (!60.in100())
			{
				return "Curio";
			}
			return "Artifact";
		}
		}
	}

	public static string GetSubtype(string type)
	{
		switch (type)
		{
		case "Axe":
		case "ShortBlade":
		case "LongBlade":
		case "Cudgel":
			return "weapon";
		case "Head":
		case "Body":
		case "Face":
		case "Arm":
		case "Feet":
		case "Hands":
		case "Floating":
			return "armor";
		case "Rifle":
		case "Pistol":
			return "ranged";
		case "Book":
			return "book";
		case "Food":
			return "food";
		case "Artifact":
		case "Curio":
			return "curio";
		case "Shield":
			return "shield";
		case "Implant":
			return "implant";
		default:
			return "curio";
		}
	}

	public static string SelectElement(GameObject Item, GameObject Owner = null, GameObject Killed = null, GameObject InfluencedBy = null)
	{
		GetItemElementsEvent E = PooledEvent<GetItemElementsEvent>.FromPool();
		E.HandleFor(Owner);
		E.HandleFor(Item);
		E.HandleFor(Killed);
		E.HandleFor(InfluencedBy);
		Dictionary<string, int> weights = E.Weights;
		string text = HistoricStringExpander.ExpandString("<spice.elements.!random>") ?? "might";
		if (weights == null)
		{
			return text;
		}
		if (weights.ContainsKey(text))
		{
			weights[text]++;
		}
		else
		{
			weights.Add(text, 1);
		}
		string randomElement = weights.GetRandomElement();
		PooledEvent<GetItemElementsEvent>.ResetTo(ref E);
		return randomElement;
	}

	private static void AddAttributeBoost(GameObject Object, string Attribute, int Tier, ref bool result)
	{
		EquipStatBoost.AppendBoostOnEquip(Object, Attribute + ":" + GetAttributeBonusFromRelicTier(Tier), Attribute + "Boost", techScan: true);
		result = true;
	}

	private static void AddResistanceBoost(GameObject Object, string Resist, int Tier, ref bool result)
	{
		EquipStatBoost.AppendBoostOnEquip(Object, Resist + ":" + GetResistanceBonusFromRelicTier(Tier), Resist + "System", techScan: true);
		result = true;
	}

	private static void AddMoveSpeedBoost(GameObject Object, int Tier, ref bool result)
	{
		EquipStatBoost.AppendBoostOnEquip(Object, "MoveSpeed:" + GetMoveSpeedBonusFromRelicTier(Tier), "KineticDriver", techScan: true);
		result = true;
	}

	private static void SyncImplantPart(GameObject Object, IPart Part)
	{
		GameObject implantee = Object.Implantee;
		if (implantee != null)
		{
			ImplantedEvent.Send(implantee, Object, Part, implantee.FindCybernetics(Object));
		}
	}

	private static bool AddImplantAttributeBoost(GameObject Object, string Attribute, int Tier, bool Minor = false)
	{
		if (Object.HasPart<CyberneticsStatModifier>())
		{
			return false;
		}
		CyberneticsStatModifier cyberneticsStatModifier = new CyberneticsStatModifier();
		cyberneticsStatModifier.AddOn = true;
		cyberneticsStatModifier.Stats = Attribute + ":" + (Minor ? 1 : GetAttributeBonusFromRelicTier(Tier));
		Object.AddPart(cyberneticsStatModifier);
		SyncImplantPart(Object, cyberneticsStatModifier);
		return true;
	}

	private static bool AddImplantResistanceBoost(GameObject Object, string Resistance, int Tier, bool Minor = false)
	{
		if (Object.HasPart<CyberneticsStatModifier>())
		{
			return false;
		}
		CyberneticsStatModifier cyberneticsStatModifier = new CyberneticsStatModifier();
		cyberneticsStatModifier.AddOn = true;
		cyberneticsStatModifier.Stats = Resistance + ":" + (Minor ? Stat.Random(4, 6) : GetResistanceBonusFromRelicTier(Tier));
		Object.AddPart(cyberneticsStatModifier);
		SyncImplantPart(Object, cyberneticsStatModifier);
		return true;
	}

	private static bool AddImplantMoveSpeedBoost(GameObject Object, int Tier, bool Minor = false)
	{
		if (Object.HasPart<CyberneticsStatModifier>())
		{
			return false;
		}
		CyberneticsStatModifier cyberneticsStatModifier = new CyberneticsStatModifier();
		cyberneticsStatModifier.AddOn = true;
		cyberneticsStatModifier.Stats = "MoveSpeed:" + (Minor ? (-Stat.Random(2, 4)) : GetMoveSpeedBonusFromRelicTier(Tier));
		Object.AddPart(cyberneticsStatModifier);
		SyncImplantPart(Object, cyberneticsStatModifier);
		return true;
	}

	public static bool ApplyBasicBestowal(GameObject Object, string Type = null, int Tier = 1, string Subtype = null, bool Standard = false, bool ShowInShortDescription = false)
	{
		if (Type == null)
		{
			Type = GetType(Object);
		}
		if (Subtype == null)
		{
			Subtype = GetSubtype(Type);
		}
		bool flag = false;
		switch (Subtype)
		{
		case "ranged":
			flag = GiveRandomMissileWeaponBestowal(Object, Tier, Standard, ShowInShortDescription);
			break;
		case "weapon":
			flag = GiveRandomMeleeWeaponBestowal(Object, Tier, Standard, ShowInShortDescription);
			break;
		case "armor":
			flag = GiveRandomArmorBestowal(Object, Tier, Standard, ShowInShortDescription);
			break;
		case "shield":
			flag = GiveRandomShieldBestowal(Object, Tier, Standard, ShowInShortDescription);
			break;
		case "implant":
			flag = GiveRandomImplantBestowal(Object, Tier, Standard, ShowInShortDescription);
			break;
		case "book":
			flag = Object.RequirePart<TrainingBook>().AssignRandomTraining();
			break;
		}
		if (flag && Object.HasStat("Hitpoints"))
		{
			Object.GetStat("Hitpoints").BaseValue += 100;
		}
		AfterBasicBestowalEvent.Send(Object, Type, Subtype, Tier, Standard);
		return flag;
	}

	public static bool ApplyElementBestowal(GameObject Object, string Element, string Type, int Tier = 1, string Subtype = null)
	{
		bool result = false;
		if (Subtype == null)
		{
			Subtype = GetSubtype(Type);
		}
		switch (Subtype)
		{
		case "ranged":
			switch (Element)
			{
			case "glass":
				if (!Object.HasPart<ModImprovedClairvoyance>())
				{
					Object.AddPart(new ModImprovedClairvoyance(Tier));
					result = true;
				}
				break;
			case "jewels":
				AddAttributeBoost(Object, "Ego", Tier, ref result);
				break;
			case "stars":
				if (!Object.HasPart<ModImprovedLightManipulation>())
				{
					Object.AddPart(new ModImprovedLightManipulation(Tier));
					result = true;
				}
				break;
			case "time":
				if (!Object.HasPart<ModImprovedTemporalFugue>())
				{
					Object.AddPart(new ModImprovedTemporalFugue(Tier));
					result = true;
				}
				break;
			case "salt":
				AddAttributeBoost(Object, "Willpower", Tier, ref result);
				break;
			case "ice":
				AddResistanceBoost(Object, "ColdResistance", Tier, ref result);
				break;
			case "scholarship":
				AddAttributeBoost(Object, "Intelligence", Tier, ref result);
				break;
			case "might":
				if (MutationFactory.HasMutation("Telekinesis") && 20.in100() && !Object.HasPart<ModImprovedTelekinesis>())
				{
					Object.AddPart(new ModImprovedTelekinesis(Tier));
					result = true;
				}
				else
				{
					AddAttributeBoost(Object, "Strength", Tier, ref result);
				}
				break;
			case "travel":
				if (50.in100() && !Object.HasPart<ModImprovedTeleportation>())
				{
					Object.AddPart(new ModImprovedTeleportation(Tier));
					result = true;
				}
				else
				{
					AddMoveSpeedBoost(Object, Tier, ref result);
				}
				break;
			case "chance":
				AddAttributeBoost(Object, "Agility", Tier, ref result);
				break;
			case "circuitry":
				if (!Object.HasPart<ModImprovedElectricalGeneration>())
				{
					Object.AddPart(new ModImprovedElectricalGeneration(Tier));
					result = true;
				}
				break;
			}
			break;
		case "weapon":
			switch (Element)
			{
			case "glass":
				if (!Object.HasPart<ModGlazed>())
				{
					Object.AddPart(new ModGlazed(Tier));
					result = true;
				}
				break;
			case "jewels":
				if (50.in100() && !Object.HasPart<ModTransmuteOnHit>())
				{
					Object.AddPart(new ModTransmuteOnHit(Tier * 4, "Gemstones"));
					result = true;
				}
				else
				{
					AddAttributeBoost(Object, "Ego", Tier, ref result);
				}
				break;
			case "stars":
				if (!Object.HasPart<ModImprovedLightManipulation>())
				{
					Object.AddPart(new ModImprovedLightManipulation(Tier));
					result = true;
				}
				break;
			case "time":
				if (!Object.HasPart<ModImprovedTemporalFugue>())
				{
					Object.AddPart(new ModImprovedTemporalFugue(Tier));
					result = true;
				}
				break;
			case "salt":
				AddAttributeBoost(Object, "Willpower", Tier, ref result);
				break;
			case "ice":
				if (!Object.HasPart<ModRelicFreezing>())
				{
					Object.AddPart(new ModRelicFreezing(Tier));
					result = true;
				}
				break;
			case "scholarship":
				if (!Object.HasPart<ModBeetlehost>())
				{
					Object.AddPart(new ModBeetlehost(Tier));
					result = true;
				}
				break;
			case "might":
				if (MutationFactory.HasMutation("Telekinesis") && 20.in100() && !Object.HasPart<ModImprovedTelekinesis>())
				{
					Object.AddPart(new ModImprovedTelekinesis(Tier));
					result = true;
				}
				else
				{
					AddAttributeBoost(Object, "Strength", Tier, ref result);
				}
				break;
			case "travel":
				if (!Object.HasPart<ModImprovedTeleportation>())
				{
					Object.AddPart(new ModImprovedTeleportation(Tier));
					result = true;
				}
				break;
			case "chance":
				if (!Object.HasPart<ModFatecaller>())
				{
					Object.AddPart(new ModFatecaller(Tier));
					result = true;
				}
				break;
			case "circuitry":
				if (!Object.HasPart<ModImprovedElectricalGeneration>())
				{
					Object.AddPart(new ModImprovedElectricalGeneration(Tier));
					result = true;
				}
				break;
			}
			break;
		case "armor":
			switch (Element)
			{
			case "glass":
				if (50.in100() && !Object.HasPart<ModGlassArmor>())
				{
					Object.AddPart(new ModGlassArmor(Tier));
					result = true;
				}
				else if (!Object.HasPart<ModImprovedClairvoyance>())
				{
					Object.AddPart(new ModImprovedClairvoyance(Tier));
					result = true;
				}
				break;
			case "jewels":
				Object.GetPart<Armor>().Ego += GetAttributeBonusFromRelicTier(Tier);
				result = true;
				break;
			case "stars":
				if (!Object.HasPart<ModImprovedLightManipulation>())
				{
					Object.AddPart(new ModImprovedLightManipulation(Tier));
					result = true;
				}
				break;
			case "time":
				if (!Object.HasPart<ModImprovedTemporalFugue>())
				{
					Object.AddPart(new ModImprovedTemporalFugue(Tier));
					result = true;
				}
				break;
			case "salt":
				Object.GetPart<Armor>().Willpower += GetAttributeBonusFromRelicTier(Tier);
				result = true;
				break;
			case "ice":
				Object.GetPart<Armor>().Cold += GetResistanceBonusFromRelicTier(Tier);
				result = true;
				break;
			case "scholarship":
				Object.GetPart<Armor>().Intelligence += GetAttributeBonusFromRelicTier(Tier);
				result = true;
				break;
			case "might":
				if (MutationFactory.HasMutation("Telekinesis") && 20.in100() && !Object.HasPart<ModImprovedTelekinesis>())
				{
					Object.AddPart(new ModImprovedTelekinesis(Tier));
					result = true;
				}
				else
				{
					Object.GetPart<Armor>().Strength += GetAttributeBonusFromRelicTier(Tier);
					result = true;
				}
				break;
			case "travel":
				if (50.in100() && !Object.HasPart<ModImprovedTeleportation>())
				{
					Object.AddPart(new ModImprovedTeleportation(Tier));
					result = true;
				}
				else
				{
					AddMoveSpeedBoost(Object, Tier, ref result);
				}
				break;
			case "chance":
				if (!Object.HasPart<ModBlinkEscape>())
				{
					Object.AddPart(new ModBlinkEscape(Tier));
					result = true;
				}
				break;
			case "circuitry":
				if (!Object.HasPart<ModImprovedElectricalGeneration>())
				{
					Object.AddPart(new ModImprovedElectricalGeneration(Tier));
					result = true;
				}
				break;
			}
			break;
		case "shield":
			switch (Element)
			{
			case "glass":
				if (50.in100() && !Object.HasPart<ModGlassArmor>())
				{
					Object.AddPart(new ModGlassArmor(Tier));
					result = true;
				}
				else if (!Object.HasPart<ModImprovedClairvoyance>())
				{
					Object.AddPart(new ModImprovedClairvoyance(Tier));
					result = true;
				}
				break;
			case "jewels":
				AddAttributeBoost(Object, "Ego", Tier, ref result);
				break;
			case "stars":
				if (!Object.HasPart<ModImprovedLightManipulation>())
				{
					Object.AddPart(new ModImprovedLightManipulation(Tier));
					result = true;
				}
				break;
			case "time":
				if (!Object.HasPart<ModImprovedTemporalFugue>())
				{
					Object.AddPart(new ModImprovedTemporalFugue(Tier));
					result = true;
				}
				break;
			case "salt":
				AddAttributeBoost(Object, "Willpower", Tier, ref result);
				break;
			case "ice":
				AddResistanceBoost(Object, "ColdResistance", Tier, ref result);
				break;
			case "scholarship":
				AddAttributeBoost(Object, "Intelligence", Tier, ref result);
				break;
			case "might":
				if (MutationFactory.HasMutation("Telekinesis") && 20.in100() && !Object.HasPart<ModImprovedTelekinesis>())
				{
					Object.AddPart(new ModImprovedTelekinesis(Tier));
					result = true;
				}
				else
				{
					AddAttributeBoost(Object, "Strength", Tier, ref result);
				}
				break;
			case "chance":
				if (!Object.HasPart<ModBlinkEscape>())
				{
					Object.AddPart(new ModBlinkEscape(Tier));
					result = true;
				}
				break;
			case "circuitry":
				if (!Object.HasPart<ModImprovedElectricalGeneration>())
				{
					Object.AddPart(new ModImprovedElectricalGeneration(Tier));
					result = true;
				}
				break;
			case "travel":
				if (50.in100() && !Object.HasPart<ModImprovedTeleportation>())
				{
					Object.AddPart(new ModImprovedTeleportation(Tier));
					result = true;
				}
				else
				{
					AddMoveSpeedBoost(Object, Tier, ref result);
				}
				break;
			}
			break;
		case "implant":
			switch (Element)
			{
			case "glass":
				if (!Object.HasPart<ModImprovedClairvoyance>())
				{
					SyncImplantPart(Object, Object.AddPart(new ModImprovedClairvoyance(Tier)));
					result = true;
				}
				break;
			case "jewels":
				if (AddImplantAttributeBoost(Object, "Ego", Tier))
				{
					result = true;
				}
				break;
			case "stars":
				if (!Object.HasPart<ModImprovedLightManipulation>())
				{
					SyncImplantPart(Object, Object.AddPart(new ModImprovedLightManipulation(Tier)));
					result = true;
				}
				break;
			case "time":
				if (!Object.HasPart<ModImprovedTemporalFugue>())
				{
					SyncImplantPart(Object, Object.AddPart(new ModImprovedTemporalFugue(Tier)));
					result = true;
				}
				break;
			case "salt":
				if (AddImplantAttributeBoost(Object, "Willpower", Tier))
				{
					result = true;
				}
				break;
			case "ice":
				if (AddImplantResistanceBoost(Object, "ColdResistance", Tier))
				{
					result = true;
				}
				break;
			case "scholarship":
				if (AddImplantAttributeBoost(Object, "Intelligence", Tier))
				{
					result = true;
				}
				break;
			case "might":
				if (MutationFactory.HasMutation("Telekinesis") && 20.in100() && !Object.HasPart<ModImprovedTelekinesis>())
				{
					SyncImplantPart(Object, Object.AddPart(new ModImprovedTelekinesis(Tier)));
					result = true;
				}
				else if (AddImplantAttributeBoost(Object, "Strength", Tier))
				{
					result = true;
				}
				break;
			case "travel":
				if (AddImplantMoveSpeedBoost(Object, Tier))
				{
					result = true;
				}
				break;
			case "chance":
				if (AddImplantAttributeBoost(Object, "Agility", Tier))
				{
					result = true;
				}
				break;
			case "circuitry":
				if (!Object.HasPart<ModImprovedElectricalGeneration>())
				{
					SyncImplantPart(Object, Object.AddPart(new ModImprovedElectricalGeneration(Tier)));
					result = true;
				}
				break;
			}
			break;
		}
		if (Element == "travel")
		{
			switch (Subtype)
			{
			case "armor":
			case "shield":
			case "implant":
				if (!Object.HasPart<CarryBonus>())
				{
					CarryBonus carryBonus = new CarryBonus(Stat.Random(0, 4) * 5 + (Tier - 1) * 10 + 20);
					if (Subtype == "implant")
					{
						carryBonus.WorksOn(AdjacentCellContents: false, Carrier: false, CellContents: false, Enclosed: false, Equipper: false, Holder: false, Implantee: true);
						carryBonus.IsTechScannable = true;
						carryBonus.IsEMPSensitive = true;
						carryBonus.NameForStatus = "LiftAmplifier";
					}
					Object.AddPart(carryBonus);
					if (Subtype == "implant")
					{
						SyncImplantPart(Object, carryBonus);
					}
					result = true;
				}
				break;
			}
		}
		if (result && Object.HasStat("Hitpoints"))
		{
			Object.GetStat("Hitpoints").BaseValue += 200;
		}
		AfterElementBestowalEvent.Send(Object, Element, Type, Subtype, Tier);
		return result;
	}

	public static GameObject GenerateRelic(string Type, int Tier, HistoricEntitySnapshot SnapRegion = null, List<string> Adjectives = null, Dictionary<string, List<string>> ListProperties = null, string Name = null, string Article = null, string LikedFactionDescriptionAddendum = null)
	{
		History sultanHistory = The.Game.sultanHistory;
		GameObject gameObject = GenerateBaseRelic(ref Type, Tier);
		gameObject.Render.ColorString = "&M";
		gameObject.Render.TileColor = "&M";
		Description part = gameObject.GetPart<Description>();
		int targetPeriod = GetPeriodFromRelicTier(Tier);
		HistoricEntityList entitiesByDelegate = sultanHistory.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("type").Equals("sultan") && int.Parse(entity.GetSnapshotAtYear(entity.lastYear).GetProperty("period")) == targetPeriod);
		if (entitiesByDelegate.Count > 0)
		{
			string property = entitiesByDelegate.GetRandomElement().GetCurrentSnapshot().GetProperty("name");
			part._Short = part._Short.Replace("*sultan*", property);
		}
		else
		{
			MetricsManager.LogError("Could not find a sultan for period " + targetPeriod + " from tier " + Tier);
			part._Short = part._Short.Replace("*sultan*", "the sultan");
		}
		List<string> list = new List<string>();
		for (int num = 0; num < Adjectives.Count; num++)
		{
			string text = TranslateAdjective(Adjectives[num]);
			if (!text.IsNullOrEmpty() && !list.Contains(text))
			{
				list.Add(text);
			}
		}
		if (list.Count == 0)
		{
			list.Add(HistoricStringExpander.ExpandString("<spice.elements.!random>", null, sultanHistory));
		}
		string subtype = GetSubtype(Type);
		ApplyBasicBestowal(gameObject, Type, Tier, subtype, Standard: true);
		List<string> list2 = null;
		foreach (string item in list)
		{
			ApplyElementBestowal(gameObject, item, Type, Tier, subtype);
			if (item != "none")
			{
				if (list2 == null)
				{
					list2 = new List<string>();
				}
				if (!list2.Contains(item))
				{
					list2.Add(item);
				}
				string newValue = HistoricStringExpander.ExpandString("<spice.elements." + item + ".nounsPlural.!random>", null, sultanHistory);
				Description description = part;
				description._Short = description._Short + " " + gameObject.Itis + " " + HistoricStringExpander.ExpandString("<spice.instancesOf.stamped_VAR.!random>", null, sultanHistory).Replace("*var*", newValue) + ".";
			}
		}
		if (!list2.IsNullOrEmpty())
		{
			ItemElements itemElements = new ItemElements();
			StringBuilder sB = Event.NewStringBuilder();
			foreach (string item2 in list2)
			{
				sB.Compound(item2, ";;").Append("::").Append(4 + Tier * 2);
			}
			itemElements._Elements = Event.FinalizeString(sB);
			gameObject.AddPart(itemElements);
		}
		if (ListProperties != null)
		{
			foreach (KeyValuePair<string, List<string>> ListProperty in ListProperties)
			{
				if (ListProperty.Key == "likedFactions")
				{
					foreach (string item3 in ListProperty.Value)
					{
						AddsRep.AddModifier(gameObject, item3 + ":200");
						Description description = part;
						description._Short = description._Short + " There's an engraving of " + Faction.GetFormattedName(item3) + " being " + HistoricStringExpander.ExpandString("<spice.instancesOf.venerated.!random>", null, sultanHistory);
						if (!LikedFactionDescriptionAddendum.IsNullOrEmpty())
						{
							part._Short += LikedFactionDescriptionAddendum;
						}
						part._Short += ".";
					}
				}
				else if (ListProperty.Key == "lovedFactions")
				{
					foreach (string item4 in ListProperty.Value)
					{
						AddsRep.AddModifier(gameObject, item4 + ":400");
						Description description = part;
						description._Short = description._Short + " There's an engraving of " + Faction.GetFormattedName(item4) + " being " + HistoricStringExpander.ExpandString("<spice.instancesOf.venerated.!random>", null, sultanHistory) + ".";
					}
				}
				else
				{
					if (!(ListProperty.Key == "hatedFactions"))
					{
						continue;
					}
					foreach (string item5 in ListProperty.Value)
					{
						AddsRep.AddModifier(gameObject, item5 + ":-200");
						if (!gameObject.HasPart<ModFactionSlayer>())
						{
							gameObject.AddPart(new ModFactionSlayer(Tier, item5));
							Description description = part;
							description._Short = description._Short + " There's an engraving of " + Faction.GetFormattedName(item5) + " being " + HistoricStringExpander.ExpandString("<spice.instancesOf.disparaged.!random>", null, sultanHistory) + ".";
						}
					}
				}
			}
		}
		if (subtype == "curio")
		{
			switch (Stat.Random(1, 2))
			{
			case 1:
			{
				string populationName = "Curio_SummoningRelic" + Tier;
				int num2 = 0;
				string text2 = null;
				string text3 = null;
				Faction faction = null;
				do
				{
					text2 = PopulationManager.RollOneFrom(populationName).Blueprint;
					text3 = GameObjectFactory.Factory.GetBlueprint(text2).GetPrimaryFaction();
					faction = ((text3 == null) ? null : Factions.Get(text3));
				}
				while ((faction == null || !faction.Old) && ++num2 < 1000);
				gameObject.RequirePart<SummoningCurio>().Creature = text2;
				break;
			}
			case 2:
				gameObject.RequirePart<GenocideCurio>();
				break;
			}
		}
		if (Type == "Book")
		{
			gameObject.GetPart<Commerce>().Value = 200 + 100 * Tier;
		}
		else
		{
			gameObject.GetPart<Commerce>().Value = 500 + 150 * Tier;
		}
		string randomElement = list.GetRandomElement();
		if (Name == null)
		{
			Name = GenerateRelicName(Type, SnapRegion, randomElement, out Article);
		}
		gameObject.GiveProperName(Name, Force: true);
		if (!Article.IsNullOrEmpty())
		{
			gameObject.SetStringProperty("IndefiniteArticle", Article);
			gameObject.SetStringProperty("DefiniteArticle", Article);
			gameObject.SetStringProperty("RelicName", Name);
		}
		else
		{
			gameObject.SetStringProperty("RelicName", Name);
		}
		gameObject.RequirePart<DisplayNameColor>().SetColor("M");
		if (subtype == "book" && gameObject.TryGetPart<MarkovBook>(out var Part))
		{
			Part.Title = Name;
		}
		gameObject.AddPart(new TakenWXU(2));
		gameObject.SetStringProperty("EquipLayerSound", "sfx_equip_armor_addLayer_relic");
		AfterRelicGeneratedEvent.Send(gameObject, list, randomElement, Type, subtype, Tier);
		string text4 = (string.IsNullOrEmpty(gameObject.Render.DetailColor) ? "y" : gameObject.Render.DetailColor);
		gameObject.SetStringProperty("EquipmentFrameColors", text4 + "M" + text4 + "M");
		return gameObject;
	}
}
