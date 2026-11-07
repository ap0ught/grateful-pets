using System;
using HistoryKit;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class Abdicate : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		HistoricEntitySnapshot currentSnapshot = entity.GetCurrentSnapshot();
		string value;
		string value2;
		if (!currentSnapshot.GetProperty("isSultan").EqualsNoCase("true"))
		{
			SetEntityProperty("isSultan", "true");
			string text = ExpandString("<spice.instancesOf.sultanKnownFor.!random>");
			value = string.Format("{0} %{1}%, {2}, the sultan of Qud {3}. Because {4}, {5} was chosen as the successor.", "<spice.instancesOf.inYear.!random.capitalize>", year, "<spice.instancesOf.afterTumultuousYears.!random>", "<spice.instancesOf.abdicated.!random>", ExpandString("<spice.instancesOf.ascensionReasons_VAR.!random>").Replace("*var*", Grammar.MakePossessive(currentSnapshot.GetProperty("name"))).Replace("*var2*", text), "<entity.subjectPronoun>");
			value2 = string.Format("{0} the {1} {2}, when {3} was dowered with the {4} and named sultan of Qud! {5} ancestors the Fossilized Saads looked on in approval, since -- just like them -- {6} was known by {8} {7}.", "<spice.commonPhrases.celebrate.!random.capitalize>", "<spice.commonPhrases.sacred.!random>", QudHistoryHelpers.GenerateSultanateYearName(), "<entity.name>", QudHistoryHelpers.GetMaskName(currentSnapshot), "<entity.possessivePronoun.capitalize>", "<entity.subjectPronoun>", text, "<entity.possessivePronoun>");
			SetEventProperty("tombInscriptionCategory", "CrownedSultan");
		}
		else
		{
			string text2 = ExpandString("<spice.instancesOf.afterTumultuousYears.!random>");
			string text3 = ExpandString("<spice.instancesOf.countered.!random>");
			value = string.Format("{0} %{1}%, {2}, {3} counselors suggested {4} {5}. Instead, {4} {6}.", "<spice.instancesOf.inYear.!random.capitalize>", year, text2, Grammar.MakePossessive(ExpandString("<entity.name>")), "<entity.subjectPronoun>", "<spice.instancesOf.abdicate.!random>", text3);
			value2 = string.Format("{0} the lesson that {1} taught {2} advisors when they suggested {3} abdicate the throne {4}. Did {3} do any such thing? No! Instead, {3} {5}.", "<spice.commonPhrases.remember.!random.capitalize>", "<entity.name>", "<entity.possessivePronoun>", "<entity.subjectPronoun>", text2, text3);
			SetEventProperty("tombInscriptionCategory", "MeetsWithCounselors");
		}
		SetEventProperty("gospel", value);
		SetEventProperty("tombInscription", value2);
	}
}
