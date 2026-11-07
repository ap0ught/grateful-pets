using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephHasStarExperience : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "While traveling alone through the Rust Wells, Resheph came across a trove of gleaming stars in a bottle. From that day forth, he always kept some stardust on his person.";
		SetEventProperty("gospel", value);
	}
}
