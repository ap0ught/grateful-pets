using System;

namespace XRL.World.Effects;

[Serializable]
public class PhasedWhileStuck : Effect
{
	public Phased PhasedEffect;

	public PhasedWhileStuck()
	{
	}

	public PhasedWhileStuck(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 33554464;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		PhasedEffect = new Phased(9999);
		return Object.ApplyEffect(PhasedEffect);
	}

	public override void Remove(GameObject Object)
	{
		if (Duration >= 0)
		{
			Duration = -1;
			if (PhasedEffect == null || PhasedEffect.Object != Object)
			{
				Object.RemoveEffect<Phased>();
			}
			Object.RemoveEffect(PhasedEffect);
			base.Remove(Object);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (!Object.HasEffect<Stuck>())
		{
			Duration--;
			if (Duration <= 0)
			{
				Object.RemoveEffect(this);
			}
		}
		return base.HandleEvent(E);
	}
}
