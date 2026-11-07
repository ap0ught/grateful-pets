using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetWaterRitualReputationAmountEvent : PooledEvent<GetWaterRitualReputationAmountEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject SpeakingWith;

	public WaterRitualRecord Record;

	public string Faction;

	public int BaseAmount;

	public int Amount;

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
		SpeakingWith = null;
		Record = null;
		Faction = null;
		BaseAmount = 0;
		Amount = 0;
	}

	public static int GetFor(GameObject Actor, GameObject SpeakingWith, WaterRitualRecord Record, string Faction, int BaseAmount)
	{
		int num = BaseAmount;
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetWaterRitualReputationAmount"))
		{
			Event obj = Event.New("GetWaterRitualReputationAmount");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("SpeakingWith", SpeakingWith);
			obj.SetParameter("Record", Record);
			obj.SetParameter("Faction", Faction);
			obj.SetParameter("BaseAmount", BaseAmount);
			obj.SetParameter("Amount", num);
			flag = Actor.FireEvent(obj);
			num = obj.GetIntParameter("Amount");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetWaterRitualReputationAmountEvent>.ID, CascadeLevel))
		{
			GetWaterRitualReputationAmountEvent getWaterRitualReputationAmountEvent = PooledEvent<GetWaterRitualReputationAmountEvent>.FromPool();
			getWaterRitualReputationAmountEvent.Actor = Actor;
			getWaterRitualReputationAmountEvent.SpeakingWith = SpeakingWith;
			getWaterRitualReputationAmountEvent.Record = Record;
			getWaterRitualReputationAmountEvent.Faction = Faction;
			getWaterRitualReputationAmountEvent.BaseAmount = BaseAmount;
			getWaterRitualReputationAmountEvent.Amount = num;
			flag = Actor.HandleEvent(getWaterRitualReputationAmountEvent);
			num = getWaterRitualReputationAmountEvent.Amount;
		}
		return num;
	}
}
