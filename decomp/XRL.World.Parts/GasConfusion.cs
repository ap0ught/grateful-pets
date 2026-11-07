using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasConfusion : IGasBehavior
{
	public string GasType = "Confusion";

	public override bool SameAs(IPart p)
	{
		if ((p as GasConfusion).GasType != GasType)
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
					E.MinWeight(StepValue(part.Density) / 2 + 20, 60);
				}
			}
			else
			{
				E.MinWeight(3);
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
					E.MinWeight(StepValue(part.Density) / 10 + 4, 12);
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
		ApplyConfusion(E.Object);
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ApplyConfusion();
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

	public void ApplyConfusion()
	{
		ApplyConfusion(ParentObject.CurrentCell);
	}

	public void ApplyConfusion(Cell C)
	{
		if (C != null)
		{
			int i = 0;
			for (int count = C.Objects.Count; i < count; i++)
			{
				ApplyConfusion(C.Objects[i]);
			}
		}
	}

	public bool IsAffectable(GameObject Object, Gas Gas = null)
	{
		if (Object == null)
		{
			return false;
		}
		if (!CheckGasCanAffectEvent.Check(Object, ParentObject, Gas))
		{
			return false;
		}
		if (Object.FireEvent("CanApplyConfusion") && Object.FireEvent("CanApplyConfusionGas") && CanApplyEffectEvent.Check(Object, "Confusion"))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public void ApplyConfusion(GameObject GO)
	{
		if (GO == ParentObject || GO.Brain == null || !GO.Respires)
		{
			return;
		}
		Gas part = ParentObject.GetPart<Gas>();
		if (IsAffectable(GO, part))
		{
			int num = GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject, part, null, 0, 0, WillAllowSave: true);
			if (num > 0 && !GO.MakeSave("Toughness", 5 + part.Level + num / 10, null, null, "Confusion Inhaled Gas", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				GO.ApplyEffect(new Confused(Stat.Roll("4d6") + part.Level, part.Level, part.Level + 2, "ToxicConfusion"));
			}
		}
	}
}
