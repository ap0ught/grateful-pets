using XRL.World.Parts;
using XRL.World.ZoneParts;

namespace XRL.World.Conversations.Parts;

public class CallToArmsResult : IConversationPart
{
	public string Reward;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != PrepareTextEvent.ID)
		{
			return ID == LeftElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		E.Text.Insert(0, "\n\n");
		E.Text.Insert(0, ScriptCallToArms.GenerateResultConversation());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftElementEvent E)
	{
		if (!The.Game.HasFinishedQuestStep("The Assessment", "Speak to Otho"))
		{
			The.Game.FinishQuestStep("The Assessment", "Speak to Otho");
			if (Reward == "Top")
			{
				CallToArmsScore.GiveCallToArmsReward_Top();
			}
			else if (Reward == "Mid")
			{
				CallToArmsScore.GiveCallToArmsReward_Mid();
			}
			else
			{
				CallToArmsScore.GiveCallToArmsReward_Bottom();
			}
		}
		return base.HandleEvent(E);
	}
}
