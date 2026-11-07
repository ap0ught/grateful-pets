using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class VillageMaker : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z, bool bRoads, string WallObject, bool RoundBuildings, string Huts, string Features, string HutTable, string ZoneTable, string Widgets, bool ClearCombatObjectsFirst)
	{
		if (ClearCombatObjectsFirst)
		{
			for (int i = 0; i < Z.Height; i++)
			{
				for (int j = 0; j < Z.Width; j++)
				{
					while (true)
					{
						using (List<GameObject>.Enumerator enumerator = Z.GetCell(j, i).GetObjectsWithPart("Combat").GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								GameObject current = enumerator.Current;
								Z.GetCell(j, i).RemoveObject(current);
								continue;
							}
						}
						break;
					}
				}
			}
		}
		int num = Huts.RollCached();
		List<Point> list = new List<Point>(num);
		List<Point> list2 = new List<Point>(num);
		bool flag = false;
		foreach (ZoneConnection item in Z.EnumerateConnections())
		{
			Point point = new Point(item.X, item.Y);
			list.Add(point);
			if (item.Type.Contains("Road"))
			{
				if (!flag)
				{
					list2.Clear();
					flag = true;
				}
				if (list2.Count == 0)
				{
					list2.Add(point);
				}
				else if (Distance(list2[0], 40, 12) > item.Loc2D.ManhattanDistance(40, 12))
				{
					list2[0] = point;
				}
			}
			if (!flag)
			{
				list2.Add(point);
			}
		}
		int num2 = 0;
		int num3 = 0;
		for (num3 = 0; num3 < num; num3++)
		{
			if (num2 >= 100)
			{
				break;
			}
			Point point2 = null;
			bool flag2 = false;
			while (!flag2 && num2 < 100)
			{
				num2++;
				flag2 = true;
				point2 = new Point(Stat.Random(5, 70), Stat.Random(5, 20));
				for (int k = 0; k < list.Count; k++)
				{
					if (XRL.Rules.Geometry.Distance(point2, list[k]) < 10)
					{
						flag2 = false;
						break;
					}
				}
			}
			if (flag2)
			{
				int num4 = Stat.Random(4, 6);
				int num5 = Stat.Random(4, 6);
				num2 = 0;
				Rect2D r = new Rect2D(point2.X - num4 / 2, point2.Y - num5 / 2, point2.X + num4 / 2, point2.Y + num5 / 2);
				do
				{
					r.GetRandomDoorCell("?", (!RoundBuildings) ? 1 : 2);
				}
				while (!Z.GetCell(r.Door.x, r.Door.y).IsEmpty() && num2++ < 8);
				list2.Add(new Point(r.Door.x, r.Door.y));
				PlaceHut(Z, r, "DirtPath", WallObject, HutTable, RoundBuildings);
			}
		}
		Point point3 = null;
		bool flag3 = false;
		if (Features != null && Features.Contains("Cistern"))
		{
			int num6 = num3;
			string[] array = Features.Split('-');
			if (num6 >= int.Parse((array != null) ? array[0] : null))
			{
				bool flag4 = false;
				while (!flag4 && num2 < 100)
				{
					num2++;
					flag4 = true;
					point3 = new Point(Stat.Random(5, 70), Stat.Random(5, 20));
					for (int l = 0; l < list.Count; l++)
					{
						if (XRL.Rules.Geometry.Distance(point3, list[l]) < 8)
						{
							flag4 = false;
							break;
						}
					}
				}
				if (flag4)
				{
					flag3 = true;
				}
			}
		}
		RoadBuilder roadBuilder = new RoadBuilder();
		if (flag3)
		{
			list2.Add(point3);
		}
		list2.Sort((Point a, Point b) => Distance(a, 40, 12).CompareTo(Distance(b, 40, 12)));
		roadBuilder.BeforePlacement = (Cell C) =>
		{
			C.ClearObjectsWithTag("Plant");
		};
		roadBuilder.ClearAdjacent = false;
		roadBuilder.BuildRoads(Z, list2, bClearSolids: true, bNoise: false);
		if (flag3)
		{
			string blueprint = "LowSandstoneWall";
			if (Features.Contains("Qlippoth"))
			{
				blueprint = "HolographicLowSandstoneWall";
			}
			for (int num7 = point3.X - 1; num7 <= point3.X + 2; num7++)
			{
				Z.GetCell(num7, point3.Y - 1).ClearAndAddObject(blueprint);
				Z.GetCell(num7, point3.Y + 1).ClearAndAddObject(blueprint);
			}
			int x = point3.X;
			int y = point3.Y;
			Z.GetCell(x - 1, y).ClearAndAddObject(blueprint);
			Z.GetCell(x + 2, y).ClearAndAddObject(blueprint);
			if (Features.Contains("BloodCistern"))
			{
				Z.GetCell(x, y).ClearAndAddObject("BloodPool");
				Z.GetCell(x + 1, y).ClearAndAddObject("BloodPool");
			}
			else
			{
				Z.GetCell(x, y).ClearAndAddObject("FreshWaterPool");
				Z.GetCell(x + 1, y).ClearAndAddObject("FreshWaterPool");
			}
		}
		if (ZoneTable != null)
		{
			string[] array2 = ZoneTable.Split(',');
			for (int num8 = 0; num8 < array2.Length; num8++)
			{
				foreach (PopulationResult item2 in PopulationManager.Generate(array2[num8], "zonetier", Z.NewTier.ToString()))
				{
					ZoneBuilderSandbox.PlaceObjectInArea(Z, Z.area, GameObjectFactory.Factory.CreateObject(item2.Blueprint), 0, 0, item2.Hint);
				}
			}
		}
		if (Widgets != null)
		{
			string[] array2 = Widgets.Split(',');
			foreach (string blueprint2 in array2)
			{
				Z.GetCell(0, 0).AddObject(blueprint2);
			}
		}
		for (int num9 = 0; num9 < Z.Height; num9++)
		{
			for (int num10 = 0; num10 < Z.Width; num10++)
			{
				if (!Z.Map[num10][num9].HasObject("DirtRoad"))
				{
					continue;
				}
				foreach (Cell localAdjacentCell in Z.Map[num10][num9].GetLocalAdjacentCells())
				{
					localAdjacentCell.ClearObjectsWithTag("Plant");
				}
			}
		}
		Z.GetCell(0, 0).RequireObject("DaylightWidget");
		return true;
	}

	public int Distance(Point A, int X, int Y)
	{
		return Math.Max(Math.Abs(A.X - X), Math.Abs(A.Y - Y));
	}
}
