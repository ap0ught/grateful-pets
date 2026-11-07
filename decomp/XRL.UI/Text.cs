using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;

namespace XRL.UI;

public class Text
{
	public static void Draw(ScreenBuffer _ScreenBuffer, string str, int x1, int y1, int x2, int y2)
	{
		int width = x2 - x1 + 1;
		int maxLines = y2 - y1 + 1;
		List<string> lines = new TextBlock(str, width, maxLines).Lines;
		for (int i = y1; i <= y2 && i - y1 < lines.Count; i++)
		{
			_ScreenBuffer.Goto(x1, i);
			_ScreenBuffer.Write(lines[i - y1]);
		}
	}

	public static List<string> DrawBottomToTop(ScreenBuffer _ScreenBuffer, StringBuilder str, int x1, int y1, int x2, int y2)
	{
		int width = x2 - x1 + 1;
		int maxLines = y2 - y1 + 1;
		List<string> lines = new TextBlock(str, width, maxLines, ReverseBlocks: true).Lines;
		int num = 0;
		int num2 = y2;
		while (num2 >= y1 && num < lines.Count)
		{
			_ScreenBuffer.Goto(x1, num2);
			_ScreenBuffer.Write(lines[num]);
			num2--;
			num++;
		}
		return lines;
	}

	public static List<string> DrawBottomToTop(ScreenBuffer _ScreenBuffer, ReadOnlySpan<char> str, int x1, int y1, int x2, int y2)
	{
		int width = x2 - x1 + 1;
		int maxLines = y2 - y1 + 1;
		List<string> lines = new TextBlock(str, width, maxLines, ReverseBlocks: true).Lines;
		int num = 0;
		int num2 = y2;
		while (num2 >= y1 && num < lines.Count)
		{
			_ScreenBuffer.Goto(x1, num2);
			_ScreenBuffer.Write(lines[num]);
			num2--;
			num++;
		}
		return lines;
	}

	public static List<string> DrawBottomToTop(ScreenBuffer _ScreenBuffer, string str, int x1, int y1, int x2, int y2)
	{
		int width = x2 - x1 + 1;
		int maxLines = y2 - y1 + 1;
		List<string> lines = new TextBlock(str, width, maxLines, ReverseBlocks: true).Lines;
		int num = 0;
		int num2 = y2;
		while (num2 >= y1 && num < lines.Count)
		{
			_ScreenBuffer.Goto(x1, num2);
			_ScreenBuffer.Write(lines[num]);
			num2--;
			num++;
		}
		return lines;
	}

	public static void DrawBottomToTop(ScreenBuffer _ScreenBuffer, List<string> Lines, int x1, int y1, int x2, int y2)
	{
		int num = 0;
		int num2 = y2;
		while (num2 >= y1 && num < Lines.Count)
		{
			_ScreenBuffer.Goto(x1, num2);
			_ScreenBuffer.Write(Lines[num]);
			num2--;
			num++;
		}
	}
}
