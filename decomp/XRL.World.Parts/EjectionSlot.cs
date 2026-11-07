using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class EjectionSlot : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneBuiltEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		LockSeats(ParentObject.CurrentCell, Silent: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		LockSeats(E.Cell, !E.Cell.ParentZone.Built);
		return base.HandleEvent(E);
	}

	public void LockSeats(Cell Cell, bool Silent = false)
	{
		List<GameObject> objectsWithPartReadonly = Cell.GetObjectsWithPartReadonly("EjectionSeat");
		if (objectsWithPartReadonly.IsNullOrEmpty())
		{
			return;
		}
		foreach (GameObject item in objectsWithPartReadonly)
		{
			if (!item.Physics.Takeable)
			{
				return;
			}
		}
		GameObject gameObject = objectsWithPartReadonly[objectsWithPartReadonly.Count - 1];
		gameObject.Physics.Takeable = false;
		if (!Silent)
		{
			gameObject.Physics.DidX("lock", "in place");
			gameObject.PlayWorldSound("Sounds/Robot/sfx_turret_deploy");
		}
	}
}
