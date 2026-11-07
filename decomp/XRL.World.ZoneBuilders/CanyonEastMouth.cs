namespace XRL.World.ZoneBuilders;

public class CanyonEastMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "Canyon", "East");
	}
}
