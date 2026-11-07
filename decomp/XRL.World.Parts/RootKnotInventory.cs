using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class RootKnotInventory : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ZoneFreezing");
		Registrar.Register(PooledEvent<GetFeelingEvent>.ID);
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneFreezing")
		{
			RemoveState();
		}
		return base.FireEvent(E);
	}

	public void RemoveState()
	{
		ParentObject.Inventory.ClearOnDeath = true;
		ParentObject.Inventory.Objects = new List<GameObject>();
		ParentObject.Brain.SetPartyLeader(null, 0, Transient: false, Silent: true);
	}

	public void SetState()
	{
		Inventory inventory = ParentObject.Inventory;
		inventory.ClearOnDeath = false;
		inventory.Objects = The.Game.RequireSystem(() => new RootKnotSystem()).inventory;
		if (inventory.Objects.Count == 0)
		{
			inventory.CheckEmptyState();
		}
		else
		{
			inventory.CheckNonEmptyState();
		}
		if (ParentObject.Brain.PartyLeader != The.Player)
		{
			ParentObject.Brain.SetPartyLeader(The.Player, 0, Transient: true, Silent: true);
			ParentObject.Brain.ClearHostileMemory();
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != ZoneActivatedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		RemoveState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		SetState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		SetState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		SetState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetFeelingEvent E)
	{
		E.Feeling = 100;
		return false;
	}
}
