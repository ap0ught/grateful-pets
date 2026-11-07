using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class SynapseSnap : Effect
{
	public SynapseSnap()
	{
		DisplayName = "synapse snap";
	}

	public SynapseSnap(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 67108866;
	}

	public override string GetStateDescription()
	{
		return "synaptically snappy";
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		Object.ParticleText("*synapse snap*", 'W');
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Object.ParticleText("*synapse snap wore off*", 'r');
		UnapplyChanges();
	}

	private void ApplyChanges()
	{
		StatShifter.SetStatShift("Agility", 4);
		StatShifter.SetStatShift("Intelligence", 4);
	}

	private void UnapplyChanges()
	{
		StatShifter.RemoveStatShifts();
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("AfterDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 25 && num < 35)
		{
			E.Tile = null;
			E.RenderString = "\u0018";
			E.ColorString = "&W";
		}
		return true;
	}
}
