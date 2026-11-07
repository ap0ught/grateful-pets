using System;
using System.Collections.Generic;

namespace Genkit;

public class RecursiveBacktrackerMaze3D
{
	public static Stack<MazeCell3D> CellStack = new Stack<MazeCell3D>();

	public static Dictionary<MazeCell3D, bool> VisitedCells = new Dictionary<MazeCell3D, bool>();

	public static Maze3D M;

	private static Random R = null;

	public static bool HasUnvisitedNeighbors(MazeCell3D C)
	{
		if (C.x > 0 && !VisitedCells.ContainsKey(M.Cell[C.x - 1, C.y, C.z]))
		{
			return true;
		}
		if (C.x < M.Width - 1 && !VisitedCells.ContainsKey(M.Cell[C.x + 1, C.y, C.z]))
		{
			return true;
		}
		if (C.y > 0 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y - 1, C.z]))
		{
			return true;
		}
		if (C.y < M.Height - 1 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y + 1, C.z]))
		{
			return true;
		}
		if (C.z > 0 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y, C.z - 1]))
		{
			return true;
		}
		if (C.z < M.Depth - 1 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y, C.z + 1]))
		{
			return true;
		}
		return false;
	}

	public static MazeCell3D GetRandomUnvisitedNeighbor(MazeCell3D C)
	{
		List<MazeCell3D> list = new List<MazeCell3D>();
		if (C.x > 0 && !VisitedCells.ContainsKey(M.Cell[C.x - 1, C.y, C.z]))
		{
			list.Add(M.Cell[C.x - 1, C.y, C.z]);
		}
		if (C.x < M.Width - 1 && !VisitedCells.ContainsKey(M.Cell[C.x + 1, C.y, C.z]))
		{
			list.Add(M.Cell[C.x + 1, C.y, C.z]);
		}
		if (C.y > 0 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y - 1, C.z]))
		{
			list.Add(M.Cell[C.x, C.y - 1, C.z]);
		}
		if (C.y < M.Height - 1 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y + 1, C.z]))
		{
			list.Add(M.Cell[C.x, C.y + 1, C.z]);
		}
		if (C.z > 0 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y, C.z - 1]))
		{
			list.Add(M.Cell[C.x, C.y, C.z - 1]);
		}
		if (C.z < M.Depth - 1 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y, C.z + 1]))
		{
			list.Add(M.Cell[C.x, C.y, C.z + 1]);
		}
		return list[R.Next(0, list.Count - 1)];
	}

	public static void MarkOpeningBetweenCells(MazeCell3D C1, MazeCell3D C2)
	{
		if (C1.z != C2.z)
		{
			if (C1.z > C2.z)
			{
				C1.U = true;
				C2.D = true;
			}
			else
			{
				C1.D = true;
				C2.U = true;
			}
		}
		else if (C1.x == C2.x)
		{
			if (C1.y + 1 == C2.y)
			{
				C1.S = true;
				C2.N = true;
			}
			else if (C1.y - 1 == C2.y)
			{
				C1.N = true;
				C2.S = true;
			}
		}
		else if (C1.y == C2.y)
		{
			if (C1.x + 1 == C2.x)
			{
				C1.E = true;
				C2.W = true;
			}
			else if (C1.x - 1 == C2.x)
			{
				C1.W = true;
				C2.E = true;
			}
		}
	}

	public static Maze3D Generate(int Seed, int Width, int Height, int Depth, bool bShow, int AddUpChance = 0, int AddDownChance = 0, int AddNChance = 0, int AddSChance = 0, int AddEChance = 0, int AddWChance = 0)
	{
		R = new Random(Seed);
		M = new Maze3D(Width, Height, Depth, InitialValue: false);
		int num = R.Next(0, Width - 1);
		int num2 = R.Next(0, Height - 1);
		int num3 = R.Next(0, Depth - 1);
		CellStack.Clear();
		VisitedCells.Clear();
		CellStack.Push(M.Cell[num, num2, num3]);
		MazeCell3D mazeCell3D = M.Cell[num, num2, num3];
		while (true)
		{
			if (!VisitedCells.ContainsKey(mazeCell3D))
			{
				VisitedCells.Add(mazeCell3D, value: true);
			}
			if (HasUnvisitedNeighbors(mazeCell3D))
			{
				CellStack.Push(mazeCell3D);
				MazeCell3D randomUnvisitedNeighbor = GetRandomUnvisitedNeighbor(mazeCell3D);
				MarkOpeningBetweenCells(randomUnvisitedNeighbor, mazeCell3D);
				CellStack.Push(mazeCell3D);
				mazeCell3D = randomUnvisitedNeighbor;
			}
			else
			{
				if (CellStack.Count == 0)
				{
					break;
				}
				mazeCell3D = CellStack.Pop();
			}
		}
		if (bShow)
		{
			M.Test(bWait: true);
		}
		VisitedCells.Clear();
		CellStack.Clear();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				for (int k = 0; k < Depth; k++)
				{
					if (R.Next(1, 100) <= AddUpChance)
					{
						M.Cell[i, j, k].U = true;
						if (k > 0)
						{
							M.Cell[i, j, k - 1].D = true;
						}
					}
					if (R.Next(1, 100) <= AddDownChance)
					{
						M.Cell[i, j, k].D = true;
						if (k < Depth - 1)
						{
							M.Cell[i, j, k + 1].U = true;
						}
					}
					if (R.Next(1, 100) <= AddNChance)
					{
						M.Cell[i, j, k].N = true;
						if (j > 0)
						{
							M.Cell[i, j - 1, k].S = true;
						}
					}
					if (R.Next(1, 100) <= AddSChance)
					{
						M.Cell[i, j, k].S = true;
						if (j < Height - 1)
						{
							M.Cell[i, j + 1, k].N = true;
						}
					}
					if (R.Next(1, 100) <= AddEChance)
					{
						M.Cell[i, j, k].E = true;
						if (i < Width - 1)
						{
							M.Cell[i + 1, j, k].W = true;
						}
					}
					if (R.Next(1, 100) <= AddWChance)
					{
						M.Cell[i, j, k].W = true;
						if (i > 0)
						{
							M.Cell[i - 1, j, k].E = true;
						}
					}
				}
			}
		}
		CellStack.Clear();
		VisitedCells.Clear();
		return M;
	}
}
