using Genkit;

namespace XRL.World.ZoneBuilders;

public class TrembleEntrance
{
	public bool BuildZone(Zone Z)
	{
		Cell cell = ((Z.GetEmptyReachableCells(new Rect2D(5, 5, 75, 15)).Shuffle().Count != 0) ? Z.GetCell(40, 12) : Z.GetCells().GetRandomElement());
		int x = cell.X;
		int y = cell.Y;
		for (int num = y; num <= y - 4; num--)
		{
			for (int i = x - 2; i <= x + 2; i++)
			{
				Z.GetCell(i, num)?.Clear();
			}
		}
		for (int j = x - 2; j <= x + 2; j++)
		{
			Z.GetCell(j, y - 4).AddObject("Fulcrete");
		}
		Z.GetCell(x - 2, y - 3).AddObject("Fulcrete");
		Z.GetCell(x + 2, y - 3).AddObject("Fulcrete");
		Z.GetCell(x - 2, y - 2).AddObject("Fulcrete");
		Z.GetCell(x, y - 2).AddObject("TrembleEntranceWormhole");
		Z.GetCell(x + 2, y - 2).AddObject("Fulcrete");
		Z.GetCell(x - 2, y - 1).AddObject("Fulcrete");
		Z.GetCell(x - 1, y - 1).AddObject("DoorSwitch");
		Z.GetCell(x + 2, y - 1).AddObject("Fulcrete");
		Z.GetCell(x - 2, y).AddObject("Fulcrete");
		Z.GetCell(x - 1, y).AddObject("Fulcrete");
		Z.GetCell(x, y).AddObject("Purple Security Door");
		Z.GetCell(x + 1, y).AddObject("Fulcrete");
		Z.GetCell(x + 2, y).AddObject("Fulcrete");
		return true;
	}
}
