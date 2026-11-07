using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Genkit;

namespace XRL.Collections;

[Serializable]
public class Rack<T> : Container<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IList, ICollection
{
	public virtual T this[int Index]
	{
		get
		{
			if ((uint)Index >= (uint)Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			return Items[Index];
		}
		set
		{
			if ((uint)Index >= (uint)Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			Items[Index] = value;
			Variant++;
		}
	}

	bool IList.IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	object IList.this[int Index]
	{
		get
		{
			if ((uint)Index < (uint)Length)
			{
				return Items[Index];
			}
			throw new ArgumentOutOfRangeException();
		}
		set
		{
			if ((uint)Index < (uint)Length)
			{
				Variant++;
				Items[Index] = (T)value;
			}
			throw new ArgumentOutOfRangeException();
		}
	}

	public Rack()
	{
	}

	public Rack(int Capacity)
	{
		EnsureCapacity(Capacity);
	}

	public Rack(IReadOnlyList<T> List)
	{
		if (List != null)
		{
			int count = List.Count;
			EnsureCapacity(count);
			for (int i = 0; i < count; i++)
			{
				Add(List[i]);
			}
		}
	}

	/// <summary>Retrieve a value by reference.</summary>
	public ref T GetReference(int Index)
	{
		if ((uint)Index >= (uint)Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		return ref Items[Index];
	}

	public virtual void Add(T Item)
	{
		if (Length == Size)
		{
			Resize(Length * 2);
		}
		Items[Length++] = Item;
		Variant++;
	}

	public void AddRange(IReadOnlyList<T> Items)
	{
		int count = Items.Count;
		EnsureCapacity(Length + count);
		for (int i = 0; i < count; i++)
		{
			Add(Items[i]);
		}
	}

	public void AddRange(IReadOnlyCollection<T> Items)
	{
		if (Items is IReadOnlyList<T> items)
		{
			AddRange(items);
			return;
		}
		EnsureCapacity(Length + Items.Count);
		foreach (T Item in Items)
		{
			Add(Item);
		}
	}

	public void AddRange(IEnumerable<T> Items)
	{
		if (Items is IReadOnlyCollection<T> items)
		{
			AddRange(items);
			return;
		}
		foreach (T Item in Items)
		{
			Add(Item);
		}
	}

	public void AddRange(ReadOnlySpan<T> Items)
	{
		EnsureCapacity(Length + Items.Length);
		Items.CopyTo(base.Items.AsSpan(Length, Items.Length));
		Length += Items.Length;
		Variant++;
	}

	public Span<T> FillSpan(int Length)
	{
		EnsureCapacity(base.Length + Length);
		Span<T> result = new Span<T>(Items, base.Length, Length);
		base.Length += Length;
		Variant++;
		return result;
	}

	public virtual void Clear()
	{
		if (Length > 0)
		{
			Array.Clear(Items, 0, Length);
			Length = 0;
		}
		Variant++;
	}

	public void Reverse()
	{
		Reverse(0, Length);
	}

	public virtual void Reverse(int Index, int Length)
	{
		Array.Reverse(Items, Index, Length);
		Variant++;
	}

	public virtual bool Remove(T Item)
	{
		int num = Array.IndexOf(Items, Item, 0, Length);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public virtual void Insert(int Index, T Item)
	{
		if (Index > Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (Length == Size)
		{
			Resize(Size * 2);
		}
		if (Index < Length)
		{
			Array.Copy(Items, Index, Items, Index + 1, Length - Index);
		}
		Items[Index] = Item;
		Length++;
		Variant++;
	}

	public virtual void RemoveAt(int Index)
	{
		if (Index >= Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		Length--;
		if (Index < Length)
		{
			Array.Copy(Items, Index + 1, Items, Index, Length - Index);
		}
		Items[Length] = default;
		Variant++;
	}

	public T TakeAt(int Index)
	{
		T result = Items[Index];
		RemoveAt(Index);
		return result;
	}

	public void ShuffleInPlace(Random Random = null)
	{
		if (Random == null)
		{
			Random = Calc.R;
		}
		for (int num = Length - 1; num >= 1; num--)
		{
			int num2 = Random.Next(num + 1);
			T val = Items[num];
			Items[num] = Items[num2];
			Items[num2] = val;
		}
	}

	public void Sort()
	{
		Sort(0, Length, null);
	}

	public void Sort(IComparer<T> Comparer)
	{
		Sort(0, Length, Comparer);
	}

	public void Sort(int Index, int Count, IComparer<T> Comparer)
	{
		Array.Sort(Items, Index, Count, Comparer);
		Variant++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T[] GetArray()
	{
		return Items;
	}

	public T[] GetArray(int Capacity)
	{
		EnsureCapacity(Capacity);
		return Items;
	}

	/// <summary>Reuse an existing array by wrapping it in a rack.</summary>
	public static Rack<T> Wrap(T[] Array, int Length)
	{
		return new Rack<T>
		{
			Items = Array,
			Size = Array.Length,
			Length = Length
		};
	}

	public static implicit operator Span<T>(Rack<T> Rack)
	{
		return Rack.AsSpan();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new Span<T> AsSpan()
	{
		return new Span<T>(Items, 0, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new Span<T> AsSpan(int Start)
	{
		if ((uint)Start > (uint)Length)
		{
			throw new ArgumentOutOfRangeException("Start");
		}
		return new Span<T>(Items, Start, Length - Start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new Span<T> AsSpan(int Start, int Length)
	{
		if ((uint)(Start + Length) > (uint)base.Length)
		{
			throw new ArgumentOutOfRangeException("Length");
		}
		return new Span<T>(Items, Start, Length);
	}

	int IList.Add(object Value)
	{
		Add((T)Value);
		return Length - 1;
	}

	bool IList.Contains(object Value)
	{
		if (Value is T item)
		{
			return Contains(item);
		}
		return false;
	}

	int IList.IndexOf(object Value)
	{
		if (Value is T item)
		{
			return IndexOf(item);
		}
		return -1;
	}

	void IList.Insert(int Index, object Value)
	{
		Insert(Index, (T)Value);
	}

	void IList.Remove(object Value)
	{
		if (Value is T item)
		{
			Remove(item);
		}
	}

	void ICollection.CopyTo(Array Array, int Index)
	{
		Array.Copy(Items, 0, Array, Index, Length);
	}
}
