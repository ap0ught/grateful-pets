using System.Collections.Generic;
using System.Text;

namespace XRL.World.Conversations;

/// <summary>Fired when preparing spoken text for display after a node has been entered.</summary>
/// <remarks>This precedes the standard variable replacements like =subject.name= and allows setting a new Subject and Object.</remarks>
[ConversationEvent(Action = Action.Send)]
public class PrepareTextEvent : ITemplateTextEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(PrepareTextEvent), null, CountPool, ResetPool);

	private static List<PrepareTextEvent> Pool;

	private static int PoolCounter;

	public PrepareTextEvent()
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

	public static void ResetTo(ref PrepareTextEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static PrepareTextEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static PrepareTextEvent FromPool(IConversationElement Element, StringBuilder Text, GameObject Subject, GameObject Object)
	{
		PrepareTextEvent prepareTextEvent = FromPool();
		prepareTextEvent.Element = Element;
		prepareTextEvent.Text = Text;
		prepareTextEvent.Subject = Subject;
		prepareTextEvent.Object = Object;
		return prepareTextEvent;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element, StringBuilder Text, ref GameObject Subject, ref GameObject Object, out string ExplicitSubject, out bool ExplicitSubjectPlural, out string ExplicitObject, out bool ExplicitObjectPlural)
	{
		if (Element.WantEvent(ID))
		{
			PrepareTextEvent prepareTextEvent = FromPool(Element, Text, Subject, Object);
			Element.HandleEvent(prepareTextEvent);
			Subject = prepareTextEvent.Subject;
			Object = prepareTextEvent.Object;
			ExplicitSubject = prepareTextEvent.ExplicitSubject;
			ExplicitSubjectPlural = prepareTextEvent.ExplicitSubjectPlural;
			ExplicitObject = prepareTextEvent.ExplicitObject;
			ExplicitObjectPlural = prepareTextEvent.ExplicitObjectPlural;
		}
		else
		{
			ExplicitSubject = null;
			ExplicitSubjectPlural = false;
			ExplicitObject = null;
			ExplicitObjectPlural = false;
		}
	}
}
