using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class GasCryo : IGasBehavior
{
	public string GasType = "Cryo";

	public override bool SameAs(IPart p)
	{
		if ((p as GasCryo).GasType != GasType)
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
		if (!E.IgnoreGases && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || E.Actor.PhaseMatches(ParentObject)))
				{
					int num = GasDensityStepped() / 2 + 3;
					if (E.Actor != null)
					{
						int num2 = E.Actor.Stat("ColdResistance");
						if (num2 != 0)
						{
							num = Math.Max(num * (100 - num2) / 100, 0);
						}
					}
					if (num > 0)
					{
						E.MinWeight(num, 53);
					}
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
		if (!E.IgnoreGases && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || E.Actor.PhaseMatches(ParentObject)))
				{
					int num = GasDensityStepped() / 10 + 1;
					if (E.Actor != null)
					{
						int num2 = E.Actor.Stat("ColdResistance");
						if (num2 != 0)
						{
							num = Math.Max(num * (100 - num2) / 100, 0);
						}
					}
					if (num > 0)
					{
						E.MinWeight(num, 11);
					}
				}
			}
			else
			{
				E.MinWeight(1);
			}
		}
		return base.HandleEvent(E);
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
					ApplyCryo(gameObject);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object != ParentObject && E.Type != "Thrown")
		{
			ApplyCryo(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DensityChange");
		base.Register(Object, Registrar);
	}

	public void ApplyCryo(GameObject GO)
	{
		Gas part = ParentObject.GetPart<Gas>();
		if (CheckGasCanAffectEvent.Check(GO, ParentObject, part) && GO.PhaseMatches(ParentObject))
		{
			Event.PinCurrentPool();
			int num = (int)Math.Ceiling(2.5f * (float)part.Density);
			if (GO.Physics.Temperature > -num)
			{
				GO.TemperatureChange(-num, ParentObject, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, ParentObject.GetPhase());
			}
			if (GO.IsPlayer() || !GO.IsFrozen())
			{
				GO.TakeDamage(1, "from the {{icy|cryogenic mist}}.", "Cold", null, null, part.Creator, null, ParentObject, null, null, Accidental: false, Environmental: true);
			}
			Event.ResetToPin();
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
