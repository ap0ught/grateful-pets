using System;
using System.Collections.Generic;
using System.Text;
using XRL.Collections;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.World;

namespace HistoryKit;

[Serializable]
public class History : HistoricEntityList
{
	public List<HistoricEvent> events = new List<HistoricEvent>();

	public StringMap<HistoricEntity> EntityByID = new StringMap<HistoricEntity>();

	public long startingYear = long.MinValue;

	public long currentYear = long.MinValue;

	private Random _r = new Random();

	private long totalEvents;

	[NonSerialized]
	private long nextEventId = 1L;

	public Random r
	{
		get
		{
			if (XRLCore.Core.Game != null)
			{
				return Stat.Rnd4;
			}
			if (_r == null)
			{
				_r = new Random();
			}
			return _r;
		}
	}

	public History(long startingYear)
	{
		currentYear = startingYear;
		this.startingYear = startingYear;
	}

	public int Random(int low, int high)
	{
		return r.Next(low, high + 1);
	}

	public static History Load(SerializationReader reader)
	{
		History history = new History(0L);
		history.startingYear = reader.ReadInt64();
		history.currentYear = reader.ReadInt64();
		history.totalEvents = reader.ReadInt64();
		history.nextEventId = history.totalEvents + 1;
		int num = reader.ReadInt32();
		history.entities = new List<HistoricEntity>();
		history.events = new List<HistoricEvent>();
		for (int i = 0; i < num; i++)
		{
			HistoricEntity historicEntity = HistoricEntity.Load(reader, history);
			history.entities.Add(historicEntity);
			history.events.AddRange(historicEntity.events);
			history.EntityByID[historicEntity.id] = historicEntity;
		}
		return history;
	}

	public void Save(SerializationWriter writer)
	{
		writer.Write(startingYear);
		writer.Write(currentYear);
		writer.Write(totalEvents);
		writer.Write(entities.Count);
		for (int i = 0; i < entities.Count; i++)
		{
			entities[i].Save(writer);
		}
	}

	public HistoricEvent GetEvent(long id)
	{
		foreach (HistoricEvent @event in events)
		{
			if (@event.id == id)
			{
				return @event;
			}
		}
		return null;
	}

	public void SetupEvent(HistoricEvent newEvent)
	{
		if (newEvent.id != 0L)
		{
			throw new Exception("event for setup already has ID");
		}
		newEvent.id = nextEventId++;
	}

	public void AddEvent(HistoricEvent newEvent)
	{
		if (newEvent.id == 0L)
		{
			throw new Exception("event to add had no ID");
		}
		events.Add(newEvent);
		totalEvents++;
		if (newEvent.year + newEvent.duration > currentYear)
		{
			currentYear = newEvent.year + newEvent.duration;
		}
	}

	public HistoricEntity CreateEntity()
	{
		return CreateEntity(Guid.NewGuid().ToString(), currentYear);
	}

	public HistoricEntity CreateEntity(long Year)
	{
		return CreateEntity(Guid.NewGuid().ToString(), Year);
	}

	public HistoricEntity CreateEntity(string ID, long Year)
	{
		HistoricEntity historicEntity = new HistoricEntity(this);
		historicEntity.id = ID;
		entities.Add(historicEntity);
		EntityByID[ID] = historicEntity;
		historicEntity.ApplyEvent(new CreatedHistoricEvent(), Year);
		return historicEntity;
	}

	[Obsolete("Use CreateEntity")]
	public HistoricEntity GetNewEntity(long createdYear)
	{
		return CreateEntity(createdYear);
	}

	public override HistoricEntity GetEntity(string ID)
	{
		return EntityByID.GetValue(ID);
	}

	public override bool TryGetEntity(string ID, out HistoricEntity Entity)
	{
		return EntityByID.TryGetValue(ID, out Entity);
	}

	public List<HistoricEntitySnapshot> GetEntitySnapshotsAtYear(long year)
	{
		List<HistoricEntitySnapshot> list = new List<HistoricEntitySnapshot>();
		for (int i = 0; i < entities.Count; i++)
		{
			if (year >= entities[i].firstYear && year <= entities[i].lastYear)
			{
				list.Add(entities[i].GetSnapshotAtYear(year));
			}
		}
		return list;
	}

	public string Dump(bool bVerbose = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		for (int i = 0; i < entities.Count; i++)
		{
			if (bVerbose)
			{
				HistoricEntitySnapshot snapshotAtYear = entities[i].GetSnapshotAtYear(entities[i].lastYear);
				if (snapshotAtYear.GetProperty("type").Equals("region"))
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("*** Region added to history: " + snapshotAtYear.properties["name"] + " ***");
				}
				else if (snapshotAtYear.GetProperty("type").Equals("location"))
				{
					stringBuilder.AppendLine("------------New location: " + snapshotAtYear.properties["name"]);
				}
			}
			for (int j = 0; j < entities[i].events.Count; j++)
			{
				if (entities[i].events[j].HasEventProperty("gospel"))
				{
					stringBuilder.AppendLine("  @" + entities[i].events[j].year + "  " + Grammar.ConvertAtoAn(entities[i].events[j].GetEventProperty("gospel")));
				}
			}
			if (entities[i].GetSnapshotAtYear(entities[i].lastYear).GetProperty("type").Equals("sultan"))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}
}
