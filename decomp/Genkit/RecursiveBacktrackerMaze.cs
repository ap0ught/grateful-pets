using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace Genkit;

public class RecursiveBacktrackerMaze
{
	public static Stack<MazeCell> CellStack = new Stack<MazeCell>();

	public static Dictionary<MazeCell, bool> VisitedCells = new Dictionary<MazeCell, bool>();

	public static Maze M;

	public static bool HasUnvisitedNeighbors(MazeCell C)
	{
		if (C.x > 0 && !VisitedCells.ContainsKey(M.Cell[C.x - 1, C.y]))
		{
			return true;
		}
		if (C.x < M.Width - 1 && !VisitedCells.ContainsKey(M.Cell[C.x + 1, C.y]))
		{
			return true;
		}
		if (C.y > 0 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y - 1]))
		{
			return true;
		}
		if (C.y < M.Height - 1 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y + 1]))
		{
			return true;
		}
		return false;
	}

	public static MazeCell GetRandomUnvisitedNeighbor(MazeCell C, Random random)
	{
		List<MazeCell> list = new List<MazeCell>();
		if (C.x > 0 && !VisitedCells.ContainsKey(M.Cell[C.x - 1, C.y]))
		{
			list.Add(M.Cell[C.x - 1, C.y]);
		}
		if (C.x < M.Width - 1 && !VisitedCells.ContainsKey(M.Cell[C.x + 1, C.y]))
		{
			list.Add(M.Cell[C.x + 1, C.y]);
		}
		if (C.y > 0 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y - 1]))
		{
			list.Add(M.Cell[C.x, C.y - 1]);
		}
		if (C.y < M.Height - 1 && !VisitedCells.ContainsKey(M.Cell[C.x, C.y + 1]))
		{
			list.Add(M.Cell[C.x, C.y + 1]);
		}
		return list[random.Next(0, list.Count)];
	}

	public static void MarkOpeningBetweenCells(MazeCell C1, MazeCell C2)
	{
		if (C1.x == C2.x)
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

	public static Maze GenerateWithHoles(int Width, int Height, bool bShow, int nHoles, bool bDefault, int seed)
	{
		Random random = new Random(seed);
		CellStack.Clear();
		VisitedCells.Clear();
		M = new Maze(Width, Height, bDefault);
		for (int i = 0; i < nHoles; i++)
		{
			int num = random.Next(0, Width);
			int num2 = random.Next(0, Height);
			if (!VisitedCells.ContainsKey(M.Cell[num, num2]))
			{
				VisitedCells.Add(M.Cell[num, num2], value: true);
			}
		}
		int num3 = random.Next(0, Width);
		int num4 = random.Next(0, Height);
		while (VisitedCells.ContainsKey(M.Cell[num3, num4]))
		{
			num3 = random.Next(0, Width);
			num4 = random.Next(0, Height);
		}
		CellStack.Push(M.Cell[num3, num4]);
		MazeCell mazeCell = M.Cell[num3, num4];
		while (true)
		{
			if (!VisitedCells.ContainsKey(mazeCell))
			{
				VisitedCells.Add(mazeCell, value: true);
			}
			if (HasUnvisitedNeighbors(mazeCell))
			{
				CellStack.Push(mazeCell);
				MazeCell randomUnvisitedNeighbor = GetRandomUnvisitedNeighbor(mazeCell, random);
				MarkOpeningBetweenCells(randomUnvisitedNeighbor, mazeCell);
				CellStack.Push(mazeCell);
				if (bShow)
				{
					M.Test(bWait: false);
				}
				mazeCell = randomUnvisitedNeighbor;
			}
			else
			{
				if (CellStack.Count == 0)
				{
					break;
				}
				mazeCell = CellStack.Pop();
			}
		}
		CellStack.Clear();
		VisitedCells.Clear();
		return M;
	}

	public static Maze Generate(int Width, int Height, bool bShow, string Seed)
	{
		return Generate(Width, Height, bShow, XRLCore.Core.Game.GetWorldSeed(Seed));
	}

	public static Maze Generate(int Width, int Height, bool bShow, int Seed)
	{
		Random random = new Random(Seed);
		M = new Maze(Width, Height, InitialValue: false);
		int num = Stat.Random(0, Width - 1);
		int num2 = Stat.Random(0, Height - 1);
		CellStack.Clear();
		VisitedCells.Clear();
		CellStack.Push(M.Cell[num, num2]);
		MazeCell mazeCell = M.Cell[num, num2];
		try
		{
			while (true)
			{
				if (!VisitedCells.ContainsKey(mazeCell))
				{
					VisitedCells.Add(mazeCell, value: true);
				}
				if (HasUnvisitedNeighbors(mazeCell))
				{
					CellStack.Push(mazeCell);
					MazeCell randomUnvisitedNeighbor = GetRandomUnvisitedNeighbor(mazeCell, random);
					MarkOpeningBetweenCells(randomUnvisitedNeighbor, mazeCell);
					CellStack.Push(mazeCell);
					if (bShow)
					{
						M.Test(bWait: false);
					}
					mazeCell = randomUnvisitedNeighbor;
				}
				else
				{
					if (CellStack.Count == 0)
					{
						break;
					}
					mazeCell = CellStack.Pop();
				}
			}
			return M;
		}
		finally
		{
			CellStack.Clear();
			VisitedCells.Clear();
		}
	}
}
