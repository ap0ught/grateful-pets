using System;

namespace XRL.World.Parts;

[Serializable]
public class HiredGuard : IBondedCompanion
{
	public HiredGuard()
	{
	}

	public HiredGuard(GameObject HiredBy = null, string Faction = "Merchants", string Honorific = null, string Title = "hired guard", string ConversationID = "MerchantGuard", bool StripGear = false)
		: base(HiredBy, Faction, Honorific, Title, ConversationID, StripGear)
	{
	}
}
