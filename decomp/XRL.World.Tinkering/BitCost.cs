using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World.Tinkering;

public class BitCost : Dictionary<char, int>
{
	[NonSerialized]
	private static StringBuilder SB = new StringBuilder();

	public int Total
	{
		get
		{
			int num = 0;
			foreach (int value in Values)
			{
				num += value;
			}
			return num;
		}
	}

	public BitCost()
		: base(BitType.BitTypes.Count)
	{
	}

	public BitCost(int Size)
		: base(Size)
	{
	}

	public BitCost(string Bits)
		: this(Math.Min(BitType.BitTypes.Count, Bits.Length))
	{
		Import(Bits);
	}

	public void Import(string Bits)
	{
		int i = 0;
		for (int length = Bits.Length; i < length; i++)
		{
			this.Increment(Bits[i]);
		}
	}

	public void CopyTo(BitCost Other)
	{
		Other.Clear();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<char, int> current = enumerator.Current;
			Other[current.Key] = current.Value;
		}
	}

	public void AddTo(BitCost Other)
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<char, int> current = enumerator.Current;
			int i = 0;
			for (int value = current.Value; i < value; i++)
			{
				Other.Increment(current.Key);
			}
		}
	}

	public string ToBits()
	{
		SB.Clear();
		int i = 0;
		for (int count = BitType.BitOrder.Count; i < count; i++)
		{
			char c = BitType.BitOrder[i];
			if (TryGetValue(c, out var value))
			{
				for (int j = 0; j < value; j++)
				{
					SB.Append(c);
				}
			}
		}
		return SB.ToString();
	}

	public override string ToString()
	{
		SB.Clear();
		ToStringBuilder(SB);
		return SB.ToString();
	}

	public void ToStringBuilder(StringBuilder SB)
	{
		if (Options.AlphanumericBits)
		{
			int i = 0;
			for (int count = BitType.BitOrder.Count; i < count; i++)
			{
				char c = BitType.BitOrder[i];
				if (TryGetValue(c, out var value))
				{
					char value2 = BitType.CharTranslateBit(c);
					SB.Append("{{").Append(c).Append('|');
					for (int j = 0; j < value; j++)
					{
						SB.Append(value2);
					}
					SB.Append("}}");
				}
			}
			return;
		}
		int k = 0;
		for (int count2 = BitType.BitOrder.Count; k < count2; k++)
		{
			char c2 = BitType.BitOrder[k];
			if (TryGetValue(c2, out var value3))
			{
				BitType.CharTranslateBit(c2);
				SB.Append("{{").Append(c2).Append('|');
				for (int l = 0; l < value3; l++)
				{
					SB.Append('\a');
				}
				SB.Append("}}");
			}
		}
	}

	public int GetHighestTier()
	{
		int num = 0;
		foreach (char key in Keys)
		{
			int bitTier = BitType.GetBitTier(key);
			if (bitTier > num)
			{
				num = bitTier;
			}
		}
		return num;
	}
}
