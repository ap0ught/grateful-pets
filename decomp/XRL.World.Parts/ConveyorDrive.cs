using System;

namespace XRL.World.Parts;

[Serializable]
public class ConveyorDrive : IPart
{
	public int TurnsPerImpulse = 2;

	public int CurrentTurn;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" || E.ID == "EnteredCell")
		{
			if (IsEMPed() || IsBroken() || IsRusted())
			{
				return true;
			}
			CurrentTurn++;
			if (CurrentTurn >= TurnsPerImpulse)
			{
				CurrentTurn = 0;
				if (ParentObject.Physics.CurrentCell != null)
				{
					foreach (Cell cardinalAdjacentCell in ParentObject.Physics.CurrentCell.GetCardinalAdjacentCells())
					{
						foreach (GameObject item in cardinalAdjacentCell.GetObjectsWithPartReadonly("ConveyorPad"))
						{
							item.GetPart<ConveyorPad>().ConveyorImpulse(60, Event.NewGameObjectList());
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
