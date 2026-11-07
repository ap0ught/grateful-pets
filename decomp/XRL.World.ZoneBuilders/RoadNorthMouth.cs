namespace XRL.World.ZoneBuilders;

public class RoadNorthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "Road", "North");
	}
}
