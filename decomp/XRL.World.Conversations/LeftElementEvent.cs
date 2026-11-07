using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired after an element has successfully been exited.</summary>
[ConversationEvent(Action = Action.Send)]
public class LeftElementEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(LeftElementEvent), null, CountPool, ResetPool);

	private static List<LeftElementEvent> Pool;

	private static int PoolCounter;

	public LeftElementEvent()
		: base(ID)
	{
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

	public static void ResetTo(ref LeftElementEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static LeftElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static LeftElementEvent FromPool(IConversationElement Element)
	{
		LeftElementEvent leftElementEvent = FromPool();
		leftElementEvent.Element = Element;
		return leftElementEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element)
	{
		if (Element.WantEvent(ID))
		{
			LeftElementEvent e = FromPool(Element);
			Element.HandleEvent(e);
		}
	}
}
