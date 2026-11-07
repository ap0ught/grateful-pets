using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using XRL.Language;
using XRL.Names;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class EaterCryptPlaque : IPart
{
	public string Inscription = "";

	public string Prefix = "";

	public string Postfix = "";

	public bool NeedsGeneration = true;

	public bool IsEmpty;

	public string Caste = "tutor";

	public string Faction = "Eater";

	public string FamilyName;

	public string EmptyText = "<The plaque has no inscription.>";

	private const int CHANCE_FOR_PLURAL_FAMILY_TITLE = 70;

	private const int CHANCE_USE_FAMILY_COGNOMEN = 60;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				Look.ShowLooker(0, cell.X, cell.Y);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (NeedsGeneration)
		{
			GeneratePlaque();
		}
		E.Prefix.Append(Prefix);
		E.Base.Clear();
		E.Base.Append(Inscription);
		E.Postfix.Append(Postfix);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void GeneratePlaque()
	{
		if (FamilyName == null)
		{
			if (Faction == "Eater")
			{
				FamilyName = NameMaker.Eater();
			}
			else
			{
				FamilyName = MutantNameMaker.MakeMutantName();
			}
		}
		int num = (If.Chance(70) ? 1 : 0);
		string text = ((Caste == "royal") ? "kindred" : Caste);
		string newValue;
		string text2;
		string text3;
		if (num != 0)
		{
			text2 = HistoricStringExpander.ExpandString("<spice.tombstones.cryptIntroPlural.!random>");
			if (If.CoinFlip())
			{
				text3 = HistoricStringExpander.ExpandString("<spice.tombstones.cryptPlaque.familyTitlePlural.!random>");
				newValue = ((!If.Chance(60)) ? "" : HistoricStringExpander.ExpandString("<spice.tombstones.cryptPlaque." + Caste + ".familyCognomen.!random>").Replace("*" + text + "*", Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.instancesOf." + text + ".!random>"))).Replace("*personNouns*", Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.personNouns.!random>"))));
			}
			else
			{
				text3 = HistoricStringExpander.ExpandString("<spice.tombstones.cryptPlaque.familyTitleSingular.!random>");
				newValue = HistoricStringExpander.ExpandString("<spice.tombstones.cryptPlaque." + Caste + ".familyCognomen.!random>").Replace("*" + text + "*", Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.instancesOf." + text + ".!random>"))).Replace("*personNouns*", Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.personNouns.!random>")));
			}
		}
		else
		{
			text2 = HistoricStringExpander.ExpandString("<spice.tombstones.cryptIntroSingular.!random>");
			newValue = "";
			text3 = HistoricStringExpander.ExpandString("<spice.tombstones.cryptPlaque.familyTitleSingular.!random>");
		}
		text2 = Grammar.MakeTitleCase(text2.Replace("*familyCognomen*", newValue).TrimEnd(' '));
		text3 = Grammar.MakeTitleCase(text3.Replace("*familyName*", FamilyName));
		string text4 = Grammar.InitCap(HistoricStringExpander.ExpandString("<spice.tombstones.cryptPlaque." + Caste + ".familyWords.!random>"));
		string text5 = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text5);
		MarkovChainData data = MarkovBook.CorpusData[text5];
		Match match = Regex.Match(text4, "\\*markovSeed:(.+?)\\*");
		if (match.Success)
		{
			text4 = text4.Replace(match.Groups[0].Value, MarkovChain.GenerateShortSentence(data, MarkovChain.GenerateSeedFromWord(data, match.Groups[1].Value.Split(',').GetRandomElement()), 12).TrimEnd(' '));
		}
		text4 = text4.Replace("*shortMarkov*", MarkovChain.GenerateShortSentence(data, null, 12)).TrimEnd(' ');
		string text6 = "\"" + text4 + "\"";
		text6 = Markup.Wrap(text6);
		List<string> list = new List<string>(16);
		int maxWidth = 26;
		list.Add("");
		if (IsEmpty)
		{
			string[] array = StringFormat.ClipText(EmptyText, maxWidth).Split('\n');
			foreach (string item in array)
			{
				list.Add(item);
			}
			list.Add("");
		}
		else
		{
			string[] array = StringFormat.ClipText(text2, maxWidth).Split('\n');
			foreach (string item2 in array)
			{
				list.Add(item2);
			}
			list.Add("");
			list.Add("");
			array = StringFormat.ClipText(text3, maxWidth).Split('\n');
			foreach (string item3 in array)
			{
				list.Add(item3);
			}
			list.Add("");
			list.Add("");
			array = StringFormat.ClipText(text6, maxWidth).Split('\n');
			foreach (string item4 in array)
			{
				list.Add(item4);
			}
			list.Add("");
		}
		string text7 = "^k&w";
		string text8 = "&W^w";
		string text9 = "w";
		if (Caste == "warrior")
		{
			text7 = "^k&r";
			text8 = "&R^r";
			text9 = "r";
		}
		if (Caste == "tutor")
		{
			text7 = "^k&c";
			text8 = "&C^c";
			text9 = "c";
		}
		if (Caste == "royal")
		{
			text7 = "^k&m";
			text8 = "&M^m";
			text9 = "m";
		}
		Inscription = Event.NewStringBuilder().Append('ÿ').Append('ÿ')
			.Append('ÿ')
			.Append(text8 + "\a" + text7)
			.Append('Ä', 31)
			.Append(text8 + "\a^k&y")
			.ToString();
		for (int j = 0; j < list.Count; j++)
		{
			Inscription += "\nÿÿÿ";
			Inscription = Inscription + text7 + "³^k&y";
			int num2 = ColorUtility.LengthExceptFormatting(list[j]);
			Inscription += list[j].PadLeft(16 + num2 / 2, 'ÿ').PadRight(31, 'ÿ');
			Inscription = Inscription + text7 + "³^k&y";
		}
		Inscription += Event.NewStringBuilder().Append('\n').Append('ÿ')
			.Append('ÿ')
			.Append('ÿ')
			.Append(text8 + "\a" + text7)
			.Append("&" + text9 + "^k")
			.Append('Ä', 31)
			.Append(text8 + "\a^k&y")
			.ToString();
		NeedsGeneration = false;
	}
}
