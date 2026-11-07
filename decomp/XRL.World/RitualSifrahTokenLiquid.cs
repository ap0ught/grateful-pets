using System;
using XRL.Liquids;
using XRL.World.Parts;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenLiquid : SocialSifrahTokenLiquid
{
	public RitualSifrahTokenLiquid()
	{
		Description = "offer liquid";
	}

	public RitualSifrahTokenLiquid(string LiquidID)
		: base(LiquidID)
	{
		BaseLiquid liquid = LiquidVolume.GetLiquid(base.LiquidID);
		Description = "offer " + liquid.GetName();
	}

	public RitualSifrahTokenLiquid(GameObject ContextObject, bool WaterRitual = false)
		: this(SocialSifrahTokenLiquid.GetAppropriate(ContextObject, WaterRitual))
	{
	}
}
