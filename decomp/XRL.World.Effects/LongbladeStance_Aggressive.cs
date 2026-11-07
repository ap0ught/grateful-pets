using System;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class LongbladeStance_Aggressive : Effect, ITierInitialized
{
	public LongbladeStance_Aggressive()
	{
		DisplayName = "{{R|aggressive stance}}";
		Duration = 9999;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(100, 500);
	}

	public override string GetStateDescription()
	{
		return "{{R|in aggressive stance}}";
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
		if (Object.HasPart<LongBladesImprovedAggressiveStance>())
		{
			return "+2 to your penetration roll and -3 to hit while wielding a long blade in the primary hand.";
		}
		return "+1 to your penetration roll and -2 to hit while wielding a long blade in the primary hand.";
	}
}
