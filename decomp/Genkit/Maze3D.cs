using System;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World;

namespace Genkit;

[Serializable]
public class Maze3D
{
	public int Width;

	public int Height;

	public int Depth;

	public MazeCell3D[,,] Cell;

	public Maze3D()
	{
	}

	public Maze3D(int Width, int Height, int Depth)
	{
		Init(Width, Height, Depth, Value: false);
	}

	public Maze3D(int Width, int Height, int Depth, bool InitialValue)
	{
		Init(Width, Height, Depth, InitialValue);
	}

	public void Init(int _Width, int _Height, int Depth)
	{
		Init(_Width, _Height, Depth, Value: false);
	}

	public static Maze3D Load(SerializationReader Reader)
	{
		Maze3D maze3D = new Maze3D();
		maze3D.Width = Reader.ReadInt32();
		maze3D.Height = Reader.ReadInt32();
		maze3D.Depth = Reader.ReadInt32();
		maze3D.Cell = new MazeCell3D[maze3D.Width, maze3D.Height, maze3D.Depth];
		for (short num = 0; num < maze3D.Width; num++)
		{
			for (short num2 = 0; num2 < maze3D.Height; num2++)
			{
				for (short num3 = 0; num3 < maze3D.Depth; num3++)
				{
					maze3D.Cell[num, num2, num3].x = num;
					maze3D.Cell[num, num2, num3].y = num2;
					maze3D.Cell[num, num2, num3].z = num3;
					maze3D.Cell[num, num2, num3].N = Reader.ReadBoolean();
					maze3D.Cell[num, num2, num3].S = Reader.ReadBoolean();
					maze3D.Cell[num, num2, num3].E = Reader.ReadBoolean();
					maze3D.Cell[num, num2, num3].W = Reader.ReadBoolean();
					maze3D.Cell[num, num2, num3].U = Reader.ReadBoolean();
					maze3D.Cell[num, num2, num3].D = Reader.ReadBoolean();
				}
			}
		}
		return maze3D;
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(Width);
		Writer.Write(Height);
		Writer.Write(Depth);
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				for (int k = 0; k < Depth; k++)
				{
					Writer.Write(Cell[i, j, k].N);
					Writer.Write(Cell[i, j, k].S);
					Writer.Write(Cell[i, j, k].E);
					Writer.Write(Cell[i, j, k].W);
					Writer.Write(Cell[i, j, k].U);
					Writer.Write(Cell[i, j, k].D);
				}
			}
		}
	}

	public void Init(int _Width, int _Height, int _Depth, bool Value)
	{
		Width = _Width;
		Height = _Height;
		Depth = _Depth;
		Cell = new MazeCell3D[Width, Height, Depth];
		for (short num = 0; num < Width; num++)
		{
			for (short num2 = 0; num2 < Height; num2++)
			{
				for (short num3 = 0; num3 < Depth; num3++)
				{
					Cell[num, num2, num3] = new MazeCell3D(Value);
					Cell[num, num2, num3].x = num;
					Cell[num, num2, num3].y = num2;
					Cell[num, num2, num3].z = num3;
				}
			}
		}
	}

	public void SetBorder(bool Value)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				for (int k = 0; k < Depth; k++)
				{
					if (i == 0 || i == Width - 1 || j == 0 || j == Height - 1 || k == 0 || k == Depth - 1)
					{
						Cell[i, j, k].N = Value;
					}
				}
			}
		}
	}

	public void Test(bool bWait)
	{
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		TextConsole textConsole = Popup._TextConsole;
		for (int i = 0; i < Depth; i++)
		{
			for (int j = 0; j < 80 && j < Width; j++)
			{
				for (int k = 0; k < 25 && k < Height; k++)
				{
					MazeCell3D mazeCell3D = Cell[j, k, i];
					scrapBuffer.Goto(j, k);
					if (mazeCell3D.U)
					{
						scrapBuffer.Write(">");
						continue;
					}
					if (mazeCell3D.D)
					{
						scrapBuffer.Write("<");
						continue;
					}
					if (!mazeCell3D.N && !mazeCell3D.S && !mazeCell3D.E && !mazeCell3D.W)
					{
						scrapBuffer.Write(46);
					}
					if (!mazeCell3D.N && !mazeCell3D.S && !mazeCell3D.E && mazeCell3D.W)
					{
						scrapBuffer.Write(182);
					}
					if (!mazeCell3D.N && !mazeCell3D.S && mazeCell3D.E && !mazeCell3D.W)
					{
						scrapBuffer.Write(199);
					}
					if (!mazeCell3D.N && !mazeCell3D.S && mazeCell3D.E && mazeCell3D.W)
					{
						scrapBuffer.Write(196);
					}
					if (!mazeCell3D.N && mazeCell3D.S && !mazeCell3D.E && !mazeCell3D.W)
					{
						scrapBuffer.Write(209);
					}
					if (!mazeCell3D.N && mazeCell3D.S && !mazeCell3D.E && mazeCell3D.W)
					{
						scrapBuffer.Write(191);
					}
					if (!mazeCell3D.N && mazeCell3D.S && mazeCell3D.E && !mazeCell3D.W)
					{
						scrapBuffer.Write(218);
					}
					if (!mazeCell3D.N && mazeCell3D.S && mazeCell3D.E && mazeCell3D.W)
					{
						scrapBuffer.Write(194);
					}
					if (mazeCell3D.N && !mazeCell3D.S && !mazeCell3D.E && !mazeCell3D.W)
					{
						scrapBuffer.Write(207);
					}
					if (mazeCell3D.N && !mazeCell3D.S && !mazeCell3D.E && mazeCell3D.W)
					{
						scrapBuffer.Write(217);
					}
					if (mazeCell3D.N && !mazeCell3D.S && mazeCell3D.E && !mazeCell3D.W)
					{
						scrapBuffer.Write(192);
					}
					if (mazeCell3D.N && !mazeCell3D.S && mazeCell3D.E && mazeCell3D.W)
					{
						scrapBuffer.Write(193);
					}
					if (mazeCell3D.N && mazeCell3D.S && !mazeCell3D.E && !mazeCell3D.W)
					{
						scrapBuffer.Write(179);
					}
					if (mazeCell3D.N && mazeCell3D.S && !mazeCell3D.E && mazeCell3D.W)
					{
						scrapBuffer.Write(180);
					}
					if (mazeCell3D.N && mazeCell3D.S && mazeCell3D.E && !mazeCell3D.W)
					{
						scrapBuffer.Write(195);
					}
					if (mazeCell3D.N && mazeCell3D.S && mazeCell3D.E && mazeCell3D.W)
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
}
