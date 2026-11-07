namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GenericNotifyEvent : PooledEvent<GenericNotifyEvent>
{
	public GameObject Object;

	public GameObject Subject;

	public GameObject Source;

	public string Notify;

	public int Level;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Subject = null;
		Source = null;
		Notify = null;
		Level = 0;
	}

	public static void Send(GameObject Object, string Notify, GameObject Subject = null, GameObject Source = null, int Level = 0)
	{
		GenericNotifyEvent genericNotifyEvent = PooledEvent<GenericNotifyEvent>.FromPool();
		genericNotifyEvent.Object = Object;
		genericNotifyEvent.Subject = Subject;
		genericNotifyEvent.Source = Source;
		genericNotifyEvent.Notify = Notify;
		genericNotifyEvent.Level = Level;
		Object.HandleEvent(genericNotifyEvent);
		The.Game.HandleEvent(genericNotifyEvent);
	}
}
