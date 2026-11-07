using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class BitLocker : IPart
{
	public Dictionary<char, int> BitStorage = new Dictionary<char, int>();

	public override IPart DeepCopy(GameObject Parent)
	{
		BitLocker obj = base.DeepCopy(Parent) as BitLocker;
		obj.BitStorage = new Dictionary<char, int>(BitStorage);
		return obj;
	}

	public int GetTotalBitCount()
	{
		int num = 0;
		foreach (int value in BitStorage.Values)
		{
			num += value;
		}
		return num;
	}

	public string GetBitsString()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		char[] array = BitStorage.Keys.ToArray();
		BitType.SortBits(array);
		char[] array2 = array;
		foreach (char c in array2)
		{
			if (BitType.BitSortOrder.ContainsKey(c) && BitStorage[c] > 0)
			{
				stringBuilder.Append(BitType.GetString(c)).Append(" x{{C|").Append(BitStorage[c])
					.Append("}} - ")
					.Append(BitType.BitMap[c].Description)
					.Append("\n");
			}
		}
		if (stringBuilder.Length != 0)
		{
			return stringBuilder.ToString();
		}
		return "no bits";
	}

	public string GetBitsSummary()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		char[] array = BitStorage.Keys.ToArray();
		BitType.SortBits(array);
		char[] array2 = array;
		foreach (char c in array2)
		{
			if (BitType.BitSortOrder.ContainsKey(c) && BitStorage[c] > 0)
			{
				stringBuilder.Append(BitType.TranslateBit(c)).Append(BitStorage[c]);
			}
		}
		if (stringBuilder.Length != 0)
		{
			return stringBuilder.ToString();
		}
		return "-";
	}

	public void AddAllBits(int num)
	{
		foreach (char key in BitType.BitSortOrder.Keys)
		{
			if (!BitStorage.ContainsKey(key))
			{
				BitStorage.Add(key, num);
			}
			else
			{
				BitStorage[key] += num;
			}
		}
	}

	public void AddBits(string Bits)
	{
		for (int i = 0; i < Bits.Length; i++)
		{
			if (!BitType.BitSortOrder.ContainsKey(Bits[i]))
			{
				MetricsManager.LogWarning($"Couldn't find bit type {Bits[i]}");
				continue;
			}
			if (!BitStorage.ContainsKey(Bits[i]))
			{
				BitStorage.Add(Bits[i], 0);
			}
			BitStorage[Bits[i]]++;
		}
	}

	public bool UseBits(string Bits)
	{
		if (!HasBits(Bits))
		{
			return false;
		}
		for (int i = 0; i < Bits.Length; i++)
		{
			BitStorage[Bits[i]]--;
		}
		return true;
	}

	public bool UseBits(Dictionary<char, int> Bits)
	{
		if (!HasBits(Bits))
		{
			return false;
		}
		foreach (KeyValuePair<char, int> Bit in Bits)
		{
			if (BitStorage.ContainsKey(Bit.Key))
			{
				BitStorage[Bit.Key] -= Bit.Value;
			}
		}
		return true;
	}

	public bool UseBits(char Bit, int Number)
	{
		if (GetBitCount(Bit) < Number)
		{
			return false;
		}
		BitStorage[Bit] -= Number;
		return true;
	}

	public bool HasBits(string Bits)
	{
		return HasBits(new BitCost(Bits));
	}

	public bool HasBits(Dictionary<char, int> Bits)
	{
		foreach (KeyValuePair<char, int> Bit in Bits)
		{
			if (BitType.BitSortOrder.ContainsKey(Bit.Key))
			{
				if (!BitStorage.TryGetValue(Bit.Key, out var value))
				{
					return false;
				}
				if (value < Bit.Value)
				{
					return false;
				}
			}
		}
		return true;
	}

	public int GetBitCount(char Bit)
	{
		BitStorage.TryGetValue(Bit, out var value);
		return value;
	}

	public static bool HasBits(GameObject who, string Bits)
	{
		return who.GetPart<BitLocker>()?.HasBits(Bits) ?? false;
	}

	public static bool HasBits(GameObject who, Dictionary<char, int> Bits)
	{
		return who.GetPart<BitLocker>()?.HasBits(Bits) ?? false;
	}

	public static int GetBitCount(GameObject who, char Bit)
	{
		return who.GetPart<BitLocker>()?.GetBitCount(Bit) ?? 0;
	}

	public static bool UseBits(GameObject who, string Bits)
	{
		return who.GetPart<BitLocker>()?.UseBits(Bits) ?? false;
	}

	public static bool UseBits(GameObject who, Dictionary<char, int> Bits)
	{
		return who.GetPart<BitLocker>()?.UseBits(Bits) ?? false;
	}

	public static bool UseBits(GameObject who, char Bit, int Number)
	{
		return who.GetPart<BitLocker>()?.UseBits(Bit, Number) ?? false;
	}
}
