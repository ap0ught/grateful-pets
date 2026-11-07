namespace XRL.World.ZoneBuilders;

public class FungalTrailNorthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "FungalTrail", "North");
	}
}
