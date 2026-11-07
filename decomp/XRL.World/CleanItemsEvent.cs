using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class CleanItemsEvent : PooledEvent<CleanItemsEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Actor;

	public GameObject CascadeFrom;

	public GameObject Using;

	public List<GameObject> Objects = new List<GameObject>();

	public List<string> Types = new List<string>();

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
		CascadeFrom = null;
		Using = null;
		Objects.Clear();
		Types.Clear();
	}

	public static CleanItemsEvent FromPool(GameObject Actor, GameObject CascadeFrom, GameObject Using)
	{
		CleanItemsEvent cleanItemsEvent = PooledEvent<CleanItemsEvent>.FromPool();
		cleanItemsEvent.Actor = Actor;
		cleanItemsEvent.CascadeFrom = CascadeFrom;
		cleanItemsEvent.Using = Using;
		cleanItemsEvent.Objects.Clear();
		cleanItemsEvent.Types.Clear();
		return cleanItemsEvent;
	}

	public void RegisterObject(GameObject obj)
	{
		if (obj != null && !Objects.Contains(obj))
		{
			Objects.Add(obj);
		}
	}

	public void RegisterType(string type)
	{
		if (!string.IsNullOrEmpty(type) && !Types.Contains(type))
		{
			Types.Add(type);
		}
	}

	public static bool PerformFor(GameObject Actor, GameObject CascadeFrom, GameObject Using, out List<GameObject> Objects, out List<string> Types)
	{
		if (GameObject.Validate(ref CascadeFrom) && CascadeFrom.WantEvent(PooledEvent<CleanItemsEvent>.ID, CascadeLevel))
		{
			CleanItemsEvent cleanItemsEvent = FromPool(Actor, CascadeFrom, Using);
			CascadeFrom.HandleEvent(cleanItemsEvent);
			Objects = cleanItemsEvent.Objects;
			Types = cleanItemsEvent.Types;
			return true;
		}
		Objects = null;
		Types = null;
		return false;
	}
}
