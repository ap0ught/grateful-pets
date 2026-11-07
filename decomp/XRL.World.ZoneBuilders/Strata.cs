using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genkit;
using HistoryKit;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.ZoneBuilders;

public class Strata : ZoneBuilderSandbox
{
	public enum CellType
	{
		Wall,
		Open,
		Detail
	}

	private const int MaxWidth = 1200;

	private const int MaxHeight = 375;

	public bool Noise;

	public bool Stairs = true;

	[NonSerialized]
	public static Maze3D TunnelMaze;

	public const int GREY_FLOORTILE_CHANCE = 5;

	private FastNoise fastNoise = new FastNoise();

	public static void Save(SerializationWriter Writer)
	{
		Writer.Write(0);
	}

	public static void Load(SerializationReader Reader)
	{
		Reader.ReadInt32();
		TunnelMaze = null;
	}

	public bool ConnectMaze(Zone Z, int minMazeWidth = 1, int maxMazeWidth = 2)
	{
		int num = Z.wX * 3 + Z.X;
		int num2 = Z.wY * 3 + Z.Y;
		int seededRange = GetSeededRange("TunnelSeed");
		if (TunnelMaze == null)
		{
			TunnelMaze = RecursiveBacktrackerMaze3D.Generate(GetSeededRange("TunnelMaze"), 240, 75, 30, bShow: false, 10, 5, 5, 5, 5, 5);
		}
		MazeCell3D mazeCell3D = TunnelMaze.Cell[num, num2, Math.Max(0, Z.Z - 11) % 30];
		if (8.in100())
		{
			if (mazeCell3D.N)
			{
				Z.CacheZoneConnection("-", new Point(GetSeededRange(num2 + seededRange * Z.Z, 10, 70), 0), "cave", null);
			}
			if (mazeCell3D.S)
			{
				Z.CacheZoneConnection("-", new Point(GetSeededRange(num2 + 1 + seededRange * Z.Z, 10, 70), Z.Height - 1), "cave", null);
			}
			if (mazeCell3D.E)
			{
				Z.CacheZoneConnection("-", new Point(Z.Width - 1, GetSeededRange(num + 1 + seededRange * Z.Z, 5, 20)), "cave", null);
			}
			if (mazeCell3D.W)
			{
				Z.CacheZoneConnection("-", new Point(0, GetSeededRange(num + seededRange * Z.Z, 5, 20)), "cave", null);
			}
			new SultanDungeon().BuildRandomZone(Z, 5);
			if (Stairs)
			{
				The.ZoneManager.AddZonePostBuilderIfNotAlreadyPresent(Z.ZoneID, "ForceConnections");
				if (mazeCell3D.D)
				{
					new StairsDown().BuildZone(Z);
				}
				if (mazeCell3D.U)
				{
					new StairsUp().BuildZone(Z);
				}
			}
			return true;
		}
		if (Stairs)
		{
			if (mazeCell3D.D)
			{
				new StairsDown().BuildZone(Z);
			}
			if (mazeCell3D.U)
			{
				new StairsUp().BuildZone(Z);
			}
		}
		List<Point> list = new List<Point>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection?.Type?.Contains("Pull") != true)
			{
				list.Add(new Point(zoneConnection.X, zoneConnection.Y));
			}
		}
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-")
			{
				list.Add(new Point(item.X, item.Y));
			}
		}
		if (mazeCell3D.N)
		{
			list.Add(new Point(GetSeededRange(num2 + seededRange * Z.Z, 10, 70), 0));
		}
		if (mazeCell3D.S)
		{
			list.Add(new Point(GetSeededRange(num2 + 1 + seededRange * Z.Z, 10, 70), Z.Height - 1));
		}
		if (mazeCell3D.E)
		{
			list.Add(new Point(Z.Width - 1, GetSeededRange(num + 1 + seededRange * Z.Z, 5, 20)));
		}
		if (mazeCell3D.W)
		{
			list.Add(new Point(0, GetSeededRange(num + seededRange * Z.Z, 5, 20)));
		}
		Algorithms.RandomShuffleInPlace(list, Stat.Rand);
		while (list.Count < 2)
		{
			list.Add(new Point(Stat.Random(10, 70), Stat.Random(5, 15)));
			list.Add(new Point(Stat.Random(10, 70), Stat.Random(5, 15)));
		}
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Z.CacheZoneConnection("-", list[i].X, list[i].Y, "tunnelnode", null);
			}
			GameObject gameObject = null;
			gameObject = GameObjectFactory.Factory.CreateObject("Drillbot");
			int x = list[0].X;
			int y = list[0].Y;
			for (int j = 1; j < list.Count; j++)
			{
				if (x == list[j].X && y == list[j].Y)
				{
					continue;
				}
				FindPath findPath = new FindPath(Z, x, y, Z, list[j].X, list[j].Y, PathGlobal: false, PathUnlimited: true, gameObject, AddNoise: true);
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
					foreach (Cell localAdjacentCell in step2.GetLocalAdjacentCells(Stat.Random(minMazeWidth, maxMazeWidth)))
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
		for (int k = 0; k < Z.Height; k++)
		{
			int num3 = 0;
			while (num3 < Z.Width)
			{
				if (!Z.GetCell(num3, k).IsReachable())
				{
					num3++;
					continue;
				}
				goto IL_06f9;
			}
			continue;
			IL_06f9:
			Z.BuildReachableMap(num3, k, bClearFirst: false);
			break;
		}
		return true;
	}

	public List<Tuple<string, int>> resolveElements(int x, int y, int z, List<Func<int, int, int, Tuple<string, int>>> input, Func<Tuple<string, int>, Tuple<string, int>> transform)
	{
		input.RemoveAll((Func<int, int, int, Tuple<string, int>> e) => e == null);
		List<Tuple<string, int>> list = input.Select((Func<int, int, int, Tuple<string, int>> e) => e(x, y, z)).ToList();
		list.RemoveAll((Tuple<string, int> e) => e == null);
		list = list.Select((Tuple<string, int> e) => transform(e)).ToList();
		list.Sort((Tuple<string, int> a, Tuple<string, int> b) => b.Item2.CompareTo(a.Item2));
		StringBuilder stringBuilder = new StringBuilder();
		for (int num = 0; num < list.Count; num++)
		{
			stringBuilder.Append(list[num].Item1 + "=" + list[num].Item2 + "\n");
		}
		return list;
	}

	public string getRandomWeightedFromElements(int x, int y, int z, List<Func<int, int, int, Tuple<string, int>>> input, Func<Tuple<string, int>, Tuple<string, int>> transform)
	{
		List<Tuple<string, int>> list = resolveElements(x, y, z, input, transform);
		int num = 0;
		foreach (Tuple<string, int> item in list)
		{
			num += item.Item2;
		}
		int num2 = Stat.Random(0, num);
		int num3 = 0;
		foreach (Tuple<string, int> item2 in list)
		{
			num3 += item2.Item2;
			if (num3 >= num2)
			{
				return item2.Item1;
			}
		}
		return null;
	}

	public int getSeed(string seed)
	{
		return XRLCore.Core.Game.GetWorldSeed(seed);
	}

	public double sampleSimplexNoise(string type, int x, int y, int z, int amplitude, float frequencyMultiplier = 1f)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f * frequencyMultiplier);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		return Math.Ceiling((double)fastNoise.GetNoise(x, y, z) * (double)amplitude);
	}

	public double sampleSimplexNoiseRange(string type, int x, int y, int z, float low, float high)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		return (fastNoise.GetNoise(x, y, z) + 1f) / 2f * (high - low) + low;
	}

	public int sampleSimplexNoiseIntRange(string type, int x, int y, int z, int low, int high)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		float num = (fastNoise.GetNoise(x, y, z) + 1f) / 2f;
		num *= (float)high - (float)low;
		num += (float)low;
		return Math.Min(Math.Max(low, (int)Math.Round(num)), high);
	}

	public double sampleZonescaleSimplexNoise(string type, int x, int y, int z, int amplitude)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.01f);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(2f);
		fastNoise.SetFractalGain(0.5f);
		return Math.Ceiling((double)fastNoise.GetNoise(x, y, z) * (double)amplitude);
	}

	public double sampleZonescaleSimplexNoiseRange(string type, int x, int y, int z, float low, float high)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.01f);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(2f);
		fastNoise.SetFractalGain(0.5f);
		return (fastNoise.GetNoise(x, y, z) + 1f) / 2f * (high - low) + low;
	}

	public int sampleZonescaleSimplexNoiseIntRange(string type, int x, int y, int z, int low, int high)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.01f);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(2f);
		fastNoise.SetFractalGain(0.5f);
		float num = (fastNoise.GetNoise(x, y, z) + 1f) / 2f;
		num *= (float)high - (float)low;
		num += (float)low;
		return Math.Min(Math.Max(low, (int)Math.Round(num)), high);
	}

	public CellType[,] getRandomWalkCaveLayout(Zone Z)
	{
		CellType[,] array = new CellType[Z.Width, Z.Height];
		int num = 500;
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				array[j, i] = CellType.Wall;
			}
		}
		int num2 = Stat.Random(0, Z.Width - 1);
		int num3 = Stat.Random(0, Z.Height - 1);
		for (int k = 0; k < 3; k++)
		{
			for (int l = 0; l < num; l++)
			{
				array[num2, num3] = CellType.Open;
				int num4 = Stat.Random(0, 3);
				if (num4 == 0)
				{
					num2++;
				}
				if (num4 == 1)
				{
					num2--;
				}
				if (num4 == 2)
				{
					num3++;
				}
				if (num4 == 3)
				{
					num3--;
				}
				if (num2 < 0)
				{
					num2 = 0;
				}
				if (num3 < 0)
				{
					num3 = 0;
				}
				if (num2 > Z.Width - 1)
				{
					num2 = Z.Width - 1;
				}
				if (num3 > Z.Height - 1)
				{
					num3 = Z.Height - 1;
				}
			}
			int[,] array2 = new int[Z.Width, Z.Height];
			for (int m = 0; m < Z.Width; m++)
			{
				for (int n = 0; n < Z.Height; n++)
				{
					if (array[m, n] == CellType.Wall)
					{
						array2[m, n] = 0;
					}
					else
					{
						array2[m, n] = 1;
					}
				}
			}
			Rect2D rect2D = GridTools.MaxRectByArea(array2);
			if (rect2D.Area <= 300)
			{
				break;
			}
			num2 = rect2D.Center.x;
			num3 = rect2D.Center.y;
		}
		return array;
	}

	public int countBoxFilterAt(int x, int y, CellType[,] cells)
	{
		if (x < 0)
		{
			return 0;
		}
		if (y < 0)
		{
			return 0;
		}
		if (x >= 80)
		{
			return 0;
		}
		if (y >= 24)
		{
			return 0;
		}
		if (cells[x, y] == CellType.Wall)
		{
			return 1;
		}
		return 0;
	}

	public int countBoxFilterAdjacentD1(int x, int y, CellType[,] cells)
	{
		return 0 + countBoxFilterAt(x - 1, y - 1, cells) + countBoxFilterAt(x - 1, y, cells) + countBoxFilterAt(x - 1, y + 1, cells) + countBoxFilterAt(x, y - 1, cells) + countBoxFilterAt(x, y + 1, cells) + countBoxFilterAt(x + 1, y - 1, cells) + countBoxFilterAt(x + 1, y, cells) + countBoxFilterAt(x + 1, y + 1, cells);
	}

	public int countBoxFilterAdjacentD2(int x, int y, CellType[,] cells)
	{
		return 0 + countBoxFilterAdjacentD1(x, y, cells) + countBoxFilterAt(x - 2, y - 1, cells) + countBoxFilterAt(x - 2, y, cells) + countBoxFilterAt(x - 2, y + 1, cells) + countBoxFilterAt(x - 1, y - 2, cells) + countBoxFilterAt(x, y - 2, cells) + countBoxFilterAt(x + 1, y - 2, cells) + countBoxFilterAt(x - 1, y + 2, cells) + countBoxFilterAt(x, y + 2, cells) + countBoxFilterAt(x + 1, y + 2, cells) + countBoxFilterAt(x + 2, y - 1, cells) + countBoxFilterAt(x + 2, y, cells) + countBoxFilterAt(x + 2, y + 1, cells);
	}

	public CellType[,] getBoxFilterCave(Zone Z)
	{
		int x = Z.wX * 3 + Z.X;
		int y = Z.wY * 3 + Z.Y;
		CellType[,] array = new CellType[Z.Width, Z.Height];
		int chance = sampleZonescaleSimplexNoiseIntRange("rwcavesteps", x, y, Z.Z, 40, 60);
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				if (j <= 2 || j >= Z.Width - 2 || i <= 2 || i >= Z.Height - 2 || chance.in100())
				{
					array[j, i] = CellType.Wall;
				}
				else
				{
					array[j, i] = CellType.Open;
				}
			}
		}
		int num = sampleZonescaleSimplexNoiseIntRange("rwcavesteps", x, y, Z.Z, 2, 4);
		CellType[,] array2 = new CellType[Z.Width, Z.Height];
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < Z.Width; l++)
			{
				for (int m = 0; m < Z.Height; m++)
				{
					int num2 = 5;
					int num3 = 2;
					if (countBoxFilterAdjacentD1(l, m, array) >= num2)
					{
						array2[l, m] = CellType.Wall;
					}
					else if (countBoxFilterAdjacentD2(l, m, array) <= num3)
					{
						array2[l, m] = CellType.Wall;
					}
					else
					{
						array2[l, m] = CellType.Open;
					}
				}
			}
			CellType[,] array3 = array;
			array = array2;
			array2 = array3;
		}
		return array;
	}

	public CellType[,] getCaveLayout(Zone Z)
	{
		int x = Z.wX * 3 + Z.X;
		int y = Z.wY * 3 + Z.Y;
		CellType[,] array = new CellType[Z.Width, Z.Height];
		CellularGrid cellularGrid = new CellularGrid();
		cellularGrid.Passes = sampleZonescaleSimplexNoiseIntRange("cavePasses", x, y, Z.Z, 1, 4);
		cellularGrid.SeedChance = sampleZonescaleSimplexNoiseIntRange("cavePasses", x, y, Z.Z, 40, 80);
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
					if (cellularGrid.cells[j, i] == 0)
					{
						array[j, i] = CellType.Open;
					}
					else
					{
						array[j, i] = CellType.Wall;
					}
				}
			}
		}
		return array;
	}

	public CellType[,] getPillarsLayout(Zone Z)
	{
		int x = Z.wX * 3 + Z.X;
		int y = Z.wY * 3 + Z.Y;
		CellType[,] array = new CellType[Z.Width, Z.Height];
		FastNoise fastNoise = new FastNoise();
		fastNoise.SetSeed(getSeed("pillars"));
		fastNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
		fastNoise.SetFrequency((float)sampleSimplexNoiseRange("pillarLayoutCrack", x, y, Z.Z, 0.025f, 0.1f));
		fastNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
		fastNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Div);
		fastNoise.SetCellularJitter((float)sampleSimplexNoiseRange("pillarLayoutCrack", x, y, Z.Z, 0.1f, 1.2f));
		fastNoise.SetGradientPerturbAmp(5f);
		int num = sampleSimplexNoiseIntRange("pillarLayoutCrack", x, y, Z.Z, 0, 2);
		fastNoise.SetCellularDistance2Indicies(num, num + 1);
		float num2 = (float)sampleSimplexNoiseRange("pillarLayoutCrackWidth", x, y, Z.Z, 0.5f, 0.9f);
		float num3 = (float)sampleSimplexNoiseRange("pillarLayoutCrackWidth", x, y, Z.Z, 0.85f, 1f);
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				float noise = fastNoise.GetNoise(i, j, Z.Z);
				if (noise < num2)
				{
					array[i, j] = CellType.Wall;
				}
				else if (noise >= num3)
				{
					array[i, j] = CellType.Detail;
				}
				else
				{
					array[i, j] = CellType.Open;
				}
			}
		}
		return array;
	}

	public CellType[,] getPorousLayout(Zone Z)
	{
		int x = Z.wX * 3 + Z.X;
		int y = Z.wY * 3 + Z.Y;
		CellType[,] array = new CellType[Z.Width, Z.Height];
		FastNoise fastNoise = new FastNoise();
		fastNoise.SetSeed(getSeed("windy"));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		float frequency = (float)sampleSimplexNoiseRange("pillarLayoutCrack", x, y, Z.Z, 0.05f, 0.15f);
		fastNoise.SetFrequency(frequency);
		fastNoise.SetFractalType(FastNoise.FractalType.Billow);
		fastNoise.SetFractalOctaves(1);
		fastNoise.SetFractalLacunarity(3f);
		fastNoise.SetFractalGain(0.5f);
		float num = (float)sampleSimplexNoiseRange("pillarLayoutCrack", x, y, Z.Z, -0.75f, -0.5f);
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				float noise = fastNoise.GetNoise(i, j);
				if (noise < num)
				{
					array[i, j] = CellType.Wall;
				}
				else if ((double)noise >= 0.05)
				{
					array[i, j] = CellType.Detail;
				}
				else
				{
					array[i, j] = CellType.Open;
				}
			}
		}
		return array;
	}

	public CellType[,] getBlockyLayout(Zone Z)
	{
		int x = Z.wX * 3 + Z.X;
		int y = Z.wY * 3 + Z.Y;
		CellType[,] array = new CellType[Z.Width, Z.Height];
		FastNoise fastNoise = new FastNoise();
		fastNoise.SetSeed(getSeed("blocky"));
		fastNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
		float frequency = (float)sampleSimplexNoiseRange("pillarLayoutCrack", x, y, Z.Z, 0.25f, 0.9f);
		fastNoise.SetFrequency(frequency);
		fastNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Manhattan);
		fastNoise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);
		fastNoise.SetCellularJitter(sampleSimplexNoiseIntRange("blockyJitter", x, y, Z.Z, 10, 50));
		fastNoise.SetCellularDistance2Indicies(0, 1);
		float num = (float)sampleSimplexNoiseRange("pillarLayoutCrack", x, y, Z.Z, -0.25f, -0.25f);
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				float noise = fastNoise.GetNoise(i, j);
				if (noise < num)
				{
					array[i, j] = CellType.Wall;
				}
				else if ((double)noise >= 0.95)
				{
					array[i, j] = CellType.Detail;
				}
				else
				{
					array[i, j] = CellType.Open;
				}
			}
		}
		return array;
	}

	public CellType[,] getWindyLayout(Zone Z)
	{
		int x = Z.wX * 3 + Z.X;
		int y = Z.wY * 3 + Z.Y;
		CellType[,] array = new CellType[Z.Width, Z.Height];
		FastNoise fastNoise = new FastNoise();
		fastNoise.SetSeed(getSeed("windy"));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		float frequency = (float)sampleSimplexNoiseRange("pillarLayoutCrack", x, y, Z.Z, 0.05f, 0.15f);
		fastNoise.SetFrequency(frequency);
		fastNoise.SetFractalType(FastNoise.FractalType.Billow);
		fastNoise.SetFractalOctaves(1);
		fastNoise.SetFractalLacunarity(3f);
		fastNoise.SetFractalGain(0.5f);
		float num = (float)sampleSimplexNoiseRange("pillarLayoutCrack", x, y, Z.Z, -0.6f, 0.25f);
		num = -0.6f;
		for (int i = 0; i < Z.Width; i++)
		{
			for (int j = 0; j < Z.Height; j++)
			{
				float noise = fastNoise.GetNoise(i, j);
				if (noise > num)
				{
					array[i, j] = CellType.Wall;
				}
				else if ((double)noise >= 0.05)
				{
					array[i, j] = CellType.Detail;
				}
				else
				{
					array[i, j] = CellType.Open;
				}
			}
		}
		return array;
	}

	public bool BuildZone(Zone Z)
	{
		int gridX = Z.wX * 3 + Z.X;
		int gridY = Z.wY * 3 + Z.Y;
		int num = gridX * Z.Width;
		int num2 = gridY * Z.Height;
		int COMMON_WALL_WEIGHT = 5;
		int UNCOMMON_WALL_WEIGHT = 3;
		int RARE_WALL_WEIGHT = 1;
		int WALLTYPE_NOISE_AMPLITUDE = 5;
		List<Func<int, int, int, Tuple<string, int>>> input = new List<Func<int, int, int, Tuple<string, int>>>
		{
			(int x, int y, int z) => (z >= 35) ? new Tuple<string, int>("Pond", 20) : new Tuple<string, int>("Pond", 1000),
			(int x, int y, int z) =>
			{
				if (z >= 35)
				{
					return new Tuple<string, int>("OilDeepPool", 20);
				}
				return (z >= 25) ? new Tuple<string, int>("OilDeepPool", 10) : null;
			},
			(int x, int y, int z) =>
			{
				if (z >= 35)
				{
					return new Tuple<string, int>("AsphaltDeepPool", 100);
				}
				return (z >= 25) ? new Tuple<string, int>("AsphaltDeepPool", 20) : null;
			},
			(int x, int y, int z) =>
			{
				if (z >= 35)
				{
					return new Tuple<string, int>("LavaPool", 1000);
				}
				return (z >= 25) ? new Tuple<string, int>("LavaPool", z * 10) : null;
			},
			(int x, int y, int z) => new Tuple<string, int>("SlimePool", 5),
			(int x, int y, int z) => new Tuple<string, int>("GooPuddle", 5),
			(int x, int y, int z) => new Tuple<string, int>("OozePuddle", 5),
			(int x, int y, int z) => new Tuple<string, int>("SludgePuddle", 5),
			(int x, int y, int z) => new Tuple<string, int>("AcidPool", 5),
			(int x, int y, int z) => new Tuple<string, int>("CiderPool", 1),
			(int x, int y, int z) => new Tuple<string, int>("GelPool", 1),
			(int x, int y, int z) => new Tuple<string, int>("PutrescencePool", 1),
			(int x, int y, int z) => new Tuple<string, int>("FreshWaterPuddle", 1),
			(int x, int y, int z) => new Tuple<string, int>("WinePool", 1),
			(int x, int y, int z) => new Tuple<string, int>("WaxPool", 1),
			(int x, int y, int z) => new Tuple<string, int>("InkPool", 1),
			(int x, int y, int z) => new Tuple<string, int>("SapPool", 1),
			(int x, int y, int z) => new Tuple<string, int>("HoneyPool", 1)
		};
		List<Func<int, int, int, Tuple<string, int>>> input2 = new List<Func<int, int, int, Tuple<string, int>>>
		{
			(int x, int y, int z) => (z <= 5) ? new Tuple<string, int>(Z.GetDefaultWall(), COMMON_WALL_WEIGHT) : null,
			(int x, int y, int z) =>
			{
				if (z <= 5)
				{
					return new Tuple<string, int>("Shale", RARE_WALL_WEIGHT);
				}
				return (z <= 25) ? new Tuple<string, int>("Shale", COMMON_WALL_WEIGHT) : null;
			},
			(int x, int y, int z) =>
			{
				if (z <= 5)
				{
					return new Tuple<string, int>("Sandstone", RARE_WALL_WEIGHT);
				}
				return (z <= 25) ? new Tuple<string, int>("Sandstone", COMMON_WALL_WEIGHT) : null;
			},
			(int x, int y, int z) =>
			{
				if (z <= 5)
				{
					return new Tuple<string, int>("Limestone", RARE_WALL_WEIGHT);
				}
				return (z <= 25) ? new Tuple<string, int>("Limestone", COMMON_WALL_WEIGHT) : null;
			},
			(int x, int y, int z) =>
			{
				if (z <= 5)
				{
					return new Tuple<string, int>("Marl", RARE_WALL_WEIGHT);
				}
				return (z <= 15) ? new Tuple<string, int>("Marl", UNCOMMON_WALL_WEIGHT) : null;
			},
			(int x, int y, int z) =>
			{
				if (z <= 5)
				{
					return new Tuple<string, int>("Halite", RARE_WALL_WEIGHT);
				}
				return (z <= 25) ? new Tuple<string, int>("Halite", UNCOMMON_WALL_WEIGHT) : null;
			},
			(int x, int y, int z) =>
			{
				if (z <= 5)
				{
					return new Tuple<string, int>("Gypsum", RARE_WALL_WEIGHT);
				}
				return (z <= 25) ? new Tuple<string, int>("Gypsum", UNCOMMON_WALL_WEIGHT) : null;
			},
			(int x, int y, int z) =>
			{
				if (z < 15)
				{
					return (Tuple<string, int>)null;
				}
				if (z <= 15)
				{
					return new Tuple<string, int>("Oolite", RARE_WALL_WEIGHT);
				}
				return (z <= 35) ? new Tuple<string, int>("Oolite", UNCOMMON_WALL_WEIGHT) : null;
			},
			(int x, int y, int z) =>
			{
				if (z <= 15)
				{
					return (Tuple<string, int>)null;
				}
				if (z <= 25)
				{
					return new Tuple<string, int>("Slate", RARE_WALL_WEIGHT);
				}
				return (z <= 35) ? new Tuple<string, int>("Slate", UNCOMMON_WALL_WEIGHT) : new Tuple<string, int>("Slate", COMMON_WALL_WEIGHT);
			},
			(int x, int y, int z) =>
			{
				if (z <= 15)
				{
					return (Tuple<string, int>)null;
				}
				return (z <= 25) ? new Tuple<string, int>("Coral Rag", UNCOMMON_WALL_WEIGHT) : new Tuple<string, int>("Coral Rag", COMMON_WALL_WEIGHT);
			},
			(int x, int y, int z) =>
			{
				if (z <= 15)
				{
					return (Tuple<string, int>)null;
				}
				return (z <= 25) ? new Tuple<string, int>("Serpentinite", 1) : new Tuple<string, int>("Serpentinite", UNCOMMON_WALL_WEIGHT);
			},
			(int x, int y, int z) =>
			{
				if (z <= 15)
				{
					return (Tuple<string, int>)null;
				}
				return (z <= 25) ? new Tuple<string, int>("Quartzite", RARE_WALL_WEIGHT) : new Tuple<string, int>("Quartzite", UNCOMMON_WALL_WEIGHT);
			},
			(int x, int y, int z) => (z <= 25) ? null : new Tuple<string, int>("Black Shale", COMMON_WALL_WEIGHT),
			(int x, int y, int z) =>
			{
				if (z <= 25)
				{
					return (Tuple<string, int>)null;
				}
				return (z <= 35) ? new Tuple<string, int>("Black Marble", RARE_WALL_WEIGHT) : new Tuple<string, int>("Black Marble", UNCOMMON_WALL_WEIGHT);
			}
		};
		int depth = Z.Z - 10;
		List<Tuple<string, int>> list = resolveElements(Z.X + num, Z.Y + num2, depth, input2, (Tuple<string, int> e) =>
		{
			int item = e.Item2;
			item += (int)sampleSimplexNoise(e.Item1, gridX, gridY, depth, WALLTYPE_NOISE_AMPLITUDE, 0.33f);
			return new Tuple<string, int>(e.Item1, item);
		});
		List<string> list2 = list.Select((Tuple<string, int> e) => e.Item1).ToList();
		string text = getRandomWeightedFromElements(Z.X + num, Z.Y + num2, depth, input, (Tuple<string, int> e) =>
		{
			int item = e.Item2;
			item += (int)sampleSimplexNoise(e.Item1, gridX, gridY, depth, WALLTYPE_NOISE_AMPLITUDE);
			return new Tuple<string, int>(e.Item1, item);
		});
		Z.SetZoneProperty("DefaultWall", list2[0]);
		CellType[,] array;
		if (!(list2[0] == "Oolite") && !(list2[0] == "Marl"))
		{
			if (list2[0] == "Black Marble")
			{
				array = getPillarsLayout(Z);
			}
			else if (!(list2[0] == "Halite") && !(list2[0] == "Gypsum") && !(list2[0] == "Porous Coral Rag"))
			{
				if (!(list2[0] == "Limestone") && !(list2[0] == "Coral Rag"))
				{
					if (list2[0] == "Serpentinite" || list2[0] == "Sandstone")
					{
						array = getWindyLayout(Z);
					}
					else
					{
						array = ((!(list2[0] == "Quartzite")) ? getCaveLayout(Z) : getBlockyLayout(Z));
					}
				}
				else
				{
					array = getRandomWalkCaveLayout(Z);
				}
			}
			else
			{
				array = getPorousLayout(Z);
			}
		}
		else
		{
			array = getBoxFilterCave(Z);
		}
		CellType[,] array2;
		if (list2[1] == "Black Marble")
		{
			array2 = getPillarsLayout(Z);
		}
		else if (!(list2[1] == "Oolite") && !(list2[1] == "Marl"))
		{
			if (!(list2[1] == "Halite") && !(list2[1] == "Gypsum") && !(list2[0] == "Porous Coral Rag"))
			{
				if (list2[1] == "Quartzite")
				{
					array2 = getBlockyLayout(Z);
				}
				else if (!(list2[1] == "Limestone") && !(list2[1] == "Coral Rag"))
				{
					array2 = ((!(list2[1] == "Serpentinite") && !(list2[1] == "Sandstone")) ? getCaveLayout(Z) : getWindyLayout(Z));
				}
				else
				{
					array2 = getRandomWalkCaveLayout(Z);
				}
			}
			else
			{
				array2 = getPorousLayout(Z);
			}
		}
		else
		{
			array2 = getBoxFilterCave(Z);
		}
		double num3 = (double)list[1].Item2 / (double)list[0].Item2 * 0.5;
		num3 += sampleSimplexNoiseRange("secondaryThreshold", gridX, gridY, Z.Z, -0.2f, 0.2f);
		if (num3 > 0.5)
		{
			num3 = 0.5;
		}
		string region = Z.GetRegion();
		if (!string.IsNullOrEmpty(region) && region.Contains("dunes"))
		{
			text = null;
		}
		for (int num4 = 0; num4 < Z.Width; num4++)
		{
			for (int num5 = 0; num5 < Z.Height; num5++)
			{
				CellType cellType;
				string blueprint;
				if (sampleZonescaleSimplexNoiseRange("layoutSelection", num4 + num, num5 + num2, Z.Z, 0f, 1f) > num3)
				{
					cellType = array[num4, num5];
					blueprint = list2[0];
				}
				else
				{
					cellType = array2[num4, num5];
					blueprint = list2[1];
				}
				switch (cellType)
				{
				case CellType.Wall:
					Z.GetCell(num4, num5).AddObject(blueprint);
					break;
				case CellType.Detail:
					if (!string.IsNullOrEmpty(text))
					{
						Z.GetCell(num4, num5).AddObject(text);
					}
					break;
				}
			}
		}
		double num6 = (double)list[1].Item2 / (double)list[0].Item2 * 0.5;
		num6 += sampleSimplexNoiseRange("secondaryThreshold", gridX, gridY, Z.Z, -0.2f, 0.2f);
		if (num6 > 0.5)
		{
			num6 = 1.0 - num6;
		}
		GameObject gameObject = GameObject.Create(list[0].Item1);
		GameObject gameObject2 = GameObject.Create(list[1].Item1);
		string text2 = "&" + gameObject.Render.GetForegroundColor();
		string paintColorString = "&" + gameObject.Render.GetBackgroundColor();
		string paintColorString2 = "&" + gameObject2.Render.GetForegroundColor();
		string paintColorString3 = "&" + gameObject2.Render.GetBackgroundColor();
		Z.SetZoneProperty("primaryFloorColor1", text2);
		for (int num7 = 0; num7 < Z.Height; num7++)
		{
			for (int num8 = 0; num8 < Z.Width; num8++)
			{
				Cell cell = Z.GetCell(num8, num7);
				if (!cell.PaintTile.IsNullOrEmpty())
				{
					continue;
				}
				if (sampleZonescaleSimplexNoiseRange("layoutSelection", num8 + num, num7 + num2, Z.Z, 0f, 1f) > num3)
				{
					if (If.CoinFlip())
					{
						cell.PaintColorString = text2;
					}
					else
					{
						cell.PaintColorString = paintColorString;
					}
					cell.PaintTile = "Tiles/tile-dirt1.png";
					cell.PaintDetailColor = "k";
				}
				else
				{
					if (If.CoinFlip())
					{
						cell.PaintColorString = paintColorString2;
					}
					else
					{
						cell.PaintColorString = paintColorString3;
					}
					cell.PaintTile = "Tiles/tile-dirt1.png";
					cell.PaintDetailColor = "k";
				}
				if (If.Chance(5))
				{
					cell.PaintColorString = "&y";
				}
				int num9 = Stat.Random(1, 5);
				if (num9 == 1)
				{
					cell.PaintRenderString = ".";
				}
				if (num9 == 2)
				{
					cell.PaintRenderString = ".";
				}
				if (num9 == 3)
				{
					cell.PaintRenderString = ".";
				}
				if (num9 == 4)
				{
					cell.PaintRenderString = ".";
				}
			}
		}
		int num10 = sampleZonescaleSimplexNoiseIntRange("mazePathWidth", gridX, gridY, Z.Z, 0, 4);
		ConnectMaze(Z, num10, num10 + 1);
		return true;
	}
}
