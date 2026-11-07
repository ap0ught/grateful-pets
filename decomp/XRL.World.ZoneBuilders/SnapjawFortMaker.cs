namespace XRL.World.ZoneBuilders;

public class SnapjawFortMaker
{
	public bool BuildZone(Zone Z)
	{
		return new FortMaker().BuildZone(Z, ClearCombatObjectsFirst: true, "BrinestalkWall", "SnapjawFortGlobals", null);
	}
}
