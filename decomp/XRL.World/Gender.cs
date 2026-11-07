using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Language;
using XRL.Names;
using XRL.UI;

namespace XRL.World;

[HasModSensitiveStaticCache]
public class Gender : BasePronounProvider
{
	public static bool EnableSelection;

	public static bool EnableGeneration;

	public static bool EnableDisplay;

	public static Dictionary<string, Gender> Genders;

	[NonSerialized]
	public static int PronounAltVowelChance;

	[NonSerialized]
	public static int PronounStemVowelChance;

	[NonSerialized]
	public static Dictionary<string, int> PronounStems;

	[NonSerialized]
	public static Dictionary<string, int> PronounVowels;

	[NonSerialized]
	public static Dictionary<string, int> PronounPrefixes;

	[NonSerialized]
	public static Dictionary<string, int> PronounSubjectiveSuffixes;

	[NonSerialized]
	public static Dictionary<string, int> PronounObjectiveSuffixes;

	[NonSerialized]
	public static Dictionary<string, int> PronounPossessiveSuffixes;

	[NonSerialized]
	public static List<string> PronounCullStrings;

	[NonSerialized]
	public static List<string> PronounCullPatterns;

	[NonSerialized]
	public static Dictionary<string, string> PronounMapStrings;

	[NonSerialized]
	public static Dictionary<string, string> PronounMapPatterns;

	[NonSerialized]
	public static List<string> NamePrefixes;

	[NonSerialized]
	public static List<string> NameInfixes;

	[NonSerialized]
	public static List<string> NameSuffixes;

	[NonSerialized]
	public static List<Gender> All;

	[NonSerialized]
	public static List<Gender> GenericPlurals;

	[NonSerialized]
	public static List<Gender> Generics;

	[NonSerialized]
	public static List<Gender> GenericSingulars;

	[NonSerialized]
	public static List<Gender> Plurals;

	[NonSerialized]
	public static List<Gender> Singulars;

	[NonSerialized]
	public static List<Gender> GenericPersonalPlurals;

	[NonSerialized]
	public static List<Gender> GenericPersonals;

	[NonSerialized]
	public static List<Gender> GenericPersonalSingulars;

	[NonSerialized]
	public static List<Gender> Personals;

	[NonSerialized]
	public static List<Gender> PersonalPlurals;

	[NonSerialized]
	public static List<Gender> PersonalSingulars;

	[NonSerialized]
	public static List<Gender> GenericNonpersonalPlurals;

	[NonSerialized]
	public static List<Gender> GenericNonpersonals;

	[NonSerialized]
	public static List<Gender> GenericNonpersonalSingulars;

	[NonSerialized]
	public static List<Gender> Nonpersonals;

	[NonSerialized]
	public static List<Gender> NonpersonalPlurals;

	[NonSerialized]
	public static List<Gender> NonpersonalSingulars;

	[NonSerialized]
	public static Gender DefaultNeuter;

	[NonSerialized]
	public static Gender DefaultNonspecific;

	[NonSerialized]
	public static Gender DefaultPlural;

	[NonSerialized]
	public static Gender DefaultCollective;

	public bool DoNotReplicateAsPronounSet;

	public string _Name = string.Empty;

	[NonSerialized]
	public string _CapitalizedName;

	public override string Name => _Name;

