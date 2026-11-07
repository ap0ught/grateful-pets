using System;
using HistoryKit;
using XRL.Core;

namespace XRL.Annals;

[Serializable]
public class LocationConstructed : HistoricEvent
{
	public override void Generate()
	{
		string[] array = new string[4] { "Tomb", "Palace", "Library", "Abbey" };
		SetEntityProperty("name", XRLCore.Core.GenerateRandomPlayerName() + "'s " + array[Random(0, 3)]);
		AddEntityListItem("spice", "glass");
		duration = Random(1, 120);
		SetEventProperty("gospel", "construction begain in " + year + " and ended in " + (year + duration));
	}
}
