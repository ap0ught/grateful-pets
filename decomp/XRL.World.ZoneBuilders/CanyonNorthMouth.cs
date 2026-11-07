namespace XRL.World.ZoneBuilders;

public class CanyonNorthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "Canyon", "North");
	}
}
