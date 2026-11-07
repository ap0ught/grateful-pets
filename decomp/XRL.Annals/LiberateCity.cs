using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class LiberateCity : HistoricEvent
{
	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property = snapshotAtYear.GetProperty("region");
		string property2 = snapshotAtYear.GetProperty("location");
		property2 = QudHistoryHelpers.GetNewLocationInRegion(history, property, property2);
		string text = QudHistoryHelpers.GenerateLocationName(snapshotAtYear.GetProperty("name"), history);
		QudHistoryHelpers.RenameLocation(property2, text, history);
		SetEntityProperty("location", text);
		string text2 = "friend";
		string text3 = "enemy";
		string text4;
		string text5;
		while (true)
		{
			switch (Random(0, 4))
			{
			case 0:
			{
				string randomElement2 = snapshotAtYear.GetList("colors").GetRandomElement();
				if (snapshotAtYear.GetList("colors").Count == 0)
				{
					continue;
				}
				text4 = "Acting against the prohibition on the color " + randomElement2 + ", ";
				text5 = text2 + " in " + randomElement2;
				break;
			}
			case 1:
			{
				string formattedName = Faction.GetFormattedName(snapshotAtYear.GetList("likedFactions").GetRandomElement());
				if (snapshotAtYear.GetList("likedFactions").Count == 0)
				{
					continue;
				}
				text4 = "Acting against the persecution of " + formattedName + ", ";
				text5 = text2 + " to " + formattedName;
				break;
			}
			case 2:
			{
				string formattedName2 = Faction.GetFormattedName(snapshotAtYear.GetList("hatedFactions").GetRandomElement());
				if (snapshotAtYear.GetList("hatedFactions").Count == 0)
				{
					continue;
				}
				text4 = "Acting against the enfranchisement of " + formattedName2 + ", ";
				text5 = text3 + " to " + formattedName2;
				break;
			}
			case 3:
			{
				if (snapshotAtYear.GetProperty("profession").Equals("unknown"))
				{
					continue;
				}
				string text6 = ExpandString("<spice.professions." + snapshotAtYear.GetProperty("profession") + ".plural>");
				text4 = "Acting against labor laws restricting the rights of " + text6 + ", ";
				text5 = text2 + " to " + text6;
				break;
			}
			default:
			{
				string randomElement = snapshotAtYear.GetList("elements").GetRandomElement();
				text4 = "Acting against the prohibition on the practice of <spice.elements." + randomElement + ".practices.!random>, ";
				text5 = "child of <spice.elements." + randomElement + ".materials.!random>";
				break;
			}
			}
			break;
		}
		string value2;
		if (Random(0, 1) == 0)
		{
			if (snapshotAtYear.GetProperty("isSultan").Equals("true"))
			{
				string value = text4 + "<entity.name> led an army to the gates of " + property2 + ". <entity.subjectPronoun.capitalize> <spice.commonPhrases.liberated.!random> its citizens, and in <entity.possessivePronoun> honor they thenceforth called it " + text + ".";
				SetEventProperty("gospel", value);
				SetEventProperty("tombInscriptionCategory", "Resists");
				value2 = string.Format("{0} {1}, {2} and {3} of {4}, who led an army to its gates and {5} its {6} {7}. The people of the city {8} in {9}, {10} {11} name, and renamed {4} to {12} in {13} honor.", "<spice.commonPhrases.love.!random.capitalize>", "<entity.name>", text5, "<spice.commonPhrases.savior.!random>", property2, "<spice.instancesOf.unseated.!random>", "<spice.commonPhrases.corrupt.!random>", "<spice.commonPhrases.despots.!random>", ExpandString("<spice.history.gospels.Celebration." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), "<spice.commonPhrases.celebration.!random>", "<spice.instancesOf.chantedOrSang.!random>", Grammar.MakePossessive(snapshotAtYear.GetProperty("name")), text, "<entity.possessivePronoun>");
			}
			else
			{
				string value = text4 + "<entity.name> led an army to the gates of " + property2 + ". <entity.subjectPronoun.capitalize> <spice.commonPhrases.liberated.!random> its citizens, and they <spice.commonPhrases.crowned.!random> <entity.objectPronoun> sultan of Qud, dowering <entity.objectPronoun> with the " + QudHistoryHelpers.GetMaskName(snapshotAtYear) + ". In <entity.possessivePronoun> honor they changed the name of " + property2 + " to " + text + ".";
				SetEventProperty("gospel", value);
				SetEntityProperty("isSultan", "true");
				value2 = string.Format("{0} {1}, {2} and {3} of {4}, who led an army to its gates and {5} its {6} {7}. The people of the city {8} in {9}, dowered {1} with the {11}, and {14} {13} sultan of Qud. In {13} honor they renamed {4} to {12}.", "<spice.commonPhrases.love.!random.capitalize>", "<entity.name>", text5, "<spice.commonPhrases.savior.!random>", property2, "<spice.instancesOf.unseated.!random>", "<spice.commonPhrases.corrupt.!random>", "<spice.commonPhrases.despots.!random>", ExpandString("<spice.history.gospels.Celebration." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), "<spice.commonPhrases.celebration.!random>", "<entity.objectPronoun>", QudHistoryHelpers.GetMaskName(snapshotAtYear), text, "<entity.possessivePronoun>", "<spice.commonPhrases.crowned.!random>");
				SetEventProperty("tombInscriptionCategory", "CrownedSultan");
			}
		}
		else
		{
			string value = text4 + "<entity.name> led an army to the gates of " + property2 + ". <entity.subjectPronoun.capitalize> sacked " + property2 + " and <spice.commonPhrases.slaughtered.!random> its citizens, forcing them to change its name to " + text + ".";
			SetEventProperty("gospel", value);
			value2 = string.Format("{0} {1}, {2} and {3} of {4}, who led an army to its gates and {5} its {6} {7}. The people of the city {8} in {9}, {10} {11} name, and renamed {4} to {12} in {13} honor.", "<spice.commonPhrases.love.!random.capitalize>", "<entity.name>", text5, "<spice.commonPhrases.savior.!random>", property2, "<spice.instancesOf.unseated.!random>", "<spice.commonPhrases.corrupt.!random>", "<spice.commonPhrases.despots.!random>", ExpandString("<spice.history.gospels.Celebration." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), "<spice.commonPhrases.celebration.!random>", "<spice.instancesOf.chantedOrSang.!random>", Grammar.MakePossessive(snapshotAtYear.GetProperty("name")), text, "<entity.possessivePronoun>");
			SetEventProperty("tombInscriptionCategory", "Resists");
		}
		SetEventProperty("tombInscription", value2);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("monuments", snapshotAtYear.GetProperty("name"));
		history.GetEntitiesWherePropertyEquals("name", text).GetRandomElement().ApplyEvent(new SetEntityProperties(null, dictionary));
	}
}
