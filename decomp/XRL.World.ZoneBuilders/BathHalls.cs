using Genkit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class BathHalls
{
	public bool BuildZone(Zone Z)
	{
		Maze maze = RecursiveBacktrackerMaze.Generate(11, 3, bShow: true, XRLCore.Core.Game.GetWorldSeed("BathHallsMaze" + Z.ZoneID));
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
		Z.Fill("Limestone");
		bool[,] array = new bool[10, 3];
		for (int j = 0; j < 3; j++)
		{
			for (int k = 0; k < 10; k++)
			{
				array[k, j] = false;
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
		for (int l = 0; l < 10; l++)
		{
			for (int m = 0; m < 3; m++)
			{
				int num7 = l * 8;
				int num8 = m * 8;
				if (array[l, m])
				{
					Z.ClearBox(new Box(num7, num8, num7 + 7, num8 + 7));
					Z.FillBox(new Box(num7 + 1, num8 + 1, num7 + 6, num8 + 6), "ConvalessenceDeepPool");
					Z.GetCell(num7 + 2, num8 + 2).AddObject("Fulcrete");
					Z.GetCell(num7 + 2, num8 + 5).AddObject("Fulcrete");
					Z.GetCell(num7 + 5, num8 + 2).AddObject("Fulcrete");
					Z.GetCell(num7 + 5, num8 + 5).AddObject("Fulcrete");
					continue;
				}
				if (maze.Cell[l, m].N)
				{
					Z.ClearBox(new Box(num7 + 1, num8, num7 + 6, num8 + 6));
					Z.GetCell(num7 + 2, num8 + 2).AddObject("Fulcrete");
					Z.GetCell(num7 + 5, num8 + 2).AddObject("Fulcrete");
				}
				if (maze.Cell[l, m].S)
				{
					Z.ClearBox(new Box(num7 + 1, num8 + 1, num7 + 6, num8 + 7));
					Z.GetCell(num7 + 2, num8 + 5).AddObject("Fulcrete");
					Z.GetCell(num7 + 5, num8 + 5).AddObject("Fulcrete");
				}
				if (maze.Cell[l, m].E)
				{
					Z.ClearBox(new Box(num7 + 1, num8 + 1, num7 + 7, num8 + 6));
					Z.GetCell(num7 + 2, num8 + 2).AddObject("Fulcrete");
					Z.GetCell(num7 + 2, num8 + 5).AddObject("Fulcrete");
				}
				if (maze.Cell[l, m].W)
				{
					Z.ClearBox(new Box(num7, num8 + 1, num7 + 6, num8 + 6));
					Z.GetCell(num7 + 5, num8 + 2).AddObject("Fulcrete");
					Z.GetCell(num7 + 5, num8 + 5).AddObject("Fulcrete");
				}
			}
		}
		for (int n = 0; n < 10; n++)
		{
			for (int num9 = 0; num9 < 3; num9++)
			{
				int num10 = n * 8;
				int num11 = num9 * 8;
				if (array[n, num9])
				{
					Z.FillBox(new Box(num10 + 1, num11 + 1, num10 + 6, num11 + 6), "ConvalessenceDeepPool");
					continue;
				}
				if (Stat.Random(1, 100) <= 50 && maze.Cell[n, num9].N)
				{
					Z.FillBox(new Box(num10 + 3, num11, num10 + 4, num11 + 6), "ConvalessenceDeepPool");
				}
				if (Stat.Random(1, 100) <= 50 && maze.Cell[n, num9].S)
				{
					Z.FillBox(new Box(num10 + 3, num11 + 1, num10 + 4, num11 + 7), "ConvalessenceDeepPool");
				}
				if (Stat.Random(1, 100) <= 50 && maze.Cell[n, num9].E)
				{
					Z.FillBox(new Box(num10 + 1, num11 + 3, num10 + 7, num11 + 4), "ConvalessenceDeepPool");
				}
				if (Stat.Random(1, 100) <= 50 && maze.Cell[n, num9].W)
				{
					Z.FillBox(new Box(num10, num11 + 3, num10 + 6, num11 + 4), "ConvalessenceDeepPool");
				}
			}
		}
		return true;
	}
}
