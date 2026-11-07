using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class BloodyBattle : HistoricEvent
{
	public string regionToReveal;

	public BloodyBattle()
	{
	}

	public BloodyBattle(string _regionToReveal)
	{
		regionToReveal = _regionToReveal;
	}

	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string randomElement = snapshotAtYear.GetList("elements").GetRandomElement();
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
		string property2 = snapshotAtYear.GetProperty("location");
		property2 = QudHistoryHelpers.GetNewLocationInRegion(history, property, property2);
		string text = ((Random(0, 1) != 0) ? Grammar.MakeTitleCaseWithArticle(ExpandString("<spice.elements." + randomElement + ".adjectives.!random>") + ExpandString("<spice.commonPhrases.ruins.!random>")) : Grammar.MakeTitleCaseWithArticle("the " + ExpandString("<spice.elements." + randomElement + ".adjectives.!random> ") + ExpandString("<spice.commonPhrases.ruins.!random>")));
		SetEntityProperty("location", text);
		bool flag = false;
		if (snapshotAtYear.GetList("items").Count != 0)
		{
			flag = true;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (flag)
		{
			string randomElement2 = snapshotAtYear.GetList("items").GetRandomElement();
			RemoveEntityListItem("items", randomElement2);
			dictionary.Add("items", randomElement2);
			string value = "In %" + year + "%, <entity.name> won a decisive victory against the combined forces of " + property + " at the bloody Battle of " + property2 + ", though <entity.subjectPronoun> lost <entity.possessivePronoun> prized " + randomElement2 + " during the course of the conflict. As a result of the battle, " + property2 + " was so <spice.elements." + randomElement + ".ruinReason> that it was renamed " + text + ".";
			SetEventProperty("gospel", value);
			string value2 = history?.GetEntitiesWherePropertyEquals("name", property)?.GetRandomElement()?.GetCurrentSnapshot()?.GetProperty("newName");
			if (string.IsNullOrEmpty(value2))
			{
				value2 = property;
			}
			SetEventProperty("revealsItem", randomElement2);
			SetEventProperty("revealsItemLocation", text);
			SetEventProperty("revealsItemRegion", value2);
		}
		else
		{
			string value = "In %" + year + "%, <entity.name> won a decisive victory against the combined forces of " + property + " at the bloody Battle of " + property2 + ". As a result of the battle, " + property2 + " was so <spice.elements." + randomElement + ".ruinReason> that it was renamed " + text + ".";
			SetEventProperty("gospel", value);
		}
		string value3 = string.Format("In the {0}, {1} {2} the {3}, who {4}. Let the {5} of {6} serve as testament to the {7} of {1} and a {8} to those who would {9} {10}.", QudHistoryHelpers.GenerateSultanateYearName(), "<entity.name>", ExpandString("<spice.commonPhrases.vanquished.!random>"), ExpandString("<spice.history.gospels.EnemyHostName." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), ExpandString("<spice.history.gospels.CommittedWrongAgainstSultan." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>"), "<spice.elements." + randomElement + ".ruinDescription>", text, ExpandString("<spice.commonPhrases.might.!random>"), ExpandString("<spice.commonPhrases.warning.!random>"), ExpandString("<spice.commonPhrases.challenge.!random>"), "<entity.objectPronoun>");
		SetEventProperty("tombInscription", value3);
		SetEventProperty("tombInscriptionCategory", "DoesSomethingDestructive");
		SetEventProperty("revealsRegion", QudHistoryHelpers.GetRegionNewName(history, property));
		QudHistoryHelpers.RenameLocation(property2, text, history);
		dictionary.Add("battles", randomElement);
		history.GetEntitiesWherePropertyEquals("name", text).GetRandomElement()?.ApplyEvent(new SetEntityProperties(null, dictionary));
		history.GetEntitiesWherePropertyEquals("name", property).GetRandomElement()?.ApplyEvent(new SetEntityProperties(null, dictionary));
	}
}
