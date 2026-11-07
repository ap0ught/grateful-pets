using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class BeyLahOutskirts : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).RequireObject("DaylightWidget");
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		NoiseMap noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, Stat.Random(30, 60), Stat.Random(50, 70), Stat.Random(125, 135), 0, 10, 0, 1, list, 5);
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (noiseMap.Noise[j, i] > 3 && 90.in100())
				{
					Z.GetCell(j, i).AddObject(PopulationManager.RollOneFrom("BeyLahOutskirtsTrees").Blueprint);
				}
			}
		}
		InfluenceMap influenceMap = new InfluenceMap(Z.Width, Z.Height);
		influenceMap.SeedAllUnseeded();
		while (influenceMap.LargestSize() > 150)
		{
			influenceMap.AddSeedAtRandom();
		}
		int num = Stat.Random(1, 3);
		for (int k = 0; k < influenceMap.Regions.Count; k++)
		{
			if (influenceMap.Regions[k].IsEdgeRegion())
			{
				continue;
			}
			for (int l = influenceMap.Regions[k].BoundingBox.x1; l <= influenceMap.Regions[k].BoundingBox.x2; l++)
			{
				for (int m = influenceMap.Regions[k].BoundingBox.y1; m <= influenceMap.Regions[k].BoundingBox.y2; m++)
				{
					if (influenceMap.Regions[k].Contains(Location2D.Get(l, m)))
					{
						if (l % 2 == 0 && !influenceMap.Regions[k].BorderCells.Contains(Location2D.Get(l, m)))
						{
							Z.GetCell(l, m).AddObject("BrackishWaterPuddle");
						}
						else
						{
							Z.GetCell(l, m).AddObject("Watervine");
						}
					}
				}
			}
			num--;
			if (num <= 0)
			{
				break;
			}
		}
		int n = 0;
		for (int num2 = Stat.Random(0, 4); n < num2; n++)
		{
			ZoneBuilderSandbox.PlaceObject("HindrenVillager", Z);
		}
		Z.ClearReachableMap();
		if (Z.BuildReachableMap(0, 0) < 400)
		{
			return false;
		}
		return true;
	}
}
