using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class GasSteam : IGasBehavior
{
	public string GasType = "Steam";

	public override bool SameAs(IPart p)
	{
		if ((p as GasSteam).GasType != GasType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != SingletonEvent<GeneralAmnestyEvent>.ID && ID != GetAdjacentNavigationWeightEvent.ID && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			List<GameObject> list = Event.NewGameObjectList(cell.Objects);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				GameObject gameObject = list[i];
				if (gameObject != ParentObject && !gameObject.IsScenery)
				{
					ApplySteam(gameObject);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!E.IgnoreGases && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || E.Actor.PhaseMatches(ParentObject)))
				{
					int num = GasDensityStepped() / 2 + 15;
					if (E.Actor != null)
					{
						int num2 = E.Actor.Stat("HeatResistance");
						if (num2 != 0)
						{
							num = num * (100 - num2) / 100;
						}
					}
					E.MinWeight(num, 65);
				}
			}
			else
			{
				E.MinWeight(5);
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
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || E.Actor.PhaseMatches(ParentObject)))
				{
					int num = GasDensityStepped() / 10 + 3;
					if (E.Actor != null)
					{
						int num2 = E.Actor.Stat("HeatResistance");
						if (num2 != 0)
						{
							num = num * (100 - num2) / 100;
						}
					}
					E.MinWeight(num, 13);
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
		if (E.Type != "Thrown")
		{
			ApplySteam(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DensityChange");
		base.Register(Object, Registrar);
	}

	public void ApplySteam(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && Object != ParentObject)
		{
			Gas part = ParentObject.GetPart<Gas>();
			if (CheckGasCanAffectEvent.Check(Object, ParentObject, part) && Object.PhaseAndFlightMatches(ParentObject) && Object.IsOrganic && (Object.IsCreature || Object.HasPart<Food>()))
			{
				Object.TakeDamage((int)Math.Max(Math.Ceiling(0.18f * (float)part.Density), 1.0), "from %t scalding steam!", "Heat Steam NoBurn", null, null, null, part.Creator, null, null, null, Accidental: false, Environmental: true);
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
