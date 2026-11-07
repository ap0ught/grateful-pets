namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class EarlyBeforeBeginTakeActionEvent : SingletonEvent<EarlyBeforeBeginTakeActionEvent>
{
	public new static readonly int CascadeLevel = 15;

	public bool PreventAction;

	public static ImmutableEvent registeredInstance = new ImmutableEvent("EarlyBeforeBeginTakeAction");

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
			SingletonEvent<EarlyBeforeBeginTakeActionEvent>.Instance.PreventAction = false;
			flag = Object.HandleEvent(SingletonEvent<EarlyBeforeBeginTakeActionEvent>.Instance) && !SingletonEvent<EarlyBeforeBeginTakeActionEvent>.Instance.PreventAction;
		}
		return flag;
	}
}
