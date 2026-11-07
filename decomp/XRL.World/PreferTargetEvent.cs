namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class PreferTargetEvent : PooledEvent<PreferTargetEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Target1;

	public GameObject Target2;

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
		Target1 = null;
		Target2 = null;
		Result = 0;
	}

	public static int Check(GameObject Actor, GameObject Target1, GameObject Target2)
	{
		int num = 0;
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("PreferTarget"))
		{
			Event obj = Event.New("PreferTarget");
			obj.SetParameter("Target1", Target1);
			obj.SetParameter("Target2", Target2);
			obj.SetParameter("Result", num);
			flag = Actor.FireEvent(obj);
			num = obj.GetIntParameter("Result");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<PreferTargetEvent>.ID, CascadeLevel))
		{
			PreferTargetEvent preferTargetEvent = PooledEvent<PreferTargetEvent>.FromPool();
			preferTargetEvent.Target1 = Target1;
			preferTargetEvent.Target2 = Target2;
			preferTargetEvent.Result = num;
			flag = Actor.HandleEvent(preferTargetEvent);
			num = preferTargetEvent.Result;
		}
		return num;
	}
}
