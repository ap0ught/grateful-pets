using System;
using System.Collections.Generic;
using System.Text;

namespace Genkit;

public static class Hash
{
	public const uint FNV32_OFFSET = 2166136261u;

	public const uint FNV32_PRIME = 16777619u;

	public const ulong FNV64_OFFSET = 14695981039346656037uL;

	public const ulong FNV64_PRIME = 1099511628211uL;

	public static readonly SortedSet<int> HASH_CAPACITY = new SortedSet<int>
	{
		3, 7, 11, 17, 23, 29, 37, 47, 59, 71,
		89, 107, 131, 163, 197, 239, 293, 353, 431, 521,
		631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
		4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
		25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
		156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
		968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
		5999471, 7199369, 7999379, 8499377, 8999371
	};

	public static readonly int[] HASH_POWERS = new int[17]
	{
		13, 31, 61, 127, 251, 509, 1021, 2039, 4093, 8191,
		16381, 32749, 65521, 131071, 262139, 524287, 1048573
	};

	public static int String(string read)
	{
		ulong num = 3074457345618258791uL;
		for (int i = 0; i < read.Length; i++)
		{
			num += read[i];
			num *= 3074457345618258799L;
		}
		return (int)(num >> 32);
	}

	public static uint FNV1A32(int Value, uint Hash = 2166136261u)
	{
		for (int i = 0; i < 4; i++)
		{
			Hash ^= (byte)Value;
			Hash *= 16777619;
			Value >>= 8;
		}
		return Hash;
	}

	public static uint FNV1A32(ReadOnlySpan<int> Value, uint Hash = 2166136261u)
	{
		int i = 0;
		for (int length = Value.Length; i < length; i++)
		{
			int num = Value[i];
			for (int j = 0; j < 4; j++)
			{
				Hash ^= (byte)num;
				Hash *= 16777619;
				num >>= 8;
			}
		}
		return Hash;
	}

	public static uint FNV1A32(string Value, uint Hash = 2166136261u)
	{
		int i = 0;
		for (int length = Value.Length; i < length; i++)
		{
			Hash ^= (byte)Value[i];
			Hash *= 16777619;
			Hash ^= (byte)((int)Value[i] >> 8);
			Hash *= 16777619;
		}
		return Hash;
	}

	public static uint FNV1A32(string Value, int Index, int Length, uint Hash = 2166136261u)
	{
		Length += Index;
		while (Index < Length)
		{
			Hash ^= (byte)Value[Index];
			Hash *= 16777619;
			Hash ^= (byte)((int)Value[Index++] >> 8);
			Hash *= 16777619;
		}
		return Hash;
	}

	public static uint FNV1A32(StringBuilder Value, int Index, int Length, uint Hash = 2166136261u)
	{
		Length += Index;
		while (Index < Length)
		{
			char c = Value[Index++];
			Hash ^= (byte)c;
			Hash *= 16777619;
			Hash ^= (byte)((int)c >> 8);
			Hash *= 16777619;
		}
		return Hash;
	}

	public static uint FNV1A32(ReadOnlySpan<char> Value, uint Hash = 2166136261u)
	{
		int i = 0;
		for (int length = Value.Length; i < length; i++)
		{
			Hash ^= (byte)Value[i];
			Hash *= 16777619;
			Hash ^= (byte)((int)Value[i] >> 8);
			Hash *= 16777619;
		}
		return Hash;
	}

	public static uint FNV1A32(ReadOnlySpan<char> Value, int Index, int Length, uint Hash = 2166136261u)
	{
		Length += Index;
		while (Index < Length)
		{
			Hash ^= (byte)Value[Index];
			Hash *= 16777619;
			Hash ^= (byte)((int)Value[Index++] >> 8);
			Hash *= 16777619;
		}
		return Hash;
	}

	public static uint FNV1A32(IList<string> Values, uint Hash = 2166136261u)
	{
		int i = 0;
		for (int count = Values.Count; i < count; i++)
		{
			string text = Values[i];
			int j = 0;
			for (int length = text.Length; j < length; j++)
			{
				Hash ^= (byte)text[j];
				Hash *= 16777619;
				Hash ^= (byte)((int)text[j] >> 8);
				Hash *= 16777619;
			}
		}
		return Hash;
	}

