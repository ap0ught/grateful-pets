using Genkit;
using XRL.Core;

namespace XRL.World.ZoneBuilders;

public class Hive
{
	public bool BuildZone(Zone Z)
	{
		Maze maze = RecursiveBacktrackerMaze.Generate(40, 13, bShow: false, XRLCore.Core.Game.GetWorldSeed("BathHallsMaze" + Z.ZoneID));
		Z.Fill("Limestone");
		for (int i = 0; i < 40; i++)
		{
			for (int j = 0; j < 12; j++)
			{
				if (maze.Cell[i, j].AnyOpen())
				{
					Z.GetCell(i * 2, j * 2).Clear();
				}
				if (maze.Cell[i, j].S)
				{
					Z.GetCell(i * 2, j * 2 + 1).Clear();
				}
				if (maze.Cell[i, j].E)
				{
					Z.GetCell(i * 2 + 1, j * 2).Clear();
				}
			}
		}
		foreach (Box item in Tools.GenerateBoxes(BoxGenerateOverlap.NeverOverlap, new Range(8, 12), new Range(6, 16), new Range(4, 10), new Range(9, 100)))
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
