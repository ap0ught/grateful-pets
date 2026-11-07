using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CommandSmartUseEarlyEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CommandSmartUseEarlyEvent), null, CountPool, ResetPool);

	private static List<CommandSmartUseEarlyEvent> Pool;

	private static int PoolCounter;

	public CommandSmartUseEarlyEvent()
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

	public static void ResetTo(ref CommandSmartUseEarlyEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CommandSmartUseEarlyEvent FromPool()
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

	public static CommandSmartUseEarlyEvent FromPool(GameObject Actor, GameObject Item)
	{
		CommandSmartUseEarlyEvent commandSmartUseEarlyEvent = FromPool();
		commandSmartUseEarlyEvent.Actor = Actor;
		commandSmartUseEarlyEvent.Item = Item;
		return commandSmartUseEarlyEvent;
	}
}
