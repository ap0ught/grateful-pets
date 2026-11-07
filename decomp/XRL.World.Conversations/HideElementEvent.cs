using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired when evaluating elements to display that are hidden by special outside conditions.</summary>
/// <remarks>One such condition is the last choice selected that will be hidden if navigation was successful but did not leave the current node.</remarks>
[ConversationEvent(Action = Action.Check)]
public class HideElementEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(HideElementEvent), null, CountPool, ResetPool);

	private static List<HideElementEvent> Pool;

	private static int PoolCounter;

	public string Context;

	public HideElementEvent()
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

	public static void ResetTo(ref HideElementEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static HideElementEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static HideElementEvent FromPool(IConversationElement Element, string Context = null)
	{
		HideElementEvent hideElementEvent = FromPool();
		hideElementEvent.Element = Element;
		hideElementEvent.Context = Context;
		return hideElementEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Context = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static bool Check(IConversationElement Element, string Context = null)
	{
		if (Element.WantEvent(ID))
		{
			HideElementEvent e = FromPool(Element, Context);
			return Element.HandleEvent(e);
		}
		return true;
	}
}
