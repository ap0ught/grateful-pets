using System;

namespace QupKit;

public static class Rng
{
	private static Random R;

	public static string GetRandomDirection()
	{
		return Next(0, 3) switch
		{
			0 => "N", 
			1 => "S", 
			2 => "E", 
			3 => "W", 
			_ => "N", 
		};
	}

	public static int Next(int Low, int High)
	{
		if (R == null)
		{
			R = new Random();
		}
		return R.Next(Low, High + 1);
	}
}
