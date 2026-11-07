using XRL.Core;

namespace XRL.World.ZoneBuilders;

public class CanyonReacher
{
	public bool BuildZone(Zone Z)
	{
		Z.ClearReachableMap();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type.Contains("Canyon"))
			{
				Z.BuildReachableMap(zoneConnection.X, zoneConnection.Y, bClearFirst: false);
			}
		}
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.Type.Contains("Canyon") && item.TargetDirection == "-")
			{
				Z.BuildReachableMap(item.X, item.Y, bClearFirst: false);
			}
		}
		return true;
	}
}
