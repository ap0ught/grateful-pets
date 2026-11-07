using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephBecomesSultan : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "Acting against a prohibition on the practice of worshipping the stars, Resheph led an army to the gates of Omonporch. There he defeated his brothers in battle and liberated its citizens, and they crowned him the sultan of Qud.";
		SetEventProperty("gospel", value);
		SetEntityProperty("isSultan", "true");
	}
}
