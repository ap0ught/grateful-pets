namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetFirefightingPerformanceEvent : PooledEvent<GetFirefightingPerformanceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public const int FIREFIGHTING_BASE_PERFORMANCE = -100;

	public const int FIREFIGHTING_ROLLING_FACTOR = 2;

	public GameObject Actor;

	public GameObject Object;

	public int Result;

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
		Result = 0;
	}

	public static int GetFor(GameObject Actor, GameObject Object = null, bool Patting = false, bool Rolling = false)
	{
		int num = -100;
		if (Rolling)
		{
			num *= 2;
		}
		if (Object == null)
		{
			Object = Actor;
		}
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetFirefightingPerformance"))
		{
			Event obj = Event.New("GetFirefightingPerformance");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Object", Object);
			obj.SetParameter("Result", num);
			flag = Actor.FireEvent(obj);
			num = obj.GetIntParameter("Result");
		}
		if (flag && GameObject.Validate(ref Object) && Object != Actor && Object.HasRegisteredEvent("GetFirefightingPerformance"))
		{
			Event obj2 = Event.New("GetFirefightingPerformance");
			obj2.SetParameter("Actor", Actor);
			obj2.SetParameter("Object", Object);
			obj2.SetParameter("Result", num);
			flag = Object.FireEvent(obj2);
			num = obj2.GetIntParameter("Result");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetFirefightingPerformanceEvent>.ID, CascadeLevel))
		{
			GetFirefightingPerformanceEvent getFirefightingPerformanceEvent = PooledEvent<GetFirefightingPerformanceEvent>.FromPool();
			getFirefightingPerformanceEvent.Actor = Actor;
			getFirefightingPerformanceEvent.Object = Object;
			getFirefightingPerformanceEvent.Result = num;
			flag = Actor.HandleEvent(getFirefightingPerformanceEvent);
			num = getFirefightingPerformanceEvent.Result;
		}
		if (flag && GameObject.Validate(ref Object) && Object != Actor && Object.WantEvent(PooledEvent<GetFirefightingPerformanceEvent>.ID, CascadeLevel))
		{
			GetFirefightingPerformanceEvent getFirefightingPerformanceEvent2 = PooledEvent<GetFirefightingPerformanceEvent>.FromPool();
			getFirefightingPerformanceEvent2.Actor = Actor;
			getFirefightingPerformanceEvent2.Object = Object;
			getFirefightingPerformanceEvent2.Result = num;
			flag = Object.HandleEvent(getFirefightingPerformanceEvent2);
			num = getFirefightingPerformanceEvent2.Result;
		}
		return num;
	}
}
