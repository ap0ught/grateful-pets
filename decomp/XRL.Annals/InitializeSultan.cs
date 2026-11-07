using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.Names;

namespace XRL.Annals;

[Serializable]
public class InitializeSultan : HistoricEvent
{
	public int period;

	public InitializeSultan(int _period)
	{
		period = _period;
	}

	public override void Generate()
	{
		SetEntityProperty("type", "sultan");
		HistoricEntityList entitiesWherePropertyEquals = history.GetEntitiesWherePropertyEquals("type", "sultan");
		int num;
		string text;
		string value;
		do
		{
			num = 0;
			bool flag = (int.Parse(QudHistoryHelpers.GetRegionalizationParametersSnapshot(history).GetProperty("successorChance")).in100() ? true : false);
			if (entitiesWherePropertyEquals.Count == 0)
			{
				flag = false;
			}
			if (flag)
			{
				HistoricEntity randomElement = entitiesWherePropertyEquals.GetRandomElement();
				text = randomElement.GetCurrentSnapshot().GetProperty("nameRoot");
				int num2 = int.Parse(randomElement.GetCurrentSnapshot().GetProperty("suffix"));
				if (num2 == 0)
				{
					num = 2;
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					dictionary.Add("suffix", 1.ToString());
					dictionary.Add("name", text + " I");
					randomElement.ApplyEvent(new SetEntityProperties(dictionary, null));
				}
				else
				{
					num = num2 + 1;
				}
			}
			else
			{
				do
				{
					text = NameMaker.MakeName(null, null, null, null, "Eater");
				}
				while (history.GetEntitiesWherePropertyEquals("name", text).Count > 0);
			}
			value = ((num != 0) ? (text + " " + Grammar.GetRomanNumeral(num)) : text);
		}
		while (history.GetEntitiesWherePropertyEquals("name", value).Count != 0);
		SetEntityProperty("nameRoot", text);
		SetEntityProperty("suffix", num.ToString());
		SetEntityProperty("name", value);
		SetEntityProperty("period", period.ToString());
		SetEntityProperty("isAlive", "true");
		string value2 = ExpandString("<spice.elements.!random>");
		AddEntityListItem("elements", value2);
		HistoricEntitySnapshot snapshotAtYear = history.GetEntitiesByDelegate((HistoricEntity entity) => entity.GetSnapshotAtYear(entity.lastYear).GetProperty("type").Equals("region") && entity.GetSnapshotAtYear(entity.lastYear).GetProperty("period") == period.ToString()).GetRandomElement().GetSnapshotAtYear(history.currentYear);
		SetEntityProperty("region", snapshotAtYear.GetProperty("name"));
		SetEntityProperty("location", QudHistoryHelpers.GetRandomLocationInRegionByPeriod(history, snapshotAtYear.GetProperty("name"), period));
		string text2 = Grammar.RandomShePronoun();
		SetEntityProperty("subjectPronoun", text2);
		SetEntityProperty("possessivePronoun", Grammar.PossessivePronoun(text2));
		SetEntityProperty("objectPronoun", Grammar.ObjectPronoun(text2));
		SetEntityProperty("reflexivePronoun", Grammar.ReflexivePronoun(text2));
		duration = 0L;
	}
}
