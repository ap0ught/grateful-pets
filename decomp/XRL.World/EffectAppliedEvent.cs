using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class EffectAppliedEvent : IActualEffectCheckEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EffectAppliedEvent), null, CountPool, ResetPool);

	private static List<EffectAppliedEvent> Pool;

	private static int PoolCounter;

	public EffectAppliedEvent()
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

	public static void ResetTo(ref EffectAppliedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EffectAppliedEvent FromPool()
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

	public static EffectAppliedEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		EffectAppliedEvent effectAppliedEvent = FromPool();
		effectAppliedEvent.Name = Name;
		effectAppliedEvent.Effect = Effect;
		effectAppliedEvent.Actor = Actor;
		effectAppliedEvent.Duration = Effect.Duration;
		return effectAppliedEvent;
	}

	public static void Send(GameObject obj, string Name, Effect Effect, GameObject Actor = null)
	{
		if (obj.WantEvent(ID, IEffectCheckEvent.CascadeLevel))
		{
			obj.HandleEvent(FromPool(Name, Effect, Actor));
		}
		if (obj.HasRegisteredEvent("EffectApplied"))
		{
			obj.FireEvent(Event.New("EffectApplied", "Effect", Effect));
		}
		if (ComponentReflection.EffectExtensions.TryGetValue(Effect.GetType(), out var value))
		{
			value.Add(obj, Effect, Registration: true, Creation: false);
		}
	}
}
