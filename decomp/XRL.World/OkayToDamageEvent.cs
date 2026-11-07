namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class OkayToDamageEvent : PooledEvent<OkayToDamageEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Object;

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
		Actor = null;
		Object = null;
	}

	public static bool Check(GameObject Object, GameObject Actor, out bool WasWanted)
	{
		WasWanted = false;
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("OkayToDamage"))
		{
			WasWanted = true;
			Event obj = Event.New("OkayToDamage");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Object", Object);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<OkayToDamageEvent>.ID, CascadeLevel))
		{
			WasWanted = true;
			OkayToDamageEvent okayToDamageEvent = PooledEvent<OkayToDamageEvent>.FromPool();
			okayToDamageEvent.Actor = Actor;
			okayToDamageEvent.Object = Object;
			flag = Object.HandleEvent(okayToDamageEvent);
		}
		return flag;
	}

	public static bool Check(GameObject Object, GameObject Actor)
	{
		bool WasWanted;
		return Check(Object, Actor, out WasWanted);
	}
}
