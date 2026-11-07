using System;
using Cysharp.Text;
using UnityEngine;
using XRL.World;

[Serializable]
public class Vector2i : IComposite
{
	public int x;

	public int y;

	public bool WantFieldReflection => false;

	public Vector2i()
	{
	}

	public Vector2i(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public Vector2i Clamp(int minx = 0, int miny = 0, int maxx = 79, int maxy = 24)
	{
		Vector2i vector2i = new Vector2i(x, y);
		vector2i.x = Mathf.Clamp(vector2i.x, minx, maxx);
		vector2i.y = Mathf.Clamp(vector2i.y, miny, maxy);
		return vector2i;
	}

	public string DirectionTo(Vector2i destination)
	{
		Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		try
		{
			if (destination.y > y)
			{
				utf16ValueStringBuilder.Append("S");
			}
			if (destination.y < y)
			{
				utf16ValueStringBuilder.Append("N");
			}
			if (destination.x < x)
			{
				utf16ValueStringBuilder.Append("W");
			}
			if (destination.x > x)
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

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is Vector2i)
		{
			return this == (Vector2i)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode();
	}

	public static bool operator ==(Vector2i x, Vector2i y)
	{
		if ((object)x == null)
		{
			return (object)y == null;
		}
		if ((object)y == null)
		{
			return false;
		}
		if (x.x == y.x)
		{
			return x.y == y.y;
		}
		return false;
	}

	public static bool operator !=(Vector2i x, Vector2i y)
	{
		return !(x == y);
	}

	public int DistanceTo(Vector2i d)
	{
		return (int)Mathf.Sqrt((d.x - x) * (d.x - x) + (d.y - y) * (d.y - y));
	}
}
