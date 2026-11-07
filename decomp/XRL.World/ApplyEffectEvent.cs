using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ApplyEffectEvent : IActualEffectCheckEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ApplyEffectEvent), null, CountPool, ResetPool);

	private static List<ApplyEffectEvent> Pool;

	private static int PoolCounter;

	public ApplyEffectEvent()
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

	public static void ResetTo(ref ApplyEffectEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ApplyEffectEvent FromPool()
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

	public static ApplyEffectEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		ApplyEffectEvent applyEffectEvent = FromPool();
		applyEffectEvent.Name = Name;
		applyEffectEvent.Effect = Effect;
		applyEffectEvent.Actor = Actor;
		applyEffectEvent.Duration = Effect.Duration;
		return applyEffectEvent;
	}

	public static bool Check(GameObject obj, string Name, Effect Effect, GameObject Actor = null)
	{
		if (!Effect.CanBeAppliedTo(obj))
		{
			return false;
		}
		if (obj.WantEvent(ID, IEffectCheckEvent.CascadeLevel) && !obj.HandleEvent(FromPool(Name, Effect, Actor)))
		{
			return false;
		}
		if (obj.HasRegisteredEvent("ApplyEffect") && !obj.FireEvent(Event.New("ApplyEffect", "Object", obj, "Effect", Effect, "Owner", Actor)))
		{
			return false;
		}
		return true;
	}
}
