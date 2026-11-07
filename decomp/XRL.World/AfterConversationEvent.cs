using System.Collections.Generic;
using XRL.World.Conversations;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterConversationEvent : IConversationMinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterConversationEvent), null, CountPool, ResetPool);

	private static List<AfterConversationEvent> Pool;

	private static int PoolCounter;

	public AfterConversationEvent()
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

	public static void ResetTo(ref AfterConversationEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterConversationEvent FromPool()
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

	public static void Send(GameObject Actor, GameObject SpeakingWith, GameObject Transmitter, GameObject Receiver, Conversation Conversation, bool CanTrade = false, bool Physical = false, bool Mental = false)
	{
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AfterConversation");
			bool flag3 = GameObject.Validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("AfterConversation");
			if (flag2 | flag3)
			{
				Event obj = Event.New("AfterConversation");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("SpeakingWith", SpeakingWith);
				obj.SetParameter("Conversation", Conversation);
				obj.SetFlag("CanTrade", CanTrade);
				obj.SetFlag("Physical", Physical);
				obj.SetFlag("Mental", Mental);
				if (flag & flag2)
				{
					flag = Actor.FireEvent(obj);
				}
				if (flag & flag3)
				{
					flag = SpeakingWith.FireEvent(obj);
				}
			}
		}
		if (flag)
		{
			AfterConversationEvent afterConversationEvent = FromPool();
			afterConversationEvent.Actor = Actor;
			afterConversationEvent.SpeakingWith = SpeakingWith;
			afterConversationEvent.Transmitter = Transmitter;
			afterConversationEvent.Receiver = Receiver;
			afterConversationEvent.Conversation = Conversation;
			afterConversationEvent.CanTrade = CanTrade;
			afterConversationEvent.Physical = Physical;
			afterConversationEvent.Mental = Mental;
			flag = IConversationMinEvent.DispatchAll(afterConversationEvent);
		}
	}
}
