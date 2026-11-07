using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired as an element is being left and can prevent navigation.</summary>
[ConversationEvent(Action = Action.Check)]
public class LeaveElementEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(LeaveElementEvent), null, CountPool, ResetPool);

	private static List<LeaveElementEvent> Pool;

	private static int PoolCounter;

	public LeaveElementEvent()
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

	public static void ResetTo(ref LeaveElementEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static LeaveElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static LeaveElementEvent FromPool(IConversationElement Element)
	{
		LeaveElementEvent leaveElementEvent = FromPool();
		leaveElementEvent.Element = Element;
		return leaveElementEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static bool Check(IConversationElement Element)
	{
		if (Element.WantEvent(ID))
		{
			LeaveElementEvent e = FromPool(Element);
			return Element.HandleEvent(e);
		}
		return true;
	}
}
