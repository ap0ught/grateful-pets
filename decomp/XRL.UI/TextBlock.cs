using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Cysharp.Text;
using Qud.UI;
using XRL.World.Text;

namespace XRL.UI;

public class TextBlock
{
	public List<string> Lines = new List<string>();

	private int MaxLines;

	private bool ReverseBlocks;

	public int AdjustFirstLine;

	public int _Width = -1;

	public int Width
	{
		get
		{
			if (_Width == -1)
			{
				foreach (string line in Lines)
				{
					if (line.Length > _Width)
					{
						_Width = line.Length;
					}
				}
			}
			return _Width;
		}
	}

	public int Height => Lines.Count;

	public static void ParseSpan(ReadOnlySpan<char> String, TextBuilder Output, int Width, int MaxLines = 35, int AdjustFirstLine = 0)
	{
		int num = -AdjustFirstLine;
		int num2 = 0;
		int num3 = 0;
		Output.Clear();
		char? Foreground = null;
		char? Background = null;
		int i = 0;
		bool flag = false;
		while (i < String.Length)
		{
			int num4 = String.IndexOfAnyStartFrom(i + 1, " \n");
			bool flag2 = num4 != -1;
			int num5 = ((num4 != -1) ? (num4 - i) : (String.Length - i));
			ReadOnlySpan<char> readOnlySpan = String.Slice(i, num5);
			int num6 = ColorUtility.LengthExceptFormatting(readOnlySpan);
			if (num6 + num + 1 > Width)
			{
				if (flag)
				{
					flag = false;
					num6--;
					num5--;
					i++;
					readOnlySpan = String.Slice(i, num5);
				}
				if (Output.Length != num3)
				{
					num2++;
					if (num2 >= MaxLines)
					{
						break;
					}
					Output.Append('\n');
					num3 = Output.Length;
				}
				if (i >= String.Length)
				{
					break;
				}
				num = 0;
				if (Foreground.HasValue)
				{
					Output.Append('&');
					Output.Append(Foreground);
				}
				if (Background.HasValue)
				{
					Output.Append('^');
					Output.Append(Background);
				}
			}
			num += num6;
			Output.Append(readOnlySpan);
			flag = flag2;
			i += num5;
			FindLastColorCodes(readOnlySpan, ref Foreground, ref Background);
			for (; i < String.Length && String[i] == '\n'; i++)
			{
				num2++;
				num = 0;
				if (num2 >= MaxLines)
				{
					break;
				}
				Output.Append('\n');
				num3 = Output.Length;
				if (Foreground.HasValue)
				{
					Output.Append('&');
					Output.Append(Foreground);
				}
				if (Background.HasValue)
				{
					Output.Append('^');
					Output.Append(Background);
				}
			}
		}
	}

	public void Format(string String)
	{
		if (String == null || String.Length == 0)
		{
			Lines.Clear();
		}
		else
		{
			ParseSpan(Markup.Transform(String));
		}
	}

	public void Format(ReadOnlySpan<char> String)
	{
		Lines.Clear();
		ParseSpan(Markup.Transform(new string(String)));
	}

	public void Format(StringBuilder stringBuilder)
	{
		Lines.Clear();
		if (stringBuilder != null)
		{
			ParseSpan(Markup.Transform(stringBuilder.ToString()));
		}
	}

