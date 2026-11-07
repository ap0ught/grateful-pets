using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using HistoryKit;
using XRL.Language;
using XRL.World;

namespace XRL.Names;

public class NameStyle
{
	public string Name;

	public int HyphenationChance;

	public bool HyphenationChanceSet;

	public int TwoNameChance;

	public bool TwoNameChanceSet;

	public string Base;

	public string Format = "AsIs";

	public List<NamePrefix> Prefixes = new List<NamePrefix>();

	public string PrefixAmount = "0";

	public List<NameInfix> Infixes = new List<NameInfix>();

	public string InfixAmount = "0";

	public List<NamePostfix> Postfixes = new List<NamePostfix>();

	public string PostfixAmount = "0";

	public List<NameScope> Scopes = new List<NameScope>();

	public List<NameTemplate> Templates = new List<NameTemplate>();

	public Dictionary<string, List<NameValue>> TemplateVars = new Dictionary<string, List<NameValue>>();

	private static StringBuilder SB = new StringBuilder();

	private static int NameGenerationBadWordsFailures;

	public string Generate(GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, string Type = null, Dictionary<string, string> NamingContext = null, bool FailureOkay = false, bool SpecialFaildown = false, NameStyle Skip = null, List<NameStyle> SkipList = null, int? HyphenationChance = null, int? TwoNameChance = null, bool? HasHonorific = null, bool? HasEpithet = null)
	{
		int num = 0;
		string text;
		while (true)
		{
			if (!Base.IsNullOrEmpty())
			{
				if (SkipList != null)
				{
					SkipList = new List<NameStyle>(SkipList);
					SkipList.Add(this);
				}
				else if (Skip != null)
				{
					SkipList = new List<NameStyle> { Skip, this };
					Skip = null;
				}
				else
				{
					Skip = this;
				}
				if (Base == "*")
				{
					text = NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, null, Type, NamingContext, FailureOkay, SpecialFaildown: false, Skip, SkipList, HyphenationChance ?? (HyphenationChanceSet ? new int?(this.HyphenationChance) : ((int?)null)), TwoNameChance ?? (TwoNameChanceSet ? new int?(this.TwoNameChance) : ((int?)null)), HasHonorific, HasEpithet, ForProcessed: true);
				}
				else
				{
					if (!NameStyles.NameStyleTable.TryGetValue(Base, out var value))
					{
						return "InvalidBase:" + Base;
					}
					text = value.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, Type, NamingContext, FailureOkay, SpecialFaildown, Skip, SkipList, HyphenationChance ?? (HyphenationChanceSet ? new int?(this.HyphenationChance) : ((int?)null)), TwoNameChance ?? (TwoNameChanceSet ? new int?(this.TwoNameChance) : ((int?)null)), HasHonorific, HasEpithet);
				}
				if (text == null)
				{
					return null;
				}
			}
			else
			{
				SB.Clear();
				int chance = HyphenationChance ?? this.HyphenationChance;
				int num2 = ((!(TwoNameChance ?? this.TwoNameChance).in100()) ? 1 : 2);
				for (int i = 0; i < num2; i++)
				{
					int num3 = PrefixAmount.RollCached();
					int num4 = InfixAmount.RollCached();
					int num5 = PostfixAmount.RollCached();
					for (int j = 0; j < num3; j++)
					{
						SB.Append(Prefixes.GetRandomNameElement());
						if (chance.in100() && (num4 > 0 || num5 > 0 || j < num3 - 1))
						{
							SB.Append('-');
						}
					}
					for (int k = 0; k < num4; k++)
					{
						SB.Append(Infixes.GetRandomNameElement());
						if (chance.in100() && (num5 > 0 || k < num4 - 1))
						{
							SB.Append('-');
						}
					}
					for (int l = 0; l < num5; l++)
					{
						SB.Append(Postfixes.GetRandomNameElement());
						if (chance.in100() && l < num5 - 1)
						{
							SB.Append('-');
						}
					}
					if (i < num2 - 1)
					{
						SB.Append(' ');
					}
				}
				text = SB.ToString().Trim();
			}
			string randomNameElement = Templates.GetRandomNameElement();
			if (!randomNameElement.IsNullOrEmpty())
			{
				string text2 = randomNameElement;
				if (text2.Contains(";"))
				{
					text2 = text2.Split(';').GetRandomElement();
				}
				Dictionary<string, string> dictionary = new Dictionary<string, string>(8);
				if (NamingContext != null)
				{
					foreach (KeyValuePair<string, string> item in NamingContext)
					{
						dictionary[item.Key] = item.Value.Split(',').GetRandomElement();
					}
				}
				if (!dictionary.ContainsKey("*Name*"))
				{
					dictionary["*Name*"] = text;
				}
				if (text2.Contains("*AltName*") && !dictionary.ContainsKey("*AltName*"))
				{
					dictionary["*AltName*"] = NameStyles.Generate(For, Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, null, null, NamingContext, FailureOkay: false, SpecialFaildown: false, Skip, SkipList, HyphenationChance ?? (HyphenationChanceSet ? new int?(this.HyphenationChance) : ((int?)null)), TwoNameChance ?? (TwoNameChanceSet ? new int?(this.TwoNameChance) : ((int?)null)), HasHonorific, HasEpithet, ForProcessed: true);
				}
				if (For != null)
				{
					if (text2.Contains("*CreatureType*") && !dictionary.ContainsKey("*CreatureType*"))
					{
						dictionary["*CreatureType*"] = For.GetCreatureType();
					}
					if (text2.Contains("*CreatureTypeCap*") && !dictionary.ContainsKey("*CreatureTypeCap*"))
					{
						dictionary["*CreatureTypeCap*"] = For.GetCreatureType(Capitalized: true);
					}
					foreach (Match item2 in Regex.Matches(text2, "(\\*([^*]+)\\*)"))
					{
						if (dictionary.ContainsKey(item2.Groups[1].Value))
						{
							continue;
						}
						string text3 = For.GetPropertyOrTag("HeroNameTitle" + item2.Groups[2].Value);
						if (text3 != null)
						{
							if (text3.Contains(","))
							{
								text3 = text3.Split(',').GetRandomElement();
							}
							dictionary[item2.Groups[1].Value] = text3;
							if (text3.Contains("spice."))
							{
								dictionary[item2.Groups[1].Value] = Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<" + text3 + ">"));
							}
						}
					}
				}
				if (TemplateVars != null && TemplateVars.Count > 0)
				{
					foreach (KeyValuePair<string, List<NameValue>> templateVar in TemplateVars)
					{
						if (!text2.Contains(templateVar.Key))
						{
							continue;
						}
						string key = "*" + templateVar.Key + "*";
						if (!dictionary.ContainsKey(key))
						{
							dictionary[key] = templateVar.Value.GetRandomNameElement();
							if (dictionary[key].Contains("spice."))
							{
								dictionary[key] = Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<" + dictionary[key] + ">"));
							}
						}
					}
				}
				foreach (KeyValuePair<string, List<NameValue>> defaultTemplateVar in NameStyles.DefaultTemplateVars)
				{
					if (text2.Contains(defaultTemplateVar.Key))
					{
						string key2 = "*" + defaultTemplateVar.Key + "*";
						if (!dictionary.ContainsKey(key2))
						{
							dictionary[key2] = defaultTemplateVar.Value.GetRandomNameElement();
						}
					}
				}
				text = HistoricStringExpander.ExpandString(text2, null, null, dictionary);
				if (text.Contains("*"))
				{
					text = Regex.Replace(text, " *\\*[^*]+\\*", "");
				}
				text = text.Trim();
			}
			if (!Grammar.ContainsBadWords(text))
			{
				break;
			}
			if (++num > 1000)
			{
				int num6 = ++NameGenerationBadWordsFailures;
				return "NameGenBadWordsFail" + num6;
			}
		}
		return ApplyFormats(text);
	}

	public string ApplyFormats(string Text)
	{
		return ApplyFormats(Format, Text);
	}

	public static string ApplyFormats(string Format, string Text)
	{
		if (Format.Contains(","))
		{
			foreach (string item in Format.CachedCommaExpansion())
			{
				Text = ApplyFormat(item, Text);
			}
			return Text;
		}
		return ApplyFormat(Format, Text);
	}

	public static string ApplyFormat(string Format, string Text)
	{
		switch (Format)
		{
		case "TitleCase":
			Text = Grammar.MakeTitleCase(Text);
			break;
		case "AllCaps":
			Text = ColorUtility.ToUpperExceptFormatting(Text);
			break;
		case "LowerCase":
			Text = ColorUtility.ToLowerExceptFormatting(Text);
			break;
		case "Capitalized":
			Text = ColorUtility.CapitalizeExceptFormatting(Text);
			break;
		case "SpacesToHyphens":
			Text = ColorUtility.ReplaceExceptFormatting(Text, ' ', '-');
			break;
		}
		return Text;
	}

	public NameScope CheckApply(string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Region = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null, string Type = null, bool HasHonorific = false, bool HasEpithet = false)
	{
		NameScope nameScope = null;
		foreach (NameScope scope in Scopes)
		{
			if (scope.ApplyTo(Genotype, Subtype, Species, Culture, Faction, Region, Gender, Mutations, Tag, Special, Type, HasHonorific, HasEpithet) && (nameScope == null || scope.Priority > nameScope.Priority))
			{
				nameScope = scope;
			}
		}
		return nameScope;
	}
}
