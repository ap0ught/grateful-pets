namespace XRL.World;

[GameEvent(Cascade = 32, Cache = Cache.Pool)]
public class CollectBroadcastChargeEvent : PooledEvent<CollectBroadcastChargeEvent>
{
	public new static readonly int CascadeLevel = 32;

	public GameObject Object;

	public Zone Zone;

	public Cell Cell;

	public int Charge;

	public int MultipleCharge;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Zone = null;
		Cell = null;
		Charge = 0;
		MultipleCharge = 0;
	}

	public static int GetFor(GameObject Object, Zone Zone, Cell Cell, int Charge, int MultipleCharge = 1)
	{
		if (Zone != null)
		{
			CollectBroadcastChargeEvent collectBroadcastChargeEvent = PooledEvent<CollectBroadcastChargeEvent>.FromPool();
			collectBroadcastChargeEvent.Object = Object;
			collectBroadcastChargeEvent.Zone = Zone;
			collectBroadcastChargeEvent.Cell = Cell;
			collectBroadcastChargeEvent.Charge = Charge;
			collectBroadcastChargeEvent.MultipleCharge = MultipleCharge;
			Zone.HandleEvent(collectBroadcastChargeEvent);
			return collectBroadcastChargeEvent.Charge;
		}
		return Charge;
	}

	public static int GetFor(GameObject Object, int Charge, int MultipleCharge = 1)
	{
		Cell currentCell = Object.CurrentCell;
		if (currentCell != null)
		{
			return GetFor(Object, currentCell.ParentZone, currentCell, Charge, MultipleCharge);
		}
		return Charge;
	}
}
