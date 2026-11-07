namespace XRL.World.ZoneBuilders;

public class RoadStartMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionStart(Z, "Road");
	}
}
