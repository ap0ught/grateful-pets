using System.Collections.Generic;
using System.Text;

namespace XRL.World.Conversations;

/// <summary>Fired before displaying the prepared text to screen.</summary>
/// <remarks>This is where you will typically add unspoken text like tooltips or other metagame information.</remarks>
[ConversationEvent(Action = Action.Send)]
public class DisplayTextEvent : ITemplateTextEvent
{
	public new static readonly int ID = ConversationEvent.RegisterEvent(typeof(DisplayTextEvent), null, CountPool, ResetPool);

	private static List<DisplayTextEvent> Pool;

	private static int PoolCounter;

	[Parameter(Output = true)]
	public bool VariableReplace;

	public DisplayTextEvent()
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

	public static void ResetTo(ref DisplayTextEvent E)
	{
		ConversationEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static DisplayTextEvent FromPool()
	{
		return ConversationEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static DisplayTextEvent FromPool(IConversationElement Element, StringBuilder Text, GameObject Subject, GameObject Object)
	{
		DisplayTextEvent displayTextEvent = FromPool();
		displayTextEvent.Element = Element;
		displayTextEvent.Text = Text;
		displayTextEvent.Subject = Subject;
		displayTextEvent.Object = Object;
		return displayTextEvent;
	}

	public override void Reset()
	{
		base.Reset();
		VariableReplace = false;
	}

	public override bool HandlePartDispatch(IConversationPart Part)
	{
		return Part.HandleEvent(this);
	}

	public static void Send(IConversationElement Element, StringBuilder Text, ref GameObject Subject, ref GameObject Object, out string ExplicitSubject, out bool ExplicitSubjectPlural, out string ExplicitObject, out bool ExplicitObjectPlural, out bool VariableReplace)
	{
		if (Element.WantEvent(ID))
		{
			DisplayTextEvent displayTextEvent = FromPool(Element, Text, Subject, Object);
			Element.HandleEvent(displayTextEvent);
			Subject = displayTextEvent.Subject;
			Object = displayTextEvent.Object;
			ExplicitSubject = displayTextEvent.ExplicitSubject;
			ExplicitSubjectPlural = displayTextEvent.ExplicitSubjectPlural;
			ExplicitObject = displayTextEvent.ExplicitObject;
			ExplicitObjectPlural = displayTextEvent.ExplicitObjectPlural;
			VariableReplace = displayTextEvent.VariableReplace;
		}
		else
		{
			ExplicitSubject = null;
			ExplicitSubjectPlural = false;
			ExplicitObject = null;
			ExplicitObjectPlural = false;
			VariableReplace = false;
		}
	}
}
