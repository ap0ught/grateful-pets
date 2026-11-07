namespace XRL.World.ZoneBuilders;

public class RiverSouthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "River", "South");
	}
}
