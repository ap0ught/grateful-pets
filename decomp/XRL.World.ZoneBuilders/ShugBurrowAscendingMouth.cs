namespace XRL.World.ZoneBuilders;

public class ShugBurrowAscendingMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionStart(Z, "ShugBurrow", "AscendingMouth");
	}
}
