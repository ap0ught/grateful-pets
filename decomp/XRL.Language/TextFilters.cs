using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HistoryKit;
using XRL.Rules;
using XRL.World.Encounters;

namespace XRL.Language;

public static class TextFilters
{
	private static StringBuilder SB = new StringBuilder();

	private static readonly string[] CORVID_WORDS = new string[5] { "{{emote|*CAAW*}}", "{{emote|*CAAAAW*}}", "{{emote|*CAAAAW*}}", "{{emote|*CAAW*}}", "{{emote|*CAAAAAAAAAAW*}}" };

	private static readonly string[] WATERBIRD_WORDS = new string[6] { "{{emote|*HONK*}}", "{{emote|*HONK*}}", "{{emote|*HONK*}}", "{{emote|*HOONK*}}", "{{emote|*HOOOONK*}}", "{{emote|*HOOOOOOOOOONK*}}" };

	private static readonly string[] FISH_WORDS = new string[6] { "{{emote|*blub*}}", "{{emote|*blub blub*}}", "{{emote|*blub blub*}}", "{{emote|*blub*}}", "{{emote|*blub*}}", "{{emote|*blub*}}" };

	private static readonly string[] FROG_WORDS = new string[13]
	{
		"{{emote|*crk*}}", "{{emote|*crrk*}}", "{{emote|*crrk*}}", "{{emote|*crrrrrk*}}", "{{emote|*crrrrrrrrrrrk*}}", "{{emote|*CRRK*}}", "{{emote|*reep*}}", "{{emote|*reep*}}", "{{emote|*reep*}}", "{{emote|*reeeeep*}}",
		"{{emote|*reeeeeeeeeeep*}}", "{{emote|*bep*}}", "{{emote|*rff*}}"
	};

	private static readonly char[] CRYPTIC_MACHINE_CHARS = new char[40]
	{
		'³', '\u00b4', 'µ', '¶', '·', '\u00b8', '¹', 'º', '»', '¼',
		'½', '¾', '¿', 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'Æ',
		'Ç', 'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï', 'Ð',
		'Ñ', 'Ò', 'Ó', 'Ô', 'Õ', 'Ö', '×', 'Ø', 'Ù', 'Ú'
	};

	private static readonly int CRYPTIC_WORD_LENGTH_LOWER_BOUND = 3;

	private static readonly int CRYPTIC_WORD_LENGTH_UPPER_BOUND = 10;

	private static StringBuilder CrypticWordSB = new StringBuilder();

	private static readonly int CRYPTIC_SENTENCE_LENGTH_LOWER_BOUND = 3;

	private static readonly int CRYPTIC_SENTENCE_LENGTH_UPPER_BOUND = 40;

	private static StringBuilder CrypticSentenceSB = new StringBuilder();

	public static string Filter(string Phrase, string Filter, string Extras = null, bool FormattingProtect = true)
	{
		return Filter switch
		{
			"Angry" => Angry(Phrase), 
			"Corvid" => Corvid(Phrase), 
			"WaterBird" => WaterBird(Phrase), 
			"Fish" => Fish(Phrase), 
			"Frog" => Frog(Phrase), 
			"Leet" => Leet(Phrase, FormattingProtect), 
			"Lallated" => Lallated(Phrase, Extras), 
			"Weird" => Weird(Phrase, Extras), 
			"Cryptic Machine" => CrypticMachine(Phrase), 
			_ => Phrase, 
		};
	}

	public static string Corvid(string Text)
	{
		SB.Clear();
		bool flag = false;
		bool flag2 = false;
		char c = '\0';
		int i = 0;
		for (int length = Text.Length; i < length; i++)
		{
			char c2 = Text[i];
			switch (c2)
			{
			case '=':
				flag = !flag;
				break;
			case '*':
				flag2 = !flag2;
				break;
			default:
				if (!flag && !flag2 && c2 == ' ' && c != ' ' && 17.in100())
				{
					SB.Append(' ').Append(CORVID_WORDS.GetRandomElement());
				}
				break;
			}
			SB.Append(c2);
			if (!flag && !flag2)
			{
				c = c2;
			}
		}
		return SB.ToString();
	}

	public static string WaterBird(string Text)
	{
		SB.Clear();
		bool flag = false;
		bool flag2 = false;
		char c = '\0';
		int i = 0;
		for (int length = Text.Length; i < length; i++)
		{
			char c2 = Text[i];
			switch (c2)
			{
			case '=':
				flag = !flag;
				break;
			case '*':
				flag2 = !flag2;
				break;
			case ' ':
				if (!flag && !flag2 && c != ' ' && 17.in100())
				{
					SB.Append(' ').Append(WATERBIRD_WORDS.GetRandomElement());
				}
				break;
			}
			SB.Append(c2);
			if (!flag && !flag2)
			{
				c = c2;
			}
		}
		return SB.ToString();
	}

