using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Collections;
using XRL.Language;
using XRL.Rules;
using XRL.World;

namespace XRL;

public static class Extensions
{
	public static RectTransform CopyFrom(this RectTransform self, RectTransform other)
	{
		if (self.anchorMin != other.anchorMin)
		{
			self.anchorMin = other.anchorMin;
		}
		if (self.anchorMax != other.anchorMax)
		{
			self.anchorMax = other.anchorMax;
		}
		if (self.anchoredPosition != other.anchoredPosition)
		{
			self.anchoredPosition = other.anchoredPosition;
		}
		if (self.sizeDelta != other.sizeDelta)
		{
			self.sizeDelta = other.sizeDelta;
		}
		if (self.pivot != other.pivot)
		{
			self.pivot = other.pivot;
		}
		return self;
	}

	public static string Things(this int num, string what, string whatPlural = null)
	{
		if (num == 1)
		{
			return "1 " + what;
		}
		return num + " " + (whatPlural ?? Grammar.Pluralize(what));
	}

	public static string Things(this float num, string what, string whatPlural = null)
	{
		if (num == 1f)
		{
			return "1 " + what;
		}
		return num + " " + (whatPlural ?? Grammar.Pluralize(what));
	}

	public static string Things(this double num, string what, string whatPlural = null)
	{
		if (num == 1.0)
		{
			return "1 " + what;
		}
		return num + " " + (whatPlural ?? Grammar.Pluralize(what));
	}

	public static StringBuilder DumpStringBuilder(this List<XRL.World.GameObject> list, StringBuilder SB = null)
	{
		if (SB == null)
		{
			SB = new StringBuilder();
		}
		for (int i = 0; i < list.Count; i++)
		{
			SB.Append(i).Append(": ").Append(list[i].DebugName)
				.Append('\n');
		}
		return SB;
	}

	public static StringBuilder AppendColored(this StringBuilder SB, string color, string text)
	{
		return SB.Append("{{").Append(color).Append("|")
			.Append(text)
			.Append("}}");
	}

	public static StringBuilder AppendRules(this StringBuilder SB, string text)
	{
		if (!text.IsNullOrEmpty())
		{
			SB.Append("\n{{rules|").Append(text).Append("}}");
		}
		return SB;
	}

	public static StringBuilder AppendRules(this StringBuilder SB, string text, Action<StringBuilder> proc)
	{
		if (!text.IsNullOrEmpty())
		{
			SB.Append("\n{{rules|").Append(text);
			proc?.Invoke(SB);
			SB.Append("}}");
		}
		return SB;
	}

	public static StringBuilder AppendRules(this StringBuilder SB, Action<StringBuilder> appender)
	{
		SB.Append("\n{{rules|");
		appender(SB);
		SB.Append("}}");
		return SB;
	}

	public static StringBuilder AppendRules(this StringBuilder SB, Action<StringBuilder> appender, Action<StringBuilder> proc)
	{
		SB.Append("\n{{rules|");
		appender(SB);
		proc(SB);
		SB.Append("}}");
		return SB;
	}

	public static string Dump(this List<XRL.World.GameObject> list)
	{
		return list.DumpStringBuilder().ToString();
	}

	public static int Roll(this string Dice)
	{
		return Stat.Roll(Dice);
	}

	public static int RollMin(this string Dice)
	{
		return Stat.RollMin(Dice);
	}

	public static int RollMax(this string Dice)
	{
		return Stat.RollMax(Dice);
	}

	public static int RollCached(this string Dice)
	{
		return Stat.RollCached(Dice);
	}

	public static int RollMinCached(this string Dice)
	{
		return Stat.RollMinCached(Dice);
	}

	public static int RollMaxCached(this string Dice)
	{
		return Stat.RollMaxCached(Dice);
	}

	public static double RollAverageCached(this string Dice)
	{
		return Stat.RollAverageCached(Dice);
	}

	public static DieRoll GetCachedDieRoll(this string Dice)
	{
		return Stat.GetCachedDieRoll(Dice);
	}

	public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context)
	{
		return new SynchronizationContextAwaiter(context);
	}

	public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (list is List<T> list2)
		{
			list2.AddRange(items);
			return;
		}
		if (list is PooledList<T> pooledList)
		{
			pooledList.AddRange(items);
			return;
		}
		if (list is Rack<T> rack)
		{
			rack.AddRange(items);
			return;
		}
		foreach (T item in items)
		{
			list.Add(item);
		}
	}

	public static void StableSortInPlace<T>(this IList<T> List) where T : IComparable<T>
	{
		Algorithms.StableSortInPlace(List);
	}

	public static void StableSortInPlace<T>(this IList<T> List, Comparison<T> Comparison)
	{
		Algorithms.StableSortInPlace(List, Comparison);
	}

	public static void StableSortInPlace<T>(this IList<T> List, IComparer<T> Comparer)
	{
		Algorithms.StableSortInPlace(List, Comparer);
	}

	/// <summary>
	///     Gets a <see cref="!:ScopeDisposedList" /> copy of the source collection.
	/// </summary>
	/// <remarks>  The resulting collection must be <c>Dispose()</c>ed of. Intended to be used
	///     with <c>using</c>: <c>using var copy = list.GetScopeDisposedCopy();</c>
	/// </remarks>
	public static ScopeDisposedList<T> GetScopeDisposedCopy<T>(this IEnumerable<T> sourceList)
	{
		return ScopeDisposedList<T>.GetFromPoolFilledWith(sourceList);
	}

	/// <summary>
	///     Gets a <see cref="!:ScopeDisposedList" /> copy of the source collection.
	/// </summary>
	/// <remarks>  The resulting collection must be <c>Dispose()</c>ed of. Intended to be used
	///     with <c>using</c>: <c>using var copy = list.GetScopeDisposedCopy();</c>
	/// </remarks>
	public static ScopeDisposedList<T> GetScopeDisposedCopy<T>(this IReadOnlyCollection<T> sourceList)
	{
		return ScopeDisposedList<T>.GetFromPoolFilledWith(sourceList);
	}

	/// <summary>
	///     Gets a <see cref="!:ScopeDisposedList" /> copy of the source collection.
	/// </summary>
	/// <remarks>  The resulting collection must be <c>Dispose()</c>ed of. Intended to be used
	///     with <c>using</c>: <c>using var copy = list.GetScopeDisposedCopy();</c>
	/// </remarks>
	public static ScopeDisposedList<T> GetScopeDisposedCopy<T>(this IReadOnlyList<T> sourceList)
	{
		return ScopeDisposedList<T>.GetFromPoolFilledWith(sourceList);
	}
}
