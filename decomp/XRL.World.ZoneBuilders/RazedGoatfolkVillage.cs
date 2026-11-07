namespace XRL.World.ZoneBuilders;

public class RazedGoatfolkVillage
{
	public bool BuildZone(Zone Z)
	{
		return new VillageMaker().BuildZone(Z, bRoads: true, "ThatchedWall", RoundBuildings: true, "6", "5-BloodCistern", "GoatfolkVillageYurtRazed", "GoatfolkVillageGlobalsRazed", null, ClearCombatObjectsFirst: true);
	}
}
