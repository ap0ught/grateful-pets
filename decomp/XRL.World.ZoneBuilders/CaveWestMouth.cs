namespace XRL.World.ZoneBuilders;

public class CaveWestMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		Range = 3;
		return ConnectionMouth(Z, "Cave", "West");
	}
}
