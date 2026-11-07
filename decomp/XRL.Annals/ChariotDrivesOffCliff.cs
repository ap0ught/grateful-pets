using System;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class ChariotDrivesOffCliff : HistoricEvent
{
	public string regionToReveal;

	public ChariotDrivesOffCliff()
	{
	}

	public ChariotDrivesOffCliff(string regionToReveal)
	{
		this.regionToReveal = regionToReveal;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
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
		string text2;
		string text3;
		string text4;
		if (50.in100())
		{
			string text = ((snapshotAtYear.GetList("hatedFactions").Count != 0) ? snapshotAtYear.GetList("hatedFactions").GetRandomElement() : QudHistoryHelpers.GetNewFaction(entity));
			if (snapshotAtYear.GetList("hatedFaction").Count == 0)
			{
				AddEntityListItem("hatedFaction", text);
			}
			text2 = Grammar.MakePossessive(snapshotAtYear.GetProperty("name")) + " chariot was driven off a cliff by <spice.commonPhrases.rogue.!random.article> <spice.commonPhrases.band.!random> of " + Faction.GetFormattedGroupName(text);
			text3 = ExpandString("<spice.commonPhrases.band.!random.article> of treacherous " + Faction.GetFormattedGroupName(text) + " <spice.history.gospels.VehicularSabotage." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random> in " + Grammar.MakePossessive(snapshotAtYear.GetProperty("name")) + " <spice.history.gospels." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".vehicle.!random>") + ", causing it to " + ExpandString("<spice.history.gospels.VehicularSabotageResult." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>");
			text4 = ExpandString("<spice.commonPhrases.band.!random.article> of treacherous " + Faction.GetFormattedGroupName(text) + " <spice.history.gospels.VehicularSabotage." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random> in " + ExpandString("<entity.possessivePronoun>") + " <spice.history.gospels." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".vehicle.!random>") + ", causing it to " + ExpandString("<spice.history.gospels.VehicularSabotageResult." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>");
		}
		else
		{
			text2 = "<entity.name> lost control of <entity.possessivePronoun> chariot and drove it off a cliff";
			text3 = "<entity.name> lost control of <entity.possessivePronoun> <spice.history.gospels." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".vehicle.!random> and <spice.history.gospels.CrashedVehicle." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>";
			text4 = "<entity.subjectPronoun> lost control of <entity.possessivePronoun> <spice.history.gospels." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".vehicle.!random> and <spice.history.gospels.CrashedVehicle." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>";
		}
		string value2;
		if (Random(0, 1) == 0)
		{
			if (Random(0, 1) == 0)
			{
				string newFaction = QudHistoryHelpers.GetNewFaction(entity);
				AddEntityListItem("likedFactions", newFaction);
				string value = string.Format("{0} {7} {1}, {6}. {4}, a group of nearby {5} came to {3} rescue. For the rest of {3} life, {2} was indebted to {5}.", Grammar.InitCap(ExpandString("<spice.commonPhrases.whileTraveling.!random>")), property, snapshotAtYear.GetProperty("name"), Grammar.PossessivePronoun(snapshotAtYear.GetProperty("subjectPronoun")), Grammar.InitCap(ExpandString("<spice.commonPhrases.luckily.!random>")), Faction.GetFormattedGroupName(newFaction), text2, "<spice.commonPhrases.in.!random>");
				SetEventProperty("gospel", value);
				value2 = string.Format("{0} the {1} {2} of {4}, who came to {3} rescue when {5}.", ExpandString("<spice.instancesOf.bless.!random.capitalize>"), ExpandString("<spice.commonPhrases.noble.!random>"), Faction.GetFormattedGroupName(newFaction), Grammar.MakePossessive(snapshotAtYear.GetProperty("name")), property, text4);
				SetEventProperty("tombInscriptionCategory", "DoesSomethingHumble");
			}
			else
			{
				string text5 = ExpandString("<spice.professions.!random>");
				SetEntityProperty("profession", text5);
				SetEntityProperty("professionRank", ExpandString("<spice.professions." + text5 + ".training>"));
				string value = string.Format("{0} {9} {1}, {6}. {4}, a group of nearby {5} came to {3} rescue. Moved by their kindness, {2} enrolled at a local {7} as a {8}.", Grammar.InitCap(ExpandString("<spice.commonPhrases.whileTraveling.!random>")), property, snapshotAtYear.GetProperty("name"), Grammar.PossessivePronoun(snapshotAtYear.GetProperty("subjectPronoun")), Grammar.InitCap(ExpandString("<spice.commonPhrases.luckily.!random>")), ExpandString("<spice.professions." + text5 + ".plural>"), text2, ExpandString("<spice.professions." + text5 + ".guildhall>"), ExpandString("<spice.professions." + text5 + ".training>"), "<spice.commonPhrases.in.!random>");
				SetEventProperty("gospel", value);
				value2 = string.Format("{0} the {1} {2} of {4}, who came to {3} rescue when {5}.", ExpandString("<spice.instancesOf.bless.!random.capitalize>"), ExpandString("<spice.commonPhrases.noble.!random>"), ExpandString("<spice.professions." + text5 + ".plural>"), Grammar.MakePossessive(snapshotAtYear.GetProperty("name")), property, text4);
				SetEventProperty("tombInscriptionCategory", "DoesSomethingHumble");
			}
		}
		else
		{
			int num = (int)(year - entity.firstYear);
			SetEntityProperty("isAlive", "false");
			string value = string.Format("{0} {2} {1}, {3}. <entity.subjectPronoun.capitalize> was killed at {4} years old.", Grammar.InitCap(ExpandString("<spice.commonPhrases.whileTraveling.!random>")), property, "<spice.commonPhrases.in.!random>", text2, num);
			SetEventProperty("gospel", value);
			value2 = string.Format("In {0}, {1}. {2} was presumed dead at {3} years old.", property, text3, "<entity.subjectPronoun.capitalize>", num);
			SetEventProperty("tombInscriptionCategory", "BodyExperienceBad");
		}
		SetEventProperty("tombInscription", value2);
	}
}
