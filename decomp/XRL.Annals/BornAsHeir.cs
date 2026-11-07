using System;
using System.Text.RegularExpressions;
using HistoryKit;
using XRL.Language;
using XRL.Names;

namespace XRL.Annals;

[Serializable]
public class BornAsHeir : HistoricEvent
{
	public override void Generate()
	{
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property = snapshotAtYear.GetProperty("name");
		string randomElement = snapshotAtYear.GetList("elements").GetRandomElement();
		string property2 = snapshotAtYear.GetProperty("subjectPronoun");
		string property3 = snapshotAtYear.GetProperty("region");
		string property4 = snapshotAtYear.GetProperty("location");
		string newValue = NameMaker.MakeName(null, null, null, null, "Eater");
		string text = "<spice.commonPhrases.oneStarryNight.!random.capitalize>";
		string text2 = ExpandString("<spice.elements." + randomElement + ".nouns.!random>");
		string text3 = ExpandString("<spice.elements." + randomElement + ".mythicalEvent.!random>").Replace("*var*", text2);
		Match match = Regex.Match(text3, "a famous \\w*");
		string text4 = ((!match.Success) ? text3 : text3.Replace(match.Groups[0].Value, newValue));
		string text5 = ((Random(1, 4) >= 3) ? Grammar.MakeTitleCase(Grammar.GetRandomMeaningfulWord(text2) + "born") : Grammar.MakeTitleCaseWithArticle("the " + ExpandString("<spice.commonPhrases.eminent.!random> ") + text2));
		string value = string.Format("{0} in {1}, a healthy child was born to the sultan at {2}. At the moment of {3} birth, {4}, and in celebration the people {5}. The babe was named {6}, but the people called {7} {8}.", text, property3, property4, Grammar.PossessivePronoun(property2), text3, ExpandString("<spice.commonPhrases.celebrated.!random>"), property, Grammar.ObjectPronoun(property2), text5);
		SetEventProperty("gospel", value);
		AddEntityListItem("cognomen", text5);
		SetEntityProperty("isHeir", "true");
		string text6 = ((snapshotAtYear.GetProperty("subjectPronoun") == "he") ? "heir" : "heiress");
		string value2 = string.Format("{0}! {1} the life of {2} -- called {3}! -- scion of the Fossilized Saads and {4} who was dowered the {5}. Observe the twin miracles of the {6}: the {7} was born, and {8}.", ExpandString("<spice.commonPhrases.onlooker.!random.capitalize>"), ExpandString("<spice.instancesOf.observeInWonder.!random.capitalize>"), "<entity.name>", text5, "<entity.subjectPronoun>", QudHistoryHelpers.GetMaskName(snapshotAtYear), QudHistoryHelpers.GenerateSultanateYearName(), text6, text4);
		SetEventProperty("tombInscription", value2);
		SetEventProperty("tombInscriptionCategory", "IsBorn");
		duration = 0L;
	}
}
