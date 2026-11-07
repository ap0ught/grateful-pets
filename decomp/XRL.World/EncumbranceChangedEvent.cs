namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class EncumbranceChangedEvent : PooledEvent<EncumbranceChangedEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Actor;

	public GameObject Object;

	public bool Silent;

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
		Silent = false;
	}

	public static bool Send(GameObject Actor, GameObject Object = null, bool Silent = false)
	{
		bool flag = true;
		if (GameObject.Validate(Actor) && Actor.HasRegisteredEvent("EncumbranceChanged"))
		{
			Event obj = Event.New("EncumbranceChanged");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Object", Object);
			obj.SetSilent(Silent);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(Actor) && Actor.WantEvent(PooledEvent<EncumbranceChangedEvent>.ID, CascadeLevel))
		{
			EncumbranceChangedEvent E = PooledEvent<EncumbranceChangedEvent>.FromPool();
			E.Actor = Actor;
			E.Object = Object;
			E.Silent = Silent;
			flag = Actor.HandleEvent(E);
			PooledEvent<EncumbranceChangedEvent>.ResetTo(ref E);
		}
		return flag;
	}
}
