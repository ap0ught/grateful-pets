namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class SyncMutationLevelsEvent : PooledEvent<SyncMutationLevelsEvent>
{
	public GameObject Object;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
	}

	public static void Send(GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("SyncMutationLevels"))
		{
			Event obj = Event.New("SyncMutationLevels");
			obj.SetParameter("Object", Object);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<SyncMutationLevelsEvent>.ID, MinEvent.CascadeLevel))
		{
			SyncMutationLevelsEvent syncMutationLevelsEvent = PooledEvent<SyncMutationLevelsEvent>.FromPool();
			syncMutationLevelsEvent.Object = Object;
			flag = Object.HandleEvent(syncMutationLevelsEvent);
		}
	}
}
