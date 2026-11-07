using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.ZoneBuilders;

public class CanyonBuilder
{
	public bool BuildZone(Zone Z)
	{
		List<CachedZoneConnection> list = new List<CachedZoneConnection>();
		bool flag = false;
		int num = -1;
		int num2 = -1;
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-" && item.Type.Contains("Canyon"))
			{
				if (item.Type.Contains("Start"))
				{
					num = item.X;
					num2 = item.Y;
				}
				list.Add(item);
			}
		}
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type.Contains("Stairs"))
			{
				list.Add(new CachedZoneConnection("-", zoneConnection.X, zoneConnection.Y, "", null));
			}
			if (zoneConnection.Type.Contains("Canyon"))
			{
				if (zoneConnection.Type.Contains("Start"))
				{
					flag = true;
					num = zoneConnection.X;
					num2 = zoneConnection.Y;
				}
				list.Add(new CachedZoneConnection("-", zoneConnection.X, zoneConnection.Y, zoneConnection.Type, null));
			}
		}
		GameObject gameObject = null;
		gameObject = GameObjectFactory.Factory.CreateObject("Drillbot");
		if (num == -1 || num2 == -1)
		{
			flag = true;
			num = Stat.Random(6, Z.Width - 6);
			num2 = Stat.Random(6, Z.Height - 6);
		}
		if (flag)
		{
			if (Z.ReachableMap == null)
			{
				Z.ClearReachableMap(bValue: false);
			}
			int num3 = num - 2;
			int num4 = num2 - 2;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					Z.ReachableMap[i + num3, j + num4] = true;
					Z.GetCell(i + num3, j + num4).Clear();
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			if (num == list[k].X && num2 == list[k].Y)
			{
				continue;
			}
			FindPath findPath = new FindPath(Z, num, num2, Z, list[k].X, list[k].Y, PathGlobal: false, PathUnlimited: true, gameObject, AddNoise: true);
			if (!findPath.Usable)
			{
				continue;
			}
			foreach (Cell step in findPath.Steps)
			{
				Z.ReachableMap[step.X, step.Y] = true;
				step.Clear();
			}
			string objectTypeForZone = ZoneManager.GetObjectTypeForZone(Z.wX, Z.wY, Z.GetZoneWorld());
			int high = 3;
			if (objectTypeForZone != "Mountains" && objectTypeForZone != "Hills")
			{
				high = 1;
			}
			foreach (Cell step2 in findPath.Steps)
			{
				foreach (Cell localAdjacentCell in step2.GetLocalAdjacentCells(Stat.Random(1, high)))
				{
					Z.ReachableMap[localAdjacentCell.X, localAdjacentCell.Y] = true;
					localAdjacentCell.Clear();
					localAdjacentCell.AddObject(GameObjectFactory.Factory.CreateObject("CanyonMarker"));
				}
			}
			foreach (Cell step3 in findPath.Steps)
			{
				step3.AddObject("CanyonMarker");
			}
		}
		return true;
	}
}
