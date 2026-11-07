namespace XRL.World.ZoneBuilders;

public class RoadEastMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "Road", "East");
	}
}
