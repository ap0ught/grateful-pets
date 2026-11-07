using System;

namespace XRL.World.Parts;

[Serializable]
public class StationMoverChute : IPart
{
	public string Tag = "MoverChute";

	public string Direction = "N";

	public string Mount = "E";

	public override bool WantEvent(int ID, int Cascade)
	{
		if (ID != PooledEvent<GetTransitiveLocationEvent>.ID && ID != ZoneBuiltEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTransitiveLocationEvent E)
	{
		if (E.IsPullDown && E.IsWorldMap && E.Actor.HasPart<StationMover>())
		{
			Cell localCellFromDirection = ParentObject.CurrentCell;
			int num = 1000;
			for (int i = 0; i < 100; i++)
			{
				GameObject gameObject = localCellFromDirection?.GetFirstObjectWithPropertyOrTag(Tag);
				if (gameObject == null)
				{
					break;
				}
				Cell localCellFromDirection2 = localCellFromDirection.GetLocalCellFromDirection(Mount);
				if (localCellFromDirection2 != null && localCellFromDirection2.IsEmpty())
				{
					E.AddLocation(localCellFromDirection2, gameObject, num);
				}
				localCellFromDirection = localCellFromDirection.GetLocalCellFromDirection(Direction);
				num -= 25;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		RequireChuteBlocker();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		RequireChuteBlocker();
		return base.HandleEvent(E);
	}

	public void RequireChuteBlocker()
	{
		Cell cell = ParentObject.CurrentCell?.GetLocalCellFromDirection(Mount);
		if (cell != null)
		{
			GameObject gameObject = cell.RequireObject("Widget");
			gameObject.RegisterEvent(this, ObjectEnteringCellEvent.ID);
			gameObject.RegisterEvent(this, GetNavigationWeightEvent.ID);
		}
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		Cell cell = ParentObject.CurrentCell?.GetLocalCellFromDirection(Mount);
		if (!E.System && !E.Forced && cell == E.Cell && E.Object != null && !E.Object.HasPart<StationMover>())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		E.MinWeight(100);
		return base.HandleEvent(E);
	}

	public static string GetAdjacentDirection(Cell Cell)
	{
		foreach (Cell item in Cell.IterateAdjacent(1, IncludeSelf: false, LocalOnly: true))
		{
			foreach (GameObject @object in item.Objects)
			{
				if (@object.TryGetPart<StationMoverChute>(out var Part))
				{
					return Part.Direction;
				}
			}
		}
		return null;
	}
}
