using System.Text;

namespace AiUnity.NLog.Core.Internal;

internal class UrlHelper
{
	private static string safeUrlPunctuation = ".()*-_!'";

	private static string hexChars = "0123456789abcdef";

	internal static string UrlEncode(string str, bool spaceAsPlus)
	{
		StringBuilder stringBuilder = new StringBuilder(str.Length + 20);
		foreach (char c in str)
		{
			if ((c == ' ') & spaceAsPlus)
			{
				stringBuilder.Append('+');
				continue;
			}
			if (IsSafeUrlCharacter(c))
			{
				stringBuilder.Append(c);
				continue;
			}
			if (c < 'Ā')
			{
				stringBuilder.Append('%');
				stringBuilder.Append(hexChars[((int)c >> 4) & 0xF]);
				stringBuilder.Append(hexChars[c & 0xF]);
				continue;
			}
			stringBuilder.Append('%');
			stringBuilder.Append('u');
			stringBuilder.Append(hexChars[((int)c >> 12) & 0xF]);
			stringBuilder.Append(hexChars[((int)c >> 8) & 0xF]);
			stringBuilder.Append(hexChars[((int)c >> 4) & 0xF]);
			stringBuilder.Append(hexChars[c & 0xF]);
		}
		return stringBuilder.ToString();
	}

	private static bool IsSafeUrlCharacter(char ch)
	{
		if (ch >= 'a' && ch <= 'z')
		{
			return true;
		}
		if (ch >= 'A' && ch <= 'Z')
		{
			return true;
		}
		if (ch >= '0' && ch <= '9')
		{
			return true;
		}
		return safeUrlPunctuation.IndexOf(ch) >= 0;
	}
}
