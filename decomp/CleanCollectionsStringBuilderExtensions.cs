using System;
using System.Text;

public static class CleanCollectionsStringBuilderExtensions
{
	public static bool Contains(this StringBuilder haystack, char needle)
	{
		return haystack.IndexOf(needle) != -1;
	}

	public static int IndexOf(this StringBuilder haystack, char needle, int startIndex = 0)
	{
		if (haystack == null)
		{
			throw new ArgumentNullException();
		}
		for (int i = startIndex; i < haystack.Length; i++)
		{
			if (haystack[i] == needle)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool Contains(this StringBuilder haystack, string needle)
	{
		return haystack.IndexOf(needle) != -1;
	}

	public static int IndexOf(this StringBuilder haystack, string needle)
	{
		if (haystack == null || needle == null)
		{
			throw new ArgumentNullException();
		}
		if (needle.Length == 0)
		{
			return 0;
		}
		int num = haystack.IndexOf(needle[0]);
		if (num == -1 || needle.Length == 1)
		{
			return num;
		}
		int num2 = 0;
		int num3 = 0;
		int[] array = KMPTable(needle);
		while (num2 + num3 < haystack.Length)
		{
			if (needle[num3] == haystack[num2 + num3])
			{
				if (num3 == needle.Length - 1)
				{
					return num2;
				}
				num3++;
			}
			else
			{
				num2 = num2 + num3 - array[num3];
				num3 = ((array[num3] > -1) ? array[num3] : 0);
			}
		}
		return -1;
	}

	private static int[] KMPTable(string sought)
	{
		int[] array = new int[sought.Length];
		int num = 2;
		int num2 = 0;
		array[0] = -1;
		array[1] = 0;
		while (num < array.Length)
		{
			if (sought[num - 1] == sought[num2])
			{
				num2 = (array[num++] = num2 + 1);
			}
			else if (num2 > 0)
			{
				num2 = array[num2];
			}
			else
			{
				array[num++] = 0;
			}
		}
		return array;
	}

	public static bool CleanContainsStartFrom(this StringBuilder builder, int Start, char C)
	{
		for (int i = Start; i < builder.Length; i++)
		{
			if (builder[i] == C)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CleanContainsAnyStartFrom(this StringBuilder builder, int Start, char[] C)
	{
		for (int i = Start; i < builder.Length; i++)
		{
			for (int j = 0; j < C.Length; j++)
			{
				if (builder[i] == C[j])
				{
					return true;
				}
			}
		}
		return false;
	}

	public static int CleanContainsStartFrom(this StringBuilder builder, int Start, char[] C)
	{
		for (int i = Start; i < builder.Length; i++)
		{
			for (int j = 0; j < C.Length; j++)
			{
				if (builder[i] == C[j])
				{
					return i;
				}
			}
		}
		return -1;
	}

	public static void Substring(this StringBuilder builder, int Start, StringBuilder Output)
	{
		Output.Length = 0;
		for (int i = Start; i < builder.Length; i++)
		{
			Output.Append(builder[i]);
		}
	}

	public static void Substring(this StringBuilder builder, int Start, int Length, StringBuilder Output)
	{
		Output.Length = 0;
		for (int i = Start; i < builder.Length && i < Start + Length; i++)
		{
			Output.Append(builder[i]);
		}
	}
}
