using System;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class FoundAsBabe : HistoricEvent
{
	public override void Generate()
	{
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property = snapshotAtYear.GetProperty("name");
		string randomElement = snapshotAtYear.GetList("elements").GetRandomElement();
		string property2 = snapshotAtYear.GetProperty("subjectPronoun");
		string property3 = snapshotAtYear.GetProperty("region");
		int num = Random(0, 2);
		string text = "<spice.commonPhrases.oneStarryNight.!random.capitalize>";
		string value;
		switch (num)
		{
		case 0:
			value = text + ", a babe was found swaddled <spice.myth.mythicPrepPlaces.!random> <spice.elements." + randomElement + ".babeTrait.!random>. That babe came to be known as " + property + ".";
			SetEventProperty("gospel", value);
			break;
		case 1:
		{
			string text3 = ExpandString("<spice.elements." + randomElement + ".professions.!random>");
			SetEntityProperty("profession", text3);
			SetEntityProperty("professionRank", ExpandString("<spice.professions." + text3 + ".training>"));
			string text4 = Grammar.RandomShePronoun();
			value = text + ", a <spice.professions." + text3 + ".singular> found a babe <spice.elements." + randomElement + ".babeTrait.!random> outside " + Grammar.PossessivePronoun(text4) + " <spice.professions." + text3 + ".guildhall>. " + Grammar.InitCap(text4) + " and " + Grammar.PossessivePronoun(text4) + " fellow <spice.professions." + text3 + ".plural> adopted the babe and named " + Grammar.ObjectPronoun(property2) + " " + property + ".";
			SetEventProperty("gospel", value);
			break;
		}
		default:
		{
			string newFaction = QudHistoryHelpers.GetNewFaction(entity);
			AddEntityListItem("likedFactions", newFaction);
			string text2 = "";
			if (Random(0, 1) == 0)
			{
				text2 += "the ";
			}
			text2 += ExpandString("<spice.elements." + randomElement + ".adjectives.!random.capitalize> <spice.commonPhrases.scion.!random.capitalize> of " + Grammar.MakeTitleCaseWithArticle(Faction.GetFormattedName(newFaction)));
			AddEntityListItem("cognomen", text2);
			value = text + ", a babe was found swaddled <spice.elements." + randomElement + ".babeTrait.!random> by a group of " + Faction.GetFormattedName(newFaction) + " in " + property3 + ". They took " + Grammar.ObjectPronoun(property2) + " into their fold and fostered " + Grammar.ObjectPronoun(property2) + ", and " + property2 + " became known as " + property + ", " + text2 + ".";
			SetEventProperty("gospel", value);
			SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property3));
			break;
		}
		}
		SetEventProperty("tombInscription", value);
		SetEventProperty("tombInscriptionCategory", "IsBorn");
		duration = 0L;
	}
}
