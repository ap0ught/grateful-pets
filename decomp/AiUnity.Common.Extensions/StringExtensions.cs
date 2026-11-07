namespace AiUnity.Common.Extensions;

public static class StringExtensions
{
	public static string After(this string value, char a, bool last = true)
	{
		int num = (last ? value.LastIndexOf(a) : value.IndexOf(a));
		return value.Substring(num + 1);
	}

	public static string After(this string value, string a, bool last = true)
	{
		int num = (last ? value.LastIndexOf(a) : value.IndexOf(a));
		return value.Substring(num + 1);
	}

	public static string Before(this string value, char a, bool last = true)
	{
		int num = (last ? value.LastIndexOf(a) : value.IndexOf(a));
		if (num != -1)
		{
			return value.Remove(num);
		}
		return string.Empty;
	}

	public static string Before(this string value, string a, bool last = true)
	{
		int num = (last ? value.LastIndexOf(a) : value.IndexOf(a));
		if (num != -1)
		{
			return value.Remove(num);
		}
		return string.Empty;
	}

	public static string TrimEnd(this string source, string value)
	{
		if (!source.EndsWith(value))
		{
			return source;
		}
		return source.Remove(source.LastIndexOf(value));
	}

	public static string LowercaseLetter(this string s, int index = 0)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		char[] array = s.ToCharArray();
		array[index] = char.ToLower(array[index]);
		return new string(array);
	}

	public static string UppercaseLetter(this string s, int index = 0)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		char[] array = s.ToCharArray();
		array[index] = char.ToUpper(array[index]);
		return new string(array);
	}
}
