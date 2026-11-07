using System;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class LongbladeStance_Defensive : Effect, ITierInitialized
{
	public LongbladeStance_Defensive()
	{
		DisplayName = "{{G|defensive stance}}";
		Duration = 9999;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(100, 500);
	}

	public override string GetStateDescription()
	{
		return "{{G|in defensive stance}}";
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

	public override string GetDetails()
	{
		if (Object.HasPart<LongBladesImprovedDefensiveStance>())
		{
			return "+3 DV while wielding a long blade in the primary hand.";
		}
		return "+2 DV while wielding a long blade in the primary hand.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RemoveEffect(typeof(LongbladeStance_Aggressive));
		Object.RemoveEffect(typeof(LongbladeStance_Defensive));
		Object.RemoveEffect(typeof(LongbladeStance_Dueling));
		return true;
	}
}
