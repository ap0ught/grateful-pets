using System.Collections.Generic;

public static class CleanDictionaryExtensions
{
	public static bool SameAs<K, V>(this Dictionary<K, V> dict1, Dictionary<K, V> dict2)
	{
		if (dict1.Count != dict2.Count)
		{
			return false;
		}
		foreach (K key in dict1.Keys)
		{
			if (!dict2.ContainsKey(key) || !dict2[key].Equals(dict1[key]))
			{
				return false;
			}
		}
		return true;
	}
}
