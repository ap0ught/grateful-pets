using System;
using System.Collections.Generic;
using Qud.API;
using XRL;
using XRL.Core;
using XRL.Language;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace HistoryKit;

[Serializable]
public class HistoricEvent
{
	public long id;

	public long year;

	public long duration = 1L;

	[NonSerialized]
	public History history;

	[NonSerialized]
	public HistoricEntity entity;

	public Dictionary<string, string> eventProperties;

	public Dictionary<string, string> entityProperties;

	public Dictionary<string, List<string>> addedListProperties;

	public Dictionary<string, List<string>> removedListProperties;

	public Dictionary<string, HistoricPerspective> perspectives;

	public static HistoricEvent Load(SerializationReader reader)
	{
		HistoricEvent historicEvent = new HistoricEvent();
		historicEvent.id = reader.ReadInt64();
		historicEvent.year = reader.ReadInt64();
		historicEvent.duration = reader.ReadInt64();
		if (reader.ReadBoolean())
		{
			historicEvent.eventProperties = reader.ReadDictionary<string, string>();
		}
		if (reader.ReadBoolean())
		{
			historicEvent.entityProperties = reader.ReadDictionary<string, string>();
		}
		if (reader.ReadBoolean())
		{
			int num = reader.ReadInt32();
			historicEvent.addedListProperties = new Dictionary<string, List<string>>(num);
			for (int i = 0; i < num; i++)
			{
				string key = reader.ReadString();
				List<string> value = reader.ReadStringList();
				historicEvent.addedListProperties.Add(key, value);
			}
		}
		if (reader.ReadBoolean())
		{
			int num2 = reader.ReadInt32();
			historicEvent.removedListProperties = new Dictionary<string, List<string>>(num2);
			for (int j = 0; j < num2; j++)
			{
				string key2 = reader.ReadString();
				List<string> value2 = reader.ReadStringList();
				historicEvent.removedListProperties.Add(key2, value2);
			}
		}
		int num3 = reader.ReadInt32();
		if (num3 > 0)
		{
			historicEvent.perspectives = new Dictionary<string, HistoricPerspective>(num3);
			for (int k = 0; k < num3; k++)
			{
				HistoricPerspective historicPerspective = HistoricPerspective.Load(reader);
				if (historicPerspective.eventId != historicEvent.id)
				{
					throw new Exception("eventId mismatch: " + historicEvent.id + " vs. " + historicPerspective.eventId);
				}
				historicEvent.perspectives.Add(historicPerspective.entityId, historicPerspective);
			}
		}
		return historicEvent;
	}

	public virtual void Save(SerializationWriter writer)
	{
		writer.Write(id);
		writer.Write(year);
		writer.Write(duration);
		writer.Write(eventProperties != null);
		if (eventProperties != null)
		{
			writer.Write(eventProperties);
		}
		writer.Write(entityProperties != null);
		if (entityProperties != null)
		{
			writer.Write(entityProperties);
		}
		writer.Write(addedListProperties != null);
		if (addedListProperties != null)
		{
			writer.Write(addedListProperties.Count);
			foreach (KeyValuePair<string, List<string>> addedListProperty in addedListProperties)
			{
				writer.Write(addedListProperty.Key);
				writer.Write(addedListProperty.Value);
			}
		}
		writer.Write(removedListProperties != null);
		if (removedListProperties != null)
		{
			writer.Write(removedListProperties.Count);
			foreach (KeyValuePair<string, List<string>> removedListProperty in removedListProperties)
			{
				writer.Write(removedListProperty.Key);
				writer.Write(removedListProperty.Value);
			}
		}
		if (perspectives == null)
		{
			writer.Write(0);
			return;
		}
		writer.Write(perspectives.Count);
		foreach (string key in perspectives.Keys)
		{
			perspectives[key].Save(writer);
		}
	}

	public void AddListProperty(string Name, List<string> Value)
	{
		if (addedListProperties == null)
		{
			addedListProperties = new Dictionary<string, List<string>>();
		}
		if (addedListProperties.TryGetValue(Name, out var value))
		{
			value.AddRange(Value);
		}
		else
		{
			addedListProperties[Name] = Value;
		}
	}

