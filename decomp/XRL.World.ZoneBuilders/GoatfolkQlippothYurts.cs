namespace XRL.World.ZoneBuilders;

public class GoatfolkQlippothYurts
{
	public bool BuildZone(Zone Z)
	{
		return new VillageMaker().BuildZone(Z, bRoads: true, "HolographicThatchedWall", RoundBuildings: true, "1-3", "3-QlippothBloodCistern", "GoatfolkQlippothVillageYurt", "GoatfolkQlippothParty", null, ClearCombatObjectsFirst: true);
	}
}
