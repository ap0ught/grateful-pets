using System;

namespace XRL.World.Parts;

[Serializable]
public class FriendorFoe
{
	public string faction;

	public string status;

	public string reason;

	public FriendorFoe()
	{
	}

	public FriendorFoe(string faction, string status, string reason)
	{
		this.faction = faction;
		this.status = status;
		this.reason = reason;
	}

	public FriendorFoe(FriendorFoe p)
	{
		faction = p.faction;
		status = p.status;
		reason = p.reason;
	}
}
