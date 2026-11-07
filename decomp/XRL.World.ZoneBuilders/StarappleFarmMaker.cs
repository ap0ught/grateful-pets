namespace XRL.World.ZoneBuilders;

public class StarappleFarmMaker
{
	public bool BuildZone(Zone Z)
	{
		return new StarappleFarm().BuildZone(Z, ClearCombatObjectsFirst: true, null, null, null);
	}
}
