namespace XRL.World.ZoneBuilders;

public class ShugBurrowWestMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		Range = 3;
		return ConnectionMouth(Z, "ShugBurrow", "West");
	}
}
