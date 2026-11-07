using System;
using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.ZoneBuilders;

public class JoppaOutskirts : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("Dirty"));
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		NoiseMap noiseMap = new NoiseMap(Z.Width, Z.Height, 10, 1, 1, Stat.Random(30, 60), Stat.Random(50, 70), Stat.Random(125, 135), 0, 10, 0, 1, list, 5);
		if (Watervine.WatervineNoise == null)
		{
			List<PerlinNoise2Df> list2 = null;
			Random seededRand = GetSeededRand("WatervineNoise");
			list2 = new List<PerlinNoise2Df>();
			list2.Add(new PerlinNoise2Df(8, 0.86f, seededRand));
			list2.Add(new PerlinNoise2Df(32, 0.72f, seededRand));
			list2.Add(new PerlinNoise2Df(64, 0.6f, seededRand));
			list2.Add(new PerlinNoise2Df(128, 0.48f, seededRand));
			list2.Add(new PerlinNoise2Df(300, 0.36f, seededRand));
			list2.Add(new PerlinNoise2Df(600, 0.12f, seededRand));
			list2.Add(new PerlinNoise2Df(1200, 0.06f, seededRand));
			Watervine.WatervineNoise = PerlinNoise2Df.sumNoiseFunctions(1200, 375, 0, 0, list2, 0.5f);
			Watervine.WatervineNoise = PerlinNoise2Df.Smooth(Watervine.WatervineNoise, 1200, 375, 1);
		}
		int num = Z.wX * 240 + Z.X * 80;
		int num2 = Z.wY * 75 + Z.Y * 25;
		num %= 1200;
		num2 %= 375;
		double[,] array = new double[Z.Width, Z.Height];
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				array[j, i] = 0.0;
			}
		}
		if (Z.X == 0 && Z.wX > 0 && !ZoneManager.GetObjectTypeForZone(Z.wX - 1, Z.wY, Z.GetZoneWorld()).Contains("TerrainSaltmarsh"))
		{
			for (int k = 0; k < Z.Height; k++)
			{
				for (int l = 0; l < 10; l++)
				{
					array[l, k] = -1.0 + 0.05 * (double)l;
				}
			}
		}
		if (Z.X == 2 && Z.wX < 79)
		{
			string objectTypeForZone = ZoneManager.GetObjectTypeForZone(Z.wX + 1, Z.wY, Z.GetZoneWorld());
			if (!objectTypeForZone.Contains("TerrainSaltmarsh") && !objectTypeForZone.Contains("TerrainWatervine"))
			{
				for (int m = 0; m < Z.Height; m++)
				{
					for (int n = Z.Width - 10; n < Z.Width; n++)
					{
						array[n, m] = -1.0 + 0.05 * (double)(Z.Width - n);
					}
				}
			}
		}
		if (Z.Y == 0 && Z.wY > 0)
		{
			string objectTypeForZone2 = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY - 1, Z.GetZoneWorld());
			if (!objectTypeForZone2.Contains("TerrainSaltmarsh") && !objectTypeForZone2.Contains("TerrainWatervine"))
			{
				for (int num3 = 0; num3 < 10; num3++)
				{
					for (int num4 = 0; num4 < Z.Width; num4++)
					{
						array[num4, num3] = -1.0 + 0.05 * (double)num3;
					}
				}
			}
		}
		if (Z.Y == 2 && Z.wY < 24)
		{
			string objectTypeForZone3 = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY + 1, Z.GetZoneWorld());
			if (!objectTypeForZone3.Contains("TerrainSaltmarsh") && !objectTypeForZone3.Contains("TerrainWatervine"))
			{
				for (int num5 = Z.Height - 10; num5 < Z.Height; num5++)
				{
					for (int num6 = 0; num6 < Z.Width; num6++)
					{
						array[num6, num5] = -1.0 + 0.05 * (double)(Z.Height - num5);
					}
				}
			}
		}
		if (Z.X == 0 && Z.wX > 0)
		{
			string objectTypeForZone4 = ZoneManager.GetObjectTypeForZone(Z.wX - 1, Z.wY, Z.GetZoneWorld());
			if (!objectTypeForZone4.Contains("TerrainSaltmarsh") && !objectTypeForZone4.Contains("TerrainWatervine"))
			{
				for (int num7 = 0; num7 < Z.Height; num7++)
				{
					for (int num8 = 0; num8 < 10; num8++)
					{
						array[num8, num7] = -1.0 + 0.1 * (double)num8;
					}
				}
			}
		}
		for (int num9 = 0; num9 < Z.Height; num9++)
		{
			for (int num10 = 0; num10 < Z.Width; num10++)
			{
				double num11 = (double)Watervine.WatervineNoise[num10 + num, num9 + num2] + array[num10, num9];
				if (noiseMap.Noise[num10, num9] >= 5)
				{
					Z.GetCell(num10, num9).AddObject(GameObjectFactory.Factory.CreateObject("SaltyWaterPuddle"));
				}
				else if ((double)noiseMap.Noise[num10, num9] >= 3.5)
				{
					Z.GetCell(num10, num9).AddObject(GameObjectFactory.Factory.CreateObject("Watervine"));
				}
				else if (num11 >= 0.8)
				{
					Z.GetCell(num10, num9).AddObject(GameObjectFactory.Factory.CreateObject("SaltyWaterPuddle"));
				}
				else if (num11 >= 0.7)
				{
					if (Stat.Random(1, 100) <= 98)
					{
						Z.GetCell(num10, num9).AddObject(GameObjectFactory.Factory.CreateObject("Watervine"));
					}
				}
				else
				{
					Z.GetCell(num10, num9).SetReachable(State: true);
				}
			}
		}
		List<Cell> list3 = new List<Cell>();
		if (Z.X == 1 && Z.Y == 0)
		{
			InfluenceMap influenceMap = ZoneBuilderSandbox.GenerateInfluenceMap(Z, null, InfluenceMapSeedStrategy.RandomPointFurtherThan4, 600);
			if (influenceMap.Regions.Count > 0)
			{
				InfluenceMapRegion randomElement = influenceMap.Regions.GetRandomElement();
				Rect2D r = GridTools.MaxRectByArea(randomElement.GetGrid()).Translate(randomElement.BoundingBox.UpperLeft);
				if (r.x1 == 0)
				{
					r = new Rect2D(1, r.y1, r.x2, r.y2);
				}
				if (r.x2 == Z.Width - 1)
				{
					r = new Rect2D(r.x1, r.y1, Z.Width - 2, r.y2);
				}
				if (r.y1 == 0)
				{
					r = new Rect2D(r.x1, 1, r.x2, r.y2);
				}
				if (r.y2 == Z.Height - 1)
				{
					r = new Rect2D(r.x1, r.y1, r.x2, Z.Height - 2);
				}
				string wall = "?";
				if (randomElement.Center.X <= 40)
				{
					wall = "E";
				}
				if (randomElement.Center.X >= 40)
				{
					wall = "W";
				}
				if (r.y2 > 16)
				{
					wall = "N";
				}
				if (r.y1 < 8)
				{
					wall = "S";
				}
				if (r.y1 <= 0)
				{
					wall = "S";
				}
				if (r.y2 >= 24)
				{
					wall = "N";
				}
				if (r.y1 <= 0 && r.y2 >= 24)
				{
					if (randomElement.Center.X <= 40)
					{
						wall = "E";
					}
					if (randomElement.Center.X >= 40)
					{
						wall = "W";
					}
				}
				Point2D randomDoorCell = r.GetRandomDoorCell(wall);
				ZoneBuilderSandbox.PlaceObjectOnRect(Z, "BrinestalkFence", r);
				GetCell(Z, randomDoorCell).Clear();
				GetCell(Z, randomDoorCell).AddObject("Brinestalk Gate");
				list3.Add(GetCell(Z, randomDoorCell));
				string cellSide = r.GetCellSide(randomDoorCell);
				Rect2D r2 = r.ReduceBy(0, 0);
				int num12 = 0;
				if (cellSide == "N")
				{
					num12 = ((Stat.Random(0, 1) == 0) ? 2 : 3);
				}
				if (cellSide == "S")
				{
					num12 = ((Stat.Random(0, 1) != 0) ? 1 : 0);
				}
				if (cellSide == "E")
				{
					num12 = ((Stat.Random(0, 1) != 0) ? 3 : 0);
				}
				if (cellSide == "W")
				{
					num12 = ((Stat.Random(0, 1) == 0) ? 1 : 2);
				}
				if (num12 == 0 || num12 == 1)
				{
					r2.y2 = r2.y1 + 3;
				}
				else
				{
					r2.y1 = r2.y2 - 3;
				}
				if (num12 == 0 || num12 == 3)
				{
					r2.x2 = r2.x1 + 3;
				}
				else
				{
					r2.x1 = r2.x2 - 3;
				}
				ClearRect(Z, r2);
				ZoneBuilderSandbox.PlaceObjectOnRect(Z, "BrinestalkWall", r2);
				Point2D randomDoorCell2 = r2.GetRandomDoorCell(cellSide, 1);
				Z.GetCell(randomDoorCell2).Clear();
				Z.GetCell(randomDoorCell2).AddObject("Door");
				int num13 = Stat.Random(3, 10);
				for (int num14 = 0; num14 < num13; num14++)
				{
					ZoneBuilderSandbox.PlaceObjectInRect(Z, r.ReduceBy(1, 1), "Tombstone");
				}
				int prefabWidth = 5;
				int prefabHeight = 3;
				List<Cell> cells = Z.GetCells((Cell c) =>
				{
					if (c.X > 75)
					{
						return false;
					}
					if (c.Y > 20)
					{
						return false;
					}
					if (!c.HasObjectWithPart("LiquidVolume"))
					{
						return false;
					}
					for (int num19 = 0; num19 < prefabHeight; num19++)
					{
						for (int num20 = 0; num20 < prefabWidth; num20++)
						{
							Cell cellFromOffset = c.GetCellFromOffset(num20, num19);
							if (cellFromOffset == null || !cellFromOffset.HasObjectWithPart("LiquidVolume"))
							{
								return false;
							}
						}
					}
					return true;
				});
				if (cells.Count <= 0)
				{
					cells.Add(Z.GetRandomCell(Math.Max(prefabWidth, prefabHeight)));
				}
				Cell randomElement2 = cells.GetRandomElement();
				for (int num15 = 0; num15 < prefabHeight; num15++)
				{
					for (int num16 = 0; num16 < prefabWidth; num16++)
					{
						randomElement2.GetCellFromOffset(num16, num15).Clear();
					}
				}
				randomElement2.AddObject("JoppaSultanShrine_5x3");
				list3.Add(randomElement2.GetCellFromOffset(prefabHeight / 2, prefabHeight - 1));
			}
		}
		using (Pathfinder pathfinder = Z.getPathfinder())
		{
			for (int num17 = 0; num17 < Z.Height; num17++)
			{
				for (int num18 = 0; num18 < Z.Width; num18++)
				{
					if (Z.GetCell(num18, num17).HasWall())
					{
						pathfinder.CurrentNavigationMap[num18, num17] = 9999;
					}
					else if (Z.GetCell(num18, num17).HasObjectWithPart("LiquidVolume"))
					{
						pathfinder.CurrentNavigationMap[num18, num17] = 200;
					}
					else
					{
						pathfinder.CurrentNavigationMap[num18, num17] = Stat.Random(0, 50);
					}
				}
			}
			Cell cell = Z.GetCell(52, 24);
			foreach (Cell item in list3)
			{
				if (!pathfinder.FindPath(cell.Location, item.Location, Display: false, CardinalDirectionsOnly: true))
				{
					continue;
				}
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Z.GetCell(step.pos).ClearObjectsWithTag("Plant");
					Z.GetCell(step.pos).ClearObjectsWithTag("PlantLike");
					Z.GetCell(step.pos).ClearObjectsWithTag("LiquidVolume");
					Z.GetCell(step.pos).AddObject("DirtPath");
				}
				cell = Z.GetCell(pathfinder.Steps.GetRandomElement().pos);
			}
		}
		Z.ClearReachableMap();
		if (Z.BuildReachableMap(0, 0) < 400)
		{
			return false;
		}
		return true;
	}
}
