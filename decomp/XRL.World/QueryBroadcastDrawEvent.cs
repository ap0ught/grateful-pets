namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class QueryBroadcastDrawEvent : PooledEvent<QueryBroadcastDrawEvent>
{
	public new static readonly int CascadeLevel = 15;

	public Zone Zone;

	public int Draw;

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
		Zone = null;
		Draw = 0;
	}

	public static QueryBroadcastDrawEvent FromPool(Zone Zone)
	{
		QueryBroadcastDrawEvent queryBroadcastDrawEvent = PooledEvent<QueryBroadcastDrawEvent>.FromPool();
		queryBroadcastDrawEvent.Zone = Zone;
		queryBroadcastDrawEvent.Draw = 0;
		return queryBroadcastDrawEvent;
	}

	public static int GetFor(Zone Z)
	{
		if (Z != null && Z.WantEvent(PooledEvent<QueryBroadcastDrawEvent>.ID, CascadeLevel))
		{
			QueryBroadcastDrawEvent queryBroadcastDrawEvent = FromPool(Z);
			Z.HandleEvent(queryBroadcastDrawEvent);
			return queryBroadcastDrawEvent.Draw;
		}
		return 0;
	}
}
