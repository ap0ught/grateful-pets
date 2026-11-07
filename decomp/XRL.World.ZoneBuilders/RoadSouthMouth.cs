namespace XRL.World.ZoneBuilders;

public class RoadSouthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "Road", "South");
	}
}
