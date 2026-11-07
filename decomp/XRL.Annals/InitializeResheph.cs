using System;
using HistoryKit;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class InitializeResheph : HistoricEvent
{
	public int period;

	public InitializeResheph(int _period)
	{
		period = _period;
	}

	public override void Generate()
	{
		SetEntityProperty("type", "sultan");
		SetEntityProperty("Resheph", "true");
		SetEntityProperty("birthYear", year.ToString());
		AddEntityListItem("cognomen", "the Above");
		SetEntityProperty("cultName", "Cult of the Coiled Lamb");
		string value = "Resheph";
		SetEntityProperty("nameRoot", value);
		SetEntityProperty("suffix", "0");
		SetEntityProperty("name", value);
		SetEntityProperty("period", period.ToString());
		SetEntityProperty("isAlive", "true");
		string text = "he";
		SetEntityProperty("subjectPronoun", text);
		SetEntityProperty("possessivePronoun", Grammar.PossessivePronoun(text));
		SetEntityProperty("objectPronoun", Grammar.ObjectPronoun(text));
		SetEntityProperty("region", "null");
		duration = 0L;
	}
}
