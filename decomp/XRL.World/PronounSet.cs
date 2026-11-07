using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ConsoleLib.Console;
using XRL.Names;
using XRL.Rules;
using XRL.UI;

namespace XRL.World;

[HasModSensitiveStaticCache]
public class PronounSet : BasePronounProvider
{
	public static bool EnableSelection = false;

	public static bool EnableGeneration = false;

	public static bool EnableConversationalExchange = false;

	public static Dictionary<string, PronounSet> PronounSets = null;

	public static Dictionary<string, PronounSet> TempPronounSets = null;

	[NonSerialized]
	public static List<PronounSet> All = null;

	[NonSerialized]
	public static List<PronounSet> GenericPlurals = null;

	[NonSerialized]
	public static List<PronounSet> Generics = null;

	[NonSerialized]
	public static List<PronounSet> GenericSingulars = null;

	[NonSerialized]
	public static List<PronounSet> Plurals = null;

	[NonSerialized]
	public static List<PronounSet> Singulars = null;

	[NonSerialized]
	public static List<PronounSet> GenericPersonalPlurals = null;

	[NonSerialized]
	public static List<PronounSet> GenericPersonals = null;

	[NonSerialized]
	public static List<PronounSet> GenericPersonalSingulars = null;

	[NonSerialized]
	public static List<PronounSet> Personals = null;

	[NonSerialized]
	public static List<PronounSet> PersonalPlurals = null;

	[NonSerialized]
	public static List<PronounSet> PersonalSingulars = null;

	[NonSerialized]
	public static List<PronounSet> GenericNonpersonalPlurals = null;

	[NonSerialized]
	public static List<PronounSet> GenericNonpersonals = null;

	[NonSerialized]
	public static List<PronounSet> GenericNonpersonalSingulars = null;

	[NonSerialized]
	public static List<PronounSet> Nonpersonals = null;

	[NonSerialized]
	public static List<PronounSet> NonpersonalPlurals = null;

	[NonSerialized]
	public static List<PronounSet> NonpersonalSingulars = null;

	[NonSerialized]
	public static PronounSet DefaultPlayer = null;

	[NonSerialized]
	public static PronounSet DefaultPlayerPlural = null;

	public bool FromGender;

	[NonSerialized]
	public string _Name;

	[NonSerialized]
	public string _CapitalizedName;

	[NonSerialized]
	private static StringBuilder SB = new StringBuilder();

	public override string Name
	{
		get
		{
			if (_Name == null)
			{
				_Name = CalculateName();
			}
			return _Name;
		}
	}

	public override string CapitalizedName
	{
		get
		{
			if (_CapitalizedName == null)
			{
				_CapitalizedName = ColorUtility.CapitalizeExceptFormatting(Name);
			}
			return _CapitalizedName;
		}
	}

	public PronounSet(bool _Generic = false, bool _Generated = false, bool _Plural = false, bool _PseudoPlural = false, string _Subjective = null, string _Objective = null, string _PossessiveAdjective = null, string _SubstantivePossessive = null, string _Reflexive = null, string _PersonTerm = null, string _ImmaturePersonTerm = null, string _FormalAddressTerm = null, string _OffspringTerm = null, string _SiblingTerm = null, string _ParentTerm = null, bool _UseBareIndicative = false, bool _FromGender = false)
		: base(_Generic, _Generated, _Plural, _PseudoPlural, _Subjective, _Objective, _PossessiveAdjective, _SubstantivePossessive, _Reflexive, _PersonTerm, _ImmaturePersonTerm, _FormalAddressTerm, _OffspringTerm, _SiblingTerm, _ParentTerm, _UseBareIndicative)
	{
		FromGender = _FromGender;
	}

	/// <summary>Copy from <paramref name="Original" /></summary>
	public PronounSet(BasePronounProvider Original)
		: base(Original)
	{
	}

	/// <summary>Empty constructor for serialization purposes</summary>
	public PronounSet()
	{
	}

	public override void CopyFrom(BasePronounProvider Original)
	{
		if (Original is Gender)
		{
			FromGender = true;
		}
		if (Original is PronounSet pronounSet)
		{
			FromGender = pronounSet.FromGender;
		}
		base.CopyFrom(Original);
	}

	public override BasePronounProvider Clone()
	{
		return new PronounSet(this);
	}

