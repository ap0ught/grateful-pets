using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Disoriented : Effect, ITierInitialized
{
	public int Level;

	public Disoriented()
	{
		DisplayName = "{{r|disoriented}}";
	}

	public Disoriented(int Duration, int Level)
		: this()
	{
		base.Duration = Duration;
		this.Level = Level;
	}

	public void Initialize(int Tier)
	{
		Level = 4;
		Duration = Stat.Random(50, 200);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "-" + Level + " DV\n-" + Level + " MA";
	}

	public override bool SameAs(Effect e)
	{
		if ((e as Disoriented).Level != Level)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("ApplyDisoriented"))
		{
			return false;
		}
		Disoriented effect = Object.GetEffect<Disoriented>();
		if (effect != null)
		{
			if (effect.Level == Level && effect.Duration < Duration)
			{
				effect.Duration = Duration;
			}
			else if (effect.Level * effect.Duration < Level * Duration)
			{
				effect.Level = Level;
				effect.Duration = Duration;
			}
			return false;
		}
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, "DV", -Level);
		StatShifter.SetStatShift(Object, "MA", -Level);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
