using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasDisease : IGasBehavior
{
	public string GasType = "Disease";

	public override bool SameAs(IPart p)
	{
		if ((p as GasDisease).GasType != GasType)
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
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor.FireEvent("CanApplyDisease") && CanApplyEffectEvent.Check(E.Actor, "Disease") && E.Actor.PhaseMatches(ParentObject))))
				{
					E.MinWeight(GasDensityStepped() / 2 + 20, 60);
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
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor.FireEvent("CanApplyDisease") && CanApplyEffectEvent.Check(E.Actor, "Disease") && E.Actor.PhaseMatches(ParentObject))))
				{
					E.MinWeight(GasDensityStepped() / 10 + 4, 12);
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
		ApplyDisease(E.Object);
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ApplyDisease();
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

	public void ApplyDisease()
	{
		ApplyDisease(ParentObject.CurrentCell);
	}

	public void ApplyDisease(Cell C)
	{
		if (C != null)
		{
			int i = 0;
			for (int count = C.Objects.Count; i < count; i++)
			{
				ApplyDisease(C.Objects[i]);
			}
		}
	}

	public void ApplyDisease(GameObject GO)
	{
		if (GO == ParentObject || GO.Brain == null || !GO.Respires)
		{
			return;
		}
		Gas part = ParentObject.GetPart<Gas>();
		if (!CheckGasCanAffectEvent.Check(GO, ParentObject, part) || !GO.FireEvent("CanApplyDisease") || !CanApplyEffectEvent.Check(GO, "Disease") || !GO.PhaseMatches(ParentObject))
		{
			return;
		}
		int num = GetRespiratoryAgentPerformanceEvent.GetFor(GO, ParentObject, part, null, 0, 0, WillAllowSave: true);
		if (num > 0 && !GO.MakeSave("Toughness", 5 + part.Level + num / 10, null, null, "Disease Inhaled Gas", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
		{
			bool flag = false;
			flag = ((GO.CurrentZone.Z % 2 != 0) ? GO.ApplyEffect(new GlotrotOnset()) : GO.ApplyEffect(new IronshankOnset()));
			if (flag && GO.IsPlayer())
			{
				Popup.Show("You feel sick.");
			}
		}
	}
}