	public static string Fish(string Text)
	{
		SB.Clear();
		bool flag = false;
		bool flag2 = false;
		char c = '\0';
		int i = 0;
		for (int length = Text.Length; i < length; i++)
		{
			char c2 = Text[i];
			switch (c2)
			{
			case '=':
				flag = !flag;
				break;
			case '*':
				flag2 = !flag2;
				break;
			case ' ':
				if (!flag && !flag2 && c != ' ' && 17.in100())
				{
					SB.Append(' ').Append(FISH_WORDS.GetRandomElement());
				}
				break;
			}
			SB.Append(c2);
			if (!flag && !flag2)
			{
				c = c2;
			}
		}
		return SB.ToString();
	}

	public static string Frog(string Text)
	{
		SB.Clear();
		bool flag = false;
		bool flag2 = false;
		char c = '\0';
		int i = 0;
		for (int length = Text.Length; i < length; i++)
		{
			char c2 = Text[i];
			switch (c2)
			{
			case '=':
				flag = !flag;
				break;
			case '*':
				flag2 = !flag2;
				break;
			case ' ':
				if (!flag && !flag2 && c != ' ' && 9.in100())
				{
					SB.Append(' ').Append(FROG_WORDS.GetRandomElement());
				}
				break;
			}
			SB.Append(c2);
			if (!flag && !flag2)
			{
				c = c2;
			}
		}
		return SB.ToString();
	}

