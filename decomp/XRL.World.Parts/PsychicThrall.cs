using System;

namespace XRL.World.Parts;

[Serializable]
public class PsychicThrall : IBondedCompanion
{
	public PsychicThrall()
	{
	}

	public PsychicThrall(GameObject EnthralledBy = null, string Faction = "Seekers", string Honorific = null, string Title = "psychic thrall", string ConversationID = "PsychicThrall", bool StripGear = false)
		: base(EnthralledBy, Faction, Honorific, Title, ConversationID, StripGear)
	{
	}
}
