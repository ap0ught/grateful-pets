using System;
using System.Collections;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public class OrderedSet<T> : CollectionBase<T>, ICollection<T>, IEnumerable<T>, IEnumerable, ICloneable
{
	[Serializable]
	private class ListView : ReadOnlyListBase<T>
	{
		private OrderedSet<T> mySet;

		private RedBlackTree<T>.RangeTester rangeTester;

		private bool entireTree;

		private bool reversed;

		public override int Count
		{
			get
			{
				if (entireTree)
				{
					return mySet.Count;
				}
				return mySet.tree.CountRange(rangeTester);
			}
		}

		public override T this[int index]
		{
			get
			{
				if (entireTree)
				{
					if (reversed)
					{
						return mySet[mySet.Count - 1 - index];
					}
					return mySet[index];
				}
				int num = mySet.tree.FirstItemInRange(rangeTester, out var item);
				int num2 = mySet.tree.LastItemInRange(rangeTester, out item);
				if (num < 0 || num2 < 0 || index < 0 || index >= num2 - num + 1)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (reversed)
				{
					return mySet[num2 - index];
				}
				return mySet[num + index];
			}
		}

		public ListView(OrderedSet<T> mySet, RedBlackTree<T>.RangeTester rangeTester, bool entireTree, bool reversed)
		{
			this.mySet = mySet;
			this.rangeTester = rangeTester;
			this.entireTree = entireTree;
			this.reversed = reversed;
		}

		public override int IndexOf(T item)
		{
			if (entireTree)
			{
				if (reversed)
				{
					return mySet.Count - 1 - mySet.IndexOf(item);
				}
				return mySet.IndexOf(item);
			}
			if (rangeTester(item) != 0)
			{
				return -1;
			}
			T item2;
			if (reversed)
			{
				int num = mySet.tree.FindIndex(item, findFirst: false);
				if (num < 0)
				{
					return -1;
				}
				return mySet.tree.LastItemInRange(rangeTester, out item2) - num;
			}
			int num2 = mySet.tree.FindIndex(item, findFirst: true);
			if (num2 < 0)
			{
				return -1;
			}
			int num3 = mySet.tree.FirstItemInRange(rangeTester, out item2);
			return num2 - num3;
		}
	}

	[Serializable]
	public class View : CollectionBase<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private OrderedSet<T> mySet;

		private RedBlackTree<T>.RangeTester rangeTester;

		private bool entireTree;

		private bool reversed;

		public sealed override int Count
		{
			get
			{
				if (entireTree)
				{
					return mySet.Count;
				}
				return mySet.tree.CountRange(rangeTester);
			}
		}

		public T this[int index]
		{
			get
			{
				if (entireTree)
				{
					if (reversed)
					{
						return mySet[mySet.Count - 1 - index];
					}
					return mySet[index];
				}
				int num = mySet.tree.FirstItemInRange(rangeTester, out var item);
				int num2 = mySet.tree.LastItemInRange(rangeTester, out item);
				if (num < 0 || num2 < 0 || index < 0 || index >= num2 - num + 1)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (reversed)
				{
					return mySet[num2 - index];
				}
				return mySet[num + index];
			}
		}

		internal View(OrderedSet<T> mySet, RedBlackTree<T>.RangeTester rangeTester, bool entireTree, bool reversed)
		{
			this.mySet = mySet;
			this.rangeTester = rangeTester;
			this.entireTree = entireTree;
			this.reversed = reversed;
		}

		private bool ItemInView(T item)
		{
			return rangeTester(item) == 0;
		}

		public sealed override IEnumerator<T> GetEnumerator()
		{
			if (reversed)
			{
				return mySet.tree.EnumerateRangeReversed(rangeTester).GetEnumerator();
			}
			return mySet.tree.EnumerateRange(rangeTester).GetEnumerator();
		}

		public sealed override void Clear()
		{
			if (entireTree)
			{
				mySet.Clear();
			}
			else
			{
				mySet.tree.DeleteRange(rangeTester);
			}
		}

		public new bool Add(T item)
		{
			if (!ItemInView(item))
			{
				throw new ArgumentException(Strings.OutOfViewRange, "item");
			}
			return mySet.Add(item);
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		public sealed override bool Remove(T item)
		{
			if (!ItemInView(item))
			{
				return false;
			}
			return mySet.Remove(item);
		}

		public sealed override bool Contains(T item)
		{
			if (!ItemInView(item))
			{
				return false;
			}
			return mySet.Contains(item);
		}

		public int IndexOf(T item)
		{
			if (entireTree)
			{
				if (reversed)
				{
					int num = mySet.tree.FindIndex(item, findFirst: false);
					if (num < 0)
					{
						return -1;
					}
					return mySet.Count - 1 - num;
				}
				return mySet.tree.FindIndex(item, findFirst: true);
			}
			if (!ItemInView(item))
			{
				return -1;
			}
			T item2;
			if (reversed)
			{
				int num2 = mySet.tree.FindIndex(item, findFirst: false);
				if (num2 < 0)
				{
					return -1;
				}
				return mySet.tree.LastItemInRange(rangeTester, out item2) - num2;
			}
			int num3 = mySet.tree.FindIndex(item, findFirst: true);
			if (num3 < 0)
			{
				return -1;
			}
			int num4 = mySet.tree.FirstItemInRange(rangeTester, out item2);
			return num3 - num4;
		}

		public IList<T> AsList()
		{
			return new ListView(mySet, rangeTester, entireTree, reversed);
		}

		public View Reversed()
		{
			return new View(mySet, rangeTester, entireTree, !reversed);
		}

		public T GetFirst()
		{
			int num = ((!reversed) ? mySet.tree.FirstItemInRange(rangeTester, out var item) : mySet.tree.LastItemInRange(rangeTester, out item));
			if (num < 0)
			{
				throw new InvalidOperationException(Strings.CollectionIsEmpty);
			}
			return item;
		}

		public T GetLast()
		{
			int num = ((!reversed) ? mySet.tree.LastItemInRange(rangeTester, out var item) : mySet.tree.FirstItemInRange(rangeTester, out item));
			if (num < 0)
			{
				throw new InvalidOperationException(Strings.CollectionIsEmpty);
			}
			return item;
		}
	}

	private IComparer<T> comparer;

	private RedBlackTree<T> tree;

	public IComparer<T> Comparer => comparer;

	public sealed override int Count => tree.ElementCount;

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return tree.GetItemByIndex(index);
		}
	}

	public OrderedSet()
		: this(Comparers.DefaultComparer<T>())
	{
	}

	public OrderedSet(Comparison<T> comparison)
		: this(Comparers.ComparerFromComparison(comparison))
	{
	}

	public OrderedSet(IComparer<T> comparer)
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		this.comparer = comparer;
		tree = new RedBlackTree<T>(comparer);
	}

	public OrderedSet(IEnumerable<T> collection)
		: this(collection, Comparers.DefaultComparer<T>())
	{
	}

	public OrderedSet(IEnumerable<T> collection, Comparison<T> comparison)
		: this(collection, Comparers.ComparerFromComparison(comparison))
	{
	}

	public OrderedSet(IEnumerable<T> collection, IComparer<T> comparer)
		: this(comparer)
	{
		AddMany(collection);
	}

	private OrderedSet(IComparer<T> comparer, RedBlackTree<T> tree)
	{
		this.comparer = comparer;
		this.tree = tree;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public OrderedSet<T> Clone()
	{
		return new OrderedSet<T>(comparer, tree.Clone());
	}

	public OrderedSet<T> CloneContents()
	{
		if (!Util.IsCloneableType(typeof(T), out var isValue))
		{
			throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof(T).FullName));
		}
		OrderedSet<T> orderedSet = new OrderedSet<T>(comparer);
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			T item;
			if (isValue)
			{
				item = current;
			}
			else
			{
				item = ((current != null) ? ((T)((ICloneable)(object)current).Clone()) : default(T));
			}
			orderedSet.Add(item);
		}
		return orderedSet;
	}

	public sealed override IEnumerator<T> GetEnumerator()
	{
		return tree.GetEnumerator();
	}

	public sealed override bool Contains(T item)
	{
		T item2;
		return tree.Find(item, findFirst: false, replace: false, out item2);
	}

	public bool TryGetItem(T item, out T foundItem)
	{
		return tree.Find(item, findFirst: true, replace: false, out foundItem);
	}

	public int IndexOf(T item)
	{
		return tree.FindIndex(item, findFirst: true);
	}

	public new bool Add(T item)
	{
		T previous;
		return !tree.Insert(item, DuplicatePolicy.ReplaceFirst, out previous);
	}

	void ICollection<T>.Add(T item)
	{
		Add(item);
	}

	public void AddMany(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (collection == this)
		{
			return;
		}
		foreach (T item in collection)
		{
			Add(item);
		}
	}

	public sealed override bool Remove(T item)
	{
		T item2;
		return tree.Delete(item, deleteFirst: true, out item2);
	}

	public int RemoveMany(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		int num = 0;
		if (collection == this)
		{
			num = Count;
			Clear();
		}
		else
		{
			foreach (T item in collection)
			{
				if (Remove(item))
				{
					num++;
				}
			}
		}
		return num;
	}

	public sealed override void Clear()
	{
		tree.StopEnumerations();
		tree = new RedBlackTree<T>(comparer);
	}

	private void CheckEmpty()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException(Strings.CollectionIsEmpty);
		}
	}

	public T GetFirst()
	{
		CheckEmpty();
		tree.FirstItemInRange(tree.EntireRangeTester, out var item);
		return item;
	}

	public T GetLast()
	{
		CheckEmpty();
		tree.LastItemInRange(tree.EntireRangeTester, out var item);
		return item;
	}

	public T RemoveFirst()
	{
		CheckEmpty();
		tree.DeleteItemFromRange(tree.EntireRangeTester, deleteFirst: true, out var item);
		return item;
	}

	public T RemoveLast()
	{
		CheckEmpty();
		tree.DeleteItemFromRange(tree.EntireRangeTester, deleteFirst: false, out var item);
		return item;
	}

	private void CheckConsistentComparison(OrderedSet<T> otherSet)
	{
		if (otherSet == null)
		{
			throw new ArgumentNullException("otherSet");
		}
		if (!object.Equals(comparer, otherSet.comparer))
		{
			throw new InvalidOperationException(Strings.InconsistentComparisons);
		}
	}

	public bool IsSupersetOf(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		if (otherSet.Count > Count)
		{
			return false;
		}
		foreach (T item in otherSet)
		{
			if (!Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsProperSupersetOf(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		if (otherSet.Count >= Count)
		{
			return false;
		}
		return IsSupersetOf(otherSet);
	}

	public bool IsSubsetOf(OrderedSet<T> otherSet)
	{
		return otherSet.IsSupersetOf(this);
	}

	public bool IsProperSubsetOf(OrderedSet<T> otherSet)
	{
		return otherSet.IsProperSupersetOf(this);
	}

	public bool IsEqualTo(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		if (otherSet.Count != Count)
		{
			return false;
		}
		using IEnumerator<T> enumerator = GetEnumerator();
		using IEnumerator<T> enumerator2 = otherSet.GetEnumerator();
		bool flag;
		bool flag2;
		while (true)
		{
			flag = enumerator.MoveNext();
			flag2 = enumerator2.MoveNext();
			if (!flag || !flag2)
			{
				break;
			}
			if (comparer.Compare(enumerator.Current, enumerator2.Current) != 0)
			{
				return false;
			}
		}
		return flag == flag2;
	}

	public void UnionWith(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		AddMany(otherSet);
	}

	public bool IsDisjointFrom(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		OrderedSet<T> orderedSet;
		OrderedSet<T> orderedSet2;
		if (otherSet.Count > Count)
		{
			orderedSet = this;
			orderedSet2 = otherSet;
		}
		else
		{
			orderedSet = otherSet;
			orderedSet2 = this;
		}
		foreach (T item in orderedSet)
		{
			if (orderedSet2.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public OrderedSet<T> Union(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		OrderedSet<T> collection;
		OrderedSet<T> orderedSet;
		if (otherSet.Count > Count)
		{
			collection = this;
			orderedSet = otherSet;
		}
		else
		{
			collection = otherSet;
			orderedSet = this;
		}
		OrderedSet<T> orderedSet2 = orderedSet.Clone();
		orderedSet2.AddMany(collection);
		return orderedSet2;
	}

	public void IntersectionWith(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		tree.StopEnumerations();
		OrderedSet<T> orderedSet;
		OrderedSet<T> orderedSet2;
		if (otherSet.Count > Count)
		{
			orderedSet = this;
			orderedSet2 = otherSet;
		}
		else
		{
			orderedSet = otherSet;
			orderedSet2 = this;
		}
		RedBlackTree<T> redBlackTree = new RedBlackTree<T>(comparer);
		foreach (T item in orderedSet)
		{
			if (orderedSet2.Contains(item))
			{
				redBlackTree.Insert(item, DuplicatePolicy.ReplaceFirst, out var _);
			}
		}
		tree = redBlackTree;
	}

	public OrderedSet<T> Intersection(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		OrderedSet<T> orderedSet;
		OrderedSet<T> orderedSet2;
		if (otherSet.Count > Count)
		{
			orderedSet = this;
			orderedSet2 = otherSet;
		}
		else
		{
			orderedSet = otherSet;
			orderedSet2 = this;
		}
		OrderedSet<T> orderedSet3 = new OrderedSet<T>(comparer);
		foreach (T item in orderedSet)
		{
			if (orderedSet2.Contains(item))
			{
				orderedSet3.Add(item);
			}
		}
		return orderedSet3;
	}

	public void DifferenceWith(OrderedSet<T> otherSet)
	{
		if (this == otherSet)
		{
			Clear();
		}
		CheckConsistentComparison(otherSet);
		if (otherSet.Count < Count)
		{
			foreach (T item in otherSet)
			{
				Remove(item);
			}
			return;
		}
		RemoveAll((T item) => otherSet.Contains(item));
	}

	public OrderedSet<T> Difference(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		OrderedSet<T> orderedSet = Clone();
		orderedSet.DifferenceWith(otherSet);
		return orderedSet;
	}

	public void SymmetricDifferenceWith(OrderedSet<T> otherSet)
	{
		if (this == otherSet)
		{
			Clear();
		}
		CheckConsistentComparison(otherSet);
		foreach (T item in otherSet)
		{
			if (Contains(item))
			{
				Remove(item);
			}
			else
			{
				Add(item);
			}
		}
	}

	public OrderedSet<T> SymmetricDifference(OrderedSet<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		OrderedSet<T> orderedSet;
		OrderedSet<T> orderedSet2;
		if (otherSet.Count > Count)
		{
			orderedSet = this;
			orderedSet2 = otherSet;
		}
		else
		{
			orderedSet = otherSet;
			orderedSet2 = this;
		}
		OrderedSet<T> orderedSet3 = orderedSet2.Clone();
		foreach (T item in orderedSet)
		{
			if (orderedSet3.Contains(item))
			{
				orderedSet3.Remove(item);
			}
			else
			{
				orderedSet3.Add(item);
			}
		}
		return orderedSet3;
	}

	public IList<T> AsList()
	{
		return new ListView(this, tree.EntireRangeTester, entireTree: true, reversed: false);
	}

	public View Reversed()
	{
		return new View(this, tree.EntireRangeTester, entireTree: true, reversed: true);
	}

	public View Range(T from, bool fromInclusive, T to, bool toInclusive)
	{
		return new View(this, tree.DoubleBoundedRangeTester(from, fromInclusive, to, toInclusive), entireTree: false, reversed: false);
	}

	public View RangeFrom(T from, bool fromInclusive)
	{
		return new View(this, tree.LowerBoundedRangeTester(from, fromInclusive), entireTree: false, reversed: false);
	}

	public View RangeTo(T to, bool toInclusive)
	{
		return new View(this, tree.UpperBoundedRangeTester(to, toInclusive), entireTree: false, reversed: false);
	}
}
