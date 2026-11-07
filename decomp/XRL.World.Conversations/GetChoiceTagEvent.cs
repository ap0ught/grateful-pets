using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired when selecting an ending tag to apply to the display text such as [begin trade].</summary>
[ConversationEvent(Action = Action.Get)]
public class GetChoiceTagEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(GetChoiceTagEvent), null, CountPool, ResetPool);

	private static List<GetChoiceTagEvent> Pool;

	private static int PoolCounter;

	[Parameter(Get = true)]
	public string Tag;

	public GetChoiceTagEvent()
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

	public static void ResetTo(ref GetChoiceTagEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetChoiceTagEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetChoiceTagEvent FromPool(IConversationElement Element, string Tag = null)
	{
		GetChoiceTagEvent getChoiceTagEvent = FromPool();
		getChoiceTagEvent.Element = Element;
		getChoiceTagEvent.Tag = Tag;
		return getChoiceTagEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Tag = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static string For(IConversationElement Element, string Tag = null)
	{
		if (Element.WantEvent(ID))
		{
			GetChoiceTagEvent getChoiceTagEvent = FromPool(Element, Tag);
			Element.HandleEvent(getChoiceTagEvent);
			return getChoiceTagEvent.Tag;
		}
		return null;
	}

	public static bool Try(IConversationElement Element, out string Tag)
	{
		if (Element.WantEvent(ID))
		{
			GetChoiceTagEvent getChoiceTagEvent = new GetChoiceTagEvent
			{
				Element = Element
			};
			if (!Element.HandleEvent(getChoiceTagEvent))
			{
				Tag = getChoiceTagEvent.Tag;
				return true;
			}
		}
		Tag = null;
		return false;
	}
}
