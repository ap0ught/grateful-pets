using System;
using Qud.API;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenGift : SocialSifrahTokenGift
{
	public RitualSifrahTokenGift()
	{
		Description = "offer an item";
	}

	public RitualSifrahTokenGift(string Blueprint)
		: this()
	{
		base.Blueprint = Blueprint;
		GameObject gameObject = GameObject.CreateSample(Blueprint);
		Description = "offer " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		gameObject.Obliterate();
	}

	public static bool IsFood(string Blueprint)
	{
		return IsFood(GameObjectFactory.Factory.GetBlueprintIfExists(Blueprint));
	}

	public static bool IsFood(GameObjectBlueprint BP)
	{
		if (BP != null && BP.HasPart("Food"))
		{
			return !BP.GetPartParameter("Food", "Gross", Default: false);
		}
		return false;
	}

	public new static RitualSifrahTokenGift GetAppropriate(GameObject ContextObject)
	{
		string stringProperty = ContextObject.GetStringProperty("SignatureItemBlueprint");
		if (!stringProperty.IsNullOrEmpty())
		{
			return new RitualSifrahTokenGift(stringProperty);
		}
		int tier = ContextObject.GetTier();
		for (int i = 0; i < 10; i++)
		{
			GameObjectBlueprint anObjectBlueprintModel = EncountersAPI.GetAnObjectBlueprintModel((GameObjectBlueprint pbp) => pbp.HasTagOrProperty("Gift") && !IsFood(pbp) && !pbp.HasPart("Brain") && pbp.GetPartParameter("Physics", "Takeable", Default: true) && !pbp.GetPartParameter<string>("Render", "DisplayName").Contains("[") && (!pbp.Props.ContainsKey("SparkingQuestBlueprint") || pbp.Name == pbp.Props["SparkingQuestBlueprint"]) && (!pbp.HasTagOrProperty("GiftTrueKinOnly") || ContextObject.IsTrueKin()) && pbp.Tier <= tier);
			if (anObjectBlueprintModel != null)
			{
				string propertyOrTag = anObjectBlueprintModel.GetPropertyOrTag("GiftSkillRestriction");
				if ((propertyOrTag.IsNullOrEmpty() || ContextObject.HasSkill(propertyOrTag)) && anObjectBlueprintModel.GetPartParameter("Examiner", "Complexity", 0) == 0)
				{
					return new RitualSifrahTokenGift(anObjectBlueprintModel.Name);
				}
			}
		}
		return null;
	}
}
