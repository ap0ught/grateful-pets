using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.Names;
using XRL.World;

namespace XRL.Annals;

[Serializable]
public class Marry : HistoricEvent
{
	public override void Generate()
	{
		duration = 0L;
		HistoricEntitySnapshot currentSnapshot = entity.GetCurrentSnapshot();
		string randomElement = currentSnapshot.GetList("elements").GetRandomElement();
		string text = ExpandString("<spice.elements." + randomElement + ".weddingConditions.!random>");
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string text6 = null;
		string text7 = null;
		string text8 = null;
		bool flag = false;
		if (50.in100())
		{
			text2 = QudHistoryHelpers.GetNewFaction(entity);
			AddEntityListItem("likedFactions", text2);
			List<GameObjectBlueprint> factionMembers = GameObjectFactory.Factory.GetFactionMembers(text2);
			text4 = ((factionMembers.Count <= 0) ? NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, null, null, "Site") : NameMaker.MakeName(GameObject.Create(factionMembers.GetRandomElement().Name)));
			text3 = Faction.GetFormattedName(text2);
			if (90.in100())
			{
				string value = ExpandString("<spice.items.types.!random>");
				text7 = QudHistoryHelpers.NameItemNounRoot(ExpandString("<spice.commonPhrases.marriage.!random>"), history, null) + ", " + Grammar.GetRandomMeaningfulWord(text3) + ExpandString("<spice.commonPhrases.gift.!random>");
				text7 = Grammar.MakeTitleCase(text7);
				HistoricEntity historicEntity = history.CreateEntity(year);
				Dictionary<string, string> dictionary = new Dictionary<string, string> { { "itemType", value } };
				QudHistoryHelpers.ExtractArticle(ref text7, out var Article);
				dictionary.Add("baseName", text7);
				dictionary.Add("article", Article);
				text7 = QudHistoryHelpers.Ansify(text7);
				dictionary.Add("name", text7);
				dictionary.Add("period", currentSnapshot.GetProperty("period"));
				historicEntity.ApplyEvent(new SetEntityProperties(dictionary, new Dictionary<string, string>
				{
					{ "lovedFactions", text2 },
					{ "elements", "none" }
				}));
				AddEntityListItem("items", text7);
				flag = true;
			}
			text8 = Grammar.Pluralize(ExpandString("<spice.placeNouns.!random>"));
		}
		else
		{
			string text9 = ExpandString("<spice.professions.!random>");
			text3 = ExpandString("<spice.professions." + text9 + ".plural>");
			SetEntityProperty("profession", text9);
			SetEntityProperty("professionRank", ExpandString("<spice.professions." + text9 + ".training>"));
			text4 = NameMaker.MakeName(null, null, null, null, "Qudish", null, null, null, null, null, null, "Site");
			text8 = Grammar.Pluralize(ExpandString("<spice.professions." + text9 + ".guildhall>"));
		}
		if (Random(0, 100) >= 70)
		{
			text6 = Grammar.MakeTitleCase(ExpandString("<spice.commonPhrases.spouse.!random>"));
		}
		else
		{
			text6 = ((currentSnapshot.GetProperty("subjectPronoun") == "he") ? "Husband" : "Wife");
		}
		if (30.in100())
		{
			text5 = Grammar.MakePossessive(text4) + " " + text6;
		}
		else
		{
			text5 = ((!50.in100()) ? (text6 + " of " + text4) : (text6 + " to " + text4));
		}
		AddEntityListItem("cognomen", text5);
		string text10 = string.Format("{0}, {1} cemented {2} friendship with {3} by marrying {4}.", text, currentSnapshot.GetProperty("name"), currentSnapshot.GetProperty("possessivePronoun"), text3, text4);
		if (flag)
		{
			text10 += string.Format(" {0}, {1} bestowed upon {2} a wedding gift they called {3}.", Grammar.InitCap(ExpandString("<spice.commonPhrases.inHonorOf.!random>")), "the " + text3, currentSnapshot.GetProperty("name"), text7);
		}
		SetEventProperty("gospel", Grammar.InitCap(text10));
		string value2 = string.Format("{0}, {1} and {2}, whose {3} alliance by marriage {4}.", "<spice.instancesOf.marriageBlessing.!random.capitalize>", "<entity.name>", text4, "<spice.commonPhrases.historic.!random>", ExpandString("<spice.history.gospels.MarriageAllianceResult." + QudHistoryHelpers.GetSultanateEra(currentSnapshot) + ".!random>").Replace("*group*", text3).Replace("*hall*", text8));
		SetEventProperty("tombInscription", value2);
		SetEventProperty("tombInscriptionCategory", "Trysts");
	}
}
