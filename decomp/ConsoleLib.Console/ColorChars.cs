using System;

namespace ConsoleLib.Console;

public struct ColorChars : IEquatable<ColorChars>
{
	public static readonly char FOREGROUND_INDICATOR = '&';

	public static readonly char BACKGROUND_INDICATOR = '^';

	public char foreground;

	public char background;

	public char detail;

	public bool Equals(ColorChars Other)
	{
		if (foreground == Other.foreground && background == Other.background)
		{
			return detail == Other.detail;
		}
		return false;
	}

	public override bool Equals(object Object)
	{
		if (Object is ColorChars other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(foreground, background, detail);
	}
}
