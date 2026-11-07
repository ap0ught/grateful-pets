namespace XRL.World.Conversations.Parts;

public class IfThenElseAchievement : IConversationPart
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
		XRLGame game = The.Game;
		if (game.HasIntGameState("ElseingComplete") && !game.HasDelimitedGameState("TauElse", ',', "Dead") && !game.HasDelimitedGameState("TauCompanion", ',', "Dead"))
		{
			Achievement.TAU_THEN.Unlock();
		}
		else
		{
			Achievement.TAU_ELSE.Unlock();
		}
		return base.HandleEvent(E);
	}
}
