using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHP_HealToFull_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	private bool didFire;

	public override string GetDescription()
	{
		return "@they heal to full 15% of the time.";
	}

	public override string GetNotification()
	{
		if (didFire)
		{
			return "@they heal to full.";
		}
		return null;
	}

	public override void Apply(GameObject go)
	{
		if (Stat.Random(1, 100) <= 15)
		{
			didFire = true;
			go.Statistics["Hitpoints"].Penalty = 0;
		}
		else
		{
			didFire = false;
		}
	}
}
