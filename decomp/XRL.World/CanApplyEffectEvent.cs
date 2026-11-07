using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanApplyEffectEvent : IEffectCheckEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CanApplyEffectEvent), null, CountPool, ResetPool);

	private static List<CanApplyEffectEvent> Pool;

	private static int PoolCounter;

	public Type Type;

	public CanApplyEffectEvent()
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

	public static void ResetTo(ref CanApplyEffectEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CanApplyEffectEvent FromPool()
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
		Type = null;
	}

	public static bool Check<T>(GameObject Object, int Duration = 0)
	{
		CanApplyEffectEvent canApplyEffectEvent = FromPool();
		canApplyEffectEvent.Name = "T";
		canApplyEffectEvent.Type = typeof(T);
		canApplyEffectEvent.Duration = Duration;
		return Object.HandleEvent(canApplyEffectEvent);
	}

	public static bool Check(GameObject Object, string Name, int Duration = 0)
	{
		CanApplyEffectEvent canApplyEffectEvent = FromPool();
		canApplyEffectEvent.Name = Name;
		canApplyEffectEvent.Duration = Duration;
		return Object.HandleEvent(canApplyEffectEvent);
	}
}
