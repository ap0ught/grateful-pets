using System;
using System.Collections.Generic;
using System.Linq;
using AiUnity.Common.Extensions;
using AiUnity.Common.Utilities;

namespace AiUnity.Common.Types;

public class PlatformEnumFlagWrapper<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
{
	public Dictionary<TEnum, int> EnumValueToFlag = new Dictionary<TEnum, int> { 
	{
		default,
		0
	} };

	private IEnumerable<string> legacyPlatforms = new List<string> { "PS3", "XBOX360", "WP8", "BB10Player", "BlackBerry", "NaCl", "FlashPlayer" };

	public int EnumFlags { get; set; }

	public IEnumerable<string> EnumNames => EnumValues.Select((TEnum b) => b.ToString());

	public IEnumerable<TEnum> EnumValues { get; set; }

	public PlatformEnumFlagWrapper(TEnum tEnum = default(TEnum))
	{
		EnumValues = (from e in EnumUtility.GetValues<TEnum>()
			where !legacyPlatforms.Any((string p) => e.ToString().Contains(p))
			select e).Distinct().ToList();
		foreach (var item in EnumValues.Select((TEnum e, int i) => new { e, i }))
		{
			EnumValueToFlag[item.e] = CollectionExtensions.GetValueOrDefault(EnumValueToFlag, item.e) | (1 << item.i);
		}
		EnumFlags = EnumValueToFlag[tEnum];
	}

	public PlatformEnumFlagWrapper(string names)
		: this(default(TEnum))
	{
		if (names == "Everything")
		{
			EnumFlags = -1;
		}
		else
		{
			if (string.IsNullOrEmpty(names))
			{
				return;
			}
			foreach (string item in from n in names.Split(',')
				select n.Trim())
			{
				Add(item.ToEnum<TEnum>());
			}
		}
	}

	public override string ToString()
	{
		if (EnumFlags == -1)
		{
			return "Everything";
		}
		return string.Join(", ", (from e in GetFlags()
			select e.ToString()).ToArray());
	}

	public bool Has(TEnum e)
	{
		return (EnumFlags & EnumValueToFlag[e]) != 0;
	}

	public void Add(TEnum e)
	{
		EnumFlags |= EnumValueToFlag[e];
	}

	public void Remove(TEnum e)
	{
		EnumFlags &= ~EnumValueToFlag[e];
	}

	public IEnumerable<TEnum> GetFlags()
	{
		int currentPow;
		for (currentPow = 1; currentPow != 0; currentPow <<= 1)
		{
			if ((currentPow & EnumFlags) != 0)
			{
				yield return EnumValueToFlag.FirstOrDefault((KeyValuePair<TEnum, int> x) => x.Value == currentPow).Key;
			}
		}
	}

	public static implicit operator PlatformEnumFlagWrapper<TEnum>(TEnum tEnum)
	{
		return new PlatformEnumFlagWrapper<TEnum>(tEnum);
	}

	public static implicit operator PlatformEnumFlagWrapper<TEnum>(string names)
	{
		return new PlatformEnumFlagWrapper<TEnum>(names);
	}
}
