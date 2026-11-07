using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class ReshephFoundsHarborage : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		string value = "After conferring with mysterious strangers, Resheph convinced them to help him found a harborage for the purpose of receiving visitors from across the clustered cosmos. They named it Starfarer's Quay.";
		SetEventProperty("gospel", value);
	}
}
