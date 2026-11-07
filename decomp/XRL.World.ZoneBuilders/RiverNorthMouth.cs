namespace XRL.World.ZoneBuilders;

public class RiverNorthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "River", "North");
	}
}
