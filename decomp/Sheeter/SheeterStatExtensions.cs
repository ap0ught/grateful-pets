using System;
using System.Collections.Generic;
using System.Linq;

namespace Sheeter;

public static class SheeterStatExtensions
{
	public static double StandardDeviation(this IList<int> sequence)
	{
		List<int> source = sequence.ToList();
		double avg = source.Average((int v) => v);
		return Math.Sqrt(source.Average((int v) => Math.Pow((double)v - avg, 2.0)));
	}

	public static double StandardDeviation(this IList<float> sequence)
	{
		List<float> source = sequence.ToList();
		float avg = source.Average((float v) => v);
		return Math.Sqrt(source.Average((float v) => Math.Pow(v - avg, 2.0)));
	}

	public static double StandardDeviation(this IList<double> sequence)
	{
		List<double> source = sequence.ToList();
		double avg = source.Average((double v) => v);
		return Math.Sqrt(source.Average((double v) => Math.Pow(v - avg, 2.0)));
	}

	private static int Partition<T>(this IList<T> list, int start, int end, Random rnd = null) where T : IComparable<T>
	{
		if (rnd != null)
		{
			list.Swap(end, rnd.Next(start, end + 1));
		}
		T other = list[end];
		int num = start - 1;
		for (int i = start; i < end; i++)
		{
			if (list[i].CompareTo(other) <= 0)
			{
				list.Swap(i, ++num);
			}
		}
		list.Swap(end, ++num);
		return num;
	}

	public static T NthOrderStatistic<T>(this IList<T> list, int n, Random rnd = null) where T : IComparable<T>
	{
		return list.NthOrderStatistic(n, 0, list.Count - 1, rnd);
	}

	private static T NthOrderStatistic<T>(this IList<T> list, int n, int start, int end, Random rnd) where T : IComparable<T>
	{
		int num;
		while (true)
		{
			num = list.Partition(start, end, rnd);
			if (num == n)
			{
				break;
			}
			if (n < num)
			{
				end = num - 1;
			}
			else
			{
				start = num + 1;
			}
		}
		return list[num];
	}

	public static void Swap<T>(this IList<T> list, int i, int j)
	{
		if (i != j)
		{
			T value = list[i];
			list[i] = list[j];
			list[j] = value;
		}
	}

	public static T Median<T>(this IList<T> list) where T : IComparable<T>
	{
		return list.NthOrderStatistic((list.Count - 1) / 2);
	}

	public static double Median<T>(this IEnumerable<T> sequence, Func<T, double> getValue)
	{
		List<double> list = sequence.Select(getValue).ToList();
		int n = (list.Count - 1) / 2;
		return list.NthOrderStatistic(n);
	}

	public static List<U> Map<T, U>(this List<T> sequence, Func<T, U> mapper)
	{
		List<U> list = new List<U>(sequence.Count);
		foreach (T item in sequence)
		{
			list.Add(mapper(item));
		}
		return list;
	}
}
