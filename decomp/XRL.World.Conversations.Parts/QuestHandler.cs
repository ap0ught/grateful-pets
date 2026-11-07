namespace XRL.World.Conversations.Parts;

public class QuestHandler : IConversationPart
{
	public const byte ACT_NONE = 0;

	public const byte ACT_START = 1;

	public const byte ACT_STEP = 2;

	public const byte ACT_FINISH = 3;

	public const byte ACT_COMPLETE = 4;

	public string QuestID;

	public string StepID;

	public string Text;

	public int XP = -1;

	public int Type;

	public string Action
	{
		set
		{
			Type = value.ToLowerInvariant() switch
			{
				"start" => 1, 
				"step" => 2, 
				"finish" => 3, 
				"complete" => 4, 
				_ => 0, 
			};
		}
	}

	public QuestHandler()
	{
		Priority = 1000;
	}

	public QuestHandler(string QuestID, string StepID = null, int XP = -1, int Type = 1)
		: this()
	{
		this.QuestID = QuestID;
		this.StepID = StepID;
		this.XP = XP;
		this.Type = Type;
	}

	public override void LoadText(string Text)
	{
		this.Text = Text;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		if (!Text.IsNullOrEmpty())
		{
			E.Tag = Text;
			return false;
		}
		if (Type == 1 && !QuestID.IsNullOrEmpty())
		{
			if (QuestLoader.Loader.QuestsByID.TryGetValue(QuestID, out var value) && !value.BonusAtLevel.IsNullOrEmpty())
			{
				E.Tag = "{{W|[Accept Quest - level-based reward]}}";
			}
			else
			{
				E.Tag = "{{W|[Accept Quest]}}";
			}
			return false;
		}
		if (Type == 2 && !QuestID.IsNullOrEmpty() && !StepID.IsNullOrEmpty())
		{
			E.Tag = "{{W|[Complete Quest Step]}}";
			return false;
		}
		if (Type >= 3 && !QuestID.IsNullOrEmpty())
		{
			if (QuestLoader.Loader.QuestsByID.TryGetValue(QuestID, out var value2) && !value2.BonusAtLevel.IsNullOrEmpty())
			{
				E.Tag = "{{W|[Complete Quest - level-based reward]}}";
			}
			else
			{
				E.Tag = "{{W|[Complete Quest]}}";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (Type == 1 && !QuestID.IsNullOrEmpty())
		{
			The.Game.StartQuest(QuestID, The.Speaker?.DisplayName);
		}
		if (Type == 2 && !QuestID.IsNullOrEmpty() && !StepID.IsNullOrEmpty())
		{
			The.Game.FinishQuestStep(QuestID, StepID, XP);
		}
		if (Type == 3 && !QuestID.IsNullOrEmpty())
		{
			The.Game.FinishQuest(QuestID);
		}
		if (Type == 4 && !QuestID.IsNullOrEmpty())
		{
			The.Game.CompleteQuest(QuestID);
		}
		return base.HandleEvent(E);
	}
}
