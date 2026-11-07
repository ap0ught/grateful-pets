using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class OwnerGetShortDescriptionEvent : IShortDescriptionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(OwnerGetShortDescriptionEvent), null, CountPool, ResetPool);

	private static List<OwnerGetShortDescriptionEvent> Pool;

	private static int PoolCounter;

	public OwnerGetShortDescriptionEvent()
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

	public static void ResetTo(ref OwnerGetShortDescriptionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static OwnerGetShortDescriptionEvent FromPool()
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

	public static OwnerGetShortDescriptionEvent FromPool(string Base, string Context = null, bool AsIfKnown = false)
	{
		OwnerGetShortDescriptionEvent ownerGetShortDescriptionEvent = FromPool();
		ownerGetShortDescriptionEvent.Context = Context;
		ownerGetShortDescriptionEvent.AsIfKnown = AsIfKnown;
		ownerGetShortDescriptionEvent.Base.Append(Base);
		return ownerGetShortDescriptionEvent;
	}

	public override string GetRegisteredEventID()
	{
		return "OwnerGetShortDescription";
	}
}
