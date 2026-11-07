using System;
using XRL.Liquids;
using XRL.World.Parts;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenLiquid : TinkeringSifrahTokenLiquid
{
	public SocialSifrahTokenLiquid()
	{
		Description = "share liquid";
	}

	public SocialSifrahTokenLiquid(string LiquidID)
		: base(LiquidID)
	{
		BaseLiquid liquid = LiquidVolume.GetLiquid(base.LiquidID);
		Description = "share " + liquid.GetName();
	}

	public SocialSifrahTokenLiquid(GameObject ContextObject, bool WaterRitual = false)
		: this(GetAppropriate(ContextObject, WaterRitual))
	{
	}

	public static string GetAppropriate(GameObject ContextObject, bool WaterRitual = false)
	{
		string waterRitualLiquid = ContextObject.GetWaterRitualLiquid(The.Player);
		if (WaterRitual)
		{
			if (!waterRitualLiquid.IsNullOrEmpty())
			{
				return waterRitualLiquid;
			}
			return "water";
		}
		switch (waterRitualLiquid)
		{
		case "water":
			return "wine";
		case "oil":
			return "gel";
		case "slime":
			return "proteangunk";
		case "blood":
			return "brainbrine";
		default:
			if (!waterRitualLiquid.IsNullOrEmpty())
			{
				return waterRitualLiquid;
			}
			return "water";
		}
	}
}
