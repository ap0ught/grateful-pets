using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class BattleItem : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property = snapshotAtYear.GetProperty("location");
		string property2 = snapshotAtYear.GetProperty("region");
		string newLocationInRegion = QudHistoryHelpers.GetNewLocationInRegion(history, property2, property);
		SetEntityProperty("location", newLocationInRegion);
		string randomElement = snapshotAtYear.GetList("elements").GetRandomElement();
		string text = ExpandString("<spice.items.weapons.!random>");
		string text2 = ExpandString("<spice.elements." + randomElement + ".adjectives.!random>");
		int num = 0;
		int num2 = Random(0, 1);
		string text3;
		string text4;
		if (num2 == 0)
		{
			text3 = "liberate";
			if (snapshotAtYear.GetList("likedFactions").Count != 0)
			{
				text4 = snapshotAtYear.GetList("likedFactions").GetRandomElement();
			}
			else
			{
				text4 = QudHistoryHelpers.GetNewFaction(entity);
				AddEntityListItem("likedFactions", text4);
				num = 1;
			}
		}
		else
		{
			text3 = "subjugate";
			if (snapshotAtYear.GetList("hatedFactions").Count != 0)
			{
				text4 = snapshotAtYear.GetList("hatedFactions").GetRandomElement();
			}
			else
			{
				text4 = QudHistoryHelpers.GetNewFaction(entity);
				AddEntityListItem("hatedFactions", text4);
				num = 1;
			}
		}
		string text5 = (text3.Equals("liberate") ? ExpandString("<spice.commonPhrases.boon.!random>") : ExpandString("<spice.commonPhrases.bane.!random>"));
		string Name = ((Random(0, 1) != 0) ? Grammar.MakeTitleCase(QudHistoryHelpers.NameItemAdjRoot(text2, history, entity) + ", the " + text5 + " of " + Faction.GetFormattedName(text4)) : Grammar.MakeTitleCase(QudHistoryHelpers.NameItemAdjRoot(text2, history, entity) + " " + Grammar.GetRandomMeaningfulWord(Faction.GetFormattedName(text4)) + text5));
		if (num == 0)
		{
			string value = "At the Battle of " + newLocationInRegion + ", <entity.name> fought to " + text3 + " " + Faction.GetFormattedName(text4) + ". <entity.subjectPronoun.capitalize> wielded " + Grammar.A(text2) + " " + text + " with such <spice.commonPhrases.finesse.!random> that it became forever known as " + Name + ".";
			SetEventProperty("gospel", value);
		}
		else
		{
			string text6 = (snapshotAtYear.GetProperty("isSultan").Equals("true") ? "sultan " : "");
			string value = "At the Battle of " + newLocationInRegion + ", <entity.name> fought as a mercenary " + text6 + "to " + text3 + " " + Faction.GetFormattedName(text4) + ". <entity.subjectPronoun.capitalize> wielded " + Grammar.A(text2) + " " + text + " with such <spice.commonPhrases.finesse.!random> that it became forever known as " + Name + ".";
			SetEventProperty("gospel", value);
		}
		string value2 = ((num2 != 0) ? string.Format("{0} the Battle of {1}, where -- {2} -- {3} wielded {5} {6} and struck down the {4} in the name of {7}. {0}, afterward, how {8} and {9} {10} in {11} for days and days.", ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), newLocationInRegion, ExpandString("<spice.elements." + randomElement + ".mythicalBattleVista.!random>").Replace("*var*", text2), "<entity.name>", ExpandString("<spice.history.gospels.EnemyHostName." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), "<entity.possessivePronoun>", text, ExpandString("<spice.instancesOf.justice.!random>"), Faction.GetFormattedName(text4), ExpandString("<spice.instancesOf.dearOnes.!random>"), ExpandString("<spice.instancesOf.criedOut.!random>"), ExpandString("<spice.commonPhrases.woe.!random>")) : string.Format("{0} the Battle of {1}, where -- {2} -- {3} wielded {5} {6} and struck down the {4} in the name of {7}. {0}, afterward, how {8} and {9} {10} in {11} for days and days.", ExpandString("<spice.commonPhrases.remember.!random.capitalize>"), newLocationInRegion, ExpandString("<spice.elements." + randomElement + ".mythicalBattleVista.!random>").Replace("*var*", text2), "<entity.name>", ExpandString("<spice.history.gospels.EnemyHostName." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), "<entity.possessivePronoun>", text, ExpandString("<spice.instancesOf.justice.!random>"), Faction.GetFormattedName(text4), ExpandString("<spice.instancesOf.dearOnes.!random>"), ExpandString("<spice.instancesOf.criedOut.!random>"), ExpandString("<spice.commonPhrases.celebration.!random>")));
		SetEventProperty("tombInscription", value2);
		SetEventProperty("tombInscriptionCategory", "WieldsItemInBattle");
		HistoricEntity historicEntity = history.CreateEntity(year);
		Dictionary<string, string> dictionary = new Dictionary<string, string> { { "itemType", text } };
		QudHistoryHelpers.ExtractArticle(ref Name, out var Article);
		dictionary.Add("baseName", Name);
		dictionary.Add("article", Article);
		Name = QudHistoryHelpers.Ansify(Name, Article);
		dictionary.Add("name", Name);
		dictionary.Add("descriptionAdj", text2);
		dictionary.Add("period", snapshotAtYear.GetProperty("period"));
		Dictionary<string, string> dictionary2 = new Dictionary<string, string> { { "elements", randomElement } };
		if (text3.Equals("liberate"))
		{
			dictionary2.Add("likedFactions", text4);
		}
		else
		{
			dictionary2.Add("hatedFactions", text4);
		}
		historicEntity.ApplyEvent(new SetEntityProperties(dictionary, dictionary2));
		AddEntityListItem("items", Name);
		Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
		dictionary3.Add("battles", "generic");
		history.GetEntitiesWherePropertyEquals("name", newLocationInRegion).GetRandomElement().ApplyEvent(new SetEntityProperties(null, dictionary3));
	}
}
