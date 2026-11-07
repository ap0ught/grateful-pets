using System;
using System.Collections.Generic;
using Genkit;

namespace XRL.World.Conversations;

[Serializable]
public class FixedHashSet : IComposite
{
	private struct Slot
	{
		public ulong Hash;

		public int Next;
	}

	private int[] Buckets = Array.Empty<int>();

	private Slot[] Slots = Array.Empty<Slot>();

	private int Size;

	private int Last;

	private int Free = -1;

	private int Length;

	public int Count => Length;

	public FixedHashSet()
	{
	}

	public FixedHashSet(ICollection<ulong> Range)
	{
		Resize(Range.Count);
		foreach (ulong item in Range)
		{
			Add(item);
		}
	}

	public bool Contains(ulong Hash)
	{
		if (Length == 0)
		{
			return false;
		}
		ulong num = Hash % (ulong)Size;
		for (int num2 = Buckets[num] - 1; num2 >= 0; num2 = Slots[num2].Next)
		{
			if (Slots[num2].Hash == Hash)
			{
				return true;
			}
		}
		return false;
	}

	public bool Add(ulong Hash)
	{
		ulong num = 0uL;
		if (Length > 0)
		{
			num = Hash % (ulong)Size;
			for (int num2 = Buckets[num] - 1; num2 >= 0; num2 = Slots[num2].Next)
			{
				if (Slots[num2].Hash == Hash)
				{
					return false;
				}
			}
		}
		int num3;
		if (Free >= 0)
		{
			num3 = Free;
			Free = Slots[num3].Next;
		}
		else
		{
			if (Last == Size)
			{
				Resize(Size * 2);
				num = Hash % (ulong)Size;
			}
			num3 = Last;
			Last++;
		}
		Slots[num3].Hash = Hash;
		Slots[num3].Next = Buckets[num] - 1;
		Buckets[num] = num3 + 1;
		Length++;
		return true;
	}

	public bool Remove(ulong Hash)
	{
		ulong num = Hash % (ulong)Size;
		int num2 = -1;
		for (int num3 = Buckets[num] - 1; num3 >= 0; num3 = Slots[num3].Next)
		{
			if (Slots[num3].Hash == Hash)
			{
				if (num2 < 0)
				{
					Buckets[num] = Slots[num3].Next + 1;
				}
				else
				{
					Slots[num2].Next = Slots[num3].Next;
				}
				Slots[num3].Hash = 0uL;
				Slots[num3].Next = Free;
				if (--Length == 0)
				{
					Last = 0;
					Free = -1;
				}
				else
				{
					Free = num3;
				}
				return true;
			}
			num2 = num3;
		}
		return false;
	}

	public void Clear()
	{
		if (Length > 0)
		{
			Array.Clear(Buckets, 0, Size);
			Array.Clear(Slots, 0, Last);
			Free = -1;
			Length = 0;
			Last = 0;
		}
	}

	protected void Resize(int Capacity)
	{
		Capacity = Hash.GetCapacity(Capacity);
		if (Capacity != Size)
		{
			int[] array = new int[Capacity];
			Slot[] array2 = new Slot[Capacity];
			for (int i = 0; i < Last; i++)
			{
				ulong num = Slots[i].Hash % (ulong)Capacity;
				array2[i].Hash = Slots[i].Hash;
				array2[i].Next = array[num] - 1;
				array[num] = i + 1;
			}
			Buckets = array;
			Slots = array2;
			Size = Capacity;
		}
	}

	public ulong[] GetValues()
	{
		ulong[] array = new ulong[Length];
		int i = 0;
		int num = 0;
		for (; i < Last; i++)
		{
			if (Slots[i].Hash != 0L)
			{
				array[num++] = Slots[i].Hash;
			}
		}
		return array;
	}

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Length);
		int num = 0;
		for (int i = 0; i < Last; i++)
		{
			if (Slots[i].Hash != 0L)
			{
				Writer.WriteOptimized(Slots[i].Hash);
				num++;
			}
		}
		if (num != Length)
		{
			MetricsManager.LogError($"Bad news {num} vs {Length}");
		}
	}

	public void Read(SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		if (num > Size)
		{
			Resize(num);
		}
		for (int i = 0; i < num; i++)
		{
			Add(Reader.ReadOptimizedUInt64());
		}
	}
}
