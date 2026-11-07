namespace XRL.World.ZoneBuilders;

public class RiverWestMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "River", "West");
	}
}
