using System;

namespace XRL.World.Parts;

[Serializable]
public class SetStateAfterConversation : IPart
{
	public string State;

	[NonSerialized]
	private bool Activated;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterConversationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterConversationEvent E)
	{
		if (!Activated)
		{
			if (!State.IsNullOrEmpty())
			{
				The.Game.SetBooleanGameState(State, Value: true);
			}
			Activated = true;
		}
		return base.HandleEvent(E);
	}
}
