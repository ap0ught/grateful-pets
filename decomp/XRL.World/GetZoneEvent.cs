namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetZoneEvent : PooledEvent<GetZoneEvent>
{
	public string ZoneID;

	public Zone Result;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		ZoneID = null;
		Result = null;
	}

	public static bool TryGetFor(XRLGame Game, string ZoneID, out Zone Zone)
	{
		GetZoneEvent E = PooledEvent<GetZoneEvent>.FromPool();
		try
		{
			E.ZoneID = ZoneID;
			Game.HandleEvent(E);
			Zone = E.Result;
			return Zone != null;
		}
		finally
		{
			PooledEvent<GetZoneEvent>.ResetTo(ref E);
		}
	}
}
