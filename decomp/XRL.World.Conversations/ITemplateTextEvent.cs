using System.Text;

namespace XRL.World.Conversations;

[ConversationEvent(Base = true)]
public abstract class ITemplateTextEvent : ConversationEvent
{
	[Parameter(Required = true)]
	public StringBuilder Text;

	[Parameter(Reference = true)]
	public GameObject Subject;

	[Parameter(Reference = true)]
	public GameObject Object;

	[Parameter(Output = true)]
	public string ExplicitSubject;

	[Parameter(Output = true)]
	public bool ExplicitSubjectPlural;

	[Parameter(Output = true)]
	public string ExplicitObject;

	[Parameter(Output = true)]
	public bool ExplicitObjectPlural;

	public ITemplateTextEvent(int ID)
		: base(ID)
	{
	}

	public override void Reset()
	{
		base.Reset();
		Text = null;
		Subject = null;
		Object = null;
		ExplicitSubject = null;
		ExplicitSubjectPlural = false;
		ExplicitObject = null;
		ExplicitObjectPlural = false;
	}
}
