using System;
using System.Collections;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public abstract class ListBase<T> : CollectionBase<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection
{
	public abstract override int Count { get; }

	public abstract T this[int index] { get; set; }

	bool IList.IsFixedSize => false;

	bool IList.IsReadOnly => ((ICollection<T>)this).IsReadOnly;

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = ConvertToItemType("value", value);
		}
	}

	public abstract override void Clear();

	public abstract void Insert(int index, T item);

	public abstract void RemoveAt(int index);

	public override IEnumerator<T> GetEnumerator()
	{
		int count = Count;
		int i = 0;
		while (i < count)
		{
			yield return this[i];
			int num = i + 1;
			i = num;
		}
	}

	public override bool Contains(T item)
	{
		return IndexOf(item) >= 0;
	}

	public override void Add(T item)
	{
		Insert(Count, item);
	}

	public override bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public virtual void CopyTo(T[] array)
	{
		CopyTo(array, 0);
	}

	public override void CopyTo(T[] array, int arrayIndex)
	{
		base.CopyTo(array, arrayIndex);
	}

	public virtual void CopyTo(int index, T[] array, int arrayIndex, int count)
	{
		Range(index, count).CopyTo(array, arrayIndex);
	}

	public new virtual IList<T> AsReadOnly()
	{
		return Algorithms.ReadOnly(this);
	}

	public virtual T Find(Predicate<T> predicate)
	{
		return Algorithms.FindFirstWhere(this, predicate);
	}

	public virtual bool TryFind(Predicate<T> predicate, out T foundItem)
	{
		return Algorithms.TryFindFirstWhere(this, predicate, out foundItem);
	}

	public virtual T FindLast(Predicate<T> predicate)
	{
		return Algorithms.FindLastWhere(this, predicate);
	}

	public virtual bool TryFindLast(Predicate<T> predicate, out T foundItem)
	{
		return Algorithms.TryFindLastWhere(this, predicate, out foundItem);
	}

	public virtual int FindIndex(Predicate<T> predicate)
	{
		return Algorithms.FindFirstIndexWhere(this, predicate);
	}

	public virtual int FindIndex(int index, Predicate<T> predicate)
	{
		int num = Algorithms.FindFirstIndexWhere(Range(index, Count - index), predicate);
		if (num < 0)
		{
			return -1;
		}
		return num + index;
	}

	public virtual int FindIndex(int index, int count, Predicate<T> predicate)
	{
		int num = Algorithms.FindFirstIndexWhere(Range(index, count), predicate);
		if (num < 0)
		{
			return -1;
		}
		return num + index;
	}

	public virtual int FindLastIndex(Predicate<T> predicate)
	{
		return Algorithms.FindLastIndexWhere(this, predicate);
	}

	public virtual int FindLastIndex(int index, Predicate<T> predicate)
	{
		return Algorithms.FindLastIndexWhere(Range(0, index + 1), predicate);
	}

	public virtual int FindLastIndex(int index, int count, Predicate<T> predicate)
	{
		int num = Algorithms.FindLastIndexWhere(Range(index - count + 1, count), predicate);
		if (num >= 0)
		{
			return num + index - count + 1;
		}
		return -1;
	}

	public virtual int IndexOf(T item)
	{
		return Algorithms.FirstIndexOf(this, item, EqualityComparer<T>.Default);
	}

	public virtual int IndexOf(T item, int index)
	{
		int num = Algorithms.FirstIndexOf(Range(index, Count - index), item, EqualityComparer<T>.Default);
		if (num >= 0)
		{
			return num + index;
		}
		return -1;
	}

	public virtual int IndexOf(T item, int index, int count)
	{
		int num = Algorithms.FirstIndexOf(Range(index, count), item, EqualityComparer<T>.Default);
		if (num >= 0)
		{
			return num + index;
		}
		return -1;
	}

	public virtual int LastIndexOf(T item)
	{
		return Algorithms.LastIndexOf(this, item, EqualityComparer<T>.Default);
	}

	public virtual int LastIndexOf(T item, int index)
	{
		return Algorithms.LastIndexOf(Range(0, index + 1), item, EqualityComparer<T>.Default);
	}

	public virtual int LastIndexOf(T item, int index, int count)
	{
		int num = Algorithms.LastIndexOf(Range(index - count + 1, count), item, EqualityComparer<T>.Default);
		if (num >= 0)
		{
			return num + index - count + 1;
		}
		return -1;
	}

	public virtual IList<T> Range(int start, int count)
	{
		return Algorithms.Range(this, start, count);
	}

	private T ConvertToItemType(string name, object value)
	{
		try
		{
			return (T)value;
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException(string.Format(Strings.WrongType, value, typeof(T)), name);
		}
	}

	int IList.Add(object value)
	{
		int count = Count;
		Insert(count, ConvertToItemType("value", value));
		return count;
	}

	void IList.Clear()
	{
		Clear();
	}

	bool IList.Contains(object value)
	{
		if (value is T || value == null)
		{
			return Contains((T)value);
		}
		return false;
	}

	int IList.IndexOf(object value)
	{
		if (value is T || value == null)
		{
			return IndexOf((T)value);
		}
		return -1;
	}

	void IList.Insert(int index, object value)
	{
		Insert(index, ConvertToItemType("value", value));
	}

	void IList.Remove(object value)
	{
		if (value is T || value == null)
		{
			Remove((T)value);
		}
	}

	void IList.RemoveAt(int index)
	{
		RemoveAt(index);
	}
}
