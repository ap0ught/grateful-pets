using System;
using System.Collections.Generic;
using Genkit;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class MoonStair : ZoneBuilderSandbox
{
	[Serializable]
	public class MoonStairZoomiesDefinition : IComposite
	{
		public List<Point2D> zoomieStarts = new List<Point2D>();

		public List<Point2D> zoomieEnds = new List<Point2D>();

		public Dictionary<Point2D, string> zoomieColors = new Dictionary<Point2D, string>();

		public void set(int x, int y, string color)
		{
			set(new Point2D(x, y), color);
		}

		public void set(Point2D pos, string color)
		{
			if (zoomieColors.ContainsKey(pos))
			{
				zoomieColors[pos] = color;
			}
			else
			{
				zoomieColors.Add(pos, color);
			}
		}

		public string get(Point2D pos)
		{
			if (zoomieColors.ContainsKey(pos))
			{
				return zoomieColors[pos];
			}
			return null;
		}
	}

	private FastNoise fastNoise = new FastNoise();

	public static readonly char[] ImpactZoomieColor = new char[5] { 'C', 'W', 'B', 'G', 'R' };

	public static readonly char[] ZoomieColor = new char[2] { 'C', 'W' };

	public static readonly char[] HexColor = new char[4] { 'm', 'M', 'y', 'Y' };

	public static void Save(SerializationWriter Writer)
	{
	}

	public static void Load(SerializationReader Reader)
	{
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

	public double sampleCellularNoise(string type, int x, int y, int z, float frequencyMultiplier = 1f)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
		fastNoise.SetFrequency(0.2f * frequencyMultiplier);
		fastNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Euclidean);
		fastNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Add);
		fastNoise.SetCellularDistance2Indicies(0, 1);
		fastNoise.SetCellularJitter(0.45f);
		return fastNoise.GetNoise(x, y, z);
	}

	public double sampleCellularNoise2(string type, int x, int y, int z, float frequencyMultiplier = 1f)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
		fastNoise.SetFrequency(0.4f * frequencyMultiplier);
		fastNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Euclidean);
		fastNoise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);
		fastNoise.SetCellularDistance2Indicies(0, 1);
		fastNoise.SetCellularJitter(0.45f);
		return fastNoise.GetNoise(x, y, z);
	}

	public void PaintCrystals(Zone Z, int x, int y, string color)
	{
		int num = x / 3;
		int num2 = x / 3 % 2;
		int num3 = y / 2;
		for (int i = num * 3; i < num * 3 + 3; i++)
		{
			for (int j = num3 * 2 + num2; j < num3 * 2 + 2 + num2; j++)
			{
				if (Z.GetCell(i, j) != null && Z.GetCell(i, j).HasObject("CrystalWall"))
				{
					Render render = Z.GetCell(i, j).GetObjects("CrystalWall")[0].Render;
					render.ColorString = "&" + color + "^k";
					render.TileColor = "&" + color + "^k";
				}
			}
		}
	}

	public int sampleZonescaleSimplexNoiseIntRange(string type, int x, int y, int z, int low, int high)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.1f);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		float num = (fastNoise.GetNoise(x, y, z) * 1.8f + 1f) / 2f;
		num *= (float)high - (float)low;
		num += (float)low;
		return Math.Min(Math.Max(low, (int)Math.Round(num)), high);
	}

	public void SpiralZoomie(MoonStairZoomiesDefinition zoomies, Rect2D zoomieArea, Point2D start, string color = null, int depth = 0)
	{
		zoomies.zoomieStarts.Add(start);
		int num = Stat.Random(30, 2800);
		float num2 = 1f;
		float num3 = Stat.Random(1, 6);
		float num4 = (float)Stat.Random(400, 1600) / 800f;
		float num5 = (float)Stat.Random(20, 80) / 800f;
		if (Stat.Random(1, 100) <= 50)
		{
			num5 = 0f - num5;
		}
		int num6 = Stat.Random(1, 5);
		int num7 = start.x;
		int num8 = start.y;
		if (color == null)
		{
			color = ZoomieColor.GetRandomElement().ToString();
		}
		HashSet<Point2D> hashSet = new HashSet<Point2D>();
		hashSet.Add(start);
		for (int i = 0; i < num; i++)
		{
			int num9 = Convert.ToInt32(start.x) + (int)((double)num2 * Math.Cos(num3));
			int num10 = Convert.ToInt32(start.y) + (int)((double)num2 * Math.Sin(num3) * 0.66);
			if (!zoomieArea.Contains(num9, num10))
			{
				break;
			}
			foreach (Pair item in MissileWeapon.ListOfVisitedSquares(num7, num8, num9, num10))
			{
				if (!hashSet.Contains(new Point2D(item.x, item.y)))
				{
					if (zoomies.get(new Point2D(item.x, item.y)) != null)
					{
						if (depth < 3)
						{
							if (Stat.Random(1, 2) == 1)
							{
								SpiralZoomie(zoomies, zoomieArea, new Point2D(item.x, item.y), ImpactZoomieColor.GetRandomElement().ToString(), depth + 1);
							}
							else
							{
								BurstZoomie(zoomies, zoomieArea, new Point2D(item.x, item.y), ImpactZoomieColor.GetRandomElement().ToString(), depth + 1);
							}
						}
						return;
					}
					hashSet.Add(new Point2D(item.x, item.y));
				}
				zoomies.set(item.x, item.y, color);
			}
			num7 = num9;
			num8 = num10;
			for (int j = 0; j < num6; j++)
			{
				num2 += num4;
				for (num3 += num5; num3 < 0f; num3 += 6.28f)
				{
				}
				while ((double)num3 > 6.28)
				{
					num3 -= 6.28f;
				}
			}
		}
		zoomies.zoomieEnds.Add(new Point2D(num7, num8));
	}

	public void BurstZoomie(MoonStairZoomiesDefinition zoomies, Rect2D zoomieArea, Point2D start, string color = null, int depth = 0)
	{
		zoomies.zoomieStarts.Add(start);
		int num = Stat.Random(1, 8);
		if (color == null)
		{
			color = ZoomieColor.GetRandomElement().ToString();
		}
		HashSet<Point2D> hashSet = new HashSet<Point2D>();
		hashSet.Add(start);
		for (int i = 0; i < num; i++)
		{
			int num2 = Stat.Random(8, 80);
			int num3 = Stat.Random(1, 9);
			int num4 = Stat.Random(1, 6);
			if (Stat.Random(1, 2) == 1)
			{
				num3 = -num3;
			}
			if (Stat.Random(1, 2) == 1)
			{
				num4 = -num4;
			}
			int num5 = start.x;
			int num6 = start.y;
			for (int j = 0; j < num2; j++)
			{
				int num7 = num5 + num3;
				int num8 = num6 + num4;
				if (!zoomieArea.Contains(num7, num8))
				{
					break;
				}
				foreach (Pair item in MissileWeapon.ListOfVisitedSquares(num5, num6, num7, num8))
				{
					if (!hashSet.Contains(new Point2D(item.x, item.y)))
					{
						if (zoomies.get(new Point2D(item.x, item.y)) != null)
						{
							if (depth < 3)
							{
								if (Stat.Random(1, 2) == 1)
								{
									SpiralZoomie(zoomies, zoomieArea, new Point2D(item.x, item.y), ImpactZoomieColor.GetRandomElement().ToString(), depth + 1);
								}
								else
								{
									BurstZoomie(zoomies, zoomieArea, new Point2D(item.x, item.y), ImpactZoomieColor.GetRandomElement().ToString(), depth + 1);
								}
							}
							goto end_IL_01e8;
						}
						hashSet.Add(new Point2D(item.x, item.y));
					}
					zoomies.set(item.x, item.y, color);
				}
				num5 += num3;
				num6 += num4;
				continue;
				end_IL_01e8:
				break;
			}
			zoomies.zoomieEnds.Add(new Point2D(num5, num6));
		}
	}

	public void MakeZoomies()
	{
		MoonStairZoomiesDefinition moonStairZoomiesDefinition = new MoonStairZoomiesDefinition();
		int num = 10;
		Rect2D zoomieArea = new Rect2D(17280, 150, 18720, 1275);
		for (int i = 0; i < num; i++)
		{
			int num2 = Stat.Random(1, 2);
			if (num2 == 1)
			{
				SpiralZoomie(moonStairZoomiesDefinition, zoomieArea, zoomieArea.randomPoint());
			}
			if (num2 == 2)
			{
				BurstZoomie(moonStairZoomiesDefinition, zoomieArea, zoomieArea.randomPoint());
			}
		}
		The.Game.SetObjectGameState("MoonStairZoomies", moonStairZoomiesDefinition);
	}

	public bool BuildZone(Zone Z)
	{
		if (!The.Game.HasObjectGameState("MoonStairZoomies"))
		{
			MakeZoomies();
		}
		MoonStairZoomiesDefinition moonStairZoomiesDefinition = The.Game.GetObjectGameState("MoonStairZoomies") as MoonStairZoomiesDefinition;
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		_ = Options.EnablePrereleaseContent;
		Z.ClearReachableMap();
		string blueprint = "Black Marble";
		int num = Z.wX * 3 + Z.X;
		int num2 = Z.wY * 3 + Z.Y;
		int num3 = sampleZonescaleSimplexNoiseIntRange("MoonStairRockAmount", num, num2, Z.Z, 0, 24);
		for (int i = 0; i < num3; i++)
		{
			int num4 = Stat.Random(3, Math.Max(12, num3));
			int num5 = Stat.Random(2, Math.Max(7, num3 / 2));
			int num6 = Stat.Random(0, 79 - num4);
			int num7 = Stat.Random(0, 24 - num5);
			for (int j = num6; j < num6 + num4; j++)
			{
				for (int k = num7; k < num7 + num5; k++)
				{
					Z.GetCell(j, k).ClearWalls();
					Z.GetCell(j, k).AddObject(blueprint);
				}
			}
		}
		ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY, Z.GetZoneWorld());
		int num8 = 0;
		for (int l = 0; l < Z.Height; l++)
		{
			for (int m = 0; m < Z.Width; m++)
			{
				if (sampleCellularNoise("rock", Z.ResolvedStartX + m, (int)((float)(Z.ResolvedStartY + l) * 1.5f), Z.Z) >= 1.25)
				{
					num8++;
					Z.GetCell(m, l).ClearWalls();
				}
			}
		}
		int num9 = sampleZonescaleSimplexNoiseIntRange("MoonStairCrystalAmount", num, num2, Z.Z, 0, 80);
		for (int n = 0; n < Z.Width / 3; n++)
		{
			for (int num10 = 0; num10 < Z.Height / 2; num10++)
			{
				int num11 = n * 3;
				int num12 = num10 * 2 + n % 2;
				if (Stat.Random(1, 100) >= num9)
				{
					continue;
				}
				for (int num13 = 0; num13 < 3; num13++)
				{
					for (int num14 = 0; num14 < 2; num14++)
					{
						Z.GetCell(num11 + num13, num12 + num14).ClearWalls();
						Z.GetCell(num11 + num13, num12 + num14).AddObject("CrystalWall");
					}
				}
			}
		}
		InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(Z, null, InfluenceMapSeedStrategy.FurthestPoint, int.MaxValue, null, bDraw: false, (Cell c) => (!c.HasObject("CrystalWall")) ? 1 : 0);
		influenceMap.SeedAllUnseeded();
		foreach (InfluenceMapRegion region in influenceMap.Regions)
		{
			string color = HexColor.GetRandomElement().ToString();
			foreach (Location2D cell4 in region.Cells)
			{
				PaintCrystals(Z, cell4.X, cell4.Y, color);
			}
		}
		foreach (Cell item in Z.GetCellsWithObject("CrystalWall"))
		{
			foreach (Cell localAdjacentCell in item.GetLocalAdjacentCells())
			{
				if (localAdjacentCell.HasObject(blueprint))
				{
					localAdjacentCell.RemoveObject(localAdjacentCell.FindObject(blueprint));
				}
			}
		}
		for (int num15 = 0; num15 < Z.Width; num15++)
		{
			for (int num16 = 0; num16 < Z.Height; num16++)
			{
				int x = num * Z.Width + num15;
				int y = num2 * Z.Height + num16;
				string text = moonStairZoomiesDefinition.get(new Point2D(x, y));
				if (text != null)
				{
					Cell cell = Z.GetCell(num15, num16);
					if (cell != null && !cell.HasObject("SmallHexFloor") && Stat.Random(1, 100) <= 75)
					{
						cell.AddObject("SmallHexFloor").Render.ColorString = "&" + text;
					}
					PaintCrystals(Z, num15, num16, text);
				}
			}
		}
		foreach (Point2D zoomieStart in moonStairZoomiesDefinition.zoomieStarts)
		{
			int num17 = num * Z.Width;
			int num18 = num17 + Z.Width;
			int num19 = num2 * Z.Height;
			int num20 = num19 + Z.Height;
			if (zoomieStart.x >= num17 && zoomieStart.x < num18 && zoomieStart.y >= num19 && zoomieStart.y < num20)
			{
				Cell cell2 = Z.GetCell(zoomieStart.x - num17, zoomieStart.y - num19);
				if (cell2 != null)
				{
					cell2.AddObject("Space-Time Rift");
					The.Game.SetIntGameState("zoomnodes", The.Game.GetIntGameState("zoomnodes"));
				}
			}
		}
		foreach (Point2D zoomieEnd in moonStairZoomiesDefinition.zoomieEnds)
		{
			int num21 = num * Z.Width;
			int num22 = num21 + Z.Width;
			int num23 = num2 * Z.Height;
			int num24 = num23 + Z.Height;
			if (zoomieEnd.x >= num21 && zoomieEnd.x < num22 && zoomieEnd.y >= num23 && zoomieEnd.y < num24)
			{
				Cell cell3 = Z.GetCell(zoomieEnd.x - num21, zoomieEnd.y - num23);
				if (cell3 != null)
				{
					cell3.AddObject("Space-Time Rift");
					The.Game.SetIntGameState("zoomnodes", The.Game.GetIntGameState("zoomnodes"));
				}
			}
		}
		for (int num25 = 0; num25 < 78; num25 += 2)
		{
			for (int num26 = 0; num26 < Z.Height; num26++)
			{
				if (Z.GetCell(num25, num26).HasObject("SmallHexFloor") && Z.GetCell(num25 + 1, num26).HasObject("SmallHexFloor"))
				{
					Z.GetCell(num25, num26).GetObjects("SmallHexFloor")[0].Render.Tile = "Terrain/sw_hex_dotted_2_l.bmp";
					Z.GetCell(num25 + 1, num26).GetObjects("SmallHexFloor")[0].Render.Tile = "Terrain/sw_hex_dotted_2_r.bmp";
				}
			}
		}
		for (int num27 = 0; num27 < Z.Height; num27++)
		{
			for (int num28 = 0; num28 < 78; num28 += 2)
			{
				if (Z.GetCell(num28, num27).HasObject("SmallHexFloor") && Z.GetCell(num28 + 1, num27).HasObject("SmallHexFloor") && !Z.GetCell(num28, num27).HasWall() && !Z.GetCell(num28 + 1, num27).HasWall())
				{
					Z.GetCell(num28, num27).GetObjects("SmallHexFloor")[0].Render.Tile = "Terrain/sw_hex_dotted_2_l.bmp";
					Z.GetCell(num28 + 1, num27).GetObjects("SmallHexFloor")[0].Render.Tile = "Terrain/sw_hex_dotted_2_r.bmp";
					Debug.Log("painting " + Z.GetCell(num28, num27).GetObjects("SmallHexFloor")[0].CurrentCell.Pos2D.ToString() + " and " + Z.GetCell(num28 + 1, num27).GetObjects("SmallHexFloor")[0].CurrentCell.Pos2D.ToString());
				}
			}
		}
		Z.ClearReachableMap();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (!Z.IsReachable(zoneConnection.X, zoneConnection.Y))
			{
				Z.BuildReachableMap(zoneConnection.X, zoneConnection.Y, bClearFirst: false);
			}
		}
		Z.BuildReachableMap();
		Z.BuildReachabilityFromEdges();
		return true;
	}
}
