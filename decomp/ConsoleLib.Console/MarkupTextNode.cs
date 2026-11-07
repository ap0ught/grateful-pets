using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Text;

namespace ConsoleLib.Console;

public class MarkupTextNode : IMarkupNode
{
	private string _Text;

	private int _TextLength = -1;

	private static Queue<MarkupTextNode> markupTextNodePool = new Queue<MarkupTextNode>();

	public string Text
	{
		get
		{
			return _Text;
		}
		set
		{
			_Text = value;
			_TextLength = -1;
		}
	}

	public int TextLength
	{
		get
		{
			if (_TextLength == -1)
			{
				_TextLength = ColorUtility.LengthExceptFormatting(_Text);
			}
			return _TextLength;
		}
	}

	public static MarkupTextNode next(ReadOnlySpan<char> text)
	{
		MarkupTextNode markupTextNode;
		lock (markupTextNodePool)
		{
			markupTextNode = ((markupTextNodePool.Count > 0) ? markupTextNodePool.Dequeue() : null) ?? new MarkupTextNode();
		}
		markupTextNode.set(text);
		return markupTextNode;
	}

	public override void release()
	{
		_Text = null;
		_TextLength = -1;
		lock (markupTextNodePool)
		{
			markupTextNodePool.Enqueue(this);
		}
	}

	public void set(ReadOnlySpan<char> text)
	{
		Text = text.ToString();
	}

	public override string ToString()
	{
		return ToString(RefreshAtNewline: false);
	}

	public override string ToString(bool RefreshAtNewline)
	{
		return Text;
	}

	public override void ToStringBuilder(StringBuilder SB, bool RefreshAtNewline, ref char? lastForeground, ref char? lastBackground)
	{
		SB.Append(Text);
		ColorUtility.FindLastForegroundAndBackground(Text, ref lastForeground, ref lastBackground);
	}

	public override void ToStringBuilder(StringBuilder SB, bool RefreshAtNewline)
	{
		SB.Append(Text);
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
		SB.Append(Text.Replace("\n", "\\n"));
		SB.Append('\n');
	}
}
