using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Greased : Effect, ITierInitialized
{
	public Greased()
	{
		DisplayName = "greased";
		Duration = 9999;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(200, 1000);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 16777248;
	}

	public override string GetDetails()
	{
		return "Can walk over webs without getting stuck.";
	}

	public override bool Apply(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_movementBuff");
		return base.Apply(Object);
	}
}
