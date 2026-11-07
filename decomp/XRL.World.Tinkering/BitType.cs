using System;
using System.Collections.Generic;
using System.Text;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Tinkering;

[HasModSensitiveStaticCache]
public class BitType
{
	public int Level;

	public char Color;

	public string Description;

	[ModSensitiveStaticCache(false)]
	private static List<BitType> _BitTypes;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<char, BitType> _BitMap;

	[ModSensitiveStaticCache(false)]
	public static Dictionary<int, List<BitType>> _LevelMap;

	[ModSensitiveStaticCache(false)]
	public static List<char> _BitOrder;

	[ModSensitiveStaticCache(false)]
	public static Dictionary<char, int> _BitSortOrder;

	[ModSensitiveStaticCache(false)]
	public static Dictionary<int, char> _TierBits;

	public static List<BitType> BitTypes
	{
		get
		{
			CheckInit();
			return _BitTypes;
		}
	}

	public static Dictionary<char, BitType> BitMap
	{
		get
		{
			CheckInit();
			return _BitMap;
		}
	}

	public static Dictionary<int, List<BitType>> LevelMap
	{
		get
		{
			CheckInit();
			return _LevelMap;
		}
	}

	public static List<char> BitOrder
	{
		get
		{
			CheckInit();
			return _BitOrder;
		}
	}

	public static Dictionary<char, int> BitSortOrder
	{
		get
		{
			CheckInit();
			return _BitSortOrder;
		}
	}

	public static Dictionary<int, char> TierBits
	{
		get
		{
			CheckInit();
			return _TierBits;
		}
	}

	public static int GetBitSortOrder(char bit)
	{
		int value = 0;
		if (BitSortOrder.TryGetValue(bit, out value))
		{
			return value;
		}
		return 0;
	}

	public static void CheckInit()
	{
		if (_BitTypes == null)
		{
			Loading.LoadTask("Initialize tinkering", Init);
		}
	}

	public BitType(int Level, char Color, string Description)
	{
		this.Level = Level;
		this.Color = Color;
		this.Description = Description;
	}

	public static string TranslateBit(char Bit)
	{
		return Bit switch
		{
			'0' => "A", 
			'R' => "A", 
			'G' => "B", 
			'B' => "C", 
			'C' => "D", 
			'r' => "1", 
			'g' => "2", 
			'b' => "3", 
			'c' => "4", 
			'K' => "5", 
			'W' => "6", 
			'Y' => "7", 
			'M' => "8", 
			_ => "?", 
		};
	}

	public static char CharTranslateBit(char Bit)
	{
		return Bit switch
		{
			'0' => 'A', 
			'R' => 'A', 
			'G' => 'B', 
			'B' => 'C', 
			'C' => 'D', 
			'r' => '1', 
			'g' => '2', 
			'b' => '3', 
			'c' => '4', 
			'K' => '5', 
			'W' => '6', 
			'Y' => '7', 
			'M' => '8', 
			_ => '?', 
		};
	}

	public static int GetBitTier(char Bit)
	{
		return Bit switch
		{
			'R' => 0, 
			'G' => 0, 
			'B' => 0, 
			'C' => 0, 
			'r' => 1, 
			'g' => 2, 
			'b' => 3, 
			'c' => 4, 
			'K' => 5, 
			'W' => 6, 
			'Y' => 7, 
			'M' => 8, 
			_ => 0, 
		};
	}

	public static string ReverseTranslateBit(char Bit)
	{
		return Bit switch
		{
			'A' => "R", 
			'B' => "G", 
			'C' => "B", 
			'D' => "C", 
			'1' => "r", 
			'2' => "g", 
			'3' => "b", 
			'4' => "c", 
			'5' => "K", 
			'6' => "W", 
			'7' => "Y", 
			'8' => "M", 
			_ => "?", 
		};
	}

	public static char ReverseCharTranslateBit(char Bit)
	{
		return Bit switch
		{
			'A' => 'R', 
			'B' => 'G', 
			'C' => 'B', 
			'D' => 'C', 
			'1' => 'r', 
			'2' => 'g', 
			'3' => 'b', 
			'4' => 'c', 
			'5' => 'K', 
			'6' => 'W', 
			'7' => 'Y', 
			'8' => 'M', 
			_ => '?', 
		};
	}

	public static void SortBits(char[] Bits)
	{
		Array.Sort(Bits, (char a, char b) => GetBitSortOrder(a).CompareTo(GetBitSortOrder(b)));
	}

	public static void SortBits(List<char> Bits)
	{
		Bits.Sort((char a, char b) => GetBitSortOrder(a).CompareTo(GetBitSortOrder(b)));
	}

	public static void SortBits(List<string> Bits)
	{
		Bits.Sort((string a, string b) => GetBitSortOrder(a[0]).CompareTo(GetBitSortOrder(b[0])));
	}

