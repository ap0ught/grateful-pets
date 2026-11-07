using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephCleansesGyre : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "In %" + year + "%, Resheph cleansed the marshlands of the plagues of the Gyre and taught Abram to sow watervine along its fertile tracks.";
		SetEventProperty("gospel", value);
		SetEventProperty("JoppaShrine", "true");
	}
}
