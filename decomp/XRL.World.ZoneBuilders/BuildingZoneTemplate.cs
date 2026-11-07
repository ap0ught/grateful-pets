using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class BuildingZoneTemplate : IComposite
{
	public BuildingTemplate Template;

	public int Width;

	public int Height;

	public int ZonesWide;

	public int ZonesHigh;

	public Point2D GetRandomWall()
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Template.Map[j, i] == BuildingTemplateTile.Wall)
				{
					num++;
				}
			}
		}
		if (num > 0)
		{
			int num2 = Stat.Random(1, num);
			int num3 = 0;
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (Template.Map[l, k] == BuildingTemplateTile.Wall && ++num3 == num2)
					{
						return new Point2D(l, k);
					}
				}
			}
		}
		return new Point2D(0, 0);
	}

	public List<Point2D> GetWalls()
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Template.Map[j, i] == BuildingTemplateTile.Wall)
				{
					num++;
				}
			}
		}
		List<Point2D> list = new List<Point2D>(num);
		for (int k = 0; k < Height; k++)
		{
			for (int l = 0; l < Width; l++)
			{
				if (Template.Map[l, k] == BuildingTemplateTile.Wall)
				{
					list.Add(new Point2D(l, k));
				}
			}
		}
		return list;
	}

	public BuildingTemplate FromWFC(string wfcTemplateTable, string segmentation, int width, int height, int hmirrorchance = 20, int vmirrorchance = 20)
	{
		BuildingTemplate buildingTemplate = new BuildingTemplate(width, height, BuildingTemplateTile.Outside);
		Grid<Color4> grid = new Grid<Color4>(width, height);
		grid.fromWFCTemplate(PopulationManager.RollOneFrom(wfcTemplateTable).Blueprint.Split(',')[0]);
		Grid<Color4> grid2 = new Grid<Color4>(width, height);
		if (segmentation == "None")
		{
			grid2.fill(Color4.white);
		}
		else
		{
			grid2.fill(Color4.red);
			switch (segmentation)
			{
			case "Block":
			case "Blocks":
			case "BlocksAndCircles":
			{
				int num = 1;
				if (segmentation == "Blocks" || segmentation == "BlocksAndCircles")
				{
					num += Stat.Random(0, 2);
				}
				for (int i = 0; i < num; i++)
				{
					int num2 = Stat.Random(Math.Min(3, width), Math.Max(3, width));
					int num3 = Stat.Random(Math.Min(3, height), Math.Max(3, height));
					int num4 = Stat.Random(0, width - num2);
					int num5 = Stat.Random(0, height - num3);
					for (int j = num4; j < num4 + num2; j++)
					{
						for (int k = num5; k < num5 + num3; k++)
						{
							if (j <= 0 || k <= 0 || j >= width || k >= height)
							{
								continue;
							}
							if (j == num4 || k == num5 || j == num4 + num2 - 1 || k == num5 + num3 - 1)
							{
								if (grid2.get(j, k) == Color4.red)
								{
									grid2.set(j, k, Color4.black);
								}
							}
							else
							{
								grid2.set(j, k, Color4.white);
							}
						}
					}
				}
				break;
			}
			}
			switch (segmentation)
			{
			case "Circle":
			case "Circles":
			case "BlocksAndCircles":
			{
				int num6 = 1;
				if (segmentation == "Circles" || segmentation == "BlocksAndCircles")
				{
					num6 += Stat.Random(0, 2);
				}
				for (int l = 0; l < num6; l++)
				{
					int num7 = Stat.Random(2, Math.Max(3, (int)((float)Math.Min(width, height) / 2f)));
					int x = Stat.Random((int)((double)num7 / 0.6), Math.Max((int)((double)num7 / 0.6), width - (int)((double)num7 / 0.6)));
					int y = Stat.Random(num7, Math.Max(num7, height - num7));
					for (int m = 0; m <= height; m++)
					{
						for (int n = 0; n <= width; n++)
						{
							if (n <= 0 || m <= 0 || n >= width || m >= height || CosmeticDistanceTo(n, m, x, y) > num7)
							{
								continue;
							}
							if (CosmeticDistanceTo(n, m, x, y) == num7 || (CosmeticDistanceTo(n, m, x, y) <= num7 && n == 0) || m == 0 || n == width - 1 || m == height - 1)
							{
								if (grid2.get(n, m) == Color4.red)
								{
									grid2.set(n, m, Color4.black);
								}
							}
							else
							{
								grid2.set(n, m, Color4.white);
							}
						}
					}
				}
				break;
			}
			}
		}
		if (hmirrorchance.in100())
		{
			grid2 = grid2.mirrorHorizontal();
		}
		if (vmirrorchance.in100())
		{
			grid2 = grid2.mirrorVertical();
		}
		if (hmirrorchance.in100())
		{
			grid = grid.mirrorHorizontal();
		}
		if (vmirrorchance.in100())
		{
			grid = grid.mirrorVertical();
		}
		for (int num8 = 0; num8 < height; num8++)
		{
			for (int num9 = 0; num9 < width; num9++)
			{
				if (grid2.get(num9, num8) == Color4.red)
				{
					buildingTemplate.Map[num9, num8] = BuildingTemplateTile.Outside;
				}
				else if (grid.get(num9, num8) == Color4.black || grid2.get(num9, num8) == Color4.black)
				{
					buildingTemplate.Map[num9, num8] = BuildingTemplateTile.Wall;
				}
				else if (grid.get(num9, num8) == Color4.white)
				{
					buildingTemplate.Map[num9, num8] = BuildingTemplateTile.Outside;
				}
				else
				{
					buildingTemplate.Map[num9, num8] = BuildingTemplateTile.Inside;
				}
			}
		}
		return buildingTemplate;
		static int CosmeticDistanceTo(int num10, int num11, int X, int Y)
		{
			return (int)Math.Sqrt((float)(num10 - X) * 0.6666f * ((float)(num10 - X) * 0.6666f) + (float)((num11 - Y) * (num11 - Y)));
		}
	}

	public void New(int nWidth, int nHeight, int ZonesWide, int ZonesHigh, string wfcTemplateTable = "Ruins_PrimaryTemplate_*Default")
	{
		Template = new BuildingTemplate(nWidth, nHeight, BuildingTemplateTile.Outside);
		Width = nWidth;
		Height = nHeight;
		string blueprint = PopulationManager.RollOneFrom("Ruins_Style").Blueprint;
		int num = 0;
		int num2 = 50;
		blueprint = "WFC Common Segmentation";
		if (blueprint == "Classic")
		{
			num = 0;
		}
		if (blueprint == "WFC Chance Per Building")
		{
			num = 20;
		}
		if (blueprint == "WFC Common Segmentation")
		{
			num = 100;
		}
		if (blueprint == "WFC Same Template")
		{
			num = 100;
		}
		string text = null;
		bool flag = false;
		BuildingTemplate source = null;
		for (int i = 0; i < ZonesWide; i++)
		{
			for (int j = 0; j < ZonesHigh; j++)
			{
				int num3 = 80 / ZonesWide;
				int num4 = 25 / ZonesHigh;
				int num5 = num3 * i;
				int num6 = num3 * (i + 1);
				int startY = num4 * j;
				if (num6 - num5 > 35 && Stat.Random(1, 100) <= 35)
				{
					num6 -= (num6 - num5) / 2;
				}
				if (flag && blueprint == "WFC Same Template")
				{
					Template.AddMap(num5, startY, source);
				}
				else
				{
					if (flag && Stat.Random(1, 100) > num2)
					{
						continue;
					}
					flag = true;
					if (Stat.Random(1, 100) <= num)
					{
						if (blueprint != "WFC Common Segmentation" || text == null)
						{
							text = PopulationManager.RollOneFrom("Ruins_Segmentation").Blueprint;
						}
						source = FromWFC(wfcTemplateTable, text, num3, num4);
					}
					else
					{
						source = new BuildingTemplate(num3, num4, 8 / (ZonesWide * ZonesHigh), FullSquare: false);
					}
					Template.AddMap(num5, startY, source);
				}
			}
		}
		for (int k = 0; k < Width; k++)
		{
			for (int l = 0; l < Height && (Template.Map[k, l] == BuildingTemplateTile.Inside || Template.Map[k, l] == BuildingTemplateTile.Outside); l++)
			{
				Template.Map[k, l] = BuildingTemplateTile.Outside;
			}
			int num7 = Height - 1;
			while (num7 >= 0 && (Template.Map[k, num7] == BuildingTemplateTile.Inside || Template.Map[k, num7] == BuildingTemplateTile.Outside))
			{
				Template.Map[k, num7] = BuildingTemplateTile.Outside;
				num7--;
			}
		}
		for (int m = 0; m < Height; m++)
		{
			for (int n = 0; n < Width && (Template.Map[n, m] == BuildingTemplateTile.Inside || Template.Map[n, m] == BuildingTemplateTile.Outside); n++)
			{
				Template.Map[n, m] = BuildingTemplateTile.Outside;
			}
			int num8 = Width - 1;
			while (num8 >= 0 && (Template.Map[num8, m] == BuildingTemplateTile.Inside || Template.Map[num8, m] == BuildingTemplateTile.Outside))
			{
				Template.Map[num8, m] = BuildingTemplateTile.Outside;
				num8--;
			}
		}
	}

	public void BuildZone(Zone Z, bool bUnderground)
	{
		GameObject terrainObject = Z.GetTerrainObject();
		string Value;
		if (terrainObject.HasTag("RuinWalls"))
		{
			if (!The.ZoneManager.TryGetWorldCellProperty<string>(Z.ZoneID, "RuinWallType", out Value))
			{
				Value = PopulationManager.RollOneFrom(terrainObject.GetTag("RuinWalls")).Blueprint;
				The.ZoneManager.SetWorldCellProperty(Z.ZoneID, "RuinWallType", Value);
			}
		}
		else
		{
			Value = "Fulcrete";
		}
		for (int i = 0; i < Z.Height; i++)
		{
			for (int j = 0; j < Z.Width; j++)
			{
				Cell cell = Z.GetCell(j, i);
				if (j >= Template.Width || i >= Template.Height)
				{
					cell.Clear(null, Important: false, Combat: false, (GameObject go) => go.HasTag("NoRuinsRemove"));
				}
				else if (Template.Map[j, i] == BuildingTemplateTile.OutsideWall || Template.Map[j, i] == BuildingTemplateTile.Wall)
				{
					cell.Clear(null, Important: false, Combat: false, (GameObject go) => go.HasTag("NoRuinsRemove"));
					cell.AddObject(Value);
				}
				else if (Template.Map[j, i] == BuildingTemplateTile.Door)
				{
					cell.Clear(null, Important: false, Combat: false, (GameObject go) => go.HasTag("NoRuinsRemove"));
					cell.AddObject("Door");
				}
				else if (Template.Map[j, i] == BuildingTemplateTile.StairsDown)
				{
					cell.Clear();
					if (Z.GetZoneZ() != 10)
					{
						if (Z.GetZoneZ() % 2 == 0)
						{
							cell.AddObject("StairsUp");
						}
						else
						{
							cell.AddObject("StairsDown");
						}
					}
				}
				else if (Template.Map[j, i] == BuildingTemplateTile.StairsUp)
				{
					cell.Clear();
					if (Z.GetZoneZ() % 2 == 0)
					{
						cell.AddObject("StairsDown");
					}
					else
					{
						cell.AddObject("StairsUp");
					}
				}
				else if ((!bUnderground || Template.Map[j, i] != BuildingTemplateTile.Outside) && !bUnderground && Template.Map[j, i] == BuildingTemplateTile.Inside && !cell.HasObjectWithTag("NoRuinsRemove"))
				{
					cell.ClearWalls();
				}
			}
		}
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection != "-")
			{
				Z.GetCell(item.X, item.Y).Clear();
			}
		}
	}
}