	public override string CapitalizedName => _CapitalizedName ?? (_CapitalizedName = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Name));

	public Gender(string __Name = null, bool _Generic = false, bool _Generated = false, bool _Plural = false, bool _PseudoPlural = false, string _Subjective = null, string _Objective = null, string _PossessiveAdjective = null, string _SubstantivePossessive = null, string _Reflexive = null, string _PersonTerm = null, string _ImmaturePersonTerm = null, string _FormalAddressTerm = null, string _OffspringTerm = null, string _SiblingTerm = null, string _ParentTerm = null, bool _UseBareIndicative = false, bool _DoNotReplicateAsPronounSet = false)
		: base(_Generic, _Generated, _Plural, _PseudoPlural, _Subjective, _Objective, _PossessiveAdjective, _SubstantivePossessive, _Reflexive, _PersonTerm, _ImmaturePersonTerm, _FormalAddressTerm, _OffspringTerm, _SiblingTerm, _ParentTerm, _UseBareIndicative)
	{
		_Name = __Name;
		DoNotReplicateAsPronounSet = _DoNotReplicateAsPronounSet;
	}

	public Gender(BasePronounProvider Original)
		: base(Original)
	{
	}

	/// <summary>Empty Constructor for deserialization purpose, you probably want to pass something.</summary>
	public Gender()
	{
	}

	public override void CopyFrom(BasePronounProvider Original)
	{
		if (Original is Gender gender)
		{
			_Name = gender.Name;
			DoNotReplicateAsPronounSet = gender.DoNotReplicateAsPronounSet;
		}
		base.CopyFrom(Original);
	}

	public override BasePronounProvider Clone()
	{
		return new Gender(this);
	}

	public static void SaveAll(SerializationWriter Writer)
	{
		Writer.Write(Genders.Count);
		foreach (string key in Genders.Keys)
		{
			Genders[key].Save(Writer);
		}
	}

	public static void LoadAll(SerializationReader Reader)
	{
		if (Genders == null)
		{
			Genders = new Dictionary<string, Gender>(32);
		}
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Gender gender = Load(Reader);
			Genders[gender.Name] = gender;
		}
		SetDefaults();
	}

	public override void Save(SerializationWriter Writer)
	{
		base.Save(Writer);
		Writer.Write(_Name);
		Writer.Write(DoNotReplicateAsPronounSet);
	}

	public static Gender Load(SerializationReader Reader)
	{
		Gender gender = new Gender();
		BasePronounProvider.Load(Reader, gender);
		gender._Name = Reader.ReadString();
		gender.DoNotReplicateAsPronounSet = Reader.ReadBoolean();
		return gender;
	}

	private static void SetDefaults()
	{
		if (!Genders.TryGetValue("neuter", out DefaultNeuter))
		{
			MetricsManager.LogError("Default gender 'neuter' not found.");
		}
		if (!Genders.TryGetValue("nonspecific", out DefaultNonspecific))
		{
			MetricsManager.LogError("Default gender 'nonspecific' not found.");
		}
		if (!Genders.TryGetValue("plural", out DefaultPlural))
		{
			MetricsManager.LogError("Default gender 'plural' not found.");
		}
		if (!Genders.TryGetValue("collective", out DefaultNonspecific))
		{
			MetricsManager.LogError("Default gender 'collective' not found.");
		}
	}

	public override async Task<bool> CustomizeAsync()
	{
		return await Customize("gender");
	}

	protected override async Task<bool> CustomizeProcess(string What)
	{
		while (true)
		{
			_Name = await Popup.AskStringAsync("What name should be used for your " + What + "? (Male, female, etc.)", Name, 12, 0, "abcdefghijklmnopqrstuvwxyz ");
			_Name = _Name.Trim(' ');
			while (_Name.Contains("  "))
			{
				_Name = _Name.Replace("  ", " ");
			}
			if (_Name.IsNullOrEmpty())
			{
				return false;
			}
			if (!Genders.ContainsKey(_Name))
			{
				break;
			}
			await Popup.ShowAsync("That name is already in use.");
		}
		DoNotReplicateAsPronounSet = false;
		return await base.CustomizeProcess(What);
	}

	public StringBuilder GetSummary(StringBuilder sb)
	{
		return sb.Append("Name: ").Append(Name).Append('\n')
			.Append("Generic: ")
			.Append(Generic ? "Yes" : "No")
			.Append('\n')
			.Append("Generated: ")
			.Append(Generated ? "Yes" : "No")
			.Append('\n')
			.Append("Plural: ")
			.Append(Plural ? "Yes" : "No")
			.Append('\n')
			.Append("Pseudo-Plural: ")
			.Append(PseudoPlural ? "Yes" : "No")
			.Append('\n')
			.Append("Subjective Pronoun: ")
			.Append(Subjective)
			.Append('\n')
			.Append("Objective Pronoun: ")
			.Append(Objective)
			.Append('\n')
			.Append("Possessive Adjective: ")
			.Append(PossessiveAdjective)
			.Append('\n')
			.Append("Substantive Possessive: ")
			.Append(SubstantivePossessive)
			.Append('\n')
			.Append("Reflexive Pronoun: ")
			.Append(Reflexive)
			.Append('\n')
			.Append("Person Term: ")
			.Append(PersonTerm)
			.Append('\n')
			.Append("Immature Person Term: ")
			.Append(ImmaturePersonTerm)
			.Append('\n')
			.Append("Formal Address Term: ")
			.Append(FormalAddressTerm)
			.Append('\n')
			.Append("Offspring Term: ")
			.Append(OffspringTerm)
			.Append('\n')
			.Append("Sibling Term: ")
			.Append(SiblingTerm)
			.Append('\n')
			.Append("Parent Term: ")
			.Append(ParentTerm)
			.Append('\n')
			.Append("Indicative Proximal: ")
			.Append(IndicativeProximal)
			.Append('\n')
			.Append("Indicative Distal: ")
			.Append(IndicativeDistal)
			.Append('\n')
			.Append("Use Bare Indicative: ")
			.Append(UseBareIndicative ? "Yes" : "No")
			.Append('\n')
			.Append("Do Not Replicate As Pronoun Set: ")
			.Append(DoNotReplicateAsPronounSet ? "Yes" : "No")
			.Append('\n');
	}

	public string GetSummary()
	{
		return GetSummary(Event.NewStringBuilder()).ToString();
	}

	public StringBuilder GetBasicSummary(StringBuilder sb)
	{
		if (Plural)
		{
			sb.Append("Plural\n");
		}
		if (PseudoPlural)
		{
			sb.Append("Pseudo-Plural\n");
		}
		if (UseBareIndicative)
		{
			sb.Append("Nonpersonal\n");
		}
		return sb.Append("Subjective Pronoun: ").Append(Subjective).Append('\n')
			.Append("Objective Pronoun: ")
			.Append(Objective)
			.Append('\n')
			.Append("Possessive Adjective: ")
			.Append(PossessiveAdjective)
			.Append('\n')
			.Append("Substantive Possessive: ")
			.Append(SubstantivePossessive)
			.Append('\n')
			.Append("Reflexive Pronoun: ")
			.Append(Reflexive)
			.Append('\n')
			.Append("Person Term: ")
			.Append(PersonTerm)
			.Append('\n')
			.Append("Immature Person Term: ")
			.Append(ImmaturePersonTerm)
			.Append('\n')
			.Append("Formal Address Term: ")
			.Append(FormalAddressTerm)
			.Append('\n')
			.Append("Offspring Term: ")
			.Append(OffspringTerm)
			.Append('\n')
			.Append("Sibling Term: ")
			.Append(SiblingTerm)
			.Append('\n')
			.Append("Parent Term: ")
			.Append(ParentTerm)
			.Append('\n');
	}

	public string GetBasicSummary()
	{
		return GetBasicSummary(Event.NewStringBuilder()).ToString();
	}

	public Gender Register()
	{
		if (!Genders.ContainsKey(Name))
		{
			Genders.Add(Name, this);
			All = null;
		}
		return this;
	}

	public static Gender Get(string Name, bool GenerateOnFail = false)
	{
		if (Genders.TryGetValue(Name, out var value))
		{
			return value;
		}
		if (GenerateOnFail)
		{
			Debug.LogError("failed to retrieve gender '" + Name + "', generating a random placeholder");
			return Generate().Register();
		}
		throw new Exception("request for unknown gender '" + Name + "'");
	}

	public static Gender GetIfExists(string Name)
	{
		if (Name == null)
		{
			return null;
		}
		if (Genders.TryGetValue(Name, out var value))
		{
			return value;
		}
		return null;
	}

	public static bool Exists(string Name)
	{
		return Genders.ContainsKey(Name);
	}

	private static string GenerateBasePronoun(string stem, string stemVowel, string mainVowel, Dictionary<string, int> suffixes)
	{
		string text;
		if (stemVowel != null)
		{
			text = stem;
		}
		else
		{
			string text2 = (PronounAltVowelChance.in100() ? PronounVowels.GetRandomElement() : mainVowel);
			text = stem + text2;
		}
		string randomElement = suffixes.GetRandomElement();
		if (!randomElement.IsNullOrEmpty() && !text.EndsWith(randomElement))
		{
			text += randomElement;
		}
		if (stem.Length == 1)
		{
			string randomElement2 = PronounPrefixes.GetRandomElement();
			if (!randomElement2.IsNullOrEmpty() && randomElement2 != stem)
			{
				text = randomElement2 + text;
			}
		}
		foreach (string key in PronounMapStrings.Keys)
		{
			if (text.Contains(key))
			{
				text = text.Replace(key, PronounMapStrings[key]);
			}
		}
		foreach (KeyValuePair<string, string> pronounMapPattern in PronounMapPatterns)
		{
			text = Regex.Replace(text, pronounMapPattern.Key, pronounMapPattern.Value);
		}
		return text;
	}

	private static bool CullPronoun(string pronoun)
	{
		foreach (string pronounCullString in PronounCullStrings)
		{
			if (pronoun.Contains(pronounCullString))
			{
				return true;
			}
		}
		foreach (string pronounCullPattern in PronounCullPatterns)
		{
			if (Regex.IsMatch(pronoun, pronounCullPattern))
			{
				return true;
			}
		}
		if (Grammar.ContainsBadWords(pronoun))
		{
			return true;
		}
		return false;
	}

	public static void GenerateBasePronouns(out string subjective, out string objective, out string possessive)
	{
		do
		{
			string text = PronounStems.GetRandomElement();
			string text2 = (PronounStemVowelChance.in100() ? PronounVowels.GetRandomElement() : null);
			if (text2 != null)
			{
				text = text2 + text;
			}
			string mainVowel = ((text2 != null) ? null : PronounVowels.GetRandomElement());
			subjective = GenerateBasePronoun(text, text2, mainVowel, PronounSubjectiveSuffixes);
			objective = GenerateBasePronoun(text, text2, mainVowel, PronounObjectiveSuffixes);
			possessive = GenerateBasePronoun(text, text2, mainVowel, PronounPossessiveSuffixes);
		}
		while (subjective == possessive || objective == possessive || CullPronoun(subjective) || CullPronoun(objective) || CullPronoun(possessive));
	}

	public static Gender Generate(bool Generic = false, int PluralChance = 0, int UseBareIndicativeChance = 0, GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null)
	{
		if (!EnableGeneration)
		{
			bool flag = PluralChance.in100();
			bool flag2 = UseBareIndicativeChance.in100();
			if (Generic)
			{
				if (flag)
				{
					if (!flag2)
					{
						return GetAnyGenericPersonalPlural();
					}
					return GetAnyGenericNonpersonalPlural();
				}
				if (!flag2)
				{
					return GetAnyGenericPersonalSingular();
				}
				return GetAnyGenericNonpersonalSingular();
			}
			if (flag)
			{
				if (!flag2)
				{
					return GetAnyPersonalPlural();
				}
				return GetAnyNonpersonalPlural();
			}
			if (!flag2)
			{
				return GetAnyPersonalSingular();
			}
			return GetAnyNonpersonalSingular();
		}
		GenerateBasePronouns(out var subjective, out var objective, out var possessive);
		int num = 0;
		string text;
		do
		{
			if (++num > 1000)
			{
				throw new Exception("cannot generate unused gender name");
			}
			text = NameMaker.MakeName(For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderName");
		}
		while (Genders.ContainsKey(text) || (num < 20 && !text.Contains(subjective, CompareOptions.IgnoreCase) && !text.Contains(objective, CompareOptions.IgnoreCase) && !text.Contains(possessive, CompareOptions.IgnoreCase)));
		Gender gender = new Gender(text, Generic, _Generated: true);
		if (PluralChance.in100())
		{
			gender.Plural = true;
		}
		if (UseBareIndicativeChance.in100())
		{
			gender.UseBareIndicative = true;
		}
		gender.Subjective = subjective;
		gender.Objective = objective;
		gender.PossessiveAdjective = possessive;
		if (!gender.Plural && gender.Subjective.EndsWith("y"))
		{
			gender.PseudoPlural = true;
		}
		NameMaker.MakeName(ref gender._PersonTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderPersonTerm");
		NameMaker.MakeName(ref gender._ImmaturePersonTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderImmaturePersonTerm");
		NameMaker.MakeName(ref gender._FormalAddressTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderFormalAddressTerm");
		NameMaker.MakeName(ref gender._OffspringTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderOffspringTerm");
		NameMaker.MakeName(ref gender._SiblingTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderSiblingTerm");
		NameMaker.MakeName(ref gender._ParentTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderParentTerm");
		return gender;
	}

	public static void Clear()
	{
		Genders = null;
		All = null;
		GenericPlurals = null;
		Generics = null;
		GenericSingulars = null;
		Plurals = null;
		Singulars = null;
		GenericPersonalPlurals = null;
		GenericPersonals = null;
		GenericPersonalSingulars = null;
		Personals = null;
		PersonalPlurals = null;
		PersonalSingulars = null;
		GenericNonpersonalPlurals = null;
		GenericNonpersonals = null;
		GenericNonpersonalSingulars = null;
		Nonpersonals = null;
		NonpersonalPlurals = null;
		NonpersonalSingulars = null;
	}

	[ModSensitiveCacheInit]
	public static void ResetModSensitiveStaticCache()
	{
		Clear();
		Loading.LoadTask("Loading genders", Init);
	}

	public static void Init()
	{
		if (Genders == null)
		{
			Genders = new Dictionary<string, Gender>(32);
		}
		PronounAltVowelChance = 0;
		PronounStemVowelChance = 0;
		PronounStems = new Dictionary<string, int>(64);
		PronounVowels = new Dictionary<string, int>(64);
		PronounPrefixes = new Dictionary<string, int>(16);
		PronounSubjectiveSuffixes = new Dictionary<string, int>(16);
		PronounObjectiveSuffixes = new Dictionary<string, int>(16);
		PronounPossessiveSuffixes = new Dictionary<string, int>(16);
		PronounCullStrings = new List<string>(64);
		PronounCullPatterns = new List<string>(32);
		PronounMapStrings = new Dictionary<string, string>(64);
		PronounMapPatterns = new Dictionary<string, string>(32);
		NamePrefixes = new List<string>(16);
		NameInfixes = new List<string>(16);
		NameSuffixes = new List<string>(16);
		string GenerateGenericSpec = null;
		Action<string, bool> action = (string file, bool mod) =>
		{
			using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(file);
			xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
			while (xmlTextReader.Read())
			{
				if (xmlTextReader.NodeType == XmlNodeType.Element)
				{
					if (xmlTextReader.Name == "genders")
					{
						LoadGendersNode(xmlTextReader, ref GenerateGenericSpec, mod);
					}
				}
				else if (xmlTextReader.NodeType == XmlNodeType.EndElement && xmlTextReader.Name == "genders")
				{
					break;
				}
			}
			xmlTextReader.Close();
		};
		foreach (DataFile item in DataManager.GetXMLFilesWithRoot("genders"))
		{
			try
			{
				action(item, item.IsMod);
			}
			catch (Exception message)
			{
				MetricsManager.LogPotentialModError(item.Mod, message);
			}
		}
		if (!GenerateGenericSpec.IsNullOrEmpty() && EnableGeneration)
		{
			int num = GenerateGenericSpec.Roll();
			int num2 = 0;
			foreach (string key in Genders.Keys)
			{
				Gender gender = Genders[key];
				if (gender.Generic && gender.Generated)
				{
					num2++;
				}
			}
			for (int num3 = num2; num3 < num; num3++)
			{
				Generate(Generic: true).Register();
			}
		}
		SetDefaults();
	}

	public static void LoadGendersNode(XmlTextReader Reader, ref string GenerateGenericSpec, bool mod = false)
	{
		string attribute = Reader.GetAttribute("EnableSelection");
		if (!attribute.IsNullOrEmpty())
		{
			EnableSelection = Convert.ToBoolean(attribute);
		}
		string attribute2 = Reader.GetAttribute("EnableGeneration");
		if (!attribute2.IsNullOrEmpty())
		{
			EnableGeneration = Convert.ToBoolean(attribute2);
		}
		string attribute3 = Reader.GetAttribute("EnableDisplay");
		if (!attribute3.IsNullOrEmpty())
		{
			EnableDisplay = Convert.ToBoolean(attribute3);
		}
		string attribute4 = Reader.GetAttribute("GenerateGeneric");
		if (!attribute4.IsNullOrEmpty())
		{
			GenerateGenericSpec = attribute4;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			if (Reader.Name == "gender")
			{
				LoadGenderNode(Reader, mod);
			}
			else if (Reader.Name == "removegender")
			{
				string attribute5 = Reader.GetAttribute("Name");
				if (attribute5.IsNullOrEmpty())
				{
					throw new Exception("removegender tag had no Name attribute");
				}
				if (Genders.ContainsKey(attribute5))
				{
					Genders.Remove(attribute5);
				}
			}
			else if (Reader.Name == "genderPronounGeneration")
			{
				LoadGenderPronounGenerationNode(Reader, mod, Reader.Name);
			}
		}
	}

	public static void LoadGenderNode(XmlTextReader Reader, bool mod = false)
	{
		string attribute = Reader.GetAttribute("Name");
		if (attribute.IsNullOrEmpty())
		{
			throw new Exception("gender tag had no Name attribute");
		}
		if (!Genders.TryGetValue(attribute, out var value))
		{
			value = new Gender(attribute, _Generic: true);
			Genders.Add(attribute, value);
		}
		string attribute2 = Reader.GetAttribute("Generic");
		if (!attribute2.IsNullOrEmpty())
		{
			value.Generic = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("Plural");
		if (!attribute2.IsNullOrEmpty())
		{
			value.Plural = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("PseudoPlural");
		if (!attribute2.IsNullOrEmpty())
		{
			value.PseudoPlural = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("Subjective");
		if (!attribute2.IsNullOrEmpty())
		{
			value.Subjective = attribute2;
		}
		attribute2 = Reader.GetAttribute("Objective");
		if (attribute2 != null)
		{
			value.Objective = ((attribute2 == "") ? null : attribute2);
		}
		attribute2 = Reader.GetAttribute("PossessiveAdjective");
		if (attribute2 != null)
		{
			value.PossessiveAdjective = ((attribute2 == "") ? null : attribute2);
		}
		attribute2 = Reader.GetAttribute("SubstantivePossessive");
		if (attribute2 != null)
		{
			value.SubstantivePossessive = ((attribute2 == "") ? null : attribute2);
		}
		attribute2 = Reader.GetAttribute("Reflexive");
		if (attribute2 != null)
		{
			value.Reflexive = ((attribute2 == "") ? null : attribute2);
		}
		attribute2 = Reader.GetAttribute("PersonTerm");
		if (!attribute2.IsNullOrEmpty())
		{
			value.PersonTerm = attribute2;
		}
		attribute2 = Reader.GetAttribute("ImmaturePersonTerm");
		if (!attribute2.IsNullOrEmpty())
		{
			value.ImmaturePersonTerm = attribute2;
		}
		attribute2 = Reader.GetAttribute("FormalAddressTerm");
		if (!attribute2.IsNullOrEmpty())
		{
			value.FormalAddressTerm = attribute2;
		}
		attribute2 = Reader.GetAttribute("OffspringTerm");
		if (!attribute2.IsNullOrEmpty())
		{
			value.OffspringTerm = attribute2;
		}
		attribute2 = Reader.GetAttribute("SiblingTerm");
		if (!attribute2.IsNullOrEmpty())
		{
			value.SiblingTerm = attribute2;
		}
		attribute2 = Reader.GetAttribute("ParentTerm");
		if (!attribute2.IsNullOrEmpty())
		{
			value.ParentTerm = attribute2;
		}
		attribute2 = Reader.GetAttribute("UseBareIndicative");
		if (!attribute2.IsNullOrEmpty())
		{
			value.UseBareIndicative = Convert.ToBoolean(attribute2);
		}
		attribute2 = Reader.GetAttribute("DoNotReplicateAsPronounSet");
		if (!attribute2.IsNullOrEmpty())
		{
			value.DoNotReplicateAsPronounSet = Convert.ToBoolean(attribute2);
		}
	}

	private static int TryInt(string Spec, string What)
	{
		try
		{
			return Convert.ToInt32(Spec);
		}
		catch
		{
			Debug.LogError("Error in " + What + ": " + Spec);
		}
		return 0;
	}

	private static void LoadTextAndWeightNode(XmlTextReader Reader, bool Mod, Dictionary<string, int> store)
	{
		string attribute = Reader.GetAttribute("Text");
		if (attribute == null)
		{
			throw new Exception(Reader.Name + " tag had no Text attribute");
		}
		int value = TryInt(Reader.GetAttribute("Weight") ?? throw new Exception(Reader.Name + " tag had no Weight attribute"), "Weight attribute");
		if (store.ContainsKey(attribute))
		{
			if (!Mod)
			{
				Debug.LogError("duplicate " + Reader.Name + " entry on " + attribute);
			}
			store[attribute] = value;
		}
		else
		{
			store.Add(attribute, value);
		}
	}

	private static void LoadTextNode(XmlTextReader Reader, bool Mod, List<string> store)
	{
		string attribute = Reader.GetAttribute("Text");
		if (attribute == null)
		{
			throw new Exception(Reader.Name + " tag had no Text attribute");
		}
		if (store.Contains(attribute))
		{
			if (!Mod)
			{
				Debug.LogError("duplicate " + Reader.Name + " entry on " + attribute);
			}
		}
		else
		{
			store.Add(attribute);
		}
	}

	private static void LoadRemoveTextNode(XmlTextReader Reader, bool Mod, List<string> store)
	{
		string attribute = Reader.GetAttribute("Text");
		if (attribute == null)
		{
			throw new Exception(Reader.Name + " tag had no Text attribute");
		}
		if (store.Contains(attribute))
		{
			store.Remove(attribute);
		}
	}

	private static void LoadSearchReplaceNode(XmlTextReader Reader, bool Mod, Dictionary<string, string> store)
	{
		string attribute = Reader.GetAttribute("Search");
		if (attribute == null)
		{
			throw new Exception(Reader.Name + " tag had no Search attribute");
		}
		string attribute2 = Reader.GetAttribute("Replace");
		if (attribute2 == null)
		{
			throw new Exception(Reader.Name + " tag had no Replace attribute");
		}
		if (store.ContainsKey(attribute))
		{
			if (!Mod)
			{
				Debug.LogError("duplicate " + Reader.Name + " entry on " + attribute);
			}
			store[attribute] = attribute2;
		}
		else
		{
			store.Add(attribute, attribute2);
		}
	}

	private static void LoadRemoveSearchReplaceNode(XmlTextReader Reader, bool Mod, Dictionary<string, string> store)
	{
		string attribute = Reader.GetAttribute("Search");
		if (attribute == null)
		{
			throw new Exception(Reader.Name + " tag had no Search attribute");
		}
		string attribute2 = Reader.GetAttribute("Replace");
		if (attribute2 == null)
		{
			throw new Exception(Reader.Name + " tag had no Replace attribute");
		}
		if (store.ContainsKey(attribute) && store[attribute] == attribute2)
		{
			store.Remove(attribute);
		}
	}

	public static void LoadGenderPronounGenerationNode(XmlTextReader Reader, bool Mod, string MainTag)
	{
		string attribute = Reader.GetAttribute("AltVowelChance");
		if (!attribute.IsNullOrEmpty())
		{
			PronounAltVowelChance = Convert.ToInt32(attribute);
		}
		string attribute2 = Reader.GetAttribute("StemVowelChance");
		if (!attribute2.IsNullOrEmpty())
		{
			PronounStemVowelChance = Convert.ToInt32(attribute2);
		}
		while (Reader.Read())
		{
			if (Reader.Name == "stem")
			{
				LoadTextAndWeightNode(Reader, Mod, PronounStems);
			}
			else if (Reader.Name == "vowel")
			{
				LoadTextAndWeightNode(Reader, Mod, PronounVowels);
			}
			else if (Reader.Name == "prefix")
			{
				LoadTextAndWeightNode(Reader, Mod, PronounPrefixes);
			}
			else if (Reader.Name == "subjectivesuffix")
			{
				LoadTextAndWeightNode(Reader, Mod, PronounSubjectiveSuffixes);
			}
			else if (Reader.Name == "objectivesuffix")
			{
				LoadTextAndWeightNode(Reader, Mod, PronounObjectiveSuffixes);
			}
			else if (Reader.Name == "possessivesuffix")
			{
				LoadTextAndWeightNode(Reader, Mod, PronounPossessiveSuffixes);
			}
			else if (Reader.Name == "possessivesuffix")
			{
				LoadTextAndWeightNode(Reader, Mod, PronounPossessiveSuffixes);
			}
			else if (Reader.Name == "cullstring")
			{
				LoadTextNode(Reader, Mod, PronounCullStrings);
			}
			else if (Reader.Name == "removecullstring")
			{
				LoadRemoveTextNode(Reader, Mod, PronounCullStrings);
			}
			else if (Reader.Name == "cullpattern")
			{
				LoadTextNode(Reader, Mod, PronounCullPatterns);
			}
			else if (Reader.Name == "removecullpattern")
			{
				LoadRemoveTextNode(Reader, Mod, PronounCullPatterns);
			}
			else if (Reader.Name == "mapstring")
			{
				LoadSearchReplaceNode(Reader, Mod, PronounMapStrings);
			}
			else if (Reader.Name == "removemapstring")
			{
				LoadRemoveSearchReplaceNode(Reader, Mod, PronounMapStrings);
			}
			else if (Reader.Name == "mappattern")
			{
				LoadSearchReplaceNode(Reader, Mod, PronounMapPatterns);
			}
			else if (Reader.Name == "removemappattern")
			{
				LoadRemoveSearchReplaceNode(Reader, Mod, PronounMapPatterns);
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == MainTag)
			{
				break;
			}
		}
	}

	public static List<Gender> Find(Predicate<Gender> Filter)
	{
		int num = 0;
		if (Genders != null)
		{
			foreach (string key in Genders.Keys)
			{
				if (Filter(Genders[key]))
				{
					num++;
				}
			}
		}
		List<Gender> list = new List<Gender>(num);
		if (num > 0)
		{
			foreach (string key2 in Genders.Keys)
			{
				Gender gender = Genders[key2];
				if (Filter(gender))
				{
					list.Add(gender);
				}
			}
		}
		return list;
	}

	public static List<Gender> GetAll()
	{
		if (All == null)
		{
			All = Find((Gender G) => true);
		}
		return All;
	}

	public static Gender GetAny()
	{
		return GetAll().GetRandomElement();
	}

	public static string GetAnyName()
	{
		return GetAny().Name;
	}

	public static List<Gender> GetAllGeneric()
	{
		if (Generics == null)
		{
			Generics = Find((Gender G) => G.Generic);
		}
		return Generics;
	}

	public static Gender GetAnyGeneric()
	{
		return GetAllGeneric().GetRandomElement();
	}

	public static string GetAnyGenericName()
	{
		return GetAnyGeneric().Name;
	}

	public static List<Gender> GetAllPlural()
	{
		if (Plurals == null)
		{
			Plurals = Find((Gender G) => G.Plural);
		}
		return Plurals;
	}

	public static Gender GetAnyPlural()
	{
		return GetAllPlural().GetRandomElement();
	}

	public static string GetAnyPluralName()
	{
		return GetAnyPlural().Name;
	}

	public static List<Gender> GetAllSingular()
	{
		if (Singulars == null)
		{
			Singulars = Find((Gender G) => !G.Plural);
		}
		return Singulars;
	}

	public static Gender GetAnySingular()
	{
		return GetAllSingular().GetRandomElement();
	}

	public static string GetAnySingularName()
	{
		return GetAnySingular().Name;
	}

	public static List<Gender> GetAllGenericSingular()
	{
		if (GenericSingulars == null)
		{
			GenericSingulars = Find((Gender G) => G.Generic && !G.Plural);
		}
		return GenericSingulars;
	}

	public static Gender GetAnyGenericSingular()
	{
		return GetAllGenericSingular().GetRandomElement();
	}

	public static string GetAnyGenericSingularName()
	{
		return GetAnyGenericSingular().Name;
	}

	public static List<Gender> GetAllGenericPlural()
	{
		if (GenericPlurals == null)
		{
			GenericPlurals = Find((Gender G) => G.Generic && G.Plural);
		}
		return GenericPlurals;
	}

	public static Gender GetAnyGenericPlural()
	{
		return GetAllGenericPlural().GetRandomElement();
	}

	public static string GetAnyGenericPluralName()
	{
		return GetAnyGenericPlural().Name;
	}

	public static List<Gender> GetAllPersonal()
	{
		if (Personals == null)
		{
			Personals = Find((Gender G) => !G.UseBareIndicative);
		}
		return Personals;
	}

	public static Gender GetAnyPersonal()
	{
		return GetAllPersonal().GetRandomElement();
	}

	public static string GetAnyPersonalName()
	{
		return GetAnyPersonal().Name;
	}

	public static List<Gender> GetAllPersonalPlural()
	{
		if (PersonalPlurals == null)
		{
			PersonalPlurals = Find((Gender G) => !G.UseBareIndicative && G.Plural);
		}
		return PersonalPlurals;
	}

	public static Gender GetAnyPersonalPlural()
	{
		return GetAllPersonalPlural().GetRandomElement();
	}

	public static string GetAnyPersonalPluralName()
	{
		return GetAnyPersonalPlural().Name;
	}

	public static List<Gender> GetAllPersonalSingular()
	{
		if (PersonalSingulars == null)
		{
			PersonalSingulars = Find((Gender G) => !G.UseBareIndicative && !G.Plural);
		}
		return PersonalSingulars;
	}

	public static Gender GetAnyPersonalSingular()
	{
		return GetAllPersonalSingular().GetRandomElement();
	}

	public static string GetAnyPersonalSingularName()
	{
		return GetAnyPersonalSingular().Name;
	}

	public static List<Gender> GetAllGenericPersonal()
	{
		if (GenericPersonals == null)
		{
			GenericPersonals = Find((Gender G) => G.Generic && !G.UseBareIndicative);
		}
		return GenericPersonals;
	}

	public static Gender GetAnyGenericPersonal()
	{
		return GetAllGenericPersonal().GetRandomElement();
	}

	public static string GetAnyGenericPersonalName()
	{
		return GetAnyGenericPersonalSingular().Name;
	}

	public static List<Gender> GetAllGenericPersonalPlural()
	{
		if (GenericPersonalPlurals == null)
		{
			GenericPersonalPlurals = Find((Gender G) => G.Generic && !G.UseBareIndicative && G.Plural);
		}
		return GenericPersonalPlurals;
	}

	public static Gender GetAnyGenericPersonalPlural()
	{
		return GetAllGenericPersonalPlural().GetRandomElement();
	}

	public static string GetAnyGenericPersonalPluralName()
	{
		return GetAnyGenericPersonalSingular().Name;
	}

	public static List<Gender> GetAllGenericPersonalSingular()
	{
		if (GenericPersonalSingulars == null)
		{
			GenericPersonalSingulars = Find((Gender G) => G.Generic && !G.UseBareIndicative && !G.Plural);
		}
		return GenericPersonalSingulars;
	}

	public static Gender GetAnyGenericPersonalSingular()
	{
		return GetAllGenericPersonalSingular().GetRandomElement();
	}

	public static string GetAnyGenericPersonalSingularName()
	{
		return GetAnyGenericPersonalSingular().Name;
	}

	public static List<Gender> GetAllNonpersonal()
	{
		if (Nonpersonals == null)
		{
			Nonpersonals = Find((Gender G) => G.UseBareIndicative);
		}
		return Nonpersonals;
	}

	public static Gender GetAnyNonpersonal()
	{
		return GetAllNonpersonal().GetRandomElement();
	}

	public static string GetAnyNonpersonalName()
	{
		return GetAnyNonpersonal().Name;
	}

	public static List<Gender> GetAllNonpersonalPlural()
	{
		if (NonpersonalPlurals == null)
		{
			NonpersonalPlurals = Find((Gender G) => G.UseBareIndicative && G.Plural);
		}
		return NonpersonalPlurals;
	}

	public static Gender GetAnyNonpersonalPlural()
	{
		return GetAllNonpersonalPlural().GetRandomElement();
	}

	public static string GetAnyNonpersonalPluralName()
	{
		return GetAnyNonpersonalPlural().Name;
	}

	public static List<Gender> GetAllNonpersonalSingular()
	{
		if (NonpersonalSingulars == null)
		{
			NonpersonalSingulars = Find((Gender G) => G.UseBareIndicative && !G.Plural);
		}
		return NonpersonalSingulars;
	}

	public static Gender GetAnyNonpersonalSingular()
	{
		return GetAllNonpersonalSingular().GetRandomElement();
	}

	public static string GetAnyNonpersonalSingularName()
	{
		return GetAnyNonpersonalSingular().Name;
	}

	public static List<Gender> GetAllGenericNonpersonal()
	{
		if (GenericNonpersonals == null)
		{
			GenericNonpersonals = Find((Gender G) => G.Generic && G.UseBareIndicative);
		}
		return GenericNonpersonals;
	}

	public static Gender GetAnyGenericNonpersonal()
	{
		return GetAllGenericNonpersonal().GetRandomElement();
	}

	public static string GetAnyGenericNonpersonalName()
	{
		return GetAnyGenericNonpersonalSingular().Name;
	}

	public static List<Gender> GetAllGenericNonpersonalPlural()
	{
		if (GenericNonpersonalPlurals == null)
		{
			GenericNonpersonalPlurals = Find((Gender G) => G.Generic && G.UseBareIndicative && G.Plural);
		}
		return GenericNonpersonalPlurals;
	}

	public static Gender GetAnyGenericNonpersonalPlural()
	{
		return GetAllGenericNonpersonalPlural().GetRandomElement();
	}

	public static string GetAnyGenericNonpersonalPluralName()
	{
		return GetAnyGenericNonpersonalSingular().Name;
	}

	public static List<Gender> GetAllGenericNonpersonalSingular()
	{
		if (GenericNonpersonalSingulars == null)
		{
			GenericNonpersonalSingulars = Find((Gender G) => G.Generic && G.UseBareIndicative && !G.Plural);
		}
		return GenericNonpersonalSingulars;
	}

	public static Gender GetAnyGenericNonpersonalSingular()
	{
		return GetAllGenericNonpersonalSingular().GetRandomElement();
	}

	public static string GetAnyGenericNonpersonalSingularName()
	{
		return GetAnyGenericNonpersonalSingular().Name;
	}

	public static string CheckSpecial(string Name)
	{
		return Name switch
		{
			"generate" => Generate().Register().Name, 
			"generatemaybeplural" => Generate(Generic: false, 10).Register().Name, 
			"generatemaybenonpersonal" => Generate(Generic: false, 0, 10).Register().Name, 
			"generatemaybepluralmaybenonpersonal" => Generate(Generic: false, 10, 10).Register().Name, 
			"any" => GetAnyName(), 
			"anyplural" => GetAnyPluralName(), 
			"anysingular" => GetAnySingularName(), 
			"generic" => GetAnyGenericName(), 
			"genericplural" => GetAnyGenericPluralName(), 
			"genericsingular" => GetAnyGenericSingularName(), 
			"personal" => GetAnyPersonalName(), 
			"personalplural" => GetAnyPersonalPluralName(), 
			"personalsingular" => GetAnyPersonalSingularName(), 
			"genericpersonal" => GetAnyGenericPersonalName(), 
			"genericpersonalplural" => GetAnyGenericPersonalPluralName(), 
			"genericpersonalsingular" => GetAnyGenericPersonalSingularName(), 
			"nonpersonal" => GetAnyNonpersonalName(), 
			"nonpersonalplural" => GetAnyNonpersonalPluralName(), 
			"nonpersonalsingular" => GetAnyNonpersonalSingularName(), 
			"genericnonpersonal" => GetAnyGenericNonpersonalName(), 
			"genericnonpersonalplural" => GetAnyGenericNonpersonalPluralName(), 
			"genericnonpersonalsingular" => GetAnyGenericNonpersonalSingularName(), 
			_ => Name, 
		};
	}
}
