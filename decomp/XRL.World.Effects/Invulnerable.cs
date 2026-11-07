using System;

namespace XRL.World.Effects;

[Serializable]
public class Invulnerable : Effect
{
	public bool VisibleFlag;

	public Invulnerable()
	{
		DisplayName = "{{O|invulnerable}}";
	}

	public Invulnerable(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override string GetDescription()
	{
		if (!VisibleFlag)
		{
			return null;
		}
		return base.GetDescription();
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.TryGetEffect<Invulnerable>(out var Effect))
		{
			Effect.Duration = Math.Max(Effect.Duration, Duration);
			Effect.VisibleFlag = VisibleFlag;
			return false;
		}
		return base.Apply(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != BeforeDieEvent.ID && ID != PooledEvent<CanBeDismemberedEvent>.ID)
		{
			return ID == PooledEvent<GetElectricalConductivityEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == Object)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDieEvent E)
	{
		if (E.Dying == Object)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeDismemberedEvent E)
	{
		if (E.Object == Object)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 1 && E.Object == Object)
		{
			E.Value = 0;
			return false;
		}
		return base.HandleEvent(E);
	}
}
