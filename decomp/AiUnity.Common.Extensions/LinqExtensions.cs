using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUnity.Common.Extensions;

public static class LinqExtensions
{
	public static IEnumerable<T> MyAppend<T>(this IEnumerable<T> list, T item)
	{
		foreach (T item2 in list)
		{
			yield return item2;
		}
		yield return item;
	}

	public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
	{
		if (enumerable != null)
		{
			return !enumerable.Any();
		}
		return true;
	}

	public static IEnumerable<T> MyPrepend<T>(this IEnumerable<T> values, T value)
	{
		yield return value;
		foreach (T value2 in values)
		{
			yield return value2;
		}
	}

	public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count = 1)
	{
		IEnumerator<T> enumerator = source.GetEnumerator();
		Queue<T> queue = new Queue<T>(count + 1);
		while (enumerator.MoveNext())
		{
			queue.Enqueue(enumerator.Current);
			if (queue.Count > count)
			{
				yield return queue.Dequeue();
			}
		}
	}

	public static IEnumerable<TSource> SkipUntil<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		using IEnumerator<TSource> iterator = source.GetEnumerator();
		while (iterator.MoveNext() && !predicate(iterator.Current))
		{
		}
		while (iterator.MoveNext())
		{
			yield return iterator.Current;
		}
	}

	public static IEnumerable<T> Yield<T>(this T item)
	{
		if (item != null)
		{
			yield return item;
		}
	}

	public static void InsertRelative<T>(this IList<T> source, IList<T> reference, params T[] items)
	{
		foreach (T item in items)
		{
			int num = reference.IndexOf(item);
			int j;
			for (j = 0; j < Enumerable.Count(source) && (num < 0 || num >= reference.IndexOf(source[j])); j++)
			{
			}
			source.Insert(j, item);
		}
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			return defaultValueProvider();
		}
		return value;
	}

	public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
	{
		if (list2 == null || list1.Count() != list2.Count())
		{
			return false;
		}
		Dictionary<T, int> dictionary = new Dictionary<T, int>();
		foreach (T item in list1)
		{
			if (dictionary.ContainsKey(item))
			{
				dictionary[item]++;
			}
			else
			{
				dictionary.Add(item, 1);
			}
		}
		foreach (T item2 in list2)
		{
			if (dictionary.ContainsKey(item2))
			{
				dictionary[item2]--;
				continue;
			}
			return false;
		}
		return dictionary.Values.All((int c) => c == 0);
	}

	public static int IndexOf<T>(this IEnumerable<T> list, T item)
	{
		return list.Select((T x, int index) => (!EqualityComparer<T>.Default.Equals(item, x)) ? (-1) : index).FirstOrDefault((int x) => x != -1, -1);
	}

	public static T FirstOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T alternate)
	{
		return source.Where(predicate).FirstOrDefault(alternate);
	}

	public static T FirstOrDefault<T>(this IEnumerable<T> source, T alternate)
	{
		return source.DefaultIfEmpty(alternate).First();
	}
}