	public static string Angry(string Phrase)
	{
		string[] array = Phrase.Split(new string[1] { ". " }, StringSplitOptions.None);
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (50.in100())
			{
				array[i] = Grammar.Stutterize(array[i], HistoricStringExpander.ExpandString("<spice.textFilters.angry.!random>"));
			}
		}
		return string.Join(". ", array);
	}

	public static string Lallated(string Text, string Noise)
	{
		Dictionary<string, string> vars = new Dictionary<string, string>
		{
			{ "*Text*", Text },
			{
				"*Noise*",
				Noise.Split(',').GetRandomElement()
			}
		};
		return HistoricStringExpander.ExpandString("<spice.textFilters.lallated.!random>", null, null, vars);
	}

	public static string Leet(string Text, bool FormattingProtect = true)
	{
		if (Text == null)
		{
			return null;
		}
		Text = Regex.Replace(Text, "atable\\b", "8able", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "anned\\b", FormattingProtect ? "&&" : "&", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "and\\b", FormattingProtect ? "&&" : "&", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "ude\\b", "00|)", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "too\\b", "2", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bto\\b", "2", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bone\\b", "1", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bwon\\b", "1", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\btwo\\b", "1", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bthree\\b", "3", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bfou?r\\b", "4", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bfive\\b", "5", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bsix\\b", "6", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bseven\\b", "7", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\beight\\b", "8", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "ate\\b", "8", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\bare\\b", "R", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "nine\\b", "9", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "\\byou\\b", "U", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "at\\b", "@", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "S\\b", "Z");
		Text = Regex.Replace(Text, "s\\b", "z");
		Text = Regex.Replace(Text, "a(?=\\w)", "4", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "(?<=\\w)a", "4", RegexOptions.IgnoreCase);
		Text = Text.Replace("EW", "00").Replace("ew", "00");
		Text = Regex.Replace(Text, "b(?=\\w)", "8", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "(?<=\\w)b", "8", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "c(?=\\w)", "(", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "(?<=\\w)c", "(", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "o(?=\\w)", "0", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "(?<=\\w)o", "0", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "w(?=\\w)", "\\/\\/", RegexOptions.IgnoreCase);
		Text = Regex.Replace(Text, "(?<=\\w)w", "\\/\\/", RegexOptions.IgnoreCase);
		Text = Text.Replace("D", "|)").Replace("d", "|)").Replace("E", "3")
			.Replace("e", "3")
			.Replace("H", "#")
			.Replace("h", "#")
			.Replace("I", "1")
			.Replace("i", "1")
			.Replace("S", "5")
			.Replace("s", "5")
			.Replace("T", "7")
			.Replace("t", "7")
			.Replace("V", "\\/")
			.Replace("v", "\\/");
		return Text;
	}

	public static string Weird(string Text, string PatternSpec = null)
	{
		int result = 0;
		if (!PatternSpec.IsNullOrEmpty())
		{
			if (The.Game.GetObjectGameState("DimensionManager") is DimensionManager dimensionManager)
			{
				foreach (PsychicFaction psychicFaction in dimensionManager.PsychicFactions)
				{
					if (psychicFaction.factionName == PatternSpec)
					{
						return psychicFaction.Weirdify(Text);
					}
				}
				foreach (ExtraDimension extraDimension in dimensionManager.ExtraDimensions)
				{
					if (extraDimension.Name == PatternSpec)
					{
						return extraDimension.Weirdify(Text);
					}
				}
			}
			int.TryParse(PatternSpec, out result);
		}
		SB.Clear();
		bool flag = false;
		bool flag2 = false;
		int i = 0;
		for (int length = Text.Length; i < length; i++)
		{
			char c = Text[i];
			switch (c)
			{
			case '=':
				flag = !flag;
				break;
			case '*':
				flag2 = !flag2;
				break;
			default:
				if (!flag && !flag2)
				{
					c = c switch
					{
						'a' => Grammar.weirdLowerAs.GetCyclicElement(result), 
						'A' => Grammar.weirdUpperAs.GetCyclicElement(result), 
						'e' => Grammar.weirdLowerEs.GetCyclicElement(result), 
						'E' => Grammar.weirdUpperEs.GetCyclicElement(result), 
						'i' => Grammar.weirdLowerIs.GetCyclicElement(result), 
						'I' => Grammar.weirdUpperIs.GetCyclicElement(result), 
						'o' => Grammar.weirdLowerOs.GetCyclicElement(result), 
						'O' => Grammar.weirdUpperOs.GetCyclicElement(result), 
						'u' => Grammar.weirdLowerUs.GetCyclicElement(result), 
						'U' => Grammar.weirdUpperUs.GetCyclicElement(result), 
						'c' => Grammar.weirdLowerCs.GetCyclicElement(result), 
						'f' => Grammar.weirdLowerFs.GetCyclicElement(result), 
						'n' => Grammar.weirdLowerNs.GetCyclicElement(result), 
						't' => Grammar.weirdLowerTs.GetCyclicElement(result), 
						'y' => Grammar.weirdLowerYs.GetCyclicElement(result), 
						'B' => Grammar.weirdUpperBs.GetCyclicElement(result), 
						'C' => Grammar.weirdUpperCs.GetCyclicElement(result), 
						'Y' => Grammar.weirdUpperYs.GetCyclicElement(result), 
						'L' => Grammar.weirdUpperLs.GetCyclicElement(result), 
						'R' => Grammar.weirdUpperRs.GetCyclicElement(result), 
						'N' => Grammar.weirdUpperNs.GetCyclicElement(result), 
						_ => c, 
					};
				}
				break;
			}
			SB.Append(c);
		}
		return SB.ToString();
	}

	public static void GenerateCrypticWord(StringBuilder SB)
	{
		int i = 0;
		for (int num = Stat.Random(CRYPTIC_WORD_LENGTH_LOWER_BOUND, CRYPTIC_WORD_LENGTH_UPPER_BOUND); i < num; i++)
		{
			SB.Append(CRYPTIC_MACHINE_CHARS.GetRandomElement());
		}
	}

	public static string GenerateCrypticWord()
	{
		CrypticWordSB.Clear();
		GenerateCrypticWord(CrypticWordSB);
		return CrypticWordSB.ToString();
	}

	public static string CrypticMachine(string Text)
	{
		if (Text.Contains("*READOUT*"))
		{
			CrypticSentenceSB.Clear();
			CrypticSentenceSB.Append("{{c|");
			int i = 0;
			for (int num = Stat.Random(CRYPTIC_SENTENCE_LENGTH_LOWER_BOUND, CRYPTIC_SENTENCE_LENGTH_UPPER_BOUND); i < num; i++)
			{
				if (i > 0)
				{
					CrypticSentenceSB.Append(' ');
				}
				GenerateCrypticWord(CrypticSentenceSB);
			}
			CrypticSentenceSB.Append("}}");
			return CrypticSentenceSB.ToString();
		}
		CrypticSentenceSB.Clear();
		bool flag = false;
		bool flag2 = false;
		char c = '\0';
		int j = 0;
		for (int length = Text.Length; j < length; j++)
		{
			char c2 = Text[j];
			switch (c2)
			{
			case '=':
				flag = !flag;
				break;
			case '*':
				flag2 = !flag2;
				break;
			case ' ':
				if (!flag && !flag2 && c != ' ' && 9.in100())
				{
					CrypticSentenceSB.Append(" {{c|");
					GenerateCrypticWord(CrypticSentenceSB);
					CrypticSentenceSB.Append("}} ");
				}
				break;
			}
			CrypticSentenceSB.Append(c2);
			if (!flag && !flag2)
			{
				c = c2;
			}
		}
		return CrypticSentenceSB.ToString();
	}
}
