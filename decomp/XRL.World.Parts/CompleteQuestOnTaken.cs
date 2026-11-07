using System;

namespace XRL.World.Parts;

[Serializable]
public class CompleteQuestOnTaken : IPart
{
	public string Quest;

	public string QuestStep;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != TakenEvent.ID && ID != DroppedEvent.ID && ID != QuestStartedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		CompleteQuest(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CompleteQuest(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		CompleteQuest(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		CompleteQuest(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QuestStartedEvent E)
	{
		if (E.Quest.Name == Quest)
		{
			CompleteQuest(The.Player);
		}
		return base.HandleEvent(E);
	}

	public void CompleteQuest(GameObject Actor)
	{
		if (Actor != null && Actor.IsPlayer())
		{
			The.Game.FinishQuestStep(Quest, QuestStep);
		}
	}
}
