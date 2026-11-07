using System;

namespace XRL.World.Effects;

[Serializable]
public class Frozen : Effect
{
	public Frozen()
	{
		DisplayName = "{{freezing|frozen}}";
	}

	public Frozen(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override string GetDescription()
	{
		if (Object == null || !Object.IsPlayer())
		{
			return base.GetDescription();
		}
		return null;
	}

	public override int GetEffectType()
	{
		return 33555456;
	}

	public override string GetDetails()
	{
		return "Can't take physical actions.";
	}

	public override bool CanApplyToStack()
	{
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (!Object.IsFrozen())
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public static bool CanApply(GameObject Object)
	{
		if (!FrozeEvent.Check(Object, Object.Physics.InflamedBy))
		{
			return false;
		}
		if (!CanApplyEffectEvent.Check<Frozen>(Object))
		{
			return false;
		}
		if (Object.HasEffect<Frozen>())
		{
			return false;
		}
		return true;
	}

	public static bool ApplyTo(GameObject Object)
	{
		if (!CanApply(Object))
		{
			return false;
		}
		return Object.ApplyEffect(new Frozen(1));
	}

	public override bool Apply(GameObject Object)
	{
		PlayWorldSound("sfx_statusEffect_frozen");
		Object.MovementModeChanged("Frozen", Involuntary: true);
		Object?.CurrentCell?.FlushNavigationCache();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Object.PlayWorldSound("sfx_statusEffect_thawed");
		ThawedEvent.Send(Object, Object.Physics.InflamedBy);
		Object?.CurrentCell?.FlushNavigationCache();
	}

	public override bool Render(RenderEvent E)
	{
		E.ApplyColors("&c", 200);
		E.ApplyBackgroundColor("^C", 200);
		return true;
	}
}
