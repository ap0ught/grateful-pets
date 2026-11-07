using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Wish;

namespace XRL.World.ZoneBuilders;

[Serializable]
[HasWishCommand]
public class FindASiteDynamicQuestManager : QuestManager
{
	[Serializable]
	public class FindASiteDynamicQuestManagerSystem : IGameSystem
	{
		public List<FindASiteDynamicQuestManager_QuestEntry> quests;

		public override void Register(XRLGame Game, IEventRegistrar Registrar)
		{
			Registrar.Register(PooledEvent<SecretVisibilityChangedEvent>.ID);
			Registrar.Register(ZoneActivatedEvent.ID);
		}

		public override bool HandleEvent(SecretVisibilityChangedEvent E)
		{
			if (E.Entry is JournalMapNote { Revealed: not false } journalMapNote)
			{
				CheckCompleted(null, journalMapNote);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(ZoneActivatedEvent E)
		{
			CheckCompleted(E.Zone);
			return base.HandleEvent(E);
		}

		public void CheckCompleted(Zone Zone = null, JournalMapNote Secret = null)
		{
			if (quests != null)
			{
				for (int num = quests.Count - 1; num >= 0 && num < quests.Count; num--)
				{
					FindASiteDynamicQuestManager_QuestEntry findASiteDynamicQuestManager_QuestEntry = quests[num];
					if (Zone == null)
					{
						Zone = The.Player.CurrentZone;
					}
					if (Zone?.ZoneID != findASiteDynamicQuestManager_QuestEntry.zoneID && !The.ZoneManager.HasVisitedZone(findASiteDynamicQuestManager_QuestEntry.zoneID))
					{
						continue;
					}
					if (Secret == null)
					{
						Secret = JournalAPI.GetMapNote(findASiteDynamicQuestManager_QuestEntry.secretID);
						if (Secret != null && !Secret.Revealed)
						{
							continue;
						}
					}
					else if (Secret.ID != findASiteDynamicQuestManager_QuestEntry.secretID)
					{
						continue;
					}
					if (Zone?.ZoneID == findASiteDynamicQuestManager_QuestEntry.zoneID || The.ZoneManager.HasVisitedZone(findASiteDynamicQuestManager_QuestEntry.zoneID) || (Secret != null && Secret.Revealed))
					{
						if (!The.Game.HasFinishedQuestStep(findASiteDynamicQuestManager_QuestEntry.questID, findASiteDynamicQuestManager_QuestEntry.questStepID))
						{
							string text = Secret?.Text ?? Zone?.DisplayName ?? "a site";
							The.Game.FinishQuestStep(findASiteDynamicQuestManager_QuestEntry.questID, findASiteDynamicQuestManager_QuestEntry.questStepID);
							JournalAPI.AddAccomplishment("You located " + text + ".", "Through the use of " + IComponent<GameObject>.ThePlayer.GetPronounProvider().PossessiveAdjective + " divinely " + HistoricStringExpander.ExpandString("<spice.elements." + The.Player.GetMythicDomain() + ".adjectives.!random>") + " eyes, =name= discovered the lost location of " + text + ".", "Acting against the persecution of " + Factions.GetMostLikedFormattedName() + ", =name= led an army to the lost gates of " + text + ". " + The.Player.GetPronounProvider().CapitalizedSubjective + " liberated its citizens, who together in " + The.Player.GetPronounProvider().PossessiveAdjective + " honor <spice.history.gospels.Celebration.LateSultanate.!random>.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Medium, null, -1L);
						}
						if (quests.Contains(findASiteDynamicQuestManager_QuestEntry))
						{
							quests.Remove(findASiteDynamicQuestManager_QuestEntry);
						}
					}
				}
			}
			if (quests != null && quests.Count == 0)
			{
				The.Game.FlagSystemsForRemoval(typeof(FindASiteDynamicQuestManagerSystem));
			}
		}
	}

	[Serializable]
	public class FindASiteDynamicQuestManager_QuestEntry : IComposite
	{
		public string zoneID;

		public string secretID;

		public string questID;

		public string questStepID;

		public bool WantFieldReflection => false;

		public void Write(SerializationWriter Writer)
		{
			Writer.WriteOptimized(zoneID);
			Writer.WriteOptimized(secretID);
			Writer.WriteOptimized(questID);
			Writer.WriteOptimized(questStepID);
		}

		public void Read(SerializationReader Reader)
		{
			zoneID = Reader.ReadOptimizedString();
			secretID = Reader.ReadOptimizedString();
			questID = Reader.ReadOptimizedString();
			questStepID = Reader.ReadOptimizedString();
		}
	}

	public string _zoneID;

	public string _secretID;

	public string _questID;

	public string _questStepID;

	public FindASiteDynamicQuestManager()
	{
	}

	public FindASiteDynamicQuestManager(string zoneID, string secretID, string questID, string questStepID)
		: this()
	{
		_zoneID = zoneID;
		_secretID = secretID;
		_questID = questID;
		_questStepID = questStepID;
	}

	[WishCommand(null, null)]
	public static bool GotoDynamicQuestWhere()
	{
		FindASiteDynamicQuestManagerSystem system = The.Game.GetSystem<FindASiteDynamicQuestManagerSystem>();
		if (system != null)
		{
			using List<FindASiteDynamicQuestManager_QuestEntry>.Enumerator enumerator = system.quests.GetEnumerator();
			if (enumerator.MoveNext())
			{
				FindASiteDynamicQuestManager_QuestEntry current = enumerator.Current;
				The.ZoneManager.GetZone(current.zoneID).GetCell(0, 0).AddObject(The.Player);
				return true;
			}
		}
		return true;
	}

	[WishCommand(null, null)]
	public static bool DynamicQuestWhere()
	{
		FindASiteDynamicQuestManagerSystem system = The.Game.GetSystem<FindASiteDynamicQuestManagerSystem>();
		if (system != null)
		{
			foreach (FindASiteDynamicQuestManager_QuestEntry quest in system.quests)
			{
				IComponent<GameObject>.AddPlayerMessage("quest in " + quest.zoneID + " secret id is " + quest.secretID + " for quest " + quest.questID);
			}
		}
		return true;
	}

	public override void OnQuestAdded()
	{
		FindASiteDynamicQuestManagerSystem findASiteDynamicQuestManagerSystem = The.Game.RequireSystem(() => new FindASiteDynamicQuestManagerSystem());
		FindASiteDynamicQuestManager_QuestEntry findASiteDynamicQuestManager_QuestEntry = new FindASiteDynamicQuestManager_QuestEntry();
		findASiteDynamicQuestManager_QuestEntry.zoneID = _zoneID;
		findASiteDynamicQuestManager_QuestEntry.secretID = _secretID;
		findASiteDynamicQuestManager_QuestEntry.questID = _questID;
		findASiteDynamicQuestManager_QuestEntry.questStepID = _questStepID;
		if (findASiteDynamicQuestManagerSystem.quests == null)
		{
			findASiteDynamicQuestManagerSystem.quests = new List<FindASiteDynamicQuestManager_QuestEntry>();
		}
		findASiteDynamicQuestManagerSystem.quests.Add(findASiteDynamicQuestManager_QuestEntry);
	}

	public override void AfterQuestAdded()
	{
		The.Game.RequireSystem(() => new FindASiteDynamicQuestManagerSystem()).CheckCompleted();
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.Find((GameObject obj) => obj.GetStringProperty("GivesDynamicQuest") == _questID);
	}

	public override string GetQuestZoneID()
	{
		FindASiteDynamicQuestManagerSystem findASiteDynamicQuestManagerSystem = The.Game.RequireSystem(() => new FindASiteDynamicQuestManagerSystem());
		if (findASiteDynamicQuestManagerSystem.quests != null)
		{
			int num = findASiteDynamicQuestManagerSystem.quests.Count - 1;
			while (num >= 0 && num < findASiteDynamicQuestManagerSystem.quests.Count)
			{
				FindASiteDynamicQuestManager_QuestEntry findASiteDynamicQuestManager_QuestEntry = findASiteDynamicQuestManagerSystem.quests[num];
				if (!findASiteDynamicQuestManager_QuestEntry.zoneID.IsNullOrEmpty())
				{
					return findASiteDynamicQuestManager_QuestEntry.zoneID;
				}
				num--;
			}
		}
		return base.GetQuestZoneID();
	}
}
