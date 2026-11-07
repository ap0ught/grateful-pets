using System;
using System.Collections.Generic;
using XRL;
using XRL.Rules;
using XRL.World;

namespace Genkit;

[Serializable]
public struct Rect2D : IComposite
{
	public static Rect2D zero = new Rect2D(0, 0, 0, 0);

	public static Rect2D invalid = new Rect2D(-1, 1, 1, -1);

	public string DoorDirection;

	public Point2D Door;

	public int x1;

	public int y1;

	public int x2;

	public int y2;

	public int Pinch;

	public Point2D Center => new Point2D((x1 + x2) / 2, (y1 + y2) / 2);

	public int Area => Math.Max(0, Width * Height);

	public Point2D UpperLeft => new Point2D(x1, y1);

	public int Width => x2 - x1 + 1;

	public int Height => y2 - y1 + 1;

	public IEnumerable<Location2D> locations
	{
		get
		{
			for (int x = x1; x <= x2; x++)
			{
				for (int y = y1; y <= y2; y++)
				{
					yield return Location2D.Get(x, y);
				}
			}
		}
	}

	public static implicit operator Box(Rect2D R)
	{
		return new Box(R.x1, R.y1, R.x2, R.y2);
	}

	public Point2D randomPoint()
	{
		return new Point2D(Stat.Random(x1, x2), Stat.Random(y1, y2));
	}

	public List<Point2D> getPoints()
	{
		List<Point2D> list = new List<Point2D>();
		for (int i = x1; i <= x2; i++)
		{
			for (int j = y1; j <= y2; j++)
			{
				list.Add(new Point2D(i, j));
			}
		}
		return list;
	}

	public IEnumerable<Location2D> allPoints()
	{
		for (int x = x1; x <= x2; x++)
		{
			for (int y = y1; y <= y2; y++)
			{
				yield return Location2D.Get(x, y);
			}
		}
	}

	public bool overlaps(Rect2D rect)
	{
		if (x1 > rect.x2 || rect.x1 > x2)
		{
			return false;
		}
		if (y1 > rect.y2 || rect.y1 > y2)
		{
			return false;
		}
		return true;
	}

	public Rect2D enforceVailidity(Rect2D source)
	{
		return new Rect2D(source.x1, source.y1, Math.Max(source.x1, source.x2), Math.Max(source.y1, source.y2));
	}

	public Rect2D(List<Location2D> pointsToWrap)
	{
		x1 = int.MaxValue;
		y1 = int.MaxValue;
		x2 = int.MinValue;
		y2 = int.MinValue;
		Door = Point2D.invalid;
		Pinch = -1;
		DoorDirection = null;
		foreach (Location2D item in pointsToWrap)
		{
			if (x1 > item.X)
			{
				x1 = item.X;
			}
			if (y1 > item.Y)
			{
				y1 = item.Y;
			}
			if (x2 < item.X)
			{
				x2 = item.X;
			}
			if (y2 < item.Y)
			{
				y2 = item.Y;
			}
		}
	}

	public Rect2D(int x1, int y1, int x2, int y2)
	{
		this.x1 = x1;
		this.y1 = y1;
		this.x2 = x2;
		this.y2 = y2;
		Door = Point2D.invalid;
		Pinch = -1;
		DoorDirection = null;
	}

	public Rect2D(int x1, int y1, int x2, int y2, Point2D Door)
	{
		this.x1 = x1;
		this.y1 = y1;
		this.x2 = x2;
		this.y2 = y2;
		this.Door = Door;
		Pinch = -1;
		DoorDirection = null;
	}

	public override string ToString()
	{
		return "[" + x1 + "," + y1 + "->" + x2 + "," + y2 + "]";
	}

