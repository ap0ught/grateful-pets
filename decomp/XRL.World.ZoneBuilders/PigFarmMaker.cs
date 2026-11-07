namespace XRL.World.ZoneBuilders;

public class PigFarmMaker
{
	public bool BuildZone(Zone Z)
	{
		return new PigFarm().BuildZone(Z, ClearCombatObjectsFirst: true, null, null, null);
	}
}
