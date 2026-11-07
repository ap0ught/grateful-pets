using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired when choosing a text element for preparation and can control the chosen text.</summary>
[ConversationEvent(Action = Action.Custom)]
public class GetTextElementEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(GetTextElementEvent), null, CountPool, ResetPool);

	private static List<GetTextElementEvent> Pool;

	private static int PoolCounter;

	[Parameter(Exclude = true)]
	public List<ConversationText> Texts = new List<ConversationText>();

	[Parameter(Exclude = true)]
	public List<ConversationText> Visible = new List<ConversationText>();

	[Parameter(Exclude = true)]
	public List<ConversationText> Group = new List<ConversationText>();

	[Parameter(Reference = true)]
	public ConversationText Selected;

	public GetTextElementEvent()
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

	public static void ResetTo(ref GetTextElementEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetTextElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetTextElementEvent FromPool(IConversationElement Element, ConversationText Selected)
	{
		GetTextElementEvent getTextElementEvent = FromPool();
		getTextElementEvent.Element = Element;
		getTextElementEvent.Selected = Selected;
		return getTextElementEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Texts.Clear();
		Visible.Clear();
		Group.Clear();
		Selected = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}
}