	private void ParseSpan(ReadOnlySpan<char> String)
	{
		int num = -AdjustFirstLine;
		using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		char? Foreground = null;
		char? Background = null;
		int i = 0;
		_ = Width;
		bool flag = false;
		while (i < String.Length)
		{
			int num2 = String.IndexOfAnyStartFrom(i + 1, " \n");
			bool flag2 = num2 != -1;
			int num3 = ((num2 != -1) ? (num2 - i) : (String.Length - i));
			ReadOnlySpan<char> readOnlySpan = String.Slice(i, num3);
			int num4 = ColorUtility.LengthExceptFormatting(readOnlySpan);
			if (num4 + num + 1 > Width)
			{
				if (flag)
				{
					flag = false;
					num4--;
					num3--;
					i++;
					readOnlySpan = String.Slice(i, num3);
				}
				if (Lines.Count > 0 || utf16ValueStringBuilder.Length > 0)
				{
					Lines.Add(utf16ValueStringBuilder.ToString());
				}
				utf16ValueStringBuilder.Clear();
				num = 0;
				if (Lines.Count >= MaxLines || i >= String.Length)
				{
					break;
				}
				if (Foreground.HasValue)
				{
					utf16ValueStringBuilder.Append('&');
					utf16ValueStringBuilder.Append(Foreground);
				}
				if (Background.HasValue)
				{
					utf16ValueStringBuilder.Append('^');
					utf16ValueStringBuilder.Append(Background);
				}
			}
			num += num4;
			utf16ValueStringBuilder.Append(readOnlySpan);
			flag = flag2;
			i += num3;
			FindLastColorCodes(readOnlySpan, ref Foreground, ref Background);
			for (; i < String.Length && String[i] == '\n'; i++)
			{
				Lines.Add(utf16ValueStringBuilder.ToString());
				utf16ValueStringBuilder.Clear();
				num = 0;
				if (Lines.Count >= MaxLines)
				{
					break;
				}
				if (Foreground.HasValue)
				{
					utf16ValueStringBuilder.Append('&');
					utf16ValueStringBuilder.Append(Foreground);
				}
				if (Background.HasValue)
				{
					utf16ValueStringBuilder.Append('^');
					utf16ValueStringBuilder.Append(Background);
				}
			}
		}
		if (utf16ValueStringBuilder.Length > 0)
		{
			Lines.Add(utf16ValueStringBuilder.ToString());
		}
		if (ReverseBlocks)
		{
			Lines.Reverse();
		}
	}

	private static void FindLastColorCodes(ReadOnlySpan<char> span, ref char? Foreground, ref char? Background)
	{
		for (int i = 0; i < span.Length - 1; i++)
		{
			if (span[i] == '&')
			{
				if (span[i + 1] == '&')
				{
					i++;
					continue;
				}
				Foreground = span[i + 1];
			}
			if (span[i] == '^')
			{
				if (span[i + 1] == '^')
				{
					i++;
				}
				else
				{
					Background = span[i + 1];
				}
			}
		}
	}

	public TextBlock(string String, int Width, int MaxLines, bool ReverseBlocks = false, int AdjustFirstLine = 0)
	{
		_Width = Width;
		this.MaxLines = MaxLines;
		this.ReverseBlocks = ReverseBlocks;
		this.AdjustFirstLine = AdjustFirstLine;
		Format(String);
	}

	public TextBlock(ReadOnlySpan<char> Span, int Width, int MaxLines, bool ReverseBlocks = false, int AdjustFirstLine = 0)
	{
		_Width = Width;
		this.MaxLines = MaxLines;
		this.ReverseBlocks = ReverseBlocks;
		this.AdjustFirstLine = AdjustFirstLine;
		Format(Span);
	}

	public TextBlock(StringBuilder String, int Width, int MaxLines, bool ReverseBlocks = false, int AdjustFirstLine = 0)
	{
		_Width = Width;
		this.MaxLines = MaxLines;
		this.ReverseBlocks = ReverseBlocks;
		this.AdjustFirstLine = AdjustFirstLine;
		Format(String);
	}

	public override string ToString()
	{
		using TextBuilder textBuilder = TextBuilder.Get();
		for (int i = 0; i < Lines.Count; i++)
		{
			if (i > 0)
			{
				textBuilder.Append('\n');
			}
			textBuilder.Append(Lines[i]);
		}
		return textBuilder.ToString();
	}

	public string GetRTFBlock()
	{
		using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		for (int i = 0; i < Lines.Count; i++)
		{
			if (i > 0)
			{
				utf16ValueStringBuilder.Append('\n');
			}
			utf16ValueStringBuilder.Append(Lines[i]);
		}
		return RTF.FormatToRTF(utf16ValueStringBuilder.ToString());
	}
}
