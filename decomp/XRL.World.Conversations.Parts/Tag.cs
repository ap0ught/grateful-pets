namespace XRL.World.Conversations.Parts;

public class Tag : IConversationPart
{
	public string Text;

	public Tag()
	{
		Priority = -500;
	}

	public override void LoadText(string Text)
	{
		this.Text = Text;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == GetChoiceTagEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = Text;
		return false;
	}
}
