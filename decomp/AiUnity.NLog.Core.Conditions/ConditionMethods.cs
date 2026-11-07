using System;

namespace AiUnity.NLog.Core.Conditions;

[ConditionMethods]
public static class ConditionMethods
{
	[ConditionMethod("equals")]
	public static bool Equals2(object firstValue, object secondValue)
	{
		return firstValue.Equals(secondValue);
	}

	[ConditionMethod("strequals")]
	public static bool Equals2(string firstValue, string secondValue, bool ignoreCase = false)
	{
		bool flag = ignoreCase;
		return firstValue.Equals(secondValue, flag ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	[ConditionMethod("contains")]
	public static bool Contains(string haystack, string needle, bool ignoreCase = true)
	{
		bool flag = ignoreCase;
		return haystack.IndexOf(needle, flag ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0;
	}

	[ConditionMethod("starts-with")]
	public static bool StartsWith(string haystack, string needle, bool ignoreCase = true)
	{
		bool flag = ignoreCase;
		return haystack.StartsWith(needle, flag ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	[ConditionMethod("ends-with")]
	public static bool EndsWith(string haystack, string needle, bool ignoreCase = true)
	{
		bool flag = ignoreCase;
		return haystack.EndsWith(needle, flag ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	[ConditionMethod("length")]
	public static int Length(string text)
	{
		return text.Length;
	}
}
