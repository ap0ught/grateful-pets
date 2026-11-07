using System;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class OmonporchGroveBuilder
{
	public bool BuildZone(Zone Zone)
	{
		Zone.BuildReachableMap(0, 0, bClearFirst: false);
		return true;
	}
}
