using System;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainStrength_DismemberOrSlam_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	private bool didFire;

	public override string GetDescription()
	{
		return "@they have a 50% chance to Dismember or Slam @their opponent.";
	}

	public override string GetNotification()
	{
		if (didFire)
		{
			return "@they perform@s an act of brutal violence.";
		}
		return null;
	}

	public override void Apply(GameObject go)
	{
		if (!go.IsPlayer())
		{
			return;
		}
		if (Stat.Random(1, 100) <= 50)
		{
			if (Stat.Random(1, 100) <= 50)
			{
				Cudgel_Slam.Cast(go);
			}
			else
			{
				Axe_Dismember.CastForceSuccess(go);
			}
			didFire = true;
		}
		else
		{
			didFire = false;
		}
	}
}
