using XRL.World.ZoneBuilders;

namespace XRL.World.Encounters.EncounterBuilders;

public class StairsDown
{
	public string x = "-1";

	public string y = "-1";

	public bool BuildEncounter(Zone NewZone)
	{
		return new XRL.World.ZoneBuilders.StairsDown
		{
			x = x,
			y = y
		}.BuildZone(NewZone);
	}
}