	public void ChangeListProperty(string Name, List<string> Old, List<string> New)
	{
		if (addedListProperties == null)
		{
			addedListProperties = new Dictionary<string, List<string>>();
		}
		if (removedListProperties == null)
		{
			removedListProperties = new Dictionary<string, List<string>>();
		}
		addedListProperties[Name] = New;
		removedListProperties[Name] = Old;
	}

	[Obsolete("Use HasEventProperty")]
	public bool hasEventProperty(string key)
	{
		return HasEventProperty(key);
	}

	public bool HasEventProperty(string Key)
	{
		if (eventProperties != null)
		{
			return eventProperties.ContainsKey(Key);
		}
		return false;
	}

	[Obsolete("Use SetEventProperty")]
	public void setEventProperty(string key, string value)
	{
		SetEventProperty(key, value);
	}

	public void SetEventProperty(string Key, string Value)
	{
		if (eventProperties == null)
		{
			eventProperties = new Dictionary<string, string>();
		}
		Value = ExpandString(Value);
		eventProperties[Key] = Value;
	}

	[Obsolete("Use GetEventProperty")]
	public string getEventProperty(string name, string defaultValue = null)
	{
		return GetEventProperty(name, defaultValue);
	}

	public string GetEventProperty(string Key, string Default = null)
	{
		if (eventProperties != null && eventProperties.TryGetValue(Key, out var value))
		{
			return value;
		}
		return Default;
	}

	public string ExpandString(string s)
	{
		return HistoricStringExpander.ExpandString(s, entity.GetSnapshotAtYear(year + duration), history);
	}

	public string ExpandString(string s, Dictionary<string, string> vars)
	{
		return HistoricStringExpander.ExpandString(s, entity.GetSnapshotAtYear(year + duration), history, vars);
	}

	public int Random(int low, int high)
	{
		return history.Random(low, high);
	}

	public virtual void SetEntityHistory(HistoricEntity Entity, History History)
	{
		entity = Entity;
		history = History;
	}

	public virtual void Generate()
	{
	}

	public void AddEntityListItem(string List, string Value, bool Force = false)
	{
		if (addedListProperties == null)
		{
			addedListProperties = new Dictionary<string, List<string>>();
		}
		Value = ExpandString(Value);
		if (!addedListProperties.ContainsKey(List))
		{
			addedListProperties.Add(List, new List<string>());
		}
		if (Force || !addedListProperties[List].Contains(Value))
		{
			addedListProperties[List].Add(Value);
		}
	}

	public void RemoveEntityListItem(string List, string Value)
	{
		if (removedListProperties == null)
		{
			removedListProperties = new Dictionary<string, List<string>>();
		}
		Value = ExpandString(Value);
		if (!removedListProperties.ContainsKey(List))
		{
			removedListProperties.Add(List, new List<string>());
		}
		removedListProperties[List].Add(Value);
	}

	public List<string> GetListProperties(string Key)
	{
		if (addedListProperties != null && addedListProperties.TryGetValue(Key, out var value))
		{
			return value;
		}
		return null;
	}

	public bool HasEntityProperty(string Key)
	{
		if (entityProperties != null)
		{
			return entityProperties.ContainsKey(Key);
		}
		return false;
	}

	public void SetEntityProperty(string Key, string Value)
	{
		if (entityProperties == null)
		{
			entityProperties = new Dictionary<string, string>();
		}
		Value = ExpandString(Value);
		entityProperties[Key] = Value;
	}

	public string GetEntityProperty(string Key, string Default = null)
	{
		if (entityProperties != null && entityProperties.TryGetValue(Key, out var value))
		{
			return value;
		}
		return Default;
	}

