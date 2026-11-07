using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.World;
using XRL.World.Capabilities;

namespace XRL.Annals;

[Serializable]
public class PopulationInflux : HistoricEvent
{
	public static readonly int MAYOR_WEIGHT = 10;

	public static readonly int MERCHANT_WEIGHT = 10;

	public static readonly int WARDEN_WEIGHT = 10;

	public static readonly int TINKER_WEIGHT = 10;

	public static readonly int APOTHECARY_WEIGHT = 10;

	public static readonly int VILLAGER_WEIGHT = 70;

	public static readonly int CHANCE_MULTIPLE_PETS = 30;

	public bool bVillageZero;

	public PopulationInflux()
	{
		bVillageZero = false;
	}

	public PopulationInflux(bool VillageZero)
	{
		bVillageZero = VillageZero;
	}

	public static BallBag<string> GetRoleBag()
	{
		return new BallBag<string>
		{
			{ "mayor", MAYOR_WEIGHT },
			{ "warden", WARDEN_WEIGHT },
			{ "merchant", MERCHANT_WEIGHT },
			{ "tinker", TINKER_WEIGHT },
			{ "apothecary", APOTHECARY_WEIGHT },
			{ "villager", VILLAGER_WEIGHT }
		};
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		int num = Random(1, 1000);
		if (num <= 400)
		{
			string text = GetRoleBag().PeekOne();
			string blueprint;
			do
			{
				blueprint = PopulationManager.RollOneFrom("LairOwners_" + snapshotAtYear.GetProperty("region")).Blueprint;
			}
			while (!EncountersAPI.IsLegendaryEligible(GameObjectFactory.Factory.Blueprints[blueprint]));
			GameObject gameObject;
			switch (text)
			{
			case "mayor":
			{
				string text2 = snapshotAtYear.GetProperty("mayorTemplate");
				if (text2 == "unknown")
				{
					text2 = "Mayor";
				}
				gameObject = HeroMaker.MakeHero(GameObject.Create(blueprint), "SpecialVillagerHeroTemplate_" + text2, -1, text2);
				break;
			}
			case "warden":
				gameObject = HeroMaker.MakeHero(GameObject.Create(blueprint), "SpecialVillagerHeroTemplate_Warden", -1, "Warden");
				break;
			case "merchant":
				gameObject = HeroMaker.MakeHero(GameObject.Create(blueprint), "SpecialVillagerHeroTemplate_Merchant", -1, "Merchant");
				break;
			case "tinker":
				gameObject = HeroMaker.MakeHero(GameObject.Create(blueprint), "SpecialVillagerHeroTemplate_Tinker", -1, "Tinker");
				SetEntityProperty("techTier", Tier.Constrain(snapshotAtYear.TechTier + 1).ToString());
				break;
			case "apothecary":
				gameObject = HeroMaker.MakeHero(GameObject.Create(blueprint), "SpecialVillagerHeroTemplate_Apothecary", -1, "Apothecary");
				break;
			default:
				gameObject = HeroMaker.MakeHero(GameObject.Create(blueprint));
				break;
			}
			Dictionary<string, string> vars = QudHistoryHelpers.BuildContextFromObjectTextFragments(blueprint);
			string displayName = gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: false, BaseOnly: true);
			string displayName2 = gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: false, BaseOnly: true);
			try
			{
				string value = ExpandString("<spice.villages." + text + ".reasonIBecame.!random>", vars);
				AddEntityListItem("immigrant_name", displayName, Force: true);
				AddEntityListItem("immigrant_gender", gameObject.GetGender().Name);
				AddEntityListItem("immigrant_role", text, Force: true);
				AddEntityListItem("immigrant_dialogWhy_Q", (If.OneIn(3) ? "Stranger" : gameObject.FormalAddressTerm) + ", why did you come to this village?", Force: true);
				AddEntityListItem("immigrant_dialogWhy_A", value, Force: true);
				AddEntityListItem("immigrant_type", blueprint, Force: true);
				AddEntityListItem("sacredThings", displayName);
				AddEntityListItem("profaneThings", ExpandString("those who would <spice.commonPhrases.harm.!random> ") + displayName);
				AddEntityListItem("itemAdjectiveRoots", displayName2);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Failed to set up immigrant.", x);
			}
			string text3;
			switch (text)
			{
			case "mayor":
			case "warden":
				text3 = "The villagers of " + snapshotAtYear.GetProperty("name") + " asked " + displayName2 + " to " + ExpandString("<spice.villages." + text + ".villageTask.!random>").Replace("my", gameObject.its);
				break;
			case "villager":
				text3 = displayName2 + " settled down among the villagers";
				break;
			default:
				text3 = displayName2 + " set up a shop for their trade";
				break;
			}
			int num2 = int.Parse(history.GetEntitiesWithProperty("Resheph").GetRandomElement().GetCurrentSnapshot()
				.GetProperty("flipYear"));
			string value2 = Grammar.InitCap(string.Format("{0}, {1} grew tired of {2} and {3} to a place {4}. There {7} came upon {5} and its inhabitants. {6}.|{8}", ExpandString("<spice.instancesOf.openingTime.!random>").Replace("*year*", "%" + (num2 + Random(900, 999)) + "%"), gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: false, BaseOnly: true), gameObject.GetxTag_CommaDelimited("TextFragments", "Activity"), ExpandString("<spice.commonPhrases.trekked.!random>"), ExpandString("<spice.history.regions.terrain." + snapshotAtYear.GetProperty("region") + ".over.!random>"), snapshotAtYear.GetProperty("name"), text3, gameObject.it, id));
			AddEntityListItem("Gospels", value2);
		}
		else if (num <= 800)
		{
			string petType = "nonHumanoid";
			int useTier = Math.Max(snapshotAtYear.Tier, 1);
			string text4 = ((!bVillageZero) ? GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint bp) => bp.HasTag("Creature") && bp.Tier <= useTier && !bp.HasTag("BaseObject") && !bp.HasTag("ExcludeFromDynamicEncounters") && !bp.HasTag("ExcludeFromVillagePopulations") && !bp.HasTag("Merchant") && !bp.HasProperName() && ((!(petType == "humanoid")) ? (!bp.HasTag("Humanoid")) : bp.HasTag("Humanoid"))).GetRandomElement().Name : GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint bp) => bp.HasTag("Creature") && bp.Tier <= useTier && !bp.HasTag("BaseObject") && !bp.HasTag("ExcludeFromDynamicEncounters") && !bp.HasTag("ExcludeFromVillageZero") && !bp.HasTag("ExcludeFromVillagePopulations") && !bp.HasTag("Merchant") && !bp.HasPart("Breeder") && !bp.HasProperName() && ((!(petType == "humanoid")) ? (!bp.HasTag("Humanoid")) : bp.HasTag("Humanoid"))).GetRandomElement().Name);
			GameObject gameObject2 = GameObject.Create(text4);
			string value3;
			int num3;
			if (If.Chance(CHANCE_MULTIPLE_PETS))
			{
				value3 = $"Why are there {Grammar.Pluralize(gameObject2.DisplayNameOnly)} here?";
				num3 = Random(2, 4);
			}
			else
			{
				value3 = $"Why{gameObject2.Is} there {gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: true)} here?";
				num3 = 1;
			}
			AddEntityListItem("pet_petType", petType, Force: true);
			AddEntityListItem("pet_dialogWhy_Q", value3, Force: true);
			AddEntityListItem("pet_number", num3.ToString(), Force: true);
			AddEntityListItem("pet_petSpecies", text4, Force: true);
		}
		else if (num <= 1000)
		{
			AddEntityListItem("populationMultiplier", Random(2, 4).ToString());
			string value4 = Grammar.InitCap(string.Format("As the gospel of {0} spread, {1} {2} the village of {3}.|{4}", snapshotAtYear.sacredThing, ExpandString("<spice.commonPhrases.folks.!random>"), ExpandString("<spice.instancesOf.flockedTo.!random>"), snapshotAtYear.GetProperty("name"), id));
			AddEntityListItem("Gospels", value4);
		}
		else
		{
			string name = Factions.GetRandomFactionWithAtLeastOneMember().Name;
			string value5 = ExpandString("<spice.villages.immigrants.immigrantPopReason.!random>", QudHistoryHelpers.BuildContextFromObjectTextFragments(GameObjectFactory.Factory.GetFactionMembers(name).GetRandomElement().Name));
			string value6 = (If.CoinFlip() ? "half" : "whole");
			string value7 = "false";
			if (snapshotAtYear.GetList("immigrantPop_amount").Count > 0)
			{
				value7 = "true";
				value6 = "whole";
			}
			AddEntityListItem("immigrantPop_type", name, Force: true);
			AddEntityListItem("immigrantPop_reason", value5, Force: true);
			AddEntityListItem("immigrantPop_dialogWhy_Q", $"Why is this village populated with {Faction.GetFormattedName(name)}?", Force: true);
			AddEntityListItem("immigrantPop_dialogWhy_A", value5, Force: true);
			AddEntityListItem("immigrantPop_amount", value6, Force: true);
			AddEntityListItem("immigrantPop_secondWave", value7, Force: true);
		}
	}
}
