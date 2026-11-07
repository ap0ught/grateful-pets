using System.Collections.Generic;

namespace XRL.World.Conversations;

/// <summary>Fired when preparing spoken text for display after a node has been entered.</summary>
/// <remarks>This comes after the standard variable replacements like =subject.name=.</remarks>
[ConversationEvent(Action = Action.Send)]
public class PrepareTextLateEvent : ConversationEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(PrepareTextLateEvent), null, CountPool, ResetPool);

	private static List<PrepareTextLateEvent> Pool;

	private static int PoolCounter;

	[Parameter(Reference = true)]
	public string Text;

	[Parameter(Required = true)]
	public GameObject Subject;

	[Parameter(Required = true)]
	public GameObject Object;

	public PrepareTextLateEvent()
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

	public static void ResetTo(ref PrepareTextLateEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static PrepareTextLateEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PrepareTextLateEvent FromPool(IConversationElement Element, GameObject Subject, GameObject Object, string Text)
	{
		PrepareTextLateEvent prepareTextLateEvent = FromPool();
		prepareTextLateEvent.Element = Element;
		prepareTextLateEvent.Subject = Subject;
		prepareTextLateEvent.Object = Object;
		prepareTextLateEvent.Text = Text;
		return prepareTextLateEvent;
	}

	public override void Reset()
	{
		base.Reset();
		Subject = null;
		Object = null;
		Text = null;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element, GameObject Subject, GameObject Object, ref string Text)
	{
		if (Element.WantEvent(ID))
		{
			PrepareTextLateEvent prepareTextLateEvent = FromPool(Element, Subject, Object, Text);
			Element.HandleEvent(prepareTextLateEvent);
			Text = prepareTextLateEvent.Text;
		}
	}
}
