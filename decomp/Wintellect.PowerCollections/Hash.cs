using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Wintellect.PowerCollections;

[Serializable]
internal class Hash<T> : IEnumerable<T>, IEnumerable, ISerializable, IDeserializationCallback
{
	private struct Slot
	{
		private uint hash_collision;

		public T item;

		public int HashValue
		{
			get
			{
				return (int)(hash_collision & 0x7FFFFFFF);
			}
			set
			{
				hash_collision = (uint)value | (hash_collision & 0x80000000u);
			}
		}

		public bool Empty => HashValue == 0;

		public bool Collision
		{
			get
			{
				return (hash_collision & 0x80000000u) != 0;
			}
			set
			{
				if (value)
				{
					hash_collision |= 2147483648u;
				}
				else
				{
					hash_collision &= 2147483647u;
				}
			}
		}

		public void Clear()
		{
			HashValue = 0;
			item = default;
		}
	}

	private IEqualityComparer<T> equalityComparer;

	private int count;

	private int usedSlots;

	private int totalSlots;

	private float loadFactor;

	private int thresholdGrow;

	private int thresholdShrink;

	private int hashMask;

	private int secondaryShift;

	private Slot[] table;

	private int changeStamp;

	private const int MINSIZE = 16;

	private SerializationInfo serializationInfo;

	public int ElementCount => count;

	internal int SlotCount => totalSlots;

	public float LoadFactor
	{
		get
		{
			return loadFactor;
		}
		set
		{
			if ((double)value < 0.25 || (double)value > 0.95)
			{
				throw new ArgumentOutOfRangeException("value", value, Strings.InvalidLoadFactor);
			}
			StopEnumerations();
			bool flag = value < loadFactor;
			loadFactor = value;
			thresholdGrow = (int)((float)totalSlots * loadFactor);
			thresholdShrink = thresholdGrow / 3;
			if (thresholdShrink <= 16)
			{
				thresholdShrink = 1;
			}
			if (flag)
			{
				EnsureEnoughSlots(0);
			}
			else
			{
				ShrinkIfNeeded();
			}
		}
	}

	public Hash(IEqualityComparer<T> equalityComparer)
	{
		this.equalityComparer = equalityComparer;
		loadFactor = 0.7f;
	}

	internal int GetEnumerationStamp()
	{
		return changeStamp;
	}

	internal void StopEnumerations()
	{
		changeStamp++;
	}

	internal void CheckEnumerationStamp(int startStamp)
	{
		if (startStamp != changeStamp)
		{
			throw new InvalidOperationException(Strings.ChangeDuringEnumeration);
		}
	}

	private int GetFullHash(T item)
	{
		uint hashCode = (uint)Util.GetHashCode(item, equalityComparer);
		hashCode += ~(hashCode << 15);
		hashCode ^= hashCode >> 10;
		hashCode += hashCode << 3;
		hashCode ^= hashCode >> 6;
		hashCode += ~(hashCode << 11);
		hashCode ^= hashCode >> 16;
		hashCode &= 0x7FFFFFFF;
		if (hashCode == 0)
		{
			hashCode = 2147483647u;
		}
		return (int)hashCode;
	}

	private void GetHashValuesFromFullHash(int hash, out int initialBucket, out int skip)
	{
		initialBucket = hash & hashMask;
		skip = ((hash >> secondaryShift) & hashMask) | 1;
	}

	private int GetHashValues(T item, out int initialBucket, out int skip)
	{
		int fullHash = GetFullHash(item);
		GetHashValuesFromFullHash(fullHash, out initialBucket, out skip);
		return fullHash;
	}

	private void EnsureEnoughSlots(int additionalItems)
	{
		StopEnumerations();
		if (usedSlots + additionalItems <= thresholdGrow)
		{
			return;
		}
		int num = Math.Max(totalSlots, 16);
		while ((int)((float)num * loadFactor) < usedSlots + additionalItems)
		{
			num *= 2;
			if (num <= 0)
			{
				throw new InvalidOperationException(Strings.CollectionTooLarge);
			}
		}
		ResizeTable(num);
	}

	private void ShrinkIfNeeded()
	{
		if (count >= thresholdShrink)
		{
			return;
		}
		int num;
		if (count > 0)
		{
			num = 16;
			while ((int)((float)num * loadFactor) < count)
			{
				num *= 2;
			}
		}
		else
		{
			num = 0;
		}
		ResizeTable(num);
	}

	private int GetSecondaryShift(int newSize)
	{
		int num = newSize - 2;
		int num2 = 0;
		while ((num & 0x40000000) == 0)
		{
			num <<= 1;
			num2++;
		}
		return num2;
	}

	private void ResizeTable(int newSize)
	{
		Slot[] array = table;
		totalSlots = newSize;
		thresholdGrow = (int)((float)totalSlots * loadFactor);
		thresholdShrink = thresholdGrow / 3;
		if (thresholdShrink <= 16)
		{
			thresholdShrink = 1;
		}
		hashMask = newSize - 1;
		secondaryShift = GetSecondaryShift(newSize);
		if (totalSlots > 0)
		{
			table = new Slot[totalSlots];
		}
		else
		{
			table = null;
		}
		if (array != null && table != null)
		{
			Slot[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Slot slot = array2[i];
				int hashValue = slot.HashValue;
				GetHashValuesFromFullHash(hashValue, out var initialBucket, out var skip);
				while (!table[initialBucket].Empty)
				{
					table[initialBucket].Collision = true;
					initialBucket = (initialBucket + skip) & hashMask;
				}
				table[initialBucket].HashValue = hashValue;
				table[initialBucket].item = slot.item;
			}
		}
		usedSlots = count;
	}

