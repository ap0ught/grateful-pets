using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class StarappleFarm
{
	public bool InAnyBox(int x, int y, List<Box> Boxes, int Grow)
	{
		foreach (Box Box in Boxes)
		{
			if (Box.Grow(Grow).contains(x, y))
			{
				return true;
			}
		}
		return false;
	}

	public bool BuildZone(Zone Z, bool ClearCombatObjectsFirst, string WallObject, string ZoneTable, string Widgets)
	{
		List<Box> list = null;
		int num = 10;
		while (true)
		{
			if (num > 0)
			{
				list = Tools.GenerateBoxes(BoxGenerateOverlap.Irrelevant, new Range(1, 3), new Range(40, 60), new Range(8, 16), null, new Range(1, 78), new Range(1, 23));
				if (list == null)
				{
					return false;
				}
				if (list.Count == 0)
				{
					return false;
				}
				foreach (Box item in list)
				{
					int num2 = item.y1;
					while (num2 <= item.y2)
					{
						int num3 = item.x1;
						while (true)
						{
							if (num3 <= item.x2)
							{
								if (Z.IsReachable(num3, num2))
								{
									goto end_IL_00be;
								}
								num3++;
								continue;
							}
							num2++;
							break;
						}
					}
				}
				num--;
				continue;
			}
			if (num > 0)
			{
				break;
			}
			return true;
			continue;
			end_IL_00be:
			break;
		}
		foreach (Box item2 in list)
		{
			Z.ClearBox(item2);
			for (int i = item2.x1; i <= item2.x2; i++)
			{
				for (int j = item2.y1; j < item2.y2; j++)
				{
					Grassy.PaintCell(Z.GetCell(i, j));
				}
			}
			Box box = item2.Grow(-2);
			int num4;
			for (num4 = box.x1 + Stat.Random(1, 3); num4 <= box.x2; num4++)
			{
				for (int k = box.y1; k <= box.y2; k += Stat.Random(1, 2))
				{
					if (Stat.Random(1, 100) <= 45)
					{
						Z.GetCell(num4, k).ClearObjectsWithPart("Combat");
						if (Z.GetCell(num4, k).IsEmpty())
						{
							Z.GetCell(num4, k).AddObject("Starapple Farm Tree");
						}
					}
				}
				num4 += Stat.Random(0, 2);
			}
			int num5 = Stat.Random(1, 4);
			int num6 = Stat.Random(item2.x1, item2.x2);
			int num7 = num6 + Stat.Random(1, 8);
			int num8 = Stat.Random(item2.y1, item2.y2);
			int num9 = num6 + Stat.Random(1, 8);
			for (int l = item2.x1; l <= item2.x2; l++)
			{
				if (!InAnyBox(l, item2.y1, list, -1) && (num5 != 0 || l < num6 || l > num7))
				{
					Fence(Z.GetCell(l, item2.y1));
				}
				if (!InAnyBox(l, item2.y2, list, -1) && (num5 != 1 || l < num6 || l > num7))
				{
					Fence(Z.GetCell(l, item2.y2));
				}
			}
			for (int m = item2.y1; m <= item2.y2; m++)
			{
				if (!InAnyBox(item2.x1, m, list, -1) && (num5 != 2 || m < num8 || m > num9))
				{
					Fence(Z.GetCell(item2.x1, m));
				}
				if (!InAnyBox(item2.x2, m, list, -1) && (num5 != 3 || m < num8 || m > num9))
				{
					Fence(Z.GetCell(item2.x2, m));
				}
			}
		}
		List<Cell> list2 = new List<Cell>();
		for (int n = 0; n < 80; n++)
		{
			for (int num10 = 0; num10 < 24; num10++)
			{
				if (Z.GetCell(n, num10).HasObjectWithBlueprint("BrinestalkFence"))
				{
					list2.Add(Z.GetCell(n, num10));
				}
			}
		}
		if (list2.Count > 0)
		{
			int num11 = 0;
			for (int num12 = Stat.Random(1, 3); num11 < num12; num11++)
			{
				int index = Stat.Random(0, list2.Count - 1);
				list2[index].Clear();
				list2[index].AddObject("Brinestalk Gate");
			}
		}
		new VillageMaker().BuildZone(Z, bRoads: true, "PigskinWall", RoundBuildings: true, "1d2", "5-Cistern", "StarappleFarmYurt", "StarappleFarmGlobals", null, ClearCombatObjectsFirst: true);
		Z.GetCell(0, 0).RequireObject("Dirty");
		return true;
	}

	public void Fence(Cell C)
	{
		C.Clear();
		C.AddObject("BrinestalkFence");
	}
}
