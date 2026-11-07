using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephHealsGyre1 : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "In %" + year + "%, the Gyre widened, and the first triad of plagues afflicted the land. The water was poisoned with salt, girshlings ravaged the fruit and wheat, and tongues rotted away in the mouths of kith and kin. Resheph walked below the chrome arches and healed the sick.";
		SetEventProperty("gospel", value);
		SetEventProperty("gyreplagues", "true");
	}
}
