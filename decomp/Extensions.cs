using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml;
using ConsoleLib.Console;
using Cysharp.Text;
using Genkit;
using Newtonsoft.Json;
using Platform.IO;
using Qud.UI;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL;
using XRL.Collections;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Text;
using XRL.World.Text.Delegates;

public static class Extensions
{
	private static class RemoveKeyCache<T>
	{
		public static List<T> Keys = new List<T>(16);
	}

	private class RTFCacheGenerator : LRUCache<(string s, int blockWrap, bool stripFormatting), string>.IGenerator
	{
		public string Generate((string s, int blockWrap, bool stripFormatting) Key)
		{
			return RTF.FormatToRTF(Key.s, "FF", Key.blockWrap, Key.stripFormatting);
		}
	}

	private class IntStringGenerator : LRUCache<int, string>.IGenerator
	{
		public string Generate(int Key)
		{
			return Key.ToString();
		}
	}

	private class CommaExpressionGenerator : LRUCache<string, List<string>>.IGenerator
	{
		public List<string> Generate(string Key)
		{
			return Key.Split(',').ToList();
		}
	}

	private static LRUCache<(string s, int blockWrap, bool stripFormatting), string> CachedRTFExpansions = new LRUCache<(string, int, bool), string>(256, new RTFCacheGenerator());

	private static LRUCache<int, string> CachedIntStringifications = new LRUCache<int, string>(256, new IntStringGenerator());

	private static LRUCache<string, List<string>> CachedCommaExpansions = new LRUCache<string, List<string>>(128, new CommaExpressionGenerator());

	private static Dictionary<string, List<string>> CachedDoubleSemicolonExpansions = new Dictionary<string, List<string>>(16);

	private static string LastDoubleSemicolonExpansionRequest;

	private static List<string> LastDoubleSemicolonExpansionResult;

	private static Dictionary<string, Dictionary<string, string>> CachedDictionaryExpansions = new Dictionary<string, Dictionary<string, string>>(32);

	private static string LastDictionaryExpansionRequest;

	private static Dictionary<string, string> LastDictionaryExpansionResult;

	private static Dictionary<string, Dictionary<string, int>> CachedNumericDictionaryExpansions = new Dictionary<string, Dictionary<string, int>>(8);

	private static string LastNumericDictionaryExpansionRequest;

	private static Dictionary<string, int> LastNumericDictionaryExpansionResult;

	private static Dictionary<Type, FieldInfo[]> TypeFields = new Dictionary<Type, FieldInfo[]>(2560);

	public static bool CompareTo(this StringBuilder s1, string s2)
	{
		if (s1 == null && s2 == null)
		{
			return true;
		}
		if (s1 == null || s2 == null)
		{
			return false;
		}
		if (s1.Length != s2.Length)
		{
			return false;
		}
		for (int i = 0; i < s1.Length; i++)
		{
			if (s1[i] != s2[i])
			{
				return false;
			}
		}
		return true;
	}

	public static string Strip(this string s)
	{
		return ConsoleLib.Console.ColorUtility.StripFormatting(s);
	}

	public static string Color(this string String, string Color)
	{
		return ConsoleLib.Console.ColorUtility.ApplyColor(String, Color);
	}

	public static string Color(this string String, char Color)
	{
		return ConsoleLib.Console.ColorUtility.ApplyColor(String, Color);
	}

	public static string Capitalize(this string s)
	{
		return ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(s);
	}

	public static string Pluralize(this string s)
	{
		return Grammar.Pluralize(s);
	}

	public static int StrippedLength(this string s)
	{
		return ConsoleLib.Console.ColorUtility.LengthExceptFormatting(s);
	}

	public static int CompareExceptFormatting(this string Text, string Vs)
	{
		return ConsoleLib.Console.ColorUtility.CompareExceptFormatting(Text, Vs);
	}

	public static int CompareExceptFormattingAndCase(this string Text, string Vs)
	{
		return ConsoleLib.Console.ColorUtility.CompareExceptFormattingAndCase(Text, Vs);
	}

	public static bool EqualsExceptFormatting(this string Text, string Vs)
	{
		return ConsoleLib.Console.ColorUtility.EqualsExceptFormatting(Text, Vs);
	}

