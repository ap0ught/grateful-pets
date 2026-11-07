using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genkit;

[Serializable]
public struct Color4 : IEquatable<Color4>, IEqualityComparer<Color4>
{
	public float r;

	public float g;

	public float b;

	public float a;

	public Color toUnityColor => new Color(r, g, b, a);

	public static Color4 black => new Color4(0f, 0f, 0f, 1f);

	public static Color4 white => new Color4(1f, 1f, 1f, 1f);

	public static Color4 red => new Color4(1f, 0f, 0f, 1f);

	public static Color4 magenta => new Color4(1f, 0f, 1f, 1f);

	public static Color4 green => new Color4(0f, 1f, 0f, 1f);

	public Color4(float r, float g, float b, float a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public Color4(float r, float g, float b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		a = 1f;
	}

	public override bool Equals(object obj)
	{
		if (obj.GetType() == typeof(Color4))
		{
			return Equals((Color4)obj);
		}
		return false;
	}

	public bool Equals(Color4 other)
	{
		if (r == other.r && g == other.g && b == other.b)
		{
			return a == other.a;
		}
		return false;
	}

	public bool Equals(Color4 x, Color4 y)
	{
		return x.Equals(y);
	}

	public override int GetHashCode()
	{
		return GetHashCode(this);
	}

	public int GetHashCode(Color4 obj)
	{
		return r.GetHashCode() ^ (g.GetHashCode() << 2) ^ (b.GetHashCode() >> 2) ^ (a.GetHashCode() << 4);
	}

	public static bool operator ==(Color4 x, Color4 y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(Color4 x, Color4 y)
	{
		return !x.Equals(y);
	}
}
