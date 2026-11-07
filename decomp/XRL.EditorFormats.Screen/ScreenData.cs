using System;
using System.Collections.Generic;

namespace XRL.EditorFormats.Screen;

[Serializable]
public class ScreenData
{
	public Cell[,] Cells;

	public int Rows;

	public int Columns;

	public List<string> Properties = new List<string>();

	public ScreenData(int Width, int Height)
	{
		Rows = Height;
		Columns = Width;
		ResizeCells();
	}

	public void Clear()
	{
		Cell[,] cells = Cells;
		foreach (Cell cell in cells)
		{
			cell.Char = ' ';
			cell.Background = 'k';
			cell.Foreground = 'k';
		}
	}

	public void ResizeCells()
	{
		Cell[,] cells = Cells;
		Cells = new Cell[Columns, Rows];
		if (cells != null)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Columns; j++)
				{
					if (j < cells.GetUpperBound(0) && i < cells.GetUpperBound(1))
					{
						Cells[j, i] = cells[j, i];
					}
					else
					{
						Cells[j, i] = new Cell();
					}
				}
			}
			return;
		}
		for (int k = 0; k < Rows; k++)
		{
			for (int l = 0; l < Columns; l++)
			{
				Cells[l, k] = new Cell();
			}
		}
	}
}