	public bool Insert(T item, bool replaceOnDuplicate, out T previous)
	{
		int num = -1;
		bool flag = true;
		EnsureEnoughSlots(1);
		int hashValues = GetHashValues(item, out var initialBucket, out var skip);
		while (true)
		{
			if (table[initialBucket].Empty)
			{
				if (num == -1)
				{
					num = initialBucket;
				}
				if (!flag || !table[initialBucket].Collision)
				{
					break;
				}
			}
			else
			{
				if (table[initialBucket].HashValue == hashValues && equalityComparer.Equals(table[initialBucket].item, item))
				{
					previous = table[initialBucket].item;
					if (replaceOnDuplicate)
					{
						table[initialBucket].item = item;
					}
					return false;
				}
				if (!table[initialBucket].Collision)
				{
					if (num >= 0)
					{
						break;
					}
					table[initialBucket].Collision = true;
					flag = false;
				}
			}
			initialBucket = (initialBucket + skip) & hashMask;
		}
		table[num].HashValue = hashValues;
		table[num].item = item;
		count++;
		if (!table[num].Collision)
		{
			usedSlots++;
		}
		previous = default;
		return true;
	}

	public bool Delete(T item, out T itemDeleted)
	{
		StopEnumerations();
		if (count == 0)
		{
			itemDeleted = default;
			return false;
		}
		int hashValues = GetHashValues(item, out var initialBucket, out var skip);
		while (true)
		{
			if (table[initialBucket].HashValue == hashValues && equalityComparer.Equals(table[initialBucket].item, item))
			{
				itemDeleted = table[initialBucket].item;
				table[initialBucket].Clear();
				count--;
				if (!table[initialBucket].Collision)
				{
					usedSlots--;
				}
				ShrinkIfNeeded();
				return true;
			}
			if (!table[initialBucket].Collision)
			{
				break;
			}
			initialBucket = (initialBucket + skip) & hashMask;
		}
		itemDeleted = default;
		return false;
	}

	public bool Find(T find, bool replace, out T item)
	{
		if (count == 0)
		{
			item = default;
			return false;
		}
		int hashValues = GetHashValues(find, out var initialBucket, out var skip);
		while (true)
		{
			if (table[initialBucket].HashValue == hashValues && equalityComparer.Equals(table[initialBucket].item, find))
			{
				item = table[initialBucket].item;
				if (replace)
				{
					table[initialBucket].item = find;
				}
				return true;
			}
			if (!table[initialBucket].Collision)
			{
				break;
			}
			initialBucket = (initialBucket + skip) & hashMask;
		}
		item = default;
		return false;
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (count <= 0)
		{
			yield break;
		}
		int startStamp = changeStamp;
		Slot[] array = table;
		for (int i = 0; i < array.Length; i++)
		{
			Slot slot = array[i];
			if (!slot.Empty)
			{
				yield return slot.item;
				CheckEnumerationStamp(startStamp);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public Hash<T> Clone(Converter<T, T> cloneItem)
	{
		Hash<T> hash = new Hash<T>(equalityComparer);
		hash.count = count;
		hash.usedSlots = usedSlots;
		hash.totalSlots = totalSlots;
		hash.loadFactor = loadFactor;
		hash.thresholdGrow = thresholdGrow;
		hash.thresholdShrink = thresholdShrink;
		hash.hashMask = hashMask;
		hash.secondaryShift = secondaryShift;
		if (table != null)
		{
			hash.table = (Slot[])table.Clone();
			if (cloneItem != null)
			{
				for (int i = 0; i < table.Length; i++)
				{
					if (!table[i].Empty)
					{
						table[i].item = cloneItem(table[i].item);
					}
				}
			}
		}
		return hash;
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("equalityComparer", equalityComparer, typeof(IEqualityComparer<T>));
		info.AddValue("loadFactor", loadFactor, typeof(float));
		T[] array = new T[count];
		int num = 0;
		Slot[] array2 = table;
		for (int i = 0; i < array2.Length; i++)
		{
			Slot slot = array2[i];
			if (!slot.Empty)
			{
				array[num++] = slot.item;
			}
		}
		info.AddValue("items", array, typeof(T[]));
	}

	protected Hash(SerializationInfo serInfo, StreamingContext context)
	{
		serializationInfo = serInfo;
	}

	void IDeserializationCallback.OnDeserialization(object sender)
	{
		if (serializationInfo != null)
		{
			loadFactor = serializationInfo.GetSingle("loadFactor");
			equalityComparer = (IEqualityComparer<T>)serializationInfo.GetValue("equalityComparer", typeof(IEqualityComparer<T>));
			T[] array = (T[])serializationInfo.GetValue("items", typeof(T[]));
			EnsureEnoughSlots(array.Length);
			T[] array2 = array;
			foreach (T item in array2)
			{
				Insert(item, replaceOnDuplicate: true, out var _);
			}
			serializationInfo = null;
		}
	}
}
