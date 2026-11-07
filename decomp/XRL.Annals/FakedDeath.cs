using System;
using HistoryKit;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class FakedDeath : HistoricEvent
{
	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		SetEntityProperty("isAlive", "true");
		string text = ((snapshotAtYear.GetList("colors").Count != 0 && Random(0, 1) != 0) ? ("the " + snapshotAtYear.GetList("colors").GetRandomElement() + " <spice.commonPhrases.ghost.!random>") : ("the " + ExpandString("<spice.elements." + snapshotAtYear.GetList("elements").GetRandomElement() + ".adjectives.!random>") + " <spice.commonPhrases.ghost.!random>"));
		if (50.in100())
		{
			text = text + " of " + snapshotAtYear.GetProperty("region");
		}
		text = Grammar.MakeTitleCaseWithArticle(ExpandString(text));
		AddEntityListItem("cognomen", text);
		string text2 = ExpandString("<spice.instancesOf.backToLife.!random>").Replace("*var*", Grammar.MakePossessive(snapshotAtYear.GetProperty("name")));
		string text3 = ExpandString("<spice.commonPhrases.itWasDiscovered.!random>");
		string value = $"In %{year.ToString()}%, {text3} that {text2}. Despite reports to the contrary, <entity.name> was alive and well. <entity.subjectPronoun.capitalize> was known thenceforth as {text}.";
		SetEventProperty("gospel", value);
		if (text2.Contains("ntropic"))
		{
			SetEntityProperty("highlyEntropicBeingWorshipAttitude", "50");
		}
		string value2 = string.Format("{0}! {1} {2}, who in the {3} {4} death and {5} from the {6}.", ExpandString("<spice.commonPhrases.onlooker.!random.capitalize>"), ExpandString("<spice.instancesOf.observeInWonder.!random.capitalize>"), text, QudHistoryHelpers.GenerateSultanateYearName(), ExpandString("<spice.commonPhrases.defied.!random>"), ExpandString("<spice.commonPhrases.emerged.!random>"), ExpandString("<spice.history.gospels." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".adjective.!random> <spice.history.gospels." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".location.!random>"));
		SetEventProperty("tombInscription", value2);
		SetEventProperty("tombInscriptionCategory", "DoesSomethingRad");
	}
}
