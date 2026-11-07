using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class FindASpecificItemDynamicQuestManager : QuestManager
{
	[Serializable]
	public class FindASpecificItemDynamicQuestManagerSystem : IPlayerSystem
	{
		public List<FindASpecificItemDynamicQuestManager_QuestEntry> quests;

		public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
		{
			Registrar.Register(AddedToInventoryEvent.ID);
			Registrar.Register(TookEvent.ID);
			Registrar.Register(EquipperEquippedEvent.ID);
			Registrar.Register(InventoryActionEvent.ID);
		}

		public override bool HandleEvent(AddedToInventoryEvent E)
		{
			CheckCompleted(E.Item);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(TookEvent E)
		{
			CheckCompleted(E.Item);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EquipperEquippedEvent E)
		{
			CheckCompleted(E.Item);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			CheckCompleted(E.Item);
			return base.HandleEvent(E);
		}

		public void CheckCompleted(GameObject go)
		{
			if (GameObject.Validate(go) && quests != null)
			{
				int num = quests.Count - 1;
				while (num >= 0 && num < quests.Count)
				{
					FindASpecificItemDynamicQuestManager_QuestEntry findASpecificItemDynamicQuestManager_QuestEntry = quests[num];
					if (go.IDMatch(findASpecificItemDynamicQuestManager_QuestEntry.itemID))
					{
						The.Game.FinishQuestStep(findASpecificItemDynamicQuestManager_QuestEntry.questID, findASpecificItemDynamicQuestManager_QuestEntry.questStepID);
						string referenceDisplayName = The.ZoneManager.peekCachedObject(findASpecificItemDynamicQuestManager_QuestEntry.itemID).GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true);
						string text = HistoricStringExpander.ExpandString("<spice.professions.!random>");
						JournalAPI.AddAccomplishment("You recovered " + referenceDisplayName + ".", "While exploring " + The.Player.CurrentZone.DisplayName + ", =name= recovered the fabled artifact called " + referenceDisplayName + ".", "While visiting an obscure <spice.professions." + text + ".guildhall>, =name= met with a group of <spice.professions." + text + ".plural> and commissed what came to be known as the " + referenceDisplayName + ".", null, "general", MuralCategory.FindsObject, MuralWeight.Medium, null, -1L);
						quests.Remove(findASpecificItemDynamicQuestManager_QuestEntry);
					}
					num--;
				}
			}
			if (quests != null && quests.Count == 0)
			{
				The.Game.FlagSystemsForRemoval(typeof(FindASpecificItemDynamicQuestManagerSystem));
			}
		}
	}

	[Serializable]
	public class FindASpecificItemDynamicQuestManager_QuestEntry : IComposite
	{
		public string itemID;

		public string questID;

		public string questStepID;

		public bool WantFieldReflection => false;

		public void Write(SerializationWriter Writer)
		{
			Writer.WriteOptimized(itemID);
			Writer.WriteOptimized(questID);
			Writer.WriteOptimized(questStepID);
		}

		public void Read(SerializationReader Reader)
		{
			itemID = Reader.ReadOptimizedString();
			questID = Reader.ReadOptimizedString();
			questStepID = Reader.ReadOptimizedString();
		}
	}

	public string _itemID;

	public string _questID;

	public string _questStepID;

	public FindASpecificItemDynamicQuestManager()
	{
	}

	public FindASpecificItemDynamicQuestManager(string itemID, string questID, string questStepID)
	{
		_itemID = itemID;
		_questID = questID;
		_questStepID = questStepID;
	}

	public override void OnQuestAdded()
	{
		RequireEntry();
	}

	public override void AfterQuestAdded()
	{
		FindASpecificItemDynamicQuestManagerSystem manager = The.Game.RequireSystem(() => new FindASpecificItemDynamicQuestManagerSystem());
		if (manager.quests != null)
		{
			IComponent<GameObject>.ThePlayer.ForeachInventoryAndEquipment((GameObject GO) =>
			{
				manager.CheckCompleted(GO);
			});
		}
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.Find((GameObject obj) => obj.GetStringProperty("GivesDynamicQuest") == _questID);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		RequireEntry();
	}

	public FindASpecificItemDynamicQuestManager_QuestEntry RequireEntry()
	{
		if (The.Game.HasFinishedQuest(_questID))
		{
			return null;
		}
		FindASpecificItemDynamicQuestManagerSystem findASpecificItemDynamicQuestManagerSystem = The.Game.RequireSystem<FindASpecificItemDynamicQuestManagerSystem>();
		FindASpecificItemDynamicQuestManagerSystem findASpecificItemDynamicQuestManagerSystem2 = findASpecificItemDynamicQuestManagerSystem;
		if (findASpecificItemDynamicQuestManagerSystem2.quests == null)
		{
			findASpecificItemDynamicQuestManagerSystem2.quests = new List<FindASpecificItemDynamicQuestManager_QuestEntry>();
		}
		foreach (FindASpecificItemDynamicQuestManager_QuestEntry quest in findASpecificItemDynamicQuestManagerSystem.quests)
		{
			if (quest.itemID == _itemID)
			{
				return quest;
			}
		}
		FindASpecificItemDynamicQuestManager_QuestEntry findASpecificItemDynamicQuestManager_QuestEntry = new FindASpecificItemDynamicQuestManager_QuestEntry
		{
			itemID = _itemID,
			questID = _questID,
			questStepID = _questStepID
		};
		findASpecificItemDynamicQuestManagerSystem.quests.Add(findASpecificItemDynamicQuestManager_QuestEntry);
		return findASpecificItemDynamicQuestManager_QuestEntry;
	}
}
