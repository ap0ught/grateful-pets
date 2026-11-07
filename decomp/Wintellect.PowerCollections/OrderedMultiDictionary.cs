using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public class OrderedMultiDictionary<TKey, TValue> : MultiDictionaryBase<TKey, TValue>, ICloneable
{
	[Serializable]
	private sealed class KeyValuePairsCollection : ReadOnlyCollectionBase<KeyValuePair<TKey, TValue>>
	{
		private OrderedMultiDictionary<TKey, TValue> myDictionary;

		public override int Count => myDictionary.CountAllValues();

		public KeyValuePairsCollection(OrderedMultiDictionary<TKey, TValue> myDictionary)
		{
			this.myDictionary = myDictionary;
		}

		public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return myDictionary.tree.GetEnumerator();
		}

		public override bool Contains(KeyValuePair<TKey, TValue> pair)
		{
			KeyValuePair<TKey, TValue> item;
			return myDictionary.tree.Find(pair, findFirst: true, replace: false, out item);
		}
	}

	[Serializable]
	public class View : MultiDictionaryBase<TKey, TValue>
	{
		private OrderedMultiDictionary<TKey, TValue> myDictionary;

		private RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester rangeTester;

		private bool entireTree;

		private bool reversed;

		public sealed override int Count
		{
			get
			{
				if (entireTree)
				{
					return myDictionary.Count;
				}
				int num = 0;
				using IEnumerator<TKey> enumerator = myDictionary.EnumerateKeys(rangeTester, reversed);
				while (enumerator.MoveNext())
				{
					num++;
				}
				return num;
			}
		}

		internal View(OrderedMultiDictionary<TKey, TValue> myDictionary, RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester rangeTester, bool entireTree, bool reversed)
		{
			this.myDictionary = myDictionary;
			this.rangeTester = rangeTester;
			this.entireTree = entireTree;
			this.reversed = reversed;
		}

		private bool KeyInView(TKey key)
		{
			return rangeTester(OrderedMultiDictionary<TKey, TValue>.NewPair(key, default(TValue))) == 0;
		}

		protected sealed override IEnumerator<TKey> EnumerateKeys()
		{
			return myDictionary.EnumerateKeys(rangeTester, reversed);
		}

		protected sealed override bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values)
		{
			if (!KeyInView(key))
			{
				values = null;
				return false;
			}
			return myDictionary.TryEnumerateValuesForKey(key, out values);
		}

		public sealed override bool ContainsKey(TKey key)
		{
			if (!KeyInView(key))
			{
				return false;
			}
			return myDictionary.ContainsKey(key);
		}

		public sealed override bool Contains(TKey key, TValue value)
		{
			if (!KeyInView(key))
			{
				return false;
			}
			return myDictionary.Contains(key, value);
		}

		protected sealed override int CountValues(TKey key)
		{
			if (!KeyInView(key))
			{
				return 0;
			}
			return myDictionary.CountValues(key);
		}

		public sealed override void Add(TKey key, TValue value)
		{
			if (!KeyInView(key))
			{
				throw new ArgumentException(Strings.OutOfViewRange, "key");
			}
			myDictionary.Add(key, value);
		}

		public sealed override bool Remove(TKey key)
		{
			if (!KeyInView(key))
			{
				return false;
			}
			return myDictionary.Remove(key);
		}

		public sealed override bool Remove(TKey key, TValue value)
		{
			if (!KeyInView(key))
			{
				return false;
			}
			return myDictionary.Remove(key, value);
		}

		public sealed override void Clear()
		{
			if (entireTree)
			{
				myDictionary.Clear();
				return;
			}
			myDictionary.keyCount -= Count;
			myDictionary.tree.DeleteRange(rangeTester);
		}

		public View Reversed()
		{
			return new View(myDictionary, rangeTester, entireTree, !reversed);
		}
	}

	private IComparer<TKey> keyComparer;

	private IComparer<TValue> valueComparer;

	private IComparer<KeyValuePair<TKey, TValue>> comparer;

	private RedBlackTree<KeyValuePair<TKey, TValue>> tree;

	private bool allowDuplicateValues;

	private int keyCount;

	public IComparer<TKey> KeyComparer => keyComparer;

	public IComparer<TValue> ValueComparer => valueComparer;

	public sealed override int Count => keyCount;

	public sealed override ICollection<KeyValuePair<TKey, TValue>> KeyValuePairs => new KeyValuePairsCollection(this);

	private static KeyValuePair<TKey, TValue> NewPair(TKey key, TValue value)
	{
		return new KeyValuePair<TKey, TValue>(key, value);
	}

	private static KeyValuePair<TKey, TValue> NewPair(TKey key)
	{
		return new KeyValuePair<TKey, TValue>(key, default);
	}

	private RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester KeyRange(TKey key)
	{
		return (KeyValuePair<TKey, TValue> pair) => keyComparer.Compare(pair.Key, key);
	}

	private RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester DoubleBoundedKeyRangeTester(TKey first, bool firstInclusive, TKey last, bool lastInclusive)
	{
		return (KeyValuePair<TKey, TValue> pair) =>
		{
			if (firstInclusive)
			{
				if (keyComparer.Compare(first, pair.Key) > 0)
				{
					return -1;
				}
			}
			else if (keyComparer.Compare(first, pair.Key) >= 0)
			{
				return -1;
			}
			if (lastInclusive)
			{
				if (keyComparer.Compare(last, pair.Key) < 0)
				{
					return 1;
				}
			}
			else if (keyComparer.Compare(last, pair.Key) <= 0)
			{
				return 1;
			}
			return 0;
		};
	}

	private RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester LowerBoundedKeyRangeTester(TKey first, bool inclusive)
	{
		return (KeyValuePair<TKey, TValue> pair) =>
		{
			if (inclusive)
			{
				if (keyComparer.Compare(first, pair.Key) > 0)
				{
					return -1;
				}
				return 0;
			}
			return (keyComparer.Compare(first, pair.Key) >= 0) ? (-1) : 0;
		};
	}

	private RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester UpperBoundedKeyRangeTester(TKey last, bool inclusive)
	{
		return (KeyValuePair<TKey, TValue> pair) =>
		{
			if (inclusive)
			{
				if (keyComparer.Compare(last, pair.Key) < 0)
				{
					return 1;
				}
				return 0;
			}
			return (keyComparer.Compare(last, pair.Key) <= 0) ? 1 : 0;
		};
	}

	public OrderedMultiDictionary(bool allowDuplicateValues)
		: this(allowDuplicateValues, Comparers.DefaultComparer<TKey>(), Comparers.DefaultComparer<TValue>())
	{
	}

	public OrderedMultiDictionary(bool allowDuplicateValues, Comparison<TKey> keyComparison)
		: this(allowDuplicateValues, Comparers.ComparerFromComparison(keyComparison), Comparers.DefaultComparer<TValue>())
	{
	}

	public OrderedMultiDictionary(bool allowDuplicateValues, Comparison<TKey> keyComparison, Comparison<TValue> valueComparison)
		: this(allowDuplicateValues, Comparers.ComparerFromComparison(keyComparison), Comparers.ComparerFromComparison(valueComparison))
	{
	}

	public OrderedMultiDictionary(bool allowDuplicateValues, IComparer<TKey> keyComparer)
		: this(allowDuplicateValues, keyComparer, Comparers.DefaultComparer<TValue>())
	{
	}

	public OrderedMultiDictionary(bool allowDuplicateValues, IComparer<TKey> keyComparer, IComparer<TValue> valueComparer)
	{
		if (keyComparer == null)
		{
			throw new ArgumentNullException("keyComparer");
		}
		if (valueComparer == null)
		{
			throw new ArgumentNullException("valueComparer");
		}
		this.allowDuplicateValues = allowDuplicateValues;
		this.keyComparer = keyComparer;
		this.valueComparer = valueComparer;
		comparer = Comparers.ComparerPairFromKeyValueComparers(keyComparer, valueComparer);
		tree = new RedBlackTree<KeyValuePair<TKey, TValue>>(comparer);
	}

	private OrderedMultiDictionary(bool allowDuplicateValues, int keyCount, IComparer<TKey> keyComparer, IComparer<TValue> valueComparer, IComparer<KeyValuePair<TKey, TValue>> comparer, RedBlackTree<KeyValuePair<TKey, TValue>> tree)
	{
		this.allowDuplicateValues = allowDuplicateValues;
		this.keyCount = keyCount;
		this.keyComparer = keyComparer;
		this.valueComparer = valueComparer;
		this.comparer = comparer;
		this.tree = tree;
	}

	public sealed override void Add(TKey key, TValue value)
	{
		KeyValuePair<TKey, TValue> item = NewPair(key, value);
		if (!ContainsKey(key))
		{
			keyCount++;
		}
		tree.Insert(item, allowDuplicateValues ? DuplicatePolicy.InsertLast : DuplicatePolicy.ReplaceLast, out var _);
	}

	public sealed override bool Remove(TKey key, TValue value)
	{
		bool flag = tree.Delete(NewPair(key, value), deleteFirst: false, out var _);
		if (flag && !ContainsKey(key))
		{
			keyCount--;
		}
		return flag;
	}

	public sealed override bool Remove(TKey key)
	{
		if (tree.DeleteRange(KeyRange(key)) > 0)
		{
			keyCount--;
			return true;
		}
		return false;
	}

	public sealed override void Clear()
	{
		tree.StopEnumerations();
		tree = new RedBlackTree<KeyValuePair<TKey, TValue>>(comparer);
		keyCount = 0;
	}

	protected sealed override bool EqualValues(TValue value1, TValue value2)
	{
		return valueComparer.Compare(value1, value2) == 0;
	}

	public sealed override bool Contains(TKey key, TValue value)
	{
		KeyValuePair<TKey, TValue> item;
		return tree.Find(NewPair(key, value), findFirst: true, replace: false, out item);
	}

	public sealed override bool ContainsKey(TKey key)
	{
		KeyValuePair<TKey, TValue> item;
		return tree.FirstItemInRange(KeyRange(key), out item) >= 0;
	}

	private IEnumerator<TKey> EnumerateKeys(RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester rangeTester, bool reversed)
	{
		bool flag = true;
		TKey lastKey = default;
		IEnumerable<KeyValuePair<TKey, TValue>> enumerable = ((!reversed) ? tree.EnumerateRange(rangeTester) : tree.EnumerateRangeReversed(rangeTester));
		foreach (KeyValuePair<TKey, TValue> item in enumerable)
		{
			if (flag || keyComparer.Compare(lastKey, item.Key) != 0)
			{
				lastKey = item.Key;
				yield return lastKey;
			}
			flag = false;
		}
	}

	private IEnumerator<TValue> EnumerateValuesForKey(TKey key)
	{
		foreach (KeyValuePair<TKey, TValue> item in tree.EnumerateRange(KeyRange(key)))
		{
			yield return item.Value;
		}
	}

	protected sealed override bool TryEnumerateValuesForKey(TKey key, out IEnumerator<TValue> values)
	{
		if (ContainsKey(key))
		{
			values = EnumerateValuesForKey(key);
			return true;
		}
		values = null;
		return false;
	}

	protected sealed override IEnumerator<TKey> EnumerateKeys()
	{
		return EnumerateKeys(tree.EntireRangeTester, reversed: false);
	}

	protected sealed override int CountValues(TKey key)
	{
		return tree.CountRange(KeyRange(key));
	}

	protected sealed override int CountAllValues()
	{
		return tree.ElementCount;
	}

	public OrderedMultiDictionary<TKey, TValue> Clone()
	{
		return new OrderedMultiDictionary<TKey, TValue>(allowDuplicateValues, keyCount, keyComparer, valueComparer, comparer, tree.Clone());
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	private void NonCloneableType(Type t)
	{
		throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, t.FullName));
	}

	public OrderedMultiDictionary<TKey, TValue> CloneContents()
	{
		if (!Util.IsCloneableType(typeof(TKey), out var isValue))
		{
			NonCloneableType(typeof(TKey));
		}
		if (!Util.IsCloneableType(typeof(TValue), out var isValue2))
		{
			NonCloneableType(typeof(TValue));
		}
		OrderedMultiDictionary<TKey, TValue> orderedMultiDictionary = new OrderedMultiDictionary<TKey, TValue>(allowDuplicateValues, keyComparer, valueComparer);
		foreach (KeyValuePair<TKey, TValue> item in tree)
		{
			TKey key;
			if (isValue)
			{
				key = item.Key;
			}
			else
			{
				key = ((item.Key != null) ? ((TKey)((ICloneable)(object)item.Key).Clone()) : default(TKey));
			}
			TValue value;
			if (isValue2)
			{
				value = item.Value;
			}
			else
			{
				value = ((item.Value != null) ? ((TValue)((ICloneable)(object)item.Value).Clone()) : default(TValue));
			}
			orderedMultiDictionary.Add(key, value);
		}
		return orderedMultiDictionary;
	}

	public View Reversed()
	{
		return new View(this, tree.EntireRangeTester, entireTree: true, reversed: true);
	}

	public View Range(TKey from, bool fromInclusive, TKey to, bool toInclusive)
	{
		return new View(this, DoubleBoundedKeyRangeTester(from, fromInclusive, to, toInclusive), entireTree: false, reversed: false);
	}

	public View RangeFrom(TKey from, bool fromInclusive)
	{
		return new View(this, LowerBoundedKeyRangeTester(from, fromInclusive), entireTree: false, reversed: false);
	}

	public View RangeTo(TKey to, bool toInclusive)
	{
		return new View(this, UpperBoundedKeyRangeTester(to, toInclusive), entireTree: false, reversed: false);
	}
}