	public virtual void ApplyToSnapshot(HistoricEntitySnapshot Snapshot)
	{
		if (entityProperties != null)
		{
			foreach (string key in entityProperties.Keys)
			{
				Snapshot.properties[key] = entityProperties[key];
			}
		}
		if (removedListProperties != null)
		{
			foreach (string key2 in removedListProperties.Keys)
			{
				foreach (string item in removedListProperties[key2])
				{
					if (Snapshot.listProperties.ContainsKey(key2) && Snapshot.listProperties[key2].Contains(item))
					{
						Snapshot.listProperties[key2].Remove(item);
					}
				}
			}
		}
		if (addedListProperties == null)
		{
			return;
		}
		foreach (string key3 in addedListProperties.Keys)
		{
			foreach (string item2 in addedListProperties[key3])
			{
				if (!Snapshot.listProperties.ContainsKey(key3))
				{
					Snapshot.listProperties.Add(key3, new List<string>());
				}
				Snapshot.listProperties[key3].Add(item2);
			}
		}
	}

	public virtual HistoricPerspective GetPerspective(HistoricEntity Entity)
	{
		if (perspectives == null)
		{
			return null;
		}
		if (!perspectives.ContainsKey(Entity.id))
		{
			return null;
		}
		return perspectives[Entity.id];
	}

	public virtual HistoricPerspective GetPerspective(HistoricEntitySnapshot Snapshot)
	{
		return GetPerspective(Snapshot.entity);
	}

	public virtual HistoricPerspective RequirePerspective(HistoricEntitySnapshot Snapshot, object UseFeeling = null)
	{
		if (perspectives == null)
		{
			perspectives = new Dictionary<string, HistoricPerspective>(1);
		}
		if (perspectives.ContainsKey(Snapshot.entity.id))
		{
			return perspectives[Snapshot.entity.id];
		}
		HistoricPerspective historicPerspective = new HistoricPerspective();
		historicPerspective.eventId = id;
		historicPerspective.entityId = Snapshot.entity.id;
		Snapshot.supplyPerspectiveColors(historicPerspective);
		if (UseFeeling == null)
		{
			historicPerspective.randomizeFeeling();
		}
		else
		{
			historicPerspective.feeling = (int)UseFeeling;
		}
		perspectives[Snapshot.entity.id] = historicPerspective;
		return historicPerspective;
	}

	public bool HasUnrevealedSecret()
	{
		if (HasUnrevealedRegion())
		{
			return true;
		}
		if (HasUnrevealedRelicQuest())
		{
			return true;
		}
		if (JournalAPI.HasUnrevealedSultanEvent(id))
		{
			return true;
		}
		return false;
	}

	public string GetUnrevealedSecretSource()
	{
		if (HasUnrevealedRegion())
		{
			return "region:" + GetEventProperty("revealsRegion");
		}
		if (HasUnrevealedRelicQuest())
		{
			return "relicquest:" + GetEventProperty("revealsItem");
		}
		if (JournalAPI.HasUnrevealedSultanEvent(id))
		{
			return "sultanevent:" + id;
		}
		return null;
	}

	public JournalSultanNote Reveal(string LearnedFrom = null)
	{
		PerformRegionReveal();
		PerformRelicQuestReveal();
		return JournalAPI.RevealSultanEvent(id, LearnedFrom);
	}

	public bool HasUnrevealedRegion()
	{
		if (HasEventProperty("revealsRegion"))
		{
			string eventProperty = GetEventProperty("revealsRegion");
			if (eventProperty != null && eventProperty != "unknown" && !The.Game.HasIntGameState("sultanRegionReveal_" + eventProperty))
			{
				return true;
			}
		}
		return false;
	}

	public void PerformRegionReveal()
	{
		if (!HasEventProperty("revealsRegion"))
		{
			return;
		}
		string eventProperty = GetEventProperty("revealsRegion");
		if (eventProperty != null && eventProperty != "unknown" && !The.Game.HasIntGameState("sultanRegionReveal_" + eventProperty))
		{
			The.Game.SetIntGameState("sultanRegionReveal_" + eventProperty, 1);
			Vector2i vector2i = The.Game.GetObjectGameState("sultanRegionPosition_" + eventProperty) as Vector2i;
			if (vector2i != null)
			{
				Zone zone = XRLCore.Core.Game.ZoneManager.GetZone("JoppaWorld");
				Popup.Show("You discover the location of " + eventProperty + ".", null, "sfx_newLocation_discovered_important");
				zone.GetCell(vector2i.x, vector2i.y).FireEvent("SultanReveal");
				int zoneTier = The.ZoneManager.GetZoneTier("JoppaWorld." + vector2i.x + "." + vector2i.y + ".1.1.10");
				AddQuestsForRegion(eventProperty, zoneTier);
				JournalAPI.AddAccomplishment("You discovered the location of " + eventProperty + ".", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered the location of " + eventProperty + ", once thought lost to the sands of time.", "In =year=, =name= appointed a corrupt administrator as minister of " + eventProperty + ". " + Grammar.InitCap(Grammar.RandomShePronoun()) + " mandated the practice of <spice.elements." + The.Player.GetMythicDomain() + ".practices.!random> in " + The.Player.GetPronounProvider().PossessiveAdjective + " name.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Medium, null, -1L);
			}
		}
	}

