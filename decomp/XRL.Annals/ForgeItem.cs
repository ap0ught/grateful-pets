using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class ForgeItem : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property = snapshotAtYear.GetProperty("region");
		string randomElement = snapshotAtYear.GetList("elements").GetRandomElement();
		string text = ExpandString("<spice.elements." + randomElement + ".adjectives.!random>");
		string text2 = ExpandString("<spice.elements." + randomElement + ".nouns.!random>");
		int num = Random(0, 0);
		if (snapshotAtYear.GetProperty("profession").Equals("unknown"))
		{
			num = 1;
		}
		string text3;
		string text5;
		string text6;
		if (num == 0)
		{
			string property2 = snapshotAtYear.GetProperty("profession");
			int num2 = Random(0, 1);
			text3 = ExpandString("<spice.professions." + property2 + ".items.!random>");
			string text4 = ((num2 != 0) ? ("At a remote <spice.professions." + property2 + ".guildhall>") : ("While visiting an obscure <spice.professions." + property2 + ".guildhall>"));
			if (Random(0, 1) == 0)
			{
				text4 = text4 + " in " + property;
				SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property));
			}
			text5 = text4 + ", <entity.name> <spice.professions." + property2 + ".forged> " + Grammar.A(text3) + " that evoked the presence of " + Grammar.A(text) + " " + text2 + ".";
			text6 = string.Format("{0} by a {1} {2} {3} {4}, {5} visited {6} {7} and {8} {9}.", "<spice.commonPhrases.inspired.!random.capitalize>", "<spice.commonPhrases.prized.!random>", "", text2, ExpandString("<spice.history.gospels.ObjectFoundBy." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), "<entity.name>", "<entity.possessivePronoun>", "<spice.professions." + property2 + ".guildhall>", "<spice.professions." + property2 + ".forged>", Grammar.A(text3));
		}
		else
		{
			string property2 = ExpandString("<spice.professions.!random>");
			int num3 = Random(0, 1);
			text3 = ExpandString("<spice.professions." + property2 + ".items.!random>");
			string text7 = ((num3 != 0) ? ("At a remote <spice.professions." + property2 + ".guildhall>") : ("While visiting an obscure <spice.professions." + property2 + ".guildhall>"));
			if (Random(0, 1) == 0)
			{
				text7 = text7 + " in " + property;
				SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property));
			}
			text5 = text7 + ", <entity.name> met with a group of <spice.professions." + property2 + ".plural> and commissioned " + Grammar.A(text3) + " that evoked the presence of " + Grammar.A(text) + " " + text2 + ".";
			text6 = string.Format("{0} by a {1} {2} {3}, {4} visited a {5} {6} and commissioned a group of {7} to {8} {9}.", "<spice.commonPhrases.inspired.!random.capitalize>", "<spice.commonPhrases.prized.!random>", text2, ExpandString("<spice.history.gospels.ObjectFoundBy." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), "<entity.name>", "<spice.commonPhrases.eminent.!random>", "<spice.professions." + property2 + ".guildhall>", "<spice.professions." + property2 + ".plural>", "<spice.professions." + property2 + ".forge>", Grammar.A(text3));
		}
		string Name = Grammar.MakeTitleCase(QudHistoryHelpers.NameItemNounRoot(text2, history, entity));
		text5 = text5 + " <entity.subjectPronoun.capitalize> named it " + Name + ".";
		text6 = text6 + " <entity.subjectPronoun.capitalize> named it " + Name + ".";
		SetEventProperty("gospel", text5);
		SetEventProperty("tombInscription", text6);
		SetEventProperty("tombInscriptionCategory", "CreatesSomething");
		HistoricEntity historicEntity = history.CreateEntity(history.currentYear);
		Dictionary<string, string> dictionary = new Dictionary<string, string> { { "itemType", text3 } };
		QudHistoryHelpers.ExtractArticle(ref Name, out var Article);
		dictionary.Add("baseName", Name);
		dictionary.Add("article", Article);
		Name = QudHistoryHelpers.Ansify(Name, Article);
		dictionary.Add("name", Name);
		dictionary.Add("descriptionAdj", text);
		dictionary.Add("descriptionNoun", text2);
		dictionary.Add("period", snapshotAtYear.GetProperty("period"));
		historicEntity.ApplyEvent(new SetEntityProperties(dictionary, new Dictionary<string, string> { { "elements", randomElement } }));
		AddEntityListItem("items", Name);
	}
}
