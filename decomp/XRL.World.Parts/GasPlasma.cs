using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasPlasma : IObjectGasBehavior
{
	public string GasType = "Plasma";

	public int GasLevel = 1;

	public override bool SameAs(IPart p)
	{
		GasPlasma gasPlasma = p as GasPlasma;
		if (gasPlasma.GasType != GasType)
		{
			return false;
		}
		if (gasPlasma.GasLevel != GasLevel)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetAdjacentNavigationWeightEvent.ID)
		{
			return ID == GetNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!E.IgnoreGases && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor.FireEvent("CanApplyCoatedInPlasma") && E.Actor.PhaseMatches(ParentObject))))
				{
					E.MinWeight(GasDensityStepped() / 3 + 40, 90);
				}
			}
			else
			{
				E.MinWeight(70);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (!E.IgnoreGases && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor.FireEvent("CanApplyCoatedInPlasma") && E.Actor.PhaseMatches(ParentObject))))
				{
					E.MinWeight(GasDensityStepped() / 15 + 8, 18);
				}
			}
			else
			{
				E.MinWeight(14);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool ApplyGas(GameObject Object)
	{
		if (Object == ParentObject)
		{
			return false;
		}
		Gas part = ParentObject.GetPart<Gas>();
		if (!CheckGasCanAffectEvent.Check(Object, ParentObject, part))
		{
			return false;
		}
		if (!Object.FireEvent("CanApplyCoatedInPlasma"))
		{
			return false;
		}
		if (!CanApplyEffectEvent.Check<CoatedInPlasma>(Object))
		{
			return false;
		}
		if (!Object.PhaseMatches(ParentObject))
		{
			return false;
		}
		if (part.Density <= 0)
		{
			return false;
		}
		int num = Stat.Random(part.Density * 2 / 5, part.Density * 3 / 5);
		if (num <= 0)
		{
			return false;
		}
		CoatedInPlasma effect = Object.GetEffect<CoatedInPlasma>();
		if (effect != null)
		{
			if (effect.Duration < num)
			{
				effect.Duration = num;
			}
			if (!GameObject.Validate(ref effect.Owner) && GameObject.Validate(part.Creator))
			{
				effect.Owner = part.Creator;
			}
			return true;
		}
		effect = new CoatedInPlasma(num, part.Creator);
		return Object.ApplyEffect(effect);
	}
}
