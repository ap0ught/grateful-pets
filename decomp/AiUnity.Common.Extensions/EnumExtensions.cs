using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using AiUnity.Common.Attributes;

namespace AiUnity.Common.Extensions;

public static class EnumExtensions
{
	public static T Add<T>(this Enum type, T value)
	{
		try
		{
			return (T)(object)((int)(object)type | (int)(object)value);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"Could not append value from enumerated type '{typeof(T).Name}'.", innerException);
		}
	}

	public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum input, bool checkZero = false, bool checkCombinators = false) where TEnum : struct, IComparable, IFormattable
	{
		if (!typeof(TEnum).IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		long setBits = Convert.ToInt32(input);
		if (!checkZero && setBits == 0L)
		{
			yield break;
		}
		foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
		{
			long num = Convert.ToInt32(value);
			if (num != 0L && (setBits & num) == num && (checkCombinators || (num & (num - 1)) == 0L))
			{
				yield return value;
			}
		}
	}

	public static bool Has<T>(this Enum type, T value)
	{
		try
		{
			return ((int)(object)type & (int)(object)value) == (int)(object)value;
		}
		catch
		{
			return false;
		}
	}

	public static bool Is<T>(this Enum type, T value)
	{
		try
		{
			return (int)(object)type == (int)(object)value;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsEnum<T>(this string s)
	{
		return s.IsEnum(typeof(T));
	}

	public static bool IsEnum(this string s, Type type)
	{
		bool flag = Enum.IsDefined(type, s);
		if (!flag && !string.IsNullOrEmpty(s) && type.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			string[] names = Enum.GetNames(type);
			flag = s.Replace(" ", string.Empty).Split(',').All((string e) => names.Any((string n) => n.Equals(e)));
		}
		return flag;
	}

	public static T Remove<T>(this Enum type, T value)
	{
		try
		{
			return (T)(object)((int)(object)type & ~(int)(object)value);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"Could not remove value from enumerated type '{typeof(T).Name}'.", innerException);
		}
	}

	public static T ToEnum<T>(this int i) where T : struct, IComparable, IFormattable, IConvertible
	{
		return (T)(object)i;
	}

	public static T ToEnum<T>(this string s, T defaultValue = default(T)) where T : struct, IComparable, IFormattable, IConvertible
	{
		try
		{
			if (s.IsEnum<T>())
			{
				return (T)Enum.Parse(typeof(T), s);
			}
		}
		catch
		{
		}
		return defaultValue;
	}

	public static Enum ToEnum(this string s, Type type)
	{
		try
		{
			if (int.TryParse(s, out var result))
			{
				return result.ToEnum(type);
			}
			if (s.IsEnum(type))
			{
				return Enum.Parse(type, s) as Enum;
			}
		}
		catch
		{
			return Activator.CreateInstance(type) as Enum;
		}
		return Activator.CreateInstance(type) as Enum;
	}

	public static Enum ToEnum(this int i, Type type)
	{
		return Convert.ChangeType(i, type) as Enum;
	}

	public static T? ToEnumSafe<T>(this string s) where T : struct
	{
		if (!s.IsEnum<T>())
		{
			return null;
		}
		return (T?)Enum.Parse(typeof(T), s);
	}

	private static bool IsFlagDefined(Enum e)
	{
		decimal result;
		return !decimal.TryParse(e.ToString(), out result);
	}

	public static string GetDescription(this Enum value)
	{
		Type type = value.GetType();
		string name = Enum.GetName(type, value);
		if (name != null)
		{
			FieldInfo field = type.GetField(name);
			if (field != null && Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
			{
				return descriptionAttribute.Description;
			}
		}
		return string.Empty;
	}

	public static string GetSymbol(this Enum value)
	{
		Type type = value.GetType();
		string name = Enum.GetName(type, value);
		if (name != null)
		{
			FieldInfo field = type.GetField(name);
			if (field != null && Attribute.GetCustomAttribute(field, typeof(EnumSymbolAttribute)) is EnumSymbolAttribute enumSymbolAttribute)
			{
				return enumSymbolAttribute.EnumSymbol;
			}
		}
		return value.ToString();
	}

	public static string GetAttributeStringOfType<T>(this Enum enumVal) where T : Attribute
	{
		object[] customAttributes = enumVal.GetType().GetMember(enumVal.ToString())[0].GetCustomAttributes(typeof(T), inherit: false);
		if (customAttributes.Length == 0)
		{
			return enumVal.ToString();
		}
		return ((T)customAttributes[0]).ToString();
	}
}
