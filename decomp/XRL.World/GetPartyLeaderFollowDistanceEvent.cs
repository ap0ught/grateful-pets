namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetPartyLeaderFollowDistanceEvent : PooledEvent<GetPartyLeaderFollowDistanceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public static readonly int DEFAULT_DISTANCE_PLAYER_LED = 1;

	public static readonly int DEFAULT_DISTANCE_NON_PLAYER_LED = 5;

	public GameObject Object;

	public GameObject PartyLeader;

	public int Distance;

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
		PartyLeader = null;
		Distance = 0;
	}

	public static int GetFor(GameObject Object, GameObject PartyLeader = null, int? DefaultDistance = null)
	{
		bool flag = true;
		if (PartyLeader == null)
		{
			PartyLeader = Object.PartyLeader;
		}
		int num = DefaultDistance ?? Object.GetIntPropertyIfSet("PartyLeaderFollowDistance") ?? (PartyLeader.IsPlayer() ? DEFAULT_DISTANCE_PLAYER_LED : DEFAULT_DISTANCE_NON_PLAYER_LED);
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetPartyLeaderFollowDistance"))
		{
			Event obj = Event.New("GetPartyLeaderFollowDistance");
			obj.SetParameter("Object", Object);
			obj.SetParameter("PartyLeader", PartyLeader);
			obj.SetParameter("Distance", num);
			flag = Object.FireEvent(obj);
			num = obj.GetIntParameter("Distance");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetPartyLeaderFollowDistanceEvent>.ID, CascadeLevel))
		{
			GetPartyLeaderFollowDistanceEvent getPartyLeaderFollowDistanceEvent = PooledEvent<GetPartyLeaderFollowDistanceEvent>.FromPool();
			getPartyLeaderFollowDistanceEvent.Object = Object;
			getPartyLeaderFollowDistanceEvent.PartyLeader = PartyLeader;
			getPartyLeaderFollowDistanceEvent.Distance = num;
			flag = Object.HandleEvent(getPartyLeaderFollowDistanceEvent);
			num = getPartyLeaderFollowDistanceEvent.Distance;
		}
		return num;
	}

	public void MinDistance(int Value)
	{
		if (Distance < Value)
		{
			Distance = Value;
		}
	}
}
