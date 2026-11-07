namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class BlocksRadarEvent : PooledEvent<BlocksRadarEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

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
	}

	public static BlocksRadarEvent FromPool(GameObject Object)
	{
		BlocksRadarEvent blocksRadarEvent = PooledEvent<BlocksRadarEvent>.FromPool();
		blocksRadarEvent.Object = Object;
		return blocksRadarEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BlocksRadar"))
		{
			Event obj = Event.New("BlocksRadar");
			obj.SetParameter("Object", Object);
			if (!Object.FireEvent(obj))
			{
				return true;
			}
		}
		if (GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<BlocksRadarEvent>.ID, CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return true;
		}
		return false;
	}

	public static bool Check(Cell C)
	{
		for (int num = C.Objects.Count - 1; num >= 0; num--)
		{
			if (Check(C.Objects[num]))
			{
				return true;
			}
		}
		return false;
	}
}
