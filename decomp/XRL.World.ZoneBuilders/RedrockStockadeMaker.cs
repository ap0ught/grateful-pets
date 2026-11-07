using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class RedrockStockadeMaker : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z, bool ClearCombatObjectsFirst, string WallObject, string ZoneTable, string Widgets, string CustomBigRoomPopulation = null, string CustomBigAlternateRoomPopulation = null, string CustomSmallRoomPopulation = null, string CustomSmallAlternateRoomPopulation = null, string BoxWidth = "30-50", string BoxHeight = "16-24", bool SpecialRedrockBuilder = false, string InsideStocakdeTable = null)
	{
		Box box = null;
		Box box2 = null;
		int num = 30;
		int num2 = Convert.ToInt32(Stat.Roll(BoxWidth));
		int num3 = Convert.ToInt32(Stat.Roll(BoxHeight));
		while (true)
		{
			if (num > 0)
			{
				List<Box> list = Tools.GenerateBoxes(BoxGenerateOverlap.Irrelevant, new Range(1, 1), new Range(num2), new Range(num3), null);
				if (list == null || list.Count <= 0)
				{
					return false;
				}
				box = list[0].Grow(-1);
				box2 = box;
				if (list[0].rect.allPoints().Any((Location2D p) => Z.GetCell(p).HasObject("StairsUp")))
				{
					num--;
					continue;
				}
				for (int num4 = list[0].y1; num4 <= list[0].y2; num4++)
				{
					for (int num5 = list[0].x1; num5 <= list[0].x2; num5++)
					{
						if (Z.IsReachable(num5, num4))
						{
							goto end_IL_0108;
						}
					}
				}
				num--;
				continue;
			}
			if (num >= 0)
			{
				break;
			}
			return true;
			continue;
			end_IL_0108:
			break;
		}
		Z.ClearBox(box, "CanyonMarker");
		if (ClearCombatObjectsFirst)
		{
			for (int num6 = box.y1; num6 <= box.y2; num6++)
			{
				for (int num7 = box.x1; num7 <= box.x2; num7++)
				{
					while (true)
					{
						using (List<GameObject>.Enumerator enumerator = Z.GetCell(num7, num6).GetObjectsWithPart("Combat").GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								GameObject current = enumerator.Current;
								Z.GetCell(num7, num6).RemoveObject(current);
								continue;
							}
						}
						break;
					}
				}
			}
		}
		Z.ClearBox(box, "CanyonMarker");
		for (int num8 = box.y1; num8 <= box.y2; num8++)
		{
			for (int num9 = box.x1; num9 <= box.x2; num9++)
			{
				Dirty.PaintCell(Z.GetCell(num9, num8));
			}
		}
		Z.FillRoundHollowBox(box, WallObject);
		Z.FillRoundHollowBox(box.Grow(-1), WallObject);
		List<Location2D> list2 = box.Grow(-2).rect.locations.ToList();
		List<Box> list3 = new List<Box>();
		if (box2.Valid)
		{
			foreach (Box B in Tools.GenerateBoxes(new List<Box>(), BoxGenerateOverlap.NeverOverlap, new Range(1, 8), new Range(9, 40), new Range(8, 14), new Range(6, 999), new Range(box2.x1, box2.x2), new Range(box2.y1, box2.y2)))
			{
				if (!B.Valid || !B.Grow(-1).Valid)
				{
					continue;
				}
				if (B.x1 == box.x1 || B.x2 == box.x2 || B.y1 == box.y1 || B.y2 == box.y2)
				{
					Z.FillRoundHollowBox(B, WallObject);
					list3.Add(B);
					list2.RemoveAll((Location2D l) => B.rect.locations.Contains(l));
				}
				else
				{
					Z.FillRoundHollowBox(B.Grow(-1), WallObject);
					list3.Add(B.Grow(-1));
					list2.RemoveAll((Location2D l) => B.Grow(-1).rect.locations.Contains(l));
				}
			}
		}
		bool flag = false;
		if ((double)num2 >= BoxWidth.GetCachedDieRoll().Average() && (double)num3 >= BoxHeight.GetCachedDieRoll().Average())
		{
			flag = true;
		}
		int num10 = 0;
		foreach (Box item in list3)
		{
			if (CustomBigRoomPopulation != null)
			{
				bool flag2 = 5.in100();
				string table = ((!flag2) ? CustomSmallRoomPopulation : CustomSmallAlternateRoomPopulation);
				if (item.Volume > 25 && item.Height > 6 && item.Width > 4)
				{
					if (flag)
					{
						if (flag2)
						{
							ZoneBuilderSandbox.PlacePopulationInRect(Z, item.rect, "LegendaryRedrockSnapjawRemains");
						}
						else
						{
							ZoneBuilderSandbox.PlacePopulationInRect(Z, item.rect, "LegendaryRedrockSnapjaw");
						}
						flag = false;
					}
					table = ((!flag2) ? CustomBigRoomPopulation : CustomBigAlternateRoomPopulation);
					ZoneBuilderSandbox.PlacePopulationInRect(Z, item.rect, table);
				}
				else
				{
					ZoneBuilderSandbox.PlacePopulationInRect(Z, item.rect, table);
				}
			}
			else if (item.Volume > 25 && item.Height > 6 && item.Width > 4)
			{
				BuildingTemplate buildingTemplate = new BuildingTemplate(item.Width - 2, item.Height - 2, 1, FullSquare: true);
				for (int num11 = 1; num11 < buildingTemplate.Height - 1; num11++)
				{
					for (int num12 = 1; num12 < buildingTemplate.Width - 1; num12++)
					{
						if (buildingTemplate.Map[num12, num11] == BuildingTemplateTile.Wall)
						{
							Z.GetCell(num12 + item.x1, num11 + item.y1).AddObject(WallObject);
						}
						if (buildingTemplate.Map[num12, num11] == BuildingTemplateTile.Door)
						{
							Z.GetCell(num12 + item.x1, num11 + item.y1).ClearWalls();
						}
					}
				}
			}
			num = 0;
			while (num < 1000)
			{
				num++;
				int num13 = 0;
				int num14 = 0;
				if (Stat.Random(0, 1) == 0)
				{
					num13 = Stat.Random(item.x1 + 2, item.x2 - 2);
					num14 = ((Stat.Random(0, 1) != 0) ? item.y2 : item.y1);
				}
				else
				{
					num14 = Stat.Random(item.y1 + 2, item.y2 - 2);
					num13 = ((Stat.Random(0, 1) != 0) ? item.x2 : item.x1);
				}
				if (num13 != box.x1 && num13 != box.x2 && num14 != box.y1 && num14 != box.y2 && ((!Z.GetCell(num13 - 1, num14).HasWall() && !Z.GetCell(num13 + 1, num14).HasWall()) || (!Z.GetCell(num13, num14 - 1).HasWall() && !Z.GetCell(num13, num14 + 1).HasWall())))
				{
					Z.GetCell(num13, num14).ClearWalls();
					break;
				}
			}
		}
		if (num10 == 0)
		{
			for (num = 1000; num > 0; num--)
			{
				Point randomPoint = box.Grow(-1).RandomPoint;
				if (Z.GetCell(randomPoint.X, randomPoint.Y).IsEmpty())
				{
					break;
				}
			}
		}
		if (ZoneTable != null)
		{
			string[] array = ZoneTable.Split(',');
			for (int num15 = 0; num15 < array.Length; num15++)
			{
				foreach (PopulationResult item2 in PopulationManager.Generate(array[num15], "zonetier", Z.NewTier.ToString()))
				{
					ZoneBuilderSandbox.PlaceObjectInArea(Z, Z.area, GameObjectFactory.Factory.CreateObject(item2.Blueprint), 0, 0, item2.Hint);
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
		int num16 = 0;
		for (int num17 = 0; num17 < 2; num17++)
		{
			int num18 = Stat.Random(1, 15);
			if ((num18 & 1) != 0)
			{
				num16 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (!Z.GetCell(num16, box.y1 + 2).HasWall() && Z.GetCell(num16, box.y1).HasWall())
				{
					Z.GetCell(num16, box.y1).ClearWalls();
					Z.GetCell(num16, box.y1).AddObject("BrinestalkArrowslit");
					Z.GetCell(num16, box.y1 + 1).ClearWalls();
					Z.GetCell(num16, box.y1 + 1).AddObject("BrinestalkArrowslit");
				}
			}
			if ((num18 & 2) != 0)
			{
				num16 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (!Z.GetCell(num16, box.y2 - 2).HasWall() && Z.GetCell(num16, box.y2).HasWall())
				{
					Z.GetCell(num16, box.y2).ClearWalls();
					Z.GetCell(num16, box.y2 - 1).ClearWalls();
					Z.GetCell(num16, box.y2).AddObject("BrinestalkArrowslit");
					Z.GetCell(num16, box.y2 - 1).AddObject("BrinestalkArrowslit");
				}
			}
			if ((num18 & 4) != 0)
			{
				int y = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (!Z.GetCell(box.x1 + 2, y).HasWall() && Z.GetCell(box.x1, y).HasWall())
				{
					Z.GetCell(box.x1, y).ClearWalls();
					Z.GetCell(box.x1 + 1, y).ClearWalls();
					Z.GetCell(box.x1, y).AddObject("BrinestalkArrowslit");
					Z.GetCell(box.x1 + 1, y).AddObject("BrinestalkArrowslit");
				}
			}
			if ((num18 & 8) != 0)
			{
				int y2 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (!Z.GetCell(box.x2 - 2, y2).HasWall() && Z.GetCell(box.x2, y2).HasWall())
				{
					Z.GetCell(box.x2, y2).ClearWalls();
					Z.GetCell(box.x2 - 1, y2).ClearWalls();
					Z.GetCell(box.x2, y2).AddObject("BrinestalkArrowslit");
					Z.GetCell(box.x2 - 1, y2).AddObject("BrinestalkArrowslit");
				}
			}
		}
		int num19 = 0;
		bool flag3;
		do
		{
			num19++;
			flag3 = false;
			int num20 = Stat.Random(1, 15);
			int num21 = -1;
			int num22 = -1;
			int num23 = -1;
			int num24 = -1;
			for (int num25 = box.x1 + 3; num25 <= box.x2 - 3; num25++)
			{
				if (Z.GetCell(num25, box.y1).HasObjectWithBlueprint("CanyonMarker"))
				{
					num21 = num25;
				}
				if (Z.GetCell(num25, box.y2).HasObjectWithBlueprint("CanyonMarker"))
				{
					num22 = num25;
				}
			}
			for (int num26 = box.y1 + 3; num26 <= box.y2 - 3; num26++)
			{
				if (Z.GetCell(box.x1, num26).HasObjectWithBlueprint("CanyonMarker"))
				{
					num24 = num26;
				}
				if (Z.GetCell(box.x2, num26).HasObjectWithBlueprint("CanyonMarker"))
				{
					num23 = num26;
				}
			}
			if ((num20 & 1) != 0 || num21 != -1)
			{
				num16 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (num21 != -1 && num19 < 10)
				{
					num16 = num21;
				}
				if (!Z.GetCell(num16, box.y1 + 2).HasWall())
				{
					Z.GetCell(num16, box.y1).ClearWalls();
					Z.GetCell(num16, box.y1 + 1).ClearWalls();
					flag3 = true;
				}
				if (!Z.GetCell(num16 + 1, box.y1 + 2).HasWall())
				{
					Z.GetCell(num16 + 1, box.y1).ClearWalls();
					Z.GetCell(num16 + 1, box.y1 + 1).ClearWalls();
					flag3 = true;
				}
			}
			if ((num20 & 2) != 0 || num22 != -1)
			{
				num16 = Stat.Random(box.x1 + 3, box.x2 - 3);
				if (num22 != -1 && num19 < 10)
				{
					num16 = num22;
				}
				if (!Z.GetCell(num16, box.y2 - 2).HasWall())
				{
					Z.GetCell(num16, box.y2).ClearWalls();
					Z.GetCell(num16, box.y2 - 1).ClearWalls();
					flag3 = true;
				}
				if (!Z.GetCell(num16 + 1, box.y2 - 2).HasWall())
				{
					Z.GetCell(num16 + 1, box.y2).ClearWalls();
					Z.GetCell(num16 + 1, box.y2 - 1).ClearWalls();
					flag3 = true;
				}
			}
			if ((num20 & 4) != 0 || num23 != -1)
			{
				int num27 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (num23 != -1 && num19 < 10)
				{
					num27 = num23;
				}
				if (!Z.GetCell(box.x1 + 2, num27).HasWall())
				{
					Z.GetCell(box.x1, num27).ClearWalls();
					Z.GetCell(box.x1 + 1, num27).ClearWalls();
					flag3 = true;
				}
				if (!Z.GetCell(box.x1 + 2, num27 + 1).HasWall())
				{
					Z.GetCell(box.x1, num27 + 1).ClearWalls();
					Z.GetCell(box.x1 + 1, num27 + 1).ClearWalls();
					flag3 = true;
				}
			}
			if ((num20 & 8) != 0 || num24 != -1)
			{
				int num28 = Stat.Random(box.y1 + 3, box.y2 - 3);
				if (num24 != -1 && num19 < 10)
				{
					num28 = num24;
				}
				if (!Z.GetCell(box.x2 - 2, num28).HasWall())
				{
					Z.GetCell(box.x2, num28).ClearWalls();
					Z.GetCell(box.x2 - 1, num28).ClearWalls();
					flag3 = true;
				}
				if (!Z.GetCell(box.x2 - 2, num28 + 1).HasWall())
				{
					Z.GetCell(box.x2, num28 + 1).ClearWalls();
					Z.GetCell(box.x2 - 1, num28 + 1).ClearWalls();
					flag3 = true;
				}
			}
		}
		while (!flag3);
		if (InsideStocakdeTable != null)
		{
			ZoneBuilderSandbox.PlacePopulationInRegion(Z, list2, InsideStocakdeTable);
		}
		ZoneBuilderSandbox.EnsureAllVoidsConnected(Z);
		return true;
	}
}
