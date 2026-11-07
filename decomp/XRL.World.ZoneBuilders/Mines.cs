using System.Collections.Generic;
using XRL.Core;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class Mines
{
	public int MinimumDepth = 5;

	public bool BuildZone(Zone Z)
	{
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-")
			{
				list.Add(new NoiseMapNode(item.X, item.Y));
			}
		}
		NoiseMap noiseMap = new NoiseMap(80, 25, 20, 3, 3, 5, 50, 60, 6, 2, 1, 1, list);
		int key = -1;
		int num = -1;
		for (int i = 0; i < noiseMap.nAreas; i++)
		{
			if (noiseMap.AreaNodes[i].Count > num)
			{
				num = noiseMap.AreaNodes[i].Count;
				key = i;
			}
		}
		foreach (List<NoiseMapNode> value in noiseMap.AreaNodes.Values)
		{
			foreach (NoiseMapNode item2 in value)
			{
				Cell cell = Z.GetCell(item2.x, item2.y);
				cell.Clear();
				if (item2.depth >= MinimumDepth)
				{
					cell.AddObject(GameObjectFactory.Factory.CreateObject("AsphaltPuddle"));
				}
			}
		}
		Z.ClearReachableMap();
		if (noiseMap.AreaNodes.ContainsKey(key) && noiseMap.AreaNodes[0].Count > 0)
		{
			Z.BuildReachableMap(noiseMap.AreaNodes[key][0].x, noiseMap.AreaNodes[key][0].y);
		}
		else
		{
			Z.ClearReachableMap(bValue: true);
		}
		return true;
	}
}
