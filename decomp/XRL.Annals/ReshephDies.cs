using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephDies : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "In %" + year + "%, Resheph, the Above, Ghost-in-Cerulean, the Coiled Lamb of Baetyls, died of natural causes. He was 216 years old.";
		SetEventProperty("gospel", value);
	}
}
