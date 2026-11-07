using System;
using System.Collections.Generic;
using System.IO;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.ZoneBuilders;

[HasModSensitiveStaticCache]
public class TileBuilding
{
	[ModSensitiveStaticCache(false)]
	private static TileManager Tiles;

	public string WallMaterial = "Fulcrete";

	public string ShellMaterial = "Fulcrete";

	public bool Shell;

	public int Wide = 13;

	public int High = 4;

	public int XCorner;

	public int YCorner;

	public string TileStartMarker;

	public string TileEndMarker;

	public int NumberOfVaults;

	public int ChancePerVault;

	public int VaultWidth;

	public int VaultHeight;

	public string VaultStartMarker;

	public string VaultEndMarker;

	public int NumberOfVaults2;

	public int ChancePerVault2;

	public int VaultWidth2;

	public int VaultHeight2;

	public string VaultStartMarker2;

	public string VaultEndMarker2;

	public string CustomCharMap;

	public Dictionary<char, string> CustomChar;

	[PreGameCacheInit]
	public static void CheckInit()
	{
		if (Tiles == null)
		{
			Loading.LoadTask("Loading BuildingTiles.txt", LoadTiles);
		}
	}

	private static void LoadTiles()
	{
		Tiles = new TileManager();
		try
		{
			List<string> TileFiles = new List<string>();
			TileFiles.Add(DataManager.FilePath("BuildingTiles.txt"));
			TileFiles.AddRange(Directory.GetFiles(DataManager.FilePath("."), "BuildingTiles_*.txt", SearchOption.AllDirectories));
			ModManager.ForEachFile("BuildingTiles.txt", (string path) =>
			{
				TileFiles.Add(path);
			});
			foreach (string item in TileFiles)
			{
				using StreamReader streamReader = new StreamReader(item);
				string text = "";
				bool flag = true;
				bool flag2 = true;
				bool flag3 = false;
				bool flag4 = true;
				bool flag5 = true;
				bool flag6 = false;
				while (text != null)
				{
					while (true)
					{
						text = streamReader.ReadLine();
						while (text != null && text.Length <= 0)
						{
							text = streamReader.ReadLine();
						}
						if (text == null || text == "end")
						{
							break;
						}
						if (text.Length > 0 && text[0] == '[')
						{
							Tiles.Markers.Add(text.Trim('[', ']'), Tiles.Tiles.Count);
							flag4 = true;
							flag5 = true;
							flag6 = false;
							continue;
						}
						switch (text)
						{
						case "*X":
							flag6 = true;
							continue;
						case "X":
							flag3 = true;
							continue;
						case "*NF":
							flag5 = false;
							continue;
						case "*NM":
							flag4 = false;
							continue;
						case "NF":
							flag2 = false;
							continue;
						case "NM":
							flag = false;
							continue;
						}
						List<string> list = new List<string>();
						list.Add(text);
						for (int num = 0; num < Tiles.TileSize - 1; num++)
						{
							list.Add(streamReader.ReadLine());
						}
						if (!flag3 && !flag6)
						{
							Tiles.AddTile(list, flag & flag4, flag2 & flag5);
						}
						flag = true;
						flag2 = true;
						flag3 = false;
						foreach (string item2 in list)
						{
							_ = item2;
						}
						goto IL_0233;
					}
					break;
					IL_0233:;
				}
			}
		}
		catch (Exception ex)
		{
			XRLCore.LogError(ex);
		}
	}

	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).AddObject("Dirty");
		CustomChar = new Dictionary<char, string>();
		if (CustomCharMap != null)
		{
			string[] array = CustomCharMap.Split(',');
			foreach (string text in array)
			{
				if (text != "")
				{
					string[] array2 = text.Split('@');
					CustomChar.Add(array2[0][0], array2[1]);
				}
			}
		}
		TileType[,] array3 = new TileType[Wide * (Tiles.TileSize - 1) + 1, High * (Tiles.TileSize - 1) + 1];
		char[,] array4 = new char[Wide * (Tiles.TileSize - 1) + 1, High * (Tiles.TileSize - 1) + 1];
		bool[,] array5 = new bool[Wide * (Tiles.TileSize - 1) + 1, High * (Tiles.TileSize - 1) + 1];
		for (int j = 0; j <= array4.GetUpperBound(0); j++)
		{
			for (int k = 0; k <= array4.GetUpperBound(1); k++)
			{
				array4[j, k] = '.';
				array5[j, k] = false;
			}
		}
		Z.ClearBox(new Box(XCorner, YCorner, XCorner + Wide * (Tiles.TileSize - 1) + 1, YCorner + High * (Tiles.TileSize - 1)));
		int[,] array6 = new int[Wide, High];
		for (int l = 0; l < Wide; l++)
		{
			for (int m = 0; m < High; m++)
			{
				array6[l, m] = -1;
			}
		}
		List<Point> list = new List<Point>();
		for (int n = 0; n < Wide; n++)
		{
			for (int num = 0; num < High; num++)
			{
				list.Add(new Point(n, num));
			}
		}
		for (int num2 = 0; num2 < NumberOfVaults; num2++)
		{
			int num3 = (Tiles.Markers[VaultEndMarker] - Tiles.Markers[VaultStartMarker]) / (VaultWidth * VaultHeight);
			if (Stat.Random(1, 100) > ChancePerVault)
			{
				continue;
			}
			for (int num4 = 0; num4 < 10; num4++)
			{
				int num5 = Stat.Random(0, Wide - VaultWidth - 1);
				int num6 = Stat.Random(0, High - VaultHeight - 1);
				for (int num7 = 0; num7 < VaultWidth; num7++)
				{
					int num8 = 0;
					while (num8 < VaultHeight)
					{
						if (array6[num5 + num7, num6 + num8] == -1)
						{
							num8++;
							continue;
						}
						goto IL_03e5;
					}
				}
				int num9 = Stat.Random(0, num3 - 1);
				int num10 = Tiles.Markers[VaultStartMarker] + num9 * (VaultWidth * VaultHeight);
				for (int num11 = 0; num11 < VaultHeight; num11++)
				{
					for (int num12 = 0; num12 < VaultWidth; num12++)
					{
						array6[num5 + num12, num6 + num11] = num10;
						num10++;
					}
				}
				for (int num13 = num5 * (Tiles.TileSize - 1); num13 < (num5 + VaultWidth) * (Tiles.TileSize - 1); num13++)
				{
					for (int num14 = num6 * (Tiles.TileSize - 1); num14 < (num6 + VaultHeight) * (Tiles.TileSize - 1); num14++)
					{
						array5[num13, num14] = true;
					}
				}
				break;
				IL_03e5:;
			}
		}
		for (int num15 = 0; num15 < NumberOfVaults2; num15++)
		{
			int num16 = (Tiles.Markers[VaultEndMarker2] - Tiles.Markers[VaultStartMarker2]) / (VaultWidth2 * VaultHeight2);
			if (Stat.Random(1, 100) > ChancePerVault2)
			{
				continue;
			}
			for (int num17 = 0; num17 < 10; num17++)
			{
				int num18 = Stat.Random(0, Wide - VaultWidth2 - 1);
				int num19 = Stat.Random(0, High - VaultHeight2 - 1);
				for (int num20 = 0; num20 < VaultWidth2; num20++)
				{
					int num21 = 0;
					while (num21 < VaultHeight2)
					{
						if (array6[num18 + num20, num19 + num21] == -1)
						{
							num21++;
							continue;
						}
						goto IL_05bc;
					}
				}
				int num22 = Stat.Random(0, num16 - 1);
				int num23 = Tiles.Markers[VaultStartMarker2] + num22 * (VaultWidth2 * VaultHeight2);
				for (int num24 = 0; num24 < VaultHeight2; num24++)
				{
					for (int num25 = 0; num25 < VaultWidth2; num25++)
					{
						array6[num18 + num25, num19 + num24] = num23;
						num23++;
					}
				}
				for (int num26 = num18 * (Tiles.TileSize - 1); num26 <= (num18 + VaultWidth2) * (Tiles.TileSize - 1); num26++)
				{
					for (int num27 = num19 * (Tiles.TileSize - 1); num27 <= (num19 + VaultHeight2) * (Tiles.TileSize - 1); num27++)
					{
						array5[num26, num27] = true;
					}
				}
				break;
				IL_05bc:;
			}
		}
		new List<Point>(Algorithms.RandomShuffle(list, Stat.Rand));
		foreach (Point item in list)
		{
			int x = item.X;
			int y = item.Y;
			if (array6[x, y] != -1)
			{
				continue;
			}
			int num28;
			for (num28 = 500; num28 > 0; num28--)
			{
				int index = (array6[x, y] = Stat.Random(Tiles.Markers[TileStartMarker], Tiles.Markers[TileEndMarker] - 1));
				if (x == 0)
				{
					int num29 = 0;
					while (num29 < Tiles.TileSize)
					{
						if (Tiles.Tiles[index].Tile[0, num29] == TileType.Wall)
						{
							num29++;
							continue;
						}
						goto IL_09e9;
					}
				}
				else if (x > 0 && array6[x - 1, y] != -1)
				{
					int num30 = 0;
					while (num30 < Tiles.TileSize)
					{
						if (BuildingTile.Matches(Tiles.Tiles[array6[x - 1, y]].Tile[Tiles.TileSize - 1, num30], Tiles.Tiles[index].Tile[0, num30]))
						{
							num30++;
							continue;
						}
						goto IL_09e9;
					}
				}
				if (x >= Wide - 1)
				{
					int num31 = 0;
					while (num31 < Tiles.TileSize)
					{
						if (Tiles.Tiles[index].Tile[Tiles.TileSize - 1, num31] == TileType.Wall)
						{
							num31++;
							continue;
						}
						goto IL_09e9;
					}
				}
				else if (x < Wide - 1 && array6[x + 1, y] != -1)
				{
					int num32 = 0;
					while (num32 < Tiles.TileSize)
					{
						if (BuildingTile.Matches(Tiles.Tiles[array6[x + 1, y]].Tile[0, num32], Tiles.Tiles[index].Tile[Tiles.TileSize - 1, num32]))
						{
							num32++;
							continue;
						}
						goto IL_09e9;
					}
				}
				if (y == 0)
				{
					int num33 = 0;
					while (num33 < Tiles.TileSize)
					{
						if (Tiles.Tiles[index].Tile[num33, 0] == TileType.Wall)
						{
							num33++;
							continue;
						}
						goto IL_09e9;
					}
				}
				else if (y > 0 && array6[x, y - 1] != -1)
				{
					int num34 = 0;
					while (num34 < Tiles.TileSize)
					{
						if (BuildingTile.Matches(Tiles.Tiles[array6[x, y - 1]].Tile[num34, Tiles.TileSize - 1], Tiles.Tiles[index].Tile[num34, 0]))
						{
							num34++;
							continue;
						}
						goto IL_09e9;
					}
				}
				if (y >= High - 1)
				{
					int num35 = 0;
					while (num35 < Tiles.TileSize)
					{
						if (Tiles.Tiles[index].Tile[num35, Tiles.TileSize - 1] == TileType.Wall)
						{
							num35++;
							continue;
						}
						goto IL_09e9;
					}
				}
				else
				{
					if (y >= High - 1 || array6[x, y + 1] == -1)
					{
						break;
					}
					int num36 = 0;
					while (num36 < Tiles.TileSize)
					{
						if (BuildingTile.Matches(Tiles.Tiles[array6[x, y + 1]].Tile[num36, 0], Tiles.Tiles[index].Tile[num36, Tiles.TileSize - 1]))
						{
							num36++;
							continue;
						}
						goto IL_09e9;
					}
				}
				break;
				IL_09e9:;
			}
			if (num28 <= 0)
			{
				if (x > 0 && array6[x - 1, y] != -1)
				{
					Tiles.Tiles[array6[x - 1, y]].Trace();
				}
				if (x < Wide - 1 && array6[x + 1, y] != -1)
				{
					Tiles.Tiles[array6[x + 1, y]].Trace();
				}
				if (y > 0 && array6[x, y - 1] != -1)
				{
					Tiles.Tiles[array6[x, y - 1]].Trace();
				}
				if (y < High - 1 && array6[x, y + 1] != -1)
				{
					Tiles.Tiles[array6[x, y + 1]].Trace();
				}
			}
		}
		for (int num37 = 0; num37 < Wide; num37++)
		{
			for (int num38 = 0; num38 < High; num38++)
			{
				int num39 = Tiles.TileSize - 1;
				int num40 = Tiles.TileSize - 1;
				if (num37 == Wide - 1)
				{
					num39 = Tiles.TileSize;
				}
				if (num38 == High - 1)
				{
					num40 = Tiles.TileSize;
				}
				for (int num41 = 0; num41 < num39; num41++)
				{
					for (int num42 = 0; num42 < num40; num42++)
					{
						List<TileType> list2 = new List<TileType>();
						list2.Add(Tiles.Tiles[array6[num37, num38]].Tile[num41, num42]);
						int num43 = num37 * (Tiles.TileSize - 1) + num41;
						int num44 = num38 * (Tiles.TileSize - 1) + num42;
						if (Tiles.Tiles[array6[num37, num38]].CustomTile[num41, num42] != '?')
						{
							array4[num43, num44] = Tiles.Tiles[array6[num37, num38]].CustomTile[num41, num42];
						}
						if (num43 < 0 || num43 >= Z.Width || num44 <= 0 || num44 >= Z.Height)
						{
							continue;
						}
						if (num41 == 0 && num37 > 0)
						{
							list2.Add(Tiles.Tiles[array6[num37 - 1, num38]].Tile[Tiles.TileSize - 1, num42]);
							if (Tiles.Tiles[array6[num37 - 1, num38]].CustomTile[Tiles.TileSize - 1, num42] != '?')
							{
								array4[num43, num44] = Tiles.Tiles[array6[num37 - 1, num38]].CustomTile[Tiles.TileSize - 1, num42];
							}
						}
						if (num42 == 0 && num38 > 0)
						{
							list2.Add(Tiles.Tiles[array6[num37, num38 - 1]].Tile[num41, Tiles.TileSize - 1]);
							if (Tiles.Tiles[array6[num37, num38 - 1]].CustomTile[num41, Tiles.TileSize - 1] != '?')
							{
								array4[num43, num44] = Tiles.Tiles[array6[num37, num38 - 1]].CustomTile[num41, Tiles.TileSize - 1];
							}
						}
						if (num41 == 0 && num42 == 0 && num37 > 0 && num38 > 0)
						{
							list2.Add(Tiles.Tiles[array6[num37 - 1, num38 - 1]].Tile[Tiles.TileSize - 1, Tiles.TileSize - 1]);
							if (Tiles.Tiles[array6[num37 - 1, num38 - 1]].CustomTile[Tiles.TileSize - 1, Tiles.TileSize - 1] != '?')
							{
								array4[num43, num44] = Tiles.Tiles[array6[num37 - 1, num38 - 1]].CustomTile[Tiles.TileSize - 1, Tiles.TileSize - 1];
							}
						}
						if (list2.CleanContains(TileType.OpenConnect))
						{
							Z.GetCell(num43 + XCorner, num44 + YCorner).AddObject("ConnectWidget");
						}
						array3[num43, num44] = TileType.Open;
						if (list2.CleanContains(TileType.Door) && !list2.CleanContains(TileType.Wall))
						{
							array3[num43, num44] = TileType.Door;
						}
						else if (list2.CleanContains(TileType.Wall))
						{
							array3[num43, num44] = TileType.Wall;
						}
						if (list2.CleanContains(TileType.Garbage))
						{
							array3[num43, num44] = TileType.Garbage;
						}
						if (list2.CleanContains(TileType.LittleChest))
						{
							array3[num43, num44] = TileType.LittleChest;
						}
						if (list2.CleanContains(TileType.BigChest))
						{
							array3[num43, num44] = TileType.BigChest;
						}
						if (list2.CleanContains(TileType.Creature1))
						{
							array3[num43, num44] = TileType.Creature1;
						}
						if (list2.CleanContains(TileType.Liquid))
						{
							array3[num43, num44] = TileType.Liquid;
						}
					}
				}
			}
		}
		bool flag = true;
		while (flag)
		{
			flag = false;
			for (int num45 = 0; num45 < array3.GetUpperBound(0) + 1; num45++)
			{
				for (int num46 = 0; num46 < array3.GetUpperBound(1) + 1; num46++)
				{
					if (num45 == 0 || num45 == array3.GetUpperBound(0) || num46 == -1 || num46 == array3.GetUpperBound(1))
					{
						array3[num45, num46] = TileType.Wall;
					}
					else if (array3[num45, num46] == TileType.Door)
					{
						int num47 = 0;
						if (array3[num45 - 1, num46] == TileType.Door)
						{
							num47++;
						}
						if (array3[num45 + 1, num46] == TileType.Door)
						{
							num47++;
						}
						if (array3[num45, num46 + 1] == TileType.Door)
						{
							num47++;
						}
						if (array3[num45, num46 + 1] == TileType.Door)
						{
							num47++;
						}
						int num48 = 0;
						if (array3[num45 - 1, num46] == TileType.Open)
						{
							num48++;
						}
						if (array3[num45 + 1, num46] == TileType.Open)
						{
							num48++;
						}
						if (array3[num45, num46 + 1] == TileType.Open)
						{
							num48++;
						}
						if (array3[num45, num46 + 1] == TileType.Open)
						{
							num48++;
						}
						if (num48 != 2)
						{
							flag = true;
							array3[num45, num46] = TileType.Wall;
						}
					}
				}
			}
		}
		for (int num49 = 0; num49 < array3.GetUpperBound(0) + 1; num49++)
		{
			for (int num50 = 0; num50 < array3.GetUpperBound(1) + 1; num50++)
			{
				if (CustomChar.ContainsKey(array4[num49, num50]))
				{
					if (CustomChar[array4[num49, num50]] == "(clear)")
					{
						Z.GetCell(num49 + XCorner, num50 + YCorner).ClearWalls();
					}
					else
					{
						Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject(CustomChar[array4[num49, num50]]);
					}
					continue;
				}
				if (array3[num49, num50] == TileType.Wall)
				{
					Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject(WallMaterial);
				}
				if (array3[num49, num50] == TileType.Door)
				{
					Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject("Door");
				}
				if (array3[num49, num50] == TileType.LittleChest)
				{
					Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject("Locker2");
				}
				if (array3[num49, num50] == TileType.BigChest)
				{
					Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject("MedLocker");
				}
				if (array3[num49, num50] == TileType.Creature1)
				{
					Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject("Boosterbot");
				}
				if (array3[num49, num50] == TileType.Liquid)
				{
					if (num49 == 0 || num49 >= array3.GetUpperBound(0) || num50 == 0 || num50 >= array3.GetUpperBound(1))
					{
						Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject("CyanMarbleWalkway");
					}
					else
					{
						int num51 = 0;
						for (int num52 = -1; num52 <= 1; num52++)
						{
							for (int num53 = -1; num53 <= 1; num53++)
							{
								if (array3[num49 + num52, num50 + num53] == TileType.Liquid)
								{
									num51++;
								}
							}
						}
						if (num51 == 9)
						{
							Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject("ConvalessencePuddle");
						}
						else
						{
							Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject("CyanMarbleWalkway");
						}
					}
				}
				if (array3[num49, num50] == TileType.Garbage)
				{
					Z.GetCell(num49 + XCorner, num50 + YCorner).AddTableObject("MedScrap");
				}
				if (array5[num49, num50])
				{
					Z.GetCell(num49 + XCorner, num50 + YCorner).AddObject("SpawnBlocker");
				}
			}
		}
		if (Shell)
		{
			Z.FillHollowBox(new Box(XCorner - 1, YCorner, Math.Min(Z.Width - 1, XCorner + Wide * (Tiles.TileSize - 1) + 1), Math.Min(Z.Height - 1, YCorner + High * (Tiles.TileSize - 1) + 1)), WallMaterial);
			Z.FillHollowBox(new Box(XCorner - 1, YCorner - 1, Math.Min(Z.Width - 1, XCorner + Wide * (Tiles.TileSize - 1) + 1), Math.Min(Z.Height - 1, YCorner + High * (Tiles.TileSize - 1) + 1)), ShellMaterial);
		}
		foreach (CachedZoneConnection item2 in Z.ZoneConnectionCache)
		{
			if (item2.TargetDirection == "-")
			{
				Z.GetCell(item2.X, item2.Y).Clear();
			}
		}
		new ForceConnections().BuildZone(Z);
		new Connecter().BuildZone(Z);
		return true;
	}
}
