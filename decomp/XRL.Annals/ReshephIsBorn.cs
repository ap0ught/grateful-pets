using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephIsBorn : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "One starry evening, a babe was found swaddled with its mouth full of circuits by a group of baetyls in Red Rock. They took him into their fold and fostered him, and he became known as Resheph, the Coiled Lamb of Baetyls.";
		SetEventProperty("gospel", value);
		AddEntityListItem("cognomen", "the Coiled Lamb of Baetyls");
	}
}
