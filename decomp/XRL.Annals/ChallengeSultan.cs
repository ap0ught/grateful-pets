using System;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class ChallengeSultan : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property = snapshotAtYear.GetProperty("region");
		string text;
		switch (Random(0, 2))
		{
		case 0:
			text = "In %" + year + "%, ";
			break;
		case 1:
			text = "While leading a small army in " + property + ", ";
			SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property));
			break;
		case 2:
			text = "In early %" + year + "%, ";
			break;
		default:
			text = "In late %" + year + "%, ";
			break;
		}
		int num = 0;
		if (snapshotAtYear.GetProperty("isSultan").EqualsNoCase("true"))
		{
			num = 1;
		}
		if (num == 0)
		{
			SetEntityProperty("isSultan", "true");
			string text2 = Random(0, 2) switch
			{
				0 => " challenged the sultan of Qud to a duel", 
				1 => " challenged the legitimacy of the sultan of Qud", 
				_ => " assassinated the sultan of Qud", 
			};
			int num2 = Random(0, 2);
			if (snapshotAtYear.GetList("likedFactions").Count == 0 && snapshotAtYear.GetProperty("profession").EqualsNoCase("unknown"))
			{
				num2 = 1;
			}
			else if (snapshotAtYear.GetList("likedFactions").Count == 0)
			{
				num2 = Random(1, 2);
			}
			else if (snapshotAtYear.GetProperty("profession").Equals("unknown"))
			{
				num2 = Random(0, 1);
			}
			string text4;
			string text5;
			switch (num2)
			{
			case 0:
			{
				string randomElement = snapshotAtYear.GetList("likedFactions").GetRandomElement();
				text4 = " over the rights of " + Faction.GetFormattedName(randomElement);
				text5 = "venerating " + Faction.GetFormattedName(randomElement);
				break;
			}
			case 1:
				text5 = ExpandString("<spice.elements.entity$elements[random].practices.!random>");
				text4 = " over an ordinance prohibiting the practice of " + text5;
				break;
			default:
			{
				string text3 = ExpandString("<spice.professions.entity$profession.plural>");
				text4 = " over the sanctioned persecution of " + text3;
				text5 = "venerating " + text3;
				break;
			}
			}
			int num3 = (int)(year - entity.firstYear);
			string value = text + "<entity.name>" + text2 + text4 + ". <entity.subjectPronoun.capitalize> won and <spice.commonPhrases.ascended.!random>. <entity.subjectPronoun.capitalize> was " + Grammar.Cardinal(num3) + " years old.";
			SetEventProperty("gospel", value);
			string value2 = string.Format("In {0}, {1} averred {9} descendancy from the Fossilized Saads, who were known for {2}. By {3} right, {4} ousted the pretender sultan, who had prohibited {5} in favor of {6} worship, and crowned {7} sultan of Qud. {10} was {8} years old.", property, "<entity.name>", text5, "<spice.commonPhrases.sacred.!random>", "<entity.subjectPronoun>", text5, ExpandString("<spice.history.gospels." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".worshipObject.!random>"), "<entity.reflexivePronoun>", Grammar.Cardinal(num3), "<entity.possessivePronoun>", "<entity.subjectPronoun.capitalize>");
			SetEventProperty("tombInscription", value2);
			SetEventProperty("tombInscriptionCategory", "CrownedSultan");
			return;
		}
		string text6 = Random(0, 2) switch
		{
			0 => " was challenged by <spice.commonPhrases.pretender.!random.article> to a duel", 
			1 => " had <entity.possessivePronoun> legitimacy challenged by <spice.commonPhrases.pretender.!random.article>", 
			_ => " was assailed by <spice.commonPhrases.pretender.!random.article>", 
		};
		int num4 = Random(0, 2);
		if (snapshotAtYear.GetList("hatedFactions").Count == 0 && snapshotAtYear.GetList("hatedProfessions").Count == 0)
		{
			num4 = 1;
		}
		else if (snapshotAtYear.GetList("hatedFactions").Count == 0)
		{
			num4 = Random(1, 2);
		}
		else if (snapshotAtYear.GetList("hatedProfessions").Count == 0)
		{
			num4 = Random(0, 1);
		}
		string text8;
		string text9;
		switch (num4)
		{
		case 0:
		{
			string randomElement2 = snapshotAtYear.GetList("hatedFactions").GetRandomElement();
			text8 = " over the rights of " + Faction.GetFormattedName(randomElement2);
			text9 = "venerating " + Faction.GetFormattedName(randomElement2);
			break;
		}
		case 1:
			text9 = ExpandString("<spice.elements.entity$elements[random].practices.!random>");
			text8 = " over an ordinance mandating the practice of " + text9;
			break;
		default:
		{
			string text7 = ExpandString("<spice.professions.entity$profession.plural>");
			text8 = " over the sanctioned persecution of " + text7;
			text9 = "venerating " + text7;
			break;
		}
		}
		int num5 = (int)(history.currentYear - entity.firstYear);
		if (90.in100())
		{
			string value = text + "<entity.name>" + text6 + text8 + ". <entity.subjectPronoun.capitalize> won and had the <spice.commonPhrases.pretender.!random> <spice.commonPhrases.killed.!random>. <entity.subjectPronoun.capitalize> was " + Grammar.Cardinal(num5) + " years old.";
			SetEventProperty("gospel", value);
			string value2 = string.Format("In the {0}, {1} {2} claimed descendancy from the Fossilized Saads and challenged {3} over the tradition of {4}. {5}, {3} won and had the {6} {7}.", QudHistoryHelpers.GenerateSultanateYearName(), "an", "aspirant", "<entity.name>", text9, "<spice.instancesOf.ofCourse.!random.capitalize>", "<spice.commonPhrases.pretender.!random>", "<spice.commonPhrases.killed.!random>");
			SetEventProperty("tombInscription", value2);
			SetEventProperty("tombInscriptionCategory", "Slays");
		}
		else
		{
			string value = text + "<entity.name>" + text6 + text8 + ". <entity.subjectPronoun.capitalize> lost and was <spice.commonPhrases.killed.!random>. <entity.subjectPronoun.capitalize> was " + Grammar.Cardinal(num5) + " years old.";
			SetEventProperty("gospel", value);
			SetEntityProperty("isAlive", "false");
			string value2 = string.Format("In the {0}, an aspirant claimed descendancy from the Fossilized Saads and challenged {1} over the tradition of {2}. The aspirant won and {1} was {3}. {4} was {5} years old.", QudHistoryHelpers.GenerateSultanateYearName(), "<entity.name>", text9, "<spice.commonPhrases.killed.!random>", "<entity.subjectPronoun.capitalize>", Grammar.Cardinal(num5));
			SetEventProperty("tombInscription", value2);
			SetEventProperty("tombInscriptionCategory", "Dies");
		}
	}
}
