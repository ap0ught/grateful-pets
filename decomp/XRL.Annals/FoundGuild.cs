using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class FoundGuild : HistoricEvent
{
	public override void Generate()
	{
		duration = 1L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string property = snapshotAtYear.GetProperty("region");
		string newRegion = QudHistoryHelpers.GetNewRegion(history, property);
		string randomElement = snapshotAtYear.GetList("elements").GetRandomElement();
		SetEntityProperty("region", newRegion);
		string text = (snapshotAtYear.GetProperty("profession").Equals("unknown") ? ExpandString("<spice.professions.!random>") : snapshotAtYear.GetProperty("profession"));
		string text2 = ExpandString("<spice.elements." + randomElement + ".nounsPlural.!random>");
		string newFaction = QudHistoryHelpers.GetNewFaction(entity);
		AddEntityListItem("likedFactions", newFaction);
		HistoricEntity historicEntity = history.CreateEntity(history.currentYear);
		historicEntity.ApplyEvent(new InitializeLocation(newRegion, int.Parse(snapshotAtYear.GetProperty("period"))));
		string text3;
		do
		{
			text3 = ((Random(0, 1) != 0) ? ("the " + Grammar.MakeTitleCase(ExpandString("<spice.professions." + text + ".guildhall> of the <spice.elements." + randomElement + ".adjectives.!random>"))) : ("the " + Grammar.MakeTitleCase(ExpandString("<spice.elements." + randomElement + ".adjectives.!random> <spice.professions." + text + ".guildhall>"))));
		}
		while (history.GetEntitiesWherePropertyEquals("name", text3).Count > 0);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("name", text3);
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		dictionary2.Add("parameters", ExpandString("<spice.professions." + text + ".guildhall>"));
		historicEntity.ApplyEvent(new SetEntityProperties(dictionary, dictionary2));
		new Dictionary<string, string>().Add("parameters", randomElement);
		historicEntity.ApplyEvent(new SetEntityProperties(null, dictionary2));
		history.GetEntitiesWherePropertyEquals("name", newRegion).GetRandomElement().ApplyEvent(new AddLocationToRegion(text3));
		string value = "After <spice.commonPhrases.treating.!random> with " + Faction.GetFormattedName(newFaction) + ", <entity.name> convinced them to help <entity.objectPronoun> found <spice.professions." + text + ".guildhall.article> in " + newRegion + " for the purpose of <spice.professions." + text + ".studying> " + text2 + ". They named it " + text3 + ".";
		SetEventProperty("gospel", value);
		string value2 = ExpandString("<spice.history.gospels.CivilizationActivity." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>");
		Dictionary<string, string> vars = new Dictionary<string, string>
		{
			{ "*var*", text2 },
			{ "*activity*", value2 }
		};
		string text4 = ExpandString("<spice.professions." + text + ".foundingVerbTombInscription.!random>", vars);
		string value3 = string.Format("In {0} we {1} the {2} of {3}, where {4} {5}.", QudHistoryHelpers.GenerateSultanateYearName(), ExpandString("<spice.instancesOf.rejoicedAt.!random>"), ExpandString("<spice.commonPhrases.inauguration.!random>"), text3, Faction.GetFormattedName(newFaction), text4);
		SetEventProperty("tombInscription", value3);
		SetEventProperty("tombInscriptionCategory", "CreatesSomething");
	}
}
