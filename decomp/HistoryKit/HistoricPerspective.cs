using System;
using XRL.Rules;
using XRL.World;

namespace HistoryKit;

[Serializable]
public class HistoricPerspective
{
	public string entityId;

	public long eventId;

	public int feeling;

	public string mainColor;

	public string supportColor;

	public static HistoricPerspective Load(SerializationReader reader)
	{
		return new HistoricPerspective
		{
			entityId = reader.ReadString(),
			eventId = reader.ReadInt64(),
			feeling = reader.ReadInt32(),
			mainColor = reader.ReadString(),
			supportColor = reader.ReadString()
		};
	}

	public virtual void Save(SerializationWriter writer)
	{
		writer.Write(entityId);
		writer.Write(eventId);
		writer.Write(feeling);
		writer.Write(mainColor);
		writer.Write(supportColor);
	}

	public void randomizeFeeling()
	{
		feeling = Stat.Random(-1000, 1000);
	}
}
