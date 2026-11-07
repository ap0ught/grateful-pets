using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetUnknownShortDescriptionEvent : IShortDescriptionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetUnknownShortDescriptionEvent), null, CountPool, ResetPool);

	private static List<GetUnknownShortDescriptionEvent> Pool;

	private static int PoolCounter;

	public GetUnknownShortDescriptionEvent()
	{
		base.ID = ID;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref GetUnknownShortDescriptionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetUnknownShortDescriptionEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public static GetUnknownShortDescriptionEvent FromPool(GameObject Object, string Base, string Context = null, bool AsIfKnown = false)
	{
		GetUnknownShortDescriptionEvent getUnknownShortDescriptionEvent = FromPool();
		getUnknownShortDescriptionEvent.Object = Object;
		getUnknownShortDescriptionEvent.Context = Context;
		getUnknownShortDescriptionEvent.AsIfKnown = AsIfKnown;
		getUnknownShortDescriptionEvent.Base.Append(Base);
		return getUnknownShortDescriptionEvent;
	}

	public override string GetRegisteredEventID()
	{
		return "GetUnknownShortDescription";
	}
}
