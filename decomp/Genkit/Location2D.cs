using System;
using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;
using XRL.Rules;
using XRL.World;

namespace Genkit;

[Serializable]
public sealed class Location2D : ILocationArea, IEquatable<Location2D>, IEquatable<Cell>
{
	public static readonly Location2D Zero;

	public static readonly Location2D Invalid;

	public static readonly int MaxX;

	public static readonly int MaxY;

	public static Location2D[,] Grid;

	public readonly int X;

	public readonly int Y;

	public readonly int Ordinal;

	public static string[] Directions;

	public static string[,] RegionDirections;

	private static readonly List<Location2D> LineReturnCache;

	public Point2D Point => new Point2D(X, Y);

	public Vector2i Vector2i => new Vector2i(X, Y);

	public IEnumerable<Location2D> cardinalNeighbors
	{
		get
		{
			Location2D location2D = Get(X - 1, 0);
			Location2D n2 = Get(X + 1, 0);
			Location2D n3 = Get(X, Y - 1);
			Location2D n4 = Get(X, Y + 1);
			if (location2D == null)
			{
				yield return location2D;
			}
			if (n2 == null)
			{
				yield return n2;
			}
			if (n3 == null)
			{
				yield return n3;
			}
			if (n4 == null)
			{
				yield return n4;
			}
		}
	}

	[Obsolete("Use Location2D.Zero")]
	public static Location2D zero => Zero;

	[Obsolete("Use Location2D.Invalid")]
	public static Location2D invalid => Invalid;

	[Obsolete("Use X")]
	public int x => X;

	[Obsolete("Use X")]
	public int y => Y;

	[Obsolete("Use X")]
	public int __x => X;

	[Obsolete("Use X")]
	public int __y => Y;

	[Obsolete("Use Point")]
	public Point2D point => Point;

	[Obsolete("Use Vector2i")]
	public Vector2i vector2i => Vector2i;

	static Location2D()
	{
		Zero = new Location2D(0, 0);
		Invalid = new Location2D(int.MinValue, int.MaxValue);
		MaxX = 250;
		MaxY = 85;
		Directions = new string[8] { "N", "S", "E", "W", "NW", "NE", "SW", "SE" };
		RegionDirections = new string[3, 3]
		{
			{ "NW", "N", "NE" },
			{ "W", ".", "E" },
			{ "SW", "S", "SE" }
		};
		LineReturnCache = new List<Location2D>(16);
		Grid = new Location2D[MaxX, MaxY];
		for (int i = 0; i < MaxX; i++)
		{
			for (int j = 0; j < MaxY; j++)
			{
				Grid[i, j] = new Location2D(i, j);
			}
		}
	}

	public Location2D(int X, int Y)
	{
		this.X = X;
		this.Y = Y;
		if (X < 80 && Y < 25)
		{
			if (X < 40)
			{
				Ordinal |= 8;
			}
			else
			{
				Ordinal |= 2;
			}
			if (Y < 13)
			{
				Ordinal |= 1;
			}
			else
			{
				Ordinal |= 4;
			}
		}
	}

	public static implicit operator Vector2(Location2D L)
	{
		return new Vector2(L.X, L.Y);
	}

	public Location2D Clamp(int minx = 0, int maxx = 79, int miny = 0, int maxy = 24)
	{
		return Get(Math.Clamp(X, minx, maxx), Math.Clamp(Y, miny, maxy));
	}

	public Location2D Wiggle(int XDelta, int YDelta)
	{
		return Get(X + Stat.Random(-XDelta, XDelta), Y + Stat.Random(-YDelta, YDelta));
	}

	public static Location2D Get(int X, int Y)
	{
		if (X < 0 || X >= MaxX || Y < 0 || Y >= MaxY)
		{
			return null;
		}
		return Grid[X, Y];
	}

	public static List<Location2D> GetWithin(int X1, int Y1, int X2, int Y2)
	{
		List<Location2D> list = new List<Location2D>((X2 - X1 + 1) * (Y2 - Y1 + 1));
		for (int i = Y1; i <= Y2; i++)
		{
			for (int j = X1; j <= X2; j++)
			{
				list.Add(Get(j, i));
			}
		}
		return list;
	}

