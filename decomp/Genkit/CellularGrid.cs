using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.UI;

namespace Genkit;

public class CellularGrid
{
	public Random R;

	public bool SeedBorders = true;

	public int BorderDepth = 1;

	public int Width;

	public int Height;

	public int SeedChance = 55;

	public int Passes = 2;

	public int[] BornList = new int[3] { 6, 7, 8 };

	public int[] SurviveList = new int[4] { 5, 6, 7, 8 };

	public int[,] cells;

	public CellularGrid()
	{
	}

	public void Draw()
	{
		ScreenBuffer scrapBuffer = TextConsole.GetScrapBuffer1();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				scrapBuffer.Goto(i, j);
				if (cells[i, j] <= 0)
				{
					scrapBuffer.Write("&K.");
				}
				else
				{
					scrapBuffer.Write("&Y#");
				}
			}
		}
		Popup._TextConsole.DrawBuffer(scrapBuffer);
		Keyboard.getch();
	}

	public void ApplyCAPass(int numIterations)
	{
		int[,] array = new int[Width, Height];
		int num = 1;
		List<int> list = new List<int>(BornList);
		List<int> list2 = new List<int>(SurviveList);
		while (numIterations > 0)
		{
			numIterations--;
			for (int i = 0; i < Width; i += num)
			{
				for (int j = 0; j < Height; j += num)
				{
					bool flag = false;
					if (i == 0 || i >= Width - num || j == 0 || j >= Height - num)
					{
						flag = SeedBorders;
					}
					else
					{
						int num2 = 0;
						num2 += cells[i - num, j - num];
						num2 += cells[i - num, j];
						num2 += cells[i - num, j + num];
						num2 += cells[i, j - num];
						num2 += cells[i, j + num];
						num2 += cells[i + num, j - num];
						num2 += cells[i + num, j];
						num2 += cells[i + num, j + num];
						int num3 = cells[i, j];
						flag = (num3 == 0 && list.Contains(num2)) || (num3 == 1 && list2.Contains(num2));
					}
					for (int k = 0; k < num; k++)
					{
						for (int l = 0; l < num; l++)
						{
							array[i + k, j + l] = (flag ? 1 : 0);
						}
					}
				}
			}
		}
		for (int m = 0; m < Width; m++)
		{
			for (int n = 0; n < Height; n++)
			{
				cells[m, n] = array[m, n];
			}
		}
	}

	public CellularGrid(Random R, int Seed, int Width, int Height)
	{
		Generate(R, Seed, Width, Height);
	}

	public CellularGrid(Random R, string Seed, int Width, int Height)
	{
		Generate(R, Hash.String(Seed), Width, Height);
	}

	public void Generate(Random R, int Width, int Height)
	{
		Generate(R, R.Next(), Width, Height);
	}

	public void Generate(Random R, string Seed, int Width, int Height)
	{
		Generate(R, Hash.String(Seed), Width, Height);
	}

	public void Generate(Random R, int Seed, int _Width, int _Height)
	{
		Width = _Width;
		Height = _Height;
		cells = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (SeedBorders && (i <= BorderDepth - 1 || j <= BorderDepth - 1 || i >= Width - BorderDepth || j >= Height - BorderDepth))
				{
					cells[i, j] = 1;
				}
				else if (R.Next(1, 100) <= SeedChance)
				{
					cells[i, j] = 1;
				}
				else
				{
					cells[i, j] = 0;
				}
			}
		}
		if (Options.GetOption("OptionDrawCASystems", "No") == "Yes")
		{
			Draw();
		}
		for (int k = 0; k < Passes; k++)
		{
			ApplyCAPass(1);
			if (Options.GetOption("OptionDrawCASystems", "No") == "Yes")
			{
				Draw();
			}
		}
	}
}
