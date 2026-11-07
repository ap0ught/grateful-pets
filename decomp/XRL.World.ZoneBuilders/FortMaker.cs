using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class FortMaker : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z, bool ClearCombatObjectsFirst, string WallObject, string ZoneTable, string Widgets)
	{
		List<Box> list = Tools.GenerateBoxes(BoxGenerateOverlap.Irrelevant, new Range(1, 1), new Range(30, 50), new Range(16, 24), null, new Range(1, 78), new Range(1, 23));
		if (list == null || list.Count == 0)
		{
			return false;
		}
		Box box = list[0].Grow(-1);
		Box box2 = box;
		Z.ClearBox(box);
		if (ClearCombatObjectsFirst)
		{
			for (int i = box.y1; i <= box.y2; i++)
			{
				for (int j = box.x1; j <= box.x2; j++)
				{
					while (true)
					{
						using (List<GameObject>.Enumerator enumerator = Z.GetCell(j, i).GetObjectsWithPartReadonly("Combat").GetEnumerator())
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
		Z.ClearBox(box);
		Z.FillBox(box, "DirtFloor");
		Z.FillHollowBox(box, WallObject);
		for (int k = box.x1; k <= box.x2; k++)
		{
			for (int l = box.y1; l <= box.y2; l++)
			{
				Cell cell = Z.GetCell(k, l);
				if (cell.HasObjectWithBlueprint("CanyonMarker"))
				{
					cell.ClearWalls();
				}
			}
		}
		List<Box> list2 = new List<Box>();
		if (box2.Valid)
		{
			foreach (Box item in Tools.GenerateBoxes(new List<Box>(), BoxGenerateOverlap.NeverOverlap, new Range(1, 8), new Range(9, 40), new Range(8, 14), new Range(6, 999), new Range(box2.x1, box2.x2), new Range(box2.y1, box2.y2)))
			{
				if (item.Valid && item.Grow(-1).Valid)
				{
					if (item.x1 == box.x1 || item.x2 == box.x2 || item.y1 == box.y1 || item.y2 == box.y2)
					{
						Z.FillHollowBox(item, WallObject);
						list2.Add(item);
					}
					else
					{
						Z.FillHollowBox(item.Grow(-1), WallObject);
						list2.Add(item.Grow(-1));
					}
				}
			}
		}
		foreach (Box item2 in list2)
		{
			if (item2.Volume > 25 && item2.Height > 4 && item2.Width > 4)
			{
				BuildingTemplate buildingTemplate = new BuildingTemplate(item2.Width, item2.Height, 1, FullSquare: true);
				for (int m = 1; m < buildingTemplate.Width - 1; m++)
				{
					for (int n = 1; n < buildingTemplate.Height - 1; n++)
					{
						if (buildingTemplate.Map[m, n] == BuildingTemplateTile.Wall)
						{
							Z.GetCell(m + item2.x1, n + item2.y1).AddObject(WallObject);
						}
						if (buildingTemplate.Map[m, n] == BuildingTemplateTile.Door)
						{
							int num = m + item2.x1;
							int num2 = n + item2.y1;
							if (num != box.x1 && num != box.x2 && num2 != box.y1 && num2 != box.y2 && num != box.x1 + 1 && num != box.x2 - 1 && num2 != box.y1 + 1 && num2 != box.y2 - 1)
							{
								Z.GetCell(m + item2.x1, n + item2.y1).AddObject("Door");
							}
						}
					}
				}
			}
			int num3 = 0;
			while (num3 < 1000)
			{
				num3++;
				int num4 = 0;
				int num5 = 0;
				if (Stat.Random(0, 1) == 0)
				{
					num4 = Stat.Random(item2.x1 + 1, item2.x2 - 1);
					num5 = ((Stat.Random(0, 1) != 0) ? item2.y2 : item2.y1);
				}
				else
				{
					num5 = Stat.Random(item2.y1 + 1, item2.y2 - 1);
					num4 = ((Stat.Random(0, 1) != 0) ? item2.x2 : item2.x1);
				}
				if (num4 != box.x1 && num4 != box.x2 && num5 != box.y1 && num5 != box.y2 && ((!Z.GetCell(num4 - 1, num5).HasWall() && !Z.GetCell(num4 + 1, num5).HasWall()) || (!Z.GetCell(num4, num5 - 1).HasWall() && !Z.GetCell(num4, num5 + 1).HasWall())))
				{
					Z.GetCell(num4, num5).Clear();
					Z.GetCell(num4, num5).AddObject("Door");
					break;
				}
			}
		}
		if (ZoneTable != null)
		{
			string[] array = ZoneTable.Split(',');
			for (int num6 = 0; num6 < array.Length; num6++)
			{
				foreach (PopulationResult item3 in PopulationManager.Generate(array[num6], "zonetier", Z.NewTier.ToString()))
				{
					ZoneBuilderSandbox.PlaceObjectInArea(Z, Z.area, GameObjectFactory.Factory.CreateObject(item3.Blueprint), 0, 0, item3.Hint);
				}
			}
		}
		if (Widgets != null)
		{
			string[] array = Widgets.Split(',');
			foreach (string objectBlueprint in array)
			{
				Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject(objectBlueprint));
			}
		}
		int num7 = Stat.Random(1, 15);
		if ((num7 & 1) != 0)
		{
			int num8 = Stat.Random(box.x1 + 1, box.x2 - 2);
			if (!Z.GetCell(num8, box.y1 + 1).HasWall())
			{
				Z.GetCell(num8, box.y1).Clear();
				Z.GetCell(num8, box.y1).AddObject("Door");
			}
			if (!Z.GetCell(num8 + 1, box.y1 + 1).HasWall())
			{
				Z.GetCell(num8 + 1, box.y1).Clear();
				Z.GetCell(num8 + 1, box.y1).AddObject("Door");
			}
		}
		if ((num7 & 2) != 0)
		{
			int num9 = Stat.Random(box.x1 + 1, box.x2 - 2);
			if (!Z.GetCell(num9, box.y2 - 1).HasWall())
			{
				Z.GetCell(num9, box.y2).Clear();
				Z.GetCell(num9, box.y2).AddObject("Door");
			}
			if (!Z.GetCell(num9 + 1, box.y2 - 1).HasWall())
			{
				Z.GetCell(num9 + 1, box.y2).Clear();
				Z.GetCell(num9 + 1, box.y2).AddObject("Door");
			}
		}
		if ((num7 & 4) != 0)
		{
			int num10 = Stat.Random(box.y1 + 1, box.y2 - 2);
			if (!Z.GetCell(box.x1 + 1, num10).HasWall())
			{
				Z.GetCell(box.x1, num10).Clear();
				Z.GetCell(box.x1, num10).AddObject("Door");
			}
			if (!Z.GetCell(box.x1 + 1, num10 + 1).HasWall())
			{
				Z.GetCell(box.x1, num10 + 1).Clear();
				Z.GetCell(box.x1, num10 + 1).AddObject("Door");
			}
		}
		if ((num7 & 8) != 0)
		{
			int num11 = Stat.Random(box.y1 + 1, box.y2 - 2);
			if (!Z.GetCell(box.x2 - 1, num11).HasWall())
			{
				Z.GetCell(box.x2, num11).Clear();
				Z.GetCell(box.x2, num11).AddObject("Door");
			}
			if (!Z.GetCell(box.x2 - 1, num11 + 1).HasWall())
			{
				Z.GetCell(box.x2, num11 + 1).Clear();
				Z.GetCell(box.x2, num11 + 1).AddObject("Door");
			}
		}
		return true;
	}
}