	public static void SaveAll(SerializationWriter Writer)
	{
		Writer.Write(PronounSets.Count);
		foreach (string key in PronounSets.Keys)
		{
			PronounSets[key].Save(Writer);
		}
	}

	public static void LoadAll(SerializationReader Reader)
	{
		if (PronounSets == null)
		{
			PronounSets = new Dictionary<string, PronounSet>(64);
		}
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PronounSet pronounSet = Load(Reader);
			PronounSets[pronounSet.Name] = pronounSet;
		}
	}

	public override void Save(SerializationWriter Writer)
	{
		base.Save(Writer);
		Writer.Write(FromGender);
	}

	public static PronounSet Load(SerializationReader Reader)
	{
		PronounSet pronounSet = new PronounSet();
		BasePronounProvider.Load(Reader, pronounSet);
		pronounSet.FromGender = Reader.ReadBoolean();
		return pronounSet;
	}

	private static void SetDefaults(Dictionary<string, PronounSet> PronounSets)
	{
		if (PronounSets.TryGetValue("player", out DefaultPlayer))
		{
			PronounSets.Remove("player");
		}
		else
		{
			MetricsManager.LogError("Default pronoun set 'player' not found.");
		}
		if (PronounSets.TryGetValue("plural/player", out DefaultPlayerPlural))
		{
			PronounSets.Remove("plural/player");
		}
		else
		{
			MetricsManager.LogError("Default pronoun set 'plural/player' not found.");
		}
	}

	public override async Task<bool> CustomizeAsync()
	{
		return await Customize("pronoun set");
	}

	protected override async Task<bool> CustomizeProcess(string What)
	{
		FromGender = false;
		return await base.CustomizeProcess(What);
	}

	protected override void ConfigurationUpdated(bool Categorizing = false)
	{
		_Name = null;
		if (Categorizing)
		{
			FlushRelevantCaches();
		}
	}

	public StringBuilder GetSummary(StringBuilder sb)
	{
		return sb.Append("Short Name: ").Append(GetShortName()).Append('\n')
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
			.Append("From Gender: ")
			.Append(FromGender ? "Yes" : "No")
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

	public string CalculateName()
	{
		StringBuilder stringBuilder = new StringBuilder(48);
		if (Plural)
		{
			stringBuilder.Append("plural/");
		}
		if (PseudoPlural)
		{
			stringBuilder.Append("pseudo-plural/");
		}
		if (UseBareIndicative)
		{
			stringBuilder.Append("nonperson/");
		}
		stringBuilder.Append(Subjective).Append('/').Append(Objective)
			.Append('/')
			.Append(PossessiveAdjective)
			.Append('/')
			.Append(SubstantivePossessive)
			.Append('/')
			.Append(Reflexive)
			.Append('/')
			.Append(PersonTerm)
			.Append('/')
			.Append(ImmaturePersonTerm)
			.Append('/')
			.Append(FormalAddressTerm)
			.Append('/')
			.Append(OffspringTerm)
			.Append('/')
			.Append(SiblingTerm)
			.Append('/')
			.Append(ParentTerm);
		return stringBuilder.ToString();
	}

	public string GetShortName()
	{
		SB.Clear();
		if (Plural)
		{
			SB.Append("plural/");
		}
		if (UseBareIndicative)
		{
			SB.Append("nonperson/");
		}
		SB.Append(Subjective).Append('/').Append(Objective);
		if (PossessiveAdjective != Objective)
		{
			SB.Append('/').Append(PossessiveAdjective);
		}
		if (SubstantivePossessive != ExpectedSubstantivePossessive())
		{
			SB.Append('/').Append(SubstantivePossessive);
		}
		if (Reflexive != ExpectedReflexive())
		{
			SB.Append('/').Append(Reflexive);
		}
		return SB.ToString();
	}

	public static PronounSet ConstructFromName(string Name)
	{
		List<string> list = new List<string>(Name.Split('/'));
		PronounSet pronounSet = new PronounSet();
		if (list[0] == "plural")
		{
			pronounSet.Plural = true;
			list.RemoveAt(0);
		}
		if (list[0] == "pseudo-plural")
		{
			pronounSet.PseudoPlural = true;
			list.RemoveAt(0);
		}
		if (list[0] == "nonperson")
		{
			pronounSet.UseBareIndicative = true;
			list.RemoveAt(0);
		}
		if (list.Count == 11)
		{
			pronounSet.Subjective = list[0];
			pronounSet.Objective = list[1];
			pronounSet.PossessiveAdjective = list[2];
			pronounSet.SubstantivePossessive = list[3];
			pronounSet.Reflexive = list[4];
			pronounSet.PersonTerm = list[5];
			pronounSet.ImmaturePersonTerm = list[6];
			pronounSet.FormalAddressTerm = list[7];
			pronounSet.OffspringTerm = list[8];
			pronounSet.SiblingTerm = list[9];
			pronounSet.ParentTerm = list[10];
		}
		else
		{
			int num = 0;
			if (list.Count <= num)
			{
				return pronounSet;
			}
			pronounSet.Subjective = list[num++];
			if (list.Count <= num)
			{
				return pronounSet;
			}
			pronounSet.Objective = list[num++];
			if (list.Count <= num)
			{
				pronounSet.PossessiveAdjective = null;
				return pronounSet;
			}
			string value = (pronounSet.Plural ? "selves" : "self");
			if (list[num].EndsWith(value))
			{
				pronounSet.PossessiveAdjective = null;
				pronounSet.Reflexive = list[num++];
			}
			else
			{
				pronounSet.PossessiveAdjective = list[num++];
				if (list.Count <= num)
				{
					return pronounSet;
				}
				if (list[num].EndsWith(value))
				{
					pronounSet.Reflexive = list[num++];
				}
				else
				{
					pronounSet.SubstantivePossessive = list[num++];
					if (list.Count <= num)
					{
						return pronounSet;
					}
					pronounSet.Reflexive = list[num++];
				}
			}
			if (list.Count <= num)
			{
				return pronounSet;
			}
			pronounSet.PersonTerm = list[num++];
			if (list.Count <= num)
			{
				return pronounSet;
			}
			pronounSet.ImmaturePersonTerm = list[num++];
			if (list.Count <= num)
			{
				return pronounSet;
			}
			pronounSet.FormalAddressTerm = list[num++];
			if (list.Count <= num)
			{
				return pronounSet;
			}
			pronounSet.OffspringTerm = list[num++];
			if (list.Count <= num)
			{
				return pronounSet;
			}
			pronounSet.SiblingTerm = list[num++];
			if (list.Count <= num)
			{
				return pronounSet;
			}
			pronounSet.ParentTerm = list[num++];
		}
		return pronounSet;
	}

	public static void Clear()
	{
		PronounSets = null;
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
	public static void Reinit()
	{
		Clear();
		Loading.LoadTask("Loading PronounSets.xml", Init);
	}

	public static void Init()
	{
		if (PronounSets == null)
		{
			PronounSets = new Dictionary<string, PronounSet>(64);
		}
		TempPronounSets = new Dictionary<string, PronounSet>(16);
		string GenerateGenericSpec = null;
		bool ReplicateGenders = false;
		Action<string, bool> action = (string file, bool mod) =>
		{
			using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(file);
			xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
			while (xmlTextReader.Read())
			{
				if (xmlTextReader.Name == "pronounsets")
				{
					string attribute = xmlTextReader.GetAttribute("EnableSelection");
					if (!string.IsNullOrEmpty(attribute))
					{
						EnableSelection = attribute.EqualsNoCase("true");
					}
					string attribute2 = xmlTextReader.GetAttribute("EnableGeneration");
					if (!string.IsNullOrEmpty(attribute2))
					{
						EnableGeneration = attribute2.EqualsNoCase("true");
					}
					string attribute3 = xmlTextReader.GetAttribute("EnableConversationalExchange");
					if (!string.IsNullOrEmpty(attribute3))
					{
						EnableConversationalExchange = attribute3.EqualsNoCase("true");
					}
					string attribute4 = xmlTextReader.GetAttribute("GenerateGeneric");
					if (!string.IsNullOrEmpty(attribute4))
					{
						GenerateGenericSpec = attribute4;
					}
					string attribute5 = xmlTextReader.GetAttribute("ReplicateGenders");
					if (attribute5.EqualsNoCase("true"))
					{
						ReplicateGenders = true;
					}
					else if (attribute5.EqualsNoCase("false"))
					{
						ReplicateGenders = false;
					}
					LoadPronounSetsNode(xmlTextReader, mod);
				}
				if (xmlTextReader.NodeType == XmlNodeType.EndElement && xmlTextReader.Name == "pronounsets")
				{
					break;
				}
			}
			xmlTextReader.Close();
		};
		foreach (DataFile item in DataManager.GetXMLFilesWithRoot("PronounSets"))
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
		if (ReplicateGenders)
		{
			foreach (Gender item2 in Gender.GetAll())
			{
				if (!item2.DoNotReplicateAsPronounSet)
				{
					PronounSet pronounSet = new PronounSet(item2);
					if (!PronounSets.ContainsKey(pronounSet.Name))
					{
						PronounSets.Add(pronounSet.Name, pronounSet);
					}
				}
			}
		}
		SetDefaults(TempPronounSets);
		foreach (PronounSet value in TempPronounSets.Values)
		{
			if (!PronounSets.ContainsKey(value.Name))
			{
				PronounSets.Add(value.Name, value);
			}
		}
		TempPronounSets = null;
		if (string.IsNullOrEmpty(GenerateGenericSpec))
		{
			return;
		}
		int num = Stat.Roll(GenerateGenericSpec);
		int num2 = 0;
		foreach (string key in PronounSets.Keys)
		{
			PronounSet pronounSet2 = PronounSets[key];
			if (pronounSet2.Generic && pronounSet2.Generated && !pronounSet2.FromGender)
			{
				num2++;
			}
		}
		for (int num3 = num2; num3 < num; num3++)
		{
			Generate(Generic: true).Register();
		}
	}

	public static void LoadPronounSetsNode(XmlTextReader Reader, bool Mod = false)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "pronounset")
			{
				PronounSet pronounSet = new PronounSet(_Generic: true);
				string attribute = Reader.GetAttribute("Generic");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.Generic = attribute.EqualsNoCase("true");
				}
				attribute = Reader.GetAttribute("Plural");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.Plural = attribute.EqualsNoCase("true");
				}
				attribute = Reader.GetAttribute("PseudoPlural");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.PseudoPlural = attribute.EqualsNoCase("true");
				}
				attribute = Reader.GetAttribute("Subjective");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.Subjective = attribute;
				}
				attribute = Reader.GetAttribute("Objective");
				if (attribute != null)
				{
					pronounSet.Objective = ((attribute == "") ? null : attribute);
				}
				attribute = Reader.GetAttribute("PossessiveAdjective");
				if (attribute != null)
				{
					pronounSet.PossessiveAdjective = ((attribute == "") ? null : attribute);
				}
				attribute = Reader.GetAttribute("SubstantivePossessive");
				if (attribute != null)
				{
					pronounSet.SubstantivePossessive = ((attribute == "") ? null : attribute);
				}
				attribute = Reader.GetAttribute("Reflexive");
				if (attribute != null)
				{
					pronounSet.Reflexive = ((attribute == "") ? null : attribute);
				}
				attribute = Reader.GetAttribute("PersonTerm");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.PersonTerm = attribute;
				}
				attribute = Reader.GetAttribute("ImmaturePersonTerm");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.ImmaturePersonTerm = attribute;
				}
				attribute = Reader.GetAttribute("FormalAddressTerm");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.FormalAddressTerm = attribute;
				}
				attribute = Reader.GetAttribute("OffspringTerm");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.OffspringTerm = attribute;
				}
				attribute = Reader.GetAttribute("SiblingTerm");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.SiblingTerm = attribute;
				}
				attribute = Reader.GetAttribute("ParentTerm");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.ParentTerm = attribute;
				}
				attribute = Reader.GetAttribute("UseBareIndicative");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet.UseBareIndicative = attribute == "true";
				}
				attribute = Reader.GetAttribute("Name");
				if (!string.IsNullOrEmpty(attribute))
				{
					pronounSet._Name = attribute;
				}
				if (!TempPronounSets.ContainsKey(pronounSet.Name))
				{
					TempPronounSets.Add(pronounSet.Name, pronounSet);
				}
			}
			else if (Reader.Name == "removepronounset")
			{
				string attribute2 = Reader.GetAttribute("Name");
				if (string.IsNullOrEmpty(attribute2))
				{
					throw new Exception("removepronounset tag had no Name attribute");
				}
				if (PronounSets.ContainsKey(attribute2))
				{
					PronounSets.Remove(attribute2);
				}
				if (TempPronounSets.ContainsKey(attribute2))
				{
					TempPronounSets.Remove(attribute2);
				}
			}
		}
	}

	public PronounSet Register()
	{
		if (!PronounSets.ContainsKey(Name))
		{
			PronounSets.Add(Name, this);
			All = null;
			FlushRelevantCaches();
		}
		return this;
	}

	public void FlushRelevantCaches()
	{
		if (Generic)
		{
			if (Plural)
			{
				GenericPlurals = null;
			}
			else
			{
				GenericSingulars = null;
			}
			if (UseBareIndicative)
			{
				GenericNonpersonals = null;
				if (Plural)
				{
					GenericNonpersonalPlurals = null;
				}
				else
				{
					GenericNonpersonalSingulars = null;
				}
			}
			else
			{
				GenericPersonals = null;
				if (Plural)
				{
					GenericPersonalPlurals = null;
				}
				else
				{
					GenericPersonalSingulars = null;
				}
			}
			return;
		}
		if (Plural)
		{
			Plurals = null;
		}
		else
		{
			Singulars = null;
		}
		if (UseBareIndicative)
		{
			Nonpersonals = null;
			if (Plural)
			{
				NonpersonalPlurals = null;
			}
			else
			{
				NonpersonalSingulars = null;
			}
		}
		else
		{
			Personals = null;
			if (Plural)
			{
				PersonalPlurals = null;
			}
			else
			{
				PersonalSingulars = null;
			}
		}
	}

	public static PronounSet Get(string Name)
	{
		if (Name == null)
		{
			return null;
		}
		if (PronounSets.ContainsKey(Name))
		{
			return PronounSets[Name];
		}
		if (!Name.Contains("/"))
		{
			throw new Exception("pronoun set " + Name + " not known and does not appear to be an explicit specification");
		}
		PronounSet pronounSet = ConstructFromName(Name);
		if (!PronounSets.ContainsKey(pronounSet.Name))
		{
			pronounSet.Register();
		}
		return pronounSet;
	}

	public static PronounSet GetIfExists(string Name)
	{
		if (Name == null)
		{
			return null;
		}
		if (PronounSets.ContainsKey(Name))
		{
			return PronounSets[Name];
		}
		return null;
	}

	public static bool Exists(string Name)
	{
		return PronounSets.ContainsKey(Name);
	}

	public static PronounSet Generate(bool Generic = false, int PluralChance = 0, int UseBareIndicativeChance = 0, GameObject For = null, string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null)
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
		int num = 0;
		PronounSet pronounSet;
		do
		{
			if (++num > 1000)
			{
				throw new Exception("cannot generate pronouns set with unused name");
			}
			pronounSet = new PronounSet(Generic, _Generated: true);
			if (PluralChance.in100())
			{
				pronounSet.Plural = true;
			}
			if (UseBareIndicativeChance.in100())
			{
				pronounSet.UseBareIndicative = true;
			}
			Gender.GenerateBasePronouns(out var subjective, out var objective, out var possessive);
			pronounSet.Subjective = subjective;
			pronounSet.Objective = objective;
			pronounSet.PossessiveAdjective = possessive;
			if (!pronounSet.Plural && pronounSet.Subjective.EndsWith("y"))
			{
				pronounSet.PseudoPlural = true;
			}
		}
		while (PronounSets.ContainsKey(pronounSet.Name));
		NameMaker.MakeName(ref pronounSet._PersonTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderPersonTerm");
		NameMaker.MakeName(ref pronounSet._ImmaturePersonTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderImmaturePersonTerm");
		NameMaker.MakeName(ref pronounSet._FormalAddressTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderFormalAddressTerm");
		NameMaker.MakeName(ref pronounSet._OffspringTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderOffspringTerm");
		NameMaker.MakeName(ref pronounSet._SiblingTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderSiblingTerm");
		NameMaker.MakeName(ref pronounSet._ParentTerm, For, Genotype, Subtype, Species, Culture, Faction, null, null, null, null, "GenderParentTerm");
		return pronounSet;
	}

	public static List<PronounSet> Find(Predicate<PronounSet> Filter)
	{
		int num = 0;
		foreach (string key in PronounSets.Keys)
		{
			if (Filter(PronounSets[key]))
			{
				num++;
			}
		}
		List<PronounSet> list = new List<PronounSet>(num);
		if (num > 0)
		{
			foreach (string key2 in PronounSets.Keys)
			{
				PronounSet pronounSet = PronounSets[key2];
				if (Filter(pronounSet))
				{
					list.Add(pronounSet);
				}
			}
		}
		return list;
	}

	public static List<PronounSet> GetAll()
	{
		if (All == null)
		{
			All = Find((PronounSet PS) => true);
		}
		return All;
	}

	public static PronounSet GetAny()
	{
		return GetAll().GetRandomElement();
	}

	public static string GetAnyName()
	{
		return GetAny().Name;
	}

	public static List<PronounSet> GetAllGeneric()
	{
		if (Generics == null)
		{
			Generics = Find((PronounSet PS) => PS.Generic);
		}
		return Generics;
	}

	public static PronounSet GetAnyGeneric()
	{
		return GetAllGeneric().GetRandomElement();
	}

	public static string GetAnyGenericName()
	{
		return GetAnyGeneric().Name;
	}

	public static List<PronounSet> GetAllPlural()
	{
		if (Plurals == null)
		{
			Plurals = Find((PronounSet PS) => PS.Plural);
		}
		return Plurals;
	}

	public static PronounSet GetAnyPlural()
	{
		return GetAllPlural().GetRandomElement();
	}

	public static string GetAnyPluralName()
	{
		return GetAnyPlural().Name;
	}

	public static List<PronounSet> GetAllSingular()
	{
		if (Singulars == null)
		{
			Singulars = Find((PronounSet PS) => !PS.Plural);
		}
		return Singulars;
	}

	public static PronounSet GetAnySingular()
	{
		return GetAllSingular().GetRandomElement();
	}

	public static string GetAnySingularName()
	{
		return GetAnySingular().Name;
	}

	public static List<PronounSet> GetAllGenericSingular()
	{
		if (GenericSingulars == null)
		{
			GenericSingulars = Find((PronounSet PS) => PS.Generic && !PS.Plural);
		}
		return GenericSingulars;
	}

	public static PronounSet GetAnyGenericSingular()
	{
		return GetAllGenericSingular().GetRandomElement();
	}

	public static string GetAnyGenericSingularName()
	{
		return GetAnyGenericSingular().Name;
	}

	public static List<PronounSet> GetAllGenericPlural()
	{
		if (GenericPlurals == null)
		{
			GenericPlurals = Find((PronounSet PS) => PS.Generic && PS.Plural);
		}
		return GenericPlurals;
	}

	public static PronounSet GetAnyGenericPlural()
	{
		return GetAllGenericPlural().GetRandomElement();
	}

	public static string GetAnyGenericPluralName()
	{
		return GetAnyGenericPlural().Name;
	}

	public static List<PronounSet> GetAllPersonal()
	{
		if (Personals == null)
		{
			Personals = Find((PronounSet PS) => !PS.UseBareIndicative);
		}
		return Personals;
	}

	public static PronounSet GetAnyPersonal()
	{
		return GetAllPersonal().GetRandomElement();
	}

	public static string GetAnyPersonalName()
	{
		return GetAnyPersonal().Name;
	}

	public static List<PronounSet> GetAllPersonalPlural()
	{
		if (PersonalPlurals == null)
		{
			PersonalPlurals = Find((PronounSet PS) => !PS.UseBareIndicative && PS.Plural);
		}
		return PersonalPlurals;
	}

	public static PronounSet GetAnyPersonalPlural()
	{
		return GetAllPersonalPlural().GetRandomElement();
	}

	public static string GetAnyPersonalPluralName()
	{
		return GetAnyPersonalPlural().Name;
	}

	public static List<PronounSet> GetAllPersonalSingular()
	{
		if (PersonalSingulars == null)
		{
			PersonalSingulars = Find((PronounSet PS) => !PS.UseBareIndicative && !PS.Plural);
		}
		return PersonalSingulars;
	}

	public static PronounSet GetAnyPersonalSingular()
	{
		return GetAllPersonalSingular().GetRandomElement();
	}

	public static string GetAnyPersonalSingularName()
	{
		return GetAnyPersonalSingular().Name;
	}

	public static List<PronounSet> GetAllGenericPersonal()
	{
		if (GenericPersonals == null)
		{
			GenericPersonals = Find((PronounSet PS) => PS.Generic && !PS.UseBareIndicative);
		}
		return GenericPersonals;
	}

	public static PronounSet GetAnyGenericPersonal()
	{
		return GetAllGenericPersonal().GetRandomElement();
	}

	public static string GetAnyGenericPersonalName()
	{
		return GetAnyGenericPersonalSingular().Name;
	}

	public static List<PronounSet> GetAllGenericPersonalPlural()
	{
		if (GenericPersonalPlurals == null)
		{
			GenericPersonalPlurals = Find((PronounSet PS) => PS.Generic && !PS.UseBareIndicative && PS.Plural);
		}
		return GenericPersonalPlurals;
	}

	public static PronounSet GetAnyGenericPersonalPlural()
	{
		return GetAllGenericPersonalPlural().GetRandomElement();
	}

	public static string GetAnyGenericPersonalPluralName()
	{
		return GetAnyGenericPersonalSingular().Name;
	}

	public static List<PronounSet> GetAllGenericPersonalSingular()
	{
		if (GenericPersonalSingulars == null)
		{
			GenericPersonalSingulars = Find((PronounSet PS) => PS.Generic && !PS.UseBareIndicative && !PS.Plural);
		}
		return GenericPersonalSingulars;
	}

	public static PronounSet GetAnyGenericPersonalSingular()
	{
		return GetAllGenericPersonalSingular().GetRandomElement();
	}

	public static string GetAnyGenericPersonalSingularName()
	{
		return GetAnyGenericPersonalSingular().Name;
	}

	public static List<PronounSet> GetAllNonpersonal()
	{
		if (Nonpersonals == null)
		{
			Nonpersonals = Find((PronounSet PS) => PS.UseBareIndicative);
		}
		return Nonpersonals;
	}

	public static PronounSet GetAnyNonpersonal()
	{
		return GetAllNonpersonal().GetRandomElement();
	}

	public static string GetAnyNonpersonalName()
	{
		return GetAnyNonpersonal().Name;
	}

	public static List<PronounSet> GetAllNonpersonalPlural()
	{
		if (NonpersonalPlurals == null)
		{
			NonpersonalPlurals = Find((PronounSet PS) => PS.UseBareIndicative && PS.Plural);
		}
		return NonpersonalPlurals;
	}

	public static PronounSet GetAnyNonpersonalPlural()
	{
		return GetAllNonpersonalPlural().GetRandomElement();
	}

	public static string GetAnyNonpersonalPluralName()
	{
		return GetAnyNonpersonalPlural().Name;
	}

	public static List<PronounSet> GetAllNonpersonalSingular()
	{
		if (NonpersonalSingulars == null)
		{
			NonpersonalSingulars = Find((PronounSet PS) => PS.UseBareIndicative && !PS.Plural);
		}
		return NonpersonalSingulars;
	}

	public static PronounSet GetAnyNonpersonalSingular()
	{
		return GetAllNonpersonalSingular().GetRandomElement();
	}

	public static string GetAnyNonpersonalSingularName()
	{
		return GetAnyNonpersonalSingular().Name;
	}

	public static List<PronounSet> GetAllGenericNonpersonal()
	{
		if (GenericNonpersonals == null)
		{
			GenericNonpersonals = Find((PronounSet PS) => PS.Generic && PS.UseBareIndicative);
		}
		return GenericNonpersonals;
	}

	public static PronounSet GetAnyGenericNonpersonal()
	{
		return GetAllGenericNonpersonal().GetRandomElement();
	}

	public static string GetAnyGenericNonpersonalName()
	{
		return GetAnyGenericNonpersonalSingular().Name;
	}

	public static List<PronounSet> GetAllGenericNonpersonalPlural()
	{
		if (GenericNonpersonalPlurals == null)
		{
			GenericNonpersonalPlurals = Find((PronounSet PS) => PS.Generic && PS.UseBareIndicative && PS.Plural);
		}
		return GenericNonpersonalPlurals;
	}

	public static PronounSet GetAnyGenericNonpersonalPlural()
	{
		return GetAllGenericNonpersonalPlural().GetRandomElement();
	}

	public static string GetAnyGenericNonpersonalPluralName()
	{
		return GetAnyGenericNonpersonalSingular().Name;
	}

	public static List<PronounSet> GetAllGenericNonpersonalSingular()
	{
		if (GenericNonpersonalSingulars == null)
		{
			GenericNonpersonalSingulars = Find((PronounSet PS) => PS.Generic && PS.UseBareIndicative && !PS.Plural);
		}
		return GenericNonpersonalSingulars;
	}

	public static PronounSet GetAnyGenericNonpersonalSingular()
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
