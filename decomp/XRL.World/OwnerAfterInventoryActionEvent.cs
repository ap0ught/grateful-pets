using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class OwnerAfterInventoryActionEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(OwnerAfterInventoryActionEvent), null, CountPool, ResetPool);

	private static List<OwnerAfterInventoryActionEvent> Pool;

	private static int PoolCounter;

	public string Command;

	public bool OwnershipHandled;

	public bool Auto;

	public bool OverrideEnergyCost;

	public bool Forced;

	public bool Silent;

	public int EnergyCostOverride;

	public int MinimumCharge;

	public GameObject ObjectTarget;

	public Cell CellTarget;

	public Cell FromCell;

	public List<GameObject> Generated = new List<GameObject>();

	public OwnerAfterInventoryActionEvent()
	{
		base.ID = ID;
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

	public static void ResetTo(ref OwnerAfterInventoryActionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static OwnerAfterInventoryActionEvent FromPool()
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
		ObjectTarget = null;
		CellTarget = null;
		FromCell = null;
		Generated.Clear();
	}

	public static void Send(GameObject Object, InventoryActionEvent Source)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			OwnerAfterInventoryActionEvent ownerAfterInventoryActionEvent = FromPool();
			ownerAfterInventoryActionEvent.Actor = Source.Actor;
			ownerAfterInventoryActionEvent.Item = Source.Item;
			ownerAfterInventoryActionEvent.Command = Source.Command;
			ownerAfterInventoryActionEvent.Auto = Source.Auto;
			ownerAfterInventoryActionEvent.OwnershipHandled = Source.OwnershipHandled;
			ownerAfterInventoryActionEvent.OverrideEnergyCost = Source.OverrideEnergyCost;
			ownerAfterInventoryActionEvent.Forced = Source.Forced;
			ownerAfterInventoryActionEvent.Silent = Source.Silent;
			ownerAfterInventoryActionEvent.EnergyCostOverride = Source.EnergyCostOverride;
			ownerAfterInventoryActionEvent.MinimumCharge = Source.MinimumCharge;
			ownerAfterInventoryActionEvent.ObjectTarget = Source.ObjectTarget;
			ownerAfterInventoryActionEvent.CellTarget = Source.CellTarget;
			ownerAfterInventoryActionEvent.FromCell = Source.FromCell;
			Object.HandleEvent(ownerAfterInventoryActionEvent);
			Source.ProcessChildEvent(ownerAfterInventoryActionEvent);
			Source.Actor = ownerAfterInventoryActionEvent.Actor;
			Source.Item = ownerAfterInventoryActionEvent.Item;
			Source.Command = ownerAfterInventoryActionEvent.Command;
			Source.Auto = ownerAfterInventoryActionEvent.Auto;
			Source.OwnershipHandled = ownerAfterInventoryActionEvent.OwnershipHandled;
			Source.OverrideEnergyCost = ownerAfterInventoryActionEvent.OverrideEnergyCost;
			Source.Forced = ownerAfterInventoryActionEvent.Forced;
			Source.Silent = ownerAfterInventoryActionEvent.Silent;
			Source.EnergyCostOverride = ownerAfterInventoryActionEvent.EnergyCostOverride;
			Source.MinimumCharge = ownerAfterInventoryActionEvent.MinimumCharge;
			Source.ObjectTarget = ownerAfterInventoryActionEvent.ObjectTarget;
			Source.CellTarget = ownerAfterInventoryActionEvent.CellTarget;
			Source.FromCell = ownerAfterInventoryActionEvent.FromCell;
		}
	}

	public static void Send(ref bool InterfaceExitRequested, GameObject Object, InventoryActionEvent Source)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			OwnerAfterInventoryActionEvent ownerAfterInventoryActionEvent = FromPool();
			ownerAfterInventoryActionEvent.Actor = Source.Actor;
			ownerAfterInventoryActionEvent.Item = Source.Item;
			ownerAfterInventoryActionEvent.Command = Source.Command;
			ownerAfterInventoryActionEvent.Auto = Source.Auto;
			ownerAfterInventoryActionEvent.OwnershipHandled = Source.OwnershipHandled;
			ownerAfterInventoryActionEvent.OverrideEnergyCost = Source.OverrideEnergyCost;
			ownerAfterInventoryActionEvent.Forced = Source.Forced;
			ownerAfterInventoryActionEvent.Silent = Source.Silent;
			ownerAfterInventoryActionEvent.EnergyCostOverride = Source.EnergyCostOverride;
			ownerAfterInventoryActionEvent.MinimumCharge = Source.MinimumCharge;
			ownerAfterInventoryActionEvent.ObjectTarget = Source.ObjectTarget;
			ownerAfterInventoryActionEvent.CellTarget = Source.CellTarget;
			ownerAfterInventoryActionEvent.FromCell = Source.FromCell;
			Object.HandleEvent(ownerAfterInventoryActionEvent);
			Source.ProcessChildEvent(ownerAfterInventoryActionEvent);
			Source.Actor = ownerAfterInventoryActionEvent.Actor;
			Source.Item = ownerAfterInventoryActionEvent.Item;
			Source.Command = ownerAfterInventoryActionEvent.Command;
			Source.Auto = ownerAfterInventoryActionEvent.Auto;
			Source.OwnershipHandled = ownerAfterInventoryActionEvent.OwnershipHandled;
			Source.OverrideEnergyCost = ownerAfterInventoryActionEvent.OverrideEnergyCost;
			Source.Forced = ownerAfterInventoryActionEvent.Forced;
			Source.Silent = ownerAfterInventoryActionEvent.Silent;
			Source.EnergyCostOverride = ownerAfterInventoryActionEvent.EnergyCostOverride;
			Source.MinimumCharge = ownerAfterInventoryActionEvent.MinimumCharge;
			Source.ObjectTarget = ownerAfterInventoryActionEvent.ObjectTarget;
			Source.CellTarget = ownerAfterInventoryActionEvent.CellTarget;
			Source.FromCell = ownerAfterInventoryActionEvent.FromCell;
			if (ownerAfterInventoryActionEvent.InterfaceExitRequested())
			{
				InterfaceExitRequested = true;
			}
		}
	}
}
