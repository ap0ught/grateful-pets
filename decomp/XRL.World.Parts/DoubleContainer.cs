using System;

namespace XRL.World.Parts;

[Serializable]
public class DoubleContainer : IPart
{
	public string Direction;

	public bool Master;

	[NonSerialized]
	public GameObject Suspended;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != LeftCellEvent.ID && ID != EnteredCellEvent.ID && ID != GetInventoryActionsEvent.ID && ID != BeforeDeathRemovalEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		Suspended = GetSibling(E.Cell);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (GameObject.Validate(ref Suspended))
		{
			GameObject suspended = Suspended;
			Cell cell = suspended.CurrentCell;
			Cell cellFromDirection = E.Cell.GetCellFromDirection(Direction, BuiltOnly: false);
			if (cellFromDirection.IsPassable(suspended, IncludeCombatObjects: false))
			{
				cell?.RemoveObject(suspended);
				cellFromDirection.AddObject(suspended);
			}
			else
			{
				ParentObject.Die(E.Dragging, null, "You broke apart.");
				suspended.Destroy("You broke apart.");
			}
			Suspended = null;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		GetSibling()?.Destroy("You broke apart.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Open", "open", "Open", null, 'o');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		if (Master)
		{
			SyncContentsToSibling();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EncumbranceChanged");
		base.Register(Object, Registrar);
	}

	public GameObject GetSibling(Cell C = null)
	{
		return (C ?? ParentObject.CurrentCell)?.GetCellFromDirection(Direction).GetFirstObjectWithPart("DoubleContainer");
	}

	public void SyncContentsToSibling()
	{
		GameObject sibling = GetSibling();
		if (sibling != null)
		{
			sibling.Inventory.Objects.Clear();
			sibling.Inventory.Objects.AddRange(ParentObject.Inventory.Objects);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EncumbranceChanged")
		{
			SyncContentsToSibling();
			return true;
		}
		return base.FireEvent(E);
	}
}
