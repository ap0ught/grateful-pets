using System;
using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenItem : SifrahPrioritizableToken
{
	public string Blueprint;

	public SocialSifrahTokenItem()
	{
		Description = "gift an item";
		Tile = "Items/sw_gadget.bmp";
		RenderString = "\n";
		ColorString = "&M";
		DetailColor = 'W';
	}

	public SocialSifrahTokenItem(string Blueprint)
		: this()
	{
		this.Blueprint = Blueprint;
		GameObject gameObject = GameObject.CreateSample(Blueprint);
		Description = "gift " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		gameObject.Obliterate();
	}

	public static SocialSifrahTokenItem GetAppropriate(GameObject ContextObject)
	{
		string stringProperty = ContextObject.GetStringProperty("SignatureItemBlueprint");
		if (!stringProperty.IsNullOrEmpty())
		{
			return new SocialSifrahTokenItem(stringProperty);
		}
		int tier = ContextObject.GetTier();
		for (int i = 0; i < 20; i++)
		{
			GameObjectBlueprint randomElement = GameObjectFactory.Factory.BlueprintList.GetRandomElement();
			if (!randomElement.HasPart("Brain") && randomElement.HasPart("Physics") && randomElement.HasPart("Render") && !randomElement.Tags.ContainsKey("NoSparkingQuest") && !randomElement.Tags.ContainsKey("BaseObject") && !randomElement.Tags.ContainsKey("ExcludeFromDynamicEncounters") && randomElement.GetPartParameter("Physics", "Takeable", Default: true) && randomElement.GetPartParameter("Physics", "IsReal", Default: true) && !randomElement.GetPartParameter<string>("Render", "DisplayName").Contains("[") && (!randomElement.Props.ContainsKey("SparkingQuestBlueprint") || randomElement.Name == randomElement.Props["SparkingQuestBlueprint"]) && randomElement.Tier <= tier && randomElement.GetPartParameter("Examiner", "Complexity", 0) == 0)
			{
				return new SocialSifrahTokenItem(randomElement.Name);
			}
		}
		return null;
	}

	public override int GetPriority()
	{
		return GetNumberAvailable();
	}

	public override int GetTiebreakerPriority()
	{
		return 0;
	}

	public int GetNumberAvailable(int Chosen = 0)
	{
		int num = -Chosen;
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.Blueprint == Blueprint)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsAvailable(int Chosen = 0)
	{
		int num = 0;
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.Blueprint == Blueprint)
			{
				num++;
				if (num > Chosen)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		return Description + " [have {{C|" + GetNumberAvailable(Game.GetTimesChosen(this, Slot)) + "}}]";
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable(Game.GetTimesChosen(this, Slot)))
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		int timesChosen = Game.GetTimesChosen(this);
		if (!IsAvailable(timesChosen))
		{
			GameObject gameObject = GameObject.CreateSample(Blueprint);
			if (gameObject == null)
			{
				Popup.ShowFail("You do not have any more of that kind of item.");
			}
			else if (timesChosen > 0)
			{
				Popup.ShowFail("You do not have any more " + gameObject.GetPluralName() + ".");
			}
			else
			{
				Popup.ShowFail("You do not have " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
			}
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override int GetPowerup(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (Slot.CurrentMove == Slot.Token)
		{
			return 1;
		}
		return base.GetPowerup(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.Blueprint == Blueprint)
			{
				item.Destroy();
				break;
			}
		}
		base.UseToken(Game, Slot, ContextObject);
	}
}
