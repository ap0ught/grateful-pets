namespace XRL.World.ZoneBuilders;

public class ReachabilityWestEdge
{
	public bool ClearFirst = true;

	public bool BuildZone(Zone Z)
	{
		if (ClearFirst)
		{
			Z.ClearReachableMap();
		}
		for (int i = 0; i < Z.Width; i++)
		{
			Z.BuildReachableMap(0, Z.Height - 1);
		}
		return true;
	}
}
