using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public class OrderedDictionary<TKey, TValue> : DictionaryBase<TKey, TValue>, ICloneable
{
	[Serializable]
	public class View : DictionaryBase<TKey, TValue>
	{
		private OrderedDictionary<TKey, TValue> myDictionary;

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
				return myDictionary.tree.CountRange(rangeTester);
			}
		}

		public sealed override TValue this[TKey key]
		{
			get
			{
				return base[key];
			}
			set
			{
				if (!KeyInView(key))
				{
					throw new ArgumentException(Strings.OutOfViewRange, "key");
				}
				myDictionary[key] = value;
			}
		}

		internal View(OrderedDictionary<TKey, TValue> myDictionary, RedBlackTree<KeyValuePair<TKey, TValue>>.RangeTester rangeTester, bool entireTree, bool reversed)
		{
			this.myDictionary = myDictionary;
			this.rangeTester = rangeTester;
			this.entireTree = entireTree;
			this.reversed = reversed;
		}

		private bool KeyInView(TKey key)
		{
			return rangeTester(OrderedDictionary<TKey, TValue>.NewPair(key, default(TValue))) == 0;
		}

		public sealed override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			if (reversed)
			{
				return myDictionary.tree.EnumerateRangeReversed(rangeTester).GetEnumerator();
			}
			return myDictionary.tree.EnumerateRange(rangeTester).GetEnumerator();
		}

		public sealed override bool ContainsKey(TKey key)
		{
			if (!KeyInView(key))
			{
				return false;
			}
			return myDictionary.ContainsKey(key);
		}

		public sealed override bool TryGetValue(TKey key, out TValue value)
		{
			if (!KeyInView(key))
			{
				value = default;
				return false;
			}
			return myDictionary.TryGetValue(key, out value);
		}

		public sealed override bool Remove(TKey key)
		{
			if (!KeyInView(key))
			{
				return false;
			}
			return myDictionary.Remove(key);
		}

		public sealed override void Clear()
		{
			if (entireTree)
			{
				myDictionary.Clear();
			}
			else
			{
				myDictionary.tree.DeleteRange(rangeTester);
			}
		}

		public View Reversed()
		{
			return new View(myDictionary, rangeTester, entireTree, !reversed);
		}
	}

	private IComparer<TKey> keyComparer;

	private IComparer<KeyValuePair<TKey, TValue>> pairComparer;

	private RedBlackTree<KeyValuePair<TKey, TValue>> tree;

	public IComparer<TKey> Comparer => keyComparer;

	public sealed override TValue this[TKey key]
	{
		get
		{
			if (tree.Find(NewPair(key), findFirst: false, replace: false, out var item))
			{
				return item.Value;
			}
			throw new KeyNotFoundException(Strings.KeyNotFound);
		}
		set
		{
			tree.Insert(NewPair(key, value), DuplicatePolicy.ReplaceLast, out var _);
		}
	}

	public sealed override int Count => tree.ElementCount;

	private static KeyValuePair<TKey, TValue> NewPair(TKey key, TValue value)
	{
		return new KeyValuePair<TKey, TValue>(key, value);
	}

	private static KeyValuePair<TKey, TValue> NewPair(TKey key)
	{
		return new KeyValuePair<TKey, TValue>(key, default);
	}

	public OrderedDictionary()
		: this(Comparers.DefaultComparer<TKey>())
	{
	}

	public OrderedDictionary(IComparer<TKey> comparer)
		: this((IEnumerable<KeyValuePair<TKey, TValue>>)null, comparer, Comparers.ComparerKeyValueFromComparerKey<TKey, TValue>(comparer))
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
	}

	public OrderedDictionary(Comparison<TKey> comparison)
		: this((IEnumerable<KeyValuePair<TKey, TValue>>)null, Comparers.ComparerFromComparison(comparison), Comparers.ComparerKeyValueFromComparisonKey<TKey, TValue>(comparison))
	{
	}

	public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues)
		: this(keysAndValues, Comparers.DefaultComparer<TKey>())
	{
	}

	public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues, IComparer<TKey> comparer)
		: this(keysAndValues, comparer, Comparers.ComparerKeyValueFromComparerKey<TKey, TValue>(comparer))
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
	}

	public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues, Comparison<TKey> comparison)
		: this(keysAndValues, Comparers.ComparerFromComparison(comparison), Comparers.ComparerKeyValueFromComparisonKey<TKey, TValue>(comparison))
	{
	}

	private OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues, IComparer<TKey> keyComparer, IComparer<KeyValuePair<TKey, TValue>> pairComparer)
	{
		this.keyComparer = keyComparer;
		this.pairComparer = pairComparer;
		tree = new RedBlackTree<KeyValuePair<TKey, TValue>>(this.pairComparer);
		if (keysAndValues != null)
		{
			AddMany(keysAndValues);
		}
	}

	private OrderedDictionary(IComparer<TKey> keyComparer, IComparer<KeyValuePair<TKey, TValue>> pairComparer, RedBlackTree<KeyValuePair<TKey, TValue>> tree)
	{
		this.keyComparer = keyComparer;
		this.pairComparer = pairComparer;
		this.tree = tree;
	}

	public OrderedDictionary<TKey, TValue> Clone()
	{
		return new OrderedDictionary<TKey, TValue>(keyComparer, pairComparer, tree.Clone());
	}

	private void NonCloneableType(Type t)
	{
		throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, t.FullName));
	}

	public OrderedDictionary<TKey, TValue> CloneContents()
	{
		if (!Util.IsCloneableType(typeof(TKey), out var isValue))
		{
			NonCloneableType(typeof(TKey));
		}
		if (!Util.IsCloneableType(typeof(TValue), out var isValue2))
		{
			NonCloneableType(typeof(TValue));
		}
		OrderedDictionary<TKey, TValue> orderedDictionary = new OrderedDictionary<TKey, TValue>(null, keyComparer, pairComparer);
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
			orderedDictionary.Add(key, value);
		}
		return orderedDictionary;
	}

	public View Reversed()
	{
		return new View(this, tree.EntireRangeTester, entireTree: true, reversed: true);
	}

	public View Range(TKey from, bool fromInclusive, TKey to, bool toInclusive)
	{
		return new View(this, tree.DoubleBoundedRangeTester(NewPair(from), fromInclusive, NewPair(to), toInclusive), entireTree: false, reversed: false);
	}

	public View RangeFrom(TKey from, bool fromInclusive)
	{
		return new View(this, tree.LowerBoundedRangeTester(NewPair(from), fromInclusive), entireTree: false, reversed: false);
	}

	public View RangeTo(TKey to, bool toInclusive)
	{
		return new View(this, tree.UpperBoundedRangeTester(NewPair(to), toInclusive), entireTree: false, reversed: false);
	}

	public sealed override bool Remove(TKey key)
	{
		KeyValuePair<TKey, TValue> key2 = NewPair(key);
		KeyValuePair<TKey, TValue> item;
		return tree.Delete(key2, deleteFirst: true, out item);
	}

	public sealed override void Clear()
	{
		tree.StopEnumerations();
		tree = new RedBlackTree<KeyValuePair<TKey, TValue>>(pairComparer);
	}

	public bool GetValueElseAdd(TKey key, ref TValue value)
	{
		KeyValuePair<TKey, TValue> item = NewPair(key, value);
		bool flag = tree.Insert(item, DuplicatePolicy.DoNothing, out var previous);
		if (!flag)
		{
			value = previous.Value;
		}
		return !flag;
	}

	public sealed override void Add(TKey key, TValue value)
	{
		KeyValuePair<TKey, TValue> item = NewPair(key, value);
		if (!tree.Insert(item, DuplicatePolicy.DoNothing, out var _))
		{
			throw new ArgumentException(Strings.KeyAlreadyPresent, "key");
		}
	}

	public void Replace(TKey key, TValue value)
	{
		KeyValuePair<TKey, TValue> key2 = NewPair(key, value);
		if (!tree.Find(key2, findFirst: true, replace: true, out var _))
		{
			throw new KeyNotFoundException(Strings.KeyNotFound);
		}
	}

	public void AddMany(IEnumerable<KeyValuePair<TKey, TValue>> keysAndValues)
	{
		if (keysAndValues == null)
		{
			throw new ArgumentNullException("keysAndValues");
		}
		foreach (KeyValuePair<TKey, TValue> keysAndValue in keysAndValues)
		{
			this[keysAndValue.Key] = keysAndValue.Value;
		}
	}

	public int RemoveMany(IEnumerable<TKey> keyCollectionToRemove)
	{
		if (keyCollectionToRemove == null)
		{
			throw new ArgumentNullException("keyCollectionToRemove");
		}
		int num = 0;
		foreach (TKey item in keyCollectionToRemove)
		{
			if (Remove(item))
			{
				num++;
			}
		}
		return num;
	}

	public sealed override bool ContainsKey(TKey key)
	{
		KeyValuePair<TKey, TValue> item;
		return tree.Find(NewPair(key), findFirst: false, replace: false, out item);
	}

	public sealed override bool TryGetValue(TKey key, out TValue value)
	{
		bool result = tree.Find(NewPair(key), findFirst: false, replace: false, out var item);
		value = item.Value;
		return result;
	}

	public sealed override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return tree.GetEnumerator();
	}

	object ICloneable.Clone()
	{
		return Clone();
	}
}
