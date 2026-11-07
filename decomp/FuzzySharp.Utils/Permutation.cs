using System.Collections.Generic;
using System.Linq;

namespace FuzzySharp.Utils;

public static class Permutation
{
	public static List<List<T>> AllPermutations<T>(this IEnumerable<T> seed)
	{
		List<T> list = new List<T>(seed);
		return Permute(list, 0, list.Count - 1).ToList();
	}

	public static List<List<T>> PermutationsOfSize<T>(this IEnumerable<T> seed, int size)
	{
		if (seed.Count() < size)
		{
			return new List<List<T>>();
		}
		return seed.PermutationsOfSize(new List<T>(), size).ToList();
	}

	private static IEnumerable<List<T>> PermutationsOfSize<T>(this IEnumerable<T> seed, List<T> set, int size)
	{
		if (size == 0)
		{
			foreach (List<T> item in set.AllPermutations())
			{
				yield return item;
			}
			yield break;
		}
		List<T> seedAsList = seed.ToList();
		for (int i = 0; i < seedAsList.Count; i++)
		{
			List<T> set2 = new List<T>(set) { seedAsList[i] };
			foreach (List<T> item2 in seedAsList.Skip(i + 1).PermutationsOfSize(set2, size - 1))
			{
				yield return item2;
			}
		}
	}

	private static IEnumerable<List<T>> Permute<T>(List<T> set, int start, int end)
	{
		if (start == end)
		{
			yield return new List<T>(set);
			yield break;
		}
		for (int i = start; i <= end; i++)
		{
			Swap(set, start, i);
			foreach (List<T> item in Permute(set, start + 1, end))
			{
				yield return item;
			}
			Swap(set, start, i);
		}
	}

	private static void Swap<T>(List<T> set, int a, int b)
	{
		T value = set[a];
		set[a] = set[b];
		set[b] = value;
	}

	public static IEnumerable<List<T>> Cycles<T>(IEnumerable<T> seed)
	{
		LinkedList<T> set = new LinkedList<T>(seed);
		for (int i = 0; i < set.Count; i++)
		{
			yield return new List<T>(set);
			T value = Enumerable.First(set);
			set.RemoveFirst();
			set.AddLast(value);
		}
	}

	public static bool IsPermutationOf<T>(this IEnumerable<T> set, IEnumerable<T> other)
	{
		return new HashSet<T>(set).SetEquals(other);
	}
}
