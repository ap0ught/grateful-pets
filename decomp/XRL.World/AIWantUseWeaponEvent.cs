namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AIWantUseWeaponEvent : PooledEvent<AIWantUseWeaponEvent>
{
	public GameObject Actor;

	public GameObject Object;

	public GameObject Target;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Object = null;
		Target = null;
	}

	public static bool Check(GameObject Object, GameObject Actor, GameObject Target)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AIWantUseWeapon"))
		{
			Event obj = Event.New("AIWantUseWeapon");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<AIWantUseWeaponEvent>.ID, MinEvent.CascadeLevel))
		{
			AIWantUseWeaponEvent aIWantUseWeaponEvent = PooledEvent<AIWantUseWeaponEvent>.FromPool();
			aIWantUseWeaponEvent.Object = Object;
			aIWantUseWeaponEvent.Actor = Actor;
			aIWantUseWeaponEvent.Target = Target;
			flag = Object.HandleEvent(aIWantUseWeaponEvent);
		}
		return flag;
	}
}
