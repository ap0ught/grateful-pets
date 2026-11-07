using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Shaken : Effect, ITierInitialized
{
	public int Level;

	public Shaken()
	{
		DisplayName = "shaken";
	}

	public Shaken(int Duration, int Level)
		: this()
	{
		base.Duration = Duration;
		this.Level = Level;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(50, 200);
		Level = Stat.Random(1, 20);
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as Shaken).Level != Level)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "-" + Level + " DV";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("CanApplyShaken"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyShaken"))
		{
			return false;
		}
		Shaken effect = Object.GetEffect<Shaken>();
		if (effect != null)
		{
			bool flag = false;
			if (Duration > effect.Duration)
			{
				effect.Duration = Duration;
				flag = true;
			}
			if (Level > effect.Level)
			{
				effect.Level = Level;
				flag = true;
			}
			if (flag)
			{
				effect.ApplyStats();
			}
			return false;
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		base.Remove(Object);
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, "DV", -Level);
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
