using System;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class BoostedImmunity : Effect
{
	public int Modifier = 4;

	public BoostedImmunity()
	{
		DisplayName = "{{G|boosted immunity}}";
		Duration = 1;
	}

	public BoostedImmunity(int Modifier)
		: this()
	{
		this.Modifier = Modifier;
	}

	public override int GetEffectType()
	{
		return 83886084;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as BoostedImmunity).Modifier != Modifier)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return ((Modifier > 0) ? "+" : "") + Modifier + " to saves vs. disease onset.";
	}

	public override bool Apply(GameObject Object)
	{
		BoostedImmunity effect = Object.GetEffect<BoostedImmunity>();
		if (effect != null)
		{
			if (Modifier > effect.Modifier)
			{
				effect.Modifier = Modifier;
			}
			return false;
		}
		if (GetDiseaseOnsetEvent.GetFor(Object) == null)
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Duration > 0 && GetDiseaseOnsetEvent.GetFor(Object) == null)
		{
			Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Disease Onset", E.Vs))
		{
			E.Roll += Modifier;
		}
		return base.HandleEvent(E);
	}
}
