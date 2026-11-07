namespace XRL.World.ZoneBuilders;

public class FungalTrailSouthMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionMouth(Z, "FungalTrail", "South");
	}
}
