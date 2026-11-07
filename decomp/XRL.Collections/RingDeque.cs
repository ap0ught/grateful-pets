using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using XRL.World;

namespace XRL.Collections;

/// <summary>A double-ended queue using an expanding ring buffer.</summary>
[Serializable]
public class RingDeque<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IComposite
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private RingDeque<T> Queue;

		private int Index;

		private int Version;

		private T Item;

		public T Current => Item;

		object IEnumerator.Current => Item;

		public Enumerator(RingDeque<T> Queue)
		{
			this.Queue = Queue;
			Version = this.Queue.Version;
			Index = -1;
			Item = default;
		}

		public bool MoveNext()
		{
			if (Version != Queue.Version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			if (++Index >= Queue.Amount)
			{
				Item = default;
				return false;
			}
			Item = Queue.Buffer[(Queue.Head + Index) % Queue.Size];
			return true;
		}

		void IEnumerator.Reset()
		{
			Index = -1;
			Item = default;
		}

		public void Dispose()
		{
		}
	}

	protected T[] Buffer;

	protected int Head;

	protected int Tail;

	protected int Size;

	protected int Amount;

	protected int Version;

	protected virtual EqualityComparer<T> Comparer => EqualityComparer<T>.Default;

	public T this[int Index]
	{
		get
		{
			return Buffer[(Head + Index) % Size];
		}
		set
		{
			Buffer[(Head + Index) % Size] = value;
		}
	}

	public T First => Buffer[Head];

	public T Last => Buffer[(Tail + Size - 1) % Size];

	public int Count => Amount;

	public int Capacity => Size;

	public bool IsReadOnly => false;

	public bool WantFieldReflection => false;

	public RingDeque()
	{
		Buffer = Array.Empty<T>();
	}

	public RingDeque(int Capacity)
	{
		Buffer = new T[Capacity];
		Size = Capacity;
		Head = 0;
		Tail = 0;
		Amount = 0;
	}

	void ICollection<T>.Add(T Item)
	{
		Enqueue(Item);
	}

	public virtual void Clear()
	{
		if (Head < Tail)
		{
			Array.Clear(Buffer, Head, Amount);
		}
		else
		{
			Array.Clear(Buffer, Head, Size - Head);
			Array.Clear(Buffer, 0, Tail);
		}
		Head = 0;
		Tail = 0;
		Amount = 0;
		Version++;
	}

	/// <summary>Add element to tail of queue.</summary>
	public virtual void Enqueue(T Item)
	{
		if (Amount == Size)
		{
			SetCapacity((Size == 0) ? 4 : (Size * 2));
		}
		Buffer[Tail] = Item;
		Tail = (Tail + 1) % Size;
		Amount++;
		Version++;
	}

	/// <summary>Add element to head of queue.</summary>
	public virtual void Push(T Item)
	{
		if (Amount == Size)
		{
			SetCapacity((Size == 0) ? 4 : (Size * 2));
		}
		Head = (Head + Size - 1) % Size;
		Buffer[Head] = Item;
		Amount++;
		Version++;
	}

	/// <summary>Remove element from head of queue.</summary>
	public T Dequeue()
	{
		TryDequeue(out var Value);
		return Value;
	}

	/// <summary>Try to remove element from head of queue, if one exists.</summary>
	public virtual bool TryDequeue(out T Value)
	{
		if (Amount == 0)
		{
			Value = default;
			return false;
		}
		Value = Buffer[Head];
		Buffer[Head] = default;
		Head = (Head + 1) % Size;
		Amount--;
		Version++;
		return true;
	}

	/// <summary>Remove element from tail of queue.</summary>
	public T Eject()
	{
		TryEject(out var Value);
		return Value;
	}

	/// <summary>Try to remove element from tail of queue, if one exists.</summary>
	public virtual bool TryEject(out T Value)
	{
		if (Amount == 0)
		{
			Value = default;
			return false;
		}
		Tail = (Tail + Size - 1) % Size;
		Value = Buffer[Tail];
		Buffer[Tail] = default;
		Amount--;
		Version++;
		return true;
	}

	/// <summary>Get index of item closest to head.</summary>
	public int IndexOf(T Item)
	{
		EqualityComparer<T> comparer = Comparer;
		for (int i = 0; i < Amount; i++)
		{
			T val = Buffer[(Head + i) % Size];
			if (val == null)
			{
				if (Item == null)
				{
					return i;
				}
			}
			else if (comparer.Equals(val, Item))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Get index of item closest to tail.</summary>
	public int LastIndexOf(T Item)
	{
		EqualityComparer<T> comparer = Comparer;
		for (int num = Amount - 1; num >= 0; num--)
		{
			T val = Buffer[(Head + num) % Size];
			if (val == null)
			{
				if (Item == null)
				{
					return num;
				}
			}
			else if (comparer.Equals(val, Item))
			{
				return num;
			}
		}
		return -1;
	}

	public bool Contains(T Item)
	{
		return IndexOf(Item) >= 0;
	}

	public bool Remove(T Item)
	{
		int num = IndexOf(Item);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	/// <remarks>Removing from start or end of queue is done in constant time, linear otherwise.</remarks>
	public T RemoveAt(int Index)
	{
		Index %= Amount;
		T result;
		if (Index == 0)
		{
			result = Buffer[Head];
			Buffer[Head] = default;
			Head = (Head + 1) % Size;
		}
		else if (Index == Amount - 1)
		{
			Tail = (Tail + Size - 1) % Size;
			result = Buffer[Tail];
			Buffer[Tail] = default;
		}
		else
		{
			int num = (Head + Index) % Size;
			result = Buffer[num];
			if (num < Tail)
			{
				Tail = (Tail + Size - 1) % Size;
				Array.Copy(Buffer, num + 1, Buffer, num, Tail - num);
				Buffer[Tail] = default;
			}
			else
			{
				Array.Copy(Buffer, Head, Buffer, Head + 1, num - Head);
				Buffer[Head] = default;
				Head = (Head + 1) % Size;
			}
		}
		Amount--;
		Version++;
		return result;
	}

	public void CopyTo(T[] Destination, int Index)
	{
		int num = Destination.Length - Index;
		int num2 = ((num < Amount) ? num : Amount);
		if (num2 > 0)
		{
			if (Size - Head < num2)
			{
				Array.Copy(Buffer, Head, Destination, Index, Size - Head);
				Array.Copy(Buffer, 0, Destination, Index + Size - Head, num2);
			}
			else
			{
				Array.Copy(Buffer, 0, Destination, Index + Size - Head, num2);
			}
		}
	}

	public T[] ToArray()
	{
		T[] array = new T[Amount];
		if (Amount == 0)
		{
			return array;
		}
		if (Head < Tail)
		{
			Array.Copy(Buffer, Head, array, 0, Amount);
		}
		else
		{
			Array.Copy(Buffer, Head, array, 0, Size - Head);
			Array.Copy(Buffer, 0, array, Size - Head, Tail);
		}
		return array;
	}

	protected void SetCapacity(int Capacity)
	{
		T[] array = new T[Capacity];
		if (Amount > 0)
		{
			if (Head < Tail)
			{
				Array.Copy(Buffer, Head, array, 0, Amount);
			}
			else
			{
				Array.Copy(Buffer, Head, array, 0, Size - Head);
				Array.Copy(Buffer, 0, array, Size - Head, Tail);
			}
		}
		Buffer = array;
		Head = 0;
		Tail = ((Amount != Capacity) ? Amount : 0);
		Size = Capacity;
		Version++;
	}

	public void EnsureCapacity(int Capacity)
	{
		if (Capacity > Size)
		{
			SetCapacity(Capacity);
		}
	}

	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Amount);
		for (int i = 0; i < Amount; i++)
		{
			Writer.WriteObject(Buffer[(Head + i) % Size]);
		}
	}

	public virtual void Read(SerializationReader Reader)
	{
		Amount += Reader.ReadOptimizedInt32();
		EnsureCapacity(Amount);
		for (int i = 0; i < Amount; i++)
		{
			T val = (T)Reader.ReadObject();
			Buffer[Tail] = val;
			Tail = (Tail + 1) % Size;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T[] GetArray()
	{
		return Buffer;
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
