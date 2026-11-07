namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class EndActionEvent : SingletonEvent<EndActionEvent>
{
	public new static readonly int CascadeLevel = 15;

	public static ImmutableEvent registeredInstance = new ImmutableEvent("EndAction");

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public static void Send(GameObject Object)
	{
		bool flag = true;
		if (flag && Object.HasRegisteredEvent(registeredInstance.ID))
		{
			flag = Object.FireEvent(registeredInstance);
		}
		if (flag)
		{
			flag = Object.HandleEvent(SingletonEvent<EndActionEvent>.Instance);
		}
	}
}
