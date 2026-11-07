using XRL.World.Quests;

namespace XRL.World.Conversations.Parts;

public class GritGateHandler : IConversationPart
{
	public string Rank;

	public int Door = -1;

	public bool Invasion;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		string text = Rank?.ToUpperInvariant();
		if (!text.IsNullOrEmpty())
		{
			switch (text)
			{
			case "APPRENTICE":
				GritGateScripts.PromoteToApprentice();
				break;
			case "JOURNEYFRIEND":
				GritGateScripts.PromoteToJourneyfriend();
				break;
			case "DISCIPLE":
				GritGateScripts.PromoteToDisciple();
				break;
			case "MEYVN":
				GritGateScripts.PromoteToMeyvn();
				break;
			}
		}
		if (Door >= 0)
		{
			GritGateScripts.OpenGritGateDoors(Door);
		}
		if (Invasion)
		{
			GritGateScripts.BeginInvasion();
		}
		return base.HandleEvent(E);
	}
}
