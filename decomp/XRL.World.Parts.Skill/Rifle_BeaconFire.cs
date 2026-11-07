using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_BeaconFire : BaseSkill
{
	public static bool MeetsCriteria(GameObject GO)
	{
		if (!GameObject.Validate(ref GO))
		{
			return false;
		}
		if (!GO.IsVisible())
		{
			return false;
		}
		if (GO.IsAflame())
		{
			return true;
		}
		if (GO.HasEffect<Luminous>())
		{
			return true;
		}
		if (GO.HasPart<Raycat>())
		{
			return true;
		}
		if (GO.HasEffect<LiquidCovered>())
		{
			foreach (LiquidCovered item in GO.YieldEffects<LiquidCovered>())
			{
				if (item.Liquid == null)
				{
					continue;
				}
				foreach (string key in item.Liquid.ComponentLiquids.Keys)
				{
					if (LiquidVolume.GetLiquid(key).Glows)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
