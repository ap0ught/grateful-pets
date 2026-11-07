using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class CaveCity : ZoneBuilderSandbox
{
	public static bool MakeRegionCaveBuilding(Zone Z, InfluenceMap IF, InfluenceMapRegion R, string DoorObject = "Door", string WallObject = "BrinestalkWall", int WallRadius = 4, string BuildingDecorationPopulation = null, string BuildingCreaturePopulation = null, Location2D Anchor = null)
	{
		bool flag = false;
		List<Location2D> list = new List<Location2D>();
		foreach (Location2D cell in R.Cells)
		{
			if (IF.CostMap[cell.X, cell.Y] < WallRadius)
			{
				list.Add(cell);
			}
			if (IF.CostMap[cell.X, cell.Y] != WallRadius)
			{
				continue;
			}
			if (!flag)
			{
				if (Anchor == null)
				{
					Z.GetCell(cell.X, cell.Y).AddObject(DoorObject);
				}
				if (Anchor != null)
				{
					Z.GetCell(cell.X, cell.Y).AddObject(WallObject);
				}
				flag = true;
			}
			else
			{
				Z.GetCell(cell.X, cell.Y).AddObject(WallObject);
			}
		}
		if (flag)
		{
			if (BuildingDecorationPopulation != null)
			{
				ZoneBuilderSandbox.PlacePopulationInRegion(Z, list, BuildingDecorationPopulation);
			}
			if (BuildingCreaturePopulation != null)
			{
				ZoneBuilderSandbox.PlacePopulationInRegion(Z, list, BuildingCreaturePopulation);
			}
		}
		return flag;
	}

	public bool BuildZone(Zone Z, InfluenceMap IF, string DoorObject = "Door", string WallObject = "BrinestalkWall", int MaxAdjacentRegions = 1, int MinRegionSize = 9, int MinRegionHeight = 3, int MinRegionWidth = 3, string WallRadius = "4", int ChancePerZone = 100, string BuildingDecorationPopulation = null, string BuildingCreaturePopulation = null, string BuildingBossPopulation = null, Location2D Anchor = null)
	{
		List<InfluenceMapRegion> list = new List<InfluenceMapRegion>();
		bool flag = false;
		foreach (InfluenceMapRegion region in IF.Regions)
		{
			if (region.AdjacentRegions.Count > MaxAdjacentRegions || Stat.Random(1, 100) > ChancePerZone || region.Size < MinRegionSize || region.BoundingBox.Width < MinRegionWidth || region.BoundingBox.Height < MinRegionHeight || region.Cells.Any((Location2D c) => Z.GetCell(c).HasStairs()))
			{
				continue;
			}
			if (!flag)
			{
				if (MakeRegionCaveBuilding(Z, IF, region, DoorObject, WallObject, Stat.Roll(WallRadius), BuildingDecorationPopulation, BuildingBossPopulation, Anchor))
				{
					list.Add(region);
					flag = true;
				}
			}
			else if (MakeRegionCaveBuilding(Z, IF, region, DoorObject, WallObject, Stat.Roll(WallRadius), BuildingDecorationPopulation, BuildingCreaturePopulation, Anchor))
			{
				list.Add(region);
			}
		}
		foreach (InfluenceMapRegion item in list)
		{
			bool bConvertedDoor = false;
			ZoneBuilderSandbox.TunnelTo(Z, Anchor, IF.Seeds[item.Seed], pathWithNoise: true, 0.2f, 0, (Cell c) =>
			{
				if (c.HasObject(WallObject))
				{
					c.Clear();
					if (!bConvertedDoor)
					{
						c.AddObject(DoorObject);
						bConvertedDoor = true;
					}
				}
				else if (c.HasWall())
				{
					c.ClearWalls();
				}
			}, (int x, int y, int c) =>
			{
				if (x == 0)
				{
					return 2000;
				}
				if (y == 0)
				{
					return 2000;
				}
				if (x == Z.Width - 1)
				{
					return 2000;
				}
				if (y == Z.Height - 1)
				{
					return 2000;
				}
				int num = (Z.GetCell(x, y).HasObjectInDirection("N", WallObject) ? 1 : 0) + (Z.GetCell(x, y).HasObjectInDirection("S", WallObject) ? 1 : 0);
				int num2 = (Z.GetCell(x, y).HasObjectInDirection("E", WallObject) ? 1 : 0) + (Z.GetCell(x, y).HasObjectInDirection("W", WallObject) ? 1 : 0);
				if (Z.GetCell(x, y).HasObject(WallObject) && num > 0 && num2 > 0)
				{
					return 80;
				}
				if (Z.GetCell(x, y).HasObject(WallObject))
				{
					return 20;
				}
				if (Z.GetCell(x, y).HasWall())
				{
					return 80;
				}
				return Z.GetCell(x, y).HasObject("InfluenceMapBlocker") ? 80 : 0;
			});
		}
		return true;
	}
}
