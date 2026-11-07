using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired after an element has successfully been entered.</summary>
[ConversationEvent(Action = Action.Send)]
public class EnteredElementEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(EnteredElementEvent), null, CountPool, ResetPool);

	private static List<EnteredElementEvent> Pool;

	private static int PoolCounter;

	public EnteredElementEvent()
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

	public static void ResetTo(ref EnteredElementEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EnteredElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static EnteredElementEvent FromPool(IConversationElement Element)
	{
		EnteredElementEvent enteredElementEvent = FromPool();
		enteredElementEvent.Element = Element;
		return enteredElementEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element)
	{
		if (Element.WantEvent(ID))
		{
			EnteredElementEvent e = FromPool(Element);
			Element.HandleEvent(e);
		}
	}
}
