using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.Names;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Annals;

[Serializable]
public class InitializeVillage : HistoricEvent
{
	public static readonly int CHANCE_REGIONAL_NAME;

	public string region;

	[NonSerialized]
	public string BaseFaction;

	[NonSerialized]
	public string BaseFactionMember;

	[NonSerialized]
	private string Culture;

	[NonSerialized]
	private List<string> Palette;

	public bool bVillageZero;

	public InitializeVillage(string Region, string BaseFaction = null, string BaseFactionMember = null, string Culture = null, List<string> Palette = null, bool VillageZero = false)
	{
		region = Region;
		this.BaseFaction = BaseFaction;
		this.BaseFactionMember = BaseFactionMember;
		this.Culture = Culture;
		this.Palette = Palette;
		bVillageZero = VillageZero;
	}

	public override void Generate()
	{
		GameObject gameObject = null;
		SetEntityProperty("type", "village");
		int num = 0;
		string culture = Culture.Coalesce("Qudish");
		string value;
		do
		{
			if (CHANCE_REGIONAL_NAME.in100())
			{
				string text = Grammar.MakeTitleCase(region);
				value = NameMaker.MakeName(null, null, null, null, culture, null, text, null, null, null, null, "Site");
			}
			else
			{
				value = NameMaker.MakeName(null, null, null, null, culture, null, null, null, null, null, null, "Site");
			}
		}
		while (history.GetEntitiesWherePropertyEquals("name", value).Count > 0 && ++num < 1000);
		SetEntityProperty("name", value);
		SetEntityProperty("region", region);
		if (Palette.IsNullOrEmpty())
		{
			AddListProperty("palette", Crayons.GetRandomDistinctColorsAll(3));
		}
		else
		{
			AddListProperty("palette", Palette);
		}
		string text2;
		if (bVillageZero)
		{
			SetEntityProperty("tier", "0");
			SetEntityProperty("techTier", "0");
			text2 = GameObject.Create(PopulationManager.RollOneFrom("VillageOneBaseFaction_" + region).Blueprint).GetPrimaryFaction();
			SetEntityProperty("villageZero", "true");
		}
		else
		{
			GameObjectBlueprint gameObjectBlueprint = (GameObjectFactory.Factory.Blueprints.ContainsKey("Terrain" + region) ? GameObjectFactory.Factory.Blueprints["Terrain" + region] : null);
			if (gameObjectBlueprint != null)
			{
				SetEntityProperty("tier", gameObjectBlueprint.GetTag("RegionTier", "1"));
			}
			else
			{
				SetEntityProperty("tier", "1");
			}
			gameObjectBlueprint = (GameObjectFactory.Factory.Blueprints.ContainsKey("Terrain" + region) ? GameObjectFactory.Factory.Blueprints["Terrain" + region] : null);
			if (gameObjectBlueprint != null)
			{
				SetEntityProperty("techTier", Random(1, int.Parse(gameObjectBlueprint.GetTag("RegionTier", "1")) + 1).ToString());
			}
			else
			{
				SetEntityProperty("techTier", "1");
			}
			int num2 = 0;
			if (BaseFaction.IsNullOrEmpty())
			{
				do
				{
					PopulationResult populationResult = PopulationManager.RollOneFrom("LairOwners_" + region);
					gameObject = ((populationResult != null) ? GameObject.Create(populationResult.Blueprint) : null);
					text2 = gameObject?.Brain?.GetPrimaryFaction();
				}
				while (text2 == null && ++num2 < 100);
				if (text2 == null)
				{
					text2 = "Prey";
				}
			}
			else
			{
				text2 = BaseFaction;
			}
		}
		GameObjectBlueprint gameObjectBlueprint2;
		if (gameObject != null)
		{
			gameObjectBlueprint2 = gameObject.GetBlueprint();
		}
		else
		{
			gameObjectBlueprint2 = (BaseFactionMember.IsNullOrEmpty() ? Faction.GetMembers(text2).GetRandomElement() : GameObjectFactory.Factory.GetBlueprint(BaseFactionMember));
		}
		SetEntityProperty("villagerPopulation", gameObjectBlueprint2.HasTag("Humanoid") ? "humanoid" : "nonHumanoid");
		SetEntityProperty("baseFaction", text2);
		SetEntityProperty("baseFactionMember", gameObjectBlueprint2.Name);
		Dictionary<string, string> vars = QudHistoryHelpers.BuildContextFromObjectTextFragments(gameObjectBlueprint2.Name);
		string text3 = ExpandString("<spice.villages.reasonForFounding.!random>", vars);
		SetEntityProperty("reasonForFounding", text3);
		SetEntityProperty("defaultSacredThing", ExpandString("<spice.villages.reasonForFounding." + text3 + ".sacredThing.!random>", vars));
		SetEntityProperty("defaultProfaneThing", ExpandString("<spice.villages.reasonForFounding." + text3 + ".profaneThing.!random>", vars));
		SetEntityProperty("governor", "the mayor");
		duration = 0L;
	}
}
