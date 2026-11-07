using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class UnderWeirdSky : HistoricEvent
{
	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string randomElement = snapshotAtYear.GetList("elements").GetRandomElement();
		string text = ExpandString("<spice.elements." + randomElement + ".nouns.!random>");
		string property = snapshotAtYear.GetProperty("region");
		string property2 = snapshotAtYear.GetProperty("location");
		property2 = QudHistoryHelpers.GetNewLocationInRegion(history, property, property2);
		SetEntityProperty("location", property2);
		string text2 = ExpandString("<spice.colors.!random>");
		AddEntityListItem("colors", text2);
		string text3 = ((Random(0, 1) != 0) ? string.Join("-", Grammar.MakeTitleCaseWithArticle(Grammar.GetRandomMeaningfulWord(text) + " in " + text2).Split(' ')) : Grammar.MakeTitleCaseWithArticle("the " + text2 + " " + text));
		AddEntityListItem("cognomen", text3);
		string value = "At <spice.time.partsOfDay.!random> under <spice.commonPhrases.strange.!random.article> and " + text2 + " sky, the people of " + property2 + " saw an image on the horizon that looked like " + Grammar.A(text) + " bathed in " + text2 + ". It was <entity.name>, and after <entity.subjectPronoun> came and left " + property2 + ", the people built a monument to <entity.objectPronoun>, and thenceforth called <entity.objectPronoun> " + text3 + ".";
		SetEventProperty("gospel", value);
		SetEventProperty("tombInscription", value);
		SetEventProperty("tombInscriptionCategory", "DoesSomethingRad");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("monuments", snapshotAtYear.GetProperty("name"));
		history.GetEntitiesWherePropertyEquals("name", property2).GetRandomElement().ApplyEvent(new SetEntityProperties(null, dictionary));
	}
}
