using System;
using System.Threading.Tasks;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World;

[Serializable]
public abstract class BasePronounProvider : IPronounProvider
{
	public bool _Generic;

	public bool _Generated;

	public bool _Plural;

	public bool _PseudoPlural;

	public string _Subjective = "they";

	[NonSerialized]
	public string _CapitalizedSubjective = "They";

	public string _Objective = "them";

	[NonSerialized]
	public string _CapitalizedObjective = "Them";

	public string _PossessiveAdjective = "their";

	[NonSerialized]
	public string _CapitalizedPossessiveAdjective = "Their";

	public string _SubstantivePossessive;

	[NonSerialized]
	public string _CapitalizedSubstantivePossessive;

	public string _Reflexive;

	[NonSerialized]
	public string _CapitalizedReflexive;

	public string _PersonTerm = "human";

	[NonSerialized]
	public string _CapitalizedPersonTerm = "Human";

	public string _ImmaturePersonTerm = "child";

	[NonSerialized]
	public string _CapitalizedImmaturePersonTerm = "Child";

	public string _FormalAddressTerm = "friend";

	[NonSerialized]
	public string _CapitalizedFormalAddressTerm;

	public string _OffspringTerm = "child";

	[NonSerialized]
	public string _CapitalizedOffspringTerm;

	public string _SiblingTerm = "sib";

	[NonSerialized]
	public string _CapitalizedSiblingTerm;

	public string _ParentTerm = "progenitor";

	[NonSerialized]
	public string _CapitalizedParentTerm;

	public bool _UseBareIndicative;

	public abstract string Name { get; }

	public abstract string CapitalizedName { get; }

	public bool Generic
	{
		get
		{
			return _Generic;
		}
		set
		{
			_Generic = value;
			ConfigurationUpdated(Categorizing: true);
		}
	}

	public bool Generated
	{
		get
		{
			return _Generated;
		}
		set
		{
			_Generated = value;
			ConfigurationUpdated();
		}
	}

	public bool Plural
	{
		get
		{
			return _Plural;
		}
		set
		{
			_Plural = value;
			ConfigurationUpdated(Categorizing: true);
		}
	}

	public bool PseudoPlural
	{
		get
		{
			return _PseudoPlural;
		}
		set
		{
			_PseudoPlural = value;
			ConfigurationUpdated(Categorizing: true);
		}
	}

