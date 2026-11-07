using System;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class LongbladeStance_Dueling : Effect, ITierInitialized
{
	public LongbladeStance_Dueling()
	{
		DisplayName = "{{W|dueling stance}}";
		Duration = 9999;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(100, 500);
	}

	public override string GetStateDescription()
	{
		return "{{W|in dueling stance}}";
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		if (Object != null && Object.HasEffect<Asleep>())
		{
			return null;
		}
		return base.GetDescription();
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override bool Apply(GameObject Object)
	{
		Object.RemoveEffect(typeof(LongbladeStance_Aggressive));
		Object.RemoveEffect(typeof(LongbladeStance_Defensive));
		Object.RemoveEffect(typeof(LongbladeStance_Dueling));
		return true;
	}

	public override string GetDetails()
	{
		if (Object.HasPart<LongBladesImprovedDuelistStance>())
		{
			return "+3 to hit while wielding a long blade in the primary hand.";
		}
		return "+2 to hit while wielding a long blade in the primary hand.";
	}
}
