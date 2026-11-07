using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class HamilcrabShop
{
	public int Width = 6;

	public int Height = 7;

	public int Padding;

	public List<Cell> GetContiguous(Zone Z, int X, int Y, int IX, int IY)
	{
		List<Cell> list = new List<Cell>();
		List<Cell> list2 = new List<Cell>();
		while (X < Z.Width && Y < Z.Height)
		{
			Cell cell = Z.GetCell(X, Y);
			if (cell.HasWall())
			{
				list.Add(cell);
			}
			else
			{
				if (list.Count > list2.Count)
				{
					list2.Clear();
					list2.AddRange(list);
				}
				list.Clear();
			}
			X += IX;
			Y += IY;
		}
		if (list.Count > list2.Count)
		{
			list2.Clear();
			list2.AddRange(list);
		}
		return list2;
	}

	public void Fill(BallBag<Cell> Bag, List<Cell> Cells)
	{
		int i = 0;
		int count = Cells.Count;
		int num = count / 2;
		for (; i < count; i++)
		{
			Cell item = Cells[i];
			Bag.Add(item, count / (1 + Math.Abs(i - num)));
		}
	}

	public bool BuildZone(Zone Z)
	{
		BallBag<Cell> ballBag = new BallBag<Cell>();
		List<Cell> contiguous = GetContiguous(Z, 0, Padding, 1, 0);
		List<Cell> contiguous2 = GetContiguous(Z, 79 - Padding, 0, 0, 1);
		List<Cell> contiguous3 = GetContiguous(Z, Padding, 0, 0, 1);
		if (contiguous.Count >= 10)
		{
			Fill(ballBag, contiguous);
		}
		if (contiguous2.Count >= 10)
		{
			Fill(ballBag, contiguous2);
		}
		if (contiguous3.Count >= 10)
		{
			Fill(ballBag, contiguous3);
		}
		Cell cell = ballBag.PeekOne() ?? Z.GetRandomCell();
		int rotation = 0;
		if (!contiguous.Contains(cell))
		{
			if (contiguous2.Contains(cell))
			{
				rotation = 1;
			}
			else if (contiguous3.Contains(cell))
			{
				rotation = 3;
			}
		}
		MapChunkPlacement.PlaceFromFile(cell, "preset_tile_chunks/HamilcrabShop.rpm", Width, Height, Padding, rotation);
		return true;
	}
}
