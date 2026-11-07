using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class EffectForceAppliedEvent : IActualEffectCheckEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EffectForceAppliedEvent), null, CountPool, ResetPool);

	private static List<EffectForceAppliedEvent> Pool;

	private static int PoolCounter;

	public EffectForceAppliedEvent()
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

	public static void ResetTo(ref EffectForceAppliedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EffectForceAppliedEvent FromPool()
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

	public static EffectForceAppliedEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		EffectForceAppliedEvent effectForceAppliedEvent = FromPool();
		effectForceAppliedEvent.Name = Name;
		effectForceAppliedEvent.Effect = Effect;
		effectForceAppliedEvent.Actor = Actor;
		effectForceAppliedEvent.Duration = Effect.Duration;
		return effectForceAppliedEvent;
	}

	public static void Send(GameObject obj, string Name, Effect Effect, GameObject Actor = null)
	{
		if (obj.WantEvent(ID, IEffectCheckEvent.CascadeLevel))
		{
			obj.HandleEvent(FromPool(Name, Effect, Actor));
		}
		if (obj.HasRegisteredEvent("EffectForceApplied"))
		{
			obj.FireEvent(Event.New("EffectForceApplied", "Effect", Effect));
		}
	}
}
