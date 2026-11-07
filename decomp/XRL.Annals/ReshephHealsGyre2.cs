using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephHealsGyre2 : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "In %" + year + "%, the Gyre widened, and the second triad of plagues afflicted the land. The legs of kith and kin annealed to iron, darkness bloomed from the earth, and the svardym blackened the sky. Resheph walked below the chrome arches and healed the sick.";
		SetEventProperty("gospel", value);
		SetEventProperty("gyreplagues", "true");
	}
}
