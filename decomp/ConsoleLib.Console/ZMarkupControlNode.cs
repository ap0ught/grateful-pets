using System;
using System.Collections.Generic;
using Cysharp.Text;
using XRL;
using XRL.World.Text;

namespace ConsoleLib.Console;

public class ZMarkupControlNode : IZMarkupNode
{
	private sealed class Destructor
	{
		~Destructor()
		{
			_markupBuilderSB.Dispose(Destroy: true);
		}
	}

	public string Action;

	public List<IZMarkupNode> Children = new List<IZMarkupNode>(64);

	private static Queue<ZMarkupControlNode> markupControlNodePool = new Queue<ZMarkupControlNode>();

	private static readonly Destructor Finalise = new Destructor();

	private static TextBuilder _markupBuilderSB = new CompositedTextBuilder(2048);

	private static HashSet<string> WarnedAbout = new HashSet<string>();

	public ZMarkupControlNode()
	{
	}

	public static ZMarkupControlNode next()
	{
		lock (markupControlNodePool)
		{
			return ((markupControlNodePool.Count > 0) ? markupControlNodePool.Dequeue() : null) ?? new ZMarkupControlNode();
		}
	}

	public static ZMarkupControlNode next(ReadOnlySpan<char> text)
	{
		ZMarkupControlNode zMarkupControlNode = next();
		zMarkupControlNode.set(text);
		return zMarkupControlNode;
	}

	public override void release()
	{
		foreach (IZMarkupNode child in Children)
		{
			child.release();
		}
		Action = null;
		Children.Clear();
		lock (markupControlNodePool)
		{
			markupControlNodePool.Enqueue(this);
		}
	}

	public void set(ReadOnlySpan<char> Text)
	{
		Action = Text.ToString();
	}

	public ZMarkupControlNode(string Action)
	{
		set(Action);
		this.Action = Action;
	}

	public ZMarkupControlNode(string Action, List<IZMarkupNode> Children)
		: this(Action)
	{
		this.Children = Children;
	}

	public void Add(ReadOnlySpan<char> text, int Start, int Length)
	{
		Children.Add(ZMarkupTextNode.next(text, Start, Length));
	}

	public void Add(ZMarkupControlNode node)
	{
		Children.Add(node);
	}

	public void ClearChildText()
	{
		int i = 0;
		for (int count = Children.Count; i < count; i++)
		{
			if (Children[i] is ZMarkupTextNode zMarkupTextNode)
			{
				zMarkupTextNode.Length = 0;
			}
			if (Children[i] is ZMarkupControlNode zMarkupControlNode)
			{
				zMarkupControlNode.ClearChildText();
			}
		}
	}

	private void AssembleCode(TextBuilder SB, char? foreground = null, char? background = null)
	{
		if (foreground.HasValue)
		{
			SB.Append('&').Append(foreground);
		}
		if (background.HasValue)
		{
			SB.Append('^').Append(background);
		}
	}

	private string AssembleCode(char? foreground = null, char? background = null)
	{
		lock (_markupBuilderSB)
		{
			_markupBuilderSB.Length = 0;
			AssembleCode(_markupBuilderSB, foreground, background);
			return _markupBuilderSB.ToString();
		}
	}