	public static bool EqualsExceptFormattingAndCase(this string Text, string Vs)
	{
		return ConsoleLib.Console.ColorUtility.EqualsExceptFormattingAndCase(Text, Vs);
	}

	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this ICollection<T> list)
	{
		if (list != null)
		{
			return list.Count < 1;
		}
		return true;
	}

	public static bool IsReadOnlyNullOrEmpty<T>([NotNullWhen(false)] this IReadOnlyCollection<T> list)
	{
		if (list != null)
		{
			return list.Count < 1;
		}
		return true;
	}

	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T> list)
	{
		if (list != null)
		{
			return !list.Any();
		}
		return true;
	}

	public static bool IsNullOrEmpty([NotNullWhen(false)] this string value)
	{
		if (value != null)
		{
			return value.Length == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty([NotNullWhen(false)] this StringBuilder value)
	{
		if (value != null)
		{
			return value.Length == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty([NotNullWhen(false)] this GlobalLocation value)
	{
		return value?.World == null;
	}

	public static T ElementAtOrDefault<T>(this IReadOnlyList<T> List, int Index, T Default = default(T))
	{
		if (List == null || Index >= List.Count)
		{
			return Default;
		}
		return List[Index];
	}

	public static string Coalesce(this string First, string Second)
	{
		if (!First.IsNullOrEmpty())
		{
			return First;
		}
		if (!Second.IsNullOrEmpty())
		{
			return Second;
		}
		return "";
	}

	public static string Coalesce(this string First, string Second, string Third)
	{
		if (!First.IsNullOrEmpty())
		{
			return First;
		}
		if (!Second.IsNullOrEmpty())
		{
			return Second;
		}
		if (!Third.IsNullOrEmpty())
		{
			return Third;
		}
		return "";
	}

	public static string Coalesce(this string First, string Second, string Third, string Fourth)
	{
		if (!First.IsNullOrEmpty())
		{
			return First;
		}
		if (!Second.IsNullOrEmpty())
		{
			return Second;
		}
		if (!Third.IsNullOrEmpty())
		{
			return Third;
		}
		if (!Fourth.IsNullOrEmpty())
		{
			return Fourth;
		}
		return "";
	}

	public static string Coalesce(this string First, string Second, string Third, string Fourth, string Fifth)
	{
		if (!First.IsNullOrEmpty())
		{
			return First;
		}
		if (!Second.IsNullOrEmpty())
		{
			return Second;
		}
		if (!Third.IsNullOrEmpty())
		{
			return Third;
		}
		if (!Fourth.IsNullOrEmpty())
		{
			return Fourth;
		}
		if (!Fifth.IsNullOrEmpty())
		{
			return Fifth;
		}
		return "";
	}

	public static string Coalesce(this string First, params string[] Rest)
	{
		if (!First.IsNullOrEmpty())
		{
			return First;
		}
		for (int i = 0; i < Rest.Length; i++)
		{
			if (!Rest[i].IsNullOrEmpty())
			{
				return Rest[i];
			}
		}
		return "";
	}

	public static string Replace(this string Text, string Old, string New, StringComparison Comparison, bool RespectTitleCase)
	{
		int num = Text.IndexOf(Old, Comparison);
		int num2 = 0;
		int num3 = 0;
		if (num == -1)
		{
			return Text;
		}
		StringBuilder stringBuilder = The.StringBuilder;
		while (num != -1)
		{
			stringBuilder.Append(Text, num3, num - num3);
			num2 = stringBuilder.Length;
			stringBuilder.Append(New);
			if (RespectTitleCase)
			{
				char Character = stringBuilder[num2];
				Character.ToCaseOf(Text[num]);
				stringBuilder[num2] = Character;
			}
			num += Old.Length;
			num3 = num;
			num = Text.IndexOf(Old, num, Comparison);
		}
		stringBuilder.Append(Text, num3, Text.Length - num3);
		return stringBuilder.ToString();
	}

	public static void ToCaseOf(this ref char Character, char Of)
	{
		if (char.IsUpper(Character))
		{
			if (char.IsUpper(Of))
			{
				return;
			}
			Character = char.ToLowerInvariant(Character);
		}
		if (char.IsUpper(Of))
		{
			Character = char.ToUpperInvariant(Character);
		}
	}

	public static bool IsDirectorySeparator(this char Value)
	{
		if (Value != Platform.IO.Path.DirectorySeparatorChar)
		{
			return Value == Platform.IO.Path.AltDirectorySeparatorChar;
		}
		return true;
	}

	public static bool IsDirectorySeparatorInvariant(this char Value)
	{
		if (Value != '/')
		{
			return Value == '\\';
		}
		return true;
	}

	public static string WithTrailingDirectorySeparator(this string Value)
	{
		if (Value.IsNullOrEmpty())
		{
			return Platform.IO.Path.DirectorySeparatorChar.GetString();
		}
		if (!Value[Value.Length - 1].IsDirectorySeparator())
		{
			return Value + Platform.IO.Path.DirectorySeparatorChar;
		}
		return Value;
	}

	public static string WithLeadingDirectorySeparator(this string Value)
	{
		if (Value.IsNullOrEmpty())
		{
			return Platform.IO.Path.DirectorySeparatorChar.GetString();
		}
		if (!Value[0].IsDirectorySeparator())
		{
			return Platform.IO.Path.DirectorySeparatorChar + Value;
		}
		return Value;
	}

	public static string GetString(this char Value)
	{
		if (Value < CharCache.Length)
		{
			return CharCache.Values[(uint)Value];
		}
		return Value.ToString();
	}

	public static bool Contains(this string text, string find, CompareOptions comp)
	{
		return CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, find, comp) >= 0;
	}

	public static bool TryParseRange(this string Range, out int Low, out int High)
	{
		High = 0;
		int num = Range.IndexOf('-');
		if (num < 0)
		{
			bool result = int.TryParse(Range, out Low);
			High = Low;
			return result;
		}
		if (int.TryParse(Range.Substring(0, num), out Low))
		{
			return int.TryParse(Range.Substring(num + 1), out High);
		}
		return false;
	}

	public static void SetBit(this ref int field, int mask, bool value)
	{
		if (value)
		{
			field |= mask;
		}
		else
		{
			field &= ~mask;
		}
	}

	public static void SetBit(this ref uint field, uint mask, bool value)
	{
		if (value)
		{
			field |= mask;
		}
		else
		{
			field &= ~mask;
		}
	}

	public static bool HasBit(this int field, int mask)
	{
		return (field & mask) != 0;
	}

	public static bool HasBit(this uint field, uint mask)
	{
		return (field & mask) != 0;
	}

	public static bool HasAllBits(this int field, int mask)
	{
		return (field & mask) == mask;
	}

	public static bool HasAllBits(this uint field, uint mask)
	{
		return (field & mask) == mask;
	}

	public static int CountBits(this int field)
	{
		field -= (field >> 1) & 0x55555555;
		field = (field & 0x33333333) + ((field >> 2) & 0x33333333);
		field = (field + (field >> 4)) & 0xF0F0F0F;
		return field * 16843009 >> 24;
	}

	public static int CountDigits(this int Number)
	{
		if (Number == 0)
		{
			return 1;
		}
		return (((long)Number > 0L) ? 1 : 2) + (int)Math.Log10(Math.Abs(Number));
	}

	public static void Insert(this ref Span<char> Text, int Index, int Value)
	{
		Text.Insert(ref Index, Value);
	}

	public static void Insert(this ref Span<char> Text, ref int Index, int Value)
	{
		if (Value < 0)
		{
			Text[Index++] = '-';
			Value *= -1;
		}
		int num = Index;
		do
		{
			int num2 = Value % 10;
			Value /= 10;
			Text[Index++] = (char)(48 + num2);
		}
		while (Value > 0);
		int num3 = Index - 1;
		while (num < num3)
		{
			char c = Text[num];
			Text[num++] = Text[num3];
			Text[num3--] = c;
		}
	}

	public static Dictionary<T, int> Increment<T>(this Dictionary<T, int> dict, T key)
	{
		dict.TryGetValue(key, out var value);
		dict[key] = value + 1;
		return dict;
	}

	public static V GetValue<K, V>(this Dictionary<K, V> Self, K Key, V Default = default(V))
	{
		if (Key == null || !Self.TryGetValue(Key, out var value))
		{
			return Default;
		}
		return value;
	}

	public static V PullValue<K, V>(this Dictionary<K, V> Self, K Key, V Default = default(V))
	{
		if (Key != null && Self.TryGetValue(Key, out var value))
		{
			Self.Remove(Key);
			return value;
		}
		return Default;
	}

	public static int RemoveAll<K, V>(this Dictionary<K, V> Self, Predicate<KeyValuePair<K, V>> Predicate)
	{
		List<K> keys = RemoveKeyCache<K>.Keys;
		int num = 0;
		try
		{
			foreach (KeyValuePair<K, V> item in Self)
			{
				if (Predicate(item))
				{
					keys.Add(item.Key);
				}
			}
			foreach (K item2 in keys)
			{
				if (Self.Remove(item2))
				{
					num++;
				}
			}
			return num;
		}
		finally
		{
			keys.Clear();
		}
	}

	public static int RemoveAll<K, V>(this Dictionary<K, V> Self, IList<K> Keys)
	{
		int num = 0;
		int i = 0;
		for (int count = Keys.Count; i < count; i++)
		{
			if (Self.Remove(Keys[i]))
			{
				num++;
			}
		}
		return num;
	}

	public static string ToRTFCached(this string s, int blockWrap = -1, bool stripFormatting = false)
	{
		if (s == null)
		{
			return RTF.FormatToRTF(s, "FF", blockWrap, stripFormatting);
		}
		return CachedRTFExpansions.Get((s, blockWrap, stripFormatting));
	}

	public static string ToStringCached(this int i)
	{
		return CachedIntStringifications.Get(i);
	}

	public static List<string> CachedCommaExpansion(this string text)
	{
		if (text == null)
		{
			return null;
		}
		return CachedCommaExpansions.Get(text);
	}

	public static List<string> CachedDoubleSemicolonExpansion(this string Text)
	{
		if (Text == null)
		{
			return null;
		}
		if (Text == LastDoubleSemicolonExpansionRequest)
		{
			return LastDoubleSemicolonExpansionResult;
		}
		if (!CachedDoubleSemicolonExpansions.TryGetValue(Text, out var value))
		{
			value = new List<string>(Text.Split(";;"));
			CachedDoubleSemicolonExpansions.Add(Text, value);
		}
		LastDoubleSemicolonExpansionRequest = Text;
		LastDoubleSemicolonExpansionResult = value;
		return value;
	}

	public static Dictionary<string, string> CachedDictionaryExpansion(this string text)
	{
		if (text == LastDictionaryExpansionRequest && text != null)
		{
			return LastDictionaryExpansionResult;
		}
		if (!CachedDictionaryExpansions.TryGetValue(text, out var value))
		{
			string[] array = text.Split(";;", StringSplitOptions.RemoveEmptyEntries);
			value = new Dictionary<string, string>(array.Length);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string[] array3 = text2.Split("::", 2);
				if (array3.Length < 2)
				{
					Debug.LogError("Bad dictionary expansion part '" + text2 + "'");
				}
				else if (value.ContainsKey(array3[0]))
				{
					Debug.LogError("Duplicate dictionary expansion entry '" + array3[0] + "'");
				}
				else
				{
					value.Add(array3[0], array3[1]);
				}
			}
			CachedDictionaryExpansions.Add(text, value);
		}
		LastDictionaryExpansionRequest = text;
		LastDictionaryExpansionResult = value;
		return value;
	}

	public static string ToStringForCachedDictionaryExpansion(this Dictionary<string, string> Dict)
	{
		StringBuilder stringBuilder = The.StringBuilder;
		foreach (KeyValuePair<string, string> item in Dict)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(";;");
			}
			stringBuilder.Append(item.Key).Append("::").Append(item.Value);
		}
		return stringBuilder.ToString();
	}

	public static string ToStringForCachedNumericDictionaryExpansion(this Dictionary<string, int> Dict)
	{
		StringBuilder stringBuilder = The.StringBuilder;
		foreach (KeyValuePair<string, int> item in Dict)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(";;");
			}
			stringBuilder.Append(item.Key).Append("::").Append(item.Value);
		}
		return stringBuilder.ToString();
	}

	public static Dictionary<string, int> CachedNumericDictionaryExpansion(this string text)
	{
		if (text == LastNumericDictionaryExpansionRequest && text != null)
		{
			return LastNumericDictionaryExpansionResult;
		}
		if (!CachedNumericDictionaryExpansions.TryGetValue(text, out var value))
		{
			string[] array = text.Split(";;", StringSplitOptions.RemoveEmptyEntries);
			value = new Dictionary<string, int>(array.Length);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string[] array3 = text2.Split("::", 2);
				if (array3.Length < 2)
				{
					Debug.LogError("Bad dictionary expansion part '" + text2 + "'");
				}
				else if (value.ContainsKey(array3[0]))
				{
					Debug.LogError("Duplicate dictionary expansion entry '" + array3[0] + "'");
				}
				else
				{
					value.Add(array3[0], Convert.ToInt32(array3[1]));
				}
			}
			CachedNumericDictionaryExpansions.Add(text, value);
		}
		LastNumericDictionaryExpansionRequest = text;
		LastNumericDictionaryExpansionResult = value;
		return value;
	}

	public static int ReverseIndexOf<T>(this List<T> list, T item)
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].Equals(item))
			{
				return num;
			}
		}
		return -1;
	}

	public static string Snippet(this string text, int howManySpaces = 4, int ifNoSpaces = 10)
	{
		int num = text.UpToNthIndex(' ', howManySpaces);
		if (num <= 0)
		{
			return text.Substring(0, ifNoSpaces);
		}
		return text.Substring(0, num);
	}

	public static int UpToNthIndex(this string Text, char Char, int Number, int Default = -1)
	{
		int i = 0;
		int num = 0;
		for (int length = Text.Length; i < length; i++)
		{
			if (Text[i] == Char)
			{
				Default = i;
				if (++num == Number)
				{
					break;
				}
			}
		}
		return Default;
	}

	public static int UpToNthIndex(this string Text, Func<char, bool> Predicate, int Number, int Default = -1)
	{
		int i = 0;
		int num = 0;
		for (int length = Text.Length; i < length; i++)
		{
			if (Predicate(Text[i]))
			{
				Default = i;
				if (++num == Number)
				{
					break;
				}
			}
		}
		return Default;
	}

	public static int UpToNthIndex<T>(this List<T> List, T Value, int Number, int Default = -1)
	{
		int i = 0;
		int num = 0;
		for (int count = List.Count; i < count; i++)
		{
			if (List[i].Equals(Value))
			{
				Default = i;
				if (++num == Number)
				{
					break;
				}
			}
		}
		return Default;
	}

	public static int FindCount<T>(this List<T> list, Predicate<T> filter)
	{
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && filter(list[i]))
			{
				num++;
			}
		}
		return num;
	}

	public static void EnsureCapacity<T>(this List<T> List, int Capacity)
	{
		if (List.Capacity < Capacity)
		{
			List.Capacity = Capacity;
		}
	}

	public static void DisposeElements<T>(this IList<T> List, int Capacity) where T : IDisposable
	{
		for (int num = List.Count - 1; num >= 0; num--)
		{
			List[num].Dispose();
		}
		List.Clear();
	}

	public static T RemoveRandomElement<T>(this List<T> list, System.Random R = null)
	{
		switch (list.Count)
		{
		case 0:
			return default;
		case 1:
			return list[0];
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			int index = R.Next(0, list.Count);
			T result = list[index];
			list.RemoveAt(index);
			return result;
		}
		}
	}

	public static T GetRandomElement<T>(this List<T> list, System.Random R = null)
	{
		switch (list.Count)
		{
		case 0:
			return default;
		case 1:
			return list[0];
		default:
			if (R == null)
			{
				R = Stat.Rand;
			}
			return list[R.Next(0, list.Count)];
		}
	}

	public static T GetRandomElement<T>(this Dictionary<T, int> list, System.Random R = null)
	{
		switch (list.Count)
		{
		case 0:
			return default;
		case 1:
			foreach (T key in list.Keys)
			{
				if (list[key] > 0)
				{
					return key;
				}
			}
			return default;
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			int num = 0;
			foreach (int value in list.Values)
			{
				num += value;
			}
			int num2 = R.Next(0, num);
			int num3 = 0;
			foreach (KeyValuePair<T, int> item in list)
			{
				num3 += item.Value;
				if (num3 >= num2)
				{
					return item.Key;
				}
			}
			throw new Exception("should be unreachable");
		}
		}
	}

	public static T GetRandomElement<T>(this Dictionary<T, int> list, ref int total, System.Random R = null)
	{
		switch (list.Count)
		{
		case 0:
			return default;
		case 1:
			foreach (T key in list.Keys)
			{
				if (list[key] > 0)
				{
					return key;
				}
			}
			return default;
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			if (total == 0)
			{
				foreach (int value in list.Values)
				{
					total += value;
				}
			}
			int num = R.Next(0, total);
			int num2 = 0;
			foreach (KeyValuePair<T, int> item in list)
			{
				num2 += item.Value;
				if (num2 >= num)
				{
					return item.Key;
				}
			}
			throw new Exception("should be unreachable");
		}
		}
	}

	public static T GetRandomElement<T>(this T[] list, System.Random R = null)
	{
		switch (list.Length)
		{
		case 0:
			return default;
		case 1:
			return list[0];
		default:
			if (R == null)
			{
				R = Stat.Rand;
			}
			return list[R.Next(0, list.Length)];
		}
	}

	public static T GetRandomElementCosmetic<T>(this T[] list)
	{
		return list.GetRandomElement(Stat.Rnd2);
	}

	public static T GetRandomElement<T>(this IEnumerable<T> enumerable, System.Random R = null)
	{
		int num = enumerable.Count();
		switch (num)
		{
		case 0:
			return default;
		case 1:
			return enumerable.ElementAt(0);
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			int index = R.Next(0, num);
			return enumerable.ElementAt(index);
		}
		}
	}

	public static T GetRandomElement<T>(this ICollection<T> collection, System.Random R = null)
	{
		int count = collection.Count;
		switch (count)
		{
		case 0:
			return default;
		case 1:
			return collection.ElementAt(0);
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			int index = R.Next(0, count);
			return collection.ElementAt(index);
		}
		}
	}

	public static T GetRandomElementCosmetic<T>(this IEnumerable<T> enumerable)
	{
		return enumerable.GetRandomElement(Stat.Rnd2);
	}

	public static char GetRandomElement(this string list, System.Random R = null)
	{
		switch (list.Length)
		{
		case 0:
			return '\0';
		case 1:
			return list[0];
		default:
			if (R == null)
			{
				R = Stat.Rand;
			}
			return list[R.Next(0, list.Length)];
		}
	}

	public static char GetRandomElementCosmetic(this string list)
	{
		return list.GetRandomElement(Stat.Rnd2);
	}

	public static T GetRandomElement<T>(this List<T> list, Predicate<T> filter, System.Random R = null)
	{
		if (list.Count == 0)
		{
			return default;
		}
		int num = 0;
		T result = default;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && filter(list[i]))
			{
				num++;
				result = list[i];
			}
		}
		switch (num)
		{
		case 0:
			return default;
		case 1:
			return result;
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			if (num == list.Count)
			{
				return list[R.Next(0, list.Count)];
			}
			int num2 = R.Next(0, num);
			if (num2 == num - 1)
			{
				return result;
			}
			int num3 = 0;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] != null && filter(list[j]) && num3++ == num2)
				{
					return list[j];
				}
			}
			return default;
		}
		}
	}

	public static T GetRandomElementCosmetic<T>(this List<T> list, Predicate<T> filter)
	{
		return list.GetRandomElement(filter, Stat.Rnd2);
	}

	public static T GetRandomElement<T>(this T[] list, Predicate<T> filter, System.Random R = null)
	{
		if (list.Length == 0)
		{
			return default;
		}
		int num = 0;
		T result = default;
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i] != null && filter(list[i]))
			{
				num++;
				result = list[i];
			}
		}
		switch (num)
		{
		case 0:
			return default;
		case 1:
			return result;
		default:
		{
			if (R == null)
			{
				R = Stat.Rand;
			}
			if (num == list.Length)
			{
				return list[R.Next(0, list.Length)];
			}
			int num2 = R.Next(0, num);
			if (num2 == num - 1)
			{
				return result;
			}
			int num3 = 0;
			for (int j = 0; j < list.Length; j++)
			{
				if (list[j] != null && filter(list[j]) && num3++ == num2)
				{
					return list[j];
				}
			}
			return default;
		}
		}
	}

	public static T GetRandomElementCosmetic<T>(this T[] list, Predicate<T> filter)
	{
		return list.GetRandomElement(filter, Stat.Rnd2);
	}

	public static string GetRandomSubstring(this string Text, char Separator, bool Trim = false, System.Random R = null)
	{
		if (Text.IsNullOrEmpty())
		{
			return "";
		}
		int i = 0;
		int num = 1;
		int num2;
		for (num2 = Text.Length - 1; i < num2; i++)
		{
			if (Text[i] == Separator)
			{
				num++;
			}
		}
		if (num == 1)
		{
			return Text;
		}
		int j = 0;
		int num3 = (R ?? Stat.Rand).Next(0, num);
		i = 0;
		num = 0;
		for (; i < num2; i++)
		{
			if (Text[i] == Separator)
			{
				if (num++ == num3)
				{
					i--;
					break;
				}
				j = i + 1;
			}
		}
		if (Trim)
		{
			for (; char.IsWhiteSpace(Text[j]); j++)
			{
			}
			while (char.IsWhiteSpace(Text[i]))
			{
				i--;
			}
		}
		return Text.Substring(j, i - j + 1);
	}

	public static string GetDelimitedSubstring(this string Text, char Separator, int Index, bool Trim = false)
	{
		if (Text.IsNullOrEmpty())
		{
			if (Index <= 0)
			{
				return Text;
			}
			return null;
		}
		int i = 0;
		int j = 0;
		int num = 0;
		for (int num2 = Text.Length - 1; j < num2; j++)
		{
			if (Text[j] == Separator)
			{
				if (num++ == Index)
				{
					j--;
					break;
				}
				i = j + 1;
			}
		}
		if (num == 0 && Index > 0)
		{
			return null;
		}
		if (Trim)
		{
			for (; char.IsWhiteSpace(Text[i]); i++)
			{
			}
			while (char.IsWhiteSpace(Text[j]))
			{
				j--;
			}
		}
		return Text.Substring(i, j - i + 1);
	}

	public static int FindDelimitedSubstring(this string Text, char Separator, string Substring, StringComparison Comparison = StringComparison.Ordinal)
	{
		if (Text == null || Substring == null)
		{
			return -1;
		}
		int num = Text.Length - 1;
		int num2 = -1;
		int num3 = -1;
		while (num2 < num)
		{
			num2 = Text.IndexOf(Substring, num2 + 1, Comparison);
			if (num2 == -1)
			{
				break;
			}
			num3++;
			if (num2 <= 0 || Text[num2 - 1] == Separator)
			{
				int num4 = num2 + Substring.Length;
				if (num4 - 1 >= num || Text[num4] == Separator)
				{
					return num3;
				}
			}
		}
		return -1;
	}

	public static bool HasDelimitedSubstring([NotNullWhen(true)] this string Text, char Separator, string Substring, StringComparison Comparison = StringComparison.Ordinal)
	{
		if (Text == null || Substring == null)
		{
			return false;
		}
		int num = Text.Length - 1;
		int num2 = -1;
		while (num2 < num)
		{
			num2 = Text.IndexOf(Substring, num2 + 1, Comparison);
			if (num2 == -1)
			{
				break;
			}
			if (num2 <= 0 || Text[num2 - 1] == Separator)
			{
				int num3 = num2 + Substring.Length;
				if (num3 - 1 >= num || Text[num3] == Separator)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasDelimitedSubstring([NotNullWhen(true)] this string Text, string Separator, string Substring, StringComparison Comparison = StringComparison.Ordinal)
	{
		if (Text == null || Substring == null || Separator.IsNullOrEmpty())
		{
			return false;
		}
		int num = Text.Length - 1;
		int num2 = -1;
		while (num2 < num)
		{
			num2 = Text.IndexOf(Substring, num2 + 1, Comparison);
			if (num2 == -1)
			{
				break;
			}
			if (num2 > 0)
			{
				if (num2 < Separator.Length)
				{
					continue;
				}
				bool flag = true;
				int i = 0;
				for (int length = Separator.Length; i < length; i++)
				{
					if (Text[num2 - Separator.Length + i] != Separator[i])
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			int num3 = num2 + Substring.Length;
			if (num3 - 1 < num)
			{
				if (num3 + Separator.Length >= num)
				{
					continue;
				}
				bool flag2 = true;
				int j = 0;
				for (int length2 = Separator.Length; j < length2; j++)
				{
					if (Text[num3 + j] != Separator[j])
					{
						flag2 = false;
						break;
					}
				}
				if (!flag2)
				{
					continue;
				}
			}
			return true;
		}
		return false;
	}

	public static string AddDelimitedSubstring(this string Text, char Separator, string Substring, StringComparison Comparison = StringComparison.Ordinal)
	{
		if (Text == null)
		{
			return Substring;
		}
		if (Text.HasDelimitedSubstring(Separator, Substring, Comparison))
		{
			return Text;
		}
		return Text + Separator + Substring;
	}

	public static string AddDelimitedSubstring(this string Text, string Separator, string Substring, StringComparison Comparison = StringComparison.Ordinal)
	{
		if (Text == null)
		{
			return Substring;
		}
		if (Text.HasDelimitedSubstring(Separator, Substring, Comparison))
		{
			return Text;
		}
		return Text + Separator + Substring;
	}

	public static IReadOnlyListEnumerator<T> Iterate<T>(this IReadOnlyList<T> List)
	{
		if (List == null)
		{
			return default;
		}
		return new IReadOnlyListEnumerator<T>(List, 0, List.Count);
	}

	public static IReadOnlyListEnumerator<T> Iterate<T>(this IReadOnlyList<T> List, int Start)
	{
		if (List == null)
		{
			return default;
		}
		return new IReadOnlyListEnumerator<T>(List, Start, List.Count - Start);
	}

	public static IReadOnlyListEnumerator<T> Iterate<T>(this IReadOnlyList<T> List, int Start, int Length)
	{
		if (List == null)
		{
			return default;
		}
		int count = List.Count;
		if (Start + Length > count)
		{
			Length = count - Start;
		}
		return new IReadOnlyListEnumerator<T>(List, Start, Length);
	}

	public static IReadOnlyListEnumerator<T> IterateFirst<T>(this IReadOnlyList<T> List, int Amount)
	{
		if (List == null)
		{
			return default;
		}
		int count = List.Count;
		if (Amount > count)
		{
			Amount = count;
		}
		return new IReadOnlyListEnumerator<T>(List, 0, Amount);
	}

	public static IReadOnlyListEnumerator<T> IterateLast<T>(this IReadOnlyList<T> List, int Amount)
	{
		if (List == null)
		{
			return default;
		}
		int count = List.Count;
		if (Amount > count)
		{
			Amount = count;
		}
		return new IReadOnlyListEnumerator<T>(List, count - Amount, Amount);
	}

	public static RingEnumerator<T> IterateRingAt<T>(this IReadOnlyList<T> List, int Start, bool IncludeSelf = false, bool Invert = false)
	{
		if (List == null)
		{
			return default;
		}
		return new RingEnumerator<T>(List, Start, IncludeSelf, Invert);
	}

	public static RingEnumerator<T> IterateRing<T>(this IReadOnlyList<T> List, T Start, bool IncludeSelf = false, bool Invert = false) where T : IEquatable<T>
	{
		if (List == null)
		{
			return default;
		}
		int i = 0;
		for (int count = List.Count; i < count; i++)
		{
			if (List[i].Equals(Start))
			{
				return new RingEnumerator<T>(List, i, IncludeSelf, Invert);
			}
		}
		return default;
	}

	public static RingSpanEnumerator<T> IterateRingAt<T>(this ReadOnlySpan<T> Span, int Start, bool IncludeSelf = false, bool Invert = false)
	{
		return new RingSpanEnumerator<T>(Span, Start, IncludeSelf, Invert);
	}

	public static RingSpanEnumerator<T> IterateRing<T>(this ReadOnlySpan<T> Span, T Start, bool IncludeSelf = false, bool Invert = false) where T : IEquatable<T>
	{
		int i = 0;
		for (int length = Span.Length; i < length; i++)
		{
			if (Span[i].Equals(Start))
			{
				return new RingSpanEnumerator<T>(Span, i, IncludeSelf, Invert);
			}
		}
		return default;
	}

	public static RingSpanEnumerator<T> IterateRingAt<T>(this Span<T> Span, int Start, bool IncludeSelf = false, bool Invert = false)
	{
		return ((ReadOnlySpan<T>)Span).IterateRingAt(Start, IncludeSelf, Invert);
	}

	public static RingSpanEnumerator<T> IterateRing<T>(this Span<T> Span, T Start, bool IncludeSelf = false, bool Invert = false) where T : IEquatable<T>
	{
		return ((ReadOnlySpan<T>)Span).IterateRing(Start, IncludeSelf, Invert);
	}

	public static RingSpanEnumerator<T> IterateRingAt<T>(this T[] Array, int Start, bool IncludeSelf = false, bool Invert = false)
	{
		return ((ReadOnlySpan<T>)Array).IterateRingAt(Start, IncludeSelf, Invert);
	}

	public static RingSpanEnumerator<T> IterateRing<T>(this T[] Array, T Start, bool IncludeSelf = false, bool Invert = false) where T : IEquatable<T>
	{
		return ((ReadOnlySpan<T>)Array).IterateRing(Start, IncludeSelf, Invert);
	}

	public static Int32Enumerator IterateInt32(this string Text)
	{
		return new Int32Enumerator(Text);
	}

	public static Int32Enumerator IterateInt32(this ReadOnlySpan<char> Text)
	{
		return new Int32Enumerator(Text);
	}

	public static void ExtractInt32(this ReadOnlySpan<char> Text, out int? First)
	{
		Int32Enumerator int32Enumerator = Text.IterateInt32();
		First = (int32Enumerator.MoveNext() ? new int?(int32Enumerator.Current) : ((int?)null));
	}

	public static void ExtractInt32(this ReadOnlySpan<char> Text, out int? First, out int? Second)
	{
		First = null;
		Second = null;
		Int32Enumerator int32Enumerator = Text.IterateInt32();
		if (int32Enumerator.MoveNext())
		{
			First = int32Enumerator.Current;
			if (int32Enumerator.MoveNext())
			{
				Second = int32Enumerator.Current;
			}
		}
	}

	public static void ExtractInt32(this ReadOnlySpan<char> Text, out int? First, out int? Second, out int? Third)
	{
		First = null;
		Second = null;
		Third = null;
		Int32Enumerator int32Enumerator = Text.IterateInt32();
		if (!int32Enumerator.MoveNext())
		{
			return;
		}
		First = int32Enumerator.Current;
		if (int32Enumerator.MoveNext())
		{
			Second = int32Enumerator.Current;
			if (int32Enumerator.MoveNext())
			{
				Third = int32Enumerator.Current;
			}
		}
	}

	public static void ExtractInt32(this ReadOnlySpan<char> Text, out int? First, out int? Second, out int? Third, out int? Fourth)
	{
		First = null;
		Second = null;
		Third = null;
		Fourth = null;
		Int32Enumerator int32Enumerator = Text.IterateInt32();
		if (!int32Enumerator.MoveNext())
		{
			return;
		}
		First = int32Enumerator.Current;
		if (!int32Enumerator.MoveNext())
		{
			return;
		}
		Second = int32Enumerator.Current;
		if (int32Enumerator.MoveNext())
		{
			Third = int32Enumerator.Current;
			if (int32Enumerator.MoveNext())
			{
				Fourth = int32Enumerator.Current;
			}
		}
	}

	public static void ExtractInt32(this string Text, out int? First)
	{
		((ReadOnlySpan<char>)Text).ExtractInt32(out First);
	}

	public static void ExtractInt32(this string Text, out int? First, out int? Second)
	{
		((ReadOnlySpan<char>)Text).ExtractInt32(out First, out Second);
	}

	public static void ExtractInt32(this string Text, out int? First, out int? Second, out int? Third)
	{
		((ReadOnlySpan<char>)Text).ExtractInt32(out First, out Second, out Third);
	}

	public static void ExtractInt32(this string Text, out int? First, out int? Second, out int? Third, out int? Fourth)
	{
		((ReadOnlySpan<char>)Text).ExtractInt32(out First, out Second, out Third, out Fourth);
	}

	public static DelimitedEnumeratorChar DelimitedBy(this string Value, char Separator)
	{
		return new DelimitedEnumeratorChar(Value, Separator);
	}

	public static DelimitedEnumeratorChar DelimitedBy(this ReadOnlySpan<char> Value, char Separator)
	{
		return new DelimitedEnumeratorChar(Value, Separator);
	}

	public static DelimitedEnumeratorString DelimitedBy(this string Value, string Separator)
	{
		return new DelimitedEnumeratorString(Value, Separator);
	}

	public static DelimitedEnumeratorString DelimitedBy(this ReadOnlySpan<char> Value, string Separator)
	{
		return new DelimitedEnumeratorString(Value, Separator);
	}

	public static void Split(this string Text, char Separator, out string First, out string Second)
	{
		First = Text;
		Second = null;
		if (Text != null)
		{
			int num = Text.IndexOf(Separator);
			if (num >= 0)
			{
				First = Text.Substring(0, num);
				Second = Text.Substring(num + 1);
			}
		}
	}

	public static void Split(this string Text, char Separator, out string First, out string Second, out string Third)
	{
		First = Text;
		Second = null;
		Third = null;
		if (Text == null)
		{
			return;
		}
		int num = Text.IndexOf(Separator);
		if (num >= 0)
		{
			First = Text.Substring(0, num);
			int num2 = Text.IndexOf(Separator, num + 1);
			if (num2 >= 0)
			{
				Second = Text.Substring(num + 1, num2 - num - 1);
				Third = Text.Substring(num2 + 1);
			}
			else
			{
				Second = Text.Substring(num + 1);
				Third = null;
			}
		}
	}

	public static void AsDelimitedSpans(this string Text, char Separator, out ReadOnlySpan<char> First, out ReadOnlySpan<char> Second)
	{
		First = null;
		Second = null;
		int num = 0;
		DelimitedEnumeratorChar enumerator = Text.DelimitedBy(Separator).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (++num == 1)
			{
				First = current;
				continue;
			}
			if (num == 2)
			{
				Second = current;
				continue;
			}
			break;
		}
	}

	public static void AsDelimitedSpans(this string Text, char Separator, out ReadOnlySpan<char> First, out ReadOnlySpan<char> Second, out ReadOnlySpan<char> Third)
	{
		First = null;
		Second = null;
		Third = null;
		int num = 0;
		DelimitedEnumeratorChar enumerator = Text.DelimitedBy(Separator).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (++num == 1)
			{
				First = current;
				continue;
			}
			switch (num)
			{
			case 2:
				Second = current;
				break;
			case 3:
				Third = current;
				break;
			default:
				return;
			}
		}
	}

	public static void AsDelimitedSpans(this string Text, char Separator, out ReadOnlySpan<char> First, out ReadOnlySpan<char> Second, out ReadOnlySpan<char> Third, out ReadOnlySpan<char> Fourth)
	{
		First = null;
		Second = null;
		Third = null;
		Fourth = null;
		int num = 0;
		DelimitedEnumeratorChar enumerator = Text.DelimitedBy(Separator).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (++num == 1)
			{
				First = current;
				continue;
			}
			switch (num)
			{
			case 2:
				Second = current;
				break;
			case 3:
				Third = current;
				break;
			case 4:
				Fourth = current;
				break;
			default:
				return;
			}
		}
	}

	public static void AsDelimitedSpans(this string Text, char Separator, out ReadOnlySpan<char> First, out ReadOnlySpan<char> Second, out ReadOnlySpan<char> Third, out ReadOnlySpan<char> Fourth, out ReadOnlySpan<char> Fifth)
	{
		First = null;
		Second = null;
		Third = null;
		Fourth = null;
		Fifth = null;
		int num = 0;
		DelimitedEnumeratorChar enumerator = Text.DelimitedBy(Separator).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (++num == 1)
			{
				First = current;
				continue;
			}
			switch (num)
			{
			case 2:
				Second = current;
				break;
			case 3:
				Third = current;
				break;
			case 4:
				Fourth = current;
				break;
			case 5:
				Fifth = current;
				break;
			default:
				return;
			}
		}
	}

	public static void AsDelimitedSpans(this string Text, char Separator, out ReadOnlySpan<char> First, out ReadOnlySpan<char> Second, out ReadOnlySpan<char> Third, out ReadOnlySpan<char> Fourth, out ReadOnlySpan<char> Fifth, out ReadOnlySpan<char> Sixth)
	{
		First = null;
		Second = null;
		Third = null;
		Fourth = null;
		Fifth = null;
		Sixth = null;
		int num = 0;
		DelimitedEnumeratorChar enumerator = Text.DelimitedBy(Separator).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (++num == 1)
			{
				First = current;
				continue;
			}
			switch (num)
			{
			case 2:
				Second = current;
				break;
			case 3:
				Third = current;
				break;
			case 4:
				Fourth = current;
				break;
			case 5:
				Fifth = current;
				break;
			case 6:
				Sixth = current;
				break;
			default:
				return;
			}
		}
	}

	public static void Split(this ReadOnlySpan<char> Text, char Separator, out ReadOnlySpan<char> First, out ReadOnlySpan<char> Second)
	{
		First = null;
		Second = null;
		int num = 0;
		DelimitedEnumeratorChar enumerator = Text.DelimitedBy(Separator).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (++num == 1)
			{
				First = current;
				continue;
			}
			if (num == 2)
			{
				Second = current;
				continue;
			}
			break;
		}
	}

	public static void Split(this ReadOnlySpan<char> Text, char Separator, out ReadOnlySpan<char> First, out ReadOnlySpan<char> Second, out ReadOnlySpan<char> Third)
	{
		First = null;
		Second = null;
		Third = null;
		int num = 0;
		DelimitedEnumeratorChar enumerator = Text.DelimitedBy(Separator).GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (++num == 1)
			{
				First = current;
				continue;
			}
			switch (num)
			{
			case 2:
				Second = current;
				break;
			case 3:
				Third = current;
				break;
			default:
				return;
			}
		}
	}

	public static int LastSubdirectoryIndex(this string Path, string Directory, StringComparison Comparison = StringComparison.Ordinal)
	{
		int length = Path.Length;
		int length2 = Directory.Length;
		int num = length - 1;
		while (num != -1)
		{
			num = Path.LastIndexOf(Directory, num, Comparison);
			if (num == -1)
			{
				break;
			}
			if (num <= 0 || Path[num - 1].IsDirectorySeparatorInvariant())
			{
				int num2 = num + length2;
				if (num2 >= length || Path[num2].IsDirectorySeparatorInvariant())
				{
					return num;
				}
			}
		}
		return -1;
	}

	public static void CopyTo(this string Source, int SourceIndex, Span<char> Destination, int DestinationIndex, int Length)
	{
		for (int i = 0; i < Length; i++)
		{
			Destination[DestinationIndex + i] = Source[SourceIndex + i];
		}
	}

	public static string WithColor(this string Text, string Color)
	{
		return string.Create(Text.Length + Color.Length + 5, (Text, Color), (Span<char> Span, (string, string) Tuple) =>
		{
			int num = 0;
			int length = Tuple.Item1.Length;
			int length2 = Tuple.Item2.Length;
			Span[num++] = '{';
			Span[num++] = '{';
			Tuple.Item2.CopyTo(0, Span, num, length2);
			num += length2;
			Span[num++] = '|';
			Tuple.Item1.CopyTo(0, Span, num, length);
			num += length;
			Span[num++] = '}';
			Span[num] = '}';
		});
	}

	public static Color WithAlpha(this Color Color, float Alpha)
	{
		Color.a = Alpha;
		return Color;
	}

	public static T GetCyclicElement<T>(this IList<T> list, int Index)
	{
		return list.Count switch
		{
			0 => default, 
			1 => list[0], 
			_ => list[Index % list.Count], 
		};
	}

	public static T GetCyclicElement<T>(this T[] list, int Index)
	{
		return list.Length switch
		{
			0 => default, 
			1 => list[0], 
			_ => list[Index % list.Length], 
		};
	}

	public static bool Exists<T>(this IList<T> List, Func<int, T, bool> Predicate)
	{
		int i = 0;
		for (int count = List.Count; i < count; i++)
		{
			if (Predicate(i, List[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static void Fill<T>(this IList<T> List, T Value)
	{
		int i = 0;
		for (int count = List.Count; i < count; i++)
		{
			List[i] = Value;
		}
	}

	public static void Fill<T>(this IList<T> List, IEnumerable<T> Values)
	{
		using IEnumerator<T> enumerator = Values.GetEnumerator();
		int i = 0;
		for (int count = List.Count; i < count; i++)
		{
			if (!enumerator.MoveNext())
			{
				break;
			}
			List[i] = enumerator.Current;
		}
	}

	public static void Rebase<T>(this IList<T> List, IReadOnlyList<T> Other)
	{
		List.Clear();
		if (!Other.IsNullOrEmpty())
		{
			int i = 0;
			for (int count = Other.Count; i < count; i++)
			{
				List.Add(Other[i]);
			}
		}
	}

	public static StringBuilder Clear(this StringBuilder SB)
	{
		SB.Length = 0;
		return SB;
	}

	public static string ClipExceptFormatting(this StringBuilder SB, int maxLength, string suffixWhenTruncated = "")
	{
		if (ConsoleLib.Console.ColorUtility.LengthExceptFormatting(SB) > maxLength)
		{
			return ConsoleLib.Console.ColorUtility.ClipExceptFormatting(SB, maxLength - suffixWhenTruncated.Length) + suffixWhenTruncated;
		}
		return SB.ToString();
	}

	public static int LengthExceptFormatting(this string @string)
	{
		if (@string.IsNullOrEmpty())
		{
			return 0;
		}
		return ConsoleLib.Console.ColorUtility.LengthExceptFormatting(@string);
	}

	public static string ClipExceptFormatting(this string @string, int maxLength, string suffixWhenTruncated = "")
	{
		if (ConsoleLib.Console.ColorUtility.LengthExceptFormatting(@string) > maxLength)
		{
			return ConsoleLib.Console.ColorUtility.ClipExceptFormatting(@string, maxLength - suffixWhenTruncated.Length) + suffixWhenTruncated;
		}
		return @string.ToString();
	}

	public static bool ValueEquals(this StringBuilder SB, string Value)
	{
		if (SB.Length != Value.Length)
		{
			return false;
		}
		int i = 0;
		for (int length = Value.Length; i < length; i++)
		{
			if (SB[i] != Value[i])
			{
				return false;
			}
		}
		return true;
	}

	public static string Signed(this int Value, bool NotIfZero = false)
	{
		if (!(NotIfZero ? (Value > 0) : (Value >= 0)))
		{
			return Value.ToString();
		}
		return "+" + Value;
	}

	public static StringBuilder CompoundSigned(this StringBuilder SB, int Value)
	{
		if (SB.Length > 0 && Value >= 0)
		{
			SB.Append('+');
		}
		SB.Append(Value);
		return SB;
	}

	public static StringBuilder AppendSigned(this StringBuilder SB, int Value)
	{
		if (Value >= 0)
		{
			SB.Append('+');
		}
		SB.Append(Value);
		return SB;
	}

	public static StringBuilder AppendSigned(this StringBuilder SB, int Value, string Color)
	{
		return SB.Append((Value >= 0) ? '+' : '-').Append("{{").Append(Color)
			.Append('|')
			.Append(Math.Abs(Value))
			.Append("}}");
	}

	public static StringBuilder AppendRange(this StringBuilder SB, IEnumerable<string> Range, string Separator)
	{
		bool flag = true;
		foreach (string item in Range)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				SB.Append(Separator);
			}
			SB.Append(item);
		}
		return SB;
	}

	public static StringBuilder AppendThings(this StringBuilder SB, int Number, string Singular, string Plural)
	{
		return SB.Append(Number).Append(' ').Append((Number == 1) ? Singular : Plural);
	}

	public static StringBuilder Compound(this StringBuilder SB, string Text)
	{
		if (!Text.IsNullOrEmpty())
		{
			if (SB.Length > 0)
			{
				SB.Append(' ');
			}
			SB.Append(Text);
		}
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, string Text, string With)
	{
		if (!Text.IsNullOrEmpty())
		{
			if (SB.Length > 0)
			{
				SB.Append(With);
			}
			SB.Append(Text);
		}
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, string Text, char With)
	{
		if (!Text.IsNullOrEmpty())
		{
			if (SB.Length > 0)
			{
				SB.Append(With);
			}
			SB.Append(Text);
		}
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, char Ch)
	{
		if (SB.Length > 0)
		{
			SB.Append(' ');
		}
		SB.Append(Ch);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, char Ch, string With)
	{
		if (SB.Length > 0)
		{
			SB.Append(With);
		}
		SB.Append(Ch);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, char Ch, char With)
	{
		if (SB.Length > 0)
		{
			SB.Append(With);
		}
		SB.Append(Ch);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, int Value)
	{
		if (SB.Length > 0)
		{
			SB.Append(' ');
		}
		SB.Append(Value);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, int Value, string With)
	{
		if (SB.Length > 0)
		{
			SB.Append(With);
		}
		SB.Append(Value);
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, int Value, char With)
	{
		if (SB.Length > 0)
		{
			SB.Append(With);
		}
		SB.Append(Value);
		return SB;
	}

	public static void Compound(this ref Utf16ValueStringBuilder SB, string Value, string With)
	{
		if (SB.Length > 0)
		{
			SB.Append(With);
		}
		SB.Append(Value);
	}

	public static void Compound(this ref Utf16ValueStringBuilder SB, string Value, char With)
	{
		if (SB.Length > 0)
		{
			SB.Append(With);
		}
		SB.Append(Value);
	}

	public static void Compound(this ref Utf16ValueStringBuilder SB, string Value)
	{
		if (SB.Length > 0)
		{
			SB.Append(' ');
		}
		SB.Append(Value);
	}

	public static void AppendMultiple(this ref Utf16ValueStringBuilder SB, string Value1, string Value2)
	{
		SB.Append(Value1);
		SB.Append(Value2);
	}

	public static void AppendMultiple(this ref Utf16ValueStringBuilder SB, string Value1, string Value2, string Value3)
	{
		SB.Append(Value1);
		SB.Append(Value2);
		SB.Append(Value3);
	}

	public static void AppendMultiple(this ref Utf16ValueStringBuilder SB, string Value1, string Value2, string Value3, string Value4)
	{
		SB.Append(Value1);
		SB.Append(Value2);
		SB.Append(Value3);
		SB.Append(Value4);
	}

	public static void AppendMultiple(this ref Utf16ValueStringBuilder SB, string Value1, string Value2, string Value3, string Value4, string Value5)
	{
		SB.Append(Value1);
		SB.Append(Value2);
		SB.Append(Value3);
		SB.Append(Value4);
		SB.Append(Value5);
	}

	public static StringBuilder Unindent(this StringBuilder SB)
	{
		int i = 0;
		int length;
		for (length = SB.Length; i < length; i++)
		{
			if (!char.IsWhiteSpace(SB[i]))
			{
				SB.Remove(0, i);
				break;
			}
		}
		for (i = SB.Length - 1; i >= 0; i--)
		{
			if (!char.IsWhiteSpace(SB[i]))
			{
				SB.Remove(i + 1, SB.Length - i - 1);
				break;
			}
		}
		i = 0;
		length = SB.Length;
		int num = length;
		for (; i < length; i++)
		{
			char c = SB[i];
			if (c == '\n')
			{
				if (i - num <= 1)
				{
					num = i;
					continue;
				}
			}
			else if (num >= length || char.IsWhiteSpace(c))
			{
				continue;
			}
			SB.Remove(num + 1, i - num - 1);
			length -= i - num - 1;
			i = num;
			num = length;
		}
		return SB;
	}

	public static StringBuilder Compound(this StringBuilder SB, IEnumerable<string> Values, string With = " ")
	{
		foreach (string Value in Values)
		{
			if (SB.Length > 0)
			{
				SB.Append(With);
			}
			SB.Append(Value);
		}
		return SB;
	}

	public static string ToMaskString(this int Mask)
	{
		Span<char> span = stackalloc char[32];
		for (int num = span.Length - 1; num >= 0; num--)
		{
			span[num] = (((Mask & (1 << num)) != 0) ? '1' : '0');
		}
		return span.ToString();
	}

	public static string ToMaskString(this short Mask)
	{
		Span<char> span = stackalloc char[16];
		for (int num = span.Length - 1; num >= 0; num--)
		{
			span[num] = (((Mask & (1 << num)) != 0) ? '1' : '0');
		}
		return span.ToString();
	}

	public static StringBuilder AppendMask(this StringBuilder Builder, int Mask, int Length = 32)
	{
		for (int num = Length - 1; num >= 0; num--)
		{
			Builder.Append(((Mask & (1 << num)) != 0) ? '1' : '0');
		}
		return Builder;
	}

	public static StringBuilder AppendMask(this StringBuilder Builder, int Mask, int Start, int Length)
	{
		for (int num = Start + Length - 1; num >= Start; num--)
		{
			Builder.Append(((Mask & (1 << num)) != 0) ? '1' : '0');
		}
		return Builder;
	}

	public static StringBuilder AppendJoin(this StringBuilder SB, string Separator, IEnumerable<string> Values)
	{
		using IEnumerator<string> enumerator = Values.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return SB;
		}
		string current = enumerator.Current;
		if (current != null)
		{
			SB.Append(current);
		}
		while (enumerator.MoveNext())
		{
			SB.Append(Separator);
			current = enumerator.Current;
			if (current != null)
			{
				SB.Append(current);
			}
		}
		return SB;
	}

	public static StringBuilder AppendItVerb(this StringBuilder SB, XRL.World.GameObject Object, string Verb, bool Capitalized = false)
	{
		return SB.Append(Capitalized ? Object.It : Object.it).Append(' ').Append(Object.GetVerb(Verb, PrependSpace: false, PronounAntecedent: true));
	}

	public static StringBuilder CompoundItVerb(this StringBuilder SB, XRL.World.GameObject Object, string Verb, char With = ' ', bool Capitalized = true)
	{
		return SB.Compound(Capitalized ? Object.It : Object.it, With).Append(' ').Append(Object.GetVerb(Verb, PrependSpace: false, PronounAntecedent: true));
	}

	public static StringBuilder AppendCase(this StringBuilder SB, string Value)
	{
		if (Value.IsNullOrEmpty())
		{
			return SB;
		}
		SB.AppendCase(Value, SB.NextIsCapital());
		return SB;
	}

	public static StringBuilder AppendCase(this StringBuilder SB, string Value, bool Capitalize)
	{
		if (Value.IsNullOrEmpty())
		{
			return SB;
		}
		SB.Append(Value);
		char c = Value[0];
		if (char.IsUpper(c) != Capitalize)
		{
			int length = Value.Length;
			SB[SB.Length - length] = (Capitalize ? char.ToUpper(c) : char.ToLower(c));
		}
		return SB;
	}

	public static StringBuilder AppendUpper(this StringBuilder SB, string Value)
	{
		if (Value.IsNullOrEmpty())
		{
			return SB;
		}
		int length = Value.Length;
		SB.EnsureCapacity(SB.Length + length);
		for (int i = 0; i < length; i++)
		{
			SB.Append(char.ToUpperInvariant(Value[i]));
		}
		return SB;
	}

	public static StringBuilder AppendLower(this StringBuilder SB, string Value)
	{
		if (Value.IsNullOrEmpty())
		{
			return SB;
		}
		int length = Value.Length;
		SB.EnsureCapacity(SB.Length + length);
		for (int i = 0; i < length; i++)
		{
			SB.Append(char.ToLowerInvariant(Value[i]));
		}
		return SB;
	}

	public static StringBuilder TrimEnd(this StringBuilder SB, char Character = ' ')
	{
		int num = SB.Length;
		while (SB[num - 1] == Character)
		{
			num = (SB.Length = num - 1);
		}
		return SB;
	}

	public static StringBuilder VariableReplace(this StringBuilder Text, StringMap<ReplacerEntry> Replacers = null, IList<TextArgument> Arguments = null, int DefaultArgument = -1)
	{
		GameText.Process(Text, Replacers, null, Arguments, DefaultArgument);
		return Text;
	}

	public static string VariableReplace(this string Text, StringMap<ReplacerEntry> Replacers = null, IList<TextArgument> Arguments = null, int DefaultArgument = -1)
	{
		GameText.Process(ref Text, Replacers, null, Arguments, DefaultArgument);
		return Text;
	}

	public static StringBuilder StripColorFormatting(this StringBuilder SB)
	{
		ConsoleLib.Console.ColorUtility.StripFormatting(SB);
		return SB;
	}

	public static bool EndsWith(this StringBuilder SB, char Value)
	{
		return SB[SB.Length - 1] == Value;
	}

	public static bool IsWordCharacter(this char ch)
	{
		switch (ch)
		{
		default:
			if (ch < 'A' || ch > 'Z')
			{
				if (ch >= '0')
				{
					return ch <= '9';
				}
				return false;
			}
			break;
		case '-':
		case 'a':
		case 'b':
		case 'c':
		case 'd':
		case 'e':
		case 'f':
		case 'g':
		case 'h':
		case 'i':
		case 'j':
		case 'k':
		case 'l':
		case 'm':
		case 'n':
		case 'o':
		case 'p':
		case 'q':
		case 'r':
		case 's':
		case 't':
		case 'u':
		case 'v':
		case 'w':
		case 'x':
		case 'y':
		case 'z':
			break;
		}
		return true;
	}

	public static char ToLowerASCII(this char Char)
	{
		if (Char >= 'A' && Char <= 'Z')
		{
			Char = (char)(Char | 0x20);
		}
		return Char;
	}

	public static char ToUpperASCII(this char Char)
	{
		if (Char >= 'a' && Char <= 'z')
		{
			Char = (char)(Char & 0xFFDF);
		}
		return Char;
	}

	public static bool NextIsCapital(this StringBuilder SB)
	{
		if (SB.Length == 0)
		{
			return true;
		}
		char c = SB[SB.Length - 1];
		if (c != '\n')
		{
			if (c == ' ' && SB.Length >= 2)
			{
				return SB[SB.Length - 2] == '.';
			}
			return false;
		}
		return true;
	}

	public static void Rebase<K, V>(this IDictionary<K, V> Dictionary, IReadOnlyDictionary<K, V> Other)
	{
		Dictionary.Clear();
		if (Other.IsNullOrEmpty())
		{
			return;
		}
		foreach (KeyValuePair<K, V> item in Other)
		{
			Dictionary.Add(item.Key, item.Value);
		}
	}

	public static void SetRange<K, V>(this IDictionary<K, V> Dictionary, IReadOnlyDictionary<K, V> Other)
	{
		if (Other.IsNullOrEmpty())
		{
			return;
		}
		foreach (KeyValuePair<K, V> item in Other)
		{
			Dictionary[item.Key] = item.Value;
		}
	}

	public static void AddRange<K, V>(this IDictionary<K, V> Dictionary, IReadOnlyDictionary<K, V> Other)
	{
		if (Other.IsNullOrEmpty())
		{
			return;
		}
		foreach (KeyValuePair<K, V> item in Other)
		{
			Dictionary.Add(item.Key, item.Value);
		}
	}

	public static int TryAddRange<K, V>(this IDictionary<K, V> Dictionary, IReadOnlyDictionary<K, V> Other)
	{
		int num = 0;
		if (!Other.IsNullOrEmpty())
		{
			foreach (KeyValuePair<K, V> item in Other)
			{
				if (Dictionary.TryAdd(item.Key, item.Value))
				{
					num++;
				}
			}
		}
		return num;
	}

	public static void Set<K, V>(this Dictionary<K, V> dictionary, K key, V value)
	{
		dictionary[key] = value;
	}

	public static bool PairEquals<K, V>(this Dictionary<K, V> Dictionary, Dictionary<K, V> Other, bool NullOrEmpty = true) where V : IComparable<V>
	{
		if (Dictionary == Other)
		{
			return true;
		}
		if (NullOrEmpty && Dictionary.IsNullOrEmpty() && Other.IsNullOrEmpty())
		{
			return true;
		}
		if (Dictionary == null || Other == null)
		{
			return false;
		}
		if (Dictionary.Count != Other.Count)
		{
			return false;
		}
		foreach (KeyValuePair<K, V> item in Dictionary)
		{
			if (!Other.TryGetValue(item.Key, out var value))
			{
				return false;
			}
			if (!item.Value.Equals(value))
			{
				return false;
			}
		}
		return true;
	}

	public static bool ChanceIn(this int Chance, int Odds)
	{
		if (Chance > 0)
		{
			if (Chance < Odds)
			{
				return Stat.Random(1, Odds) <= Chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this int chance)
	{
		if (chance > 0)
		{
			if (chance < 10)
			{
				return Stat.Random(1, 10) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this int? chance)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 10))
			{
				return Stat.Random(1, 10) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this int chance)
	{
		if (chance > 0)
		{
			if (chance < 100)
			{
				return Stat.Random(1, 100) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this int? chance)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 100))
			{
				return Stat.Random(1, 100) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this int chance)
	{
		if (chance > 0)
		{
			if (chance < 1000)
			{
				return Stat.Random(1, 1000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this int? chance)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 1000))
			{
				return Stat.Random(1, 1000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this int chance)
	{
		if (chance > 0)
		{
			if (chance < 10000)
			{
				return Stat.Random(1, 10000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this int? chance)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 10000))
			{
				return Stat.Random(1, 10000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this int chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 10)
			{
				return rnd.Next(1, 11) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this int? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 10))
			{
				return rnd.Next(1, 11) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this int chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 100)
			{
				return rnd.Next(1, 101) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this int? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 100))
			{
				return rnd.Next(1, 101) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this int chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 1000)
			{
				return rnd.Next(1, 1001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this int? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 1000))
			{
				return rnd.Next(1, 1001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this int chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 10000)
			{
				return rnd.Next(1, 10001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this int? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 10000))
			{
				return rnd.Next(1, 10001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this long chance)
	{
		if (chance > 0)
		{
			if (chance < 10)
			{
				return Stat.Random(1, 10) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this long? chance)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 10))
			{
				return Stat.Random(1, 10) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this long chance)
	{
		if (chance > 0)
		{
			if (chance < 100)
			{
				return Stat.Random(1, 100) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this long? chance)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 100))
			{
				return Stat.Random(1, 100) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this long chance)
	{
		if (chance > 0)
		{
			if (chance < 1000)
			{
				return Stat.Random(1, 1000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this long? chance)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 1000))
			{
				return Stat.Random(1, 1000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this long chance)
	{
		if (chance > 0)
		{
			if (chance < 10000)
			{
				return Stat.Random(1, 10000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this long? chance)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 10000))
			{
				return Stat.Random(1, 10000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this long chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 10)
			{
				return rnd.Next(1, 11) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this long? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 10))
			{
				return rnd.Next(1, 11) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this long chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 100)
			{
				return rnd.Next(1, 101) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this long? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 100))
			{
				return rnd.Next(1, 101) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this long chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 1000)
			{
				return rnd.Next(1, 1001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this long? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 1000))
			{
				return rnd.Next(1, 1001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this long chance, System.Random rnd)
	{
		if (chance > 0)
		{
			if (chance < 10000)
			{
				return rnd.Next(1, 10001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this long? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0)
		{
			if (!(chance >= 10000))
			{
				return rnd.Next(1, 10001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this float chance)
	{
		if (chance > 0f)
		{
			if (!(chance >= 10f))
			{
				return (float)Stat.Random(1, 10) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this float? chance)
	{
		if (chance.HasValue && chance > 0f)
		{
			if (!(chance >= 10f))
			{
				return (float)Stat.Random(1, 10) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this float chance)
	{
		if (chance > 0f)
		{
			if (!(chance >= 100f))
			{
				return (float)Stat.Random(1, 100) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this float? chance)
	{
		if (chance.HasValue && chance > 0f)
		{
			if (!(chance >= 100f))
			{
				return (float)Stat.Random(1, 100) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this float chance)
	{
		if (chance > 0f)
		{
			if (!(chance >= 1000f))
			{
				return (float)Stat.Random(1, 1000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this float? chance)
	{
		if (chance.HasValue && chance > 0f)
		{
			if (!(chance >= 1000f))
			{
				return (float)Stat.Random(1, 1000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this float chance)
	{
		if (chance > 0f)
		{
			if (!(chance >= 10000f))
			{
				return (float)Stat.Random(1, 10000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this float? chance)
	{
		if (chance.HasValue && chance > 0f)
		{
			if (!(chance >= 10000f))
			{
				return (float)Stat.Random(1, 10000) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this float chance, System.Random rnd)
	{
		if (chance > 0f)
		{
			if (!(chance >= 10f))
			{
				return (float)rnd.Next(1, 11) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10(this float? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0f)
		{
			if (!(chance >= 10f))
			{
				return (float)rnd.Next(1, 11) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this float chance, System.Random rnd)
	{
		if (chance > 0f)
		{
			if (!(chance >= 100f))
			{
				return (float)rnd.Next(1, 101) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in100(this float? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0f)
		{
			if (!(chance >= 100f))
			{
				return (float)rnd.Next(1, 101) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this float chance, System.Random rnd)
	{
		if (chance > 0f)
		{
			if (!(chance >= 1000f))
			{
				return (float)rnd.Next(1, 1001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in1000(this float? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0f)
		{
			if (!(chance >= 1000f))
			{
				return (float)rnd.Next(1, 1001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this float chance, System.Random rnd)
	{
		if (chance > 0f)
		{
			if (!(chance >= 10000f))
			{
				return (float)rnd.Next(1, 10001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static bool in10000(this float? chance, System.Random rnd)
	{
		if (chance.HasValue && chance > 0f)
		{
			if (!(chance >= 10000f))
			{
				return (float)rnd.Next(1, 10001) <= chance;
			}
			return true;
		}
		return false;
	}

	public static void AddIf<T>(this List<T> list, T element, Func<T, bool> conditional)
	{
		if (conditional(element))
		{
			list.Add(element);
		}
	}

	public static void AddIfNot<T>(this List<T> list, T element, Func<T, bool> conditional)
	{
		if (!conditional(element))
		{
			list.Add(element);
		}
	}

	public static void AddIfNotNull<T>(this List<T> list, T element)
	{
		if (element != null)
		{
			list.Add(element);
		}
	}

	public static bool ShowSuccess(this XRL.World.GameObject Object, string Message, bool Log = true)
	{
		if (Object != null && Object.IsPlayer())
		{
			Popup.Show(Message, null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, Log);
		}
		return true;
	}

	public static bool ShowFailure(this XRL.World.GameObject Object, string Message)
	{
		if (Object != null && Object.IsPlayer())
		{
			Popup.ShowFail(Message);
		}
		return false;
	}

	public static void Return<T>(this ArrayPool<T> Pool, ref T[] Array, bool Clear = false)
	{
		Pool.Return(Array, Clear);
		Array = System.Array.Empty<T>();
	}

	public static List<T> ShuffleInPlace<T>(this List<T> list, System.Random R = null)
	{
		if (R == null)
		{
			Algorithms.RandomShuffleInPlace(list);
		}
		else
		{
			Algorithms.RandomShuffleInPlace(list, R);
		}
		return list;
	}

	public static IList<T> ShuffleInPlace<T>(this IList<T> list, System.Random R = null)
	{
		if (R == null)
		{
			Algorithms.RandomShuffleInPlace(list);
		}
		else
		{
			Algorithms.RandomShuffleInPlace(list, R);
		}
		return list;
	}

	public static T[] ShuffleInPlace<T>(this T[] list, System.Random R = null)
	{
		if (R == null)
		{
			Algorithms.RandomShuffleInPlace(list);
		}
		else
		{
			Algorithms.RandomShuffleInPlace(list, R);
		}
		return list;
	}

	public static IEnumerable<int> ShuffledRange(int low, int high, System.Random rng = null)
	{
		if (rng == null)
		{
			rng = Stat.Rnd;
		}
		int count = high - low;
		int num = count + 3;
		if (num % 2 == 0)
		{
			num++;
		}
		bool flag;
		do
		{
			num += 2;
			flag = true;
			int num2 = (int)Math.Sqrt(num) + 1;
			int num3 = 3;
			while (flag && num3 <= num2)
			{
				flag &= num % num3 != 0;
				num3 += 2;
			}
		}
		while (!flag);
		long a = rng.Next(2, num);
		long pl = num;
		int n = 0;
		int found = 0;
		while (found < count)
		{
			n++;
			int num4 = (int)(n * a % pl);
			if (num4 < count)
			{
				found++;
				yield return num4 + low;
			}
		}
	}

	public static IEnumerable<T> InRandomOrderNoAlloc<T>(this List<T> list, System.Random rng = null)
	{
		if (rng == null)
		{
			rng = Stat.Rnd;
		}
		foreach (int item in ShuffledRange(0, list.Count - 1, rng))
		{
			yield return list[item];
		}
	}

	public static IEnumerable<T> InRandomOrder<T>(this List<T> list, System.Random R = null)
	{
		if (R == null)
		{
			R = Stat.Rnd;
		}
		foreach (int item in Enumerable.Range(0, list.Count).ToList().Shuffle(R))
		{
			yield return list[item];
		}
	}

	public static List<T> Shuffle<T>(this List<T> list, System.Random R = null)
	{
		if (R == null)
		{
			return new List<T>(Algorithms.RandomShuffle(list));
		}
		return new List<T>(Algorithms.RandomShuffle(list, R));
	}

	public static double toRadians(this double angle)
	{
		return Math.PI * angle / 180.0;
	}

	public static float toRadians(this float angle)
	{
		return (float)((double)angle).toRadians();
	}

	public static float toRadians(this int angle)
	{
		return (float)((double)angle).toRadians();
	}

	public static double toDegrees(this double angle)
	{
		return angle * (180.0 / Math.PI);
	}

	public static float toDegrees(this float angle)
	{
		return (float)((double)angle).toDegrees();
	}

	public static double normalizeRadians(this double angle)
	{
		return angle - Math.PI * 2.0 * Math.Floor((angle + Math.PI) / (Math.PI * 2.0));
	}

	public static float normalizeRadians(this float angle)
	{
		return (float)((double)angle).normalizeRadians();
	}

	public static int normalizeDegrees(this int angle)
	{
		return angle % 360;
	}

	public static string DebugNode(this XmlTextReader Reader)
	{
		if (Reader.NodeType == XmlNodeType.Element || Reader.NodeType == XmlNodeType.EndElement)
		{
			string text = Reader.LineNumber + ":" + Reader.LinePosition + ((Reader.NodeType == XmlNodeType.EndElement && !Reader.IsEmptyElement) ? "</" : "<") + Reader.Name;
			bool isEmptyElement = Reader.IsEmptyElement;
			if (Reader.HasAttributes)
			{
				while (Reader.MoveToNextAttribute())
				{
					text = text + " " + Reader.Name + "=\"" + Reader.Value + "\"";
				}
			}
			return text + (isEmptyElement ? " />" : ">");
		}
		return "[" + Reader.NodeType.ToString() + "]";
	}

	public static bool SkipToEnd(this XmlTextReader Reader)
	{
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return true;
		}
		int depth = Reader.Depth;
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.EndElement && depth == Reader.Depth)
			{
				return true;
			}
		}
		return false;
	}

	public static double DiminishingReturns(this double val, double scale)
	{
		if (val < 0.0)
		{
			return 0.0 - (0.0 - val).DiminishingReturns(scale);
		}
		double num = val / scale;
		return (Math.Sqrt(8.0 * num + 1.0) - 1.0) / 2.0 * scale;
	}

	public static float DiminishingReturns(this float val, double scale)
	{
		if (val < 0f)
		{
			return 0f - (0f - val).DiminishingReturns(scale);
		}
		double num = (double)val / scale;
		return (float)((Math.Sqrt(8.0 * num + 1.0) - 1.0) / 2.0 * scale);
	}

	public static int DiminishingReturns(this int val, double scale)
	{
		if (val < 0)
		{
			return -(-val).DiminishingReturns(scale);
		}
		double num = (double)val / scale;
		return (int)((Math.Sqrt(8.0 * num + 1.0) - 1.0) / 2.0 * scale);
	}

	public static bool EqualsNoCase(this string val, string cmp)
	{
		return string.Equals(val, cmp, StringComparison.OrdinalIgnoreCase);
	}

	public static bool EqualsNoCase(this string val, ReadOnlySpan<char> cmp)
	{
		return MemoryExtensions.Equals(cmp, val, StringComparison.OrdinalIgnoreCase);
	}

	public static bool EqualsNoCase(this ReadOnlySpan<char> val, ReadOnlySpan<char> cmp)
	{
		return val.Equals(cmp, StringComparison.OrdinalIgnoreCase);
	}

	public static uint GetStableHashCode32(this string Value)
	{
		return Hash.FNV1A32(Value);
	}

	public static uint GetStableHashCode32(this string Value, uint Hash)
	{
		return Genkit.Hash.FNV1A32(Value, Hash);
	}

	public static void GetStableHashCode32(this string Value, ref uint Hash)
	{
		Hash = Genkit.Hash.FNV1A32(Value, Hash);
	}

	public static ulong GetStableHashCode64(this string Value)
	{
		return Hash.FNV1A64(Value);
	}

	public static ulong GetStableHashCode64(this string Value, ulong Hash)
	{
		return Genkit.Hash.FNV1A64(Value, Hash);
	}

	public static void GetStableHashCode64(this string Value, ref ulong Hash)
	{
		Hash = Genkit.Hash.FNV1A64(Value, Hash);
	}

	public static ulong GetStableHashCode64(this Type Type, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		FieldInfo[] fields = Type.GetFields(Flags);
		ulong Hash = Type.GetName(Full: true).GetStableHashCode64();
		int i = 0;
		for (int num = fields.Length; i < num; i++)
		{
			if ((fields[i].Attributes & Mask) == 0)
			{
				fields[i].FieldType.GetName(Full: true).GetStableHashCode64(ref Hash);
			}
		}
		return Hash;
	}

	public static bool EqualsEmptyEqualsNull(this string val, string cmp)
	{
		if (val == cmp)
		{
			return true;
		}
		if (string.IsNullOrEmpty(val) && string.IsNullOrEmpty(cmp))
		{
			return true;
		}
		return false;
	}

	public static T Deserialize<T>(this JsonSerializer serializer, string filePath)
	{
		using Stream stream = Platform.IO.File.OpenRead(filePath);
		using StreamReader reader = new StreamReader(stream);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		return serializer.Deserialize<T>(reader2);
	}

	public static T Decompress<T>(this JsonSerializer serializer, string file)
	{
		using Stream stream = Platform.IO.File.OpenRead(file);
		using GZipStream stream2 = new GZipStream(stream, System.IO.Compression.CompressionLevel.Fastest);
		using StreamReader reader = new StreamReader(stream2);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		return serializer.Deserialize<T>(reader2);
	}

	public static void Serialize(this JsonSerializer serializer, string file, object value)
	{
		using Stream stream = Platform.IO.File.OpenWrite(file);
		using StreamWriter textWriter = new StreamWriter(stream);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		serializer.Serialize(jsonWriter, value);
	}

	public static void Compress(this JsonSerializer serializer, string file, object value)
	{
		using Stream stream = Platform.IO.File.OpenWrite(file);
		using GZipStream stream2 = new GZipStream(stream, System.IO.Compression.CompressionLevel.Fastest);
		using StreamWriter textWriter = new StreamWriter(stream2);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		serializer.Serialize(jsonWriter, value);
	}

	public static int IndexOf<T>(this T[] Self, T Value)
	{
		return Array.IndexOf(Self, Value);
	}

	public static bool Contains<T>(this T[] Self, T Value)
	{
		return Array.IndexOf(Self, Value) != -1;
	}

	public static T[][] With<T>(this T[][] Array, int Size)
	{
		int i = 0;
		for (int num = Array.Length; i < num; i++)
		{
			Array[i] = new T[Size];
		}
		return Array;
	}

	public static int IndexOf<T>(this IReadOnlyList<T> Self, T Value)
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for (int i = 0; i < Self.Count; i++)
		{
			if (equalityComparer.Equals(Self[i], Value))
			{
				return i;
			}
		}
		return -1;
	}

	public static bool Contains<T>(this IReadOnlyList<T> Self, T Value)
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for (int i = 0; i < Self.Count; i++)
		{
			if (equalityComparer.Equals(Self[i], Value))
			{
				return true;
			}
		}
		return false;
	}

	public static void Populate(this JsonSerializer serializer, string file, object target)
	{
		using Stream stream = Platform.IO.File.OpenRead(file);
		using StreamReader reader = new StreamReader(stream);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		serializer.Populate(reader2, target);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadLockSlim TakeReadLock(this ReaderWriterLockSlim Lock)
	{
		Lock.EnterReadLock();
		return new ReadLockSlim(Lock);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static UpgradeableReadLockSlim TakeUpgradeableReadLock(this ReaderWriterLockSlim Lock)
	{
		Lock.EnterUpgradeableReadLock();
		return new UpgradeableReadLockSlim(Lock);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WriteLockSlim TakeWriteLock(this ReaderWriterLockSlim Lock)
	{
		Lock.EnterWriteLock();
		return new WriteLockSlim(Lock);
	}

	public static ConstructorInfo GetDefaultConstructor(this Type Value, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
	{
		return Value.GetConstructor(Flags, null, Type.EmptyTypes, null);
	}

	public static IEnumerable<Type> YieldInheritedTypes(this Type Value)
	{
		yield return Value.BaseType;
		Type[] interfaces = Value.GetInterfaces();
		for (int i = 0; i < interfaces.Length; i++)
		{
			yield return interfaces[i];
		}
	}

	public static int GetSubclassDepth(this Type Type)
	{
		Type type = Type;
		int num = 0;
		while (true)
		{
			type = type.BaseType;
			if ((object)type == null)
			{
				break;
			}
			num++;
		}
		return num;
	}

	public static int GetSubclassDepth(this Type Type, Type BaseType)
	{
		Type type = Type;
		int num = 0;
		while (true)
		{
			if ((object)type == BaseType)
			{
				return num;
			}
			type = type.BaseType;
			if ((object)type == null)
			{
				break;
			}
			num++;
		}
		throw new ArgumentException(Type.Name + " is not a subclass of " + BaseType.Name + ".");
	}

	public static bool IsSubclassOfGeneric(this Type Current, Type Base)
	{
		do
		{
			if (Current.IsGenericType && (object)Current.GetGenericTypeDefinition() == Base)
			{
				return true;
			}
			Current = Current.BaseType;
		}
		while (Current != null);
		return false;
	}

	public static string GetName(this MethodInfo Method, bool Signature = false, bool Full = false)
	{
		if (!Full && !Signature)
		{
			return Method.Name;
		}
		using TextBuilder textBuilder = TextBuilder.Get();
		Method.AppendName(textBuilder, Signature, Full);
		return textBuilder.ToString();
	}

	public static void AppendName(this MethodInfo Method, TextBuilder Builder, bool Signature = false, bool Full = false)
	{
		if (Full)
		{
			Method.DeclaringType.AppendName(Builder, Full);
			Builder.Append('.');
		}
		Builder.Append(Method.Name);
		if (!Signature)
		{
			return;
		}
		ParameterInfo[] parameters = Method.GetParameters();
		Builder.Append('(');
		for (int i = 0; i < parameters.Length; i++)
		{
			if (i != 0)
			{
				Builder.Append(", ");
			}
			parameters[i].ParameterType.AppendName(Builder, Full);
		}
		Builder.Append(')');
	}

	public static string GetName(this Type Type, bool Full = false)
	{
		bool flag = true;
		Type type = Nullable.GetUnderlyingType(Type);
		if ((object)type == null)
		{
			flag = false;
			type = Type;
		}
		bool isGenericType = type.IsGenericType;
		if (!flag && !isGenericType)
		{
			if (!Full)
			{
				return type.Name;
			}
			return type.FullName;
		}
		using TextBuilder textBuilder = TextBuilder.Get();
		AppendNameInternal(type, textBuilder, flag, isGenericType, Full);
		return textBuilder.ToString();
	}

	public static void AppendName(this Type Type, TextBuilder Builder, bool Full = false)
	{
		bool nullable = true;
		Type type = Nullable.GetUnderlyingType(Type);
		if ((object)type == null)
		{
			nullable = false;
			type = Type;
		}
		AppendNameInternal(type, Builder, nullable, type.IsGenericType, Full);
	}

	private static void AppendNameInternal(Type Type, TextBuilder Builder, bool Nullable, bool Generic, bool Full)
	{
		string text = (Full ? Type.FullName : Type.Name);
		if (Generic)
		{
			int num = text.IndexOf('`');
			if (num >= 0)
			{
				Builder.Append(text, 0, num);
			}
			else
			{
				Builder.Append(text);
			}
			Builder.Append('<');
			Type[] genericArguments = Type.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (i != 0)
				{
					Builder.Append(',');
				}
				genericArguments[i].AppendName(Builder, Full);
			}
			Builder.Append('>');
		}
		else
		{
			Builder.Append(text);
		}
		if (Nullable)
		{
			Builder.Append('?');
		}
	}

	public static FieldInfo[] GetCachedFields(this Type Type)
	{
		if (!TypeFields.TryGetValue(Type, out var value))
		{
			value = (TypeFields[Type] = Type.GetFields(BindingFlags.Instance | BindingFlags.Public));
		}
		return value;
	}
}
