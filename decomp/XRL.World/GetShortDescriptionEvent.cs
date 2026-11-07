using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetShortDescriptionEvent : IShortDescriptionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetShortDescriptionEvent), null, CountPool, ResetPool);

	private static List<GetShortDescriptionEvent> Pool;

	private static int PoolCounter;

	public GetShortDescriptionEvent()
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

	public static void ResetTo(ref GetShortDescriptionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetShortDescriptionEvent FromPool()
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

	public static GetShortDescriptionEvent FromPool(GameObject Object, string Base, string Context = null, bool AsIfKnown = false)
	{
		GetShortDescriptionEvent getShortDescriptionEvent = FromPool();
		getShortDescriptionEvent.Object = Object;
		getShortDescriptionEvent.Context = Context;
		getShortDescriptionEvent.AsIfKnown = AsIfKnown;
		getShortDescriptionEvent.Base.Append(Base);
		return getShortDescriptionEvent;
	}

	public override string GetRegisteredEventID()
	{
		return "GetShortDescription";
	}
}
