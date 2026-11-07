namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetWaterRitualSellSecretBehaviorEvent : PooledEvent<GetWaterRitualSellSecretBehaviorEvent>
{
	public GameObject Actor;

	public GameObject SpeakingWith;

	public string Message;

	public int ReputationProvided;

	public int BonusReputationProvided;

	public bool IsSecret;

	public bool IsGossip;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		SpeakingWith = null;
		Message = null;
		ReputationProvided = 0;
		BonusReputationProvided = 0;
		IsSecret = false;
		IsGossip = false;
	}

	public static void Send(GameObject Actor, GameObject SpeakingWith, ref string Message, ref int ReputationProvided, ref int BonusReputationProvided, bool IsSecret = false, bool IsGossip = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetWaterRitualSellSecretBehavior"))
		{
			Event obj = Event.New("GetWaterRitualSellSecretBehavior");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("SpeakingWith", SpeakingWith);
			obj.SetParameter("Message", Message);
			obj.SetParameter("ReputationProvided", ReputationProvided);
			obj.SetParameter("BonusReputationProvided", BonusReputationProvided);
			obj.SetFlag("IsSecret", IsSecret);
			obj.SetFlag("IsGossip", IsGossip);
			flag = Actor.FireEvent(obj);
			Message = obj.GetStringParameter("Message");
			ReputationProvided = obj.GetIntParameter("ReputationProvided");
			BonusReputationProvided = obj.GetIntParameter("BonusReputationProvided");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetWaterRitualSellSecretBehaviorEvent>.ID, MinEvent.CascadeLevel))
		{
			GetWaterRitualSellSecretBehaviorEvent getWaterRitualSellSecretBehaviorEvent = PooledEvent<GetWaterRitualSellSecretBehaviorEvent>.FromPool();
			getWaterRitualSellSecretBehaviorEvent.Actor = Actor;
			getWaterRitualSellSecretBehaviorEvent.SpeakingWith = SpeakingWith;
			getWaterRitualSellSecretBehaviorEvent.Message = Message;
			getWaterRitualSellSecretBehaviorEvent.ReputationProvided = ReputationProvided;
			getWaterRitualSellSecretBehaviorEvent.BonusReputationProvided = BonusReputationProvided;
			getWaterRitualSellSecretBehaviorEvent.IsSecret = IsSecret;
			getWaterRitualSellSecretBehaviorEvent.IsGossip = IsGossip;
			flag = Actor.HandleEvent(getWaterRitualSellSecretBehaviorEvent);
			Message = getWaterRitualSellSecretBehaviorEvent.Message;
			ReputationProvided = getWaterRitualSellSecretBehaviorEvent.ReputationProvided;
			BonusReputationProvided = getWaterRitualSellSecretBehaviorEvent.BonusReputationProvided;
		}
	}
}
