using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephBetrayed : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "In %" + year + "%, Rebekah betrayed Resheph by stealing the Mark of Death from the gate to the Tomb of the Eaters. In punishment, Resheph excommunicated her from the sultanate.";
		SetEventProperty("gospel", value);
	}
}
