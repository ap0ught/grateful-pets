using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class TimeDilatedIndependent : ITimeDilated, ITierInitialized
{
	public double? SpeedPenaltyMultiplier;

	public TimeDilatedIndependent()
	{
	}

	public TimeDilatedIndependent(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public TimeDilatedIndependent(int Duration, double SpeedPenaltyMultiplier)
		: this(Duration)
	{
		this.SpeedPenaltyMultiplier = SpeedPenaltyMultiplier;
	}

	public override void Initialize(int Tier)
	{
		Tier = Stat.Random(Tier - 2, Tier + 2);
		if (Tier < 1)
		{
			Tier = 1;
		}
		if (Tier > 8)
		{
			Tier = 8;
		}
		Duration = Stat.Random(30, 40);
		SpeedPenaltyMultiplier = Stat.Random(1, 9);
		SpeedPenaltyMultiplier /= 10.0;
	}

	public override bool DoTimeDilationVisualEffects()
	{
		return Duration > 0;
	}

	public override bool Apply(GameObject Object)
	{
		bool flag = base.Apply(Object);
		if (flag)
		{
			Sync();
		}
		return flag;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public void Sync()
	{
		if (SpeedPenaltyMultiplier.HasValue)
		{
			int num = Math.Max((int)((double)Object.BaseStat("Speed") * SpeedPenaltyMultiplier).Value, 1);
			if (num != SpeedPenalty)
			{
				UnapplyChanges();
				SpeedPenalty = num;
				ApplyChanges();
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && --Duration > 0)
		{
			Sync();
		}
		return base.FireEvent(E);
	}
}
