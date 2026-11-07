using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Annals;
using XRL.Core;
using XRL.Rules;
using XRL.World;

namespace HistoryKit;

[Serializable]
public class HistoricEntity : IComposite
{
	public delegate bool eventMatchingDelegate(HistoricEvent ev);

	public const long INVALID_DATE = long.MinValue;

	public const long LAST_ACTIVE_YEAR = long.MinValue;

	public string id;

	[NonSerialized]
	public History _history;

	public List<HistoricEvent> events = new List<HistoricEvent>();

	[NonSerialized]
	private static List<string> ListProperties = new List<string>();

	public string Name => GetEntityProperty("name", -1L);

	public History history
	{
		get
		{
			if (_history == null)
			{
				return XRLCore.Core.Game.sultanHistory;
			}
			return _history;
		}
		set
		{
			_history = value;
		}
	}

	public long firstYear
	{
		get
		{
			if (events == null || events.Count == 0)
			{
				return long.MinValue;
			}
			return events[0].year;
		}
	}

	public long lastYear
	{
		get
		{
			if (events == null || events.Count == 0)
			{
				return long.MinValue;
			}
			return events[events.Count - 1].year + events[events.Count - 1].duration;
		}
	}

	private HistoricEntity()
	{
	}

	public HistoricEntity(History history)
	{
		this.history = history;
	}

