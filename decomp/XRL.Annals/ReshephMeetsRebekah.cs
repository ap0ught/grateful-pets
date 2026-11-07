using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephMeetsRebekah : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "While traveling near Bethesda Susa, Resheph lost control of his chariot and drove it off a cliff. Luckily, a local physician named Rebekah came to his rescue. Moved by her kindness, Resheph enrolled at a nearby hospice as her apprentice.";
		SetEventProperty("gospel", value);
		SetEventProperty("rebekah", "true");
		SetEventProperty("rebekahWasHealer", "true");
	}
}
