using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.World.Conversations;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeginConversationEvent : IConversationMinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeginConversationEvent), null, CountPool, ResetPool);

	private static List<BeginConversationEvent> Pool;

	private static int PoolCounter;

	public Node StartNode;

	public IRenderable Icon;

	public BeginConversationEvent()
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

	public static void ResetTo(ref BeginConversationEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeginConversationEvent FromPool()
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
		StartNode = null;
		Icon = null;
	}

	public static bool Check(GameObject Actor, GameObject SpeakingWith, GameObject Transmitter, GameObject Receiver, Conversation Conversation, Node StartNode, ref IRenderable Icon, bool CanTrade = false, bool Physical = false, bool Mental = false)
	{
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("BeginConversation");
			bool flag3 = GameObject.Validate(ref SpeakingWith) && SpeakingWith.HasRegisteredEvent("BeginConversation");
			if (flag2 | flag3)
			{
				Event obj = Event.New("BeginConversation");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("SpeakingWith", SpeakingWith);
				obj.SetParameter("Conversation", Conversation);
				obj.SetParameter("Icon", Icon);
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
				Icon = obj.GetParameter<IRenderable>("Icon");
			}
		}
		if (flag)
		{
			BeginConversationEvent beginConversationEvent = FromPool();
			beginConversationEvent.Actor = Actor;
			beginConversationEvent.SpeakingWith = SpeakingWith;
			beginConversationEvent.Transmitter = Transmitter;
			beginConversationEvent.Receiver = Receiver;
			beginConversationEvent.Conversation = Conversation;
			beginConversationEvent.StartNode = StartNode;
			beginConversationEvent.Icon = Icon;
			beginConversationEvent.CanTrade = CanTrade;
			beginConversationEvent.Physical = Physical;
			beginConversationEvent.Mental = Mental;
			flag = IConversationMinEvent.DispatchAll(beginConversationEvent);
			Icon = beginConversationEvent.Icon;
		}
		return flag;
	}
}
