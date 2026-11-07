using System;

public class exList<T> where T : struct
{
	private static readonly T[] emptyArray = new T[0];

	private static exList<T> tempList_;

	public T[] buffer;

	public int Count;

	public int Capacity
	{
		get
		{
			return buffer.Length;
		}
		set
		{
			if (value < Count)
			{
				throw new ArgumentOutOfRangeException();
			}
			Array.Resize(ref buffer, value);
		}
	}

	public static exList<T> GetTempList()
	{
		if (tempList_ == null)
		{
			tempList_ = new exList<T>();
		}
		tempList_.Clear();
		return tempList_;
	}

	public exList()
	{
		buffer = emptyArray;
	}

	public exList(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		buffer = new T[capacity];
	}

	public void Add(T _item)
	{
		if (Count == buffer.Length)
		{
			GrowIfNeeded(1);
		}
		buffer[Count++] = _item;
	}

	public void AddRange(int _count)
	{
		int num = Count + _count;
		if (num > buffer.Length)
		{
			Capacity = Math.Max(Math.Max(Capacity * 2, 4), num);
		}
		Count = num;
	}

	public void RemoveRange(int _index, int _count)
	{
		if (_count > 0)
		{
			Shift(_index, -_count);
		}
	}

	public void Clear()
	{
		Count = 0;
	}

	public void TrimExcess()
	{
		if (Count > 0)
		{
			if (Count < buffer.Length)
			{
				T[] destinationArray = new T[Count];
				Array.Copy(buffer, destinationArray, Count);
				buffer = destinationArray;
			}
		}
		else
		{
			buffer = emptyArray;
		}
	}

	public T[] FastToArray()
	{
		TrimExcess();
		return buffer;
	}

	public T[] ToArray()
	{
		T[] array = new T[Count];
		Array.Copy(buffer, array, Count);
		return array;
	}

	public void FromArray(ref T[] _array)
	{
		buffer = _array;
		Count = _array.Length;
		_array = null;
	}

	private void GrowIfNeeded(int _newCount)
	{
		int num = Count + _newCount;
		if (num > buffer.Length)
		{
			Capacity = Math.Max(Math.Max(Capacity * 2, 4), num);
		}
	}

	private void Shift(int start, int delta)
	{
		if (delta < 0)
		{
			start -= delta;
		}
		if (start < Count)
		{
			Array.Copy(buffer, start, buffer, start + delta, Count - start);
		}
		Count += delta;
		if (delta < 0)
		{
			Array.Clear(buffer, Count, -delta);
		}
	}
}
