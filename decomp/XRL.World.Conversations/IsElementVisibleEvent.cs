using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired when determining whether an element is possibly available for rendering and selection, after any predicates defined on the element.</summary>
[ConversationEvent(Action = Action.Check)]
public class IsElementVisibleEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(IsElementVisibleEvent), null, CountPool, ResetPool);

	private static List<IsElementVisibleEvent> Pool;

	private static int PoolCounter;

	public IsElementVisibleEvent()
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

	public static void ResetTo(ref IsElementVisibleEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static IsElementVisibleEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static IsElementVisibleEvent FromPool(IConversationElement Element)
	{
		IsElementVisibleEvent isElementVisibleEvent = FromPool();
		isElementVisibleEvent.Element = Element;
		return isElementVisibleEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static bool Check(IConversationElement Element)
	{
		if (Element.WantEvent(ID))
		{
			IsElementVisibleEvent e = FromPool(Element);
			return Element.HandleEvent(e);
		}
		return true;
	}
}
