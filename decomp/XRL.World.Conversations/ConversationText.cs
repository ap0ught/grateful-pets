namespace XRL.World.Conversations;

public class ConversationText : IConversationElement
{
	public override int Propagation => Parent?.Propagation ?? 0;
}
