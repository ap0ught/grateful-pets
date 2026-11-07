using System.Collections.Generic;
using XRL.World.Conversations;
using XRL.World.Effects;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanGiveDirectionsEvent : IConversationMinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CanGiveDirectionsEvent), null, CountPool, ResetPool);

	private static List<CanGiveDirectionsEvent> Pool;

	private static int PoolCounter;

	public bool PlayerCompanion;

	public bool CanGiveDirections;

	public CanGiveDirectionsEvent()
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

	public static void ResetTo(ref CanGiveDirectionsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CanGiveDirectionsEvent FromPool()
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
		PlayerCompanion = false;
		CanGiveDirections = false;
	}

	public static bool Check(GameObject Actor, GameObject SpeakingWith, GameObject Transmitter, GameObject Receiver, Conversation Conversation, bool CanTrade = false, bool Physical = false, bool Mental = false)
	{
		bool flag = !SpeakingWith.IsPlayer() && (SpeakingWith.GetIntProperty("TurnsAsPlayerMinion") > 0 || SpeakingWith.IsPlayerLed());
		bool flag2 = !flag && (SpeakingWith.HasTagOrProperty("ClearLost") || SpeakingWith.IsPlayer()) && !SpeakingWith.HasEffect<Lost>() && !SpeakingWith.HasEffect<Confused>() && !SpeakingWith.HasEffect<FuriouslyConfused>();
		bool flag3 = true;
		if (flag3)
		{
			bool flag4 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("CanGiveDirections");
			bool flag5 = GameObject.Validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("CanGiveDirections");
			if (flag4 | flag5)
			{
				Event obj = Event.New("CanGiveDirections");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("SpeakingWith", SpeakingWith);
				obj.SetParameter("Conversation", Conversation);
				obj.SetFlag("CanTrade", CanTrade);
				obj.SetFlag("Physical", Physical);
				obj.SetFlag("Mental", Mental);
				obj.SetFlag("PlayerCompanion", flag);
				obj.SetFlag("CanGiveDirections", flag2);
				if (flag3 & flag4)
				{
					flag3 = Actor.FireEvent(obj);
				}
				if (flag3 & flag5)
				{
					flag3 = SpeakingWith.FireEvent(obj);
				}
				flag2 = obj.HasFlag("CanGiveDirections");
			}
		}
		if (flag3)
		{
			CanGiveDirectionsEvent canGiveDirectionsEvent = FromPool();
			canGiveDirectionsEvent.Actor = Actor;
			canGiveDirectionsEvent.SpeakingWith = SpeakingWith;
			canGiveDirectionsEvent.Transmitter = Transmitter;
			canGiveDirectionsEvent.Receiver = Receiver;
			canGiveDirectionsEvent.Conversation = Conversation;
			canGiveDirectionsEvent.CanTrade = CanTrade;
			canGiveDirectionsEvent.Physical = Physical;
			canGiveDirectionsEvent.Mental = Mental;
			canGiveDirectionsEvent.PlayerCompanion = flag;
			canGiveDirectionsEvent.CanGiveDirections = flag2;
			flag3 = IConversationMinEvent.DispatchAll(canGiveDirectionsEvent);
			flag2 = canGiveDirectionsEvent.CanGiveDirections;
		}
		return flag2;
	}
}
