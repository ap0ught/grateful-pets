using System;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class InspiringExperience : HistoricEvent
{
	public override void Generate()
	{
		duration = 1L;
		string value = null;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string text = ExpandString("<spice.elements.!random>");
		while (snapshotAtYear.GetList("elements").Contains(text))
		{
			text = ExpandString("<spice.elements.!random>");
		}
		AddEntityListItem("elements", text);
		string property = snapshotAtYear.GetProperty("region");
		int num = Random(0, 2);
		int num2 = Random(0, 1);
		string text2;
		switch (num)
		{
		case 0:
			text2 = "Somewhere in " + property + ", ";
			SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property));
			break;
		case 1:
			text2 = "While traveling alone in " + property + ", ";
			SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property));
			break;
		default:
			text2 = "One auspicious day, ";
			break;
		}
		switch (num2)
		{
		case 0:
			value = ExpandString(text2 + "<entity.name> <spice.elements." + text + ".inspirationsVerbPhrase.!random>. <spice.commonPhrases.fromThenOn.!random.capitalize>, <entity.subjectPronoun> was obsessed with <spice.elements." + text + ".nounsPlural.!random>.");
			SetEventProperty("gospel", value);
			break;
		case 1:
			value = ExpandString(text2 + "<entity.name> <spice.elements." + text + ".inspirationsVerbPhrase.!random>. <spice.commonPhrases.fromThenOn.!random.capitalize>, <entity.subjectPronoun> always kept some <spice.elements." + text + ".nounsPlural.!random> hidden on <entity.possessivePronoun> person.");
			SetEventProperty("gospel", value);
			break;
		}
		SetEventProperty("tombInscription", value);
		SetEventProperty("tombInscriptionCategory", "HasInspiringExperience");
	}
}
