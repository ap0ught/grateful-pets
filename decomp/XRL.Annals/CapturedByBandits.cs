using System;
using HistoryKit;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class CapturedByBandits : HistoricEvent
{
	public override void Generate()
	{
		duration = Random(2, 8);
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property = snapshotAtYear.GetProperty("location");
		string property2 = snapshotAtYear.GetProperty("region");
		int num = Random(0, 2);
		int num2 = Random(0, 1);
		string value = null;
		string text;
		switch (num)
		{
		case 0:
			text = "While traveling near " + property + " in " + property2 + ", ";
			SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property2));
			break;
		case 1:
			text = "On an expedition around " + property2 + ", ";
			SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property2));
			break;
		default:
			text = "Near the location of " + property + ", ";
			break;
		}
		switch (num2)
		{
		case 0:
		{
			string text3 = snapshotAtYear.GetList("elements")[Random(0, snapshotAtYear.GetList("elements").Count - 1)];
			string text4 = Grammar.InitCap(Grammar.MakeCompoundWord(Grammar.GetRandomMeaningfulWord(ExpandString("<spice.elements." + text3 + ".nouns.!random.capitalize>")), ExpandString("<spice.actornouns.!random>")));
			AddEntityListItem("cognomen", text4);
			string text5 = text + ExpandString("<entity.name> was captured by bandits. <entity.subjectPronoun.capitalize> murdered their leader <spice.elements.entity$elements[random].murdermethods.!random>, and was thenceforth known as " + text4 + ".");
			SetEventProperty("gospel", text5);
			if (text5.Contains("entropy"))
			{
				SetEntityProperty("highlyEntropicBeingWorshipAttitude", "-50");
			}
			value = string.Format("{0} the {1} of {2} {3}! " + text5, ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), ExpandString("<spice.commonPhrases.fate.!random>"), Grammar.MakePossessive(snapshotAtYear.GetProperty("name")), ExpandString("<spice.commonPhrases.foes.!random>"));
			SetEventProperty("tombInscriptionCategory", "Slays");
			break;
		}
		case 1:
		{
			string newLocationInRegion = QudHistoryHelpers.GetNewLocationInRegion(history, property2, property);
			string text2 = ExpandString("<spice.history.gospels.HumblePractice." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>");
			SetEntityProperty("location", newLocationInRegion);
			SetEventProperty("gospel", text + "<entity.name> was captured by bandits. <entity.subjectPronoun.capitalize> languished in captivity for " + Grammar.Cardinal((int)duration) + " years, eventually escaping to " + newLocationInRegion + ".");
			value = string.Format("For the sake of {12}, {0} {1} at {2} in {3} to {4} among the {5} and {6}. After {7} {8} years, {9} {10} at {11}.", "<entity.name>", ExpandString("<spice.commonPhrases.settled.!random>"), property, property2, ExpandString("<spice.instancesOf.dwellOrWork.!random>"), ExpandString("<spice.instancesOf.commonFolk.!random>"), text2, Grammar.Cardinal((int)duration), ExpandString("<spice.commonPhrases.humble.!random>"), "<entity.subjectPronoun>", ExpandString("<spice.instancesOf.reemerged.!random>"), newLocationInRegion, ExpandString("<spice.commonPhrases.wisdom.!random>"));
			SetEventProperty("tombInscriptionCategory", "EnduresHardship");
			break;
		}
		}
		SetEventProperty("tombInscription", value);
	}
}
