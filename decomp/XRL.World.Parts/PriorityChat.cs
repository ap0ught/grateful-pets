using System;

namespace XRL.World.Parts;

[Serializable]
public class PriorityChat : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (ID != CanSmartUseEvent.ID)
		{
			return ID == CommandSmartUseEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && ParentObject.HasPart<ConversationScript>())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && ParentObject.TryGetPart<ConversationScript>(out var Part) && Part.AttemptConversation())
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