	public static ulong FNV1A64(int Value, ulong Hash = 14695981039346656037uL)
	{
		for (int i = 0; i < 4; i++)
		{
			Hash ^= (byte)Value;
			Hash *= 1099511628211L;
			Value >>= 8;
		}
		return Hash;
	}

	public static ulong FNV1A64(string Value, ulong Hash = 14695981039346656037uL)
	{
		int i = 0;
		for (int length = Value.Length; i < length; i++)
		{
			Hash ^= (byte)Value[i];
			Hash *= 1099511628211L;
			Hash ^= (byte)((int)Value[i] >> 8);
			Hash *= 1099511628211L;
		}
		return Hash;
	}

	public static ulong FNV1A64(string Value, int Index, int Length, ulong Hash = 14695981039346656037uL)
	{
		Length += Index;
		while (Index < Length)
		{
			Hash ^= (byte)Value[Index];
			Hash *= 1099511628211L;
			Hash ^= (byte)((int)Value[Index++] >> 8);
			Hash *= 1099511628211L;
		}
		return Hash;
	}

	public static ulong FNV1A64(StringBuilder Value, int Index, int Length, ulong Hash = 14695981039346656037uL)
	{
		Length += Index;
		while (Index < Length)
		{
			char c = Value[Index++];
			Hash ^= (byte)c;
			Hash *= 1099511628211L;
			Hash ^= (byte)((int)c >> 8);
			Hash *= 1099511628211L;
		}
		return Hash;
	}

	public static ulong FNV1A64(ReadOnlySpan<char> Value, ulong Hash = 14695981039346656037uL)
	{
		int i = 0;
		for (int length = Value.Length; i < length; i++)
		{
			Hash ^= (byte)Value[i];
			Hash *= 1099511628211L;
			Hash ^= (byte)((int)Value[i] >> 8);
			Hash *= 1099511628211L;
		}
		return Hash;
	}

	public static ulong FNV1A64(ReadOnlySpan<char> Value, int Index, int Length, ulong Hash = 14695981039346656037uL)
	{
		Length += Index;
		while (Index < Length)
		{
			Hash ^= (byte)Value[Index];
			Hash *= 1099511628211L;
			Hash ^= (byte)((int)Value[Index++] >> 8);
			Hash *= 1099511628211L;
		}
		return Hash;
	}

	public static ulong FNV1A64(IList<string> Values, ulong Hash = 14695981039346656037uL)
	{
		int i = 0;
		for (int count = Values.Count; i < count; i++)
		{
			string text = Values[i];
			int j = 0;
			for (int length = text.Length; j < length; j++)
			{
				Hash ^= (byte)text[j];
				Hash *= 1099511628211L;
				Hash ^= (byte)((int)text[j] >> 8);
				Hash *= 1099511628211L;
			}
		}
		return Hash;
	}

	public static bool IsPrime(int Number)
	{
		if ((Number & 1) != 0)
		{
			int num = (int)Math.Sqrt(Number);
			for (int i = 3; i <= num; i += 2)
			{
				if (Number % i == 0)
				{
					return false;
				}
			}
			return true;
		}
		return Number == 2;
	}

	public static int GetSharedCapacity(int Minimum)
	{
		int[] hASH_POWERS = HASH_POWERS;
		foreach (int num in hASH_POWERS)
		{
			if (num >= Minimum)
			{
				return num;
			}
		}
		return GetCapacity(Minimum);
	}

	public static int GetCapacity(int Minimum)
	{
		foreach (int item in HASH_CAPACITY)
		{
			if (item >= Minimum)
			{
				return item;
			}
		}
		for (int i = Minimum | 1; i < int.MaxValue; i += 2)
		{
			if (IsPrime(i))
			{
				HASH_CAPACITY.Add(i);
				return i;
			}
		}
		return Minimum;
	}
}
