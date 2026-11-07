using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class GasShame : IGasBehavior
{
	public string GasType = "Shame";

	public override bool SameAs(IPart p)
	{
		if ((p as GasShame).GasType != GasType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetAdjacentNavigationWeightEvent.ID && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
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
				Gas part = ParentObject.GetPart<Gas>();
				if (IsAffectable(E.Actor, part))
				{
					E.MinWeight(StepValue(part.Density) / 5 + 5, 60);
				}
			}
			else
			{
				E.MinWeight(2);
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
				Gas part = ParentObject.GetPart<Gas>();
				if (IsAffectable(E.Actor, part))
				{
					E.MinWeight(StepValue(part.Density) / 25 + 1, 12);
				}
			}
			else
			{
				E.MinWeight(1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ApplyShame(E.Object);
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ApplyShame();
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DensityChange");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DensityChange" && StepValue(E.GetIntParameter("OldValue")) != StepValue(E.GetIntParameter("NewValue")))
		{
			FlushNavigationCaches();
		}
		return base.FireEvent(E);
	}

	public void ApplyShame()
	{
		ApplyShame(ParentObject.CurrentCell);
	}

	public void ApplyShame(Cell C)
	{
		if (C != null)
		{
			int i = 0;
			for (int count = C.Objects.Count; i < count; i++)
			{
				ApplyShame(C.Objects[i]);
			}
		}
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
		if (Object.FireEvent("CanApplyShamed") && Object.FireEvent("CanApplyShameGas") && CanApplyEffectEvent.Check<Shamed>(Object))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public void ApplyShame(GameObject GO)
	{
		if (GO == ParentObject || GO.Brain == null || !GO.Respires)
		{
			return;
		}
		Gas part = ParentObject.GetPart<Gas>();
		if (IsAffectable(GO, part))
		{
			int num = GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject, part, null, 0, 0, WillAllowSave: true);
			if (num > 0 && !GO.MakeSave("Willpower", 5 + part.Level + num / 10, null, null, "Shame Inhaled Gas", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				GO.ApplyEffect(new Shamed("2d6".RollCached() + part.Level * 2));
			}
		}
	}
}
