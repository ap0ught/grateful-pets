using System;

namespace Genkit;

public static class Mathx
{
	public static T Min<T>(T[] d, T max) where T : IComparable<T>
	{
		T val = max;
		for (int i = 0; i < d.Length; i++)
		{
			if (d[i].CompareTo(val) < 0)
			{
				val = d[i];
			}
		}
		return val;
	}

	public static T Min<T>(T[,] d, T max) where T : IComparable<T>
	{
		T val = max;
		for (int i = 0; i <= d.GetUpperBound(0); i++)
		{
			for (int j = 0; j <= d.GetUpperBound(1); j++)
			{
				if (d[i, j].CompareTo(val) < 0)
				{
					val = d[i, j];
				}
			}
		}
		return val;
	}

	public static T Min<T>(T[,,] d, T max) where T : IComparable<T>
	{
		T val = max;
		for (int i = 0; i <= d.GetUpperBound(0); i++)
		{
			for (int j = 0; j <= d.GetUpperBound(1); j++)
			{
				for (int k = 0; k <= d.GetUpperBound(2); k++)
				{
					if (d[i, j, k].CompareTo(val) < 0)
					{
						val = d[i, j, k];
					}
				}
			}
		}
		return val;
	}

	public static T Max<T>(T[] d, T min) where T : IComparable<T>
	{
		T val = min;
		for (int i = 0; i < d.Length; i++)
		{
			if (d[i].CompareTo(val) > 0)
			{
				val = d[i];
			}
		}
		return val;
	}

	public static T Max<T>(T[,] d, T min) where T : IComparable<T>
	{
		T val = min;
		for (int i = 0; i < d.GetUpperBound(0); i++)
		{
			for (int j = 0; j < d.GetUpperBound(1); j++)
			{
				if (d[i, j].CompareTo(val) > 0)
				{
					val = d[i, j];
				}
			}
		}
		return val;
	}

	public static T Max<T>(T[,,] d, T min) where T : IComparable<T>
	{
		T val = min;
		for (int i = 0; i < d.GetUpperBound(0); i++)
		{
			for (int j = 0; j < d.GetUpperBound(1); j++)
			{
				for (int k = 0; k < d.GetUpperBound(2); k++)
				{
					if (d[i, j, k].CompareTo(val) > 0)
					{
						val = d[i, j, k];
					}
				}
			}
		}
		return val;
	}
}
