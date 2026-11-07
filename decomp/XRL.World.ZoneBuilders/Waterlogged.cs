using System.Collections.Generic;
using XRL.Core;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class Waterlogged
{
	public bool BuildZone(Zone Z)
	{
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 6, 20, 20, 4, 3, 0, 1, list);
		int num = -1;
		for (int i = 0; i < noiseMap.nAreas; i++)
		{
			if (noiseMap.AreaNodes[i].Count > num)
			{
				num = noiseMap.AreaNodes[i].Count;
			}
		}
		for (int j = 0; j < Z.Height; j++)
		{
			for (int k = 0; k < Z.Width; k++)
			{
				if (noiseMap.Noise[k, j] > 1 && Z.GetCell(k, j).IsEmptyOfSolid())
				{
					Z.GetCell(k, j).AddObject("SaltyWaterPuddle");
				}
			}
		}
		return true;
	}
}
