using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using XRL.World;

namespace XRL.Collections;

[Serializable]
public abstract class Container<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, IComposite
{
	[Serializable]
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private Container<T> List;

		private int Index;

		private int Variant;

		private T Item;

		public T Current => Item;

		object IEnumerator.Current => Item;

		public Enumerator(Container<T> List)
		{
			this.List = List;
			Index = 0;
			Variant = List.Variant;
			Item = default;
		}

		public bool MoveNext()
		{
			if (List.Variant != Variant)
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
			if (List.Variant != Variant)
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

	protected T[] Items = Array.Empty<T>();

	protected int Size;

	protected int Length;

	protected int Variant;

	protected virtual int DefaultCapacity => 4;

	public bool WantFieldReflection => false;

	public bool IsReadOnly => false;

	public int Capacity => Size;

	public int Count => Length;

	public int Version => Variant;

	T IReadOnlyList<T>.this[int Index]
	{
		get
		{
			if ((uint)Index >= (uint)Size)
			{
				throw new ArgumentOutOfRangeException();
			}
			return Items[Index];
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
		T[] array = new T[Capacity];
		Array.Copy(Items, 0, array, 0, Length);
		Items = array;
		Size = Capacity;
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
		Size = (Length = Reader.ReadOptimizedInt32());
		Items = new T[Size];
		for (int i = 0; i < Length; i++)
		{
			Items[i] = (T)Reader.ReadObject();
		}
	}

	public T[] ToArray()
	{
		T[] array = new T[Length];
		Array.Copy(Items, 0, array, 0, Length);
		return array;
	}

	public static implicit operator ReadOnlySpan<T>(Container<T> Container)
	{
		return Container.AsSpan();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<T> AsSpan()
	{
		return new ReadOnlySpan<T>(Items, 0, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<T> AsSpan(int Start)
	{
		if ((uint)Start > (uint)Length)
		{
			throw new ArgumentOutOfRangeException("Start");
		}
		return new ReadOnlySpan<T>(Items, Start, Length - Start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<T> AsSpan(int Start, int Length)
	{
		if ((uint)(Start + Length) > (uint)this.Length)
		{
			throw new ArgumentOutOfRangeException("Length");
		}
		return new ReadOnlySpan<T>(Items, Start, Length);
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
