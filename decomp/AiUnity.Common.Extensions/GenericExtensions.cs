using System;
using System.Collections.Generic;

namespace AiUnity.Common.Extensions;

public static class GenericExtensions
{
	public static bool AddUnique<T>(this List<T> list, T item)
	{
		if (!list.Exists((T p) => p.Equals(item)))
		{
			list.Add(item);
			return true;
		}
		return false;
	}

	public static V GetOrAdd<K, V>(this Dictionary<K, V> dictionary, K key, V defaultValue = default(V))
	{
		if (dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		dictionary.Add(key, defaultValue);
		return defaultValue;
	}

	public static bool IsNullOrEmpty(this Array array)
	{
		if (array != null)
		{
			return array.Length == 0;
		}
		return true;
	}
}
