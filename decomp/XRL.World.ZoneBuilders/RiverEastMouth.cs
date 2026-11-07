namespace XRL.World.ZoneBuilders;

public class RiverEastMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "River", "East");
	}
}
