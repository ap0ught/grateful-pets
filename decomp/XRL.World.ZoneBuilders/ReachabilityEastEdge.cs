namespace XRL.World.ZoneBuilders;

public class ReachabilityEastEdge
{
	public bool ClearFirst = true;

	public bool BuildZone(Zone Z)
	{
		if (ClearFirst)
		{
			Z.ClearReachableMap();
		}
		for (int i = 0; i < Z.Height; i++)
		{
			Z.BuildReachableMap(Z.Width - 1, i);
		}
		return true;
	}
}
