using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 17)]
public class BeforePullDownEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforePullDownEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<BeforePullDownEvent> Pool;

	private static int PoolCounter;

	public GameObject Pit;

	public GameObject Object;

	public Cell DestinationCell;

	public BeforePullDownEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
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

	public static void ResetTo(ref BeforePullDownEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforePullDownEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Pit = null;
		Object = null;
		DestinationCell = null;
	}

	public static bool Check(GameObject Pit, GameObject Object, ref Cell DestinationCell)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BeforePullDown"))
		{
			Event obj = Event.New("BeforePullDown");
			obj.SetParameter("Pit", Pit);
			obj.SetParameter("Object", Object);
			obj.SetParameter("DestinationCell", DestinationCell);
			flag = Object.FireEvent(obj);
			DestinationCell = obj.GetParameter<Cell>("DestinationCell");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			BeforePullDownEvent beforePullDownEvent = FromPool();
			beforePullDownEvent.Pit = Pit;
			beforePullDownEvent.Object = Object;
			beforePullDownEvent.DestinationCell = DestinationCell;
			flag = Object.HandleEvent(beforePullDownEvent);
			DestinationCell = beforePullDownEvent.DestinationCell;
		}
		return flag;
	}
}
