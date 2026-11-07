namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetMissileStatusColorEvent : PooledEvent<GetMissileStatusColorEvent>
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

	public static GetMissileStatusColorEvent FromPool(GameObject Object, string Color = null)
	{
		GetMissileStatusColorEvent getMissileStatusColorEvent = PooledEvent<GetMissileStatusColorEvent>.FromPool();
		getMissileStatusColorEvent.Object = Object;
		getMissileStatusColorEvent.Color = Color;
		return getMissileStatusColorEvent;
	}

	public static string GetFor(GameObject Object, string Color = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetMissileStatusColor"))
		{
			Event obj = Event.New("GetMissileStatusColor");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Color", Color);
			flag = Object.FireEvent(obj);
			Color = obj.GetStringParameter("Color");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetMissileStatusColorEvent>.ID, MinEvent.CascadeLevel))
		{
			GetMissileStatusColorEvent getMissileStatusColorEvent = FromPool(Object, Color);
			flag = Object.HandleEvent(getMissileStatusColorEvent);
			Color = getMissileStatusColorEvent.Color;
		}
		return Color ?? Object.DisplayNameColor;
	}
}
