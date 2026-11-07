using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.ZoneBuilders;

public class FungalTrailBuilder
{
	public string Puddle = "FungalTrailBrick";

	public bool Pairs;

	public bool BuildZone(Zone Z)
	{
		List<CachedZoneConnection> list = new List<CachedZoneConnection>();
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-" && item.Type.Contains("FungalTrail"))
			{
				list.Add(item);
			}
		}
		int num = 40;
		int num2 = 20;
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type.Contains("FungalTrail"))
			{
				if (zoneConnection.Type.Contains("Start"))
				{
					num = zoneConnection.X;
					num2 = zoneConnection.Y;
				}
				list.Add(new CachedZoneConnection("-", zoneConnection.X, zoneConnection.Y, "", null));
			}
		}
		bool flag = false;
		if (list.Count == 1)
		{
			flag = true;
		}
		if (list.Count <= 1)
		{
			num = 40;
			num2 = 20;
		}
		else
		{
			num = list[0].X;
			num2 = list[0].Y;
		}
		GameObject looker = null;
		if (Z.BuildTries > 5 || Pairs)
		{
			looker = GameObjectFactory.Factory.CreateObject("Drillbot");
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
						Z.GetCell(j, i).ClearTerrain();
						Z.GetCell(j, i).AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
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
			FindPath findPath = new FindPath(Z, num, num2, Z, list[k].X, list[k].Y, PathGlobal: false, PathUnlimited: true, looker, AddNoise: true);
			if (!findPath.Usable)
			{
				continue;
			}
			foreach (Cell step in findPath.Steps)
			{
				Z.ReachableMap[step.X, step.Y] = true;
				if (Stat.Random(1, 100) <= 100)
				{
					step.ClearTerrain();
					step.AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
				}
			}
			foreach (Cell step2 in findPath.Steps)
			{
				foreach (Cell localAdjacentCell in step2.GetLocalAdjacentCells())
				{
					Z.ReachableMap[localAdjacentCell.X, localAdjacentCell.Y] = true;
					if (Stat.Random(1, 100) <= 100)
					{
						localAdjacentCell.ClearTerrain();
						localAdjacentCell.AddObject(GameObjectFactory.Factory.CreateObject(Puddle));
					}
				}
			}
		}
		return true;
	}
}
