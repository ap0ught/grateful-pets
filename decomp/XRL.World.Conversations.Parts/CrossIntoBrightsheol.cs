using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class CrossIntoBrightsheol : IConversationPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnterElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{M|[lesser victory]}}";
		return false;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		return ThinWorld.CrossIntoBrightsheol();
	}
}
