using System;
using HistoryKit;
using XRL.Names;

namespace XRL.Annals;

[Serializable]
public class InitializeLocation : HistoricEvent
{
	private string region;

	private int period;

	public InitializeLocation(string _region, int _period)
	{
		region = _region;
		period = _period;
	}

	public override void Generate()
	{
		SetEntityProperty("type", "location");
		SetEntityProperty("region", region);
		SetEntityProperty("period", period.ToString());
		string text;
		do
		{
			text = NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, null, null, "Site");
			if (50.in100())
			{
				switch (Random(0, 14))
				{
				case 0:
					text += " Spire";
					break;
				case 1:
					text = "the Hamlet of " + text;
					break;
				case 2:
					text = "the Shrine at " + text;
					break;
				case 3:
					text += " Steeple";
					break;
				case 4:
					text += " Mesh";
					break;
				case 5:
					text = "New " + text;
					break;
				case 6:
					text = "Old " + text;
					break;
				case 7:
					text += " Grotto";
					break;
				case 8:
					text += " Cave";
					break;
				case 9:
					text += " Den";
					break;
				case 10:
					text += " Hollow";
					break;
				case 11:
					text += " Dune";
					break;
				case 12:
					text += " Tangle";
					break;
				case 13:
					text += " Morass";
					break;
				case 14:
					text += " Knot";
					break;
				}
			}
		}
		while (history.GetEntitiesWherePropertyEquals("name", text).Count > 0);
		SetEntityProperty("name", text);
		duration = 0L;
	}
}
