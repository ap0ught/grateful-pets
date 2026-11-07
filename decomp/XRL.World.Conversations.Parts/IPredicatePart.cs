using System.Collections.Generic;

namespace XRL.World.Conversations.Parts;

/// <summary>Base consumer of predicates to satisfy condition for a derived action.</summary>
/// <seealso cref="T:XRL.World.Conversations.Parts.ChangeTarget" />
public abstract class IPredicatePart : IConversationPart
{
	public Dictionary<string, string> Predicates;

	public override void LoadAttributes(Dictionary<string, string> Attributes)
	{
		foreach (KeyValuePair<string, string> Attribute in Attributes)
		{
			if (ConversationDelegates.Predicates.ContainsKey(Attribute.Key))
			{
				if (Predicates == null)
				{
					Predicates = new Dictionary<string, string>();
				}
				Predicates[Attribute.Key] = Attribute.Value;
			}
		}
		base.LoadAttributes(Attributes);
	}

	public virtual bool Check(bool AnyPredicate = false, bool? Default = null)
	{
		if (Default.HasValue)
		{
			if (!AnyPredicate)
			{
				return All(Default.Value);
			}
			return Any(Default.Value);
		}
		if (!AnyPredicate)
		{
			return All();
		}
		return Any();
	}

	public virtual bool All(bool Default = true)
	{
		if (Predicates.IsNullOrEmpty())
		{
			return Default;
		}
		foreach (KeyValuePair<string, string> predicate in Predicates)
		{
			if (!ParentElement.CheckPredicate(predicate.Key, predicate.Value))
			{
				return false;
			}
		}
		return Default;
	}

	public virtual bool Any(bool Default = false)
	{
		if (Predicates.IsNullOrEmpty())
		{
			return Default;
		}
		foreach (KeyValuePair<string, string> predicate in Predicates)
		{
			if (ParentElement.CheckPredicate(predicate.Key, predicate.Value, Default: false))
			{
				return true;
			}
		}
		return Default;
	}
}
