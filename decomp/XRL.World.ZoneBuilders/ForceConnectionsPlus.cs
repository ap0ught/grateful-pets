using System.Collections.Generic;
using Genkit;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.ZoneBuilders;

public class ForceConnectionsPlus : ZoneBuilderSandbox
{
	public bool CaveLike = true;

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
		List<Cell> cells = Z.GetCells((Cell c) => c.IsPassable());
		Z.ClearReachableMap();
		Z.SetReachable(cells[0].X, cells[0].Y);
		Z.BuildReachableMap(cells[0].X, cells[0].Y, bClearFirst: false);
		GameObject gameObject = null;
		gameObject = GameObjectFactory.Factory.CreateObject("Drillbot");
		for (int num = 1; num < cells.Count; num++)
		{
			if (Z.IsReachable(cells[num].X, cells[num].Y))
			{
				continue;
			}
			FindPath findPath = new FindPath(Z, cells[0].X, cells[0].Y, Z, cells[num].X, cells[num].Y, PathGlobal: false, PathUnlimited: true, gameObject, AddNoise: true);
			if (!findPath.Usable)
			{
				continue;
			}
			foreach (Cell step in findPath.Steps)
			{
				Z.ReachableMap[step.X, step.Y] = true;
				step.ClearWalls();
			}
			if (CaveLike)
			{
				foreach (Cell step2 in findPath.Steps)
				{
					if (step2.IsReachable() || step2.HasWall())
					{
						continue;
					}
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
			Z.BuildReachableMap(cells[num].X, cells[num].Y, bClearFirst: false);
		}
		return true;
	}
}
