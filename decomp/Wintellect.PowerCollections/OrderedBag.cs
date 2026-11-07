using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public class OrderedBag<T> : CollectionBase<T>, ICloneable
{
	[Serializable]
	private class ListView : ReadOnlyListBase<T>
	{
		private OrderedBag<T> myBag;

		private RedBlackTree<T>.RangeTester rangeTester;

		private bool entireTree;

		private bool reversed;

		public sealed override int Count
		{
			get
			{
				if (entireTree)
				{
					return myBag.Count;
				}
				return myBag.tree.CountRange(rangeTester);
			}
		}

		public sealed override T this[int index]
		{
			get
			{
				if (entireTree)
				{
					if (reversed)
					{
						return myBag[myBag.Count - 1 - index];
					}
					return myBag[index];
				}
				int num = myBag.tree.FirstItemInRange(rangeTester, out var item);
				int num2 = myBag.tree.LastItemInRange(rangeTester, out item);
				if (num < 0 || num2 < 0 || index < 0 || index >= num2 - num + 1)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (reversed)
				{
					return myBag[num2 - index];
				}
				return myBag[num + index];
			}
			set
			{
				base[index] = value;
			}
		}

		public ListView(OrderedBag<T> myBag, RedBlackTree<T>.RangeTester rangeTester, bool entireTree, bool reversed)
		{
			this.myBag = myBag;
			this.rangeTester = rangeTester;
			this.entireTree = entireTree;
			this.reversed = reversed;
		}

		public sealed override int IndexOf(T item)
		{
			if (entireTree)
			{
				if (reversed)
				{
					return myBag.Count - 1 - myBag.LastIndexOf(item);
				}
				return myBag.IndexOf(item);
			}
			if (rangeTester(item) != 0)
			{
				return -1;
			}
			T item2;
			if (reversed)
			{
				int num = myBag.tree.FindIndex(item, findFirst: false);
				if (num < 0)
				{
					return -1;
				}
				return myBag.tree.LastItemInRange(rangeTester, out item2) - num;
			}
			int num2 = myBag.tree.FindIndex(item, findFirst: true);
			if (num2 < 0)
			{
				return -1;
			}
			int num3 = myBag.tree.FirstItemInRange(rangeTester, out item2);
			return num2 - num3;
		}
	}

	[Serializable]
	public class View : CollectionBase<T>
	{
		private OrderedBag<T> myBag;

		private RedBlackTree<T>.RangeTester rangeTester;

		private bool entireTree;

		private bool reversed;

		public sealed override int Count
		{
			get
			{
				if (entireTree)
				{
					return myBag.Count;
				}
				return myBag.tree.CountRange(rangeTester);
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
						return myBag[myBag.Count - 1 - index];
					}
					return myBag[index];
				}
				int num = myBag.tree.FirstItemInRange(rangeTester, out var item);
				int num2 = myBag.tree.LastItemInRange(rangeTester, out item);
				if (num < 0 || num2 < 0 || index < 0 || index >= num2 - num + 1)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (reversed)
				{
					return myBag[num2 - index];
				}
				return myBag[num + index];
			}
		}

		internal View(OrderedBag<T> myBag, RedBlackTree<T>.RangeTester rangeTester, bool entireTree, bool reversed)
		{
			this.myBag = myBag;
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
				return myBag.tree.EnumerateRangeReversed(rangeTester).GetEnumerator();
			}
			return myBag.tree.EnumerateRange(rangeTester).GetEnumerator();
		}

		public sealed override void Clear()
		{
			if (entireTree)
			{
				myBag.Clear();
			}
			else
			{
				myBag.tree.DeleteRange(rangeTester);
			}
		}

		public sealed override void Add(T item)
		{
			if (!ItemInView(item))
			{
				throw new ArgumentException(Strings.OutOfViewRange, "item");
			}
			myBag.Add(item);
		}

		public sealed override bool Remove(T item)
		{
			if (!ItemInView(item))
			{
				return false;
			}
			return myBag.Remove(item);
		}

		public sealed override bool Contains(T item)
		{
			if (!ItemInView(item))
			{
				return false;
			}
			return myBag.Contains(item);
		}

		public int IndexOf(T item)
		{
			if (entireTree)
			{
				if (reversed)
				{
					int num = myBag.tree.FindIndex(item, findFirst: false);
					if (num < 0)
					{
						return -1;
					}
					return myBag.Count - 1 - num;
				}
				return myBag.tree.FindIndex(item, findFirst: true);
			}
			if (!ItemInView(item))
			{
				return -1;
			}
			T item2;
			if (reversed)
			{
				int num2 = myBag.tree.FindIndex(item, findFirst: false);
				if (num2 < 0)
				{
					return -1;
				}
				return myBag.tree.LastItemInRange(rangeTester, out item2) - num2;
			}
			int num3 = myBag.tree.FindIndex(item, findFirst: true);
			if (num3 < 0)
			{
				return -1;
			}
			int num4 = myBag.tree.FirstItemInRange(rangeTester, out item2);
			return num3 - num4;
		}

		public int LastIndexOf(T item)
		{
			if (entireTree)
			{
				if (reversed)
				{
					int num = myBag.tree.FindIndex(item, findFirst: true);
					if (num < 0)
					{
						return -1;
					}
					return myBag.Count - 1 - num;
				}
				return myBag.tree.FindIndex(item, findFirst: false);
			}
			if (!ItemInView(item))
			{
				return -1;
			}
			T item2;
			if (reversed)
			{
				int num2 = myBag.tree.FindIndex(item, findFirst: true);
				if (num2 < 0)
				{
					return -1;
				}
				return myBag.tree.LastItemInRange(rangeTester, out item2) - num2;
			}
			int num3 = myBag.tree.FindIndex(item, findFirst: false);
			if (num3 < 0)
			{
				return -1;
			}
			int num4 = myBag.tree.FirstItemInRange(rangeTester, out item2);
			return num3 - num4;
		}

		public IList<T> AsList()
		{
			return new ListView(myBag, rangeTester, entireTree, reversed);
		}

		public View Reversed()
		{
			return new View(myBag, rangeTester, entireTree, !reversed);
		}

		public T GetFirst()
		{
			int num = ((!reversed) ? myBag.tree.FirstItemInRange(rangeTester, out var item) : myBag.tree.LastItemInRange(rangeTester, out item));
			if (num < 0)
			{
				throw new InvalidOperationException(Strings.CollectionIsEmpty);
			}
			return item;
		}

		public T GetLast()
		{
			int num = ((!reversed) ? myBag.tree.LastItemInRange(rangeTester, out var item) : myBag.tree.FirstItemInRange(rangeTester, out item));
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

	public OrderedBag()
		: this(Comparers.DefaultComparer<T>())
	{
	}

	public OrderedBag(Comparison<T> comparison)
		: this(Comparers.ComparerFromComparison(comparison))
	{
	}

	public OrderedBag(IComparer<T> comparer)
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		this.comparer = comparer;
		tree = new RedBlackTree<T>(comparer);
	}

	public OrderedBag(IEnumerable<T> collection)
		: this(collection, Comparers.DefaultComparer<T>())
	{
	}

	public OrderedBag(IEnumerable<T> collection, Comparison<T> comparison)
		: this(collection, Comparers.ComparerFromComparison(comparison))
	{
	}

	public OrderedBag(IEnumerable<T> collection, IComparer<T> comparer)
		: this(comparer)
	{
		AddMany(collection);
	}

	private OrderedBag(IComparer<T> comparer, RedBlackTree<T> tree)
	{
		this.comparer = comparer;
		this.tree = tree;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public OrderedBag<T> Clone()
	{
		return new OrderedBag<T>(comparer, tree.Clone());
	}

	public OrderedBag<T> CloneContents()
	{
		if (!Util.IsCloneableType(typeof(T), out var isValue))
		{
			throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof(T).FullName));
		}
		OrderedBag<T> orderedBag = new OrderedBag<T>(comparer);
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
			orderedBag.Add(item);
		}
		return orderedBag;
	}

	public int NumberOfCopies(T item)
	{
		return tree.CountRange(tree.EqualRangeTester(item));
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

	public IEnumerable<T> GetEqualItems(T item)
	{
		return tree.EnumerateRange(tree.EqualRangeTester(item));
	}

	public IEnumerable<T> DistinctItems()
	{
		T y = default;
		bool flag = true;
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T item = enumerator.Current;
			if (flag || comparer.Compare(item, y) != 0)
			{
				yield return item;
			}
			y = item;
			flag = false;
		}
	}

	public int LastIndexOf(T item)
	{
		return tree.FindIndex(item, findFirst: false);
	}

	public int IndexOf(T item)
	{
		return tree.FindIndex(item, findFirst: true);
	}

	public sealed override void Add(T item)
	{
		tree.Insert(item, DuplicatePolicy.InsertLast, out var _);
	}

	public void AddMany(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (this == collection)
		{
			collection = ToArray();
		}
		foreach (T item in collection)
		{
			Add(item);
		}
	}

	public sealed override bool Remove(T item)
	{
		T item2;
		return tree.Delete(item, deleteFirst: false, out item2);
	}

	public int RemoveAllCopies(T item)
	{
		return tree.DeleteRange(tree.EqualRangeTester(item));
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
		tree.Clear();
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

	private void CheckConsistentComparison(OrderedBag<T> otherBag)
	{
		if (otherBag == null)
		{
			throw new ArgumentNullException("otherBag");
		}
		if (!object.Equals(comparer, otherBag.comparer))
		{
			throw new InvalidOperationException(Strings.InconsistentComparisons);
		}
	}

	public bool IsSupersetOf(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		if (otherBag.Count > Count)
		{
			return false;
		}
		foreach (T item in otherBag.DistinctItems())
		{
			if (NumberOfCopies(item) < otherBag.NumberOfCopies(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsProperSupersetOf(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		if (otherBag.Count >= Count)
		{
			return false;
		}
		return IsSupersetOf(otherBag);
	}

	public bool IsSubsetOf(OrderedBag<T> otherBag)
	{
		return otherBag.IsSupersetOf(this);
	}

	public bool IsProperSubsetOf(OrderedBag<T> otherBag)
	{
		return otherBag.IsProperSupersetOf(this);
	}

	public bool IsDisjointFrom(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		OrderedBag<T> orderedBag;
		OrderedBag<T> orderedBag2;
		if (otherBag.Count > Count)
		{
			orderedBag = this;
			orderedBag2 = otherBag;
		}
		else
		{
			orderedBag = otherBag;
			orderedBag2 = this;
		}
		foreach (T item in orderedBag)
		{
			if (orderedBag2.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEqualTo(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		if (otherBag.Count != Count)
		{
			return false;
		}
		using IEnumerator<T> enumerator = GetEnumerator();
		using IEnumerator<T> enumerator2 = otherBag.GetEnumerator();
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

	public void UnionWith(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		T y = default;
		bool flag = true;
		int num = 0;
		int num2 = 0;
		foreach (T item in otherBag)
		{
			if (flag || comparer.Compare(item, y) != 0)
			{
				num = NumberOfCopies(item);
				num2 = 1;
			}
			else
			{
				num2++;
			}
			if (num2 > num)
			{
				Add(item);
			}
			y = item;
			flag = false;
		}
	}

	public OrderedBag<T> Union(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		OrderedBag<T> otherBag2;
		OrderedBag<T> orderedBag;
		if (otherBag.Count > Count)
		{
			otherBag2 = this;
			orderedBag = otherBag;
		}
		else
		{
			otherBag2 = otherBag;
			orderedBag = this;
		}
		OrderedBag<T> orderedBag2 = orderedBag.Clone();
		orderedBag2.UnionWith(otherBag2);
		return orderedBag2;
	}

	public void SumWith(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		AddMany(otherBag);
	}

	public OrderedBag<T> Sum(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		OrderedBag<T> collection;
		OrderedBag<T> orderedBag;
		if (otherBag.Count > Count)
		{
			collection = this;
			orderedBag = otherBag;
		}
		else
		{
			collection = otherBag;
			orderedBag = this;
		}
		OrderedBag<T> orderedBag2 = orderedBag.Clone();
		orderedBag2.AddMany(collection);
		return orderedBag2;
	}

	public void IntersectionWith(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		tree.StopEnumerations();
		OrderedBag<T> orderedBag;
		OrderedBag<T> orderedBag2;
		if (otherBag.Count > Count)
		{
			orderedBag = this;
			orderedBag2 = otherBag;
		}
		else
		{
			orderedBag = otherBag;
			orderedBag2 = this;
		}
		RedBlackTree<T> redBlackTree = new RedBlackTree<T>(comparer);
		T y = default;
		bool flag = true;
		int num = 0;
		int num2 = 0;
		foreach (T item in orderedBag)
		{
			if (flag || comparer.Compare(item, y) != 0)
			{
				num2 = orderedBag2.NumberOfCopies(item);
				num = 1;
			}
			else
			{
				num++;
			}
			if (num <= num2)
			{
				redBlackTree.Insert(item, DuplicatePolicy.InsertLast, out var _);
			}
			y = item;
			flag = false;
		}
		tree = redBlackTree;
	}

	public OrderedBag<T> Intersection(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		OrderedBag<T> orderedBag;
		OrderedBag<T> orderedBag2;
		if (otherBag.Count > Count)
		{
			orderedBag = this;
			orderedBag2 = otherBag;
		}
		else
		{
			orderedBag = otherBag;
			orderedBag2 = this;
		}
		T y = default;
		bool flag = true;
		int num = 0;
		int num2 = 0;
		OrderedBag<T> orderedBag3 = new OrderedBag<T>(comparer);
		foreach (T item in orderedBag)
		{
			if (flag || comparer.Compare(item, y) != 0)
			{
				num2 = orderedBag2.NumberOfCopies(item);
				num = 1;
			}
			else
			{
				num++;
			}
			if (num <= num2)
			{
				orderedBag3.Add(item);
			}
			y = item;
			flag = false;
		}
		return orderedBag3;
	}

	public void DifferenceWith(OrderedBag<T> otherBag)
	{
		if (this == otherBag)
		{
			Clear();
		}
		CheckConsistentComparison(otherBag);
		T y = default;
		bool flag = true;
		int num = 0;
		int num2 = 0;
		foreach (T item in otherBag)
		{
			if (flag || comparer.Compare(item, y) != 0)
			{
				num = NumberOfCopies(item);
				num2 = 1;
			}
			else
			{
				num2++;
			}
			if (num2 <= num)
			{
				Remove(item);
			}
			y = item;
			flag = false;
		}
	}

	public OrderedBag<T> Difference(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		OrderedBag<T> orderedBag = Clone();
		orderedBag.DifferenceWith(otherBag);
		return orderedBag;
	}

	public void SymmetricDifferenceWith(OrderedBag<T> otherBag)
	{
		tree = SymmetricDifference(otherBag).tree;
	}

	public OrderedBag<T> SymmetricDifference(OrderedBag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		OrderedBag<T> orderedBag = new OrderedBag<T>(comparer);
		IEnumerator<T> enumerator = GetEnumerator();
		IEnumerator<T> enumerator2 = otherBag.GetEnumerator();
		bool flag = enumerator.MoveNext();
		bool flag2 = enumerator2.MoveNext();
		while (true)
		{
			int num;
			if (flag)
			{
				num = (flag2 ? comparer.Compare(enumerator.Current, enumerator2.Current) : (-1));
			}
			else
			{
				if (!flag2)
				{
					break;
				}
				num = 1;
			}
			if (num == 0)
			{
				flag = enumerator.MoveNext();
				flag2 = enumerator2.MoveNext();
			}
			else if (num < 0)
			{
				orderedBag.Add(enumerator.Current);
				flag = enumerator.MoveNext();
			}
			else
			{
				orderedBag.Add(enumerator2.Current);
				flag2 = enumerator2.MoveNext();
			}
		}
		return orderedBag;
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
