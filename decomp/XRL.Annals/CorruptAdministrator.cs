using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class CorruptAdministrator : HistoricEvent
{
	public string regionToReveal;

	public CorruptAdministrator()
	{
	}

	public CorruptAdministrator(string _regionToReveal)
	{
		regionToReveal = _regionToReveal;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string value = null;
		string text = QudHistoryHelpers.GenerateSultanateYearName();
		string property;
		if (string.IsNullOrEmpty(regionToReveal))
		{
			property = snapshotAtYear.GetProperty("region");
		}
		else
		{
			property = regionToReveal;
			SetEntityProperty("region", regionToReveal);
		}
		SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property));
		int num = Random(0, 1);
		List<string> list = snapshotAtYear.GetList("likedFactions");
		if (list.Count == 0)
		{
			num = 0;
		}
		switch (num)
		{
		case 0:
		{
			string text2 = ExpandString("<spice.elements.entity$elements[random].practices.!random>");
			if (!snapshotAtYear.GetProperty("isSultan").Equals("true"))
			{
				string newRegion2 = QudHistoryHelpers.GetNewRegion(history, property);
				SetEntityProperty("region", newRegion2);
				SetEntityProperty("location", QudHistoryHelpers.GetRandomLocationInRegionByPeriod(history, newRegion2, int.Parse(snapshotAtYear.GetProperty("period"))));
				SetEventProperty("gospel", "In %" + year + "%, a corrupt administrator was appointed minister of " + property + ". " + Grammar.InitCap(Grammar.RandomShePronoun()) + " outlawed the practice of " + text2 + ", and <entity.name> was forced to flee.");
				value = string.Format("{0} the {1}, when a corrupt administrator was appointed minister of {2}. {3} outlawed the practice of {4}, and {5} was forced to flee.", ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), text, property, Grammar.InitCap(Grammar.RandomShePronoun()), text2, "<entity.name>");
				SetEventProperty("tombInscriptionCategory", "EnduresHardship");
			}
			else
			{
				SetEventProperty("gospel", "In %" + year + "%, <entity.name> appointed a corrupt administrator as minister of " + property + ". " + Grammar.InitCap(Grammar.RandomShePronoun()) + " mandated the practice of " + text2 + " in <entity.name>'s name.");
				value = string.Format("{0} the {1}, when the {2} {10} of {3} was {4} in {5}. {0} that {6}, to {7} {8}, appointed a minister who mandated the practice of {9}.", ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), text, "moral", ExpandString("<spice.history.gospels.ImmoralPractice." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), ExpandString("<spice.commonPhrases.rife.!random>"), property, "<entity.name>", ExpandString("<spice.commonPhrases.restore.!random>"), ExpandString("<spice.commonPhrases.morality.!random>"), text2, ExpandString("<spice.commonPhrases.depravity.!random>"));
				SetEventProperty("tombInscriptionCategory", "DoesBureaucracy");
			}
			break;
		}
		case 1:
		{
			string formattedName = Faction.GetFormattedName(list[Random(0, list.Count - 1)]);
			if (!snapshotAtYear.GetProperty("isSultan").Equals("true"))
			{
				string newRegion = QudHistoryHelpers.GetNewRegion(history, property);
				SetEntityProperty("region", newRegion);
				SetEntityProperty("location", QudHistoryHelpers.GetRandomLocationInRegionByPeriod(history, newRegion, int.Parse(snapshotAtYear.GetProperty("period"))));
				SetEventProperty("gospel", "In %" + year + "%, a corrupt administrator was appointed minister of " + property + ". " + Grammar.InitCap(Grammar.RandomShePronoun()) + " outlawed association with " + formattedName + ", and <entity.name> was forced to flee.");
				value = string.Format("{0} the {1}, when a corrupt administrator was appointed minister of {2}. {3} outlawed association with {4}, and {5} was forced to flee.", ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), text, property, Grammar.InitCap(Grammar.RandomShePronoun()), formattedName, "<entity.name>");
				SetEventProperty("tombInscriptionCategory", "EnduresHardship");
			}
			else
			{
				SetEventProperty("gospel", "In %" + year + "%, <entity.name> appointed a corrupt administrator as minister of " + property + ". " + Grammar.InitCap(Grammar.RandomShePronoun()) + " mandated association with " + formattedName + " in <entity.name>'s name.");
				value = string.Format("{0} the {1}, when the {2} {10} of {3} was {4} in {5}. {0} that {6}, to {7} {8}, appointed a minister who mandated association with {9}.", ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), text, "moral", ExpandString("<spice.history.gospels.ImmoralPractice." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), ExpandString("<spice.commonPhrases.rife.!random>"), property, "<entity.name>", ExpandString("<spice.commonPhrases.restore.!random>"), ExpandString("<spice.commonPhrases.morality.!random>"), formattedName, ExpandString("<spice.commonPhrases.depravity.!random>"));
				SetEventProperty("tombInscriptionCategory", "DoesBureaucracy");
			}
			break;
		}
		}
		SetEventProperty("tombInscription", value);
	}
}
