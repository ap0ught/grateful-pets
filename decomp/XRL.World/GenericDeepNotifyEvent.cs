namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class GenericDeepNotifyEvent : PooledEvent<GenericDeepNotifyEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Object;

	public GameObject Subject;

	public GameObject Source;

	public string Notify;

	public int Level;

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
		Subject = null;
		Source = null;
		Notify = null;
		Level = 0;
	}

	public static void Send(GameObject Object, string Notify, GameObject Subject = null, GameObject Source = null, int Level = 0)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GenericDeepNotify"))
		{
			Event obj = Event.New("GenericDeepNotify");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Subject", Subject);
			obj.SetParameter("Source", Source);
			obj.SetParameter("Notify", Notify);
			obj.SetParameter("Level", Level);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GenericDeepNotifyEvent>.ID, CascadeLevel))
		{
			GenericDeepNotifyEvent genericDeepNotifyEvent = PooledEvent<GenericDeepNotifyEvent>.FromPool();
			genericDeepNotifyEvent.Object = Object;
			genericDeepNotifyEvent.Subject = Subject;
			genericDeepNotifyEvent.Source = Source;
			genericDeepNotifyEvent.Notify = Notify;
			genericDeepNotifyEvent.Level = Level;
			flag = Object.HandleEvent(genericDeepNotifyEvent);
		}
	}
}
