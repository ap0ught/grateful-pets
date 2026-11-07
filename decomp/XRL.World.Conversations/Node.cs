namespace XRL.World.Conversations;

public class Node : IConversationElement
{
	public bool AllowEscape = true;

	public override int Propagation => 2;
}
