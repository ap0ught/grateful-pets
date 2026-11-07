using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephClosesTomb : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "In %" + year + "%, Resheph closed the gates to the Tomb of the Eaters, abdicated the throne, and dissolved the sultanate in order to lift the curse of the Gyre.";
		SetEventProperty("gospel", value);
		SetEntityProperty("flipYear", year.ToString());
	}
}
