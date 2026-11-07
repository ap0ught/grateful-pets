using System;
using System.Collections.Generic;
using System.Reflection;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Collections;
using XRL.Language;
using XRL.Wish;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Text.Attributes;

namespace XRL.World.Text.Delegates;

[HasModSensitiveStaticCache]
[HasVariableReplacer]
[HasWishCommand]
public static class VariableReplacers
{
	private static readonly string DefaultPoeticFeatures = "shiny teeth,coarse hair,sunken eyes";

	private static readonly string DefaultActivities = "roaming around idly";

	private static readonly string DefaultVillageActivities = "sleeping in our homes";

	private static readonly string DefaultSacredThings = "the act of procreating";

	private static readonly string DefaultArableLands = "arable land";

	private static readonly string DefaultValuedOres = "precious metals";

	public static StringMap<ReplacerEntry> Map = new StringMap<ReplacerEntry>();

	public static StringMap<ReplacerEntry> PostMap = new StringMap<ReplacerEntry>();

	public static bool Initialized;

	[VariableObjectReplacer]
	public static string Subjective(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.It ?? Context.Pronouns.CapitalizedSubjective;
		}
		else
		{
			obj = Context.Target?.it;
			if (obj == null)
			{
				return Context.Pronouns.Subjective;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string Objective(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.Them ?? Context.Pronouns.CapitalizedObjective;
		}
		else
		{
			obj = Context.Target?.them;
			if (obj == null)
			{
				return Context.Pronouns.Objective;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer(new string[] { "possessive", "possessiveAdjective" })]
	public static string Possessive(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.Its ?? Context.Pronouns.CapitalizedPossessiveAdjective;
		}
		else
		{
			obj = Context.Target?.its;
			if (obj == null)
			{
				return Context.Pronouns.PossessiveAdjective;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string SubstantivePossessive(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.Theirs ?? Context.Pronouns.CapitalizedSubstantivePossessive;
		}
		else
		{
			obj = Context.Target?.theirs;
			if (obj == null)
			{
				return Context.Pronouns.SubstantivePossessive;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string Reflexive(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.Itself ?? Context.Pronouns.CapitalizedReflexive;
		}
		else
		{
			obj = Context.Target?.itself;
			if (obj == null)
			{
				return Context.Pronouns.Reflexive;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string PersonTerm(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.PersonTerm ?? Context.Pronouns.CapitalizedPersonTerm;
		}
		else
		{
			obj = Context.Target?.personTerm;
			if (obj == null)
			{
				return Context.Pronouns.PersonTerm;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string ImmaturePersonTerm(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.ImmaturePersonTerm ?? Context.Pronouns.CapitalizedImmaturePersonTerm;
		}
		else
		{
			obj = Context.Target?.immaturePersonTerm;
			if (obj == null)
			{
				return Context.Pronouns.ImmaturePersonTerm;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string FormalAddressTerm(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.FormalAddressTerm ?? Context.Pronouns.CapitalizedFormalAddressTerm;
		}
		else
		{
			obj = Context.Target?.formalAddressTerm;
			if (obj == null)
			{
				return Context.Pronouns.FormalAddressTerm;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string OffspringTerm(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.OffspringTerm ?? Context.Pronouns.CapitalizedOffspringTerm;
		}
		else
		{
			obj = Context.Target?.offspringTerm;
			if (obj == null)
			{
				return Context.Pronouns.OffspringTerm;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string SiblingTerm(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.SiblingTerm ?? Context.Pronouns.CapitalizedSiblingTerm;
		}
		else
		{
			obj = Context.Target?.siblingTerm;
			if (obj == null)
			{
				return Context.Pronouns.SiblingTerm;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string ParentTerm(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.ParentTerm ?? Context.Pronouns.CapitalizedParentTerm;
		}
		else
		{
			obj = Context.Target?.parentTerm;
			if (obj == null)
			{
				return Context.Pronouns.ParentTerm;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string IndicativeProximal(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.IndicativeProximal ?? Context.Pronouns.CapitalizedIndicativeProximal;
		}
		else
		{
			obj = Context.Target?.indicativeProximal;
			if (obj == null)
			{
				return Context.Pronouns.IndicativeProximal;
			}
		}
		return (string)obj;
	}

	[VariableObjectReplacer]
	public static string IndicativeDistal(DelegateContext Context)
	{
		object obj;
		if (Context.Capitalize)
		{
			obj = Context.Target?.IndicativeDistal ?? Context.Pronouns.CapitalizedIndicativeDistal;
		}
		else
		{
			obj = Context.Target?.indicativeDistal;
			if (obj == null)
			{
				return Context.Pronouns.IndicativeDistal;
			}
		}
		return (string)obj;
	}

	[VariableReplacer(new string[] { "stringgamestate", "state.string" }, Default = "")]
	public static string StringGameState(DelegateContext Context)
	{
		string text = Context.Default;
		if (Context.Parameters.Count > 0)
		{
			text = The.Game?.GetStringGameState(Context.Parameters[0], null) ?? text;
			if (Context.Parameters.Count > 1 && text.IsNullOrEmpty())
			{
				text = Context.Parameters[1];
			}
		}
		return text;
	}

	[VariableReplacer(new string[] { "intgamestate", "state.int" }, Default = "0")]
	public static string Int32GameState(DelegateContext Context)
	{
		string result = Context.Default;
		if (Context.Parameters.Count > 0 && The.Game != null)
		{
			int intGameState = The.Game.GetIntGameState(Context.Parameters[0]);
			result = ((Context.Parameters.Count > 1) ? NumeralTransform(Context.Parameters[1], intGameState) : intGameState.ToString());
		}
		return result;
	}

	[VariableReplacer(new string[] { "int64gamestate", "state.long" }, Default = "0")]
	public static string Int64GameState(DelegateContext Context)
	{
		string result = Context.Default;
		if (Context.Parameters.Count > 0 && The.Game != null)
		{
			long int64GameState = The.Game.GetInt64GameState(Context.Parameters[0], 0L);
			if (Context.Parameters.Count <= 1)
			{
				result = int64GameState.ToString();
			}
			else
			{
				result = ((Context.Parameters.Count > 1) ? NumeralTransform(Context.Parameters[1], int64GameState) : int64GameState.ToString());
			}
		}
		return result;
	}

	[VariableReplacer(new string[] { "booleangamestate", "state.bool" }, Default = "false")]
	public static string BooleanGameState(DelegateContext Context)
	{
		string result = Context.Default;
		if (Context.Parameters.Count > 0 && The.Game != null)
		{
			if (Context.Parameters.Count > 1)
			{
				string text = "true";
				string text2 = "false";
				bool result2 = false;
				if (Context.Parameters.Count > 3)
				{
					result2 = bool.TryParse(Context.Parameters[1], out result2);
					Context.Parameters.RemoveAt(1);
				}
				if (Context.Parameters.Count > 1)
				{
					text = Context.Parameters[1];
					text2 = ((Context.Parameters.Count > 2) ? Context.Parameters[2] : "");
				}
				result = (The.Game.GetBooleanGameState(Context.Parameters[0], result2) ? text : text2);
			}
			else
			{
				result = The.Game.GetBooleanGameState(Context.Parameters[0]).ToString();
			}
		}
		return result;
	}

	[VariableReplacer(new string[] { "MarkOfDeath" }, Default = "*MARK OF DEATH MISSING*")]
	public static string GameStateMarkOfDeath(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("MarkOfDeath") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "GR1" }, Default = "*GLOTROT CURE LIQUID 1 MISSING*")]
	public static string GameStateGlotrotCure1(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("GlotrotCure1") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "GR2" }, Default = "*GLOTROT CURE LIQUID 2 MISSING*")]
	public static string GameStateGlotrotCure2(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("GlotrotCure2") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "GR3" }, Default = "*GLOTROT CURE LIQUID 3 MISSING*")]
	public static string GameStateGlotrotCure3(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("GlotrotCure3") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "IS1" }, Default = "*IRONSHANK CURE MISSING*")]
	public static string GameStateIronshankCure(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("IronshankCure") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "MC1" }, Default = "*MONOCHROME CURE MISSING*")]
	public static string GameStateMonochromeCure(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("MonochromeCure") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "FCC" }, Default = "*FUNGAL CURE WORM MISSING*")]
	public static string GameStateFungalCureWorm(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("FungalCureWormDisplay") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "FCL" }, Default = "*FUNGAL CURE LIQUID MISSING*")]
	public static string GameStateFungalCureLiquid(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("FungalCureLiquidDisplay") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "EskhindRoadDirection" }, Default = "*ESKHIND ROAD DIRECTION MISSING*")]
	public static string GameStateEskhindRoadDirection(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("EskhindRoadDirection") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "SEEKERENEMY" }, Default = "*SEEKER ENEMY FACTION MISSING*")]
	public static string GameStateSeekerEnemy(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("SeekerEnemyFaction") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "villageZeroName" }, Default = "*VILLAGE ZERO NAME MISSING*")]
	public static string GameStateVillageZeroName(DelegateContext Context)
	{
		return The.Game?.GetStringGameState("villageZeroName") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "V0tinkeraddendum" }, Default = "*VILLAGE ZERO TINKER ADDENDUM MISSING*")]
	public static string GameStateVillageZeroTinkerAddendum(DelegateContext Context)
	{
		if (string.IsNullOrEmpty(The.Game?.GetStringGameState("villageZeroName")))
		{
			return " It's been live for over a year.";
		}
		return " A tinker must have detected it and then recorded it onto the data disk you brought us. However, we've known about since it went live over a year ago.";
	}

	[VariableReplacer(new string[] { "RebekahRegion" }, Default = "*REBEKAH REGION MISSING*")]
	public static string GameStateRebekahRegion(DelegateContext Context)
	{
		return HistoryAPI.GetResheph().GetEvent((HistoricEvent x) => x.HasEventProperty("rebekah") && x.HasEventProperty("region"), -1L)?.GetEventProperty("region") ?? Context.Default;
	}

	[VariableReplacer(new string[] { "MARKOVPARAGRAPH" })]
	public static string MarkovParagraph(DelegateContext Context)
	{
		return GameText.GenerateMarkovMessageParagraph();
	}

	[VariableReplacer(new string[] { "MARKOVSENTENCE" })]
	public static string MarkovSentence(DelegateContext Context)
	{
		return GameText.GenerateMarkovMessageSentence();
	}

	[VariableReplacer(new string[] { "MARKOVCORVIDSENTENCE" })]
	public static string MarkovCorvidSentence(DelegateContext Context)
	{
		return TextFilters.Corvid(GameText.GenerateMarkovMessageSentence());
	}

	[VariableReplacer(new string[] { "MARKOVWATERBIRDSENTENCE" })]
	public static string MarkovWaterBirdSentence(DelegateContext Context)
	{
		return TextFilters.WaterBird(GameText.GenerateMarkovMessageSentence());
	}

	[VariableReplacer(new string[] { "MARKOVFISHSENTENCE" })]
	public static string MarkovFishSentence(DelegateContext Context)
	{
		return TextFilters.Fish(GameText.GenerateMarkovMessageSentence());
	}

	[VariableReplacer(new string[] { "WEIRDMARKOVSENTENCE" })]
	public static string MarkovWeirdSentence(DelegateContext Context)
	{
		return Grammar.Weirdify(GameText.GenerateMarkovMessageSentence());
	}

	private static string NumeralTransform(string Command, int Value)
	{
		return Command switch
		{
			"cardinal" => Grammar.Cardinal(Value), 
			"ordinal" => Grammar.Ordinal(Value), 
			"roman" => Grammar.GetRomanNumeral(Value), 
			"multiplicative" => Grammar.Multiplicative(Value), 
			_ => Value.ToString(), 
		};
	}

	private static string NumeralTransform(string Command, long Value)
	{
		return Command switch
		{
			"cardinal" => Grammar.Cardinal(Value), 
			"ordinal" => Grammar.Ordinal(Value), 
			"roman" => Grammar.GetRomanNumeral(Value), 
			"multiplicative" => Grammar.Multiplicative(Value), 
			_ => Value.ToString(), 
		};
	}

	[VariableReplacer]
	public static string Year(DelegateContext Context)
	{
		int year = Calendar.GetYear();
		if (Context.Parameters.Contains("noera"))
		{
			return year.ToString();
		}
		int Index = 0;
		Span<char> Text = stackalloc char[year.CountDigits() + 3];
		Text.Insert(ref Index, year);
		Text[Index++] = ' ';
		Text[Index++] = 'A';
		Text[Index] = 'R';
		return new string(Text);
	}

	[VariableReplacer]
	public static string Month(DelegateContext Context)
	{
		return Calendar.GetMonth();
	}

	[VariableReplacer]
	public static string Day(DelegateContext Context)
	{
		return Calendar.GetDay();
	}

	[VariableReplacer]
	public static string Time(DelegateContext Context)
	{
		return Calendar.GetTime();
	}

	private static string GetFragment(GameObject Target, string Key, string Default)
	{
		if (Target != null && Target.GetBlueprint().xTags.TryGetValue("TextFragments", out var value) && value.TryGetValue(Key, out var value2) && !value2.IsNullOrEmpty())
		{
			return value2.GetRandomSubstring(',');
		}
		return Default.GetRandomSubstring(',');
	}

	[VariableObjectReplacer(new string[] { "fragment.poetic" }, Capitalization = false)]
	public static string FragmentPoetic(DelegateContext Context)
	{
		return GetFragment(Context.Target, "PoeticFeatures", DefaultPoeticFeatures);
	}

	[VariableObjectReplacer(new string[] { "fragment.activity" }, Capitalization = false)]
	public static string FragmentActivity(DelegateContext Context)
	{
		return GetFragment(Context.Target, "Activity", DefaultActivities);
	}

	[VariableObjectReplacer(new string[] { "fragment.village" }, Capitalization = false)]
	public static string FragmentVillage(DelegateContext Context)
	{
		return GetFragment(Context.Target, "VillageActivity", DefaultVillageActivities);
	}

	[VariableObjectReplacer(new string[] { "fragment.sacred" }, Capitalization = false)]
	public static string FragmentSacred(DelegateContext Context)
	{
		return GetFragment(Context.Target, "SacredThing", DefaultSacredThings);
	}

	[VariableObjectReplacer(new string[] { "fragment.arable" }, Capitalization = false)]
	public static string FragmentArable(DelegateContext Context)
	{
		return GetFragment(Context.Target, "ArableLand", DefaultArableLands);
	}

	[VariableObjectReplacer(new string[] { "fragment.ore" }, Capitalization = false)]
	public static string FragmentOre(DelegateContext Context)
	{
		return GetFragment(Context.Target, "ValuedOre", DefaultValuedOres);
	}

	[VariableObjectReplacer(new string[] { "fragment" }, Capitalization = false)]
	public static string Fragment(DelegateContext Context)
	{
		string text = DefaultSacredThings;
		if (Context.Parameters.Count > 0)
		{
			if (Context.Parameters.Count > 1)
			{
				text = Context.Parameters[1];
			}
			text = GetFragment(Context.Target, Context.Parameters[0], text);
		}
		return text;
	}

	[VariableReplacer(new string[] { "EitherOrWhisper" })]
	public static string EitherOrWhisper(DelegateContext Context)
	{
		return PetEitherOr.GetEitherOrAccomplishment();
	}

	[VariableReplacer(new string[] { "sultan" })]
	public static string SultanName(DelegateContext Context)
	{
		if (Context.Parameters.Count == 0 || !int.TryParse(Context.Parameters[0], out var result))
		{
			return "a sultan";
		}
		return HistoryAPI.GetSultanForPeriod(result)?.GetProperty("name") ?? ("the " + Grammar.Ordinal(result) + " sultan");
	}

	[VariableReplacer(Capitalization = true)]
	public static string SultanTerm(DelegateContext Context)
	{
		string text = HistoryAPI.GetSultanTerm();
		if (Context.Capitalize)
		{
			text = Grammar.InitCap(text);
		}
		return text;
	}

	[VariableReplacer(new string[] { "generic" })]
	public static string SimpleConversation(DelegateContext Context)
	{
		string text = Context.Target?.GetPropertyOrTag("SimpleConversation");
		if (text != null)
		{
			return text.GetRandomSubstring('~');
		}
		return "";
	}

	[VariableObjectReplacer(Default = "do", Capitalization = false)]
	public static string Verb(DelegateContext Context)
	{
		string text = Context.Default;
		bool flag = Context.Parameters.Count >= 2 && Context.Parameters[1] == "afterpronoun";
		if (Context.Parameters.Count >= 1)
		{
			text = Context.Parameters[0];
		}
		if (Context.Target != null)
		{
			return Context.Target.GetVerb(text, PrependSpace: false, flag);
		}
		if (Context.Explicit == null)
		{
			return text;
		}
		if (Context.Pronouns != null)
		{
			if (Context.Pronouns.Plural)
			{
				return text;
			}
			if (Context.Pronouns.PseudoPlural & flag)
			{
				return text;
			}
		}
		return Grammar.ThirdPerson(text);
	}

	[VariableObjectReplacer(Default = "the thing does")]
	public static string Does(DelegateContext Context)
	{
		string text = ((Context.Parameters.Count >= 1) ? Context.Parameters[0] : "do");
		if (Context.Target != null)
		{
			if (!Context.Capitalize)
			{
				return Context.Target.does(text, int.MaxValue, null, null, null, AsIfKnown: false, Single: true);
			}
			return Context.Target.Does(text, int.MaxValue, null, null, null, AsIfKnown: false, Single: true);
		}
		if (Context.Explicit != null)
		{
			return Context.Value.Append(Context.Capitalize ? "The " : "the ").Append(Context.Explicit).Compound(Grammar.ThirdPerson(text))
				.ToString();
		}
		return Context.Default;
	}

	[VariableObjectReplacer(Default = "creature")]
	public static string Species(DelegateContext Context)
	{
		string text = Context.Default;
		if (Context.Target != null)
		{
			text = Context.Target.GetSpecies();
			if (Context.Capitalize)
			{
				text.Capitalize();
			}
		}
		return text;
	}

	[VariableObjectReplacer(Default = "creature")]
	public static string ApparentSpecies(DelegateContext Context)
	{
		string text = Context.Default;
		if (Context.Target != null)
		{
			text = Context.Target.GetApparentSpecies();
			if (Context.Capitalize)
			{
				text.Capitalize();
			}
		}
		return text;
	}

	[VariableObjectReplacer(new string[] { "t" }, Default = "the thing")]
	public static string WithDefiniteArticle(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "you";
				}
				return "You";
			}
			return Context.Target.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true, null, IndicateHidden: false, Context.Capitalize);
		}
		if (Context.Explicit != null)
		{
			return (Context.Capitalize ? "The " : "the ") + Context.Explicit;
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "t's" }, Default = "the thing's")]
	public static string PossessiveWithDefiniteArticle(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "your";
				}
				return "Your";
			}
			return Grammar.MakePossessive(Context.Target.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true, null, IndicateHidden: false, Context.Capitalize));
		}
		if (Context.Explicit != null)
		{
			return (Context.Capitalize ? "The " : "the ") + Grammar.MakePossessive(Context.Explicit);
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "faction" }, Default = "faction")]
	public static string FactionName(DelegateContext Context)
	{
		string text = Factions.GetIfExists(Context.Target?.GetPrimaryFaction())?.DisplayName;
		if (!text.IsNullOrEmpty())
		{
			return text;
		}
		if (Context.Parameters.Count > 0)
		{
			return Context.Parameters[0];
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "faction.t" }, Default = "the faction")]
	public static string TheFactionName(DelegateContext Context)
	{
		string text = Factions.GetIfExists(Context.Target?.GetPrimaryFaction())?.GetFormattedName();
		if (!text.IsNullOrEmpty())
		{
			return text;
		}
		if (Context.Parameters.Count > 0)
		{
			return Context.Parameters[0];
		}
		return Context.Default;
	}

	[VariableReplacer(new string[] { "mostHatedFaction" })]
	public static string MostHatedFactionName(DelegateContext Context)
	{
		return Factions.GetMostHated().DisplayName;
	}

	[VariableReplacer(new string[] { "mostHatedFaction.t" })]
	public static string TheMostHatedFactionName(DelegateContext Context)
	{
		return Factions.GetMostHated().GetFormattedName();
	}

	[VariableReplacer(new string[] { "secondMostHatedFaction" })]
	public static string MostSecondHatedFactionName(DelegateContext Context)
	{
		return Factions.GetSecondMostHated().DisplayName;
	}

	[VariableReplacer(new string[] { "secondMostHatedFaction.t" })]
	public static string TheSecondMostHatedFactionName(DelegateContext Context)
	{
		return Factions.GetSecondMostHated().GetFormattedName();
	}

	[VariableObjectReplacer(Default = "water")]
	public static string WaterRitualLiquid(DelegateContext Context)
	{
		string text = Context.Default;
		if (Context.Target != null)
		{
			text = Context.Target.GetWaterRitualLiquidName();
			if (Context.Capitalize)
			{
				text = text.Capitalize();
			}
		}
		return ColorUtility.StripFormatting(text);
	}

	[VariableObjectReplacer(Default = "somewhere")]
	public static string Direction(DelegateContext Context)
	{
		string text = Context.Default;
		if (The.Player != null && Context.Target != null)
		{
			text = The.Player.DescribeDirectionToward(Context.Target);
			if (Context.Capitalize)
			{
				text = text.Capitalize();
			}
		}
		return text;
	}

	[VariableObjectReplacer]
	public static string DirectionIfAny(DelegateContext Context)
	{
		if (Context.Target != null && The.Player != null)
		{
			string text = The.Player.DescribeDirectionToward(Context.Target);
			if (text.IsNullOrEmpty() || text == "here")
			{
				return "";
			}
			if (Context.Capitalize)
			{
				text = text.Capitalize();
			}
			return " " + text;
		}
		return "";
	}

	[VariableObjectReplacer(new string[] { "an" }, Default = "a thing")]
	public static string WithIndefiniteArticle(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "you";
				}
				return "You";
			}
			return Context.Target.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true, WithDefiniteArticle: false, null, IndicateHidden: false, Context.Capitalize);
		}
		if (Context.Explicit == null)
		{
			return Context.Default;
		}
		return Grammar.A(Context.Explicit, Context.Capitalize);
	}

	[VariableObjectReplacer(new string[] { "an's" }, Default = "a thing's")]
	public static string PossessiveWithIndefiniteArticle(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "your";
				}
				return "Your";
			}
			return Grammar.MakePossessive(Context.Target.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true, WithDefiniteArticle: false, null, IndicateHidden: false, Context.Capitalize));
		}
		if (Context.Explicit == null)
		{
			return Context.Default;
		}
		return Grammar.MakePossessive(Grammar.A(Context.Explicit, Context.Capitalize));
	}

	[VariableObjectReplacer(new string[] { "the" }, Default = "the ")]
	public static string DefiniteArticle(DelegateContext Context)
	{
		if (Context.Target == null)
		{
			return Context.Default;
		}
		return Context.Target.DefiniteArticle(Context.Capitalize);
	}

	[VariableObjectReplacer(new string[] { "a" }, Default = "a ")]
	public static string IndefiniteArticle(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			return Context.Target.IndefiniteArticle(Context.Capitalize);
		}
		if (Grammar.IndefiniteArticleShouldBeAn(Context.Explicit))
		{
			if (!Context.Capitalize)
			{
				return "an ";
			}
			return "An ";
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "name" }, Default = "thing")]
	public static string TargetName(DelegateContext Context)
	{
		if (Context.Type == TargetType.None)
		{
			return The.Game?.PlayerName ?? "";
		}
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "you";
				}
				return "You";
			}
			return Context.Target.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Context.Capitalize);
		}
		if (Context.Explicit != null)
		{
			if (!Context.Capitalize)
			{
				return Context.Explicit;
			}
			return Context.Explicit.Capitalize();
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "name's" }, Default = "thing's")]
	public static string TargetNamePossessive(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "your";
				}
				return "Your";
			}
			return Grammar.MakePossessive(Context.Target.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Context.Capitalize));
		}
		if (Context.Explicit != null)
		{
			return Grammar.MakePossessive(Context.Capitalize ? Context.Explicit.Capitalize() : Context.Explicit);
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "nameSingle" }, Default = "thing")]
	public static string TargetNameSingle(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "you";
				}
				return "You";
			}
			return Context.Target.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Context.Capitalize);
		}
		if (Context.Explicit != null)
		{
			if (!Context.Capitalize)
			{
				return Context.Explicit;
			}
			return Context.Explicit.Capitalize();
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "longname" }, Default = "thing")]
	public static string TargetNameLong(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "you";
				}
				return "You";
			}
			return Context.Target.GetDisplayName();
		}
		if (Context.Explicit != null)
		{
			if (!Context.Capitalize)
			{
				return Context.Explicit;
			}
			return Context.Explicit.Capitalize();
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "refname" }, Default = "thing")]
	public static string TargetNameReference(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "you";
				}
				return "You";
			}
			return Context.Target.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: true, Short: true);
		}
		if (Context.Explicit != null)
		{
			if (!Context.Capitalize)
			{
				return Context.Explicit;
			}
			return Context.Explicit.Capitalize();
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "refname's" }, Default = "thing's")]
	public static string TargetNameReferencePossessive(DelegateContext Context)
	{
		if (Context.Target != null)
		{
			if (Context.Target.IsPlayer() && Grammar.AllowSecondPerson)
			{
				if (!Context.Capitalize)
				{
					return "your";
				}
				return "Your";
			}
			return Grammar.MakePossessive(Context.Target.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: true, Short: true));
		}
		if (Context.Explicit != null)
		{
			return Grammar.MakePossessive(Context.Capitalize ? Context.Explicit.Capitalize() : Context.Explicit);
		}
		return Context.Default;
	}

	[VariableObjectReplacer(new string[] { "bodypart" }, Capitalization = false)]
	public static string BodyPartOrdinalName(DelegateContext Context)
	{
		BodyPart bodyPart = Context.Target?.Body?.GetBody();
		if (bodyPart != null)
		{
			List<BodyPart> list = new List<BodyPart>();
			int i = 0;
			for (int count = Context.Parameters.Count; i < count; i++)
			{
				list.Clear();
				bodyPart.GetPart(Context.Parameters[i], list);
				list.ShuffleInPlace();
				int j = 0;
				for (int count2 = list.Count; j < count2; j++)
				{
					BodyPart bodyPart2 = list[i];
					if (!bodyPart2.Abstract)
					{
						return bodyPart2.GetOrdinalName();
					}
				}
			}
			return bodyPart.GetOrdinalName();
		}
		return "body";
	}

	[VariableObjectReplacer(new string[] { "bodypart.shuffled" }, Capitalization = false)]
	public static string BodyPartOrdinalNameShuffled(DelegateContext Context)
	{
		Context.Parameters.ShuffleInPlace();
		return BodyPartOrdinalName(Context);
	}

	[VariableObjectReplacer(Default = "somewhere")]
	public static string GeneralDirection(DelegateContext Context)
	{
		string text = The.Player?.DescribeDirectionToward(Context.Target, General: true);
		if (text != null)
		{
			if (!Context.Capitalize)
			{
				return text;
			}
			return text.Capitalize();
		}
		return Context.Default;
	}

	[VariableObjectReplacer]
	public static string GeneralDirectionIfAny(DelegateContext Context)
	{
		string text = The.Player?.DescribeDirectionToward(Context.Target, General: true);
		if (!text.IsNullOrEmpty() && text != "here")
		{
			if (!Context.Capitalize)
			{
				return text;
			}
			return text.Capitalize();
		}
		return "here";
	}

	[VariableObjectReplacer(new string[] { "isplural" }, Capitalization = false)]
	public static string TernaryPlural(DelegateContext Context)
	{
		string result = "";
		if (Context.Parameters.Count > 0)
		{
			if (Context.Target?.IsPlural ?? Context.Pronouns.Plural)
			{
				result = Context.Parameters[0];
			}
			else if (Context.Parameters.Count > 1)
			{
				result = Context.Parameters[1];
			}
		}
		return result;
	}

	[VariableReplacer(new string[] { "ifplayerplural" })]
	public static string PlayerPlural(DelegateContext Context)
	{
		Context.Target = The.Player;
		return TernaryPlural(Context);
	}

	[VariableReplacer(new string[] { "factionaddress" })]
	public static string FactionAddress(DelegateContext Context)
	{
		string text = Context.Default;
		if (Context.Parameters.Count > 0)
		{
			string text2 = Context.Parameters[0];
			bool flag = false;
			bool flag2 = false;
			text = The.Game?.PlayerReputation.GetFactionRank(text2);
			if (Context.Parameters.Count > 1)
			{
				flag = Context.Parameters.Remove("singular");
				flag2 = Context.Parameters.Remove("formal");
				if (Context.Parameters.Count > 1 && text.IsNullOrEmpty())
				{
					text = Context.Parameters[1];
				}
			}
			if (text.IsNullOrEmpty())
			{
				text = Faction.GetDefaultAddress(text2);
			}
			if (text.IsNullOrEmpty())
			{
				text = (flag2 ? The.Player.formalAddressTerm : The.Player.personTerm);
			}
			if (!flag && The.Player.IsPlural)
			{
				text = text.Pluralize();
			}
		}
		return text;
	}

	[VariableReplacer(new string[] { "factionrank" })]
	public static string FactionRank(DelegateContext Context)
	{
		string text = Context.Default;
		if (Context.Parameters.Count > 0)
		{
			string faction = Context.Parameters[0];
			bool flag = false;
			text = The.Game?.PlayerReputation.GetFactionRank(faction) ?? "";
			if (Context.Parameters.Count > 1)
			{
				flag = Context.Parameters.Remove("singular");
				if (Context.Parameters.Count > 1 && text.IsNullOrEmpty())
				{
					text = Context.Parameters[1];
				}
			}
			if (!flag && The.Player.IsPlural)
			{
				text = text.Pluralize();
			}
		}
		return text;
	}

	[VariableObjectReplacer(new string[] { "terrain.t" }, Capitalization = false)]
	public static string TerrainDisplayName(DelegateContext Context)
	{
		return ((Context.Type == TargetType.None) ? The.ActiveZone : Context.Target?.CurrentZone)?.GetTerrainDisplayName() ?? "the unknown";
	}

	[VariableObjectReplacer(new string[] { "landmark.nearest" }, Capitalization = false)]
	public static string NearestLandmark(DelegateContext Context)
	{
		return JournalAPI.GetLandmarkNearest((Context.Type == TargetType.None) ? The.ActiveZone : Context.Target.CurrentZone).Text;
	}

	[VariablePostProcessor]
	public static void Capitalize(DelegateContext Context)
	{
		if (Context.Value.Length > 0)
		{
			char c = Context.Value[0];
			if (char.IsLetter(c))
			{
				Context.Value[0] = char.ToUpperInvariant(Context.Value[0]);
			}
			else if (c == '{')
			{
				int index = Context.Value.IndexOf('|') + 1;
				Context.Value[index] = char.ToUpperInvariant(Context.Value[index]);
			}
		}
	}

	[VariablePostProcessor]
	public static void Lower(DelegateContext Context)
	{
		int i = 0;
		for (int length = Context.Value.Length; i < length; i++)
		{
			Context.Value[i] = char.ToLowerInvariant(Context.Value[i]);
		}
	}

	[VariablePostProcessor]
	public static void Upper(DelegateContext Context)
	{
		int i = 0;
		for (int length = Context.Value.Length; i < length; i++)
		{
			Context.Value[i] = char.ToUpperInvariant(Context.Value[i]);
		}
	}

	[VariablePostProcessor]
	public static void Strip(DelegateContext Context)
	{
		ColorUtility.StripFormatting(Context.Value);
	}

	[VariablePostProcessor]
	public static void Pluralize(DelegateContext Context)
	{
		string word = Context.Value.ToString();
		word = Grammar.Pluralize(word);
		Context.Value.Clear();
		Context.Value.Append(word);
	}

	[VariablePostProcessor(new string[] { "playerpluralize" })]
	public static void PluralizeForPlayer(DelegateContext Context)
	{
		if (The.Player != null && The.Player.IsPlural)
		{
			Pluralize(Context);
		}
	}

	[VariablePostProcessor(Capitalization = true)]
	public static void Article(DelegateContext Context)
	{
		string word = Context.Value.ToString();
		Context.Value.Clear();
		Grammar.A(word, Context.Value, Context.Capitalize);
	}

	[VariablePostProcessor]
	public static void Title(DelegateContext Context)
	{
		string phrase = Context.Value.ToString();
		phrase = Grammar.MakeTitleCase(phrase);
		Context.Value.Clear();
		Context.Value.Append(phrase);
	}

	[ModSensitiveCacheInit]
	public static void Reset()
	{
		Map.Clear();
		PostMap.Clear();
		Initialized = false;
	}

	public static void LoadReplacers()
	{
		List<string> list = new List<string>();
		List<ReplacerEntry> list2 = new List<ReplacerEntry>();
		foreach (MethodInfo item in ModManager.GetMethodsWithAttribute(typeof(VariableReplacerAttribute), typeof(HasVariableReplacerAttribute), Cache: false))
		{
			try
			{
				list.Clear();
				VariableReplacerAttribute customAttribute = item.GetCustomAttribute<VariableReplacerAttribute>();
				StringMap<ReplacerEntry> stringMap = ((customAttribute is VariablePostProcessorAttribute) ? PostMap : Map);
				if (customAttribute.Keys.IsNullOrEmpty())
				{
					list.Add(Grammar.InitLower(item.Name));
				}
				else
				{
					list.AddRange(customAttribute.Keys);
				}
				list2.Clear();
				Replacer replacer;
				if (item.ReturnType == typeof(void))
				{
					Action<DelegateContext> action = (Action<DelegateContext>)item.CreateDelegate(typeof(Action<DelegateContext>));
					replacer = (DelegateContext x) =>
					{
						action(x);
						return (string)null;
					};
				}
				else
				{
					replacer = (Replacer)item.CreateDelegate(typeof(Replacer));
				}
				list2.Add(new ReplacerEntry(replacer, customAttribute.Capitalization ? Grammar.InitLower(customAttribute.Default) : customAttribute.Default, customAttribute.Flags));
				if (customAttribute.Capitalization)
				{
					list2.Add(new ReplacerEntry(replacer, customAttribute.Capitalization ? Grammar.InitCap(customAttribute.Default) : customAttribute.Default, customAttribute.Flags, Capitalize: true));
				}
				int num = 0;
				for (int count = list2.Count; num < count; num++)
				{
					ReplacerEntry value = list2[num];
					bool flag = num % 2 != 0;
					int num2 = 0;
					for (int count2 = list.Count; num2 < count2; num2++)
					{
						string key = list[num2];
						if (customAttribute.Capitalization)
						{
							key = (flag ? Grammar.InitCap(list[num2]) : Grammar.InitLower(list[num2]));
						}
						if (customAttribute.Override)
						{
							stringMap[key] = value;
						}
						else
						{
							stringMap.TryAdd(key, value);
						}
					}
				}
			}
			catch (Exception message)
			{
				MetricsManager.LogAssemblyError(item, message);
			}
		}
		Initialized = true;
	}
}
