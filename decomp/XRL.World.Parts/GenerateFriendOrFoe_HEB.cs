using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Language;
using XRL.World.Encounters;

namespace XRL.World.Parts;

[HasModSensitiveStaticCache]
public class GenerateFriendOrFoe_HEB
{
	[ModSensitiveStaticCache(false)]
	public static List<string> hateReasons;

	[ModSensitiveStaticCache(false)]
	public static List<string> likeReasons;

	[ModSensitiveStaticCache(false)]
	public static List<Func<string, string>> reasonReplacers;

	[ModSensitiveCacheInit]
	public static void Init()
	{
		if (hateReasons == null)
		{
			hateReasons = new List<string>();
		}
		hateReasons.Add("inventing the irrational numbers");
		hateReasons.Add("destroying the $adjective numbers");
		hateReasons.Add("dreaming $dimension into being");
		hateReasons.Add("inventing the concept of $nouns");
		hateReasons.Add("swapping how $objnouns and $objnoun2s are perceived");
		hateReasons.Add("warping a pocket of spacetime into a $weirdobj");
		if (likeReasons == null)
		{
			likeReasons = new List<string>();
		}
		likeReasons.Add("inventing the irrational numbers");
		likeReasons.Add("destroying the $adjective numbers");
		likeReasons.Add("dreaming $dimension into being");
		likeReasons.Add("inventing the concept of $nouns");
		likeReasons.Add("swapping how $objnouns and $objnoun2s are perceived");
		likeReasons.Add("warping a pocket of spacetime into a $weirdobj");
		if (reasonReplacers == null)
		{
			reasonReplacers = new List<Func<string, string>>();
		}
		reasonReplacers.Add(replacePlaceholders);
	}

	public static string replacePlaceholders(string reason)
	{
		reason = HistoricStringExpander.ExpandString(reason, null, null, new Dictionary<string, string>
		{
			{
				"$nouns",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.nouns.!random>"))
			},
			{
				"$noun2s",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.nouns.!random>"))
			},
			{
				"$objnouns",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.objectNouns.!random>"))
			},
			{
				"$objnoun2s",
				Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.objectNouns.!random>"))
			},
			{
				"$weirdobj",
				HistoricStringExpander.ExpandString("<spice.history.gospels.EarlySultanate.location.!random>")
			},
			{
				"$adjective",
				HistoricStringExpander.ExpandString("<spice.adjectives.!random>")
			}
		});
		if (reason.Contains("$dimension"))
		{
			List<ExtraDimension> list = (The.Game.GetObjectGameState("DimensionManager") as DimensionManager)?.ExtraDimensions;
			if (list.IsNullOrEmpty())
			{
				reason = reason.Replace("$dimension", "an uncharted dimension");
			}
			else
			{
				ExtraDimension randomElement = list.GetRandomElement();
				string newValue = randomElement.Name.Replace("*DimensionSymbol*", ((char)randomElement.Symbol).ToString());
				reason = reason.Replace("$dimension", newValue);
			}
		}
		return reason;
	}

	public static string getHateReason()
	{
		string text = hateReasons.GetRandomElement();
		foreach (Func<string, string> reasonReplacer in reasonReplacers)
		{
			text = reasonReplacer(text);
		}
		return text;
	}

	public static string getLikeReason()
	{
		string text = likeReasons.GetRandomElement();
		foreach (Func<string, string> reasonReplacer in reasonReplacers)
		{
			text = reasonReplacer(text);
		}
		return text;
	}
}
