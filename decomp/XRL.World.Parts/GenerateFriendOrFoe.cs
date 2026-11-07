using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Collections;
using XRL.Language;
using XRL.World.AI;

namespace XRL.World.Parts;

[HasModSensitiveStaticCache]
public class GenerateFriendOrFoe
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
			hateReasons = new List<string>(30);
		}
		hateReasons.Add("insulting their $noun");
		hateReasons.Add("stealing a cherished heirloom");
		hateReasons.Add("slaying one of their leaders");
		hateReasons.Add("eating one of their young");
		hateReasons.Add("casting doubt on their beliefs");
		hateReasons.Add("tricking them into sharing their freshwater");
		hateReasons.Add("some reason no one remembers");
		hateReasons.Add("sowing their fields with salt");
		hateReasons.Add("disproving a famous theorem");
		hateReasons.Add("digging up the remains of their ancestors");
		hateReasons.Add("torching one of their villages");
		hateReasons.Add("repeatedly beating them at dice");
		hateReasons.Add("releasing snakes into one of their camps");
		hateReasons.Add("disparaging a famous poet");
		hateReasons.Add("poisoning their freshwater");
		hateReasons.Add("impersonating one of their leaders");
		hateReasons.Add("leading a raiding party on one of their camps");
		hateReasons.Add("cooking them a rancid meal");
		hateReasons.Add("telling bawdy jokes");
		hateReasons.Add("lighting a beacon fire to warn their enemies");
		hateReasons.Add("refusing them entrance to a local library");
		hateReasons.Add("selling a map of their vaults to adventurers");
		hateReasons.Add("eating all their fruit");
		hateReasons.Add("eavesdropping on their secret ceremonies");
		hateReasons.Add("reprogramming their favorite robot");
		hateReasons.Add("ruining the festival of Ut yara Ux");
		hateReasons.Add("burning one of their leaders in effigy");
		hateReasons.Add("giving one of their kind an unfavorable horoscope reading");
		hateReasons.Add("questioning the origins of the moon");
		hateReasons.Add("worshipping a highly entropic being");
		if (likeReasons == null)
		{
			likeReasons = new List<string>(30);
		}
		likeReasons.Add("praising their $noun");
		likeReasons.Add("sharing freshwater with them");
		likeReasons.Add("making them feel welcomed at a supper feast");
		likeReasons.Add("saving one of their young from drowning");
		likeReasons.Add("resembling one of their idols");
		likeReasons.Add("respecting the sanctity of a burial site");
		likeReasons.Add("worshipping the same deities");
		likeReasons.Add("attending a funeral for their leader");
		likeReasons.Add("uncovering a plot against them");
		likeReasons.Add("penning a moving poem");
		likeReasons.Add("telling bawdy jokes");
		likeReasons.Add("cooking them a splendid meal");
		likeReasons.Add("giving one of their kind a favorable horoscope reading");
		likeReasons.Add("faithfully adapting one of their plays");
		likeReasons.Add("pouring asphalt on one of their enemies");
		likeReasons.Add("explaining the meaning of the Canticles Chromaic");
		likeReasons.Add("reprogramming their least favorite robot");
		likeReasons.Add("fervently celebrating the solstice");
		likeReasons.Add("providing shelter during a glass storm");
		if (reasonReplacers == null)
		{
			reasonReplacers = new List<Func<string, string>>();
		}
		reasonReplacers.Add(replacePlaceholders);
	}

	public static string replacePlaceholders(string reason)
	{
		return reason?.Replace("$noun", Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.nouns.!random>")));
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

	public static string getRandomFaction(GameObject parent)
	{
		using ScopeDisposedList<string> scopeDisposedList = ScopeDisposedList<string>.GetFromPool();
		AllegianceSet baseAllegiance = parent.Brain.GetBaseAllegiance();
		AllegianceSet allegiance = parent.Brain.Allegiance;
		string text = parent.GetxTag("Reputation", "LovedBy");
		foreach (Faction item in Factions.Loop())
		{
			if (item.Visible && !baseAllegiance.ContainsKey(item.Name) && !allegiance.ContainsKey(item.Name) && (text == null || !text.HasDelimitedSubstring(',', item.Name)))
			{
				scopeDisposedList.Add(item.Name);
			}
		}
		return scopeDisposedList.GetRandomElement();
	}
}
