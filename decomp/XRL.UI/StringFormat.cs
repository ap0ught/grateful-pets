using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;

namespace XRL.UI;

public class StringFormat
{
	public static int GetNextWordLength(string Input, ref int Position, ref bool Newline, ref bool inControl, ref int controlDepth, ref int numControls)
	{
		int num = 0;
		Newline = false;
		while (Position < Input.Length)
		{
			char c = Input[Position];
			if (inControl)
			{
				if (c == '|')
				{
					inControl = false;
				}
				Position++;
				continue;
			}
			if (Position < Input.Length - 1)
			{
				if (c == '{' && Input[Position + 1] == '{')
				{
					inControl = true;
					controlDepth++;
					numControls++;
					Position += 2;
					continue;
				}
				if (controlDepth > 0 && c == '}' && Input[Position + 1] == '}')
				{
					controlDepth--;
					Position += 2;
					continue;
				}
			}
			if (c == ' ')
			{
				break;
			}
			if (c == '\r')
			{
				Position++;
				if (Position < Input.Length && Input[Position] == '\n')
				{
					Newline = true;
					break;
				}
				num++;
				continue;
			}
			if (c == '\n')
			{
				Newline = true;
				break;
			}
			if (c == '&' || c == '^')
			{
				Position++;
				if (Position >= Input.Length || Input[Position] == c)
				{
					num++;
				}
			}
			else
			{
				num++;
			}
			Position++;
		}
		return num;
	}

	public static int GetNextWordLength(StringBuilder Input, ref int Position, ref bool Newline, ref bool inControl, ref int controlDepth, ref int numControls)
	{
		int num = 0;
		Newline = false;
		while (Position < Input.Length)
		{
			char c = Input[Position];
			if (inControl)
			{
				if (c == '|')
				{
					inControl = false;
				}
				Position++;
				continue;
			}
			if (Position < Input.Length - 1)
			{
				if (c == '{' && Input[Position + 1] == '{')
				{
					inControl = true;
					controlDepth++;
					numControls++;
					Position += 2;
					continue;
				}
				if (controlDepth > 0 && c == '}' && Input[Position + 1] == '}')
				{
					controlDepth--;
					Position += 2;
					continue;
				}
			}
			if (c == ' ')
			{
				break;
			}
			if (c == '\n')
			{
				Newline = true;
				break;
			}
			if (c == '&' || c == '^')
			{
				Position++;
				if (Position >= Input.Length || Input[Position] == c)
				{
					num++;
				}
			}
			else
			{
				num++;
			}
			Position++;
		}
		return num;
	}

