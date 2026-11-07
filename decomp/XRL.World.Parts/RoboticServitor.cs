using System;

namespace XRL.World.Parts;

[Serializable]
public class RoboticServitor : IBondedCompanion
{
	public RoboticServitor()
	{
	}

	public RoboticServitor(GameObject ServitorOf = null, string Faction = null, string Honorific = null, string Title = null, string ConversationID = null, bool StripGear = false)
		: base(ServitorOf, Faction, Honorific, Title, ConversationID, StripGear)
	{
	}
}
