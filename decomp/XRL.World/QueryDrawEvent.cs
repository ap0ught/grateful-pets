using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class QueryDrawEvent : PooledEvent<QueryDrawEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public int Draw;

	public bool BroadcastDrawDone;

	public int BroadcastDraw;

	public int HighTransmitRate;

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
		Draw = 0;
		BroadcastDrawDone = false;
		BroadcastDraw = 0;
		HighTransmitRate = 0;
	}

	public static QueryDrawEvent FromPool(GameObject Object)
	{
		QueryDrawEvent queryDrawEvent = PooledEvent<QueryDrawEvent>.FromPool();
		queryDrawEvent.Object = Object;
		queryDrawEvent.Draw = 0;
		queryDrawEvent.BroadcastDrawDone = false;
		queryDrawEvent.BroadcastDraw = 0;
		queryDrawEvent.HighTransmitRate = 0;
		return queryDrawEvent;
	}

	public static int GetFor(GameObject Object)
	{
		if (Object != null && Object.WantEvent(PooledEvent<QueryDrawEvent>.ID, CascadeLevel))
		{
			QueryDrawEvent queryDrawEvent = FromPool(Object);
			Object.HandleEvent(queryDrawEvent);
			return queryDrawEvent.Draw;
		}
		return 0;
	}

	public static int GetFor(List<GameObject> Objects)
	{
		QueryDrawEvent queryDrawEvent = null;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				GameObject gameObject = Objects[i];
				if (gameObject.WantEvent(PooledEvent<QueryDrawEvent>.ID, CascadeLevel))
				{
					if (queryDrawEvent == null)
					{
						queryDrawEvent = FromPool(gameObject);
					}
					else
					{
						queryDrawEvent.Object = gameObject;
					}
					gameObject.HandleEvent(queryDrawEvent);
				}
			}
		}
		return queryDrawEvent?.Draw ?? 0;
	}
}
