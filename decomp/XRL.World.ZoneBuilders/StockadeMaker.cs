using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class StockadeMaker : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z, bool ClearCombatObjectsFirst, string WallObject, string ZoneTable, string Widgets, string CustomRoomPopulation = null, string BoxWidth = "30-50", string BoxHeight = "16-24", bool SpecialRedrockBuilder = false)
	{
		Box box = null;
		Box box2 = null;
		int num;
		for (num = 10; num > 0; num--)
		{
			List<Box> list = Tools.GenerateBoxes(BoxGenerateOverlap.Irrelevant, new Range(1, 1), new Range(Convert.ToInt32(BoxWidth.Split('-')[0]), Convert.ToInt32(BoxWidth.Split('-')[1])), new Range(Convert.ToInt32(BoxHeight.Split('-')[0]), Convert.ToInt32(BoxHeight.Split('-')[1])), null);
			if (list == null || list.Count == 0)
			{
				return false;
			}
			box = list[0].Grow(-1);
			box2 = box;
			for (int i = list[0].x1; i <= list[0].x2; i++)
			{
				for (int j = list[0].y1; j <= list[0].y2 && !Z.IsReachable(i, j); j++)
				{
				}
			}
		}
		if (num < 0)
		{
			return true;
		}
		Z.ClearBox(box, "CanyonMarker");
		if (ClearCombatObjectsFirst)
		{
			for (int k = box.y1; k <= box.y2; k++)
			{
				for (int l = box.x1; l <= box.x2; l++)
				{
					while (true)
					{
						using (List<GameObject>.Enumerator enumerator = Z.GetCell(l, k).GetObjectsWithPart("Combat").GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								GameObject current = enumerator.Current;
								Z.GetCell(l, k).RemoveObject(current);
								continue;
							}
						}
						break;
					}
				}
			}
		}
		Z.ClearBox(box, "CanyonMarker");
		for (int m = box.x1; m <= box.x2; m++)
		{
			for (int n = box.y1; n <= box.y2; n++)
			{
				Dirty.PaintCell(Z.GetCell(m, n));
			}
		}
		Z.FillRoundHollowBox(box, WallObject);
		Z.FillRoundHollowBox(box.Grow(-1), WallObject);
		List<Box> list2 = new List<Box>();
		if (box2.Valid)
		{
			foreach (Box item in Tools.GenerateBoxes(new List<Box>(), BoxGenerateOverlap.NeverOverlap, new Range(1, 8), new Range(9, 40), new Range(8, 14), new Range(6, 999), new Range(box2.x1, box2.x2), new Range(box2.y1, box2.y2)))
			{
				if (item.Valid && item.Grow(-1).Valid)
				{
					if (item.x1 == box.x1 || item.x2 == box.x2 || item.y1 == box.y1 || item.y2 == box.y2)
					{
						Z.FillRoundHollowBox(item, WallObject);
						list2.Add(item);
					}
					else
					{
						Z.FillRoundHollowBox(item.Grow(-1), WallObject);
						list2.Add(item.Grow(-1));
					}
				}
			}
		}
		bool flag = true;
		int num2 = 0;
		foreach (Box item2 in list2)
		{
			if (CustomRoomPopulation != null)
			{
				if (item2.Volume > 25 && item2.Height > 6 && item2.Width > 4)
				{
					if (flag & SpecialRedrockBuilder)
					{
						ZoneBuilderSandbox.PlacePopulationInRect(Z, item2.rect, "LegendarySnapjawParty");
						flag = false;
					}
					else
					{
						ZoneBuilderSandbox.PlacePopulationInRect(Z, item2.rect, CustomRoomPopulation);
					}
				}
			}
			else if (item2.Volume > 25 && item2.Height > 6 && item2.Width > 4)
			{
				BuildingTemplate buildingTemplate = new BuildingTemplate(item2.Width - 2, item2.Height - 2, 1, FullSquare: true);
				for (int num3 = 1; num3 < buildingTemplate.Width - 1; num3++)
				{
					for (int num4 = 1; num4 < buildingTemplate.Height - 1; num4++)
					{
						if (buildingTemplate.Map[num3, num4] == BuildingTemplateTile.Wall)
						{
							Z.GetCell(num3 + item2.x1, num4 + item2.y1).AddObject(WallObject);
						}
						if (buildingTemplate.Map[num3, num4] == BuildingTemplateTile.Door)
						{
							Z.GetCell(num3 + item2.x1, num4 + item2.y1).ClearWalls();
						}
					}
				}
				foreach (RoomData room in buildingTemplate.RoomList)
				{
					int x = item2.x1 + room.Left + 1;
					int y = item2.y1 + room.Top + 1;
					switch (num2)
					{
					case 0:
					{
						List<Cell> localEmptyAdjacentCells2 = Z.GetCell(x, y).GetLocalEmptyAdjacentCells();
						if (localEmptyAdjacentCells2.Count > 0)
						{
							int num5 = Stat.Random(1, 100);
							if (num5 <= 70)
							{
								localEmptyAdjacentCells2[0].AddObject("Fort Snapjaw Warlord");
							}
							else if (num5 <= 90)
							{
								localEmptyAdjacentCells2[0].AddObject("Snapjaw Hero 0");
							}
							else
							{
								localEmptyAdjacentCells2[0].AddObject("Snapjaw Hero 1");
							}
						}
						if (localEmptyAdjacentCells2.Count > 1)
						{
							localEmptyAdjacentCells2[1].AddObject("Chest" + Stat.Random(1, 2));
						}
						if (localEmptyAdjacentCells2.Count > 2 && Stat.Random(1, 100) <= 50)
						{
							localEmptyAdjacentCells2[2].AddObject("Table");
						}
						break;
					}
					case 1:
					{
						List<Cell> localEmptyAdjacentCells = Z.GetCell(x, y).GetLocalEmptyAdjacentCells();
						if (localEmptyAdjacentCells.Count > 0)
						{
							localEmptyAdjacentCells[0].AddObject("Table");
						}
						if (localEmptyAdjacentCells.Count > 1)
						{
							localEmptyAdjacentCells[1].AddObject("Chest2");
						}
						break;
					}
					case 2:
					{
						List<Cell> localEmptyAdjacentCells3 = Z.GetCell(x, y).GetLocalEmptyAdjacentCells();
						if (localEmptyAdjacentCells3.Count > 0)
						{
							localEmptyAdjacentCells3[0].AddObject("Forge");
						}
						if (localEmptyAdjacentCells3.Count > 1)
						{
							localEmptyAdjacentCells3[1].AddObject("Anvil");
						}
						if (localEmptyAdjacentCells3.Count > 2 && Stat.Random(1, 100) <= 50)
						{
							localEmptyAdjacentCells3[2].AddObject("ChestBronzeIngots");
						}
						break;
					}
					default:
					{
						List<Cell> emptyAdjacentCells = Z.GetCell(x, y).GetEmptyAdjacentCells();
						if (emptyAdjacentCells.Count > 0 && Stat.Random(1, 100) <= 25)
						{
							emptyAdjacentCells[0].AddObject("Table");
						}
						break;
					}
					}
					num2++;
				}
			}
			num = 0;
			while (num < 1000)
			{
				num++;
				int num6 = 0;
				int num7 = 0;
				if (Stat.Random(0, 1) == 0)
				{
					num6 = Stat.Random(item2.x1 + 2, item2.x2 - 2);
					num7 = ((Stat.Random(0, 1) != 0) ? item2.y2 : item2.y1);
				}
				else
				{
					num7 = Stat.Random(item2.y1 + 2, item2.y2 - 2);
					num6 = ((Stat.Random(0, 1) != 0) ? item2.x2 : item2.x1);
				}
				if (num6 != box.x1 && num6 != box.x2 && num7 != box.y1 && num7 != box.y2 && ((!Z.GetCell(num6 - 1, num7).HasWall() && !Z.GetCell(num6 + 1, num7).HasWall()) || (!Z.GetCell(num6, num7 - 1).HasWall() && !Z.GetCell(num6, num7 + 1).HasWall())))
				{
					Z.GetCell(num6, num7).ClearWalls();
					break;
				}
			}
		}
		if (num2 == 0)
		{
			for (num = 1000; num > 0; num--)
			{
				Point randomPoint = box.Grow(-1).RandomPoint;
				if (Z.GetCell(randomPoint.X, randomPoint.Y).IsEmpty())
				{
					Z.GetCell(randomPoint.X, randomPoint.Y).AddObject("Snapjaw Hero 0");
					break;
				}
			}
		}
		if (ZoneTable != null)
		{
			string[] array = ZoneTable.Split(',');
			for (int num8 = 0; num8 < array.Length; num8++)
			{
				foreach (PopulationResult item3 in PopulationManager.Generate(array[num8], "zonetier", Z.NewTier.ToString()))
				{
					ZoneBuilderSandbox.PlaceObjectInArea(Z, Z.area, GameObjectFactory.Factory.CreateObject(item3.Blueprint), 0, 0, item3.Hint);
				}
			}
		}
		if (Widgets != null)
		{
			string[] array = Widgets.Split(',');
			foreach (string blueprint in array)
			{
				Z.GetCell(0, 0).AddObject(blueprint);
			}
		}
		int num9 = 0;
		for (int num10 = 0; num10 < 2; num10++)
		{
			int num11 = Stat.Random(1, 15);
			if ((num11 & 1) != 0)
			{
				num9 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (!Z.GetCell(num9, box.y1 + 2).HasWall() && Z.GetCell(num9, box.y1).HasWall())
				{
					Z.GetCell(num9, box.y1).ClearWalls();
					Z.GetCell(num9, box.y1).AddObject("BrinestalkArrowslit");
					Z.GetCell(num9, box.y1 + 1).ClearWalls();
					Z.GetCell(num9, box.y1 + 1).AddObject("BrinestalkArrowslit");
				}
			}
			if ((num11 & 2) != 0)
			{
				num9 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (!Z.GetCell(num9, box.y2 - 2).HasWall() && Z.GetCell(num9, box.y2).HasWall())
				{
					Z.GetCell(num9, box.y2).ClearWalls();
					Z.GetCell(num9, box.y2 - 1).ClearWalls();
					Z.GetCell(num9, box.y2).AddObject("BrinestalkArrowslit");
					Z.GetCell(num9, box.y2 - 1).AddObject("BrinestalkArrowslit");
				}
			}
			if ((num11 & 4) != 0)
			{
				int y2 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (!Z.GetCell(box.x1 + 2, y2).HasWall() && Z.GetCell(box.x1, y2).HasWall())
				{
					Z.GetCell(box.x1, y2).ClearWalls();
					Z.GetCell(box.x1 + 1, y2).ClearWalls();
					Z.GetCell(box.x1, y2).AddObject("BrinestalkArrowslit");
					Z.GetCell(box.x1 + 1, y2).AddObject("BrinestalkArrowslit");
				}
			}
			if ((num11 & 8) != 0)
			{
				int y3 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (!Z.GetCell(box.x2 - 2, y3).HasWall() && Z.GetCell(box.x2, y3).HasWall())
				{
					Z.GetCell(box.x2, y3).ClearWalls();
					Z.GetCell(box.x2 - 1, y3).ClearWalls();
					Z.GetCell(box.x2, y3).AddObject("BrinestalkArrowslit");
					Z.GetCell(box.x2 - 1, y3).AddObject("BrinestalkArrowslit");
				}
			}
		}
		int num12 = 0;
		bool flag2;
		do
		{
			num12++;
			flag2 = false;
			int num13 = Stat.Random(1, 15);
			int num14 = -1;
			int num15 = -1;
			int num16 = -1;
			int num17 = -1;
			for (int num18 = box.x1 + 3; num18 <= box.x2 - 3; num18++)
			{
				if (Z.GetCell(num18, box.y1).HasObjectWithBlueprint("CanyonMarker"))
				{
					num14 = num18;
				}
				if (Z.GetCell(num18, box.y2).HasObjectWithBlueprint("CanyonMarker"))
				{
					num15 = num18;
				}
			}
			for (int num19 = box.y1 + 3; num19 <= box.y2 - 3; num19++)
			{
				if (Z.GetCell(box.x1, num19).HasObjectWithBlueprint("CanyonMarker"))
				{
					num17 = num19;
				}
				if (Z.GetCell(box.x2, num19).HasObjectWithBlueprint("CanyonMarker"))
				{
					num16 = num19;
				}
			}
			if ((num13 & 1) != 0 || num14 != -1)
			{
				num9 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (num14 != -1 && num12 < 10)
				{
					num9 = num14;
				}
				if (!Z.GetCell(num9, box.y1 + 2).HasWall())
				{
					Z.GetCell(num9, box.y1).ClearWalls();
					Z.GetCell(num9, box.y1 + 1).ClearWalls();
					flag2 = true;
				}
				if (!Z.GetCell(num9 + 1, box.y1 + 2).HasWall())
				{
					Z.GetCell(num9 + 1, box.y1).ClearWalls();
					Z.GetCell(num9 + 1, box.y1 + 1).ClearWalls();
					flag2 = true;
				}
			}
			if ((num13 & 2) != 0 || num15 != -1)
			{
				num9 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (num15 != -1 && num12 < 10)
				{
					num9 = num15;
				}
				if (!Z.GetCell(num9, box.y2 - 2).HasWall())
				{
					Z.GetCell(num9, box.y2).ClearWalls();
					Z.GetCell(num9, box.y2 - 1).ClearWalls();
					flag2 = true;
				}
				if (!Z.GetCell(num9 + 1, box.y2 - 2).HasWall())
				{
					Z.GetCell(num9 + 1, box.y2).ClearWalls();
					Z.GetCell(num9 + 1, box.y2 - 1).ClearWalls();
					flag2 = true;
				}
			}
			if ((num13 & 4) != 0 || num16 != -1)
			{
				int num20 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (num16 != -1 && num12 < 10)
				{
					num20 = num16;
				}
				if (!Z.GetCell(box.x1 + 2, num20).HasWall())
				{
					Z.GetCell(box.x1, num20).ClearWalls();
					Z.GetCell(box.x1 + 1, num20).ClearWalls();
					flag2 = true;
				}
				if (!Z.GetCell(box.x1 + 2, num20 + 1).HasWall())
				{
					Z.GetCell(box.x1, num20 + 1).ClearWalls();
					Z.GetCell(box.x1 + 1, num20 + 1).ClearWalls();
					flag2 = true;
				}
			}
			if ((num13 & 8) != 0 || num17 != -1)
			{
				int num21 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (num17 != -1 && num12 < 10)
				{
					num21 = num17;
				}
				if (!Z.GetCell(box.x2 - 2, num21).HasWall())
				{
					Z.GetCell(box.x2, num21).ClearWalls();
					Z.GetCell(box.x2 - 1, num21).ClearWalls();
					flag2 = true;
				}
				if (!Z.GetCell(box.x2 - 2, num21 + 1).HasWall())
				{
					Z.GetCell(box.x2, num21 + 1).ClearWalls();
					Z.GetCell(box.x2 - 1, num21 + 1).ClearWalls();
					flag2 = true;
				}
			}
		}
		while (!flag2);
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z);
		return true;
	}
}
