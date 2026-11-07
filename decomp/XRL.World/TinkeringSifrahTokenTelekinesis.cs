using System;
using XRL.World.Parts.Mutation;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenTelekinesis : SifrahToken
{
	public TinkeringSifrahTokenTelekinesis()
	{
		Description = "telekinetic manipulation";
		Tile = "Items/sw_wind_turbine_2.bmp";
		RenderString = "\u00a8";
		ColorString = "&M";
		DetailColor = 'm';
	}

	public TinkeringSifrahTokenTelekinesis(string AltDesc)
		: this()
	{
		Description = AltDesc;
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!UsabilityCheckedThisTurn)
		{
			Telekinesis part = The.Player.GetPart<Telekinesis>();
			if (part == null || !part.Activate())
			{
				DisabledThisTurn = true;
				return false;
			}
		}
		return true;
	}
}
