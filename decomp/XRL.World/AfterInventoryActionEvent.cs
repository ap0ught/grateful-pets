using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterInventoryActionEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterInventoryActionEvent), null, CountPool, ResetPool);

	private static List<AfterInventoryActionEvent> Pool;

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

	public AfterInventoryActionEvent()
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

	public static void ResetTo(ref AfterInventoryActionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterInventoryActionEvent FromPool()
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
			AfterInventoryActionEvent afterInventoryActionEvent = FromPool();
			afterInventoryActionEvent.Actor = Source.Actor;
			afterInventoryActionEvent.Item = Source.Item;
			afterInventoryActionEvent.Command = Source.Command;
			afterInventoryActionEvent.Auto = Source.Auto;
			afterInventoryActionEvent.OwnershipHandled = Source.OwnershipHandled;
			afterInventoryActionEvent.OverrideEnergyCost = Source.OverrideEnergyCost;
			afterInventoryActionEvent.Forced = Source.Forced;
			afterInventoryActionEvent.Silent = Source.Silent;
			afterInventoryActionEvent.EnergyCostOverride = Source.EnergyCostOverride;
			afterInventoryActionEvent.MinimumCharge = Source.MinimumCharge;
			afterInventoryActionEvent.ObjectTarget = Source.ObjectTarget;
			afterInventoryActionEvent.CellTarget = Source.CellTarget;
			afterInventoryActionEvent.FromCell = Source.FromCell;
			Object.HandleEvent(afterInventoryActionEvent);
			Source.ProcessChildEvent(afterInventoryActionEvent);
			Source.Actor = afterInventoryActionEvent.Actor;
			Source.Item = afterInventoryActionEvent.Item;
			Source.Command = afterInventoryActionEvent.Command;
			Source.Auto = afterInventoryActionEvent.Auto;
			Source.OwnershipHandled = afterInventoryActionEvent.OwnershipHandled;
			Source.OverrideEnergyCost = afterInventoryActionEvent.OverrideEnergyCost;
			Source.Forced = afterInventoryActionEvent.Forced;
			Source.Silent = afterInventoryActionEvent.Silent;
			Source.EnergyCostOverride = afterInventoryActionEvent.EnergyCostOverride;
			Source.MinimumCharge = afterInventoryActionEvent.MinimumCharge;
			Source.ObjectTarget = afterInventoryActionEvent.ObjectTarget;
			Source.CellTarget = afterInventoryActionEvent.CellTarget;
			Source.FromCell = afterInventoryActionEvent.FromCell;
		}
	}

	public static void Send(ref bool InterfaceExitRequested, GameObject Object, InventoryActionEvent Source)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AfterInventoryActionEvent afterInventoryActionEvent = FromPool();
			afterInventoryActionEvent.Actor = Source.Actor;
			afterInventoryActionEvent.Item = Source.Item;
			afterInventoryActionEvent.Command = Source.Command;
			afterInventoryActionEvent.Auto = Source.Auto;
			afterInventoryActionEvent.OwnershipHandled = Source.OwnershipHandled;
			afterInventoryActionEvent.OverrideEnergyCost = Source.OverrideEnergyCost;
			afterInventoryActionEvent.Forced = Source.Forced;
			afterInventoryActionEvent.Silent = Source.Silent;
			afterInventoryActionEvent.EnergyCostOverride = Source.EnergyCostOverride;
			afterInventoryActionEvent.MinimumCharge = Source.MinimumCharge;
			afterInventoryActionEvent.ObjectTarget = Source.ObjectTarget;
			afterInventoryActionEvent.CellTarget = Source.CellTarget;
			afterInventoryActionEvent.FromCell = Source.FromCell;
			Object.HandleEvent(afterInventoryActionEvent);
			Source.ProcessChildEvent(afterInventoryActionEvent);
			Source.Actor = afterInventoryActionEvent.Actor;
			Source.Item = afterInventoryActionEvent.Item;
			Source.Command = afterInventoryActionEvent.Command;
			Source.Auto = afterInventoryActionEvent.Auto;
			Source.OwnershipHandled = afterInventoryActionEvent.OwnershipHandled;
			Source.OverrideEnergyCost = afterInventoryActionEvent.OverrideEnergyCost;
			Source.Forced = afterInventoryActionEvent.Forced;
			Source.Silent = afterInventoryActionEvent.Silent;
			Source.EnergyCostOverride = afterInventoryActionEvent.EnergyCostOverride;
			Source.MinimumCharge = afterInventoryActionEvent.MinimumCharge;
			Source.ObjectTarget = afterInventoryActionEvent.ObjectTarget;
			Source.CellTarget = afterInventoryActionEvent.CellTarget;
			Source.FromCell = afterInventoryActionEvent.FromCell;
			if (afterInventoryActionEvent.InterfaceExitRequested())
			{
				InterfaceExitRequested = true;
			}
		}
	}
}
