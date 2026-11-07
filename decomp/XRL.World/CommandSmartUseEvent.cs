using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CommandSmartUseEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CommandSmartUseEvent), null, CountPool, ResetPool);

	private static List<CommandSmartUseEvent> Pool;

	private static int PoolCounter;

	public int MinPriority;

	public CommandSmartUseEvent()
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

	public static void ResetTo(ref CommandSmartUseEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CommandSmartUseEvent FromPool()
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

	public override void Reset()
	{
		base.Reset();
		MinPriority = 0;
	}

	public static CommandSmartUseEvent FromPool(GameObject Actor, GameObject Item, int MinPriority)
	{
		CommandSmartUseEvent commandSmartUseEvent = FromPool();
		commandSmartUseEvent.Actor = Actor;
		commandSmartUseEvent.Item = Item;
		commandSmartUseEvent.MinPriority = MinPriority;
		return commandSmartUseEvent;
	}
}
