using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using HistoryKit;
using XRL.Language;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Skills.Cooking;

namespace XRL.Annals;

[Serializable]
public class ImportedFoodorDrink : HistoricEvent
{
	public bool bVillageZero;

	public ImportedFoodorDrink()
	{
		bVillageZero = false;
	}

	public ImportedFoodorDrink(bool VillageZero)
	{
		bVillageZero = VillageZero;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		int num = Random(1, 1000);
		int num2 = 0;
		if (num <= 334)
		{
			List<string> list = new List<string>();
			string text = "";
			string liquidTierGroup = getLiquidTierGroup(int.Parse(snapshotAtYear.GetProperty("tier")));
			int num3 = Random(1, 100);
			int num4 = ((num3 <= 4) ? 3 : ((num3 > 52) ? 1 : 2));
			for (int i = 1; i <= num4; i++)
			{
				string text2 = generateUniqueVillageLiquid(liquidTierGroup, list);
				text += text2;
				text = text + "-" + 2000.0 / Math.Pow(2.0, i) + ",";
				list.Add(text2);
			}
			text = text.TrimEnd(',');
			LiquidVolume part = GameObject.Create("Waterskin").GetPart<LiquidVolume>();
			part.ComponentLiquids.Clear();
			part.InitialLiquid = text;
			string text3 = ColorUtility.StripFormatting(part.GetLiquidName());
			AddEntityListItem("signatureLiquids", part.GetLiquidDesignation());
			string value = ((!If.CoinFlip()) ? string.Format("No {0} shall be {1} in {2} without lifting a {3} of {4} and proposing a toast to {5}!|{6}", ExpandString("<spice.items.blessing.!random>"), ExpandString("<spice.commonPhrases.honored.!random>"), snapshotAtYear.Name, ExpandString("<spice.commonPhrases.mug.!random>"), text3, snapshotAtYear.sacredThing, id) : string.Format("No {0} shall be {1} in {2} without lifting a {3} of {4}!|{5}", ExpandString("<spice.items.blessing.!random>"), ExpandString("<spice.commonPhrases.honored.!random>"), snapshotAtYear.Name, ExpandString("<spice.commonPhrases.mug.!random>"), text3, id));
			AddEntityListItem("Gospels", value);
			if (If.Chance(7))
			{
				SetEntityProperty("newFactionName", generateFactionName(text3));
			}
			return;
		}
		num2 += 334;
		if (num <= num2 + 333)
		{
			string randomMeaningfulWord = Grammar.GetRandomMeaningfulWord(snapshotAtYear.sacredThing.Split(',')[0]);
			string property = snapshotAtYear.GetProperty("baseFaction");
			string text4 = ((!GameObjectFactory.Factory.Blueprints.ContainsKey(property + "_Data")) ? "generic" : GameObjectFactory.Factory.Blueprints[property + "_Data"].GetTag("DishNames", "generic"));
			text4 = text4.Split(',').GetRandomElement();
			string text5 = Grammar.MakeTitleCase(ExpandString("<spice.villages.food.specialDishNames.!random>").Replace("*descriptor*", randomMeaningfulWord).Replace("*descriptor.possessive*", Grammar.MakePossessive(randomMeaningfulWord)).Replace("*dish*", ExpandString("<spice.cooking.recipeNames.categorizedFoods." + text4 + ".!random>")));
			SetEntityProperty("signatureDishName", text5);
			AddEntityListItem("sacredThings", "the dish known as " + text5);
			string value = string.Format("Since the first {0}, the villagers of {1} have {2} feasted on {3}.|{4}", Grammar.MakeTitleCaseWithArticle(ExpandString("<spice.villages.festivalName.!random>")), snapshotAtYear.Name, ExpandString("<spice.commonPhrases.merrily.!random>"), text5, id);
			AddEntityListItem("Gospels", value);
			if (If.Chance(7))
			{
				SetEntityProperty("newFactionName", generateFactionName(text5));
			}
			return;
		}
		num2 += 333;
		if (num <= num2 + 333)
		{
			string text6 = (bVillageZero ? CookingRecipe.RollOvenSafeIngredient("Ingredients0") : CookingRecipe.RollOvenSafeIngredient("Ingredients" + Stat.Roll(0, 8)));
			GameObject gameObject = GameObject.Create(text6);
			string text7 = (gameObject.HasPart<LiquidVolume>() ? ColorUtility.StripFormatting(gameObject.GetPart<LiquidVolume>().GetPrimaryLiquid().GetName(null)) : gameObject.DisplayNameOnlyDirectAndStripped);
			text7 = text7.Replace(" injector", "");
			AddEntityListItem("signatureDishIngredients", text6);
			string value = string.Format("The villagers of {0} are known throughout the {1} for their {2} use of {3} in meal preparation.|{4}", snapshotAtYear.Name, GameObject.Create("Terrain" + snapshotAtYear.GetProperty("region")).DisplayNameOnlyDirectAndStripped, ExpandString("<spice.adjectivesJudgement.!random>"), text7, id);
			AddEntityListItem("Gospels", value);
			if (If.Chance(7))
			{
				SetEntityProperty("newFactionName", generateFactionName(text7));
			}
		}
	}

	public static string generateUniqueVillageLiquid(string tierGroup, List<string> exclusions)
	{
		string blueprint;
		do
		{
			blueprint = PopulationManager.RollOneFrom("Villages_LiquidType_" + tierGroup + "_*Default").Blueprint;
		}
		while (exclusions.Contains(blueprint));
		return blueprint;
	}

	public static string getLiquidTierGroup(int tier)
	{
		if (tier < 3)
		{
			return "Lowtier";
		}
		if (tier < 6)
		{
			return "Midtier";
		}
		return "Hightier";
	}

	public static string generateFactionName(string root)
	{
		if (If.CoinFlip())
		{
			return Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.commonPhrases.cult.!random>") + " of the " + root);
		}
		return Grammar.MakeTitleCase(root + HistoricStringExpander.ExpandString(" <spice.commonPhrases.cult.!random>"));
	}
}
