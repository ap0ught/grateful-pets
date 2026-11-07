using System.Collections.Generic;
using HistoryKit;
using XRL.Annals;
using XRL.Language;
using XRL.Rules;

namespace XRL.Names;

public static class SettlementNames
{
	public static string GenerateGoatfolkVillageName(History history)
	{
		return NameMaker.MakeName(null, null, null, null, "Goatfolk", null, null, null, null, null, null, "Site", new Dictionary<string, string> { 
		{
			"*Sultan*",
			QudHistoryHelpers.GetRandomSultan(history).GetCurrentSnapshot().GetProperty("nameRoot") + " "
		} });
	}

	public static string GenerateGoatfolkQlippothVillageName(History history)
	{
		return NameMaker.MakeName(null, null, null, null, "Goatfolk", null, null, null, null, null, "Qlippoth", "Site", new Dictionary<string, string> { 
		{
			"*Sultan*",
			QudHistoryHelpers.GetRandomSultan(history).GetCurrentSnapshot().GetProperty("nameRoot") + " "
		} }, FailureOkay: false, SpecialFaildown: true);
	}

	public static string GenerateStarappleFarmName(History history)
	{
		return GenerateFarmName(history, "starapple");
	}

	public static string GeneratePigFarmName(History history)
	{
		return GenerateFarmName(history, "pig");
	}

	private static string GenerateFarmNameInner(History history, string type)
	{
		HistoricEntityList sultans = QudHistoryHelpers.GetSultans(history);
		string text = MutantNameMaker.MakeMutantName();
		if (text.IsNullOrEmpty())
		{
			MetricsManager.LogError("got " + ((text == null) ? "null" : "empty") + " name from MakeMutantName()");
			text = "Urist";
		}
		string text2 = null;
		string text3 = null;
		string text4 = "";
		int num = Stat.Random(0, 100);
		int num2 = Stat.Random(0, 100);
		if (15.in100())
		{
			if (type == "pig")
			{
				return Grammar.A(HistoricStringExpander.ExpandString("<spice.commonPhrases.secluded.!random>", null, history) + " pig farm");
			}
			return Grammar.A(HistoricStringExpander.ExpandString("<spice.commonPhrases.secluded.!random>", null, history) + " starapple farm");
		}
		if (num < 80)
		{
			text2 = text;
			if (30.in100())
			{
				text4 = "the ";
			}
			else if (50.in100())
			{
				text2 = Grammar.MakePossessive(text);
			}
		}
		else if (num < 90)
		{
			text2 = sultans.GetRandomElement().GetCurrentSnapshot().GetProperty("nameRoot");
			if (text2.IsNullOrEmpty())
			{
				MetricsManager.LogError("got " + ((text == null) ? "null" : "empty") + " name root from sultan");
				text2 = "Urist";
			}
		}
		else
		{
			text2 = QudHistoryHelpers.GetRandomCognomen(history);
		}
		if (num2 < 50)
		{
			text3 = HistoricStringExpander.ExpandString("<spice.commonPhrases." + type + "farm.!random>", null, history);
		}
		else
		{
			text3 = HistoricStringExpander.ExpandString("<spice.commonPhrases.shire.!random>", null, history);
			if (20.in100())
			{
				text2 = HistoricStringExpander.ExpandString("<spice.instancesOf." + type + "farmyPrefixes.!random>", null, history);
				return Grammar.MakeTitleCase(text2 + text3);
			}
		}
		if (num < 90)
		{
			return text4 + Grammar.MakeTitleCase(text2 + " " + text3);
		}
		return "the " + Grammar.MakeTitleCase(text3 + " of " + text4 + text2);
	}

	public static string GenerateFarmName(History History, string Type)
	{
		string text = GenerateFarmNameInner(History, Type);
		if (text.IsNullOrEmpty() || text.EndsWith("of") || text.EndsWith("the") || text.EndsWith(" "))
		{
			MetricsManager.LogError("generated " + Type + " farm name \"" + (text ?? "NULL") + "\"");
		}
		return text;
	}
}
