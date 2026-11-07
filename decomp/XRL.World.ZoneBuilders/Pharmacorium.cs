using Genkit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Pharmacorium
{
	public bool BuildZone(Zone Z)
	{
		Maze maze = RecursiveBacktrackerMaze.GenerateWithHoles(20, 6, bShow: false, 30, bDefault: false, XRLCore.Core.Game.GetWorldSeed("PharmacoriumMaze" + Z.ZoneID));
		for (int i = 0; i < 15; i++)
		{
			int num = Stat.Random(1, 9);
			int num2 = Stat.Random(0, 2);
			if (Stat.Random(0, 100) <= 25)
			{
				maze.Cell[num, num2].N = true;
			}
			if (Stat.Random(0, 100) <= 25)
			{
				maze.Cell[num, num2].S = true;
			}
			if (Stat.Random(0, 100) <= 25)
			{
				maze.Cell[num, num2].E = true;
			}
			if (Stat.Random(0, 100) <= 25)
			{
				maze.Cell[num, num2].W = true;
			}
		}
		string blueprint = "Marble";
		Z.Fill(blueprint);
		bool[,] array = new bool[20, 6];
		for (int j = 0; j < 20; j++)
		{
			for (int k = 0; k < 6; k++)
			{
				if (maze.Cell[j, k].AnyOpen())
				{
					array[j, k] = false;
				}
				else
				{
					array[j, k] = true;
				}
			}
		}
		bool flag = true;
		while (flag)
		{
			flag = false;
			for (int l = 1; l < 19; l++)
			{
				for (int m = 1; m < 5; m++)
				{
					if (!array[l, m])
					{
						continue;
					}
					if (array[l - 1, m - 1] && !array[l - 1, m] && !array[l, m - 1])
					{
						if (Stat.Random(0, 1) == 0)
						{
							array[l - 1, m] = true;
						}
						else
						{
							array[l, m - 1] = true;
						}
						flag = true;
					}
					if (array[l - 1, m + 1] && !array[l - 1, m] && !array[l, m + 1])
					{
						if (Stat.Random(0, 1) == 0)
						{
							array[l - 1, m] = true;
						}
						else
						{
							array[l, m + 1] = true;
						}
						flag = true;
					}
					if (array[l + 1, m - 1] && !array[l + 1, m] && !array[l, m - 1])
					{
						if (Stat.Random(0, 1) == 0)
						{
							array[l + 1, m] = true;
						}
						else
						{
							array[l, m - 1] = true;
						}
						flag = true;
					}
					if (array[l + 1, m + 1] && !array[l + 1, m] && !array[l, m + 1])
					{
						if (Stat.Random(0, 1) == 0)
						{
							array[l + 1, m] = true;
						}
						else
						{
							array[l, m + 1] = true;
						}
						flag = true;
					}
				}
			}
		}
		for (int n = 0; n < 20; n++)
		{
			for (int num3 = 0; num3 < 6; num3++)
			{
				int num4 = n * 4;
				int num5 = num3 * 4;
				Z.ClearBox(new Box(num4, num5, num4 + 3, num5 + 3));
				if (!array[n, num3])
				{
					if (!maze.Cell[n, num3].N)
					{
						Z.GetCell(num4, num5).AddObject(blueprint);
						Z.GetCell(num4 + 1, num5).AddObject(blueprint);
						Z.GetCell(num4 + 2, num5).AddObject(blueprint);
						Z.GetCell(num4 + 3, num5).AddObject(blueprint);
					}
					if (!maze.Cell[n, num3].S)
					{
						Z.GetCell(num4, num5 + 3).AddObject(blueprint);
						Z.GetCell(num4 + 1, num5 + 3).AddObject(blueprint);
						Z.GetCell(num4 + 2, num5 + 3).AddObject(blueprint);
						Z.GetCell(num4 + 3, num5 + 3).AddObject(blueprint);
					}
					if (!maze.Cell[n, num3].E)
					{
						Z.GetCell(num4 + 3, num5).AddObject(blueprint);
						Z.GetCell(num4 + 3, num5 + 1).AddObject(blueprint);
						Z.GetCell(num4 + 3, num5 + 2).AddObject(blueprint);
						Z.GetCell(num4 + 3, num5 + 3).AddObject(blueprint);
					}
					if (!maze.Cell[n, num3].W)
					{
						Z.GetCell(num4, num5).AddObject(blueprint);
						Z.GetCell(num4, num5 + 1).AddObject(blueprint);
						Z.GetCell(num4, num5 + 2).AddObject(blueprint);
						Z.GetCell(num4, num5 + 3).AddObject(blueprint);
					}
					if (maze.Cell[n, num3].N && maze.Cell[n, num3].W)
					{
						Z.GetCell(num4, num5).AddObject(blueprint);
					}
					if (maze.Cell[n, num3].N && maze.Cell[n, num3].E)
					{
						Z.GetCell(num4 + 3, num5).AddObject(blueprint);
					}
					if (maze.Cell[n, num3].S && maze.Cell[n, num3].W)
					{
						Z.GetCell(num4, num5 + 3).AddObject(blueprint);
					}
					if (maze.Cell[n, num3].S && maze.Cell[n, num3].E)
					{
						Z.GetCell(num4 + 3, num5 + 3).AddObject(blueprint);
					}
				}
			}
		}
		for (int num6 = 0; num6 < 20; num6++)
		{
			for (int num7 = 0; num7 < 6; num7++)
			{
				int num8 = num6 * 4;
				int num9 = num7 * 4;
				if (!array[num6, num7])
				{
					if (maze.Cell[num6, num7].N && num7 > 0 && array[num6, num7 - 1])
					{
						Z.GetCell(num8 + 1, num9).AddObject("Door");
						Z.GetCell(num8 + 2, num9).AddObject("Door");
					}
					if (maze.Cell[num6, num7].S && num7 < 5 && array[num6, num7 + 1])
					{
						Z.GetCell(num8 + 1, num9 + 3).AddObject("Door");
						Z.GetCell(num8 + 2, num9 + 3).AddObject("Door");
					}
					if (maze.Cell[num6, num7].E && num6 < 19 && array[num6 + 1, num7])
					{
						Z.GetCell(num8 + 3, num9 + 1).AddObject("Door");
						Z.GetCell(num8 + 3, num9 + 2).AddObject("Door");
					}
					if (maze.Cell[num6, num7].W && num6 > 0 && array[num6 - 1, num7])
					{
						Z.GetCell(num8, num9 + 1).AddObject("Door");
						Z.GetCell(num8, num9 + 2).AddObject("Door");
					}
					if (maze.Cell[num6, num7].DeadEnd() && num7 > 0 && array[num6, num7 - 1])
					{
						Z.GetCell(num8 + 1, num9).ClearAndAddObject("Door");
						Z.GetCell(num8 + 2, num9).ClearAndAddObject("Door");
					}
					if (maze.Cell[num6, num7].DeadEnd() && num7 < 5 && array[num6, num7 + 1])
					{
						Z.GetCell(num8 + 1, num9 + 3).ClearAndAddObject("Door");
						Z.GetCell(num8 + 2, num9 + 3).ClearAndAddObject("Door");
					}
					if (maze.Cell[num6, num7].DeadEnd() && num6 > 0 && array[num6 - 1, num7])
					{
						Z.GetCell(num8, num9 + 1).ClearAndAddObject("Door");
						Z.GetCell(num8, num9 + 2).ClearAndAddObject("Door");
					}
					if (maze.Cell[num6, num7].DeadEnd() && num6 < 19 && array[num6 + 1, num7])
					{
						Z.GetCell(num8 + 3, num9 + 1).ClearAndAddObject("Door");
						Z.GetCell(num8 + 3, num9 + 2).ClearAndAddObject("Door");
					}
				}
			}
		}
		Z.FillHollowBox(new Box(0, 0, Z.Width - 1, Z.Height - 1), blueprint);
		Z.ClearReachableMap();
		for (int num10 = 5; num10 < 75; num10++)
		{
			int num11 = 5;
			while (num11 < 15)
			{
				if (Z.GetCell(num10, num11).IsSolid())
				{
					num11++;
					continue;
				}
				goto IL_0983;
			}
			continue;
			IL_0983:
			Z.BuildReachableMap(num10, num11);
			break;
		}
		return true;
	}
}
