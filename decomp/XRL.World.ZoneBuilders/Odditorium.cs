using Genkit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Odditorium
{
	public bool BuildZone(Zone Z)
	{
		Maze maze = RecursiveBacktrackerMaze.GenerateWithHoles(20, 6, bShow: false, 30, bDefault: true, XRLCore.Core.Game.GetWorldSeed("OdditorimMaze" + Z.ZoneID));
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
				array[j, k] = false;
			}
		}
		int num3 = Stat.Random(3, 6);
		int num4 = 0;
		while (num4 < num3)
		{
			int num5 = Stat.Random(0, 19);
			int num6 = Stat.Random(0, 5);
			if (!array[num5, num6])
			{
				array[num5, num6] = true;
				num4++;
			}
		}
		for (int l = 0; l < 20; l++)
		{
			for (int m = 0; m < 6; m++)
			{
				int num7 = l * 4;
				int num8 = m * 4;
				Z.ClearBox(new Box(num7, num8, num7 + 3, num8 + 3));
				if (maze.Cell[l, m].AnyOpen())
				{
					if (!maze.Cell[l, m].N)
					{
						Z.GetCell(num7, num8).AddObject(blueprint);
						Z.GetCell(num7 + 1, num8).AddObject(blueprint);
						Z.GetCell(num7 + 2, num8).AddObject(blueprint);
						Z.GetCell(num7 + 3, num8).AddObject(blueprint);
					}
					if (!maze.Cell[l, m].S)
					{
						Z.GetCell(num7, num8 + 3).AddObject(blueprint);
						Z.GetCell(num7 + 1, num8 + 3).AddObject(blueprint);
						Z.GetCell(num7 + 2, num8 + 3).AddObject(blueprint);
						Z.GetCell(num7 + 3, num8 + 3).AddObject(blueprint);
					}
					if (!maze.Cell[l, m].E)
					{
						Z.GetCell(num7 + 3, num8).AddObject(blueprint);
						Z.GetCell(num7 + 3, num8 + 1).AddObject(blueprint);
						Z.GetCell(num7 + 3, num8 + 2).AddObject(blueprint);
						Z.GetCell(num7 + 3, num8 + 3).AddObject(blueprint);
					}
					if (!maze.Cell[l, m].W)
					{
						Z.GetCell(num7, num8).AddObject(blueprint);
						Z.GetCell(num7, num8 + 1).AddObject(blueprint);
						Z.GetCell(num7, num8 + 2).AddObject(blueprint);
						Z.GetCell(num7, num8 + 3).AddObject(blueprint);
					}
					if (maze.Cell[l, m].N && maze.Cell[l, m].W)
					{
						Z.GetCell(num7, num8).AddObject(blueprint);
					}
					if (maze.Cell[l, m].N && maze.Cell[l, m].E)
					{
						Z.GetCell(num7 + 3, num8).AddObject(blueprint);
					}
					if (maze.Cell[l, m].S && maze.Cell[l, m].W)
					{
						Z.GetCell(num7, num8 + 3).AddObject(blueprint);
					}
					if (maze.Cell[l, m].S && maze.Cell[l, m].E)
					{
						Z.GetCell(num7 + 3, num8 + 3).AddObject(blueprint);
					}
				}
			}
		}
		Z.ClearReachableMap();
		for (int n = 5; n < 75; n++)
		{
			int num9 = 5;
			while (num9 < 15)
			{
				if (Z.GetCell(n, num9).IsSolid())
				{
					num9++;
					continue;
				}
				goto IL_04c5;
			}
			continue;
			IL_04c5:
			Z.BuildReachableMap(n, num9);
			break;
		}
		return true;
	}
}
