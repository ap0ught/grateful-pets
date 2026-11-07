using System;

namespace XRL.World.Effects;

[Serializable]
public class ToughnessBoosted : Effect
{
	private int Bonus;

	public ToughnessBoosted()
	{
		DisplayName = "{{g|vitalized}}";
	}

	public ToughnessBoosted(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		int num = 67108868;
		if (Bonus < 0)
		{
			num |= 0x2000000;
		}
		if (Math.Abs(Bonus) < 6)
		{
			num |= 0x1000000;
		}
		return num;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as ToughnessBoosted).Bonus != Bonus)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return Bonus.Signed() + " Toughness";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<ToughnessBoosted>())
		{
			ToughnessBoosted effect = Object.GetEffect<ToughnessBoosted>();
			if (effect.Duration < Duration)
			{
				effect.Duration = Duration;
			}
			effect.UnapplyStats();
			effect.Bonus++;
			effect.ApplyStats();
			return false;
		}
		if (Object.FireEvent(Event.New("ApplyToughnessBoosted", "Duration", Duration)))
		{
			Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
			Bonus = 1;
			ApplyStats();
			return true;
		}
		return false;
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift("Toughness", Bonus);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts();
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		Bonus = 0;
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
