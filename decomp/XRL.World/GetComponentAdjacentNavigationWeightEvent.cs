using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetComponentAdjacentNavigationWeightEvent : IAdjacentNavigationWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetComponentAdjacentNavigationWeightEvent), null, CountPool, ResetPool);

	private static List<GetComponentAdjacentNavigationWeightEvent> Pool;

	private static int PoolCounter;

	public GetComponentAdjacentNavigationWeightEvent()
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

	public static void ResetTo(ref GetComponentAdjacentNavigationWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetComponentAdjacentNavigationWeightEvent FromPool()
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

	public static GetComponentAdjacentNavigationWeightEvent FromPool(INavigationWeightEvent Source)
	{
		GetComponentAdjacentNavigationWeightEvent getComponentAdjacentNavigationWeightEvent = FromPool();
		Source.ApplyTo(getComponentAdjacentNavigationWeightEvent);
		return getComponentAdjacentNavigationWeightEvent;
	}

	public static void Process(GameObject Object, INavigationWeightEvent ParentEvent)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetComponentAdjacentNavigationWeightEvent getComponentAdjacentNavigationWeightEvent = FromPool(ParentEvent);
			getComponentAdjacentNavigationWeightEvent.Object = Object;
			getComponentAdjacentNavigationWeightEvent.PriorWeight = getComponentAdjacentNavigationWeightEvent.Weight;
			Object.HandleEvent(getComponentAdjacentNavigationWeightEvent);
			getComponentAdjacentNavigationWeightEvent.ApplyTo(ParentEvent);
		}
	}
}
