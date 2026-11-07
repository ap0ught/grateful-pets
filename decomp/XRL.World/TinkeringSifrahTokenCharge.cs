using System;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenCharge : SifrahPrioritizableToken
{
	public int Amount = 1;

	public TinkeringSifrahTokenCharge()
	{
		Description = "use {{C|1}} charge from an energy cell";
		Tile = "Items/sw_electricalgeneration.bmp";
		RenderString = "\b";
		ColorString = "&W";
		DetailColor = 'B';
	}

	public TinkeringSifrahTokenCharge(int Amount)
		: this()
	{
		this.Amount = Amount;
		Description = "use {{C|" + Amount + "}} charge from an energy cell";
	}

	public override int GetPriority()
	{
		ElectricalGeneration part = The.Player.GetPart<ElectricalGeneration>();
		if (part != null && part.GetCharge() >= Amount)
		{
			return int.MaxValue;
		}
		int num = 0;
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.HasPartDescendedFrom<IEnergyCell>() && item.Understood())
			{
				try
				{
					num = checked(num + item.QueryCharge(LiveOnly: false, 0L));
				}
				catch (OverflowException)
				{
					return 2147483646;
				}
			}
		}
		return num;
	}

	public override int GetTiebreakerPriority()
	{
		return 2147483646;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		string text = Description;
		ElectricalGeneration part = The.Player.GetPart<ElectricalGeneration>();
		if (part != null && part.GetCharge() >= Amount)
		{
			text = text.Replace("from an energy cell", "via Electrical Generation");
		}
		return text;
	}

	public bool IsAvailable()
	{
		ElectricalGeneration part = The.Player.GetPart<ElectricalGeneration>();
		if (part != null && part.GetCharge() >= Amount)
		{
			return true;
		}
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.HasPartDescendedFrom<IEnergyCell>() && item.Understood() && item.TestCharge(Amount, LiveOnly: false, 0L))
			{
				return true;
			}
		}
		return false;
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable())
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable())
		{
			if (The.Player.GetPart<ElectricalGeneration>() != null)
			{
				Popup.ShowFail("You do not have any energy cells with " + Amount + " charge available, and your electrical generation capacity is unable to meet the demand.");
			}
			else
			{
				Popup.ShowFail("You do not have any energy cells with " + Amount + " charge available.");
			}
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		bool flag = false;
		ElectricalGeneration part = The.Player.GetPart<ElectricalGeneration>();
		if (part != null && part.GetCharge() >= Amount)
		{
			part.UseCharge(Amount);
			flag = true;
		}
		if (!flag)
		{
			foreach (GameObject item in The.Player.GetInventoryAndEquipment())
			{
				if (!item.HasPartDescendedFrom<IEnergyCell>() || !item.Understood() || !item.TestCharge(Amount, LiveOnly: false, 0L))
				{
					continue;
				}
				item.SplitFromStack();
				try
				{
					if (item.UseCharge(Amount, LiveOnly: false, 0L))
					{
						break;
					}
				}
				finally
				{
					if (GameObject.Validate(item))
					{
						item.CheckStack();
					}
				}
			}
		}
		base.UseToken(Game, Slot, ContextObject);
	}
}
