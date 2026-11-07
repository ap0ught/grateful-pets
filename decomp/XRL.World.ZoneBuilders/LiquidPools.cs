using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.World.Parts;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class LiquidPools
{
	public bool BuildZone(Zone Z, string PuddleObject, int Density, string FlamingPools = "0", string PlantReplacements = null)
	{
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, 3 + Density, 20, 20, 4, 3, 0, 1, list);
		int num = -1;
		for (int i = 0; i < noiseMap.nAreas; i++)
		{
			if (noiseMap.AreaNodes[i].Count > num)
			{
				num = noiseMap.AreaNodes[i].Count;
			}
		}
		List<GameObject> list2 = new List<GameObject>();
		List<Cell> list3 = new List<Cell>();
		for (int j = 0; j < Z.Height; j++)
		{
			for (int k = 0; k < Z.Width; k++)
			{
				if (noiseMap.Noise[k, j] > 1)
				{
					if (Z.GetCell(k, j).IsEmpty())
					{
						list2.Add(Z.GetCell(k, j).AddObject(GameObjectFactory.Factory.CreateObject(PuddleObject)));
					}
				}
				else if ((double)noiseMap.Noise[k, j] >= 0.8 && PlantReplacements != null)
				{
					list3.Add(Z.GetCell(k, j));
				}
			}
		}
		if (PlantReplacements != null)
		{
			List<GameObject> list4 = new List<GameObject>();
			for (int l = 0; l < Z.Width; l++)
			{
				for (int m = 0; m < Z.Height; m++)
				{
					List<GameObject> objectsWithPart = Z.GetCell(l, m).GetObjectsWithPart("PlantProperties");
					for (int n = 0; n < objectsWithPart.Count; n++)
					{
						if (!objectsWithPart[n].HasPart<Brain>())
						{
							list4.Add(objectsWithPart[n]);
						}
					}
				}
			}
			int num2 = Math.Min(list4.Count, list3.Count);
			if (num2 > 0)
			{
				Algorithms.RandomShuffleInPlace(list3);
				Algorithms.RandomShuffleInPlace(list4);
				for (int num3 = 0; num3 < num2; num3++)
				{
					list4.RemoveAt(0);
					list3[num3].AddObject(PopulationManager.RollOneFrom(PlantReplacements).Blueprint);
				}
			}
		}
		int num4 = FlamingPools.RollCached();
		if (num4 > 0)
		{
			Algorithms.RandomShuffleInPlace(list2);
			for (int num5 = 0; num5 < num4 && num5 < list2.Count; num5++)
			{
				list2[num5].Physics.Temperature = (list2[num5].Physics.FlameTemperature + list2[num5].Physics.VaporTemperature) / 2;
			}
		}
		return true;
	}
}
