namespace XRL.World.ZoneBuilders;

public class FungalTrailStartMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionStart(Z, "FungalTrail");
	}
}
