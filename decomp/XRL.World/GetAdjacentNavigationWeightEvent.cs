using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetAdjacentNavigationWeightEvent : IAdjacentNavigationWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetAdjacentNavigationWeightEvent), null, CountPool, ResetPool);

	private static List<GetAdjacentNavigationWeightEvent> Pool;

	private static int PoolCounter;

	public GetAdjacentNavigationWeightEvent()
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

	public static void ResetTo(ref GetAdjacentNavigationWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetAdjacentNavigationWeightEvent FromPool()
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
		int weight = Parent.Weight;
		if (weight >= 100 || Parent.Cell == null)
		{
			return;
		}
		GetAdjacentNavigationWeightEvent E = FromPool();
		Parent.ApplyTo(E);
		List<Cell> localAdjacentCells = E.Cell.GetLocalAdjacentCells();
		int i = 0;
		for (int count = localAdjacentCells.Count; i < count; i++)
		{
			Cell cell = localAdjacentCells[i];
			GameObject[] array = cell.Objects.GetArray();
			int j = 0;
			for (int count2 = cell.Objects.Count; j < count2; j++)
			{
				GameObject gameObject = array[j];
				if (gameObject.IntProperty.TryGetValue("AdjacentNavigationWeight", out var value) && value > weight)
				{
					E.Weight = value;
				}
				E.Object = gameObject;
				E.PriorWeight = weight;
				E.AdjacentCell = cell;
				gameObject.HandleEvent(E);
				weight = E.Weight;
				if (weight >= 100)
				{
					i = count;
					break;
				}
			}
		}
		E.ApplyTo(Parent);
		ResetTo(ref E);
	}

	public static int GetFor(Cell Cell, GameObject Actor, ref bool Uncacheable, int Weight = 0, int Nav = 0)
	{
		if (Weight >= 100)
		{
			return Weight;
		}
		if (Cell != null)
		{
			GetAdjacentNavigationWeightEvent getAdjacentNavigationWeightEvent = null;
			List<Cell> localAdjacentCells = Cell.GetLocalAdjacentCells();
			int i = 0;
			for (int count = localAdjacentCells.Count; i < count; i++)
			{
				Cell cell = localAdjacentCells[i];
				int j = 0;
				for (int count2 = cell.Objects.Count; j < count2; j++)
				{
					int priorWeight = Weight;
					GameObject gameObject = cell.Objects[j];
					int intProperty = gameObject.GetIntProperty("AdjacentNavigationWeight");
					if (intProperty > Weight)
					{
						Weight = intProperty;
					}
					if (gameObject.WantEvent(ID, MinEvent.CascadeLevel))
					{
						if (getAdjacentNavigationWeightEvent == null)
						{
							getAdjacentNavigationWeightEvent = FromPool();
							getAdjacentNavigationWeightEvent.Cell = Cell;
							getAdjacentNavigationWeightEvent.Actor = Actor;
							getAdjacentNavigationWeightEvent.Uncacheable = Uncacheable;
							getAdjacentNavigationWeightEvent.Weight = 0;
							getAdjacentNavigationWeightEvent.PriorWeight = 0;
							getAdjacentNavigationWeightEvent.Nav = Nav;
						}
						getAdjacentNavigationWeightEvent.Object = gameObject;
						getAdjacentNavigationWeightEvent.Weight = Weight;
						getAdjacentNavigationWeightEvent.PriorWeight = priorWeight;
						getAdjacentNavigationWeightEvent.AdjacentCell = cell;
						gameObject.HandleEvent(getAdjacentNavigationWeightEvent);
						Weight = getAdjacentNavigationWeightEvent.Weight;
						Uncacheable = getAdjacentNavigationWeightEvent.Uncacheable;
					}
					if (Weight >= 100)
					{
						return Weight;
					}
				}
			}
		}
		return Weight;
	}
}
