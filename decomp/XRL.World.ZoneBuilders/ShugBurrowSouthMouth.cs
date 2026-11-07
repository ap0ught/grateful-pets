namespace XRL.World.ZoneBuilders;

public class ShugBurrowSouthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		Range = 3;
		return ConnectionMouth(Z, "ShugBurrow", "South");
	}
}
