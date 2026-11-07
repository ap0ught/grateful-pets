namespace XRL.World;

[GameEvent(Cascade = 143, Cache = Cache.Singleton)]
public class BeginTakeActionEvent : SingletonEvent<BeginTakeActionEvent>
{
	public new static readonly int CascadeLevel = 143;

	public GameObject Object;

	public bool Traveling;

	public bool TravelMessagesSuppressed;

	public bool PreventAction;

	public static ImmutableEvent registeredInstance = new ImmutableEvent("BeginTakeAction");

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
		Traveling = false;
		TravelMessagesSuppressed = false;
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
			SingletonEvent<BeginTakeActionEvent>.Instance.Object = Object;
			SingletonEvent<BeginTakeActionEvent>.Instance.Traveling = false;
			SingletonEvent<BeginTakeActionEvent>.Instance.TravelMessagesSuppressed = false;
			SingletonEvent<BeginTakeActionEvent>.Instance.PreventAction = false;
			flag = Object.HandleEvent(SingletonEvent<BeginTakeActionEvent>.Instance) && !SingletonEvent<BeginTakeActionEvent>.Instance.PreventAction;
		}
		return flag;
	}

	public static bool Check(GameObject Object, bool Traveling, ref bool TravelMessagesSuppressed)
	{
		bool flag = true;
		if (flag && Object.HasRegisteredEvent(registeredInstance.ID))
		{
			flag = Object.FireEvent(registeredInstance);
		}
		if (flag)
		{
			SingletonEvent<BeginTakeActionEvent>.Instance.Object = Object;
			SingletonEvent<BeginTakeActionEvent>.Instance.Traveling = Traveling;
			SingletonEvent<BeginTakeActionEvent>.Instance.TravelMessagesSuppressed = TravelMessagesSuppressed;
			SingletonEvent<BeginTakeActionEvent>.Instance.PreventAction = false;
			flag = Object.HandleEvent(SingletonEvent<BeginTakeActionEvent>.Instance) && !SingletonEvent<BeginTakeActionEvent>.Instance.PreventAction;
			TravelMessagesSuppressed = SingletonEvent<BeginTakeActionEvent>.Instance.TravelMessagesSuppressed;
		}
		return flag;
	}
}
