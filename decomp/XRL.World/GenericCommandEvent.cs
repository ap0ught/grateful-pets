namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GenericCommandEvent : PooledEvent<GenericCommandEvent>
{
	public string Command;

	public string Type;

	public object Object;

	public object Source;

	public int Level;

	public int Flags;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Command = null;
		Type = null;
		Object = null;
		Source = null;
		Level = 0;
		Flags = 0;
	}

	public static void Send(IEventSource EventSource, string Command, string Type = null, object Object = null, object Source = null, int Level = 0, int Flags = 0)
	{
		GenericCommandEvent E = PooledEvent<GenericCommandEvent>.FromPool();
		E.Command = Command;
		E.Type = Type;
		E.Object = Object;
		E.Source = Source;
		E.Level = Level;
		E.Flags = Flags;
		EventSource.HandleEvent(E);
		PooledEvent<GenericCommandEvent>.ResetTo(ref E);
	}
}
