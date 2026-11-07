using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephWeirdSky : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "At twilight in the shadow of the Spindle, the people of Omonporch saw an image on the horizon that looked like a ghost bathed in cerulean. It was Resheph, and after he came and left Omonporch, the people built a monument to him, and thenceforth called him Ghost-in-Cerulean.";
		SetEventProperty("gospel", value);
		AddEntityListItem("cognomen", "Ghost-in-Cerulean");
	}
}
