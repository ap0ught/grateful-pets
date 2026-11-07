using System;
using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class RiverBuilder
{
	public string Puddle = "SaltyWaterDeepPool";

	public bool Pairs;

	public bool HardClear;

	private bool VillageMode;

	public RiverBuilder()
	{
	}

	public RiverBuilder(bool hardClear, string Puddle = "SaltyWaterDeepPool", bool VillageMode = false)
	{
		this.VillageMode = VillageMode;
		this.Puddle = Puddle;
		HardClear = hardClear;
	}

	public bool BuildZone(Zone Z)
	{
		if (Z.IsCheckpoint())
		{
			VillageMode = true;
		}
		if (Z.GetTerrainObject().HasTag("NoRivers"))
		{
			return true;
		}
		if (Z.GetTerrainObject().Blueprint == "TerrainSaltdunes" || Z.GetTerrainObject().Blueprint == "TerrainSaltdunes2" || Z.GetTerrainObject().Blueprint == "TerrainTremblezone")
		{
			Puddle = "SaltDeepPool";
		}
		if (Z.GetTerrainObject().GetBlueprint().InheritsFrom("TerrainFungalOuter"))
		{
			Puddle = "ProteanDeepPool";
		}
		if (Z.GetTerrainObject().GetBlueprint().InheritsFrom("TerrainPalladiumReef") || Z.GetTerrainObject().GetBlueprint().InheritsFrom("TerrainLakeHinnom"))
		{
			Puddle = "AlgalWaterDeepPool";
		}
		if (Z.GetTerrainObject().Blueprint == "TerrainMoonStair" || Z.GetTerrainObject().GetBlueprint().InheritsFrom("TerrainMoonStair"))
		{
			Puddle = "DilutedWarmStaticPuddle";
		}
		List<CachedZoneConnection> list = new List<CachedZoneConnection>();
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-" && item.Type.Contains("River"))
			{
				list.Add(item);
			}
		}
		int num = 40;
		int num2 = 20;
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type.Contains("River"))
			{
				if (zoneConnection.Type.Contains("Start"))
				{
					num = zoneConnection.X;
					num2 = zoneConnection.Y;
				}
				list.Add(new CachedZoneConnection("-", zoneConnection.X, zoneConnection.Y, zoneConnection.Type, null));
			}
		}
		bool flag = list.Count == 1;
		if (list.Count <= 1)
		{
			num = Stat.Random(5, 75);
			num2 = Stat.Random(5, 20);
		}
		else
		{
			num = list[0].X;
			num2 = list[0].Y;
		}
		if (Z.BuildTries > 5 || Pairs)
		{
			GameObjectFactory.Factory.CreateObject("Drillbot");
		}
		if (flag)
		{
			CellularGrid cellularGrid = new CellularGrid();
			cellularGrid.SeedBorders = false;
			cellularGrid.Passes = 4;
			cellularGrid.SeedChance = 40;
			cellularGrid.Generate(Stat.Rand, 80, 30);
			int i = 1;
			for (int num3 = Z.Height - 1; i < num3; i++)
			{
				int j = 1;
				for (int num4 = Z.Width - 1; j < num4; j++)
				{
					if (cellularGrid.cells[j, i] == 1)
					{
						Z.ReachableMap[j, i] = true;
						if (HardClear)
						{
							Z.GetCell(j, i).Clear();
						}
						else
						{
							Z.GetCell(j, i).ClearWalls();
						}
						Z.GetCell(j, i).AddObject(Puddle);
					}
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			if (num == list[k].X && num2 == list[k].Y)
			{
				continue;
			}
			FastNoise pathNoise = new FastNoise();
			pathNoise.SetSeed(Stat.Random(int.MinValue, int.MaxValue));
			pathNoise.SetNoiseType(FastNoise.NoiseType.Simplex);
			pathNoise.SetFractalOctaves(4);
			pathNoise.SetFrequency(0.1f);
			Pathfinder pathfinder = Z.getPathfinder((int x, int y, Cell c) =>
			{
				int num5 = 0;
				num5 = (int)(Math.Abs(pathNoise.GetNoise((x + Z.wX * 80) / 3, y + Z.wY * 25)) * 190f);
				if (VillageMode)
				{
					num5 = 0;
				}
				if (Z.GetCell(x, y).HasWall())
				{
					if (!VillageMode)
					{
						return 20 + num5;
					}
					return 8000;
				}
				return (VillageMode && Z.GetCell(x, y).AnyLocalAdjacentCell((Cell cell3) => cell3.HasWall())) ? 8000 : num5;
			});
			if (pathfinder.FindPath(Location2D.Get(num, num2), Location2D.Get(list[k].X, list[k].Y), Display: false, CardinalDirectionsOnly: true, 24300, ShuffleDirections: true))
			{
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Cell cell = Z.GetCell(step.X, step.Y);
					if (VillageMode)
					{
						if (cell.AnyObject((GameObject o) => o.GetBlueprint().DescendsFrom("Floor")))
						{
							cell.AddObject("Bridge");
						}
						else if (HardClear)
						{
							cell.Clear();
						}
						else
						{
							cell.ClearTerrain();
						}
					}
					else if (HardClear)
					{
						cell.Clear();
					}
					else
					{
						cell.ClearTerrain();
					}
					Z.ReachableMap[step.X, step.Y] = true;
					cell.AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
				}
			}
			int radius = 2;
			if (VillageMode)
			{
				radius = 1;
			}
			foreach (PathfinderNode step2 in pathfinder.Steps)
			{
				Cell cell2 = Z.GetCell(step2.X, step2.Y);
				foreach (Cell localAdjacentCell in cell2.GetLocalAdjacentCells(radius, IncludeSelf: true))
				{
					if (localAdjacentCell.CosmeticDistanceTo(cell2.Pos2D) > 1)
					{
						continue;
					}
					if (VillageMode)
					{
						if (localAdjacentCell.AnyObject((GameObject o) => o.GetBlueprint().DescendsFrom("Floor")))
						{
							localAdjacentCell.AddObject("Bridge");
						}
						else if (localAdjacentCell == cell2 || localAdjacentCell.IsEmpty())
						{
							Z.ReachableMap[step2.X, step2.Y] = true;
							localAdjacentCell.ClearTerrain();
							if (localAdjacentCell.IsEmpty())
							{
								localAdjacentCell.AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
							}
						}
						continue;
					}
					Z.ReachableMap[localAdjacentCell.X, localAdjacentCell.Y] = true;
					if (HardClear)
					{
						localAdjacentCell.Clear();
						localAdjacentCell.AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
						continue;
					}
					localAdjacentCell.ClearTerrain();
					if (localAdjacentCell.IsEmpty())
					{
						localAdjacentCell.AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
					}
				}
			}
			pathfinder.Dispose();
		}
		if (list.Count > 0)
		{
			Z.SetZoneProperty("ambient_bed_3", "Sounds/Ambiences/amb_bed_river");
		}
		return true;
	}
}
