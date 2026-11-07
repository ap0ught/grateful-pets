using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class EncounterChanceEvent : ITravelEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EncounterChanceEvent), null, CountPool, ResetPool);

	private static List<EncounterChanceEvent> Pool;

	private static int PoolCounter;

	public EncounterEntry Encounter;

	public EncounterChanceEvent()
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

	public static void ResetTo(ref EncounterChanceEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EncounterChanceEvent FromPool()
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

	public override void Reset()
	{
		base.Reset();
		Encounter = null;
	}

	public static int GetFor(GameObject Actor, string TravelClass = null, int PercentageBonus = 0, EncounterEntry Encounter = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("EncounterChance"))
		{
			Event obj = Event.New("EncounterChance");
			obj.SetParameter("Object", Actor);
			obj.SetParameter("TravelClass", TravelClass);
			obj.SetParameter("PercentageBonus", PercentageBonus);
			obj.SetParameter("Encounter", Encounter);
			flag = Actor.FireEvent(obj);
			PercentageBonus = obj.GetIntParameter("PercentageBonus");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, ITravelEvent.CascadeLevel))
		{
			EncounterChanceEvent encounterChanceEvent = FromPool();
			encounterChanceEvent.Actor = Actor;
			encounterChanceEvent.TravelClass = TravelClass;
			encounterChanceEvent.PercentageBonus = PercentageBonus;
			encounterChanceEvent.Encounter = Encounter;
			flag = Actor.HandleEvent(encounterChanceEvent);
			PercentageBonus = encounterChanceEvent.PercentageBonus;
		}
		return PercentageBonus;
	}
}
