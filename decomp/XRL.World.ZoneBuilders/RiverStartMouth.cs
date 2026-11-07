namespace XRL.World.ZoneBuilders;

public class RiverStartMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionStart(Z, "River");
	}
}
