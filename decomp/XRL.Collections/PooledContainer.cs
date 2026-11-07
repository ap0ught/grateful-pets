using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using XRL.World;

namespace XRL.Collections;

[Serializable]
public abstract class PooledContainer<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IComposite, IDisposable
{
	[Serializable]
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private PooledContainer<T> List;

		private int Index;

		private int Version;

		private T Item;

		public T Current => Item;

		object IEnumerator.Current => Item;

		public Enumerator(PooledContainer<T> List)
		{
			this.List = List;
			Index = 0;
			Version = List.Version;
			Item = default;
		}

		public bool MoveNext()
		{
			if (List.Version != Version)
			{
				throw new InvalidOperationException();
			}
			if (Index >= List.Length)
			{
				Item = default;
				return false;
			}
			Item = List.Items[Index++];
			return true;
		}

		public void Reset()
		{
			if (List.Version != Version)
			{
				throw new InvalidOperationException();
			}
			Index = 0;
			Item = default;
		}

		public void Dispose()
		{
		}
	}

	protected static readonly ArrayPool<T> DefaultPool = ArrayPool<T>.Shared;

	protected T[] Items = Array.Empty<T>();

	protected int Size;

	protected int Length;

	protected int Version;

	protected virtual ArrayPool<T> Pool => DefaultPool;

	protected virtual int DefaultCapacity => 4;

	public bool WantFieldReflection => false;

	public bool IsReadOnly => false;

	public int Capacity => Size;

	public int Count => Length;

	T IList<T>.this[int Index]
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
			throw new NotSupportedException();
		}
	}

	public T this[int Index]
	{
		get
		{
			if ((uint)Index < (uint)Length)
			{
				return Items[Index];
			}
			throw new ArgumentOutOfRangeException();
		}
	}

	public void EnsureCapacity(int Capacity)
	{
		if (Size < Capacity)
		{
			Resize(Capacity);
		}
	}

	protected void Resize(int Capacity)
	{
		if (Capacity == 0)
		{
			Capacity = DefaultCapacity;
		}
		T[] array = Pool.Rent(Capacity);
		if (Size > 0)
		{
			Array.Copy(Items, 0, array, 0, Length);
			Array.Clear(Items, 0, Length);
			Pool.Return(Items);
		}
		Items = array;
		Size = array.Length;
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	void IList<T>.Insert(int Index, T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int Index)
	{
		throw new NotSupportedException();
	}

	public bool Contains(T Item)
	{
		return Array.IndexOf(Items, Item, 0, Length) >= 0;
	}

	public void CopyTo(T[] Buffer, int Index)
	{
		Array.Copy(Items, 0, Buffer, Index, Length);
	}

	public int IndexOf(T Item)
	{
		return Array.IndexOf(Items, Item, 0, Length);
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
		Version++;
	}

	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Length);
		for (int i = 0; i < Length; i++)
		{
			Writer.WriteObject(Items[i]);
		}
	}

	public virtual void Read(SerializationReader Reader)
	{
		Length = Reader.ReadOptimizedInt32();
		Items = Pool.Rent(Length);
		Size = Items.Length;
		for (int i = 0; i < Length; i++)
		{
			Items[i] = (T)Reader.ReadObject();
		}
	}

	public virtual void Dispose()
	{
		if (Size > 0)
		{
			Array.Clear(Items, 0, Length);
			Pool.Return(ref Items);
			Length = (Size = 0);
		}
	}

	public T[] ToArray()
	{
		T[] array = new T[Length];
		Array.Copy(Items, 0, array, 0, Length);
		return array;
	}

	~PooledContainer()
	{
		Dispose();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}
}
