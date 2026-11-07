using System;
using XRL.Messages;
using XRL.World;

namespace XRL;

[Serializable]
public class HolyPlaceSystem : IGameSystem
{
	public static Faction LastHolyFaction;

	public static string LastZoneID;

	public override bool WantFieldReflection => false;

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(ZoneActivatedEvent.ID);
		Registrar.Register(SingletonEvent<AfterGameLoadedEvent>.ID);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (E.Zone.IsWorldMap())
		{
			return base.HandleEvent(E);
		}
		if (E.Zone.ZoneID == LastZoneID)
		{
			return base.HandleEvent(E);
		}
		SetHolyZone(E.Zone, Factions.GetZoneHolyFaction(E.Zone.ZoneID));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		Zone activeZone = The.ActiveZone;
		if (activeZone != null)
		{
			SetHolyZone(activeZone, Factions.GetZoneHolyFaction(activeZone.ZoneID));
		}
		return base.HandleEvent(E);
	}

	public void SetHolyZone(Zone Z, Faction Faction = null)
	{
		if (Faction != null && Faction != LastHolyFaction)
		{
			MessageQueue.AddPlayerMessage("You feel a sense of holiness here.");
		}
		LastHolyFaction = Faction;
		LastZoneID = Z.ZoneID;
	}
}
