using System;

namespace XRL.World.Parts;

[Serializable]
public class Companion : IBondedCompanion
{
	public Companion()
	{
	}

	public Companion(GameObject CompanionOf = null, string Faction = null, string Honorific = null, string Title = null, string ConversationID = null, bool StripGear = false)
		: base(CompanionOf, Faction, Honorific, Title, ConversationID, StripGear)
	{
	}
}
