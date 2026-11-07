using System;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenBit : SifrahPrioritizableToken
{
	public char Color;

	public TinkeringSifrahTokenBit()
	{
		Description = "use bit";
		RenderString = "?";
	}

	public TinkeringSifrahTokenBit(BitType bitType)
		: this()
	{
		Description = "use " + bitType.Description;
		RenderString = BitType.TranslateBit(bitType.Color);
		ColorString = "&" + bitType.Color;
		Color = bitType.Color;
	}

	public override int GetPriority()
	{
		BitType bitType = BitType.BitMap[Color];
		int num = BitLocker.GetBitCount(The.Player, Color);
		if (num > 0 && bitType.Level > 0)
		{
			num = Math.Max(num * (10 - bitType.Level) / 10, 1);
		}
		return num;
	}

	public override int GetTiebreakerPriority()
	{
		return 30 - BitType.GetBitSortOrder(Color);
	}

	public int GetNumberAvailable(int Chosen = 0)
	{
		return BitLocker.GetBitCount(The.Player, Color) - Chosen;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		return Description + " [have {{C|" + GetNumberAvailable(Game.GetTimesChosen(this, Slot)) + "}}]";
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (GetNumberAvailable(Game.GetTimesChosen(this, Slot)) <= 0)
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		int timesChosen = Game.GetTimesChosen(this, Slot);
		if (GetNumberAvailable(timesChosen) <= 0)
		{
			Popup.ShowFail("You do not have any " + ((timesChosen > 0) ? "more " : "") + BitType.BitMap[Color].Description + ".");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		BitLocker.UseBits(The.Player, Color, 1);
	}
}
