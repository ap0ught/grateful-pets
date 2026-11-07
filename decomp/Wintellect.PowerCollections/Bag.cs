using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public class Bag<T> : CollectionBase<T>, ICloneable
{
	private IEqualityComparer<KeyValuePair<T, int>> equalityComparer;

	private IEqualityComparer<T> keyEqualityComparer;

	private Hash<KeyValuePair<T, int>> hash;

	private int count;

	public IEqualityComparer<T> Comparer => keyEqualityComparer;

	public sealed override int Count => count;

	private static KeyValuePair<T, int> NewPair(T item, int count)
	{
		return new KeyValuePair<T, int>(item, count);
	}

	private static KeyValuePair<T, int> NewPair(T item)
	{
		return new KeyValuePair<T, int>(item, 0);
	}

	public Bag()
		: this((IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public Bag(IEqualityComparer<T> equalityComparer)
	{
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		keyEqualityComparer = equalityComparer;
		this.equalityComparer = Comparers.EqualityComparerKeyValueFromComparerKey<T, int>(equalityComparer);
		hash = new Hash<KeyValuePair<T, int>>(this.equalityComparer);
	}

	public Bag(IEnumerable<T> collection)
		: this(collection, (IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public Bag(IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
		: this(equalityComparer)
	{
		AddMany(collection);
	}

	private Bag(IEqualityComparer<KeyValuePair<T, int>> equalityComparer, IEqualityComparer<T> keyEqualityComparer, Hash<KeyValuePair<T, int>> hash, int count)
	{
		this.equalityComparer = equalityComparer;
		this.keyEqualityComparer = keyEqualityComparer;
		this.hash = hash;
		this.count = count;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public Bag<T> Clone()
	{
		return new Bag<T>(equalityComparer, keyEqualityComparer, hash.Clone(null), count);
	}

	public Bag<T> CloneContents()
	{
		if (!Util.IsCloneableType(typeof(T), out var isValue))
		{
			throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof(T).FullName));
		}
		Hash<KeyValuePair<T, int>> hash = new Hash<KeyValuePair<T, int>>(equalityComparer);
		foreach (KeyValuePair<T, int> item3 in this.hash)
		{
			T item = ((isValue || item3.Key == null) ? item3.Key : ((T)((ICloneable)(object)item3.Key).Clone()));
			KeyValuePair<T, int> item2 = NewPair(item, item3.Value);
			hash.Insert(item2, replaceOnDuplicate: true, out var _);
		}
		return new Bag<T>(equalityComparer, keyEqualityComparer, hash, count);
	}

	public int NumberOfCopies(T item)
	{
		if (hash.Find(NewPair(item), replace: false, out var item2))
		{
			return item2.Value;
		}
		return 0;
	}

	public int GetRepresentativeItem(T item, out T representative)
	{
		if (hash.Find(NewPair(item), replace: false, out var item2))
		{
			representative = item2.Key;
			return item2.Value;
		}
		representative = item;
		return 0;
	}

	public sealed override IEnumerator<T> GetEnumerator()
	{
		foreach (KeyValuePair<T, int> pair in hash)
		{
			int i = 0;
			while (i < pair.Value)
			{
				yield return pair.Key;
				int num = i + 1;
				i = num;
			}
		}
	}

	public sealed override bool Contains(T item)
	{
		KeyValuePair<T, int> item2;
		return hash.Find(NewPair(item), replace: false, out item2);
	}

	public IEnumerable<T> DistinctItems()
	{
		foreach (KeyValuePair<T, int> item in hash)
		{
			yield return item.Key;
		}
	}

	public sealed override void Add(T item)
	{
		KeyValuePair<T, int> previous = NewPair(item, 1);
		if (!hash.Insert(previous, replaceOnDuplicate: false, out var previous2))
		{
			KeyValuePair<T, int> item2 = NewPair(previous2.Key, previous2.Value + 1);
			hash.Insert(item2, replaceOnDuplicate: true, out previous);
		}
		count++;
	}

	public void AddRepresentative(T item)
	{
		KeyValuePair<T, int> previous = NewPair(item, 1);
		if (!hash.Insert(previous, replaceOnDuplicate: false, out var previous2))
		{
			KeyValuePair<T, int> item2 = NewPair(previous.Key, previous2.Value + 1);
			hash.Insert(item2, replaceOnDuplicate: true, out previous);
		}
		count++;
	}

	public void ChangeNumberOfCopies(T item, int numCopies)
	{
		if (numCopies == 0)
		{
			RemoveAllCopies(item);
			return;
		}
		KeyValuePair<T, int> item3;
		if (hash.Find(NewPair(item), replace: false, out var item2))
		{
			count += numCopies - item2.Value;
			item3 = NewPair(item2.Key, numCopies);
		}
		else
		{
			count += numCopies;
			item3 = NewPair(item, numCopies);
		}
		hash.Insert(item3, replaceOnDuplicate: true, out var _);
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
		if (hash.Delete(NewPair(item), out var itemDeleted))
		{
			if (itemDeleted.Value > 1)
			{
				KeyValuePair<T, int> item2 = NewPair(itemDeleted.Key, itemDeleted.Value - 1);
				hash.Insert(item2, replaceOnDuplicate: true, out var _);
			}
			count--;
			return true;
		}
		return false;
	}

	public int RemoveAllCopies(T item)
	{
		if (hash.Delete(NewPair(item), out var itemDeleted))
		{
			count -= itemDeleted.Value;
			return itemDeleted.Value;
		}
		return 0;
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
		hash.StopEnumerations();
		hash = new Hash<KeyValuePair<T, int>>(equalityComparer);
		count = 0;
	}

	private void CheckConsistentComparison(Bag<T> otherBag)
	{
		if (otherBag == null)
		{
			throw new ArgumentNullException("otherBag");
		}
		if (!object.Equals(equalityComparer, otherBag.equalityComparer))
		{
			throw new InvalidOperationException(Strings.InconsistentComparisons);
		}
	}

	public bool IsEqualTo(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		if (otherBag.Count != Count)
		{
			return false;
		}
		foreach (T item in otherBag.DistinctItems())
		{
			if (NumberOfCopies(item) != otherBag.NumberOfCopies(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsSupersetOf(Bag<T> otherBag)
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

	public bool IsProperSupersetOf(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		if (otherBag.Count >= Count)
		{
			return false;
		}
		return IsSupersetOf(otherBag);
	}

	public bool IsSubsetOf(Bag<T> otherBag)
	{
		return otherBag.IsSupersetOf(this);
	}

	public bool IsProperSubsetOf(Bag<T> otherBag)
	{
		return otherBag.IsProperSupersetOf(this);
	}

	public bool IsDisjointFrom(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		Bag<T> bag;
		Bag<T> bag2;
		if (otherBag.Count > Count)
		{
			bag = this;
			bag2 = otherBag;
		}
		else
		{
			bag = otherBag;
			bag2 = this;
		}
		foreach (T item in bag.DistinctItems())
		{
			if (bag2.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public void UnionWith(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		if (otherBag == this)
		{
			return;
		}
		foreach (T item in otherBag.DistinctItems())
		{
			int num = NumberOfCopies(item);
			int num2 = otherBag.NumberOfCopies(item);
			if (num2 > num)
			{
				ChangeNumberOfCopies(item, num2);
			}
		}
	}

	public Bag<T> Union(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		Bag<T> otherBag2;
		Bag<T> bag;
		if (otherBag.Count > Count)
		{
			otherBag2 = this;
			bag = otherBag;
		}
		else
		{
			otherBag2 = otherBag;
			bag = this;
		}
		Bag<T> bag2 = bag.Clone();
		bag2.UnionWith(otherBag2);
		return bag2;
	}

	public void SumWith(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		if (this == otherBag)
		{
			AddMany(otherBag);
			return;
		}
		foreach (T item in otherBag.DistinctItems())
		{
			int num = NumberOfCopies(item);
			int num2 = otherBag.NumberOfCopies(item);
			ChangeNumberOfCopies(item, num + num2);
		}
	}

	public Bag<T> Sum(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		Bag<T> otherBag2;
		Bag<T> bag;
		if (otherBag.Count > Count)
		{
			otherBag2 = this;
			bag = otherBag;
		}
		else
		{
			otherBag2 = otherBag;
			bag = this;
		}
		Bag<T> bag2 = bag.Clone();
		bag2.SumWith(otherBag2);
		return bag2;
	}

	public void IntersectionWith(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		this.hash.StopEnumerations();
		Bag<T> bag;
		Bag<T> bag2;
		if (otherBag.Count > Count)
		{
			bag = this;
			bag2 = otherBag;
		}
		else
		{
			bag = otherBag;
			bag2 = this;
		}
		Hash<KeyValuePair<T, int>> hash = new Hash<KeyValuePair<T, int>>(equalityComparer);
		int num = 0;
		foreach (T item in bag.DistinctItems())
		{
			int val = bag2.NumberOfCopies(item);
			int val2 = bag.NumberOfCopies(item);
			int num2 = Math.Min(val, val2);
			if (num2 > 0)
			{
				hash.Insert(NewPair(item, num2), replaceOnDuplicate: true, out var _);
				num += num2;
			}
		}
		this.hash = hash;
		count = num;
	}

	public Bag<T> Intersection(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		Bag<T> bag;
		Bag<T> bag2;
		if (otherBag.Count > Count)
		{
			bag = this;
			bag2 = otherBag;
		}
		else
		{
			bag = otherBag;
			bag2 = this;
		}
		Bag<T> bag3 = new Bag<T>(keyEqualityComparer);
		foreach (T item in bag.DistinctItems())
		{
			int val = bag2.NumberOfCopies(item);
			int val2 = bag.NumberOfCopies(item);
			int num = Math.Min(val, val2);
			if (num > 0)
			{
				bag3.ChangeNumberOfCopies(item, num);
			}
		}
		return bag3;
	}

	public void DifferenceWith(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		if (this == otherBag)
		{
			Clear();
			return;
		}
		foreach (T item in otherBag.DistinctItems())
		{
			int num = NumberOfCopies(item);
			int num2 = otherBag.NumberOfCopies(item);
			int num3 = num - num2;
			if (num3 < 0)
			{
				num3 = 0;
			}
			ChangeNumberOfCopies(item, num3);
		}
	}

	public Bag<T> Difference(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		Bag<T> bag = Clone();
		bag.DifferenceWith(otherBag);
		return bag;
	}

	public void SymmetricDifferenceWith(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		if (this == otherBag)
		{
			Clear();
			return;
		}
		foreach (T item in otherBag.DistinctItems())
		{
			int num = NumberOfCopies(item);
			int num2 = otherBag.NumberOfCopies(item);
			int num3 = Math.Abs(num - num2);
			if (num3 != num)
			{
				ChangeNumberOfCopies(item, num3);
			}
		}
	}

	public Bag<T> SymmetricDifference(Bag<T> otherBag)
	{
		CheckConsistentComparison(otherBag);
		Bag<T> otherBag2;
		Bag<T> bag;
		if (otherBag.Count > Count)
		{
			otherBag2 = this;
			bag = otherBag;
		}
		else
		{
			otherBag2 = otherBag;
			bag = this;
		}
		Bag<T> bag2 = bag.Clone();
		bag2.SymmetricDifferenceWith(otherBag2);
		return bag2;
	}
}
