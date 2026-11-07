using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ForceApplyEffectEvent : IActualEffectCheckEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ForceApplyEffectEvent), null, CountPool, ResetPool);

	private static List<ForceApplyEffectEvent> Pool;

	private static int PoolCounter;

	public ForceApplyEffectEvent()
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

	public static void ResetTo(ref ForceApplyEffectEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ForceApplyEffectEvent FromPool()
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

	public static ForceApplyEffectEvent FromPool(string Name, Effect Effect, GameObject Actor = null)
	{
		ForceApplyEffectEvent forceApplyEffectEvent = FromPool();
		forceApplyEffectEvent.Name = Name;
		forceApplyEffectEvent.Effect = Effect;
		forceApplyEffectEvent.Actor = Actor;
		forceApplyEffectEvent.Duration = Effect.Duration;
		return forceApplyEffectEvent;
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
		if (obj.HasRegisteredEvent("ForceApplyEffect") && !obj.FireEvent(Event.New("ForceApplyEffect", "Object", obj, "Effect", Effect, "Owner", Actor)))
		{
			return false;
		}
		return true;
	}
}
