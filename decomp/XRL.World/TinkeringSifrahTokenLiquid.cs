using System;
using XRL.Liquids;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenLiquid : SifrahPrioritizableToken
{
	public string LiquidID;

	public TinkeringSifrahTokenLiquid()
	{
		Description = "use liquid";
		Tile = "Items/sw_vial.bmp";
		RenderString = "~";
	}

	public TinkeringSifrahTokenLiquid(string LiquidID)
		: this()
	{
		this.LiquidID = LiquidID;
		BaseLiquid liquid = LiquidVolume.GetLiquid(this.LiquidID);
		Description = "use " + liquid.GetName();
		switch (LiquidID)
		{
		case "water":
			Tile = "Items/sw_waterskin.bmp";
			ColorString = "&w";
			DetailColor = 'y';
			return;
		case "oil":
			Tile = "Items/sw_waterskin.bmp";
			ColorString = "&K";
			DetailColor = 'y';
			return;
		case "wine":
			Tile = "Items/sw_jug.bmp";
			ColorString = "&w";
			DetailColor = 'm';
			return;
		case "brainbrine":
			Tile = "Items/sw_jug6.bmp";
			ColorString = "&w";
			DetailColor = 'g';
			return;
		case "proteangunk":
			Tile = "Items/sw_jug7.bmp";
			ColorString = "&w";
			DetailColor = 'c';
			return;
		case "slime":
			Tile = "Items/sw_jar2.bmp";
			ColorString = "&w";
			DetailColor = 'g';
			return;
		case "acid":
			Tile = "Items/sw_spray.bmp";
			ColorString = "&G";
			DetailColor = 'C';
			return;
		case "lava":
			Tile = "Items/sw_vial.bmp";
			ColorString = "&y";
			DetailColor = 'R';
			return;
		case "neutronflux":
			Tile = "Items/sw_vial.bmp";
			ColorString = "&y";
			DetailColor = 'B';
			return;
		}
		Tile = "Items/sw_jug2.bmp";
		TileColor = "&y";
		string color = liquid.GetColor();
		if (!string.IsNullOrEmpty(color))
		{
			ColorString = "&" + color;
			DetailColor = color[0];
		}
	}

	public override int GetPriority()
	{
		int num = The.Player.GetFreeDrams(LiquidID);
		if (num > 0)
		{
			BaseLiquid liquid = LiquidVolume.GetLiquid(LiquidID);
			num = Math.Max((int)((float)num / liquid.GetValuePerDram()), 1);
		}
		return num;
	}

	public override int GetTiebreakerPriority()
	{
		return 1;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		int num = The.Player.GetFreeDrams(LiquidID) - Game.GetTimesChosen(this, Slot);
		return Description + " [have {{C|" + num + "}} " + ((num == 1) ? "dram" : "drams") + "]";
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (The.Player.GetFreeDrams(LiquidID) - Game.GetTimesChosen(this, Slot) <= 0)
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		int timesChosen = Game.GetTimesChosen(this, Slot);
		if (The.Player.GetFreeDrams(LiquidID) - timesChosen <= 0)
		{
			Popup.ShowFail("You do not have any " + ((timesChosen > 0) ? " more" : "") + LiquidVolume.GetLiquid(LiquidID).GetName() + ".");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		The.Player.UseDrams(1, LiquidID);
	}
}
