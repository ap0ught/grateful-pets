using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephLearnsCurse : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "At Starfarer's Quay, Resheph consulted with mysterious strangers and learned the secret of the Gyre. By cause of the misdeeds of the elder saads and sultans, star beings had levied a curse on Qud. Resheph vowed to make right from this wrong and redeem our doomed world.";
		SetEventProperty("gospel", value);
	}
}
