using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World.AI.Pathfinding;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class RoadBuilder
{
	public bool HardClear;

	public bool ClearSolids = true;

	public bool ClearAdjacent = true;

	public bool Noise = true;

	[NonSerialized]
	public Action<Cell> BeforePlacement;

	public RoadBuilder()
	{
	}

	public RoadBuilder(bool HardClear)
	{
		this.HardClear = HardClear;
	}

	public bool BuildRoads(Zone Z, List<Point> Roads, bool bClearSolids, bool bNoise)
	{
		for (int i = 1; i < Roads.Count; i++)
		{
			GameObject looker = null;
			if (Z.BuildTries > 5)
			{
				looker = GameObject.Create("Drillbot");
			}
			if (Roads[0].X == Roads[i].X && Roads[0].Y == Roads[i].Y)
			{
				continue;
			}
			FindPath findPath = new FindPath();
			findPath.PerformPathfind(Z, Roads[0].X, Roads[0].Y, Z, Roads[i].X, Roads[i].Y, PathGlobal: false, looker, Unlimited: true, bNoise, CardinalDirectionsOnly: false, null, int.MaxValue);
			if (!findPath.Usable)
			{
				return false;
			}
			foreach (Cell step in findPath.Steps)
			{
				BeforePlacement?.Invoke(step);
				GameObject firstObjectWithPart = step.GetFirstObjectWithPart("LiquidVolume");
				if (firstObjectWithPart != null)
				{
					if (!firstObjectWithPart.LiquidVolume.ContainsLiquid("blood") && !step.HasBridge())
					{
						step.AddObject(GameObject.Create("Bridge"));
					}
				}
				else
				{
					if (!bClearSolids && step.IsOccluding())
					{
						continue;
					}
					Z.ReachableMap[step.X, step.Y] = true;
					if (bClearSolids)
					{
						if (HardClear)
						{
							step.Clear();
						}
						else
						{
							step.ClearTerrain();
						}
					}
					if (!step.HasObjectWithBlueprint("DirtRoad"))
					{
						step.AddObject(GameObject.Create("DirtRoad"));
					}
				}
			}
			foreach (Cell step2 in findPath.Steps)
			{
				foreach (Cell localAdjacentCell in step2.GetLocalAdjacentCells())
				{
					BeforePlacement?.Invoke(localAdjacentCell);
					GameObject openLiquidVolume = localAdjacentCell.GetOpenLiquidVolume();
					if (openLiquidVolume != null)
					{
						if (!openLiquidVolume.LiquidVolume.ContainsLiquid("blood") && !localAdjacentCell.HasBridge())
						{
							localAdjacentCell.AddObject(GameObject.Create("Bridge"));
						}
					}
					else
					{
						if (!bClearSolids && localAdjacentCell.IsOccluding())
						{
							continue;
						}
						Z.ReachableMap[localAdjacentCell.X, localAdjacentCell.Y] = true;
						if (bClearSolids && ClearAdjacent)
						{
							if (HardClear)
							{
								localAdjacentCell.Clear();
							}
							else
							{
								localAdjacentCell.ClearTerrain();
							}
						}
						if (!localAdjacentCell.HasObjectWithBlueprint("DirtRoad"))
						{
							localAdjacentCell.AddObject(GameObjectFactory.Factory.CreateObject("DirtRoad"));
						}
					}
				}
			}
		}
		return true;
	}

	public bool BuildZone(Zone Z)
	{
		List<Point> list = new List<Point>();
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-" && item.Type.Contains("Road"))
			{
				list.Add(new Point(item.X, item.Y));
			}
		}
		bool flag = false;
		int num = 40;
		int num2 = 20;
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type.Contains("Road"))
			{
				if (zoneConnection.Type.Contains("Start"))
				{
					flag = true;
					num = zoneConnection.X;
					num2 = zoneConnection.Y;
					list.Insert(0, new Point(zoneConnection.X, zoneConnection.Y));
				}
				else
				{
					list.Add(new Point(zoneConnection.X, zoneConnection.Y));
				}
			}
		}
		if (flag)
		{
			int num3 = num - 5;
			int num4 = num2 - 5;
			NoiseMap noiseMap = new NoiseMap(10, 10, 10, 3, 3, 2, 80, 80, 3, 1, 0, 1, null);
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					if (noiseMap.Noise[i, j] > 1)
					{
						Z.GetCell(i + num3, j + num4).AddObject(GameObjectFactory.Factory.CreateObject("DirtPath"));
					}
				}
			}
		}
		if (list.Count > 0)
		{
			BuildRoads(Z, list, ClearSolids, Noise);
		}
		return true;
	}
}