	public static HistoricEntity Load(SerializationReader reader, History history)
	{
		HistoricEntity historicEntity = new HistoricEntity(history);
		historicEntity.id = reader.ReadString();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			HistoricEvent historicEvent = HistoricEvent.Load(reader);
			historicEvent.entity = historicEntity;
			historicEvent.history = history;
			historicEntity.events.Add(historicEvent);
		}
		return historicEntity;
	}

	public void Save(SerializationWriter writer)
	{
		writer.Write(id);
		writer.Write(events.Count);
		foreach (HistoricEvent @event in events)
		{
			@event.Save(writer);
		}
	}

	public HistoricEvent GetRandomEventWhereDelegate(eventMatchingDelegate matcher, Random R = null)
	{
		if (R == null)
		{
			R = Stat.Rand;
		}
		List<HistoricEvent> list = new List<HistoricEvent>();
		for (int i = 0; i < events.Count; i++)
		{
			if (matcher(events[i]))
			{
				list.Add(events[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[R.Next(0, list.Count)];
	}

	public bool HasEntityProperty(string Key, long Year = -1L)
	{
		foreach (HistoricEvent @event in events)
		{
			if ((Year == -1 || @event.year <= Year) && @event.entityProperties != null && @event.entityProperties.ContainsKey(Key))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEventProperty(string Key, long Year = -1L)
	{
		foreach (HistoricEvent @event in events)
		{
			if ((Year == -1 || @event.year <= Year) && @event.eventProperties != null && @event.eventProperties.ContainsKey(Key))
			{
				return true;
			}
		}
		return false;
	}

	public string GetEntityProperty(string Key, long Year = -1L)
	{
		for (int num = events.Count - 1; num >= 0; num--)
		{
			HistoricEvent historicEvent = events[num];
			if ((Year == -1 || historicEvent.year <= Year) && historicEvent.entityProperties != null && historicEvent.entityProperties.TryGetValue(Key, out var value))
			{
				return value;
			}
		}
		return null;
	}

	public string GetEventProperty(string Key, long Year = -1L)
	{
		for (int num = events.Count - 1; num >= 0; num--)
		{
			HistoricEvent historicEvent = events[num];
			if ((Year == -1 || historicEvent.year <= Year) && historicEvent.eventProperties != null && historicEvent.eventProperties.TryGetValue(Key, out var value))
			{
				return value;
			}
		}
		return null;
	}

	public HistoricEvent GetEvent(Predicate<HistoricEvent> Predicate, long Year = -1L)
	{
		for (int num = events.Count - 1; num >= 0; num--)
		{
			HistoricEvent historicEvent = events[num];
			if ((Year == -1 || historicEvent.year <= Year) && Predicate(historicEvent))
			{
				return historicEvent;
			}
		}
		return null;
	}

	public HistoricEvent GetEventWithEventProperty(string Key, long Year = -1L)
	{
		for (int num = events.Count - 1; num >= 0; num--)
		{
			HistoricEvent historicEvent = events[num];
			if ((Year == -1 || historicEvent.year <= Year) && historicEvent.eventProperties != null && historicEvent.eventProperties.ContainsKey(Key))
			{
				return historicEvent;
			}
		}
		return null;
	}

	public HistoricEvent GetEventWithEntityProperty(string Key, long Year = -1L)
	{
		for (int num = events.Count - 1; num >= 0; num--)
		{
			HistoricEvent historicEvent = events[num];
			if ((Year == -1 || historicEvent.year <= Year) && historicEvent.entityProperties != null && historicEvent.entityProperties.ContainsKey(Key))
			{
				return historicEvent;
			}
		}
		return null;
	}

	public bool HasListProperties(string Key, long Year = -1L)
	{
		for (int num = events.Count - 1; num >= 0; num--)
		{
			HistoricEvent historicEvent = events[num];
			if ((Year == -1 || historicEvent.year <= Year) && historicEvent.addedListProperties != null && historicEvent.addedListProperties.ContainsKey(Key))
			{
				return true;
			}
		}
		return false;
	}

	public List<string> GetListProperties(string Key, long Year = -1L, bool ReadOnly = true)
	{
		List<string> list = (ReadOnly ? ListProperties : new List<string>());
		list.Clear();
		foreach (HistoricEvent @event in events)
		{
			if (Year != -1 && @event.year > Year)
			{
				continue;
			}
			if (@event.addedListProperties != null && @event.addedListProperties.TryGetValue(Key, out var value))
			{
				foreach (string item in value)
				{
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			else
			{
				if (@event.removedListProperties == null || !@event.removedListProperties.TryGetValue(Key, out var value2))
				{
					continue;
				}
				foreach (string item2 in value2)
				{
					list.Remove(item2);
				}
			}
		}
		return list;
	}

	public void SetEntityPropertyAtCurrentYear(string name, string value)
	{
		ApplyEvent(new SetEntityProperty(name, value));
	}

	public void MutateListPropertyAtCurrentYear(string name, Func<string, string> mutation)
	{
		ApplyEvent(new MutateListProperty(name, mutation, GetCurrentSnapshot()));
	}

	public void ApplyEvent(HistoricEvent newEvent, long year = long.MinValue)
	{
		if (year == long.MinValue)
		{
			newEvent.year = lastYear;
		}
		else
		{
			newEvent.year = year;
		}
		newEvent.SetEntityHistory(this, history);
		history.SetupEvent(newEvent);
		newEvent.Generate();
		AddEvent(newEvent);
		history.AddEvent(newEvent);
	}

	public void AddEvent(HistoricEvent Event)
	{
		int num = events.Count;
		int num2 = num - 1;
		while (num2 >= 0 && Event.year < events[num2].year)
		{
			num = num2;
			num2--;
		}
		events.Insert(num, Event);
	}

	public HistoricEntitySnapshot GetCurrentSnapshot()
	{
		return GetSnapshotAtYear(history.currentYear);
	}

	public HistoricEntitySnapshot GetSnapshotAtYear(long year)
	{
		HistoricEntitySnapshot historicEntitySnapshot = new HistoricEntitySnapshot(this);
		if (events == null || events.Count == 0)
		{
			return historicEntitySnapshot;
		}
		foreach (HistoricEvent item in events.OrderBy((HistoricEvent ev) => ev.year))
		{
			if (item.year <= year)
			{
				item.ApplyToSnapshot(historicEntitySnapshot);
			}
		}
		return historicEntitySnapshot;
	}
}
