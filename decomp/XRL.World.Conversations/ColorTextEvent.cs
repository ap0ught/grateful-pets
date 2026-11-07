using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired when coloring the display text of an element.</summary>
/// <see cref="T:XRL.World.Conversations.DisplayTextEvent" />
[ConversationEvent(Action = Action.Send)]
public class ColorTextEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(ColorTextEvent), null, CountPool, ResetPool);

	private static List<ColorTextEvent> Pool;

	private static int PoolCounter;

	[Parameter(Reference = true)]
	public string Color;

	public ColorTextEvent()
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

	public static void ResetTo(ref ColorTextEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ColorTextEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static ColorTextEvent FromPool(IConversationElement Element, string Color)
	{
		ColorTextEvent colorTextEvent = FromPool();
		colorTextEvent.Element = Element;
		colorTextEvent.Color = Color;
		return colorTextEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Color = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element, ref string Color)
	{
		if (Element.WantEvent(ID))
		{
			ColorTextEvent colorTextEvent = FromPool(Element, Color);
			Element.HandleEvent(colorTextEvent);
			Color = colorTextEvent.Color;
		}
	}
}
