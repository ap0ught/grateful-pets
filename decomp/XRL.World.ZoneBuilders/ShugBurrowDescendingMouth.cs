namespace XRL.World.ZoneBuilders;

public class ShugBurrowDescendingMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionStart(Z, "ShugBurrow", "DescendingMouth");
	}
}