	public static string SubstringNotCountingFormat(string Input, int Start, int Length)
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		int num3 = 0;
		while (num2 < Start && num < Input.Length)
		{
			char c = Input[num];
			if (flag)
			{
				if (c == '|')
				{
					flag = false;
				}
				num++;
				continue;
			}
			if (num < Input.Length - 1)
			{
				if (c == '{' && Input[num + 1] == '{')
				{
					flag = true;
					num3++;
					num += 2;
					continue;
				}
				if (num3 > 0 && c == '}' && Input[num + 1] == '}')
				{
					num3--;
					num += 2;
					continue;
				}
			}
			if (c == '&' || c == '^')
			{
				num++;
				if (num < Input.Length && Input[num] != c)
				{
					num++;
					continue;
				}
			}
			num2++;
			num++;
		}
		int startIndex = num;
		num2 = 0;
		while (num2 < Length && num < Input.Length)
		{
			char c2 = Input[num];
			if (flag)
			{
				if (c2 == '|')
				{
					flag = false;
				}
				num++;
				continue;
			}
			if (num < Input.Length - 1)
			{
				if (c2 == '{' && Input[num + 1] == '{')
				{
					flag = true;
					num3++;
					num += 2;
					continue;
				}
				if (num3 > 0 && c2 == '}' && Input[num + 1] == '}')
				{
					num3--;
					num += 2;
					continue;
				}
			}
			if (c2 == '&' || c2 == '^')
			{
				num++;
				if (num < Input.Length && Input[num] != c2)
				{
					num++;
					continue;
				}
			}
			num2++;
			num++;
		}
		int length = num;
		return Input.Substring(startIndex, length);
	}

	public static List<string> ClipTextToArray(string Input, int MaxWidth, out int MaxClippedWidth, bool KeepNewlines = false, bool KeepColorsAcrossNewlines = true, bool TransformMarkup = true, bool TransformMarkupIfMultipleLines = false)
	{
		StringBuilder stringBuilder = new StringBuilder((MaxWidth == int.MaxValue) ? 256 : MaxWidth);
		List<string> list = new List<string>(64);
		MaxClippedWidth = 0;
		int num = 0;
		int num2 = 0;
		int i = 0;
		bool inControl = false;
		int controlDepth = 0;
		int numControls = 0;
		for (; i < Input.Length; i++)
		{
			int num3 = i;
			bool Newline = false;
			int nextWordLength = GetNextWordLength(Input, ref i, ref Newline, ref inControl, ref controlDepth, ref numControls);
			if (nextWordLength + num + ((num2 > 0) ? 1 : 0) > MaxWidth)
			{
				if (stringBuilder.Length > 0)
				{
					list.Add(stringBuilder.ToString());
				}
				if (num > MaxClippedWidth)
				{
					MaxClippedWidth = num;
				}
				stringBuilder.Clear().Append(Input.Substring(num3, i - num3));
				if (Newline & KeepNewlines)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Clear();
					if (num > MaxClippedWidth)
					{
						MaxClippedWidth = num;
					}
					num = 0;
					num2 = 0;
				}
				else
				{
					num = nextWordLength + 4;
					num2 = 1;
				}
				continue;
			}
			if (num2 > 0)
			{
				stringBuilder.Append(' ');
				num++;
			}
			if (i > num3)
			{
				stringBuilder.Append(Input.Substring(num3, i - num3));
			}
			if (KeepNewlines & Newline)
			{
				list.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
				num += nextWordLength;
				if (num > MaxClippedWidth)
				{
					MaxClippedWidth = num;
				}
				num = 0;
				num2 = 0;
			}
			else if (nextWordLength > 0)
			{
				num += nextWordLength;
				num2++;
			}
		}
		for (int j = 0; j < controlDepth; j++)
		{
			stringBuilder.Append("}}");
		}
		if (stringBuilder.Length > 0)
		{
			list.Add(stringBuilder.ToString());
		}
		if (num > MaxClippedWidth)
		{
			MaxClippedWidth = num;
		}
		if ((TransformMarkup || (TransformMarkupIfMultipleLines && list.Count > 1)) && numControls > 0)
		{
			list = Markup.Transform(list);
		}
		if (KeepColorsAcrossNewlines)
		{
			char? c = null;
			char? c2 = null;
			for (int k = 0; k < list.Count; k++)
			{
				if (c.HasValue)
				{
					List<string> list2 = list;
					int index = k;
					char? c3 = c;
					list2[index] = "&" + c3 + list[k];
				}
				if (c2.HasValue)
				{
					List<string> list3 = list;
					int index2 = k;
					char? c3 = c2;
					list3[index2] = "&" + c3 + list[k];
				}
				for (int l = 0; l < list[k].Length - 1; l++)
				{
					if (list[k][l] == '&')
					{
						l++;
						if (list[k][l] != '&')
						{
							c = list[k][l];
						}
					}
					if (list[k][l] == '^')
					{
						l++;
						if (list[k][l] != '^')
						{
							c2 = list[k][l];
						}
					}
				}
			}
		}
		return list;
	}

	public static List<string> ClipTextToArray(string Input, int MaxWidth, bool KeepNewlines = false, bool KeepColorsAcrossNewlines = true)
	{
		int MaxClippedWidth;
		return ClipTextToArray(Input, MaxWidth, out MaxClippedWidth, KeepNewlines, KeepColorsAcrossNewlines);
	}

	public static string ClipLine(string Input, int MaxWidth, bool AddEllipsis = true, StringBuilder Return = null)
	{
		bool flag = Return != null;
		if (Return == null)
		{
			Return = new StringBuilder();
		}
		ClipLine(new StringBuilder(Input), MaxWidth, AddEllipsis, Return);
		if (!flag)
		{
			return Return.ToString();
		}
		return null;
	}

	public static string ClipLine(StringBuilder Input, int MaxWidth, bool AddEllipsis = true, StringBuilder Return = null)
	{
		bool flag = Return != null;
		if (Return == null)
		{
			Return = new StringBuilder();
		}
		Return.Length = 0;
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int num2 = 0;
		int i = 0;
		bool inControl = false;
		int num3 = 0;
		int numControls = 0;
		for (; i < Input.Length; i++)
		{
			int num4 = i;
			bool Newline = false;
			int controlDepth = num3;
			int nextWordLength = GetNextWordLength(Input, ref i, ref Newline, ref inControl, ref controlDepth, ref numControls);
			if (nextWordLength + num + ((num2 > 0) ? 1 : 0) > MaxWidth)
			{
				if (!AddEllipsis)
				{
					break;
				}
				if (nextWordLength + 3 <= MaxWidth)
				{
					for (int j = 0; j < num3; j++)
					{
						Return.Append("}}");
					}
					Return.Append("&y...");
					return Return.ToString();
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				Return.Substring(0, Return.Length - 3, stringBuilder2);
				for (int k = 0; k < num3; k++)
				{
					stringBuilder2.Append("}}");
				}
				stringBuilder2.Append("&y...");
			}
			else
			{
				num3 = controlDepth;
				if (num2 > 0)
				{
					Return.Append(' ');
					num++;
				}
				if (i > num4)
				{
					stringBuilder.Length = 0;
					Input.Substring(num4, i - num4, stringBuilder);
					Return.Append(stringBuilder);
				}
				if (Newline)
				{
					Return.Append('\n');
					num = 0;
					num2 = 0;
				}
				else
				{
					num += nextWordLength;
					num2++;
				}
			}
		}
		for (int l = 0; l < num3; l++)
		{
			Return.Append("}}");
		}
		if (!flag)
		{
			return Return.ToString();
		}
		return null;
	}

	public static string ClipText(string Input, int MaxWidth, bool KeepNewlines = false, bool TransformMarkup = true, bool TransformMarkupIfMultipleLines = false)
	{
		StringBuilder stringBuilder = new StringBuilder(Input.Length);
		int num = 0;
		int num2 = 0;
		int i = 0;
		bool inControl = false;
		int controlDepth = 0;
		int numControls = 0;
		int num3 = 1;
		for (; i < Input.Length; i++)
		{
			int num4 = i;
			bool Newline = false;
			int nextWordLength = GetNextWordLength(Input, ref i, ref Newline, ref inControl, ref controlDepth, ref numControls);
			if (nextWordLength + num + ((num2 > 0) ? 1 : 0) > MaxWidth)
			{
				stringBuilder.Append('\n');
				num3++;
				stringBuilder.Append(Input.Substring(num4, i - num4));
				if (Newline & KeepNewlines)
				{
					stringBuilder.Append('\n');
					num3++;
					num = 0;
				}
				else
				{
					num = nextWordLength + 4;
				}
				continue;
			}
			if (num2 > 0)
			{
				stringBuilder.Append(' ');
				num++;
			}
			if (i > num4)
			{
				stringBuilder.Append(Input.Substring(num4, i - num4));
			}
			if (KeepNewlines & Newline)
			{
				stringBuilder.Append('\n');
				num3++;
				num = 0;
				num2 = 0;
			}
			else
			{
				num += nextWordLength;
				num2++;
			}
		}
		if ((TransformMarkup || (TransformMarkupIfMultipleLines && num3 > 1)) && numControls > 0)
		{
			Markup.Transform(stringBuilder);
		}
		return stringBuilder.ToString();
	}
}
