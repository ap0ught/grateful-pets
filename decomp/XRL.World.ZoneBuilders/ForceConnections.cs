using System;
using System.Collections.Generic;
using Genkit;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class ForceConnections : ZoneBuilderSandbox
{
	public bool CaveLike = true;

	public new static FastNoise pathNoise = new FastNoise();

	public bool BuildZone(Zone Z)
	{
		return _BuildZone(Z);
	}

	public bool _BuildZone(Zone Z, List<Point2D> additional = null)
	{
		if (Z.GetZoneProperty("DisableForcedConnections") == "Yes")
		{
			return true;
		}
		List<Point2D> list = new List<Point2D>();
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				if ((Z.GetCell(i, j).HasObjectWithPart("StairsUp") || Z.GetCell(i, j).HasObjectWithPart("StairsDown")) && !list.Contains(new Point2D(i, j)))
				{
					list.Add(new Point2D(i, j));
				}
			}
		}
		if (additional != null)
		{
			foreach (Point2D item in additional)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		foreach (CachedZoneConnection item2 in Z.ZoneConnectionCache)
		{
			if (item2.TargetDirection == "-" && !list.Contains(new Point2D(item2.X, item2.Y)))
			{
				list.Add(new Point2D(item2.X, item2.Y));
			}
		}
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (!list.Contains(new Point2D(zoneConnection.X, zoneConnection.Y)))
			{
				list.Add(new Point2D(zoneConnection.X, zoneConnection.Y));
			}
		}
		Algorithms.RandomShuffleInPlace(list);
		List<Cell> emptyReachableCells = Z.GetEmptyReachableCells();
		if (emptyReachableCells.Count == 0 && list.Count > 0)
		{
			emptyReachableCells.Add(Z.GetCell(list[0].location));
		}
		if (emptyReachableCells.Count <= 0)
		{
			return true;
		}
		emptyReachableCells.ShuffleInPlace();
		list.Add(emptyReachableCells[0].Pos2D);
		if (list.Count == 0)
		{
			return true;
		}
		Z.ClearReachableMap();
		Z.SetReachable(list[0].x, list[0].y);
		Z.BuildReachableMap(list[0].x, list[0].y, bClearFirst: false);
		pathNoise.SetSeed(Stat.Random(int.MinValue, int.MaxValue));
		pathNoise.SetNoiseType(FastNoise.NoiseType.Simplex);
		pathNoise.SetFractalOctaves(2);
		pathNoise.SetFrequency(0.2f);
		GameObject.Create("Drillbot");
		using (Pathfinder pathfinder = Z.getPathfinder((int x, int y, Cell c) =>
		{
			int num2 = 0;
			num2 = (int)(Math.Abs(pathNoise.GetNoise((x + Z.wX * 80) / 3, y + Z.wY * 25)) * 160f);
			if (Z.GetCell(x, y).HasWall())
			{
				return 80 + num2;
			}
			return Z.GetCell(x, y).HasObject("InfluenceMapBlocker") ? (80 + num2) : num2;
		}))
		{
			for (int num = 1; num < list.Count; num++)
			{
				if (Z.IsReachable(list[num].x, list[num].y) || !pathfinder.FindPath(list[0].location, list[num].location, Display: false, CardinalDirectionsOnly: true, 24300, ShuffleDirections: true))
				{
					continue;
				}
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Z.ReachableMap[step.X, step.Y] = true;
					Z.GetCell(step.X, step.Y)?.ClearWalls();
				}
				if (CaveLike)
				{
					foreach (PathfinderNode step2 in pathfinder.Steps)
					{
						Cell cell = Z.GetCell(step2.X, step2.Y);
						if (cell == null || cell.IsReachable() || cell.HasWall())
						{
							continue;
						}
						foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells(Stat.Random(1, 2)))
						{
							if (Stat.Random(1, 100) <= 75)
							{
								Z.ReachableMap[localAdjacentCell.X, localAdjacentCell.Y] = true;
								localAdjacentCell.ClearWalls();
							}
						}
					}
				}
				Z.BuildReachableMap(list[num].x, list[num].y, bClearFirst: false);
			}
		}
		return true;
	}
}
