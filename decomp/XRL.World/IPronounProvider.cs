namespace XRL.World;

public interface IPronounProvider
{
	/// The name of the category providing the pronouns: male, female, neuter, nonspecific, masculine, feminine, etc.
	string Name { get; }

	string CapitalizedName { get; }

	/// Whether the pronoun-providing category is generic to the world, as opposed to being specific to an entity, species,
	/// or culture.
	bool Generic { get; }

	/// Whether the pronoun-providing category is procedurally generated.
	bool Generated { get; }

	/// Whether the entity is plural.
	bool Plural { get; }

	/// Whether the entity is treated plural only when being referred to by pronoun.
	bool PseudoPlural { get; }

	/// Subjective-case personal pronoun: he, she, it, they, etc.
	string Subjective { get; }

	string CapitalizedSubjective { get; }

	/// Objective-case personal pronoun: him, her, it, them, etc.
	string Objective { get; }

	string CapitalizedObjective { get; }

	/// Adjectival possessive pronoun: his, her, its, their, etc.
	string PossessiveAdjective { get; }

	string CapitalizedPossessiveAdjective { get; }

	/// Substantive possessive pronoun: his, hers, its, theirs, etc.
	string SubstantivePossessive { get; }

	string CapitalizedSubstantivePossessive { get; }

	/// Reflexive personal pronoun: himself, herself, itself, themselves, etc.
	string Reflexive { get; }

	string CapitalizedReflexive { get; }

	/// The term for a mature person with the pronouns: man, woman, etc.
	string PersonTerm { get; }

	string CapitalizedPersonTerm { get; }

	/// The term for an immature person with the pronouns: boy, girl, etc.
	string ImmaturePersonTerm { get; }

	string CapitalizedImmaturePersonTerm { get; }

	/// Formal form of address: sir, madam, etc.
	string FormalAddressTerm { get; }

	string CapitalizedFormalAddressTerm { get; }

	/// Term for entity as offspring: son, daughter, etc.
	string OffspringTerm { get; }

	string CapitalizedOffspringTerm { get; }

	/// Term for entity as sibling: brother, sister, etc.
	string SiblingTerm { get; }

	string CapitalizedSiblingTerm { get; }

	/// Term for entity as parent: father, mother, etc.
	string ParentTerm { get; }

	string CapitalizedParentTerm { get; }

	/// Proximal indicative pronoun: this, these.
	string IndicativeProximal { get; }

	string CapitalizedIndicativeProximal { get; }

	/// Distal indicative pronoun: that, those.
	string IndicativeDistal { get; }

	string CapitalizedIndicativeDistal { get; }

	/// Whether it is acceptable to use a bare indicative pronoun, for example
	/// saying "look at that".  (This would be objectifying and insulting if
	/// referring to a person; if one isn't being intentionally hostile one
	/// would say either "look at him" or "look at that man" instead.)
	bool UseBareIndicative { get; }
}
