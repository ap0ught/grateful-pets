using System;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World;

namespace Genkit;

[Serializable]
public class Maze : IComposite
{
	public int Width;

	public int Height;

	public MazeCell[,] Cell;

	public bool WantFieldReflection => false;

	public Maze()
	{
	}

	public Maze(int Width, int Height)
	{
		Init(Width, Height, Value: false);
	}

	public Maze(int Width, int Height, bool InitialValue)
	{
		Init(Width, Height, InitialValue);
	}

	public void Init(int _Width, int _Height)
	{
		Init(_Width, _Height, Value: false);
	}

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Width);
		Writer.WriteOptimized(Height);
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Writer.Write(Cell[i, j].N);
				Writer.Write(Cell[i, j].S);
				Writer.Write(Cell[i, j].E);
				Writer.Write(Cell[i, j].W);
			}
		}
	}

	public void Read(SerializationReader Reader)
	{
		Width = Reader.ReadOptimizedInt32();
		Height = Reader.ReadOptimizedInt32();
		Cell = new MazeCell[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Cell[i, j] = new MazeCell
				{
					x = i,
					y = j,
					N = Reader.ReadBoolean(),
					S = Reader.ReadBoolean(),
					E = Reader.ReadBoolean(),
					W = Reader.ReadBoolean()
				};
			}
		}
	}

	public void Init(int _Width, int _Height, bool Value)
	{
		Width = _Width;
		Height = _Height;
		Cell = new MazeCell[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Cell[i, j] = new MazeCell(Value);
				Cell[i, j].x = i;
				Cell[i, j].y = j;
			}
		}
	}

	public void SetBorder(bool Value)
	{
		for (int i = 0; i < Width; i++)
		{
			Cell[i, 0].N = Value;
			Cell[i, Height - 1].S = Value;
		}
		for (int j = 0; j < Height; j++)
		{
			Cell[0, j].W = Value;
			Cell[Width - 1, j].E = Value;
		}
	}

	public void Test(bool bWait)
	{
		ScreenBuffer scrapBuffer = TextConsole.GetScrapBuffer1();
		TextConsole textConsole = Popup._TextConsole;
		for (int i = 0; i < 80 && i < Width; i++)
		{
			for (int j = 0; j < 25 && j < Height; j++)
			{
				MazeCell mazeCell = Cell[i, j];
				scrapBuffer.Goto(i, j);
				if (!mazeCell.N && !mazeCell.S && !mazeCell.E && !mazeCell.W)
				{
					scrapBuffer.Write(46);
				}
				if (!mazeCell.N && !mazeCell.S && !mazeCell.E && mazeCell.W)
				{
					scrapBuffer.Write(182);
				}
				if (!mazeCell.N && !mazeCell.S && mazeCell.E && !mazeCell.W)
				{
					scrapBuffer.Write(199);
				}
				if (!mazeCell.N && !mazeCell.S && mazeCell.E && mazeCell.W)
				{
					scrapBuffer.Write(196);
				}
				if (!mazeCell.N && mazeCell.S && !mazeCell.E && !mazeCell.W)
				{
					scrapBuffer.Write(209);
				}
				if (!mazeCell.N && mazeCell.S && !mazeCell.E && mazeCell.W)
				{
					scrapBuffer.Write(191);
				}
				if (!mazeCell.N && mazeCell.S && mazeCell.E && !mazeCell.W)
				{
					scrapBuffer.Write(218);
				}
				if (!mazeCell.N && mazeCell.S && mazeCell.E && mazeCell.W)
				{
					scrapBuffer.Write(194);
				}
				if (mazeCell.N && !mazeCell.S && !mazeCell.E && !mazeCell.W)
				{
					scrapBuffer.Write(207);
				}
				if (mazeCell.N && !mazeCell.S && !mazeCell.E && mazeCell.W)
				{
					scrapBuffer.Write(217);
				}
				if (mazeCell.N && !mazeCell.S && mazeCell.E && !mazeCell.W)
				{
					scrapBuffer.Write(192);
				}
				if (mazeCell.N && !mazeCell.S && mazeCell.E && mazeCell.W)
				{
					scrapBuffer.Write(193);
				}
				if (mazeCell.N && mazeCell.S && !mazeCell.E && !mazeCell.W)
				{
					scrapBuffer.Write(179);
				}
				if (mazeCell.N && mazeCell.S && !mazeCell.E && mazeCell.W)
				{
					scrapBuffer.Write(180);
				}
				if (mazeCell.N && mazeCell.S && mazeCell.E && !mazeCell.W)
				{
					scrapBuffer.Write(195);
				}
				if (mazeCell.N && mazeCell.S && mazeCell.E && mazeCell.W)
				{
					scrapBuffer.Write(197);
				}
			}
		}
		textConsole.DrawBuffer(scrapBuffer);
		if (bWait)
		{
			Keyboard.getch();
		}
	}
}
