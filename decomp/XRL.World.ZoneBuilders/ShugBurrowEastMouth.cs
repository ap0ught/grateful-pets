namespace XRL.World.ZoneBuilders;

public class ShugBurrowEastMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		Range = 3;
		return ConnectionMouth(Z, "ShugBurrow", "East");
	}
}
