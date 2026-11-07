namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class BeforeTakeActionEvent : SingletonEvent<BeforeTakeActionEvent>
{
	public new static readonly int CascadeLevel = 15;

	public bool PreventAction;

	public static ImmutableEvent registeredInstance = new ImmutableEvent("BeforeTakeAction");

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
		PreventAction = false;
	}

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (flag && Object.HasRegisteredEvent(registeredInstance.ID))
		{
			flag = Object.FireEvent(registeredInstance);
		}
		if (flag)
		{
			SingletonEvent<BeforeTakeActionEvent>.Instance.PreventAction = false;
			flag = Object.HandleEvent(SingletonEvent<BeforeTakeActionEvent>.Instance) && !SingletonEvent<BeforeTakeActionEvent>.Instance.PreventAction;
		}
		return flag;
	}
}
