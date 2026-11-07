using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class AngorNegotiation : IConversationPart
{
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
		The.Speaker.RequirePart<SpindleNegotiation>();
		The.Speaker.FireEvent("BeginSpindleNegotiation");
		return base.HandleEvent(E);
	}
}
