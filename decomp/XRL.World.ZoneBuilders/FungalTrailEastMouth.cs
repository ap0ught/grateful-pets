namespace XRL.World.ZoneBuilders;

public class FungalTrailEastMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "FungalTrail", "East");
	}
}
