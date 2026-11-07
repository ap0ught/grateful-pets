using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class GetReplaceCellInteractionsEvent : PooledEvent<GetReplaceCellInteractionsEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Actor;

	public List<GameObject> Objects = new List<GameObject>();

	public Dictionary<GameObject, int> Priorities = new Dictionary<GameObject, int>();

	public Dictionary<GameObject, string> Interactions = new Dictionary<GameObject, string>();

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
		Actor = null;
		Objects.Clear();
		Priorities.Clear();
		Interactions.Clear();
	}

	public void Add(GameObject Object, int Priority, string Interaction)
	{
		if (GameObject.Validate(ref Object))
		{
			if (!Objects.Contains(Object))
			{
				Objects.Add(Object);
			}
			Priorities[Object] = Priority;
			Interactions[Object] = Interaction;
		}
		else
		{
			MetricsManager.LogError("tried to add replace cell interaction with invalid object");
		}
	}

	public int Sorter(GameObject A, GameObject B)
	{
		if (A == null || B == null)
		{
			return 0;
		}
		int num = Priorities[A].CompareTo(Priorities[B]);
		if (num != 0)
		{
			return -num;
		}
		return A.GetCachedDisplayNameForSort().CompareTo(B.GetCachedDisplayNameForSort());
	}

	public static GetReplaceCellInteractionsEvent GetFor(GameObject Actor)
	{
		GetReplaceCellInteractionsEvent getReplaceCellInteractionsEvent = null;
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetReplaceCellInteractionsEvent>.ID, CascadeLevel))
		{
			getReplaceCellInteractionsEvent = PooledEvent<GetReplaceCellInteractionsEvent>.FromPool();
			getReplaceCellInteractionsEvent.Actor = Actor;
			Actor.HandleEvent(getReplaceCellInteractionsEvent);
			if (getReplaceCellInteractionsEvent.Objects.Count > 1)
			{
				getReplaceCellInteractionsEvent.Objects.Sort(getReplaceCellInteractionsEvent.Sorter);
			}
		}
		return getReplaceCellInteractionsEvent;
	}
}
