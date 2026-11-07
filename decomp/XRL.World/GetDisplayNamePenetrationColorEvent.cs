namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetDisplayNamePenetrationColorEvent : PooledEvent<GetDisplayNamePenetrationColorEvent>
{
	public GameObject Object;

	public string Color;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Color = null;
	}

	public static string GetFor(GameObject Object, string Default = "c")
	{
		string text = Default;
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetDisplayNamePenetrationColor"))
		{
			Event obj = Event.New("GetDisplayNamePenetrationColor");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Color", text);
			flag = Object.FireEvent(obj);
			text = obj.GetStringParameter("Color");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetDisplayNamePenetrationColorEvent>.ID, MinEvent.CascadeLevel))
		{
			GetDisplayNamePenetrationColorEvent getDisplayNamePenetrationColorEvent = PooledEvent<GetDisplayNamePenetrationColorEvent>.FromPool();
			getDisplayNamePenetrationColorEvent.Object = Object;
			getDisplayNamePenetrationColorEvent.Color = text;
			flag = Object.HandleEvent(getDisplayNamePenetrationColorEvent);
			text = getDisplayNamePenetrationColorEvent.Color;
		}
		return text;
	}
}
