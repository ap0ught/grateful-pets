using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Language;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class InteractWithAnObjectDynamicQuestManager : QuestManager
{
	[Serializable]
	public class System : IPlayerSystem
	{
		public List<QuestEntry> quests;

		public void QuestableInteract(GameObject Object)
		{
			if (Object == null || quests == null)
			{
				return;
			}
			for (int num = quests.Count - 1; num >= 0; num--)
			{
				QuestEntry questEntry = quests[num];
				if (Object.IDMatch(questEntry.itemID))
				{
					FinishEntry(questEntry, Object);
				}
			}
		}

		public void FinishEntry(QuestEntry Entry, GameObject Object)
		{
			The.Game.FinishQuestStep(Entry.questID, Entry.questStepID);
			GameObject gameObject = The.ZoneManager.peekCachedObject(Entry.itemID);
			if (gameObject == null)
			{
				Debug.LogError("no cached object for item ID " + Entry.itemID);
				return;
			}
			string[] array = Entry.verb.Split(' ');
			string text = ((array.Length > 1) ? (Grammar.PastTenseOf(array[0]) + " " + string.Join(" ", array.Skip(1).ToArray())) : Grammar.PastTenseOf(Entry.verb));
			string text2 = HistoricStringExpander.ExpandString("<spice.professions.!random>");
			JournalAPI.AddAccomplishment("You " + text + " " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: false) + ".", "While exploring " + Object.CurrentZone.DisplayName + ", =name= " + text + " the fabled contraption called " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: true) + ".", "While visiting an obscure <spice.professions." + text2 + ".guildhall>, =name= met with a group of <spice.professions." + text2 + ".plural> and commissed what came to be known as " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: true) + ".", null, "general", MuralCategory.FindsObject, MuralWeight.Medium, null, -1L);
			quests.Remove(Entry);
		}
	}

	[Serializable]
	public class QuestEntry : IComposite
	{
		public string itemID;

		public string verb;

		public string questID;

		public string questStepID;

		public bool WantFieldReflection => false;

		public void Write(SerializationWriter Writer)
		{
			Writer.WriteOptimized(itemID);
			Writer.WriteOptimized(verb);
			Writer.WriteOptimized(questID);
			Writer.WriteOptimized(questStepID);
		}

		public void Read(SerializationReader Reader)
		{
			itemID = Reader.ReadOptimizedString();
			verb = Reader.ReadOptimizedString();
			questID = Reader.ReadOptimizedString();
			questStepID = Reader.ReadOptimizedString();
		}
	}

	public string _itemID;

	public string _questID;

	public string _questStepID;

	public List<QuestEntry> quests;

	public InteractWithAnObjectDynamicQuestManager()
	{
	}

	public InteractWithAnObjectDynamicQuestManager(string itemID, string questID, string questStepID)
	{
		_itemID = itemID;
		_questID = questID;
		_questStepID = questStepID;
	}

	public override void OnQuestAdded()
	{
		System system = The.Game.RequireSystem<System>();
		QuestEntry questEntry = new QuestEntry();
		GameObject gameObject = The.ZoneManager.peekCachedObject(_itemID);
		questEntry.itemID = _itemID;
		questEntry.verb = gameObject.GetStringProperty("QuestVerb");
		questEntry.questID = _questID;
		questEntry.questStepID = _questStepID;
		if (system.quests == null)
		{
			system.quests = new List<QuestEntry>();
		}
		system.quests.Add(questEntry);
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.Find((GameObject obj) => obj.GetStringProperty("GivesDynamicQuest") == _questID);
	}
}
