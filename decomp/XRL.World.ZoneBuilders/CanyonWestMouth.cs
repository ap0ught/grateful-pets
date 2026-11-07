namespace XRL.World.ZoneBuilders;

public class CanyonWestMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "Canyon", "West");
	}
}
