using System.Collections.Generic;
using XRL.World.Conversations;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeConversationEvent : IConversationMinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeConversationEvent), null, CountPool, ResetPool);

	private static List<BeforeConversationEvent> Pool;

	private static int PoolCounter;

	public BeforeConversationEvent()
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

	public static void ResetTo(ref BeforeConversationEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeConversationEvent FromPool()
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

	public static bool Check(GameObject Actor, GameObject SpeakingWith, GameObject Transmitter, GameObject Receiver, Conversation Conversation, bool CanTrade = false, bool Physical = false, bool Mental = false)
	{
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("BeforeConversation");
			bool flag3 = GameObject.Validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("BeforeConversation");
			if (flag2 | flag3)
			{
				Event obj = Event.New("BeforeConversation");
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
			BeforeConversationEvent beforeConversationEvent = FromPool();
			beforeConversationEvent.Actor = Actor;
			beforeConversationEvent.SpeakingWith = SpeakingWith;
			beforeConversationEvent.Transmitter = Transmitter;
			beforeConversationEvent.Receiver = Receiver;
			beforeConversationEvent.Conversation = Conversation;
			beforeConversationEvent.CanTrade = CanTrade;
			beforeConversationEvent.Physical = Physical;
			beforeConversationEvent.Mental = Mental;
			flag = IConversationMinEvent.DispatchAll(beforeConversationEvent);
		}
		return flag;
	}
}
