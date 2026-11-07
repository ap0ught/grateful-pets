namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetGameObjectSortEvent : PooledEvent<GetGameObjectSortEvent>
{
	public GameObject Object1;

	public GameObject Object2;

	public string Category1;

	public string Category2;

	public int Sort;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object1 = null;
		Object2 = null;
		Category1 = null;
		Category2 = null;
		Sort = 0;
	}

	public static int GetFor(GameObject Object1, GameObject Object2, string Category1, string Category2)
	{
		int num = 0;
		if (1 != 0 && GameObject.Validate(ref Object1) && GameObject.Validate(ref Object2))
		{
			bool flag = Object1.WantEvent(PooledEvent<GetGameObjectSortEvent>.ID, MinEvent.CascadeLevel);
			bool flag2 = Object2.WantEvent(PooledEvent<GetGameObjectSortEvent>.ID, MinEvent.CascadeLevel);
			if (flag | flag2)
			{
				GetGameObjectSortEvent E = PooledEvent<GetGameObjectSortEvent>.FromPool();
				E.Object1 = Object1;
				E.Object2 = Object2;
				E.Category1 = Category1;
				E.Category2 = Category2;
				E.Sort = num;
				if (!flag || Object1.HandleEvent(E))
				{
					if (!flag2)
					{
						_ = 1;
					}
					else
						Object2.HandleEvent(E);
				}
				else
					_ = 0;
				num = E.Sort;
				PooledEvent<GetGameObjectSortEvent>.ResetTo(ref E);
			}
		}
		return num;
	}
}
