using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Waterway
{
	public bool BuildZone(Zone Z)
	{
		MazeCell mazeCell = XRLCore.Core.Game.WorldMazes["QudWaterwayMaze"].Cell[Z.wX, Z.wY];
		if ((Z.X == 1 && Z.Y == 0 && mazeCell.N) || (Z.X == 1 && Z.Y == 2 && mazeCell.S) || (Z.X == 1 && Z.Y == 1 && !mazeCell.E && !mazeCell.W && mazeCell.N && mazeCell.S))
		{
			for (int i = 30; i <= 50; i++)
			{
				for (int j = 0; j < Z.Height; j++)
				{
					Z.GetCell(i, j).ClearWalls();
					Z.GetCell(i, j).ClearObjectsWithPart("Door");
				}
			}
			for (int k = 0; k < Z.Height; k++)
			{
			}
			for (int l = 0; l < Z.Height; l++)
			{
				foreach (GameObject wall in Z.GetCell(29, l).GetWalls())
				{
					Z.GetCell(29, l).RemoveObject(wall);
					Z.GetCell(29, l).AddObject("Fulcrete");
				}
				foreach (GameObject wall2 in Z.GetCell(51, l).GetWalls())
				{
					Z.GetCell(51, l).RemoveObject(wall2);
					Z.GetCell(51, l).AddObject("Fulcrete");
				}
			}
			Z.CacheZoneConnection("-", Z.Width / 2, 0, "RiverNorthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2, Z.Height - 1, "RiverSouthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2 - 5, 0, "RiverNorthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2 + 5, Z.Height - 1, "RiverSouthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2 + 5, 0, "RiverNorthMouth", null);
			Z.CacheZoneConnection("-", Z.Width / 2 - 5, Z.Height - 1, "RiverSouthMouth", null);
			if (!new RiverBuilder
			{
				Pairs = true
			}.BuildZone(Z))
			{
				_ = Z.BuildTries;
				_ = 15;
				return true;
			}
		}
		else if ((Z.X == 0 && Z.Y == 1 && mazeCell.W) || (Z.X == 2 && Z.Y == 1 && mazeCell.E) || (Z.X == 1 && Z.Y == 1 && mazeCell.E && mazeCell.W && !mazeCell.N && !mazeCell.S))
		{
			for (int m = 7; m <= 18; m++)
			{
				for (int n = 0; n < Z.Width; n++)
				{
					Z.GetCell(n, m).ClearWalls();
					Z.GetCell(n, m).ClearObjectsWithPart("Door");
				}
			}
			for (int num = 0; num < Z.Width; num++)
			{
				foreach (Cell localAdjacentCell in Z.GetCell(num, 12).GetLocalAdjacentCells(Stat.Random(1, 4)))
				{
					localAdjacentCell.AddObject("SaltyWaterDeepPool");
				}
			}
			for (int num2 = 0; num2 < Z.Width; num2++)
			{
				foreach (GameObject wall3 in Z.GetCell(num2, 6).GetWalls())
				{
					Z.GetCell(num2, 6).RemoveObject(wall3);
					Z.GetCell(num2, 6).AddObject("Fulcrete");
				}
				foreach (GameObject wall4 in Z.GetCell(num2, 19).GetWalls())
				{
					Z.GetCell(num2, 19).RemoveObject(wall4);
					Z.GetCell(num2, 19).AddObject("Fulcrete");
				}
			}
		}
		else if (Z.X == 1 && Z.Y == 1)
		{
			if (mazeCell.N)
			{
				for (int num3 = 0; num3 < 18; num3++)
				{
					for (int num4 = 30; num4 <= 50; num4++)
					{
						Z.GetCell(num4, num3).ClearWalls();
					}
				}
				for (int num5 = 0; num5 < 18; num5++)
				{
					foreach (GameObject wall5 in Z.GetCell(29, num5).GetWalls())
					{
						Z.GetCell(29, num5).RemoveObject(wall5);
						Z.GetCell(29, num5).AddObject("Fulcrete");
					}
					foreach (GameObject wall6 in Z.GetCell(51, num5).GetWalls())
					{
						Z.GetCell(51, num5).RemoveObject(wall6);
						Z.GetCell(51, num5).AddObject("Fulcrete");
					}
				}
			}
			if (mazeCell.S)
			{
				for (int num6 = 7; num6 < Z.Height; num6++)
				{
					for (int num7 = 30; num7 <= 50; num7++)
					{
						Z.GetCell(num7, num6).ClearWalls();
					}
				}
				for (int num8 = 7; num8 < Z.Height; num8++)
				{
					foreach (GameObject wall7 in Z.GetCell(29, num8).GetWalls())
					{
						Z.GetCell(29, num8).RemoveObject(wall7);
						Z.GetCell(29, num8).AddObject("Fulcrete");
					}
					foreach (GameObject wall8 in Z.GetCell(51, num8).GetWalls())
					{
						Z.GetCell(51, num8).RemoveObject(wall8);
						Z.GetCell(51, num8).AddObject("Fulcrete");
					}
				}
			}
			if (mazeCell.W)
			{
				for (int num9 = 7; num9 <= 18; num9++)
				{
					for (int num10 = 0; num10 < 40; num10++)
					{
						Z.GetCell(num10, num9).ClearWalls();
					}
				}
			}
			if (mazeCell.E)
			{
				for (int num11 = 7; num11 <= 18; num11++)
				{
					for (int num12 = 30; num12 < Z.Width; num12++)
					{
						Z.GetCell(num12, num11).ClearWalls();
					}
				}
			}
			for (int num13 = 0; num13 < Z.Height; num13++)
			{
				List<GameObject> walls = Z.GetCell(29, num13).GetWalls();
				foreach (GameObject item in walls)
				{
					Z.GetCell(29, num13).RemoveObject(item);
					if (item == walls[0])
					{
						Z.GetCell(29, num13).AddObject("Fulcrete");
					}
				}
				walls = Z.GetCell(51, num13).GetWalls();
				foreach (GameObject item2 in walls)
				{
					Z.GetCell(51, num13).RemoveObject(item2);
					if (item2 == walls[0])
					{
						Z.GetCell(51, num13).AddObject("Fulcrete");
					}
				}
			}
			for (int num14 = 0; num14 < Z.Width; num14++)
			{
				foreach (GameObject wall9 in Z.GetCell(num14, 6).GetWalls())
				{
					Z.GetCell(num14, 6).RemoveObject(wall9);
					Z.GetCell(num14, 6).AddObject("Fulcrete");
				}
				foreach (GameObject wall10 in Z.GetCell(num14, 19).GetWalls())
				{
					Z.GetCell(num14, 19).RemoveObject(wall10);
					Z.GetCell(num14, 19).AddObject("Fulcrete");
				}
			}
		}
		return true;
	}
}
