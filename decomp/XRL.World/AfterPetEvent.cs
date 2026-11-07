namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class AfterPetEvent : PooledEvent<AfterPetEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public GameObject Actor;

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
		Actor = null;
	}

	public static void Send(GameObject Object, GameObject Actor)
	{
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AfterPet");
			bool flag3 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AfterPet");
			if (flag2 | flag3)
			{
				Event obj = Event.New("AfterPet");
				obj.SetParameter("Object", Object);
				obj.SetParameter("Actor", Actor);
				flag = (!flag2 || Object.FireEvent(obj)) && (!flag3 || Actor.FireEvent(obj));
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<AfterPetEvent>.ID, CascadeLevel);
			bool flag5 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<AfterPetEvent>.ID, CascadeLevel);
			if (flag4 | flag5)
			{
				AfterPetEvent afterPetEvent = PooledEvent<AfterPetEvent>.FromPool();
				afterPetEvent.Object = Object;
				afterPetEvent.Actor = Actor;
				flag = (!flag4 || Object.HandleEvent(afterPetEvent)) && (!flag5 || Actor.HandleEvent(afterPetEvent));
			}
		}
	}
}
