using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class GetContentsEvent : PooledEvent<GetContentsEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Object;

	public List<GameObject> Objects = new List<GameObject>();

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Objects.Clear();
	}

	public static GetContentsEvent FromPool(GameObject Object)
	{
		GetContentsEvent getContentsEvent = PooledEvent<GetContentsEvent>.FromPool();
		getContentsEvent.Object = Object;
		getContentsEvent.Objects.Clear();
		return getContentsEvent;
	}

	public static IList<GameObject> GetFor(GameObject Object, IList<GameObject> ListToFill = null)
	{
		if (ListToFill == null)
		{
			ListToFill = Event.NewGameObjectList();
		}
		if (Object.WantEvent(PooledEvent<GetContentsEvent>.ID, CascadeLevel))
		{
			GetContentsEvent getContentsEvent = FromPool(Object);
			Object.HandleEvent(getContentsEvent);
			ListToFill.AddRange(getContentsEvent.Objects);
		}
		return ListToFill;
	}

	public static IList<GameObject> GetFor(Cell C, IList<GameObject> ListToFill = null)
	{
		if (ListToFill == null)
		{
			ListToFill = Event.NewGameObjectList();
		}
		if (C.WantEvent(PooledEvent<GetContentsEvent>.ID, CascadeLevel))
		{
			GetContentsEvent getContentsEvent = FromPool(null);
			C.HandleEvent(getContentsEvent);
			ListToFill.AddRange(getContentsEvent.Objects);
		}
		return ListToFill;
	}

	public static IList<GameObject> GetFor(Zone Z, IList<GameObject> ListToFill = null)
	{
		if (ListToFill == null)
		{
			ListToFill = Event.NewGameObjectList();
		}
		if (Z.WantEvent(PooledEvent<GetContentsEvent>.ID, CascadeLevel))
		{
			GetContentsEvent getContentsEvent = FromPool(null);
			Z.HandleEvent(getContentsEvent);
			ListToFill.AddRange(getContentsEvent.Objects);
		}
		return ListToFill;
	}
}
