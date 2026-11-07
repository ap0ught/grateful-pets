namespace XRL.World.ZoneBuilders;

public class CanyonStartMouth : IConnectionBuilder
{
	public bool BuildZone(Zone Z)
	{
		return ConnectionStart(Z, "Canyon");
	}
}
