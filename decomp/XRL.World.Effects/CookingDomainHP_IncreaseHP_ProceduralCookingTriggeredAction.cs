using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public int Tier;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(30, 40);
		base.Init(target);
	}

	public override string GetDescription()
	{
		return "@they get +" + Tier + "% max HP for 1 hour.";
	}

	public override string GetTemplatedDescription()
	{
		return "@they get +30-40% max HP for 1 hour.";
	}

	public override string GetNotification()
	{
		return "@they become heartier.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer() && !go.HasEffect<CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainHP_IncreaseHP_ProceduralCookingTriggeredActionEffect(Tier));
		}
	}
}