	public override void ToTextBuilder(TextBuilder SB, ReadOnlySpan<char> Source, bool RefreshAtNewline, ref char? lastForeground, ref char? lastBackground)
	{
		IMarkupShader markupShader = null;
		if (!WarnedAbout.Contains(Action))
		{
			try
			{
				if (!string.IsNullOrEmpty(Action))
				{
					if (Action == "?KB")
					{
						if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
						{
							ClearChildText();
						}
						markupShader = MarkupShaders.Find("y");
					}
					else if (Action == "?Gamepad")
					{
						if (ControlManager.activeControllerType != ControlManager.InputDeviceType.Gamepad)
						{
							ClearChildText();
						}
						markupShader = MarkupShaders.Find("y");
					}
					else
					{
						markupShader = MarkupShaders.Find(Action);
					}
				}
			}
			catch (Exception innerException)
			{
				MetricsManager.LogError(new Exception("Error finding Markup Shader \"" + Action + "\"", innerException));
				WarnedAbout.Add(Action);
			}
		}
		int num = 0;
		int num2 = 0;
		if (markupShader != null)
		{
			int i = 0;
			for (int count = Children.Count; i < count; i++)
			{
				if (Children[i] is ZMarkupTextNode zMarkupTextNode)
				{
					num2 += zMarkupTextNode.GetTextLengthWithoutFormatting(Source);
				}
			}
		}
		bool flag = false;
		char? c = lastForeground;
		char? c2 = lastBackground;
		int j = 0;
		for (int count2 = Children.Count; j < count2; j++)
		{
			if (Children[j] is ZMarkupTextNode zMarkupTextNode2)
			{
				if (flag)
				{
					if (markupShader == null && !string.IsNullOrEmpty(Action))
					{
						SB.Append(Action);
					}
					flag = false;
				}
				if (markupShader == null)
				{
					zMarkupTextNode2.ToTextBuilder(SB, Source, RefreshAtNewline, ref lastForeground, ref lastBackground);
					c = lastForeground;
					c2 = lastBackground;
					continue;
				}
				int num3 = 0;
				int textLengthWithoutFormatting = zMarkupTextNode2.GetTextLengthWithoutFormatting(Source);
				ReadOnlySpan<char> span = zMarkupTextNode2.GetSpan(Source);
				char? value = null;
				int Index = 0;
				int length = zMarkupTextNode2.Length;
				while (Index < length)
				{
					ReadOnlySpan<char> nextPrintableSpan = GraphemeSplitter.GetNextPrintableSpan(span, ref Index);
					if (nextPrintableSpan.IsEmpty)
					{
						continue;
					}
					if (nextPrintableSpan[0] == '&' && Index < length)
					{
						nextPrintableSpan = GraphemeSplitter.GetNextPrintableSpan(span, ref Index);
						if (nextPrintableSpan[0] != '&')
						{
							SB.Append('&').Append(nextPrintableSpan);
							lastForeground = nextPrintableSpan[0];
							if (num3 == 0)
							{
								c = markupShader.GetForegroundColor(nextPrintableSpan[0], num3, textLengthWithoutFormatting, num, num2);
							}
							continue;
						}
						value = '&';
					}
					else if (nextPrintableSpan[0] == '^' && Index < length)
					{
						nextPrintableSpan = GraphemeSplitter.GetNextPrintableSpan(span, ref Index);
						if (nextPrintableSpan[0] != '^')
						{
							SB.Append('^').Append(nextPrintableSpan);
							lastBackground = nextPrintableSpan[0];
							if (num3 == 0)
							{
								c2 = markupShader.GetBackgroundColor(nextPrintableSpan[0], num3, textLengthWithoutFormatting, num, num2);
							}
							continue;
						}
						value = '^';
					}
					char? foregroundColor = markupShader.GetForegroundColor(nextPrintableSpan[0], num3, textLengthWithoutFormatting, num, num2);
					char? backgroundColor = markupShader.GetBackgroundColor(nextPrintableSpan[0], num3, textLengthWithoutFormatting, num, num2);
					if (foregroundColor.HasValue && foregroundColor != c)
					{
						if (backgroundColor.HasValue && backgroundColor != c2)
						{
							AssembleCode(SB, foregroundColor, backgroundColor);
							c = (lastForeground = foregroundColor);
							c2 = (lastBackground = backgroundColor);
						}
						else
						{
							AssembleCode(SB, foregroundColor);
							c = (lastForeground = foregroundColor);
						}
					}
					else if (backgroundColor.HasValue && backgroundColor != lastBackground)
					{
						char? background = backgroundColor;
						AssembleCode(SB, null, background);
						c2 = (lastBackground = backgroundColor);
					}
					SB.Append(nextPrintableSpan);
					if (value.HasValue)
					{
						SB.Append(value);
						value = null;
					}
					if (RefreshAtNewline && nextPrintableSpan[0] == '\n')
					{
						AssembleCode(SB, (foregroundColor == 'y') ? ((char?)null) : foregroundColor, (backgroundColor == 'k') ? ((char?)null) : backgroundColor);
					}
					num3++;
					num++;
				}
				continue;
			}
			flag = Children[j] is ZMarkupControlNode;
			Children[j].ToTextBuilder(SB, Source, RefreshAtNewline, ref lastForeground, ref lastBackground);
			if (markupShader == null && string.IsNullOrEmpty(Action))
			{
				if (c != lastForeground)
				{
					SB.Append('&').Append(c ?? 'y');
					lastForeground = c;
				}
				if (c2 != lastBackground)
				{
					SB.Append('^').Append(c2 ?? 'k');
					lastBackground = c2;
				}
			}
			else
			{
				c = lastForeground;
				c2 = lastBackground;
			}
		}
	}

	public override void ToTextBuilder(TextBuilder SB, ReadOnlySpan<char> Source, bool RefreshAtNewline)
	{
		char? lastForeground = null;
		char? lastBackground = 'k';
		ToTextBuilder(SB, Source, RefreshAtNewline, ref lastForeground, ref lastBackground);
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
		SB.Append("control node, ");
		SB.Append(Children.Count.Things("child", "children"));
		SB.Append(":");
		SB.Append((Action == null) ? "null" : Action.Replace("&", "&&").Replace("^", "^^"));
		SB.Append('\n');
		int j = 0;
		for (int count = Children.Count; j < count; j++)
		{
			Children[j].DebugDump(ref SB, depth + 1);
		}
	}
}
