using System;
using System.Collections.Generic;
using Genkit;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.ZoneBuilders;

[HasGameBasedStaticCache]
public class Cave : ZoneBuilderSandbox
{
	public static float[,] CaveNoise;

	private const int MaxWidth = 1200;

	private const int MaxHeight = 375;

	public bool Noise;

	[NonSerialized]
	[GameBasedStaticCache(true, false, CreateInstance = false)]
	public static Maze3D TunnelMaze;

	public static void Save(SerializationWriter Writer)
	{
		Writer.Write(0);
	}

	public static void Load(SerializationReader Reader)
	{
		Reader.ReadInt32();
		CaveNoise = null;
		TunnelMaze = null;
	}

	public bool BuildZone(Zone Z)
	{
		string defaultWall = GetDefaultWall(Z);
		Z.ClearReachableMap();
		Z.Fill(defaultWall);
		int seededRange = GetSeededRange("TunnelSeed");
		if (TunnelMaze == null)
		{
			TunnelMaze = RecursiveBacktrackerMaze3D.Generate(GetSeededRange("TunnelMaze"), 240, 75, 30, bShow: false, 10, 5, 5, 5, 5, 5);
		}
		if (CaveNoise == null)
		{
			List<PerlinNoise2Df> list = null;
			Random seededRand = GetSeededRand("CaveNoise");
			list = new List<PerlinNoise2Df>();
			list.Add(new PerlinNoise2Df(4, 1f, seededRand));
			list.Add(new PerlinNoise2Df(8, 0.86f, seededRand));
			list.Add(new PerlinNoise2Df(64, 0.6f, seededRand));
			list.Add(new PerlinNoise2Df(300, 0.36f, seededRand));
			list.Add(new PerlinNoise2Df(1200, 0.06f, seededRand));
			CaveNoise = PerlinNoise2Df.sumNoiseFunctions(1200, 375, 0, 0, list, 0.5f);
			CaveNoise = PerlinNoise2Df.Smooth(CaveNoise, 1200, 375, 1);
		}
		CellularGrid cellularGrid = new CellularGrid();
		cellularGrid.Passes = 2;
		cellularGrid.SeedChance = 60;
		cellularGrid.SeedBorders = true;
		cellularGrid.BorderDepth = 2;
		cellularGrid.Generate(Stat.Rand, Z.Width, Z.Height);
		int num = Z.wX * 240 + Z.X * 80 + Z.Z * 3;
		int num2 = Z.wY * 75 + Z.Y * 25 + Z.Z * 5;
		num %= 1119;
		num2 %= 349;
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (j + num < 1200 && i + num2 < 375)
				{
					double num3 = CaveNoise[j + num, i + num2];
					if (cellularGrid.cells[j, i] == 0 || num3 <= 0.4699999988079071)
					{
						Z.GetCell(j, i).Clear();
						Z.GetCell(j, i).SetReachable(State: true);
					}
				}
			}
		}
		int num4 = Z.wX * 3 + Z.X;
		int num5 = Z.wY * 3 + Z.Y;
		MazeCell3D mazeCell3D = TunnelMaze.Cell[num4, num5, Math.Max(0, Z.Z - 11) % 30];
		if (8.in100())
		{
			if (mazeCell3D.N)
			{
				Z.CacheZoneConnection("-", new Point(GetSeededRange(num5 + seededRange * Z.Z, 10, 70), 0), "cave", null);
			}
			if (mazeCell3D.S)
			{
				Z.CacheZoneConnection("-", new Point(GetSeededRange(num5 + 1 + seededRange * Z.Z, 10, 70), Z.Height - 1), "cave", null);
			}
			if (mazeCell3D.E)
			{
				Z.CacheZoneConnection("-", new Point(Z.Width - 1, GetSeededRange(num4 + 1 + seededRange * Z.Z, 5, 20)), "cave", null);
			}
			if (mazeCell3D.W)
			{
				Z.CacheZoneConnection("-", new Point(0, GetSeededRange(num4 + seededRange * Z.Z, 5, 20)), "cave", null);
			}
			new SultanDungeon().BuildRandomZone(Z, 5);
			The.ZoneManager.AddZonePostBuilderIfNotAlreadyPresent(Z.ZoneID, "ForceConnections");
			if (mazeCell3D.D)
			{
				new StairsDown().BuildZone(Z);
			}
			if (mazeCell3D.U)
			{
				new StairsUp().BuildZone(Z);
			}
			return true;
		}
		if (mazeCell3D.D)
		{
			new StairsDown().BuildZone(Z);
		}
		if (mazeCell3D.U)
		{
			new StairsUp().BuildZone(Z);
		}
		List<Point> list2 = new List<Point>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list2.Add(new Point(zoneConnection.X, zoneConnection.Y));
		}
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-")
			{
				list2.Add(new Point(item.X, item.Y));
			}
		}
		if (mazeCell3D.N)
		{
			list2.Add(new Point(GetSeededRange(num5 + seededRange * Z.Z, 10, 70), 0));
		}
		if (mazeCell3D.S)
		{
			list2.Add(new Point(GetSeededRange(num5 + 1 + seededRange * Z.Z, 10, 70), Z.Height - 1));
		}
		if (mazeCell3D.E)
		{
			list2.Add(new Point(Z.Width - 1, GetSeededRange(num4 + 1 + seededRange * Z.Z, 5, 20)));
		}
		if (mazeCell3D.W)
		{
			list2.Add(new Point(0, GetSeededRange(num4 + seededRange * Z.Z, 5, 20)));
		}
		Algorithms.RandomShuffleInPlace(list2, Stat.Rand);
		while (list2.Count < 2)
		{
			list2.Add(new Point(Stat.Random(10, 70), Stat.Random(5, 15)));
			list2.Add(new Point(Stat.Random(10, 70), Stat.Random(5, 15)));
		}
		if (list2.Count > 0)
		{
			for (int k = 0; k < list2.Count; k++)
			{
				Z.CacheZoneConnection("-", list2[k].X, list2[k].Y, "tunnelnode", null);
			}
			GameObject gameObject = null;
			gameObject = GameObjectFactory.Factory.CreateObject("Drillbot");
			int x = list2[0].X;
			int y = list2[0].Y;
			for (int l = 1; l < list2.Count; l++)
			{
				if (x == list2[l].X && y == list2[l].Y)
				{
					continue;
				}
				FindPath findPath = new FindPath(Z, x, y, Z, list2[l].X, list2[l].Y, PathGlobal: false, PathUnlimited: true, gameObject, AddNoise: true);
				if (!findPath.Usable)
				{
					return false;
				}
				foreach (Cell step in findPath.Steps)
				{
					Z.ReachableMap[step.X, step.Y] = true;
					step.ClearWalls();
				}
				foreach (Cell step2 in findPath.Steps)
				{
					foreach (Cell localAdjacentCell in step2.GetLocalAdjacentCells(Stat.Random(1, 2)))
					{
						if (Stat.Random(1, 100) <= 75)
						{
							Z.ReachableMap[localAdjacentCell.X, localAdjacentCell.Y] = true;
							localAdjacentCell.ClearWalls();
						}
					}
				}
			}
		}
		foreach (ZoneConnection zoneConnection2 in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (!Z.GetCell(zoneConnection2.X, zoneConnection2.Y).IsSolid())
			{
				Z.BuildReachableMap(zoneConnection2.X, zoneConnection2.Y, bClearFirst: false);
			}
		}
		foreach (CachedZoneConnection item2 in Z.ZoneConnectionCache)
		{
			if (item2.TargetDirection == "-" && !Z.GetCell(item2.X, item2.Y).IsSolid())
			{
				Z.BuildReachableMap(item2.X, item2.Y, bClearFirst: false);
			}
		}
		for (int m = 0; m < Z.Height; m++)
		{
			int num6 = 0;
			while (num6 < Z.Width)
			{
				if (!Z.GetCell(num6, m).IsReachable())
				{
					num6++;
					continue;
				}
				goto IL_08c4;
			}
			continue;
			IL_08c4:
			Z.BuildReachableMap(num6, m, bClearFirst: false);
			break;
		}
		if (Z.Z > 49)
		{
			new LiquidPools().BuildZone(Z, "LavaPool", Z.Z - 50, "0-2");
		}
		if (Stat.Random(1, 1000) <= 2)
		{
			InfluenceMap iF = ZoneBuilderSandbox.GenerateInfluenceMap(Z, list2, InfluenceMapSeedStrategy.FurthestPoint, 100);
			new CaveCity().BuildZone(Z, iF);
			Z.GetCell(0, 0).AddObject("Rocky");
		}
		else
		{
			Z.GetCell(0, 0).AddObject("Rocky");
		}
		return true;
	}
}