	public static void AddQuestsForRegion(string regionName, int zoneTier, string QuestGiverName = null, string QuestGiverLocationName = null, string QuestGiverLocationZoneID = null)
	{
		if (!The.Game.IsCoda)
		{
			QuestStep questStep = new QuestStep();
			questStep.ID = Guid.NewGuid().ToString();
			questStep.Name = "Travel to the historical site of " + regionName;
			questStep.Text = "";
			questStep.XP = 250 * zoneTier;
			Quest quest = new Quest();
			quest.ID = "Visit " + regionName;
			quest.Name = "Visit " + regionName;
			quest.StepsByID = new Dictionary<string, QuestStep>();
			quest.StepsByID.Add(questStep.ID, questStep);
			quest.Level = 1;
			quest._Manager = new VisitSultanDungeonQuestManager();
			(quest._Manager as VisitSultanDungeonQuestManager).Region = regionName;
			The.Game.StartQuest(quest, QuestGiverName, QuestGiverLocationName, QuestGiverLocationZoneID);
		}
	}

	public bool HasUnrevealedRelicQuest()
	{
		if (HasEventProperty("revealsItem"))
		{
			string eventProperty = GetEventProperty("revealsItem");
			if (!The.Game.HasIntGameState("sultanItemReveal_" + eventProperty))
			{
				return true;
			}
		}
		return false;
	}

	public void PerformRelicQuestReveal()
	{
		if (!The.Game.IsCoda && HasEventProperty("revealsItem"))
		{
			string eventProperty = GetEventProperty("revealsItem");
			if (!The.Game.HasIntGameState("sultanItemReveal_" + eventProperty))
			{
				The.Game.SetIntGameState("sultanItemReveal_" + eventProperty, 1);
				string eventProperty2 = GetEventProperty("revealsItemLocation");
				string eventProperty3 = GetEventProperty("revealsItemRegion");
				AddQuestsForRelic(eventProperty, eventProperty2, eventProperty3);
			}
		}
	}

	public void AddQuestsForRelic(string relicName, string locationName, string regionName, string QuestGiverName = null, string QuestGiverLocationName = null, string QuestGiverLocationZoneID = null)
	{
		if (!The.Game.IsCoda)
		{
			string text = relicName.Strip();
			QuestStep questStep = new QuestStep();
			questStep.ID = Guid.NewGuid().ToString();
			questStep.Name = "Locate " + regionName;
			questStep.Text = "Travel to the historical site of " + regionName + ".";
			questStep.XP = 1000;
			QuestStep questStep2 = new QuestStep();
			questStep2.ID = Guid.NewGuid().ToString();
			questStep2.Name = "Recover " + text;
			questStep2.Text = "Recover " + relicName + " at " + locationName + ".";
			questStep2.XP = 1000;
			Quest quest = new Quest();
			quest.ID = "Recover " + text;
			quest.Name = "Recover " + text;
			quest.StepsByID = new Dictionary<string, QuestStep>();
			quest.StepsByID.Add(questStep.ID, questStep);
			quest.StepsByID.Add(questStep2.ID, questStep2);
			quest.Level = 1;
			LocateRelicQuestManager locateRelicQuestManager = new LocateRelicQuestManager();
			locateRelicQuestManager.QuestID = quest.ID;
			locateRelicQuestManager.Relic = relicName;
			quest._Manager = locateRelicQuestManager;
			The.Game.StartQuest(quest, QuestGiverName, QuestGiverLocationName, QuestGiverLocationZoneID);
		}
	}
}
