using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;

namespace Genkit;

[Serializable]
public class InfluenceMapRegion : ILocationArea
{
	public InfluenceMap map;

	public Rect2D BoundingBox;

	public int Seed;

	public int Size;

	public List<InfluenceMapRegion> AdjacentRegions = new List<InfluenceMapRegion>();

	public List<Location2D> Cells = new List<Location2D>();

	public List<Location2D> BorderCells = new List<Location2D>();

	public List<Location2D> NonBorderCells = new List<Location2D>();

	public List<string> Tags = new List<string>();

	private Rect2D _maxRect = Rect2D.invalid;

	public Location2D Center => Location2D.Get((BoundingBox.x2 + BoundingBox.x1) / 2, (BoundingBox.y2 + BoundingBox.y1) / 2);

	public Rect2D maxRect
	{
		get
		{
			if (_maxRect.Equals(Rect2D.invalid))
			{
				_maxRect = GridTools.MaxRectByArea(GetGrid()).Translate(BoundingBox.UpperLeft);
			}
			return _maxRect;
		}
	}

	public InfluenceMapRegion(int Seed, InfluenceMap parent)
	{
		map = parent;
		this.Seed = Seed;
		BoundingBox.x1 = int.MaxValue;
		BoundingBox.y1 = int.MaxValue;
		BoundingBox.x2 = int.MinValue;
		BoundingBox.y2 = int.MinValue;
	}

	public List<Location2D> reducyBy(int radius)
	{
		List<Location2D> list = new List<Location2D>(Cells);
		List<Location2D> list2 = new List<Location2D>();
		if (radius == 0)
		{
			return list;
		}
		for (int i = 0; i < radius; i++)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list.Contains(list[num].FromDirection("N")) && list.Contains(list[num].FromDirection("S")) && list.Contains(list[num].FromDirection("E")) && list.Contains(list[num].FromDirection("W")))
				{
					list2.Add(list[num]);
				}
			}
			List<Location2D> list3 = list;
			list = list2;
			list2 = list3;
			list2.Clear();
		}
		return list;
	}

	public List<Location2D> getBorder(int radius)
	{
		List<Location2D> list = new List<Location2D>();
		List<Location2D> list2 = new List<Location2D>(Cells);
		List<Location2D> list3 = new List<Location2D>();
		if (radius == 0)
		{
			return list2;
		}
		for (int i = 0; i < radius; i++)
		{
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				if (!list2.Contains(list2[num].FromDirection("N")) || !list2.Contains(list2[num].FromDirection("S")) || !list2.Contains(list2[num].FromDirection("E")) || !list2.Contains(list2[num].FromDirection("W")))
				{
					list.Add(list2[num]);
				}
				else
				{
					list3.Add(list2[num]);
				}
			}
			List<Location2D> list4 = list2;
			list2 = list3;
			list3 = list4;
			list3.Clear();
		}
		return list;
	}

	public bool AnyPointsIn(List<Location2D> P)
	{
		for (int i = 0; i < P.Count; i++)
		{
			if (PointIn(P[i]))
			{
				return true;
			}
		}
		return false;
	}

	public int minRadius()
	{
		return Math.Min(BoundingBox.Width, BoundingBox.Height);
	}

	public bool PointIn(Location2D P)
	{
		return Cells.Contains(P);
	}

	public bool PointIn(Point2D P)
	{
		return PointIn(P.location);
	}

	public bool IsEdgeRegion()
	{
		if (BoundingBox.x1 <= 0)
		{
			return true;
		}
		if (BoundingBox.x2 >= 79)
		{
			return true;
		}
		if (BoundingBox.y1 <= 0)
		{
			return true;
		}
		if (BoundingBox.y2 >= 24)
		{
			return true;
		}
		return false;
	}

	public bool IsCornerRegion()
	{
		if (BoundingBox.x1 <= 0 || BoundingBox.x2 >= 79)
		{
			if (BoundingBox.y1 > 0)
			{
				return BoundingBox.y2 >= 24;
			}
			return true;
		}
		return false;
	}

	public void draw(bool wait = true, bool drawempty = true, string color = "&B")
	{
		ScreenBuffer scrapBuffer = Popup.ScrapBuffer;
		for (int i = 0; i < 80; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				scrapBuffer.Goto(i, j);
				if (Cells.Contains(Location2D.Get(i, j)))
				{
					scrapBuffer.Write(color + "#");
				}
				else if (drawempty)
				{
					scrapBuffer.Write("&K.");
				}
			}
		}
		XRLCore._Console.DrawBuffer(scrapBuffer);
		if (wait)
		{
			Keyboard.getch();
		}
	}

	public InfluenceMapRegion deepCopy()
	{
		return new InfluenceMapRegion(Seed, map)
		{
			map = map,
			BoundingBox = BoundingBox,
			Seed = Seed,
			Size = Size,
			AdjacentRegions = new List<InfluenceMapRegion>(AdjacentRegions),
			Cells = new List<Location2D>(Cells),
			BorderCells = new List<Location2D>(BorderCells),
			NonBorderCells = new List<Location2D>(NonBorderCells),
			Tags = new List<string>(Tags)
		};
	}

	public void removeCell(Location2D c)
	{
		Cells.Remove(c);
		BorderCells.Remove(c);
		NonBorderCells.Remove(c);
	}

	public IEnumerable<Location2D> EnumerateLocations()
	{
		return Cells;
	}

	public bool Contains(Location2D cell)
	{
		return Cells.Contains(cell);
	}

	public bool HasTag(string tag)
	{
		return Tags.Contains(tag);
	}

	public bool ConnectsToTag(string tag, List<InfluenceMapRegion> Visited = null)
	{
		if (Visited == null)
		{
			Visited = new List<InfluenceMapRegion>();
		}
		Visited.Add(this);
		if (Tags.Contains(tag))
		{
			return true;
		}
		for (int i = 0; i < AdjacentRegions.Count; i++)
		{
			if (!Visited.Contains(AdjacentRegions[i]))
			{
				Visited.Add(AdjacentRegions[i]);
				if (AdjacentRegions[i].ConnectsToTag(tag, Visited))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddCell(Location2D P)
	{
		_maxRect = Rect2D.invalid;
		if (P.X < BoundingBox.x1)
		{
			BoundingBox.x1 = P.X;
		}
		if (P.Y < BoundingBox.y1)
		{
			BoundingBox.y1 = P.Y;
		}
		if (P.X > BoundingBox.x2)
		{
			BoundingBox.x2 = P.X;
		}
		if (P.Y > BoundingBox.y2)
		{
			BoundingBox.y2 = P.Y;
		}
		Cells.Add(P);
	}

	public int[,] GetGrid()
	{
		int[,] array = new int[BoundingBox.x2 - BoundingBox.x1 + 1, BoundingBox.y2 - BoundingBox.y1 + 1];
		for (int i = BoundingBox.x1; i <= BoundingBox.x2; i++)
		{
			for (int j = BoundingBox.y1; j <= BoundingBox.y2; j++)
			{
				int num = 1;
				if (Cells.Contains(Location2D.Get(i, j)))
				{
					num = 0;
				}
				array[i - BoundingBox.x1, j - BoundingBox.y1] = num;
			}
		}
		return array;
	}

	public IEnumerable<Location2D> EnumerateBorderLocations()
	{
		return BorderCells;
	}

	public IEnumerable<Location2D> EnumerateNonBorderLocations()
	{
		return NonBorderCells;
	}

	public Location2D GetCenter()
	{
		return Center;
	}
}
