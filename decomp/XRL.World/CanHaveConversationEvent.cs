using System.Collections.Generic;
using XRL.World.Conversations;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class CanHaveConversationEvent : IConversationMinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CanHaveConversationEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<CanHaveConversationEvent> Pool;

	private static int PoolCounter;

	public CanHaveConversationEvent()
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

	public static void ResetTo(ref CanHaveConversationEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CanHaveConversationEvent FromPool()
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
			bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("CanHaveConversation");
			bool flag3 = GameObject.Validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("CanHaveConversation");
			if (flag2 | flag3)
			{
				Event obj = Event.New("CanHaveConversation");
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
			CanHaveConversationEvent canHaveConversationEvent = FromPool();
			canHaveConversationEvent.Actor = Actor;
			canHaveConversationEvent.SpeakingWith = SpeakingWith;
			canHaveConversationEvent.Transmitter = Transmitter;
			canHaveConversationEvent.Receiver = Receiver;
			canHaveConversationEvent.Conversation = Conversation;
			canHaveConversationEvent.CanTrade = CanTrade;
			canHaveConversationEvent.Physical = Physical;
			canHaveConversationEvent.Mental = Mental;
			flag = IConversationMinEvent.DispatchAll(canHaveConversationEvent);
		}
		return flag;
	}
}
