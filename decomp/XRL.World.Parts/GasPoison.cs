using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasPoison : IObjectGasBehavior
{
	public string GasType = "Poison";

	public int GasLevel = 1;

	public override bool SameAs(IPart p)
	{
		GasPoison gasPoison = p as GasPoison;
		if (gasPoison.GasType != GasType)
		{
			return false;
		}
		if (gasPoison.GasLevel != GasLevel)
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
		if (!E.IgnoreGases && !E.Unbreathing && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (IsAffectable(E.Actor))
				{
					E.MinWeight(GasDensityStepped() / 2 + GasLevel * 10, Math.Min(50 + GasLevel * 10, 80));
				}
			}
			else
			{
				E.MinWeight(8);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (!E.IgnoreGases && !E.Unbreathing && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (IsAffectable(E.Actor))
				{
					E.MinWeight(GasDensityStepped() / 10 + GasLevel * 2, Math.Min(10 + GasLevel * 2, 16));
				}
			}
			else
			{
				E.MinWeight(2);
			}
		}
		return base.HandleEvent(E);
	}

	public bool IsAffectable(GameObject Object, Gas Gas = null)
	{
		if (!CheckGasCanAffectEvent.Check(Object, ParentObject, Gas))
		{
			return false;
		}
		if (Object == null)
		{
			return true;
		}
		if (Object.FireEvent("CanApplyPoisonGasPoison") && CanApplyEffectEvent.Check<PoisonGasPoison>(Object))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public override bool ApplyGas(GameObject Object)
	{
		if (Object == ParentObject)
		{
			return false;
		}
		if (!Object.Respires)
		{
			return false;
		}
		if (!Object.HasTag("Creature"))
		{
			return false;
		}
		Gas part = ParentObject.GetPart<Gas>();
		if (!IsAffectable(Object, part))
		{
			return false;
		}
		int num = GetRespiratoryAgentPerformanceEvent.GetFor(Object, ParentObject, part);
		if (num <= 0)
		{
			return false;
		}
		Object.RemoveEffect<PoisonGasPoison>();
		PoisonGasPoison poisonGasPoison = new PoisonGasPoison(Stat.Random(1, 10), part.Creator);
		poisonGasPoison.Damage = GasLevel * 2;
		Object.ApplyEffect(poisonGasPoison);
		int amount = (int)Math.Max(Math.Floor((double)(num + 1) / 20.0), 1.0);
		return Object.TakeDamage(amount, "from %t {{g|poison}}!", "InhaleDanger Poison Gas", null, null, null, part.Creator, null, null, null, Accidental: false, Environmental: true);
	}
}
