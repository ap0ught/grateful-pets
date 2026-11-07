namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckAttackableEvent : PooledEvent<CheckAttackableEvent>
{
	public GameObject Object;

	public GameObject Attacker;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Attacker = null;
	}

	public static CheckAttackableEvent FromPool(GameObject Object, GameObject Attacker)
	{
		CheckAttackableEvent checkAttackableEvent = PooledEvent<CheckAttackableEvent>.FromPool();
		checkAttackableEvent.Object = Object;
		checkAttackableEvent.Attacker = Attacker;
		return checkAttackableEvent;
	}

	public static bool Check(GameObject Object, GameObject Attacker)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckAttackableEvent>.ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, Attacker)))
		{
			return false;
		}
		return true;
	}
}
