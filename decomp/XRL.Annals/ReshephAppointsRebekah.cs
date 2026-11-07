using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephAppointsRebekah : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string newRegion = QudHistoryHelpers.GetNewRegion(history, snapshotAtYear.GetProperty("region"));
		string value = "In %" + year + "%, Resheph appointed Rebekah administrator of " + newRegion + ". She tendered alms for the sick in his name.";
		SetEventProperty("gospel", value);
		SetEventProperty("rebekah", "true");
		SetEventProperty("rebekahWasHealer", "true");
		SetEventProperty("region", newRegion);
	}
}
