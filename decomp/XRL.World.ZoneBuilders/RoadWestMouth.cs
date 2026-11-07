namespace XRL.World.ZoneBuilders;

public class RoadWestMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "Road", "West");
	}
}
