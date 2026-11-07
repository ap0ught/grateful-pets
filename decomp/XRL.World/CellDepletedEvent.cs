using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CellDepletedEvent : PooledEvent<CellDepletedEvent>
{
	public GameObject Object;

	public IEnergyCell Cell;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Cell = null;
	}

	public static void Send(GameObject Object, IEnergyCell Cell)
	{
		bool flag = Object?.WantEvent(PooledEvent<CellDepletedEvent>.ID, MinEvent.CascadeLevel) ?? false;
		bool flag2 = Cell?.SlottedIn != null && Cell.SlottedIn.WantEvent(PooledEvent<CellDepletedEvent>.ID, MinEvent.CascadeLevel);
		if (flag | flag2)
		{
			CellDepletedEvent cellDepletedEvent = PooledEvent<CellDepletedEvent>.FromPool();
			cellDepletedEvent.Object = Object;
			cellDepletedEvent.Cell = Cell;
			if ((flag && !Object.HandleEvent(cellDepletedEvent)) || (flag2 && !Cell.SlottedIn.HandleEvent(cellDepletedEvent)))
			{
				return;
			}
		}
		bool flag3 = Object?.HasRegisteredEvent("CanBeSlotted") ?? false;
		bool flag4 = Cell?.SlottedIn != null && Cell.SlottedIn.HasRegisteredEvent("CanBeSlotted");
		if (flag3 | flag4)
		{
			Event e = Event.New("CanBeSlotted", "Object", Object, "Cell", Cell);
			if ((!flag3 || Object.FireEvent(e)) && flag4)
			{
				Cell.SlottedIn.FireEvent(e);
			}
		}
	}
}
