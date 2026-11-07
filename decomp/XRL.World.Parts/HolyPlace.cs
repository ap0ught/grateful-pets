using System;

namespace XRL.World.Parts;

[Serializable]
public class HolyPlace : IPart
{
	public string Faction;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!Faction.IsNullOrEmpty())
		{
			Faction ifExists = Factions.GetIfExists(Faction);
			if (ifExists != null)
			{
				The.Game.GetSystem<HolyPlaceSystem>()?.SetHolyZone(ParentObject.CurrentZone, ifExists);
			}
		}
		return base.HandleEvent(E);
	}
}
