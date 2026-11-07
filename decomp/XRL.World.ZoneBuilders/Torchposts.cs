using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.ZoneBuilders;

public class Torchposts
{
	public bool BuildZone(Zone Z)
	{
		int x = Stat.Random(1, 78);
		int y = Stat.Random(0, 1) * 23 + 1;
		for (int i = 0; i < 80; i++)
		{
			int num = 0;
			while (num < 25)
			{
				Cell cell = Z.GetCell(i, num);
				if (!cell.HasObjectWithPart("StairsUp") && !cell.HasObjectWithPart("StairsDown"))
				{
					num++;
					continue;
				}
				goto IL_0048;
			}
			continue;
			IL_0048:
			x = i;
			y = num;
			break;
		}
		FindPath findPath = new FindPath(Z, x, y, Z, Stat.Random(0, 1) * 77 + 1, Stat.Random(1, 23), PathGlobal: false, PathUnlimited: true, null, AddNoise: true);
		if (!findPath.Usable)
		{
			return true;
		}
		int num2 = 0;
		foreach (Cell step in findPath.Steps)
		{
			num2--;
			if (num2 <= 0)
			{
				List<Cell> localAdjacentCells = step.GetLocalAdjacentCells();
				if (localAdjacentCells.Count >= 7)
				{
					localAdjacentCells[2].AddObject(GameObjectFactory.Factory.CreateObject("Torchpost"));
					localAdjacentCells[6].AddObject(GameObjectFactory.Factory.CreateObject("Torchpost"));
				}
				num2 += Stat.Random(3, 5);
			}
		}
		return true;
	}
}
