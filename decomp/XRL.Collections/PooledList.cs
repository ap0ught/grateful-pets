using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace XRL.Collections;

/// <summary>A list using the <see cref="P:System.Buffers.ArrayPool`1.Shared" /> pool for its internal arrays.</summary>
[Serializable]
public class PooledList<T> : PooledContainer<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
{
	public new T this[int Index]
	{
		get
		{
			if ((uint)Index < (uint)Length)
			{
				return Items[Index];
			}
			throw new IndexOutOfRangeException();
		}
		set
		{
			if ((uint)Index >= (uint)Length)
			{
				throw new IndexOutOfRangeException();
			}
			Version++;
			Items[Index] = value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PooledList<T> Get()
	{
		return ScopeDisposedList<T>.GetFromPool();
	}

	public PooledList()
	{
	}

	public PooledList(int Capacity)
	{
		EnsureCapacity(Capacity);
	}

	public PooledList(IReadOnlyList<T> List)
	{
		AddRange(List);
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

	public void Add(T Item)
	{
		if (Length == Size)
		{
			Resize(Length * 2);
		}
		Items[Length++] = Item;
		Version++;
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
		Version++;
	}

	public void Clear()
	{
		if (Length > 0)
		{
			Array.Clear(Items, 0, Length);
			Length = 0;
		}
		Version++;
	}

	public bool Remove(T Item)
	{
		int num = Array.IndexOf(Items, Item, 0, Length);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public void Insert(int Index, T Item)
	{
		if (Index > Size)
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
		Version++;
	}

	public void RemoveAt(int Index)
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
		Version++;
	}

	public T TakeAt(int Index)
	{
		T result = Items[Index];
		RemoveAt(Index);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> AsSpan()
	{
		return new Span<T>(Items);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> AsSpan(int Start)
	{
		return new Span<T>(Items, Start, Length - Start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> AsSpan(int Start, int Length)
	{
		return new Span<T>(Items, Start, Length);
	}
}
