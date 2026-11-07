using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class EffectRemovedEvent : IActualEffectCheckEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EffectRemovedEvent), null, CountPool, ResetPool);

	private static List<EffectRemovedEvent> Pool;

	private static int PoolCounter;

	public EffectRemovedEvent()
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

	public static void ResetTo(ref EffectRemovedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EffectRemovedEvent FromPool()
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

	public static EffectRemovedEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		EffectRemovedEvent effectRemovedEvent = FromPool();
		effectRemovedEvent.Name = Name;
		effectRemovedEvent.Effect = Effect;
		effectRemovedEvent.Actor = Actor;
		effectRemovedEvent.Duration = Effect.Duration;
		return effectRemovedEvent;
	}

	public static void Send(GameObject obj, string Name, Effect Effect, GameObject Actor = null)
	{
		EffectRemovedEvent effectRemovedEvent = FromPool();
		effectRemovedEvent.Name = Name;
		effectRemovedEvent.Effect = Effect;
		effectRemovedEvent.Actor = Actor;
		effectRemovedEvent.Duration = Effect.Duration;
		obj.HandleEvent(effectRemovedEvent);
	}
}
