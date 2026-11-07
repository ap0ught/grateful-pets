using System.Collections.Generic;
using Genkit;

namespace XRL.World.ZoneBuilders;

public class RuinedWharf
{
	public bool BuildZone(Zone Z)
	{
		Maze maze = RecursiveBacktrackerMaze.Generate(20, 6, bShow: false, "WharfMaze" + Z.ZoneID);
		Z.Fill("Limestone");
		for (int i = 0; i < 20; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				int num = i * 4 + 1;
				int num2 = j * 4 + 1;
				if (maze.Cell[i, j].AnyOpen())
				{
					Z.GetCell(num, num2).Clear();
					Z.GetCell(num + 1, num2).Clear();
					Z.GetCell(num, num2 + 1).Clear();
					Z.GetCell(num + 1, num2 + 1).Clear();
				}
				if (maze.Cell[i, j].S)
				{
					if (num2 + 2 < Z.Height)
					{
						Z.GetCell(num, num2 + 2).Clear();
						Z.GetCell(num + 1, num2 + 2).Clear();
					}
					if (num2 + 3 < Z.Height)
					{
						Z.GetCell(num, num2 + 3).Clear();
						Z.GetCell(num + 1, num2 + 3).Clear();
					}
				}
				if (maze.Cell[i, j].E)
				{
					Z.GetCell(num + 2, num2).Clear();
					Z.GetCell(num + 3, num2).Clear();
					Z.GetCell(num + 2, num2 + 1).Clear();
					Z.GetCell(num + 3, num2 + 1).Clear();
				}
			}
		}
		List<Box> list = Tools.GenerateBoxes(BoxGenerateOverlap.NeverOverlap, new Range(8, 16), new Range(6, 12), new Range(4, 10), new Range(9, 100));
		foreach (Box item in list)
		{
			Box box = item.Grow(-1);
			for (int k = box.x1; k <= box.x2; k++)
			{
				for (int l = box.y1; l <= box.y2; l++)
				{
					Z.GetCell(k, l).Clear();
				}
			}
		}
		for (int m = 0; m < 4; m++)
		{
			for (int n = 0; n < 2; n++)
			{
				for (int num3 = m * 12 + 4; num3 <= m * 8 + 12; num3++)
				{
					for (int num4 = n * 4 + 4; num4 <= n * 4 + 8; num4++)
					{
						Z.GetCell(num3, num4).Clear();
					}
				}
			}
		}
		Z.BuildReachableMap(list[0].x1 + 1, list[0].y1 + 1);
		return true;
	}
}
