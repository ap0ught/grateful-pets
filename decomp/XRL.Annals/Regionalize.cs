using System;
using HistoryKit;
using XRL.Names;

namespace XRL.Annals;

[Serializable]
public class Regionalize : HistoricEvent
{
	public override void Generate()
	{
		SetEntityProperty("name", "regionalizationParameters");
		SetEntityProperty("government", ExpandString("<spice.history.regions.government.types.!random>"));
		SetEntityProperty("topology", ExpandString("<spice.history.regions.topology.types.!random>"));
		SetEntityProperty("organizingPrinciple", ExpandString("<spice.history.regions.organizingPrinciple.types.!random>"));
		SetEntityProperty("successorChance", Random(0, 100).ToString());
		SetEntityProperty("siteName1", NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, null, null, "Site"));
		SetEntityProperty("siteName2", NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, null, null, "Site"));
		SetEntityProperty("siteName3", NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, null, null, "Site"));
		SetEntityProperty("siteTopologyTheChance", ((Random(0, 1) == 0) ? 20 : 80).ToString());
		string text = ExpandString("<spice.siteModifiers1.!random>");
		SetEntityProperty("siteTopology1", text);
		string text2;
		do
		{
			text2 = ExpandString("<spice.siteModifiers2.!random>");
		}
		while (text2 == text);
		SetEntityProperty("siteTopology2", text2);
		duration = 0L;
	}
}
