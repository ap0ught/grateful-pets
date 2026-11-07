using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 143, Cache = Cache.Pool)]
public class GetRecoilersEvent : PooledEvent<GetRecoilersEvent>
{
	public new static readonly int CascadeLevel = 143;

	public GameObject Actor;

	public List<GameObject> Objects;

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
		Objects = null;
	}

	public static List<GameObject> GetFor(GameObject Actor)
	{
		bool flag = true;
		List<GameObject> list = Event.NewGameObjectList();
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetRecoilers"))
		{
			Event obj = Event.New("GetRecoilers");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Objects", list);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetRecoilersEvent>.ID, CascadeLevel))
		{
			GetRecoilersEvent getRecoilersEvent = PooledEvent<GetRecoilersEvent>.FromPool();
			getRecoilersEvent.Actor = Actor;
			getRecoilersEvent.Objects = list;
			flag = Actor.HandleEvent(getRecoilersEvent);
		}
		return list;
	}
}
