using System.Collections.Generic;

namespace HistoryKit;

public static class HistoricDictionaryExtension
{
	public static void SetValue<K, V>(this Dictionary<K, V> list, K key, V value)
	{
		if (list.ContainsKey(key))
		{
			list[key] = value;
		}
		else
		{
			list.Add(key, value);
		}
	}
}
