using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 256, Cache = Cache.Pool)]
public class InventoryActionEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(InventoryActionEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 256;

	private static List<InventoryActionEvent> Pool;

	private static int PoolCounter;

	public string Command;

	public bool OwnershipHandled;

	public bool Auto;

	public bool OverrideEnergyCost;

	public bool Forced;

	public bool Silent;

	public int EnergyCostOverride;

	public int MinimumCharge;

	public int StandoffDistance;

	public GameObject ObjectTarget;

	public Cell CellTarget;

	public Cell FromCell;

	public IInventory InventoryTarget;

	public List<GameObject> Generated = new List<GameObject>();

	public InventoryActionEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref InventoryActionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static InventoryActionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Command = null;
		OwnershipHandled = false;
		Auto = false;
		OverrideEnergyCost = false;
		Forced = false;
		Silent = false;
		EnergyCostOverride = 0;
		MinimumCharge = 0;
		StandoffDistance = 0;
		ObjectTarget = null;
		CellTarget = null;
		FromCell = null;
		InventoryTarget = null;
		Generated.Clear();
	}

	public static bool Check(GameObject Object, GameObject Actor, GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false, bool OverrideEnergyCost = false, bool Forced = false, bool Silent = false, int EnergyCostOverride = 0, int MinimumCharge = 0, int StandoffDistance = 0, GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null, IInventory InventoryTarget = null)
	{
		if (GameObject.Validate(ref Object))
		{
			InventoryActionEvent inventoryActionEvent = FromPool();
			inventoryActionEvent.Actor = Actor;
			inventoryActionEvent.Item = Item;
			inventoryActionEvent.Command = Command;
			inventoryActionEvent.Auto = Auto;
			inventoryActionEvent.OwnershipHandled = OwnershipHandled;
			inventoryActionEvent.OverrideEnergyCost = OverrideEnergyCost;
			inventoryActionEvent.Forced = Forced;
			inventoryActionEvent.Silent = Silent;
			inventoryActionEvent.EnergyCostOverride = EnergyCostOverride;
			inventoryActionEvent.MinimumCharge = MinimumCharge;
			inventoryActionEvent.StandoffDistance = StandoffDistance;
			inventoryActionEvent.ObjectTarget = ObjectTarget;
			inventoryActionEvent.CellTarget = CellTarget;
			inventoryActionEvent.FromCell = FromCell;
			inventoryActionEvent.InventoryTarget = InventoryTarget;
			if (!Object.HandleEvent(inventoryActionEvent))
			{
				return false;
			}
			AfterInventoryActionEvent.Send(Object, inventoryActionEvent);
			OwnerAfterInventoryActionEvent.Send(Actor, inventoryActionEvent);
		}
		return true;
	}

	public static bool Check(ref bool InterfaceExitRequested, GameObject Object, GameObject Actor, GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false, bool OverrideEnergyCost = false, bool Forced = false, bool Silent = false, int EnergyCostOverride = 0, int MinimumCharge = 0, int StandoffDistance = 0, GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null, IInventory InventoryTarget = null)
	{
		if (GameObject.Validate(ref Object))
		{
			InventoryActionEvent inventoryActionEvent = FromPool();
			inventoryActionEvent.Actor = Actor;
			inventoryActionEvent.Item = Item;
			inventoryActionEvent.Command = Command;
			inventoryActionEvent.Auto = Auto;
			inventoryActionEvent.OwnershipHandled = OwnershipHandled;
			inventoryActionEvent.OverrideEnergyCost = OverrideEnergyCost;
			inventoryActionEvent.Forced = Forced;
			inventoryActionEvent.Silent = Silent;
			inventoryActionEvent.EnergyCostOverride = EnergyCostOverride;
			inventoryActionEvent.MinimumCharge = MinimumCharge;
			inventoryActionEvent.StandoffDistance = StandoffDistance;
			inventoryActionEvent.ObjectTarget = ObjectTarget;
			inventoryActionEvent.CellTarget = CellTarget;
			inventoryActionEvent.FromCell = FromCell;
			inventoryActionEvent.InventoryTarget = InventoryTarget;
			bool flag = Object.HandleEvent(inventoryActionEvent);
			if (inventoryActionEvent.InterfaceExitRequested())
			{
				InterfaceExitRequested = true;
			}
			if (!flag)
			{
				return false;
			}
			AfterInventoryActionEvent.Send(ref InterfaceExitRequested, Object, inventoryActionEvent);
			OwnerAfterInventoryActionEvent.Send(ref InterfaceExitRequested, Actor, inventoryActionEvent);
		}
		return true;
	}

	public static bool Check(out IEvent GeneratedEvent, GameObject Object, GameObject Actor, GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false, bool OverrideEnergyCost = false, bool Forced = false, bool Silent = false, int EnergyCostOverride = 0, int MinimumCharge = 0, int StandoffDistance = 0, GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null, IInventory InventoryTarget = null)
	{
		if (GameObject.Validate(ref Object))
		{
			InventoryActionEvent inventoryActionEvent = FromPool();
			inventoryActionEvent.Actor = Actor;
			inventoryActionEvent.Item = Item;
			inventoryActionEvent.Command = Command;
			inventoryActionEvent.Auto = Auto;
			inventoryActionEvent.OwnershipHandled = OwnershipHandled;
			inventoryActionEvent.OverrideEnergyCost = OverrideEnergyCost;
			inventoryActionEvent.Forced = Forced;
			inventoryActionEvent.Silent = Silent;
			inventoryActionEvent.EnergyCostOverride = EnergyCostOverride;
			inventoryActionEvent.MinimumCharge = MinimumCharge;
			inventoryActionEvent.ObjectTarget = ObjectTarget;
			inventoryActionEvent.CellTarget = CellTarget;
			inventoryActionEvent.StandoffDistance = StandoffDistance;
			inventoryActionEvent.FromCell = FromCell;
			inventoryActionEvent.InventoryTarget = InventoryTarget;
			GeneratedEvent = inventoryActionEvent;
			if (!Object.HandleEvent(inventoryActionEvent))
			{
				return false;
			}
			AfterInventoryActionEvent.Send(Object, inventoryActionEvent);
			OwnerAfterInventoryActionEvent.Send(Actor, inventoryActionEvent);
		}
		else
		{
			GeneratedEvent = null;
		}
		return true;
	}

	public static bool Check(out InventoryActionEvent E, GameObject Object, GameObject Actor, GameObject Item, string Command, bool Auto = false, bool OwnershipHandled = false, bool OverrideEnergyCost = false, bool Forced = false, bool Silent = false, int EnergyCostOverride = 0, int MinimumCharge = 0, int StandoffDistance = 0, GameObject ObjectTarget = null, Cell CellTarget = null, Cell FromCell = null, IInventory InventoryTarget = null)
	{
		if (GameObject.Validate(ref Object))
		{
			E = FromPool();
			E.Actor = Actor;
			E.Item = Item;
			E.Command = Command;
			E.Auto = Auto;
			E.OwnershipHandled = OwnershipHandled;
			E.OverrideEnergyCost = OverrideEnergyCost;
			E.Forced = Forced;
			E.Silent = Silent;
			E.EnergyCostOverride = EnergyCostOverride;
			E.MinimumCharge = MinimumCharge;
			E.StandoffDistance = StandoffDistance;
			E.ObjectTarget = ObjectTarget;
			E.CellTarget = CellTarget;
			E.FromCell = FromCell;
			E.InventoryTarget = InventoryTarget;
			if (!Object.HandleEvent(E))
			{
				return false;
			}
			AfterInventoryActionEvent.Send(Object, E);
			OwnerAfterInventoryActionEvent.Send(Actor, E);
		}
		else
		{
			E = null;
		}
		return true;
	}
}
