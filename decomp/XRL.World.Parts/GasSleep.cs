using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasSleep : IGasBehavior
{
	public string GasType = "Sleep";

	public override bool SameAs(IPart p)
	{
		if ((p as GasSleep).GasType != GasType)
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
					int num = part.Level * 5;
					E.MinWeight(StepValue(part.Density) / 2 + num, Math.Min(50 + num, 70));
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
					int num = part.Level * 5;
					E.MinWeight(StepValue(part.Density) / 10 + num / 5, Math.Min(10 + num / 5, 14));
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
		ApplySleep(E.Object);
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ApplySleep();
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

	public void ApplySleep()
	{
		ApplySleep(ParentObject.CurrentCell);
	}

	public void ApplySleep(Cell C)
	{
		if (C != null)
		{
			for (int i = 0; i < C.Objects.Count; i++)
			{
				ApplySleep(C.Objects[i]);
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
		if (Object.FireEvent("CanApplySleep") && Object.FireEvent("CanApplySleegas") && Object.FireEvent("CanApplyInvoluntarySleep") && CanApplyEffectEvent.Check(Object, "Sleep"))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public bool ApplySleep(GameObject Object)
	{
		if (Object == ParentObject)
		{
			return false;
		}
		if (Object.Brain == null)
		{
			return false;
		}
		if (!Object.Respires)
		{
			return false;
		}
		if (Object.HasEffect<Asleep>())
		{
			return false;
		}
		Gas part = ParentObject.GetPart<Gas>();
		if (!IsAffectable(Object, part))
		{
			return false;
		}
		int num = GetRespiratoryAgentPerformanceEvent.GetFor(Object, ParentObject, part, null, 0, 0, WillAllowSave: true);
		if (num > 0 && !Object.MakeSave("Toughness", 5 + part.Level + num / 10, null, null, "Sleep Inhaled Gas", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
		{
			return Object.ApplyEffect(new Asleep("4d6".RollCached() + part.Level));
		}
		return false;
	}
}
