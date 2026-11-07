using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetNavigationWeightEvent : INavigationWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetNavigationWeightEvent), null, CountPool, ResetPool);

	private static List<GetNavigationWeightEvent> Pool;

	private static int PoolCounter;

	public GetNavigationWeightEvent()
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

	public static void ResetTo(ref GetNavigationWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetNavigationWeightEvent FromPool()
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

	public static GetNavigationWeightEvent HandleFor(Cell Cell, GameObject Actor, bool Uncacheable, int Weight = 0, int Nav = 0)
	{
		GetNavigationWeightEvent getNavigationWeightEvent = FromPool();
		getNavigationWeightEvent.Actor = Actor;
		getNavigationWeightEvent.Cell = Cell;
		getNavigationWeightEvent.Weight = Weight;
		getNavigationWeightEvent.Nav = Nav;
		getNavigationWeightEvent.Uncacheable = Uncacheable;
		if (Weight >= 100 || Cell == null)
		{
			return getNavigationWeightEvent;
		}
		GameObject[] array = Cell.Objects.GetArray();
		int i = 0;
		for (int count = Cell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = array[i];
			if (gameObject.IntProperty.TryGetValue("NavigationWeight", out var value) && value > Weight)
			{
				getNavigationWeightEvent.Weight = value;
			}
			getNavigationWeightEvent.Object = gameObject;
			getNavigationWeightEvent.PriorWeight = Weight;
			gameObject.HandleEvent(getNavigationWeightEvent);
			Weight = getNavigationWeightEvent.Weight;
			if (Weight >= 100)
			{
				break;
			}
		}
		return getNavigationWeightEvent;
	}

	public static int GetFor(Cell Cell, GameObject Actor, ref bool Uncacheable, int Weight = 0, int Nav = 0)
	{
		if (Weight >= 100)
		{
			return Weight;
		}
		if (Cell != null)
		{
			GetNavigationWeightEvent getNavigationWeightEvent = null;
			int i = 0;
			for (int count = Cell.Objects.Count; i < count; i++)
			{
				int priorWeight = Weight;
				GameObject gameObject = Cell.Objects[i];
				int intProperty = gameObject.GetIntProperty("NavigationWeight");
				if (intProperty > Weight)
				{
					Weight = intProperty;
				}
				if (gameObject.WantEvent(ID, MinEvent.CascadeLevel))
				{
					if (getNavigationWeightEvent == null)
					{
						getNavigationWeightEvent = FromPool();
						getNavigationWeightEvent.Cell = Cell;
						getNavigationWeightEvent.Actor = Actor;
						getNavigationWeightEvent.Uncacheable = Uncacheable;
						getNavigationWeightEvent.Weight = 0;
						getNavigationWeightEvent.PriorWeight = 0;
						getNavigationWeightEvent.Nav = Nav;
					}
					getNavigationWeightEvent.Object = gameObject;
					getNavigationWeightEvent.Weight = Weight;
					getNavigationWeightEvent.PriorWeight = priorWeight;
					gameObject.HandleEvent(getNavigationWeightEvent);
					Weight = getNavigationWeightEvent.Weight;
					Uncacheable = getNavigationWeightEvent.Uncacheable;
				}
				if (Weight >= 100)
				{
					return Weight;
				}
			}
		}
		return Weight;
	}
}
