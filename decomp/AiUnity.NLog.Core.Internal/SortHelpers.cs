using System.Collections.Generic;

namespace AiUnity.NLog.Core.Internal;

internal static class SortHelpers
{
	internal delegate TKey KeySelector<TValue, TKey>(TValue value);

	public static Dictionary<TKey, List<TValue>> BucketSort<TValue, TKey>(this IEnumerable<TValue> inputs, KeySelector<TValue, TKey> keySelector)
	{
		Dictionary<TKey, List<TValue>> dictionary = new Dictionary<TKey, List<TValue>>();
		foreach (TValue input in inputs)
		{
			TKey key = keySelector(input);
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = new List<TValue>();
				dictionary.Add(key, value);
			}
			value.Add(input);
		}
		return dictionary;
	}
}
