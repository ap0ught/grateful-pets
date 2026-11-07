using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class PigFarm
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
				list = Tools.GenerateBoxes(BoxGenerateOverlap.Irrelevant, new Range(1, 3), new Range(40, 60), new Range(8, 16), null, new Range(1, Z.Width - 2), new Range(1, Z.Height - 2));
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
									goto end_IL_00cb;
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
			end_IL_00cb:
			break;
		}
		List<Cell> list2 = new List<Cell>();
		foreach (Box item2 in list)
		{
			Z.ClearBox(item2);
			for (int i = item2.x1; i <= item2.x2; i++)
			{
				for (int j = item2.y1; j <= item2.y2; j++)
				{
					Dirty.PaintCell(Z.GetCell(i, j));
				}
			}
			Box box = item2.Grow(-2);
			for (int k = box.x1; k <= box.x2; k++)
			{
				for (int l = box.y1; l <= box.y2; l++)
				{
					list2.Add(Z.GetCell(k, l));
				}
			}
			int num4 = Stat.Random(1, 4);
			int num5 = Stat.Random(item2.x1, item2.x2);
			int num6 = num5 + Stat.Random(1, 8);
			int num7 = Stat.Random(item2.y1, item2.y2);
			int num8 = num5 + Stat.Random(1, 8);
			for (int m = item2.x1; m <= item2.x2; m++)
			{
				if (!InAnyBox(m, item2.y1, list, -1) && (num4 != 0 || m < num5 || m > num6))
				{
					Fence(Z.GetCell(m, item2.y1));
				}
				if (!InAnyBox(m, item2.y2, list, -1) && (num4 != 1 || m < num5 || m > num6))
				{
					Fence(Z.GetCell(m, item2.y2));
				}
			}
			for (int n = item2.y1; n <= item2.y2; n++)
			{
				if (!InAnyBox(item2.x1, n, list, -1) && (num4 != 2 || n < num7 || n > num8))
				{
					Fence(Z.GetCell(item2.x1, n));
				}
				if (!InAnyBox(item2.x2, n, list, -1) && (num4 != 3 || n < num7 || n > num8))
				{
					Fence(Z.GetCell(item2.x2, n));
				}
			}
		}
		List<Cell> list3 = new List<Cell>();
		for (int num9 = 0; num9 < Z.Height; num9++)
		{
			for (int num10 = 0; num10 < Z.Width; num10++)
			{
				if (Z.GetCell(num10, num9).HasObjectWithBlueprint("BrinestalkFence"))
				{
					list3.Add(Z.GetCell(num10, num9));
				}
			}
		}
		if (list3.Count > 0)
		{
			int num11 = 0;
			for (int num12 = Stat.Random(1, 3); num11 < num12; num11++)
			{
				int index = Stat.Random(0, list3.Count - 1);
				list3[index].Clear();
				list3[index].AddObject("Brinestalk Gate");
			}
		}
		new VillageMaker().BuildZone(Z, bRoads: true, "PigskinWall", RoundBuildings: true, "1d2", "5-Cistern", "PigFarmYurt", "PigFarmGlobals", null, ClearCombatObjectsFirst: true);
		Z.GetCell(0, 0).RequireObject("Dirty");
		int num13 = "8d2".RollCached();
		num = 200;
		while (num13 > 0 && num > 0 && list2.Count > 0)
		{
			Cell randomElement = list2.GetRandomElement();
			if (randomElement.IsEmpty())
			{
				randomElement.AddObject("Pig");
				num13--;
			}
			list2.Remove(randomElement);
			num--;
		}
		return true;
	}

	public void Fence(Cell C)
	{
		C.Clear();
		C.AddObject("BrinestalkFence");
	}
}
