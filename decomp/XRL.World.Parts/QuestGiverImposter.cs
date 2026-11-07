using System;

namespace XRL.World.Parts;

[Serializable]
[Obsolete]
public class QuestGiverImposter : IPartWithPrefabImposter
{
	public string GivesQuests = "";

	public string AcceptsQuests = "";

	[NonSerialized]
	public string[] GivesQuestList;

	[NonSerialized]
	public string[] AcceptsQuestList;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterConversationEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != EnteredCellEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterConversationEvent E)
	{
		UpdateIcon();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		UpdateIcon();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		UpdateIcon();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		UpdateIcon();
		return base.HandleEvent(E);
	}

	public void UpdateIcon()
	{
		if (GivesQuestList == null)
		{
			GivesQuestList = GivesQuests.Split(',');
		}
		if (AcceptsQuestList == null)
		{
			AcceptsQuestList = AcceptsQuests.Split(',');
		}
		string[] givesQuestList = GivesQuestList;
		foreach (string iD in givesQuestList)
		{
			if (!IComponent<GameObject>.TheGame.HasQuest(iD))
			{
				prefabID = "Prefabs/Imposters/QuestAvailable";
				return;
			}
		}
		givesQuestList = AcceptsQuestList;
		foreach (string text in givesQuestList)
		{
			if (IComponent<GameObject>.TheGame.HasQuest(text) && !IComponent<GameObject>.TheGame.FinishedQuests.ContainsKey(text) && IComponent<GameObject>.TheGame.Quests[text].ReadyToTurnIn())
			{
				prefabID = "Prefabs/Imposters/QuestComplete";
				return;
			}
		}
		prefabID = null;
	}
}
