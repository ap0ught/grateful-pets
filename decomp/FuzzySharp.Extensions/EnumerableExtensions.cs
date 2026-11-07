using System;
using System.Collections.Generic;
using FuzzySharp.Utils;

namespace FuzzySharp.Extensions;

public static class EnumerableExtensions
{
	public static IEnumerable<T> MaxN<T>(this IEnumerable<T> source, int n) where T : IComparable<T>
	{
		MinHeap<T> queue = new MinHeap<T>(Comparer<T>.Create((T x, T y) => x.CompareTo(y)));
		foreach (T item in source)
		{
			if (queue.Count < n)
			{
				queue.Add(item);
			}
			else if (item.CompareTo(queue.GetMin()) > 0)
			{
				queue.ExtractDominating();
				queue.Add(item);
			}
		}
		for (int i = 0; i < n; i++)
		{
			if (queue.Count <= 0)
			{
				break;
			}
			yield return queue.ExtractDominating();
		}
	}

	public static IEnumerable<T> MaxNBy<T, TVal>(this IEnumerable<T> source, int n, Func<T, TVal> selector) where TVal : IComparable<TVal>
	{
		MinHeap<T> queue = new MinHeap<T>(Comparer<T>.Create((T x, T y) => selector(x).CompareTo(selector(y))));
		foreach (T item in source)
		{
			if (queue.Count < n)
			{
				queue.Add(item);
			}
			else if (selector(item).CompareTo(selector(queue.GetMin())) > 0)
			{
				queue.ExtractDominating();
				queue.Add(item);
			}
		}
		for (int i = 0; i < n; i++)
		{
			if (queue.Count <= 0)
			{
				break;
			}
			yield return queue.ExtractDominating();
		}
	}
}
