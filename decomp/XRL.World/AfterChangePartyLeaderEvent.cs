namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class AfterChangePartyLeaderEvent : PooledEvent<AfterChangePartyLeaderEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject NewLeader;

	public GameObject OldLeader;

	public bool Transient;

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
		NewLeader = null;
		OldLeader = null;
		Transient = false;
	}

	public static void Send(GameObject Actor, GameObject NewLeader, GameObject OldLeader, bool Transient = false)
	{
		if (NewLeader == OldLeader || !GameObject.Validate(ref Actor))
		{
			return;
		}
		bool flag = Actor.WantEvent(PooledEvent<AfterChangePartyLeaderEvent>.ID, CascadeLevel);
		bool flag2 = NewLeader?.WantEvent(PooledEvent<AfterChangePartyLeaderEvent>.ID, CascadeLevel) ?? false;
		bool flag3 = OldLeader?.WantEvent(PooledEvent<AfterChangePartyLeaderEvent>.ID, CascadeLevel) ?? false;
		if (flag | flag2 | flag3)
		{
			AfterChangePartyLeaderEvent afterChangePartyLeaderEvent = PooledEvent<AfterChangePartyLeaderEvent>.FromPool();
			afterChangePartyLeaderEvent.Actor = Actor;
			afterChangePartyLeaderEvent.NewLeader = NewLeader;
			afterChangePartyLeaderEvent.OldLeader = OldLeader;
			afterChangePartyLeaderEvent.Transient = Transient;
			if (flag)
			{
				Actor.HandleEvent(afterChangePartyLeaderEvent);
			}
			if (flag2)
			{
				NewLeader.HandleEvent(afterChangePartyLeaderEvent);
			}
			if (flag3)
			{
				OldLeader.HandleEvent(afterChangePartyLeaderEvent);
			}
		}
	}
}
