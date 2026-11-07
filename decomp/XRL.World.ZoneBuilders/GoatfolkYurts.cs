namespace XRL.World.ZoneBuilders;

public class GoatfolkYurts
{
	public bool BuildZone(Zone Z)
	{
		return new VillageMaker().BuildZone(Z, bRoads: true, "ThatchedWall", RoundBuildings: true, "1d6", "5-Cistern", "GoatfolkVillageYurt", "GoatfolkParty", null, ClearCombatObjectsFirst: true);
	}
}
