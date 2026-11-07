namespace XRL.World.ZoneBuilders;

public class CanyonSouthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "Canyon", "South");
	}
}
