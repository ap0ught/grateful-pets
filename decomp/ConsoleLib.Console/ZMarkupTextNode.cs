using System;
using System.Collections.Generic;
using Cysharp.Text;
using XRL.World.Text;

namespace ConsoleLib.Console;

public class ZMarkupTextNode : IZMarkupNode
{
	private int _TextLength = -1;

	private int Start;

	public int Length;

	private static Queue<ZMarkupTextNode> markupTextNodePool = new Queue<ZMarkupTextNode>();

	public int GetTextLengthWithoutFormatting(ReadOnlySpan<char> Input)
	{
		if (_TextLength == -1)
		{
			_TextLength = ColorUtility.LengthExceptFormatting(Input.Slice(Start, Length));
		}
		return _TextLength;
	}

	public ReadOnlySpan<char> GetSpan(ReadOnlySpan<char> Input)
	{
		ReadOnlySpan<char> readOnlySpan = Input;
		int start = Start;
		return readOnlySpan.Slice(start, Start + Length - start);
	}

	public static ZMarkupTextNode next(ReadOnlySpan<char> text, int Start, int Length)
	{
		ZMarkupTextNode zMarkupTextNode;
		lock (markupTextNodePool)
		{
			zMarkupTextNode = ((markupTextNodePool.Count > 0) ? markupTextNodePool.Dequeue() : null) ?? new ZMarkupTextNode();
		}
		zMarkupTextNode.set(Start, Length);
		return zMarkupTextNode;
	}

	public override void release()
	{
		Start = 0;
		Length = 0;
		_TextLength = -1;
		lock (markupTextNodePool)
		{
			markupTextNodePool.Enqueue(this);
		}
	}

	public void set(int Start, int Length)
	{
		this.Start = Start;
		this.Length = Length;
	}

	public override void ToTextBuilder(TextBuilder SB, ReadOnlySpan<char> Source, bool RefreshAtNewline, ref char? lastForeground, ref char? lastBackground)
	{
		SB.Append(Source, Start, Length);
		ColorUtility.FindLastForegroundAndBackground(Source.Slice(Start, Length), ref lastForeground, ref lastBackground);
	}

	public override void ToTextBuilder(TextBuilder SB, ReadOnlySpan<char> Source, bool RefreshAtNewline)
	{
		SB.Append(Source, Start, Length);
	}

	public override string DebugDump(int depth = 0)
	{
		Utf16ValueStringBuilder SB = ZString.CreateStringBuilder();
		try
		{
			DebugDump(ref SB, depth);
			return SB.ToString();
		}
		finally
		{
			SB.Dispose();
		}
	}

	public override void DebugDump(ref Utf16ValueStringBuilder SB, int depth)
	{
		int i = 0;
		for (int num = depth * 2; i < num; i++)
		{
			SB.Append('_');
		}
		SB.Append("text: ");
		SB.Append(ToString().Replace("\n", "\\n"));
		SB.Append('\n');
	}
}
