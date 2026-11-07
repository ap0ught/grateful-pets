using Genkit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class Cryobarrio
{
	public bool BuildZone(Zone Z)
	{
		Maze maze = RecursiveBacktrackerMaze.Generate(10, 3, bShow: false, The.Game.GetWorldSeed("CryobarriorMaze" + Z.ZoneID));
		for (int i = 0; i < 15; i++)
		{
			int num = Stat.Random(1, 9);
			int num2 = Stat.Random(0, 2);
			if (25.in100())
			{
				maze.Cell[num, num2].N = true;
			}
			if (25.in100())
			{
				maze.Cell[num, num2].S = true;
			}
			if (25.in100())
			{
				maze.Cell[num, num2].E = true;
			}
			if (25.in100())
			{
				maze.Cell[num, num2].W = true;
			}
		}
		Z.Fill("Limestone");
		bool[,] array = new bool[10, 3];
		for (int j = 0; j < 10; j++)
		{
			for (int k = 0; k < 3; k++)
			{
				array[j, k] = false;
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
					Z.GetCell(num7 + 2, num8 + 3).AddObject("VGlassWall");
					Z.GetCell(num7 + 2, num8 + 4).AddObject("VGlassWall");
					Z.GetCell(num7 + 5, num8 + 3).AddObject("VGlassWall");
					Z.GetCell(num7 + 5, num8 + 4).AddObject("VGlassWall");
					Z.GetCell(num7 + 3, num8 + 2).AddObject("HGlassWall");
					Z.GetCell(num7 + 4, num8 + 2).AddObject("HGlassWall");
					Z.GetCell(num7 + 3, num8 + 5).AddObject("HGlassWall");
					Z.GetCell(num7 + 4, num8 + 5).AddObject("HGlassWall");
					Z.GetCell(num7 + 2, num8 + 2).AddObject("CryochamberWallSE");
					Z.GetCell(num7 + 2, num8 + 5).AddObject("CryochamberWallNE");
					Z.GetCell(num7 + 5, num8 + 2).AddObject("CryochamberWallSW");
					Z.GetCell(num7 + 5, num8 + 5).AddObject("CryochamberWallNW");
					Z.GetCell(num7 + 3, num8 + 3).AddObject("CryoGas");
					Z.GetCell(num7 + 3, num8 + 4).AddObject("CryoGas");
					Z.GetCell(num7 + 4, num8 + 3).AddObject("CryoGas");
					Z.GetCell(num7 + 4, num8 + 4).AddObject("CryoGas");
					Z.GetCell(num7 + 3, num8 + 3).AddObject("Equimax");
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
				goto IL_0563;
			}
			continue;
			IL_0563:
			Z.BuildReachableMap(n, num9);
			break;
		}
		return true;
	}
}
