using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephRebuffsCurse : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "In %" + year + "%, the Gyre widened, and the final plague afflicted the land. The Girsh Nephilim rose from their cradles on the Moon Stair and slouched toward Qud to eat its young. Resheph rose to meet them in battle and cast them back.";
		SetEventProperty("gospel", value);
		SetEventProperty("gyreplagues", "true");
	}
}
