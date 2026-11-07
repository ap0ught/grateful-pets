using System;
using System.Collections.Generic;
using HistoryKit;
using UnityEngine;
using XRL.Language;

namespace XRL.Annals;

[Serializable]
public class LoseItemAtTavern : HistoricEvent
{
	public string regionToReveal;

	public LoseItemAtTavern()
	{
	}

	public LoseItemAtTavern(string _regionToReveal)
	{
		regionToReveal = _regionToReveal;
	}

	public override void Generate()
	{
		duration = 0L;
		HistoricEntitySnapshot snapshotAtYear = entity.GetSnapshotAtYear(entity.lastYear);
		string text = ((!string.IsNullOrEmpty(regionToReveal)) ? regionToReveal : QudHistoryHelpers.GetNewRegion(history, snapshotAtYear.GetProperty("region")));
		int period = int.Parse(snapshotAtYear.GetProperty("period"));
		string randomLocationInRegionByPeriod = QudHistoryHelpers.GetRandomLocationInRegionByPeriod(history, text, period);
		if (string.IsNullOrEmpty(randomLocationInRegionByPeriod))
		{
			Debug.LogError("LoseItemAtTavern::Generate could not get a random historical location in region " + text + " for period " + period);
			return;
		}
		string randomElement = snapshotAtYear.GetList("elements").GetRandomElement();
		string text2 = ExpandString("<spice.elements." + randomElement + ".adjectives.!random>");
		SetEntityProperty("region", text);
		string newLocationInRegion = QudHistoryHelpers.GetNewLocationInRegion(history, text, randomLocationInRegionByPeriod);
		SetEntityProperty("location", newLocationInRegion);
		string text3 = null;
		bool flag = false;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string text4;
		if (snapshotAtYear.GetList("items").Count != 0)
		{
			text4 = snapshotAtYear.GetList("items").GetRandomElement();
			RemoveEntityListItem("items", text4);
		}
		else
		{
			flag = true;
			do
			{
				text4 = Grammar.MakeTitleCase(QudHistoryHelpers.NameItemAdjRoot(text2, history, entity) + " " + Grammar.GetRandomMeaningfulWord(snapshotAtYear.GetProperty("name")) + ExpandString("<spice.commonPhrases.boon.!random>"));
			}
			while (history.GetEntitiesWherePropertyEquals("name", text4).Count > 0);
			text3 = ExpandString("<spice.items.types.!random>");
			HistoricEntity historicEntity = history.CreateEntity(year);
			dictionary.Add("itemType", text3);
			QudHistoryHelpers.ExtractArticle(ref text4, out var Article);
			dictionary.Add("baseName", text4);
			dictionary.Add("article", Article);
			text4 = QudHistoryHelpers.Ansify(text4, Article);
			dictionary.Add("name", text4);
			dictionary.Add("descriptionAdj", text2);
			dictionary.Add("location", randomLocationInRegionByPeriod);
			dictionary.Add("period", snapshotAtYear.GetProperty("period"));
			historicEntity.ApplyEvent(new SetEntityProperties(dictionary, null));
		}
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		dictionary2.Add("items", text4);
		HistoricEntityList entitiesWherePropertyEquals = history.GetEntitiesWherePropertyEquals("name", randomLocationInRegionByPeriod);
		if (entitiesWherePropertyEquals.Count == 0)
		{
			Debug.LogError("no matching location (1) for " + randomLocationInRegionByPeriod);
		}
		HistoricEntity randomElement2 = entitiesWherePropertyEquals.GetRandomElement();
		if (randomElement2 == null)
		{
			Debug.LogError("no matching location (2) " + randomLocationInRegionByPeriod);
		}
		if (randomElement2 != null)
		{
			randomElement2.ApplyEvent(new SetEntityProperties(null, dictionary2));
			history.GetEntitiesWherePropertyEquals("name", text)?.GetRandomElement().ApplyEvent(new SetEntityProperties(null, dictionary2));
		}
		if (flag)
		{
			string value = "While traveling through " + text + ", <entity.name> stopped at a market in " + randomLocationInRegionByPeriod + ". At an obscure shop, <entity.subjectPronoun> purchased " + Grammar.A(text2) + " " + text3 + " and named it " + text4 + ". Then <entity.subjectPronoun> went to a nearby tavern and lost " + text4 + " <spice.commonPhrases.lostInTavern.!random>. <entity.subjectPronoun.capitalize> cursed the tavern and left " + randomLocationInRegionByPeriod + ".";
			SetEventProperty("gospel", value);
		}
		else
		{
			string value = "While traveling through " + text + ", <entity.name> stopped at a tavern in " + randomLocationInRegionByPeriod + ". There <entity.subjectPronoun> lost <entity.possessivePronoun> prized " + text4 + " <spice.commonPhrases.lostInTavern.!random>. <entity.subjectPronoun.capitalize> cursed the tavern and left " + randomLocationInRegionByPeriod + ".";
			SetEventProperty("gospel", value);
		}
		string value2 = string.Format("{0} {1} of {2}! It was there that {3} {4}.", ExpandString("<spice.instancesOf.aCurseUpon.!random.capitalize>"), randomLocationInRegionByPeriod, text, "<entity.name>", ExpandString("<spice.history.gospels.LostItem." + QudHistoryHelpers.GetSultanateEra(snapshotAtYear) + ".!random>").Replace("*item*", text4));
		SetEventProperty("tombInscription", value2);
		SetEventProperty("tombInscriptionCategory", "CommitsFolly");
		string regionNewName = QudHistoryHelpers.GetRegionNewName(history, text);
		SetEventProperty("revealsRegion", regionNewName);
		SetEventProperty("revealsItem", text4);
		SetEventProperty("revealsItemLocation", randomLocationInRegionByPeriod);
		SetEventProperty("revealsItemRegion", regionNewName);
		Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
		dictionary3.Add("parameters", "tavern");
		HistoricEntity randomElement3 = history.GetEntitiesWherePropertyEquals("name", randomLocationInRegionByPeriod).GetRandomElement();
		if (randomElement3 != null)
		{
			randomElement3.ApplyEvent(new SetEntityProperties(null, dictionary3));
		}
		else
		{
			Debug.LogError("LoseItemAtTavern::Generate could not find an entity with name=" + randomLocationInRegionByPeriod);
		}
	}
}
