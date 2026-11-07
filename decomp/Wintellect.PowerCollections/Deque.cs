using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public class Deque<T> : ListBase<T>, ICloneable
{
	private const int INITIAL_SIZE = 8;

	private T[] buffer;

	private int start;

	private int end;

	private int changeStamp;

	public sealed override int Count
	{
		get
		{
			if (end >= start)
			{
				return end - start;
			}
			return end + buffer.Length - start;
		}
	}

	public int Capacity
	{
		get
		{
			if (buffer == null)
			{
				return 0;
			}
			return buffer.Length - 1;
		}
		set
		{
			if (value < Count)
			{
				throw new ArgumentOutOfRangeException("value", Strings.CapacityLessThanCount);
			}
			if (value > 2147483646)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (value != Capacity)
			{
				T[] array = new T[value + 1];
				CopyTo(array, 0);
				end = Count;
				start = 0;
				buffer = array;
			}
		}
	}

	public sealed override T this[int index]
	{
		get
		{
			int num = index + start;
			if (num < start)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (end >= start)
			{
				if (num >= end)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return buffer[num];
			}
			int num2 = buffer.Length;
			if (num >= num2)
			{
				num -= num2;
				if (num >= end)
				{
					throw new ArgumentOutOfRangeException("index");
				}
			}
			return buffer[num];
		}
		set
		{
			StopEnumerations();
			int num = index + start;
			if (num < start)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (end >= start)
			{
				if (num >= end)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				buffer[num] = value;
				return;
			}
			int num2 = buffer.Length;
			if (num >= num2)
			{
				num -= num2;
				if (num >= end)
				{
					throw new ArgumentOutOfRangeException("index");
				}
			}
			buffer[num] = value;
		}
	}

	private void StopEnumerations()
	{
		changeStamp++;
	}

	private void CheckEnumerationStamp(int startStamp)
	{
		if (startStamp != changeStamp)
		{
			throw new InvalidOperationException(Strings.ChangeDuringEnumeration);
		}
	}

	public Deque()
	{
	}

	public Deque(IEnumerable<T> collection)
	{
		AddManyToBack(collection);
	}

	public sealed override void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int num = ((buffer != null) ? buffer.Length : 0);
		if (start > end)
		{
			Array.Copy(buffer, start, array, arrayIndex, num - start);
			Array.Copy(buffer, 0, array, arrayIndex + num - start, end);
		}
		else if (end > start)
		{
			Array.Copy(buffer, start, array, arrayIndex, end - start);
		}
	}

	public void TrimToSize()
	{
		Capacity = Count;
	}

	public sealed override void Clear()
	{
		StopEnumerations();
		buffer = null;
		start = (end = 0);
	}

	public sealed override IEnumerator<T> GetEnumerator()
	{
		int startStamp = changeStamp;
		int count = Count;
		int i = 0;
		while (i < count)
		{
			yield return this[i];
			CheckEnumerationStamp(startStamp);
			int num = i + 1;
			i = num;
		}
	}

	private void CreateInitialBuffer(T firstItem)
	{
		buffer = new T[8];
		start = 0;
		end = 1;
		buffer[0] = firstItem;
	}

	public sealed override void Insert(int index, T item)
	{
		StopEnumerations();
		int count = Count;
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (buffer == null)
		{
			CreateInitialBuffer(item);
			return;
		}
		int num = buffer.Length;
		int num2;
		if (index < count / 2)
		{
			start--;
			if (start < 0)
			{
				start += num;
			}
			num2 = index + start;
			if (num2 >= num)
			{
				num2 -= num;
				if (num - 1 > start)
				{
					Array.Copy(buffer, start + 1, buffer, start, num - 1 - start);
				}
				buffer[num - 1] = buffer[0];
				if (num2 > 0)
				{
					Array.Copy(buffer, 1, buffer, 0, num2);
				}
			}
			else if (num2 > start)
			{
				Array.Copy(buffer, start + 1, buffer, start, num2 - start);
			}
		}
		else
		{
			num2 = index + start;
			if (num2 >= num)
			{
				num2 -= num;
			}
			if (num2 <= end)
			{
				if (end > num2)
				{
					Array.Copy(buffer, num2, buffer, num2 + 1, end - num2);
				}
				end++;
				if (end >= num)
				{
					end -= num;
				}
			}
			else
			{
				if (end > 0)
				{
					Array.Copy(buffer, 0, buffer, 1, end);
				}
				buffer[0] = buffer[num - 1];
				if (num - 1 > num2)
				{
					Array.Copy(buffer, num2, buffer, num2 + 1, num - 1 - num2);
				}
				end++;
			}
		}
		buffer[num2] = item;
		if (start == end)
		{
			IncreaseBuffer();
		}
	}

	public void InsertRange(int index, IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		StopEnumerations();
		int count = Count;
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		ICollection<T> collection2 = ((!(collection is ICollection<T>)) ? new List<T>(collection) : ((ICollection<T>)collection));
		if (collection2.Count == 0)
		{
			return;
		}
		if (Capacity < Count + collection2.Count)
		{
			Capacity = Count + collection2.Count;
		}
		int num = buffer.Length;
		int num3;
		if (index < count / 2)
		{
			int num2 = start;
			num3 = num2 - collection2.Count;
			if (num3 < 0)
			{
				num3 += num;
			}
			start = num3;
			int num4 = index;
			while (num4 > 0)
			{
				int num5 = num4;
				if (num - num3 < num5)
				{
					num5 = num - num3;
				}
				if (num - num2 < num5)
				{
					num5 = num - num2;
				}
				Array.Copy(buffer, num2, buffer, num3, num5);
				num4 -= num5;
				if ((num3 += num5) >= num)
				{
					num3 -= num;
				}
				if ((num2 += num5) >= num)
				{
					num2 -= num;
				}
			}
		}
		else
		{
			int num2 = end;
			num3 = num2 + collection2.Count;
			if (num3 >= num)
			{
				num3 -= num;
			}
			end = num3;
			int num6 = count - index;
			while (num6 > 0)
			{
				int num7 = num6;
				if (num3 > 0 && num3 < num7)
				{
					num7 = num3;
				}
				if (num2 > 0 && num2 < num7)
				{
					num7 = num2;
				}
				if ((num3 -= num7) < 0)
				{
					num3 += num;
				}
				if ((num2 -= num7) < 0)
				{
					num2 += num;
				}
				Array.Copy(buffer, num2, buffer, num3, num7);
				num6 -= num7;
			}
			num3 -= collection2.Count;
			if (num3 < 0)
			{
				num3 += num;
			}
		}
		foreach (T item in collection2)
		{
			buffer[num3] = item;
			if (++num3 >= num)
			{
				num3 -= num;
			}
		}
	}

	public sealed override void RemoveAt(int index)
	{
		StopEnumerations();
		int count = Count;
		if (index < 0 || index >= count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int num = buffer.Length;
		int num2;
		if (index < count / 2)
		{
			num2 = index + start;
			if (num2 >= num)
			{
				num2 -= num;
				if (num2 > 0)
				{
					Array.Copy(buffer, 0, buffer, 1, num2);
				}
				buffer[0] = buffer[num - 1];
				if (num - 1 > start)
				{
					Array.Copy(buffer, start, buffer, start + 1, num - 1 - start);
				}
			}
			else if (num2 > start)
			{
				Array.Copy(buffer, start, buffer, start + 1, num2 - start);
			}
			buffer[start] = default;
			start++;
			if (start >= num)
			{
				start -= num;
			}
			return;
		}
		num2 = index + start;
		if (num2 >= num)
		{
			num2 -= num;
		}
		end--;
		if (end < 0)
		{
			end = num - 1;
		}
		if (num2 <= end)
		{
			if (end > num2)
			{
				Array.Copy(buffer, num2 + 1, buffer, num2, end - num2);
			}
		}
		else
		{
			if (num - 1 > num2)
			{
				Array.Copy(buffer, num2 + 1, buffer, num2, num - 1 - num2);
			}
			buffer[num - 1] = buffer[0];
			if (end > 0)
			{
				Array.Copy(buffer, 1, buffer, 0, end);
			}
		}
		buffer[end] = default;
	}

	public void RemoveRange(int index, int count)
	{
		StopEnumerations();
		int count2 = Count;
		if (index < 0 || index >= count2)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0 || count > count2 - index)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count == 0)
		{
			return;
		}
		int num = buffer.Length;
		int num2;
		int num3;
		if (index < count2 / 2)
		{
			num2 = start + index;
			if (num2 >= num)
			{
				num2 -= num;
			}
			num3 = num2 + count;
			if (num3 >= num)
			{
				num3 -= num;
			}
			int num4 = index;
			while (num4 > 0)
			{
				int num5 = num4;
				if (num3 > 0 && num3 < num5)
				{
					num5 = num3;
				}
				if (num2 > 0 && num2 < num5)
				{
					num5 = num2;
				}
				if ((num3 -= num5) < 0)
				{
					num3 += num;
				}
				if ((num2 -= num5) < 0)
				{
					num2 += num;
				}
				Array.Copy(buffer, num2, buffer, num3, num5);
				num4 -= num5;
			}
			for (num4 = 0; num4 < count; num4++)
			{
				buffer[num2] = default;
				if (++num2 >= num)
				{
					num2 -= num;
				}
			}
			start = num2;
			return;
		}
		int num6 = count2 - index - count;
		num2 = end - num6;
		if (num2 < 0)
		{
			num2 += num;
		}
		num3 = num2 - count;
		if (num3 < 0)
		{
			num3 += num;
		}
		int num7 = num6;
		while (num7 > 0)
		{
			int num8 = num7;
			if (num - num3 < num8)
			{
				num8 = num - num3;
			}
			if (num - num2 < num8)
			{
				num8 = num - num2;
			}
			Array.Copy(buffer, num2, buffer, num3, num8);
			num7 -= num8;
			if ((num3 += num8) >= num)
			{
				num3 -= num;
			}
			if ((num2 += num8) >= num)
			{
				num2 -= num;
			}
		}
		for (num7 = 0; num7 < count; num7++)
		{
			if (--num2 < 0)
			{
				num2 += num;
			}
			buffer[num2] = default;
		}
		end = num2;
	}

	private void IncreaseBuffer()
	{
		_ = Count;
		int num = buffer.Length;
		T[] destinationArray = new T[num * 2];
		if (start >= end)
		{
			Array.Copy(buffer, start, destinationArray, 0, num - start);
			Array.Copy(buffer, 0, destinationArray, num - start, end);
			end = end + num - start;
			start = 0;
		}
		else
		{
			Array.Copy(buffer, start, destinationArray, 0, end - start);
			end -= start;
			start = 0;
		}
		buffer = destinationArray;
	}

	public void AddToFront(T item)
	{
		StopEnumerations();
		if (buffer == null)
		{
			CreateInitialBuffer(item);
			return;
		}
		if (--start < 0)
		{
			start += buffer.Length;
		}
		buffer[start] = item;
		if (start == end)
		{
			IncreaseBuffer();
		}
	}

	public void AddManyToFront(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		InsertRange(0, collection);
	}

	public void AddToBack(T item)
	{
		StopEnumerations();
		if (buffer == null)
		{
			CreateInitialBuffer(item);
			return;
		}
		buffer[end] = item;
		if (++end >= buffer.Length)
		{
			end -= buffer.Length;
		}
		if (start == end)
		{
			IncreaseBuffer();
		}
	}

	public sealed override void Add(T item)
	{
		AddToBack(item);
	}

	public void AddManyToBack(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		foreach (T item in collection)
		{
			AddToBack(item);
		}
	}

	public T RemoveFromFront()
	{
		if (start == end)
		{
			throw new InvalidOperationException(Strings.CollectionIsEmpty);
		}
		StopEnumerations();
		T result = buffer[start];
		buffer[start] = default;
		if (++start >= buffer.Length)
		{
			start -= buffer.Length;
		}
		return result;
	}

	public T RemoveFromBack()
	{
		if (start == end)
		{
			throw new InvalidOperationException(Strings.CollectionIsEmpty);
		}
		StopEnumerations();
		if (--end < 0)
		{
			end += buffer.Length;
		}
		T result = buffer[end];
		buffer[end] = default;
		return result;
	}

	public T GetAtFront()
	{
		if (start == end)
		{
			throw new InvalidOperationException(Strings.CollectionIsEmpty);
		}
		return buffer[start];
	}

	public T GetAtBack()
	{
		if (start == end)
		{
			throw new InvalidOperationException(Strings.CollectionIsEmpty);
		}
		if (end == 0)
		{
			return buffer[buffer.Length - 1];
		}
		return buffer[end - 1];
	}

	public Deque<T> Clone()
	{
		return new Deque<T>(this);
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public Deque<T> CloneContents()
	{
		if (!Util.IsCloneableType(typeof(T), out var isValue))
		{
			throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof(T).FullName));
		}
		Deque<T> deque = new Deque<T>();
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
			deque.AddToBack(item);
		}
		return deque;
	}
}