	public string Subjective
	{
		get
		{
			return _Subjective;
		}
		set
		{
			_Subjective = value;
			_CapitalizedSubjective = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedSubjective
	{
		get
		{
			if (_CapitalizedSubjective == null)
			{
				_CapitalizedSubjective = ColorUtility.CapitalizeExceptFormatting(Subjective);
			}
			return _CapitalizedSubjective;
		}
	}

	public string Objective
	{
		get
		{
			if (_Objective == null)
			{
				_Objective = Subjective;
			}
			return _Objective;
		}
		set
		{
			_Objective = value;
			_CapitalizedObjective = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedObjective
	{
		get
		{
			if (_CapitalizedObjective == null)
			{
				_CapitalizedObjective = ColorUtility.CapitalizeExceptFormatting(Objective);
			}
			return _CapitalizedObjective;
		}
	}

	public string PossessiveAdjective
	{
		get
		{
			if (_PossessiveAdjective == null)
			{
				_PossessiveAdjective = ExpectedPossessiveAdjective();
			}
			return _PossessiveAdjective;
		}
		set
		{
			_PossessiveAdjective = value;
			_CapitalizedPossessiveAdjective = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedPossessiveAdjective
	{
		get
		{
			if (_CapitalizedPossessiveAdjective == null)
			{
				_CapitalizedPossessiveAdjective = ColorUtility.CapitalizeExceptFormatting(PossessiveAdjective);
			}
			return _CapitalizedPossessiveAdjective;
		}
	}

	public string SubstantivePossessive
	{
		get
		{
			if (_SubstantivePossessive == null)
			{
				_SubstantivePossessive = ExpectedSubstantivePossessive();
			}
			return _SubstantivePossessive;
		}
		set
		{
			_SubstantivePossessive = value;
			_CapitalizedSubstantivePossessive = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedSubstantivePossessive
	{
		get
		{
			if (_CapitalizedSubstantivePossessive == null)
			{
				_CapitalizedSubstantivePossessive = ColorUtility.CapitalizeExceptFormatting(SubstantivePossessive);
			}
			return _CapitalizedSubstantivePossessive;
		}
	}

	public string Reflexive
	{
		get
		{
			if (_Reflexive == null)
			{
				_Reflexive = ExpectedReflexive();
			}
			return _Reflexive;
		}
		set
		{
			_Reflexive = value;
			_CapitalizedReflexive = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedReflexive
	{
		get
		{
			if (_CapitalizedReflexive == null)
			{
				_CapitalizedReflexive = ColorUtility.CapitalizeExceptFormatting(Reflexive);
			}
			return _CapitalizedReflexive;
		}
	}

	public string PersonTerm
	{
		get
		{
			return _PersonTerm;
		}
		set
		{
			_PersonTerm = value;
			_CapitalizedPersonTerm = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedPersonTerm
	{
		get
		{
			if (_CapitalizedPersonTerm == null)
			{
				_CapitalizedPersonTerm = ColorUtility.CapitalizeExceptFormatting(PersonTerm);
			}
			return _CapitalizedPersonTerm;
		}
	}

	public string ImmaturePersonTerm
	{
		get
		{
			return _ImmaturePersonTerm;
		}
		set
		{
			_ImmaturePersonTerm = value;
			_CapitalizedImmaturePersonTerm = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedImmaturePersonTerm
	{
		get
		{
			if (_CapitalizedImmaturePersonTerm == null)
			{
				_CapitalizedImmaturePersonTerm = ColorUtility.CapitalizeExceptFormatting(ImmaturePersonTerm);
			}
			return _CapitalizedImmaturePersonTerm;
		}
	}

	public string FormalAddressTerm
	{
		get
		{
			if (_FormalAddressTerm == null)
			{
				_FormalAddressTerm = Objective;
			}
			return _FormalAddressTerm;
		}
		set
		{
			_FormalAddressTerm = value;
			_CapitalizedFormalAddressTerm = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedFormalAddressTerm
	{
		get
		{
			if (_CapitalizedFormalAddressTerm == null)
			{
				_CapitalizedFormalAddressTerm = ColorUtility.CapitalizeExceptFormatting(FormalAddressTerm);
			}
			return _CapitalizedFormalAddressTerm;
		}
	}

	public string OffspringTerm
	{
		get
		{
			if (_OffspringTerm == null)
			{
				_OffspringTerm = Objective;
			}
			return _OffspringTerm;
		}
		set
		{
			_OffspringTerm = value;
			_CapitalizedOffspringTerm = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedOffspringTerm
	{
		get
		{
			if (_CapitalizedOffspringTerm == null)
			{
				_CapitalizedOffspringTerm = ColorUtility.CapitalizeExceptFormatting(OffspringTerm);
			}
			return _CapitalizedOffspringTerm;
		}
	}

	public string SiblingTerm
	{
		get
		{
			if (_SiblingTerm == null)
			{
				_SiblingTerm = Objective;
			}
			return _SiblingTerm;
		}
		set
		{
			_SiblingTerm = value;
			_CapitalizedSiblingTerm = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedSiblingTerm
	{
		get
		{
			if (_CapitalizedSiblingTerm == null)
			{
				_CapitalizedSiblingTerm = ColorUtility.CapitalizeExceptFormatting(SiblingTerm);
			}
			return _CapitalizedSiblingTerm;
		}
	}

	public string ParentTerm
	{
		get
		{
			if (_ParentTerm == null)
			{
				_ParentTerm = Objective;
			}
			return _ParentTerm;
		}
		set
		{
			_ParentTerm = value;
			_CapitalizedParentTerm = null;
			ConfigurationUpdated();
		}
	}

	public string CapitalizedParentTerm
	{
		get
		{
			if (_CapitalizedParentTerm == null)
			{
				_CapitalizedParentTerm = ColorUtility.CapitalizeExceptFormatting(ParentTerm);
			}
			return _CapitalizedParentTerm;
		}
	}

	public string IndicativeProximal
	{
		get
		{
			if (!Plural)
			{
				return "this";
			}
			return "these";
		}
	}

	public string CapitalizedIndicativeProximal
	{
		get
		{
			if (!Plural)
			{
				return "This";
			}
			return "These";
		}
	}

	public string IndicativeDistal
	{
		get
		{
			if (!Plural)
			{
				return "that";
			}
			return "those";
		}
	}

	public string CapitalizedIndicativeDistal
	{
		get
		{
			if (!Plural)
			{
				return "That";
			}
			return "Those";
		}
	}

	public bool UseBareIndicative
	{
		get
		{
			return _UseBareIndicative;
		}
		set
		{
			_UseBareIndicative = value;
			ConfigurationUpdated(Categorizing: true);
		}
	}

	protected virtual void ConfigurationUpdated(bool Categorizing = false)
	{
	}

	public BasePronounProvider(bool _Generic = false, bool _Generated = false, bool _Plural = false, bool _PseudoPlural = false, string _Subjective = null, string _Objective = null, string _PossessiveAdjective = null, string _SubstantivePossessive = null, string _Reflexive = null, string _PersonTerm = null, string _ImmaturePersonTerm = null, string _FormalAddressTerm = null, string _OffspringTerm = null, string _SiblingTerm = null, string _ParentTerm = null, bool _UseBareIndicative = false)
	{
		Generic = _Generic;
		Generated = _Generated;
		Plural = _Plural;
		PseudoPlural = _PseudoPlural;
		if (_Subjective != null)
		{
			Subjective = _Subjective;
		}
		if (_Objective != null)
		{
			Objective = _Objective;
		}
		if (_PossessiveAdjective != null)
		{
			PossessiveAdjective = _PossessiveAdjective;
		}
		if (_SubstantivePossessive != null)
		{
			SubstantivePossessive = _SubstantivePossessive;
		}
		if (_Reflexive != null)
		{
			Reflexive = _Reflexive;
		}
		if (_PersonTerm != null)
		{
			PersonTerm = _PersonTerm;
		}
		if (_ImmaturePersonTerm != null)
		{
			PersonTerm = _PersonTerm;
		}
		if (_FormalAddressTerm != null)
		{
			FormalAddressTerm = _FormalAddressTerm;
		}
		if (_OffspringTerm != null)
		{
			OffspringTerm = _OffspringTerm;
		}
		if (_SiblingTerm != null)
		{
			SiblingTerm = _SiblingTerm;
		}
		if (_ParentTerm != null)
		{
			ParentTerm = _ParentTerm;
		}
		UseBareIndicative = _UseBareIndicative;
	}

	public BasePronounProvider(BasePronounProvider Original)
	{
		CopyFrom(Original);
	}

	public virtual void CopyFrom(BasePronounProvider Original)
	{
		_Generic = Original._Generic;
		_Generated = Original._Generated;
		_Plural = Original._Plural;
		_PseudoPlural = Original._PseudoPlural;
		_Subjective = Original._Subjective;
		_Objective = Original._Objective;
		_PossessiveAdjective = Original._PossessiveAdjective;
		_SubstantivePossessive = Original._SubstantivePossessive;
		_Reflexive = Original._Reflexive;
		_PersonTerm = Original._PersonTerm;
		_ImmaturePersonTerm = Original._ImmaturePersonTerm;
		_FormalAddressTerm = Original._FormalAddressTerm;
		_OffspringTerm = Original._OffspringTerm;
		_SiblingTerm = Original._SiblingTerm;
		_ParentTerm = Original._ParentTerm;
		_UseBareIndicative = Original._UseBareIndicative;
		ConfigurationUpdated(Categorizing: true);
	}

	public abstract BasePronounProvider Clone();

	public virtual string ExpectedPossessiveAdjective()
	{
		return Objective;
	}

	public virtual string ExpectedSubstantivePossessive()
	{
		if (!PossessiveAdjective.EndsWith("s") && !PossessiveAdjective.EndsWith("z"))
		{
			return PossessiveAdjective + "s";
		}
		return PossessiveAdjective;
	}

	public virtual string ExpectedReflexive()
	{
		return Objective + (Plural ? "selves" : "self");
	}

	public virtual void Save(SerializationWriter Writer)
	{
		Writer.Write(_Generic);
		Writer.Write(_Generated);
		Writer.Write(_Plural);
		Writer.Write(_PseudoPlural);
		Writer.Write(_Subjective);
		Writer.Write(_Objective);
		Writer.Write(_PossessiveAdjective);
		Writer.Write(_SubstantivePossessive);
		Writer.Write(_Reflexive);
		Writer.Write(_PersonTerm);
		Writer.Write(_ImmaturePersonTerm);
		Writer.Write(_FormalAddressTerm);
		Writer.Write(_OffspringTerm);
		Writer.Write(_SiblingTerm);
		Writer.Write(_ParentTerm);
		Writer.Write(_UseBareIndicative);
	}

	public static void Load(SerializationReader Reader, BasePronounProvider Obj)
	{
		Obj._Generic = Reader.ReadBoolean();
		Obj._Generated = Reader.ReadBoolean();
		Obj._Plural = Reader.ReadBoolean();
		Obj._PseudoPlural = Reader.ReadBoolean();
		Obj._Subjective = Reader.ReadString();
		Obj._CapitalizedSubjective = null;
		Obj._Objective = Reader.ReadString();
		Obj._CapitalizedObjective = null;
		Obj._PossessiveAdjective = Reader.ReadString();
		Obj._CapitalizedPossessiveAdjective = null;
		Obj._SubstantivePossessive = Reader.ReadString();
		Obj._CapitalizedSubstantivePossessive = null;
		Obj._Reflexive = Reader.ReadString();
		Obj._CapitalizedReflexive = null;
		Obj._PersonTerm = Reader.ReadString();
		Obj._CapitalizedPersonTerm = null;
		Obj._ImmaturePersonTerm = Reader.ReadString();
		Obj._CapitalizedImmaturePersonTerm = null;
		Obj._FormalAddressTerm = Reader.ReadString();
		Obj._CapitalizedFormalAddressTerm = null;
		Obj._OffspringTerm = Reader.ReadString();
		Obj._CapitalizedOffspringTerm = null;
		Obj._SiblingTerm = Reader.ReadString();
		Obj._CapitalizedSiblingTerm = null;
		Obj._ParentTerm = Reader.ReadString();
		Obj._CapitalizedParentTerm = null;
		Obj._UseBareIndicative = Reader.ReadBoolean();
	}

	public abstract Task<bool> CustomizeAsync();

	protected async Task<bool> Customize(string What)
	{
		BasePronounProvider Temp = Clone();
		if (await CustomizeProcess(What))
		{
			return true;
		}
		CopyFrom(Temp);
		return false;
	}

	protected virtual async Task<bool> CustomizeProcess(string What)
	{
		_Generic = false;
		_Generated = false;
		DialogResult dialogResult = await Popup.ShowYesNoCancelAsync("Should your " + What + " be treated as fully plural, with you being addressed as a multiple subject in all circumstances?");
		if (dialogResult == DialogResult.Cancel)
		{
			return false;
		}
		_Plural = dialogResult == DialogResult.Yes;
		if (_Plural)
		{
			_PseudoPlural = false;
		}
		else
		{
			_PseudoPlural = await Popup.ShowYesNoCancelAsync("Should your " + What + " be treated as conditionally plural, with you being addressed as a multiple subject only following a pronoun, as with with singular \"they\"?") == DialogResult.Yes;
		}
		_Subjective = await Popup.AskStringAsync("What subjective pronoun (he, she, they, etc.) should be used for this " + What + "?", Subjective, 6, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_Subjective))
		{
			return false;
		}
		_Objective = await Popup.AskStringAsync("What objective pronoun (him, her, them, etc.) should be used for this " + What + "?", Objective, 6, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_Objective))
		{
			return false;
		}
		_PossessiveAdjective = await Popup.AskStringAsync("What possessive adjective (his, her, their, etc.) should be used for this " + What + "?", PossessiveAdjective, 6, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_PossessiveAdjective))
		{
			return false;
		}
		_SubstantivePossessive = null;
		_SubstantivePossessive = await Popup.AskStringAsync("What substantive possessive (his, hers, theirs, etc.) should be used for this " + What + "?", SubstantivePossessive, 6, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_SubstantivePossessive))
		{
			return false;
		}
		_Reflexive = null;
		_Reflexive = await Popup.AskStringAsync("What reflexive pronoun (himself, herself, themself, themselves, etc.) should be used for this " + What + "?", Reflexive, 12, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_Reflexive))
		{
			return false;
		}
		_PersonTerm = await Popup.AskStringAsync("What term should be used for a mature person of this " + What + "? (Man, woman, person, etc.)", PersonTerm, 12, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_PersonTerm))
		{
			return false;
		}
		_ImmaturePersonTerm = await Popup.AskStringAsync("What term should be used for an immature person of this " + What + "? (Boy, girl, child, etc.)", ImmaturePersonTerm, 12, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_ImmaturePersonTerm))
		{
			return false;
		}
		_FormalAddressTerm = await Popup.AskStringAsync("What term should be used to formally address a person of this " + What + "? (Sir, madam, friend, etc.)", FormalAddressTerm, 12, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_FormalAddressTerm))
		{
			return false;
		}
		_OffspringTerm = await Popup.AskStringAsync("What term should be used to address a person of this " + What + " as an offspring? (Son, daughter, child, etc.)", OffspringTerm, 12, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_OffspringTerm))
		{
			return false;
		}
		_SiblingTerm = await Popup.AskStringAsync("What term should be used to address a person of this " + What + " as a sibling? (Brother, sister, sib, etc.)", SiblingTerm, 12, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_SiblingTerm))
		{
			return false;
		}
		_ParentTerm = await Popup.AskStringAsync("What term should be used to address a person of this " + What + " as a parent? (Father, mother, progenitor, etc.)", ParentTerm, 12, 0, "abcdefghijklmnopqrstuvwxyz");
		if (string.IsNullOrEmpty(_ParentTerm))
		{
			return false;
		}
		dialogResult = await Popup.ShowYesNoCancelAsync("Is an entity with this " + What + " treated grammatically as a person, such that it would be improper to say \"look at " + IndicativeDistal + "\" in reference to " + Objective + " -- one would say \"look at " + IndicativeDistal + " " + PersonTerm + "\" or \"look at " + Objective + "\" instead?");
		if (dialogResult == DialogResult.Cancel)
		{
			return false;
		}
		_UseBareIndicative = dialogResult != DialogResult.Yes;
		return true;
	}
}
