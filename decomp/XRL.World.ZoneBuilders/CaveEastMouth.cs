namespace XRL.World.ZoneBuilders;

public class CaveEastMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		Range = 3;
		return ConnectionMouth(Z, "Cave", "East");
	}
}
