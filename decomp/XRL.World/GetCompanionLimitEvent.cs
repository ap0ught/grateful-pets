namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class GetCompanionLimitEvent : PooledEvent<GetCompanionLimitEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Actor;

	public string Means;

	public int BaseLimit;

	public int Limit;

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
		Means = null;
		BaseLimit = 0;
		Limit = 0;
	}

	public static int GetFor(GameObject Actor, string Means, int BaseLimit = 0)
	{
		int num = BaseLimit;
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetCompanionLimit"))
		{
			Event obj = Event.New("GetCompanionLimit");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Means", Means);
			obj.SetParameter("BaseLimit", BaseLimit);
			obj.SetParameter("Limit", num);
			flag = Actor.FireEvent(obj);
			num = obj.GetIntParameter("Limit");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetCompanionLimitEvent>.ID, CascadeLevel))
		{
			GetCompanionLimitEvent getCompanionLimitEvent = PooledEvent<GetCompanionLimitEvent>.FromPool();
			getCompanionLimitEvent.Actor = Actor;
			getCompanionLimitEvent.Means = Means;
			getCompanionLimitEvent.BaseLimit = BaseLimit;
			getCompanionLimitEvent.Limit = num;
			flag = Actor.HandleEvent(getCompanionLimitEvent);
			num = getCompanionLimitEvent.Limit;
		}
		return num;
	}
}
