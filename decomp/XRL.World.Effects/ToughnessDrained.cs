using System;

namespace XRL.World.Effects;

[Serializable]
public class ToughnessDrained : Effect
{
	private int Penalty;

	public ToughnessDrained()
	{
		DisplayName = "{{r|drained}}";
	}

	public ToughnessDrained(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as ToughnessDrained).Penalty != Penalty)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return Penalty + " Toughness";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<ToughnessDrained>())
		{
			ToughnessDrained effect = Object.GetEffect<ToughnessDrained>();
			if (effect.Duration > Duration)
			{
				effect.Duration = Duration;
			}
			effect.UnapplyStats();
			effect.Penalty++;
			effect.ApplyStats();
			return false;
		}
		if (Object.FireEvent(Event.New("ApplyToughnessDrained", "Duration", Duration)))
		{
			Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_negativeVitality");
			Penalty = 1;
			ApplyStats();
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		Penalty = 0;
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift("Toughness", -Penalty);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts();
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		return true;
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
