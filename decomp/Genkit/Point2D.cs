using System;
using System.Collections.Generic;
using XRL.World;

namespace Genkit;

[Serializable]
public struct Point2D : IComposite
{
	public static readonly Point2D zero = new Point2D(0, 0);

	public static readonly Point2D invalid = new Point2D(int.MinValue, int.MaxValue);

	public static string[] Directions = new string[8] { "N", "S", "E", "W", "NW", "NE", "SW", "SE" };

	public static string[,] RegionDirections = new string[3, 3]
	{
		{ "NW", "N", "NE" },
		{ "W", ".", "E" },
		{ "SW", "S", "SE" }
	};

	public int x;

	public int y;

	public Location2D location => Location2D.Get(x, y);

	public bool WantFieldReflection => false;

	public override string ToString()
	{
		return x + "," + y;
	}

	public string RegionDirection(int RegionWidth, int RegionHeight)
	{
		int num = Calc.Clamp((int)((float)x / (float)RegionWidth * 3f), 0, 2);
		int num2 = Calc.Clamp((int)((float)y / (float)RegionHeight * 3f), 0, 2);
		return RegionDirections[num2, num];
	}

	public Point2D(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(x);
		Writer.WriteOptimized(y);
	}

	public void Read(SerializationReader Reader)
	{
		x = Reader.ReadOptimizedInt32();
		y = Reader.ReadOptimizedInt32();
	}

	public readonly Point2D FromDirection(string D)
	{
		return D switch
		{
			"N" => new Point2D(x, y - 1), 
			"S" => new Point2D(x, y + 1), 
			"E" => new Point2D(x + 1, y), 
			"W" => new Point2D(x - 1, y), 
			"NW" => new Point2D(x - 1, y - 1), 
			"NE" => new Point2D(x + 1, y - 1), 
			"SW" => new Point2D(x - 1, y + 1), 
			"SE" => new Point2D(x + 1, y + 1), 
			_ => this, 
		};
	}

	public override int GetHashCode()
	{
		return x ^ y;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return (Point2D)obj == this;
	}

	public static bool operator ==(Point2D c1, Point2D c2)
	{
		if (c1.x == c2.x)
		{
			return c1.y == c2.y;
		}
		return false;
	}

	public static bool operator !=(Point2D c1, Point2D c2)
	{
		if (c1.x == c2.x)
		{
			return c1.y != c2.y;
		}
		return true;
	}

	public static Point2D operator -(Point2D a, Point2D b)
	{
		return new Point2D(a.x - b.x, a.y - b.y);
	}

	public static Point2D operator +(Point2D a, Point2D b)
	{
		return new Point2D(a.x + b.x, a.y + b.y);
	}

	public int Distance(Point2D c2)
	{
		return (int)Math.Sqrt(SquareDistance(c2));
	}

	public int SquareDistance(Point2D c2)
	{
		return (c2.x - x) * (c2.x - x) + (c2.y - y) * (c2.y - y);
	}

	public int ManhattanDistance(Point2D c2)
	{
		return Math.Max(Math.Abs(x - c2.x), Math.Abs(y - c2.y));
	}

	public int ManhattanDistance(int X, int Y)
	{
		return Math.Max(Math.Abs(x - X), Math.Abs(y - Y));
	}

	public List<Point2D> GetRadialPoints(int Radius, List<Point2D> listToUse = null)
	{
		List<Point2D> list;
		if (listToUse != null)
		{
			list = listToUse;
			list.Clear();
		}
		else
		{
			list = new List<Point2D>();
		}
		for (int i = x - Radius; i <= x + Radius; i++)
		{
			for (int j = y - Radius; j <= y + Radius; j++)
			{
				if ((int)Math.Sqrt(SquareDistance(new Point2D(i, j))) == Radius)
				{
					list.Add(new Point2D(i, j));
				}
			}
		}
		return list;
	}
}
