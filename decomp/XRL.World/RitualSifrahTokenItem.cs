using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenItem : SocialSifrahTokenItem
{
	public RitualSifrahTokenItem()
	{
		Description = "offer an item";
	}

	public RitualSifrahTokenItem(string Blueprint)
		: this()
	{
		base.Blueprint = Blueprint;
		GameObject gameObject = GameObject.CreateSample(Blueprint);
		Description = "offer " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		gameObject.Obliterate();
	}

	public new static RitualSifrahTokenItem GetAppropriate(GameObject ContextObject)
	{
		string stringProperty = ContextObject.GetStringProperty("SignatureItemBlueprint");
		if (!stringProperty.IsNullOrEmpty() && !RitualSifrahTokenFood.IsFood(stringProperty))
		{
			return new RitualSifrahTokenItem(stringProperty);
		}
		int tier = ContextObject.GetTier();
		for (int i = 0; i < 30; i++)
		{
			GameObjectBlueprint randomElement = GameObjectFactory.Factory.BlueprintList.GetRandomElement();
			if (!randomElement.HasPart("Brain") && randomElement.HasPart("Physics") && randomElement.HasPart("Render") && !RitualSifrahTokenFood.IsFood(randomElement) && !randomElement.Tags.ContainsKey("NoSparkingQuest") && !randomElement.Tags.ContainsKey("BaseObject") && !randomElement.Tags.ContainsKey("ExcludeFromDynamicEncounters") && randomElement.GetPartParameter("Physics", "Takeable", Default: true) && randomElement.GetPartParameter("Physics", "IsReal", Default: true) && !randomElement.GetPartParameter<string>("Render", "DisplayName").Contains("[") && (!randomElement.Props.ContainsKey("SparkingQuestBlueprint") || randomElement.Name == randomElement.Props["SparkingQuestBlueprint"]) && randomElement.Tier <= tier && randomElement.GetPartParameter("Examiner", "Complexity", 0) == 0)
			{
				return new RitualSifrahTokenItem(randomElement.Name);
			}
		}
		return null;
	}
}
