using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetComponentNavigationWeightEvent : INavigationWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetComponentNavigationWeightEvent), null, CountPool, ResetPool);

	private static List<GetComponentNavigationWeightEvent> Pool;

	private static int PoolCounter;

	public GetComponentNavigationWeightEvent()
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

	public static void ResetTo(ref GetComponentNavigationWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetComponentNavigationWeightEvent FromPool()
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

	public static GetComponentNavigationWeightEvent FromPool(INavigationWeightEvent Source)
	{
		GetComponentNavigationWeightEvent getComponentNavigationWeightEvent = FromPool();
		Source.ApplyTo(getComponentNavigationWeightEvent);
		return getComponentNavigationWeightEvent;
	}

	public static void Process(GameObject Object, INavigationWeightEvent ParentEvent)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetComponentNavigationWeightEvent getComponentNavigationWeightEvent = FromPool(ParentEvent);
			getComponentNavigationWeightEvent.Object = Object;
			getComponentNavigationWeightEvent.PriorWeight = getComponentNavigationWeightEvent.Weight;
			Object.HandleEvent(getComponentNavigationWeightEvent);
			getComponentNavigationWeightEvent.ApplyTo(ParentEvent);
		}
	}
}
