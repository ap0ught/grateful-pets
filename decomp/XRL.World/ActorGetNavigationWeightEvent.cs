using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ActorGetNavigationWeightEvent : INavigationWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ActorGetNavigationWeightEvent), null, CountPool, ResetPool);

	private static List<ActorGetNavigationWeightEvent> Pool;

	private static int PoolCounter;

	public ActorGetNavigationWeightEvent()
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

	public static void ResetTo(ref ActorGetNavigationWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ActorGetNavigationWeightEvent FromPool()
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

	public static void Process(INavigationWeightEvent Parent)
	{
		if (Parent.Weight < 100 && Parent.Actor.IsValid())
		{
			ActorGetNavigationWeightEvent E = FromPool();
			Parent.ApplyTo(E);
			E.PriorWeight = E.Weight;
			E.Actor.HandleEvent(E);
			E.ApplyTo(Parent);
			ResetTo(ref E);
		}
	}

	public static int GetFor(Cell Cell, GameObject Actor, ref bool Uncacheable, int Weight = 0, int Nav = 0)
	{
		if (Weight >= 100)
		{
			return Weight;
		}
		if (Cell != null && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			ActorGetNavigationWeightEvent actorGetNavigationWeightEvent = FromPool();
			actorGetNavigationWeightEvent.Cell = Cell;
			actorGetNavigationWeightEvent.Actor = Actor;
			actorGetNavigationWeightEvent.Object = null;
			actorGetNavigationWeightEvent.Uncacheable = Uncacheable;
			actorGetNavigationWeightEvent.Weight = Weight;
			actorGetNavigationWeightEvent.PriorWeight = Weight;
			actorGetNavigationWeightEvent.Nav = Nav;
			Actor.HandleEvent(actorGetNavigationWeightEvent);
			Weight = actorGetNavigationWeightEvent.Weight;
			Uncacheable = actorGetNavigationWeightEvent.Uncacheable;
		}
		return Weight;
	}
}
