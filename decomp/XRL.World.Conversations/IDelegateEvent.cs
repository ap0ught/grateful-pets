namespace XRL.World.Conversations;

[ConversationEvent(Base = true)]
public abstract class IDelegateEvent : ConversationEvent
{
	[Parameter(Required = true)]
	public string Delegate;

	[Parameter(Required = true)]
	public string Command;

	public IDelegateEvent(int ID)
		: base(ID)
	{
	}

	public override void Reset()
	{
		base.Reset();
		Delegate = null;
		Command = null;
	}
}
