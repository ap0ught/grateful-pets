using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class CarbonFarm
{
	public bool BuildZone(Zone Z)
	{
		Maze maze = RecursiveBacktrackerMaze.Generate(10, 3, bShow: false, "CarbonFarm");
		for (int i = 0; i < 10; i += 2)
		{
			for (int j = 0; j < 3; j++)
			{
				maze.Cell[i, j].N = true;
				maze.Cell[i, j].S = true;
			}
		}
		for (int k = 0; k < 15; k++)
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
		Z.Fill("Limestone");
		bool[,] array = new bool[10, 3];
		for (int l = 0; l < 10; l++)
		{
			for (int m = 0; m < 3; m++)
			{
				array[l, m] = false;
			}
		}
		int num3 = Stat.Random(1, 3);
		int num4 = 0;
		while (num4 < num3)
		{
			int num5 = Stat.Random(0, 9);
			int num6 = Stat.Random(0, 2);
			if (!array[num5, num6])
			{
				array[num5, num6] = true;
				num4++;
			}
		}
		for (int n = 0; n < 10; n++)
		{
			for (int num7 = 0; num7 < 3; num7++)
			{
				int num8 = n * 8;
				int num9 = num7 * 8;
				if (array[n, num7])
				{
					Z.ClearBox(new Box(num8, num9, num8 + 7, num9 + 7));
					continue;
				}
				if (maze.Cell[n, num7].N)
				{
					Z.ClearBox(new Box(num8 + 1, num9, num8 + 6, num9 + 6));
				}
				if (maze.Cell[n, num7].S)
				{
					Z.ClearBox(new Box(num8 + 1, num9 + 1, num8 + 6, num9 + 7));
				}
				if (maze.Cell[n, num7].E)
				{
					Z.ClearBox(new Box(num8 + 1, num9 + 1, num8 + 7, num9 + 6));
				}
				if (maze.Cell[n, num7].W)
				{
					Z.ClearBox(new Box(num8, num9 + 1, num8 + 6, num9 + 6));
				}
			}
		}
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
				goto IL_02cb;
			}
			continue;
			IL_02cb:
			Z.BuildReachableMap(num10, num11);
			break;
		}
		return true;
	}
}
