using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephAbsolvesRebekah : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "In %" + year + "%, Rebekah died of glotrot, and on her deathbed Resheph forgave her and absolved her of sin. He allowed her to be buried in the village of Ezra, outside the gate to the Tomb of the Eaters and beneath the shadow of the Spindle.";
		SetEventProperty("gospel", value);
		SetEventProperty("rebekah", "true");
	}
}
