namespace XRL.World;

[GameEvent(Cache = Cache.Singleton)]
public class RadiatesHeatAdjacentEvent : SingletonEvent<RadiatesHeatAdjacentEvent>
{
	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && !Object.HandleEvent(SingletonEvent<RadiatesHeatAdjacentEvent>.Instance))
		{
			return true;
		}
		return false;
	}

	public static bool Check(Cell C)
	{
		if (C != null)
		{
			int i = 0;
			for (int count = C.Objects.Count; i < count; i++)
			{
				if (Check(C.Objects[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CheckAdjacent(Cell C)
	{
		if (C != null)
		{
			foreach (Cell localAdjacentCell in C.GetLocalAdjacentCells())
			{
				if (Check(localAdjacentCell))
				{
					return true;
				}
			}
		}
		return false;
	}
}
