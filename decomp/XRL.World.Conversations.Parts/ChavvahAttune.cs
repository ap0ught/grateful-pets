using Qud.API;

namespace XRL.World.Conversations.Parts;

public class ChavvahAttune : IConversationPart
{
	public string FailTarget;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == GetTargetElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTargetElementEvent E)
	{
		if (!The.Game.GetSystem<ChavvahSystem>().Reveal())
		{
			E.Target = FailTarget;
		}
		else
		{
			JournalAPI.AddAccomplishment("You touched the chiming rock and attuned to Chavvah, the Tree of Life.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
		}
		return base.HandleEvent(E);
	}
}
