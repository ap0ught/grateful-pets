namespace XRL.World.ZoneBuilders;

public class MinorRazedGoatfolkVillage
{
	public bool BuildZone(Zone Z)
	{
		return new VillageMaker().BuildZone(Z, bRoads: true, "ThatchedWall", RoundBuildings: true, "1", null, "GoatfolkVillageYurtMinorRazed", "GoatfolkVillageGlobalsMinorRazed", null, ClearCombatObjectsFirst: true);
	}
}
