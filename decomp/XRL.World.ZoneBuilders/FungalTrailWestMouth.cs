namespace XRL.World.ZoneBuilders;

public class FungalTrailWestMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "FungalTrail", "West");
	}
}