	public bool IsPointAdjacentToAvoid(Point2D P, List<Point2D> Avoid)
	{
		for (int i = 0; i < Avoid.Count; i++)
		{
			if (IsPointAdjacentToPoint(P, Avoid[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPointAdjacentToAvoid(Location2D P, List<Location2D> Avoid)
	{
		if (P == null)
		{
			return false;
		}
		for (int i = 0; i < Avoid.Count; i++)
		{
			if (IsPointAdjacentToPoint(P, Avoid[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPointAdjacentToPoint(Point2D P, Point2D P2)
	{
		if (P.x == P2.x && P.y >= P2.y - 1 && P.y <= P2.y + 1)
		{
			return true;
		}
		if (P.y == P2.y && P.x >= P2.x - 1 && P.x <= P2.x + 1)
		{
			return true;
		}
		return false;
	}

	public bool IsPointAdjacentToPoint(Location2D P, Location2D P2)
	{
		if (P == null)
		{
			return false;
		}
		if (P2 == null)
		{
			return false;
		}
		if (P.X == P2.X && P.Y >= P2.Y - 1 && P.Y <= P2.Y + 1)
		{
			return true;
		}
		if (P.Y == P2.Y && P.X >= P2.X - 1 && P.X <= P2.X + 1)
		{
			return true;
		}
		return false;
	}

	public bool IsPointAdjacentToDoor(Point2D P)
	{
		if (Door == Point2D.invalid)
		{
			return false;
		}
		if (P.x == Door.x && P.y >= Door.y - 2 && P.y <= Door.y + 2)
		{
			return true;
		}
		if (P.y == Door.y && P.x >= Door.x - 2 && P.x <= Door.x + 2)
		{
			return true;
		}
		return false;
	}

	public bool IsPointAdjacentToDoor(Location2D P)
	{
		if (Door == Point2D.invalid)
		{
			return false;
		}
		if (P == null)
		{
			return false;
		}
		if (P.X == Door.x && P.Y >= Door.y - 2 && P.Y <= Door.y + 2)
		{
			return true;
		}
		if (P.Y == Door.y && P.X >= Door.x - 2 && P.X <= Door.x + 2)
		{
			return true;
		}
		return false;
	}

	public bool Contains(Rect2D rect)
	{
		if (rect.x1 >= x1 && rect.y1 >= y1 && rect.x2 <= x2)
		{
			return rect.y2 <= y2;
		}
		return false;
	}

	public bool Contains(Point2D point)
	{
		return Contains(point.x, point.y);
	}

	public bool Contains(int x, int y)
	{
		if (x >= x1 && x <= x2 && y >= y1)
		{
			return y <= y2;
		}
		return false;
	}

	public string GetCellSide(Point2D C)
	{
		if (C.x == x1)
		{
			return "W";
		}
		if (C.x == x2)
		{
			return "E";
		}
		if (C.y == y1)
		{
			return "N";
		}
		if (C.y == y2)
		{
			return "S";
		}
		return ".";
	}

	public bool isInvalid()
	{
		return this == invalid;
	}

	private static int Rnd(int Low, int High)
	{
		if (Low >= High)
		{
			return Low;
		}
		return Calc.Random(Low, High);
	}

	public Point2D GetRandomDoorCell(string Wall = "?", int MinDistance = 2)
	{
		int num = 0;
		if (Wall == "?")
		{
			num = Calc.Random(0, 3);
		}
		if (Wall == "N")
		{
			num = 0;
		}
		if (Wall == "S")
		{
			num = 1;
		}
		if (Wall == "E")
		{
			num = 2;
		}
		if (Wall == "W")
		{
			num = 3;
		}
		Door = Point2D.invalid;
		if (num == 0)
		{
			Door = new Point2D(Rnd(x1 + MinDistance, x2 - MinDistance), y1);
		}
		if (num == 1)
		{
			Door = new Point2D(Rnd(x1 + MinDistance, x2 - MinDistance), y2);
		}
		if (num == 2)
		{
			Door = new Point2D(x2, Rnd(y1 + MinDistance, y2 - MinDistance));
		}
		if (num == 3)
		{
			Door = new Point2D(x1, Rnd(y1 + MinDistance, y2 - MinDistance));
		}
		DoorDirection = GetCellSide(Door);
		return Door;
	}

	public bool PointIn(Location2D P)
	{
		if (P == null)
		{
			return false;
		}
		if (P.X >= x1 && P.X <= x2 && P.Y >= y1)
		{
			return P.Y <= y2;
		}
		return false;
	}

	public bool PointIn(Point2D P)
	{
		if (P.x >= x1 && P.x <= x2 && P.y >= y1)
		{
			return P.y <= y2;
		}
		return false;
	}

	public Rect2D ReduceBy(int x, int y)
	{
		return new Rect2D(x1 + x, y1 + y, x2 - x, y2 - y, Door);
	}

	public Rect2D Translate(Point2D P)
	{
		return new Rect2D(x1 + P.x, y1 + P.y, x2 + P.x, y2 + P.y, Door);
	}

	public Rect2D Clamp(int minx, int miny, int maxx, int maxy)
	{
		return new Rect2D(Math.Max(x1, minx), Math.Max(y1, miny), Math.Min(x2, maxx), Math.Min(y2, maxy));
	}

	public void ForEachLocation(Action<Location2D> action)
	{
		for (int i = x1; i <= x2; i++)
		{
			for (int j = y1; j <= y2; j++)
			{
				action(Location2D.Get(i, j));
			}
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		Rect2D rect2D = (Rect2D)obj;
		if (rect2D.x1 == x1 && rect2D.y1 == y1 && rect2D.x2 == x2)
		{
			return rect2D.y2 == y2;
		}
		return false;
	}

	public static bool operator !=(Rect2D x, Rect2D y)
	{
		return !x.Equals(y);
	}

	public static bool operator ==(Rect2D x, Rect2D y)
	{
		return x.Equals(y);
	}

	public override int GetHashCode()
	{
		return ((DoorDirection != null) ? DoorDirection.GetHashCode() : 0) ^ Door.GetHashCode() ^ x1 ^ x2 ^ y1 ^ y2 ^ Pinch;
	}

	public Rect2D ExpandBy(int n, bool clampToScreenSize = false)
	{
		if (clampToScreenSize)
		{
			return new Rect2D(Math.Max(0, x1 - n), Math.Max(0, y1 - n), Math.Min(79, x2 + n), Math.Min(24, y2 + n), Door);
		}
		return new Rect2D(x1 - n, y1 - n, x2 + n, y2 + n, Door);
	}
}
