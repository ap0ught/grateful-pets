using Genkit;
using XRL.Core;

namespace XRL.World.ZoneBuilders;

public class WideHive
{
	public bool BuildZone(Zone Z)
	{
		Maze maze = RecursiveBacktrackerMaze.Generate(20, 6, bShow: false, XRLCore.Core.Game.GetWorldSeed("BathHallsMaze"));
		Z.Fill("Limestone");
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 20; j++)
			{
				int num = j * 4 + 1;
				int num2 = i * 4 + 1;
				if (maze.Cell[j, i].AnyOpen())
				{
					Z.GetCell(num, num2).Clear();
					Z.GetCell(num + 1, num2).Clear();
					Z.GetCell(num, num2 + 1).Clear();
					Z.GetCell(num + 1, num2 + 1).Clear();
				}
				if (maze.Cell[j, i].S)
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
				if (maze.Cell[j, i].E)
				{
					Z.GetCell(num + 2, num2).Clear();
					Z.GetCell(num + 3, num2).Clear();
					Z.GetCell(num + 2, num2 + 1).Clear();
					Z.GetCell(num + 3, num2 + 1).Clear();
				}
			}
		}
		foreach (Box item in Tools.GenerateBoxes(BoxGenerateOverlap.NeverOverlap, new Range(8, 16), new Range(6, 12), new Range(4, 10), new Range(9, 100)))
		{
			Box box = item.Grow(-1);
			for (int k = box.y1; k <= box.y2; k++)
			{
				for (int l = box.x1; l <= box.x2; l++)
				{
					Z.GetCell(l, k).Clear();
				}
			}
		}
		return true;
	}
}
