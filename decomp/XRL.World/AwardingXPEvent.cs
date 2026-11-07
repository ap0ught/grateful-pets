using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AwardingXPEvent : IXPEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AwardingXPEvent), null, CountPool, ResetPool);

	private static List<AwardingXPEvent> Pool;

	private static int PoolCounter;

	public AwardingXPEvent()
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

	public static void ResetTo(ref AwardingXPEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AwardingXPEvent FromPool()
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

	public static bool Check(IXPEvent ParentEvent, IEventSource Source)
	{
		if (Source.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AwardingXPEvent awardingXPEvent = FromPool();
			ParentEvent.ApplyTo(awardingXPEvent);
			if (!The.Game.HandleEvent(awardingXPEvent) || !Source.HandleEvent(awardingXPEvent))
			{
				return false;
			}
			awardingXPEvent.ApplyTo(ParentEvent);
		}
		return true;
	}
}
