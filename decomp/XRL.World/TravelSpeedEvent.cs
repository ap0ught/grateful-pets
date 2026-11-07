using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class TravelSpeedEvent : ITravelEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(TravelSpeedEvent), null, CountPool, ResetPool);

	private static List<TravelSpeedEvent> Pool;

	private static int PoolCounter;

	public TravelSpeedEvent()
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

	public static void ResetTo(ref TravelSpeedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static TravelSpeedEvent FromPool()
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

	public static int GetFor(GameObject Actor, string TravelClass = null, int PercentageBonus = 0)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("TravelSpeed"))
		{
			Event obj = Event.New("TravelSpeed");
			obj.SetParameter("Object", Actor);
			obj.SetParameter("TravelClass", TravelClass);
			obj.SetParameter("PercentageBonus", PercentageBonus);
			flag = Actor.FireEvent(obj);
			PercentageBonus = obj.GetIntParameter("PercentageBonus");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, ITravelEvent.CascadeLevel))
		{
			TravelSpeedEvent travelSpeedEvent = FromPool();
			travelSpeedEvent.Actor = Actor;
			travelSpeedEvent.TravelClass = TravelClass;
			travelSpeedEvent.PercentageBonus = PercentageBonus;
			flag = Actor.HandleEvent(travelSpeedEvent);
			PercentageBonus = travelSpeedEvent.PercentageBonus;
		}
		return PercentageBonus;
	}
}