	public static string GetString(string Bits)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (Options.AlphanumericBits)
		{
			for (int i = 0; i < Bits.Length; i++)
			{
				char c = CharTranslateBit(Bits[i]);
				stringBuilder.Append("{{").Append(ReverseTranslateBit(c)).Append("|")
					.Append(c)
					.Append("}}");
			}
		}
		else
		{
			for (int j = 0; j < Bits.Length; j++)
			{
				char bit = CharTranslateBit(Bits[j]);
				stringBuilder.Append("{{").Append(ReverseTranslateBit(bit)).Append("|")
					.Append('\a')
					.Append("}}");
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetString(char Bit)
	{
		return GetString(Bit.ToString());
	}

	public static string GetDisplayString(string Bits)
	{
		if (Bits.Length > 20)
		{
			char[] array = new char[Bits.Length];
			int i = 0;
			for (int length = Bits.Length; i < length; i++)
			{
				array[i] = Bits[i];
			}
			SortBits(array);
			bool alphanumericBits = Options.AlphanumericBits;
			StringBuilder stringBuilder = Event.NewStringBuilder();
			for (int j = 0; j < array.Length; j++)
			{
				char c = array[j];
				int num = 1;
				while (j < array.Length - 1 && array[j + 1] == c)
				{
					j++;
					num++;
				}
				c = CharTranslateBit(c);
				if (num <= 4)
				{
					stringBuilder.Append("{{").Append(ReverseTranslateBit(c)).Append('|');
					for (int k = 0; k < num; k++)
					{
						stringBuilder.Append(alphanumericBits ? c : '\a');
					}
					stringBuilder.Append("}}");
				}
				else
				{
					stringBuilder.Append("{{").Append(ReverseTranslateBit(c)).Append('|')
						.Append(alphanumericBits ? c : '\a')
						.Append("}}(")
						.Append(num)
						.Append(')');
				}
			}
			return stringBuilder.ToString();
		}
		return GetString(Bits);
	}

	public static string ToRealBits(string BitTemplate, string Blueprint)
	{
		CheckInit();
		if (Blueprint == null)
		{
			Blueprint = "dummyvalue";
		}
		List<string> list = new List<string>();
		Random random = null;
		for (int i = 0; i < BitTemplate.Length; i++)
		{
			if (_BitMap.ContainsKey(BitTemplate[i]))
			{
				list.Add(BitTemplate[i].ToString());
				continue;
			}
			int num = Convert.ToInt32(BitTemplate[i].ToString());
			int num2 = 1;
			bool flag = false;
			if (random == null)
			{
				random = Stat.GetSeededRandomGenerator(Blueprint);
			}
			if (random.Next(0, 101) <= 0)
			{
				flag = true;
			}
			while ((num > 0) & flag)
			{
				num--;
				num2++;
				if (random.Next(0, 101) <= 90)
				{
					flag = false;
				}
			}
			for (int j = 0; j < num2; j++)
			{
				_ = LevelMap[num].Count;
				list.Add(LevelMap[num].GetRandomElement().Color.ToString());
			}
		}
		SortBits(list);
		StringBuilder stringBuilder = new StringBuilder();
		for (int k = 0; k < list.Count; k++)
		{
			stringBuilder.Append(list[k]);
		}
		return stringBuilder.ToString();
	}

	public static string GetRandomBits(int Num, int UpToLevel = 8)
	{
		if (Num < 1 || UpToLevel < 0)
		{
			return null;
		}
		List<char> list = new List<char>(BitTypes.Count);
		foreach (BitType bitType in BitTypes)
		{
			if (bitType.Level <= UpToLevel)
			{
				list.Add(bitType.Color);
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		char[] array = new char[Num];
		for (int i = 0; i < Num; i++)
		{
			array[i] = list.GetRandomElement();
		}
		SortBits(array);
		return new string(array);
	}

	private static void AddType(BitType NewType, bool TierPriority = false, bool visible = true)
	{
		_BitSortOrder.Add(NewType.Color, _BitTypes.Count);
		if (visible)
		{
			_BitTypes.Add(NewType);
		}
		if (visible)
		{
			_BitOrder.Add(NewType.Color);
		}
		_BitMap.Add(NewType.Color, NewType);
		if (_LevelMap.TryGetValue(NewType.Level, out var value))
		{
			value.Add(NewType);
		}
		else
		{
			_LevelMap.Add(NewType.Level, new List<BitType> { NewType });
		}
		if (TierPriority || !_TierBits.ContainsKey(NewType.Level))
		{
			_TierBits[NewType.Level] = NewType.Color;
		}
	}

	public static void Init()
	{
		_BitTypes = new List<BitType>(12);
		_LevelMap = new Dictionary<int, List<BitType>>(12);
		_BitMap = new Dictionary<char, BitType>(12);
		_BitOrder = new List<char>(12);
		_BitSortOrder = new Dictionary<char, int>(12);
		_TierBits = new Dictionary<int, char>(12);
		AddType(new BitType(0, 'R', "{{R|scrap power systems}}"));
		AddType(new BitType(0, 'G', "{{G|scrap crystal}}"));
		AddType(new BitType(0, 'B', "{{B|scrap metal}}"), TierPriority: true);
		AddType(new BitType(0, 'C', "{{C|scrap electronics}}"));
		AddType(new BitType(1, 'r', "{{r|phasic power systems}}"));
		AddType(new BitType(2, 'g', "{{g|flawless crystal}}"));
		AddType(new BitType(3, 'b', "{{b|pure alloy}}"));
		AddType(new BitType(4, 'c', "{{c|pristine electronics}}"));
		AddType(new BitType(5, 'K', "{{K|nanomaterials}}"));
		AddType(new BitType(6, 'W', "{{W|photonics}}"));
		AddType(new BitType(7, 'Y', "{{Y|AI microcontrollers}}"));
		AddType(new BitType(8, 'M', "{{M|metacrystal}}"));
	}
}
