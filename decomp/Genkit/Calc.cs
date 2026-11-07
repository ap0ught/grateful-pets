using System;
using System.Collections.Generic;

namespace Genkit;

public static class Calc
{
	public static Random _R = new Random();

	public static Random R
	{
		get
		{
			return _R;
		}
		set
		{
			_R = value;
		}
	}

	public static void Reseed(int s)
	{
		R = new Random(s);
	}

	public static int Clamp(int n, int Min, int Max)
	{
		if (n < Min)
		{
			return Min;
		}
		if (n > Max)
		{
			return Max;
		}
		return n;
	}

	public static string GetOppositeDirection(string D)
	{
		return D switch
		{
			"N" => "S", 
			"S" => "N", 
			"E" => "W", 
			"W" => "E", 
			"NW" => "SE", 
			"NE" => "SW", 
			"SW" => "NE", 
			"SE" => "NW", 
			_ => D, 
		};
	}

	public static string GetOppositeDirection(char D)
	{
		return D switch
		{
			'N' => "S", 
			'S' => "N", 
			'E' => "W", 
			'W' => "E", 
			_ => D.ToString(), 
		};
	}

	public static int Random(int Low, int High)
	{
		return R.Next(Low, High);
	}

	public static T Random<T>(List<T> L)
	{
		return L[Random(0, L.Count - 1)];
	}
}
