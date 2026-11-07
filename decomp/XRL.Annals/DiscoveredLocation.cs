using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class DiscoveredLocation : HistoricEvent
{
	public override void Generate()
	{
		duration = Random(5, 8);
		HistoricEntity historicEntity = history.CreateEntity(year - Random(0, 1000));
		historicEntity.ApplyEvent(new LocationConstructed());
		SetEventProperty("location", historicEntity.id);
		SetEventProperty("gospel", "discovered " + historicEntity.GetSnapshotAtYear(year).GetProperty("name"));
	}
}
