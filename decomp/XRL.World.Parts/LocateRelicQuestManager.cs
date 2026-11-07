using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class LocateRelicQuestManager : QuestManager
{
	[Serializable]
	public class LocateRelicQuestManagerSystem : IPlayerSystem
	{
		public List<LocateRelicQuestManager_QuestEntry> quests;

		public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
		{
			Registrar.Register(AddedToInventoryEvent.ID);
			Registrar.Register(TookEvent.ID);
			Registrar.Register(EquippedEvent.ID);
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

		public override bool HandleEvent(EquippedEvent E)
		{
			CheckCompleted(E.Item);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			CheckCompleted(E.Item);
			return base.HandleEvent(E);
		}

		public void CheckCompleted(GameObject Object)
		{
			if (quests != null)
			{
				for (int num = quests.Count - 1; num >= 0; num--)
				{
					LocateRelicQuestManager_QuestEntry locateRelicQuestManager_QuestEntry = quests[num];
					if (!locateRelicQuestManager_QuestEntry.Relic.IsNullOrEmpty() && Object != null && Object.HasStringProperty("RelicName") && (Object.GetStringProperty("RelicName").EqualsExceptFormattingAndCase(locateRelicQuestManager_QuestEntry.Relic) || Object.GetStringProperty("RelicName").Replace("the the ", "the ").EqualsExceptFormattingAndCase(locateRelicQuestManager_QuestEntry.Relic)))
					{
						if (The.Game.HasFinishedQuest(locateRelicQuestManager_QuestEntry.QuestID))
						{
							MetricsManager.LogError(locateRelicQuestManager_QuestEntry.QuestID + " manager for " + locateRelicQuestManager_QuestEntry.Relic + " present after completion, removing");
							quests.RemoveAt(num);
						}
						else
						{
							The.Game.CompleteQuest(locateRelicQuestManager_QuestEntry.QuestID);
							JournalAPI.AddAccomplishment("You recovered the historic relic, " + locateRelicQuestManager_QuestEntry.Relic + ".", HistoricStringExpander.ExpandString("<spice.commonPhrases.intrepid.!random.capitalize> =name= recovered " + locateRelicQuestManager_QuestEntry.Relic + ", a historic relic once thought lost to the sands of time."), "In an excavation at a site of deep history near " + JournalAPI.GetLandmarkNearestPlayer().Text + ", =name= recovered " + locateRelicQuestManager_QuestEntry.Relic + ", the historic relic once thought lost to the sands of time.", null, "general", MuralCategory.VisitsLocation, MuralWeight.High, null, -1L);
							quests.RemoveAt(num);
						}
					}
				}
			}
			if (quests.Count == 0)
			{
				The.Game.FlagSystemsForRemoval(typeof(LocateRelicQuestManagerSystem));
			}
		}
	}

	[Serializable]
	public sealed class LocateRelicQuestManager_QuestEntry : IComposite
	{
		public string Relic;

		public string QuestID;

		public bool WantFieldReflection => false;

		public void Write(SerializationWriter Writer)
		{
			Writer.WriteOptimized(Relic);
			Writer.WriteOptimized(QuestID);
		}

		public void Read(SerializationReader Reader)
		{
			Relic = Reader.ReadOptimizedString();
			QuestID = Reader.ReadOptimizedString();
		}
	}

	public string Relic;

	public string QuestID;

	public override void OnQuestAdded()
	{
		LocateRelicQuestManagerSystem locateRelicQuestManagerSystem = The.Game.RequireSystem(() => new LocateRelicQuestManagerSystem());
		LocateRelicQuestManager_QuestEntry item = new LocateRelicQuestManager_QuestEntry
		{
			Relic = Relic,
			QuestID = QuestID
		};
		LocateRelicQuestManagerSystem locateRelicQuestManagerSystem2 = locateRelicQuestManagerSystem;
		if (locateRelicQuestManagerSystem2.quests == null)
		{
			locateRelicQuestManagerSystem2.quests = new List<LocateRelicQuestManager_QuestEntry>();
		}
		locateRelicQuestManagerSystem.quests.Add(item);
	}

	public override void AfterQuestAdded()
	{
		LocateRelicQuestManagerSystem manager = The.Game.RequireSystem(() => new LocateRelicQuestManagerSystem());
		if (manager.quests != null)
		{
			IComponent<GameObject>.ThePlayer.ForeachInventoryAndEquipment((GameObject GO) =>
			{
				manager.CheckCompleted(GO);
			});
		}
	}
}