	public override string ToString()
	{
		int num = X;
		string text = num.ToString();
		num = Y;
		return text + "," + num;
	}

	public string RegionDirection(int RegionWidth, int RegionHeight)
	{
		int num = Calc.Clamp((int)((float)X / (float)RegionWidth * 3f), 0, 2);
		int num2 = Calc.Clamp((int)((float)Y / (float)RegionHeight * 3f), 0, 2);
		return RegionDirections[num2, num];
	}

	public bool SameAs(Location2D o)
	{
		if (o.X == X)
		{
			return o.Y == Y;
		}
		return false;
	}

	public bool Inside(int X1, int Y1, int X2, int Y2)
	{
		if (X < X1)
		{
			return false;
		}
		if (X > X2)
		{
			return false;
		}
		if (Y < Y1)
		{
			return false;
		}
		if (Y > Y2)
		{
			return false;
		}
		return true;
	}

	public string DirectionTo(Location2D destination)
	{
		Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		try
		{
			if (destination.Y > Y)
			{
				utf16ValueStringBuilder.Append("S");
			}
			if (destination.Y < Y)
			{
				utf16ValueStringBuilder.Append("N");
			}
			if (destination.X < X)
			{
				utf16ValueStringBuilder.Append("W");
			}
			if (destination.X > X)
			{
				utf16ValueStringBuilder.Append("E");
			}
			if (utf16ValueStringBuilder.Length == 0)
			{
				return ".";
			}
			return utf16ValueStringBuilder.ToString();
		}
		finally
		{
			utf16ValueStringBuilder.Dispose();
		}
	}

	public string DirectionToCenter()
	{
		string text = "";
		text = ((Y >= 12) ? (text + "N") : (text + "S"));
		if (X < 40)
		{
			return text + "E";
		}
		return text + "W";
	}

	public string CardinalDirectionToCenter()
	{
		return DirectionToCenter()[Stat.Random(0, DirectionToCenter().Length - 1)].ToString();
	}

	public bool OppositeDirections(Location2D a, Location2D b)
	{
		int num = a.X - X;
		int num2 = b.X - X;
		if (num > 0 && num2 < 0)
		{
			return true;
		}
		if (num < 0 && num2 > 0)
		{
			return true;
		}
		int num3 = a.Y - Y;
		int num4 = b.Y - Y;
		if (num3 > 0 && num4 < 0)
		{
			return true;
		}
		if (num3 < 0 && num4 > 0)
		{
			return true;
		}
		return false;
	}

	public bool Backtracking(Location2D a, Location2D b)
	{
		int num = a.X - X;
		int num2 = b.X - a.X;
		if (num > 0 && num2 < 0)
		{
			return true;
		}
		if (num < 0 && num2 > 0)
		{
			return true;
		}
		int num3 = a.Y - Y;
		int num4 = b.Y - a.Y;
		if (num3 > 0 && num4 < 0)
		{
			return true;
		}
		if (num3 < 0 && num4 > 0)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (X << 7) + Y;
	}

	public Location2D FromDirection(string D)
	{
		return D switch
		{
			"N" => Get(X, Y - 1), 
			"S" => Get(X, Y + 1), 
			"E" => Get(X + 1, Y), 
			"W" => Get(X - 1, Y), 
			"NW" => Get(X - 1, Y - 1), 
			"NE" => Get(X + 1, Y - 1), 
			"SW" => Get(X - 1, Y + 1), 
			"SE" => Get(X + 1, Y + 1), 
			_ => this, 
		};
	}

	public bool Equals(Location2D L)
	{
		if ((object)L == null)
		{
			return false;
		}
		if (X == L.X)
		{
			return Y == L.Y;
		}
		return false;
	}

