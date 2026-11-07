using System.Collections.Generic;
using XRL.World.Conversations;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GaveDirectionsEvent : IConversationMinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GaveDirectionsEvent), null, CountPool, ResetPool);

	private static List<GaveDirectionsEvent> Pool;

	private static int PoolCounter;

	public GaveDirectionsEvent()
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

	public static void ResetTo(ref GaveDirectionsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GaveDirectionsEvent FromPool()
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
		GaveDirectionsEvent gaveDirectionsEvent = FromPool();
		gaveDirectionsEvent.Actor = Actor;
		gaveDirectionsEvent.SpeakingWith = SpeakingWith;
		gaveDirectionsEvent.Transmitter = Transmitter;
		gaveDirectionsEvent.Receiver = Receiver;
		gaveDirectionsEvent.Conversation = Conversation;
		gaveDirectionsEvent.CanTrade = CanTrade;
		gaveDirectionsEvent.Physical = Physical;
		gaveDirectionsEvent.Mental = Mental;
		IConversationMinEvent.DispatchAll(gaveDirectionsEvent);
	}
}
