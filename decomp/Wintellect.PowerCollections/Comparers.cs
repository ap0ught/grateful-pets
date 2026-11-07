using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

internal static class Comparers
{
	[Serializable]
	private class KeyValueEqualityComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
	{
		private IEqualityComparer<TKey> keyEqualityComparer;

		public KeyValueEqualityComparer(IEqualityComparer<TKey> keyEqualityComparer)
		{
			this.keyEqualityComparer = keyEqualityComparer;
		}

		public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
		{
			return keyEqualityComparer.Equals(x.Key, y.Key);
		}

		public int GetHashCode(KeyValuePair<TKey, TValue> obj)
		{
			return Util.GetHashCode(obj.Key, keyEqualityComparer);
		}

		public override bool Equals(object obj)
		{
			if (obj is KeyValueEqualityComparer<TKey, TValue>)
			{
				return object.Equals(keyEqualityComparer, ((KeyValueEqualityComparer<TKey, TValue>)obj).keyEqualityComparer);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return keyEqualityComparer.GetHashCode();
		}
	}

	[Serializable]
	private class KeyValueComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
	{
		private IComparer<TKey> keyComparer;

		public KeyValueComparer(IComparer<TKey> keyComparer)
		{
			this.keyComparer = keyComparer;
		}

		public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
		{
			return keyComparer.Compare(x.Key, y.Key);
		}

		public override bool Equals(object obj)
		{
			if (obj is KeyValueComparer<TKey, TValue>)
			{
				return object.Equals(keyComparer, ((KeyValueComparer<TKey, TValue>)obj).keyComparer);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return keyComparer.GetHashCode();
		}
	}

	[Serializable]
	private class PairComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
	{
		private IComparer<TKey> keyComparer;

		private IComparer<TValue> valueComparer;

		public PairComparer(IComparer<TKey> keyComparer, IComparer<TValue> valueComparer)
		{
			this.keyComparer = keyComparer;
			this.valueComparer = valueComparer;
		}

		public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
		{
			int num = keyComparer.Compare(x.Key, y.Key);
			if (num == 0)
			{
				return valueComparer.Compare(x.Value, y.Value);
			}
			return num;
		}

		public override bool Equals(object obj)
		{
			if (obj is PairComparer<TKey, TValue>)
			{
				if (object.Equals(keyComparer, ((PairComparer<TKey, TValue>)obj).keyComparer))
				{
					return object.Equals(valueComparer, ((PairComparer<TKey, TValue>)obj).valueComparer);
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return keyComparer.GetHashCode() ^ valueComparer.GetHashCode();
		}
	}

	[Serializable]
	private class ComparisonComparer<T> : IComparer<T>
	{
		private Comparison<T> comparison;

		public ComparisonComparer(Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		public int Compare(T x, T y)
		{
			return comparison(x, y);
		}

		public override bool Equals(object obj)
		{
			if (obj is ComparisonComparer<T>)
			{
				return comparison.Equals(((ComparisonComparer<T>)obj).comparison);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return comparison.GetHashCode();
		}
	}

	[Serializable]
	private class ComparisonKeyValueComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
	{
		private Comparison<TKey> comparison;

		public ComparisonKeyValueComparer(Comparison<TKey> comparison)
		{
			this.comparison = comparison;
		}

		public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
		{
			return comparison(x.Key, y.Key);
		}

		public override bool Equals(object obj)
		{
			if (obj is ComparisonKeyValueComparer<TKey, TValue>)
			{
				return comparison.Equals(((ComparisonKeyValueComparer<TKey, TValue>)obj).comparison);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return comparison.GetHashCode();
		}
	}

	public static IComparer<T> ComparerFromComparison<T>(Comparison<T> comparison)
	{
		if (comparison == null)
		{
			throw new ArgumentNullException("comparison");
		}
		return new ComparisonComparer<T>(comparison);
	}

	public static IComparer<KeyValuePair<TKey, TValue>> ComparerKeyValueFromComparerKey<TKey, TValue>(IComparer<TKey> keyComparer)
	{
		if (keyComparer == null)
		{
			throw new ArgumentNullException("keyComparer");
		}
		return new KeyValueComparer<TKey, TValue>(keyComparer);
	}

	public static IEqualityComparer<KeyValuePair<TKey, TValue>> EqualityComparerKeyValueFromComparerKey<TKey, TValue>(IEqualityComparer<TKey> keyEqualityComparer)
	{
		if (keyEqualityComparer == null)
		{
			throw new ArgumentNullException("keyEqualityComparer");
		}
		return new KeyValueEqualityComparer<TKey, TValue>(keyEqualityComparer);
	}

	public static IComparer<KeyValuePair<TKey, TValue>> ComparerPairFromKeyValueComparers<TKey, TValue>(IComparer<TKey> keyComparer, IComparer<TValue> valueComparer)
	{
		if (keyComparer == null)
		{
			throw new ArgumentNullException("keyComparer");
		}
		if (valueComparer == null)
		{
			throw new ArgumentNullException("valueComparer");
		}
		return new PairComparer<TKey, TValue>(keyComparer, valueComparer);
	}

	public static IComparer<KeyValuePair<TKey, TValue>> ComparerKeyValueFromComparisonKey<TKey, TValue>(Comparison<TKey> keyComparison)
	{
		if (keyComparison == null)
		{
			throw new ArgumentNullException("keyComparison");
		}
		return new ComparisonKeyValueComparer<TKey, TValue>(keyComparison);
	}

	public static IComparer<T> DefaultComparer<T>()
	{
		if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)) || typeof(IComparable).IsAssignableFrom(typeof(T)))
		{
			return Comparer<T>.Default;
		}
		throw new InvalidOperationException(string.Format(Strings.UncomparableType, typeof(T).FullName));
	}

	public static IComparer<KeyValuePair<TKey, TValue>> DefaultKeyValueComparer<TKey, TValue>()
	{
		return ComparerKeyValueFromComparerKey<TKey, TValue>(DefaultComparer<TKey>());
	}
}
