using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;
using XRL.World;

namespace XRL;

[Serializable]
public class Box : IComposite
{
	public int x1;

	public int y1;

	public int x2;

	public int y2;

	public Rect2D rect => new Rect2D(x1, y1, x2, y2);

	public Point RandomPoint => new Point(0, 0)
	{
		X = Stat.Random(x1, x2),
		Y = Stat.Random(y1, y2)
	};

	public int Volume => (x2 - x1) * (y2 - y1);

	public bool Valid
	{
		get
		{
			if (x1 >= 0 && x1 <= 79 && x2 >= 0 && x2 <= 79 && y1 >= 0 && y1 <= 24 && y2 >= 0)
			{
				return y2 <= 24;
			}
			return false;
		}
	}

	public Location2D center => Location2D.Get(MidX, MidY);

	public int MidX => (x2 + x1) / 2;

	public int MidY => (y2 + y1) / 2;

	public int Width => x2 - x1 + 1;

	public int Height => y2 - y1 + 1;

	public Box()
	{
		x1 = 0;
		y1 = 0;
		x2 = 0;
		y2 = 0;
	}

	public Box(int _x1, int _y1, int _x2, int _y2)
	{
		x1 = _x1;
		y1 = _y1;
		x2 = _x2;
		y2 = _y2;
		if (x2 < x1)
		{
			int num = x1;
			x1 = x2;
			x2 = num;
		}
		if (y2 < y1)
		{
			int num2 = y1;
			y1 = y2;
			y2 = num2;
		}
	}

	public static implicit operator Rect2D(Box B)
	{
		return new Rect2D(B.x1, B.y1, B.x2, B.y2);
	}

	public static Box fromRect(Rect2D rect)
	{
		return new Box(rect.x1, rect.y1, rect.x2, rect.y2);
	}

	public Box clamp(int minx, int miny, int maxx, int maxy)
	{
		Box box = new Box(x1, y1, x2, y2);
		if (box.x1 < minx)
		{
			box.x1 = minx;
		}
		if (box.x2 < minx)
		{
			box.x2 = minx;
		}
		if (box.y1 < miny)
		{
			box.y1 = miny;
		}
		if (box.y2 < miny)
		{
			box.y2 = miny;
		}
		if (box.x1 > maxx)
		{
			box.x1 = maxx;
		}
		if (box.x2 > maxx)
		{
			box.x2 = maxx;
		}
		if (box.y1 > maxy)
		{
			box.y1 = maxy;
		}
		if (box.y2 > maxy)
		{
			box.y2 = maxy;
		}
		return box;
	}

	public bool Equals(Box B)
	{
		if (B.x1 == x1 && B.x2 == x2 && B.y1 == y1)
		{
			return B.y2 == y2;
		}
		return false;
	}

	public Box Grow(int Size)
	{
		return new Box(x1 - Size, y1 - Size, x2 + Size, y2 + Size);
	}

	public bool contains(Location2D location)
	{
		return contains(location.X, location.Y);
	}

	public bool contains(int x, int y)
	{
		if (x < x1)
		{
			return false;
		}
		if (x > x2)
		{
			return false;
		}
		if (y < y1)
		{
			return false;
		}
		if (y > y2)
		{
			return false;
		}
		return true;
	}

	public IEnumerable<Location2D> contents()
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
