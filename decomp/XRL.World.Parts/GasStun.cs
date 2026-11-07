using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasStun : IGasBehavior
{
	public string GasType = "Stun";

	public override bool SameAs(IPart p)
	{
		if ((p as GasStun).GasType != GasType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetAdjacentNavigationWeightEvent.ID && ID != GetNavigationWeightEvent.ID)
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
					E.MinWeight(StepValue(part.Density) / 2 + 1, 51);
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
					E.MinWeight(StepValue(part.Density) / 10, 10);
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
		if (E.Object != ParentObject && E.Object.PhaseMatches(ParentObject))
		{
			ApplyStun(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = cell.Objects[i];
				if (gameObject != ParentObject && gameObject.Brain != null && ParentObject.PhaseMatches(gameObject))
				{
					ApplyStun(gameObject);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DensityChange");
		base.Register(Object, Registrar);
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
		if (Object.FireEvent("CanApplyStunGasStun") && CanApplyEffectEvent.Check<StunGasStun>(Object))
		{
			return Object.PhaseMatches(ParentObject);
		}
		return false;
	}

	public void ApplyStun(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && Object.Respires && IsAffectable(Object))
		{
			int num = GetRespiratoryAgentPerformanceEvent.GetFor(Object, ParentObject);
			if (num > 0)
			{
				Object.RemoveEffect<StunGasStun>();
				Object.ApplyEffect(new StunGasStun(num));
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DensityChange" && StepValue(E.GetIntParameter("OldValue")) != StepValue(E.GetIntParameter("NewValue")))
		{
			FlushNavigationCaches();
		}
		return base.FireEvent(E);
	}
}
