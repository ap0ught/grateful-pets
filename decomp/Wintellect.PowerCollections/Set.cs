using System;
using System.Collections;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public class Set<T> : CollectionBase<T>, ICollection<T>, IEnumerable<T>, IEnumerable, ICloneable
{
	private IEqualityComparer<T> equalityComparer;

	private Hash<T> hash;

	public IEqualityComparer<T> Comparer => equalityComparer;

	public sealed override int Count => hash.ElementCount;

	public Set()
		: this((IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public Set(IEqualityComparer<T> equalityComparer)
	{
		if (equalityComparer == null)
		{
			throw new ArgumentNullException("equalityComparer");
		}
		this.equalityComparer = equalityComparer;
		hash = new Hash<T>(equalityComparer);
	}

	public Set(IEnumerable<T> collection)
		: this(collection, (IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public Set(IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
		: this(equalityComparer)
	{
		AddMany(collection);
	}

	private Set(IEqualityComparer<T> equalityComparer, Hash<T> hash)
	{
		this.equalityComparer = equalityComparer;
		this.hash = hash;
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public Set<T> Clone()
	{
		return new Set<T>(equalityComparer, hash.Clone(null));
	}

	public Set<T> CloneContents()
	{
		if (!Util.IsCloneableType(typeof(T), out var isValue))
		{
			throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof(T).FullName));
		}
		Set<T> set = new Set<T>(equalityComparer);
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
			set.Add(item);
		}
		return set;
	}

	public sealed override IEnumerator<T> GetEnumerator()
	{
		return hash.GetEnumerator();
	}

	public sealed override bool Contains(T item)
	{
		T item2;
		return hash.Find(item, replace: false, out item2);
	}

	public bool TryGetItem(T item, out T foundItem)
	{
		return hash.Find(item, replace: false, out foundItem);
	}

	public new bool Add(T item)
	{
		T previous;
		return !hash.Insert(item, replaceOnDuplicate: true, out previous);
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
		T itemDeleted;
		return hash.Delete(item, out itemDeleted);
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
		hash = new Hash<T>(equalityComparer);
	}

	private void CheckConsistentComparison(Set<T> otherSet)
	{
		if (otherSet == null)
		{
			throw new ArgumentNullException("otherSet");
		}
		if (!object.Equals(equalityComparer, otherSet.equalityComparer))
		{
			throw new InvalidOperationException(Strings.InconsistentComparisons);
		}
	}

	public bool IsSupersetOf(Set<T> otherSet)
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

	public bool IsProperSupersetOf(Set<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		if (otherSet.Count >= Count)
		{
			return false;
		}
		return IsSupersetOf(otherSet);
	}

	public bool IsSubsetOf(Set<T> otherSet)
	{
		return otherSet.IsSupersetOf(this);
	}

	public bool IsProperSubsetOf(Set<T> otherSet)
	{
		return otherSet.IsProperSupersetOf(this);
	}

	public bool IsEqualTo(Set<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		if (otherSet.Count != Count)
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

	public bool IsDisjointFrom(Set<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		Set<T> set;
		Set<T> set2;
		if (otherSet.Count > Count)
		{
			set = this;
			set2 = otherSet;
		}
		else
		{
			set = otherSet;
			set2 = this;
		}
		foreach (T item in set)
		{
			if (set2.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public void UnionWith(Set<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		AddMany(otherSet);
	}

	public Set<T> Union(Set<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		Set<T> collection;
		Set<T> set;
		if (otherSet.Count > Count)
		{
			collection = this;
			set = otherSet;
		}
		else
		{
			collection = otherSet;
			set = this;
		}
		Set<T> set2 = set.Clone();
		set2.AddMany(collection);
		return set2;
	}

	public void IntersectionWith(Set<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		this.hash.StopEnumerations();
		Set<T> set;
		Set<T> set2;
		if (otherSet.Count > Count)
		{
			set = this;
			set2 = otherSet;
		}
		else
		{
			set = otherSet;
			set2 = this;
		}
		Hash<T> hash = new Hash<T>(equalityComparer);
		foreach (T item in set)
		{
			if (set2.Contains(item))
			{
				hash.Insert(item, replaceOnDuplicate: true, out var _);
			}
		}
		this.hash = hash;
	}

	public Set<T> Intersection(Set<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		Set<T> set;
		Set<T> set2;
		if (otherSet.Count > Count)
		{
			set = this;
			set2 = otherSet;
		}
		else
		{
			set = otherSet;
			set2 = this;
		}
		Set<T> set3 = new Set<T>(equalityComparer);
		foreach (T item in set)
		{
			if (set2.Contains(item))
			{
				set3.Add(item);
			}
		}
		return set3;
	}

	public void DifferenceWith(Set<T> otherSet)
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

	public Set<T> Difference(Set<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		Set<T> set = Clone();
		set.DifferenceWith(otherSet);
		return set;
	}

	public void SymmetricDifferenceWith(Set<T> otherSet)
	{
		if (this == otherSet)
		{
			Clear();
		}
		CheckConsistentComparison(otherSet);
		if (otherSet.Count > Count)
		{
			this.hash.StopEnumerations();
			Hash<T> hash = otherSet.hash.Clone(null);
			using (IEnumerator<T> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					T current = enumerator.Current;
					if (hash.Find(current, replace: false, out var item))
					{
						hash.Delete(current, out item);
					}
					else
					{
						hash.Insert(current, replaceOnDuplicate: true, out item);
					}
				}
			}
			this.hash = hash;
			return;
		}
		foreach (T item2 in otherSet)
		{
			if (Contains(item2))
			{
				Remove(item2);
			}
			else
			{
				Add(item2);
			}
		}
	}

	public Set<T> SymmetricDifference(Set<T> otherSet)
	{
		CheckConsistentComparison(otherSet);
		Set<T> set;
		Set<T> set2;
		if (otherSet.Count > Count)
		{
			set = this;
			set2 = otherSet;
		}
		else
		{
			set = otherSet;
			set2 = this;
		}
		Set<T> set3 = set2.Clone();
		foreach (T item in set)
		{
			if (set3.Contains(item))
			{
				set3.Remove(item);
			}
			else
			{
				set3.Add(item);
			}
		}
		return set3;
	}
}
