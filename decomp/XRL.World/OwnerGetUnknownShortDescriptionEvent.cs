using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class OwnerGetUnknownShortDescriptionEvent : IShortDescriptionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(OwnerGetUnknownShortDescriptionEvent), null, CountPool, ResetPool);

	private static List<OwnerGetUnknownShortDescriptionEvent> Pool;

	private static int PoolCounter;

	public OwnerGetUnknownShortDescriptionEvent()
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

	public static void ResetTo(ref OwnerGetUnknownShortDescriptionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static OwnerGetUnknownShortDescriptionEvent FromPool()
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

	public static OwnerGetUnknownShortDescriptionEvent FromPool(string Base, string Context = null, bool AsIfKnown = false)
	{
		OwnerGetUnknownShortDescriptionEvent ownerGetUnknownShortDescriptionEvent = FromPool();
		ownerGetUnknownShortDescriptionEvent.Context = Context;
		ownerGetUnknownShortDescriptionEvent.AsIfKnown = AsIfKnown;
		ownerGetUnknownShortDescriptionEvent.Base.Append(Base);
		return ownerGetUnknownShortDescriptionEvent;
	}

	public override string GetRegisteredEventID()
	{
		return "OwnerGetUnknownShortDescription";
	}
}