	public bool Equals(Cell C)
	{
		if (C == null)
		{
			return false;
		}
		if (X == C.X)
		{
			return Y == C.Y;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return (Location2D)obj == this;
	}

	public static Location2D operator -(Location2D a, Location2D b)
	{
		return Get(a.X - b.X, a.Y - b.Y);
	}

	public static Location2D operator +(Location2D a, Location2D b)
	{
		return Get(a.X + b.X, a.Y + b.Y);
	}

	public static bool operator ==(Location2D c1, Location2D c2)
	{
		if ((object)c1 == null)
		{
			return (object)c2 == null;
		}
		if ((object)c2 == null)
		{
			return false;
		}
		if (c1.X == c2.X)
		{
			return c1.Y == c2.Y;
		}
		return false;
	}

	public static bool operator !=(Location2D c1, Location2D c2)
	{
		return !(c1 == c2);
	}

	public int Distance(Location2D c2)
	{
		if (c2 == null)
		{
			return int.MaxValue;
		}
		if (c2.X == X && c2.Y == Y)
		{
			return 0;
		}
		return (int)Math.Sqrt(SquareDistance(c2));
	}

	public int Distance(int X, int Y)
	{
		if (X == this.X && Y == this.Y)
		{
			return 0;
		}
		return (int)Math.Sqrt(SquareDistance(X, Y));
	}

	public int SquareDistance(Location2D c2)
	{
		if (c2 == null)
		{
			return int.MaxValue;
		}
		if (c2.X == X && c2.Y == Y)
		{
			return 0;
		}
		return (c2.X - X) * (c2.X - X) + (c2.Y - Y) * (c2.Y - Y);
	}

	public int SquareDistance(int X, int Y)
	{
		return (X - this.X) * (X - this.X) + (Y - this.Y) * (Y - this.Y);
	}

	public int ManhattanDistance(Location2D c2)
	{
		if (c2 == null)
		{
			return int.MaxValue;
		}
		if (c2.X == X && c2.Y == Y)
		{
			return 0;
		}
		return Math.Max(Math.Abs(X - c2.X), Math.Abs(Y - c2.Y));
	}

	public int ManhattanDistance(int X, int Y)
	{
		return Math.Max(Math.Abs(this.X - X), Math.Abs(this.Y - Y));
	}

	public List<Location2D> GetRadialPoints(int Radius)
	{
		List<Location2D> list = new List<Location2D>();
		for (int i = X - Radius; i <= X + Radius; i++)
		{
			for (int j = Y - Radius; j <= Y + Radius; j++)
			{
				Location2D location2D = Get(i, j);
				if (location2D != null && (int)Math.Sqrt(SquareDistance(location2D)) == Radius)
				{
					list.Add(location2D);
				}
			}
		}
		return list;
	}

	public static List<Location2D> GetLine(int X1, int Y1, int X2, int Y2, bool ReadOnly = true)
	{
		List<Location2D> list;
		if (ReadOnly)
		{
			list = LineReturnCache;
			list.Clear();
		}
		else
		{
			list = new List<Location2D>();
		}
		GetLine(X1, Y1, X2, Y2, list);
		return list;
	}

	public static void GetLine(int X1, int Y1, int X2, int Y2, List<Location2D> Return)
	{
		int num = Math.Abs(X1 - X2);
		int num2 = -Math.Abs(Y1 - Y2);
		int num3 = ((X1 < X2) ? 1 : (-1));
		int num4 = ((Y1 < Y2) ? 1 : (-1));
		int num5 = num + num2;
		int num6 = num5 * 2;
		Return.EnsureCapacity(Return.Count + Math.Max(num, -num2) + 1);
		while (true)
		{
			Return.Add(Grid[X1, Y1]);
			if (num6 >= num2)
			{
				if (X1 == X2)
				{
					break;
				}
				num5 += num2;
				X1 += num3;
			}
			if (num6 <= num)
			{
				if (Y1 == Y2)
				{
					break;
				}
				num5 += num;
				Y1 += num4;
			}
			num6 = num5 * 2;
		}
	}

	public static List<Location2D> GetCardinalLine(int X1, int Y1, int X2, int Y2, bool ReadOnly = true)
	{
		List<Location2D> list;
		if (ReadOnly)
		{
			list = LineReturnCache;
			list.Clear();
		}
		else
		{
			list = new List<Location2D>();
		}
		GetCardinalLine(X1, Y1, X2, Y2, list);
		return list;
	}

	public static void GetCardinalLine(int X1, int Y1, int X2, int Y2, List<Location2D> Return)
	{
		int num = Math.Abs(X1 - X2);
		int num2 = -Math.Abs(Y1 - Y2);
		int num3 = ((X1 < X2) ? 1 : (-1));
		int num4 = ((Y1 < Y2) ? 1 : (-1));
		int num5 = num + num2;
		Return.EnsureCapacity(Return.Count + num - num2 + 1);
		Return.Add(Grid[X1, Y1]);
		while (X1 != X2 || Y1 != Y2)
		{
			if (2 * num5 - num2 > num - 2 * num5)
			{
				num5 += num2;
				X1 += num3;
			}
			else
			{
				num5 += num;
				Y1 += num4;
			}
			Return.Add(Grid[X1, Y1]);
		}
	}

	public IEnumerable<Location2D> YieldAdjacent(int Radius)
	{
		Radius = Radius * 2 + 1;
		int x = X;
		int y = Y;
		int x2 = 1;
		int y2 = 0;
		int l = 1;
		int i = 0;
		int p = 1;
		int c = Radius * Radius - 1;
		while (i < c)
		{
			x += x2;
			y += y2;
			if (p >= l)
			{
				p = 0;
				int num = x2;
				x2 = -y2;
				y2 = num;
				if (y2 == 0)
				{
					l++;
				}
			}
			Location2D location2D = Get(x, y);
			if (location2D != null)
			{
				yield return location2D;
			}
			i++;
			p++;
		}
	}

	public float AngleTo(Location2D Location)
	{
		return Vector2.Angle(this, Location);
	}

	public float AngleTo(Location2D fulcrum, Location2D to)
	{
		Vector2 vector = new Vector2(X - fulcrum.X, Y - fulcrum.Y);
		Vector2 to2 = new Vector2(to.X - fulcrum.X, to.Y - fulcrum.Y);
		return Vector2.Angle(vector, to2);
	}

	public IEnumerable<Location2D> YieldPerimeter(int Size = 1)
	{
		int x1 = X - Size;
		int x2 = X + Size;
		int y1 = Y - Size;
		int y2 = Y + Size;
		for (; x1 <= x2; x1++)
		{
			if (x1 >= 0 && x1 < MaxX)
			{
				if (y1 >= 0)
				{
					yield return Grid[x1, y1];
				}
				if (y2 < MaxY)
				{
					yield return Grid[x1, y2];
				}
			}
		}
		y2 = X - Size;
		y1 = X + Size;
		x2 = Y - Size + 1;
		for (x1 = Y + Size - 1; x2 <= x1; x2++)
		{
			if (x2 >= 0 && x2 < MaxY)
			{
				if (y2 >= 0)
				{
					yield return Grid[y2, x2];
				}
				if (y1 < MaxX)
				{
					yield return Grid[y1, x2];
				}
			}
		}
	}

	public IEnumerable<Location2D> EnumerateLocations()
	{
		yield return this;
	}

	public IEnumerable<Location2D> EnumerateBorderLocations()
	{
		yield return this;
	}

	public IEnumerable<Location2D> EnumerateNonBorderLocations()
	{
		yield return this;
	}

	public Location2D GetCenter()
	{
		return this;
	}

	public bool PointIn(Location2D location)
	{
		return location == this;
	}

	public int GetCourseFrom(Location2D Target)
	{
		int num = 0;
		if ((object)Target == null || (object)Target == this)
		{
			return num;
		}
		int num2 = Target.X;
		int num3 = Target.Y;
		if (X < num2)
		{
			num |= 2;
		}
		else if (X > num2)
		{
			num |= 8;
		}
		if (Y < num3)
		{
			num |= 4;
		}
		else if (Y > num3)
		{
			num |= 1;
		}
		return num;
	}

	public int GetCourseTo(Location2D Target)
	{
		return Courses.Opposite[GetCourseFrom(Target)];
	}

	[Obsolete("Use Location2D.Get")]
	public static Location2D get(int x, int y)
	{
		return Get(x, y);
	}
}
