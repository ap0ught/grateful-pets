using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Text;
using XRL;

namespace ConsoleLib.Console;

public class MarkupControlNode : IMarkupNode
{
	public string Action;

	public List<IMarkupNode> Children = new List<IMarkupNode>(64);

	private static Queue<MarkupControlNode> markupControlNodePool = new Queue<MarkupControlNode>();

	[NonSerialized]
	public static Dictionary<char?, string> _charToString = new Dictionary<char?, string>();

	private static StringBuilder _markupBuilderSB = new StringBuilder(2048);

	private static HashSet<string> WarnedAbout = new HashSet<string>();

	public MarkupControlNode()
	{
	}

	public static MarkupControlNode next()
	{
		lock (markupControlNodePool)
		{
			return ((markupControlNodePool.Count > 0) ? markupControlNodePool.Dequeue() : null) ?? new MarkupControlNode();
		}
	}

	public static MarkupControlNode next(ReadOnlySpan<char> text)
	{
		MarkupControlNode markupControlNode = next();
		markupControlNode.set(text);
		return markupControlNode;
	}

	public override void release()
	{
		foreach (IMarkupNode child in Children)
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

	public MarkupControlNode(string Action)
	{
		set(Action);
		this.Action = Action;
	}

	public MarkupControlNode(string Action, List<IMarkupNode> Children)
		: this(Action)
	{
		this.Children = Children;
	}

	public void Add(ReadOnlySpan<char> text)
	{
		Children.Add(MarkupTextNode.next(text));
	}

	public void Add(MarkupControlNode node)
	{
		Children.Add(node);
	}

	public void ClearChildText()
	{
		int i = 0;
		for (int count = Children.Count; i < count; i++)
		{
			if (Children[i] is MarkupTextNode markupTextNode)
			{
				markupTextNode.Text = "";
			}
			if (Children[i] is MarkupControlNode markupControlNode)
			{
				markupControlNode.ClearChildText();
			}
		}
	}

	public static string charToString(char? c)
	{
		if (!c.HasValue)
		{
			return null;
		}
		string value = null;
		if (_charToString.TryGetValue(c, out value))
		{
			return value;
		}
		string text = c.ToString();
		_charToString.Add(c, text);
		return text;
	}

	private void AssembleCode(StringBuilder SB, char? foreground = null, char? background = null)
	{
		if (foreground.HasValue)
		{
			SB.Append(charToString('&')).Append(charToString(foreground));
		}
		if (background.HasValue)
		{
			SB.Append(charToString('^')).Append(charToString(background));
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

	public override string ToString()
	{
		return ToString(RefreshAtNewline: false);
	}

	public override string ToString(bool RefreshAtNewline)
	{
		lock (_markupBuilderSB)
		{
			_markupBuilderSB.Clear();
			ToStringBuilder(_markupBuilderSB, RefreshAtNewline);
			return _markupBuilderSB.ToString();
		}
	}

	public override void ToStringBuilder(StringBuilder SB, bool RefreshAtNewline, ref char? lastForeground, ref char? lastBackground)
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
				if (Children[i] is MarkupTextNode markupTextNode)
				{
					num2 += markupTextNode.TextLength;
				}
			}
		}
		bool flag = false;
		char? c = lastForeground;
		char? c2 = lastBackground;
		int j = 0;
		for (int count2 = Children.Count; j < count2; j++)
		{
			if (Children[j] is MarkupTextNode markupTextNode2)
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
					markupTextNode2.ToStringBuilder(SB, RefreshAtNewline, ref lastForeground, ref lastBackground);
					c = lastForeground;
					c2 = lastBackground;
					continue;
				}
				int num3 = 0;
				int textLength = markupTextNode2.TextLength;
				char? c3 = null;
				int Index = 0;
				int length = markupTextNode2.Text.Length;
				while (Index < length)
				{
					ReadOnlySpan<char> nextPrintableSpan = GraphemeSplitter.GetNextPrintableSpan(markupTextNode2.Text, ref Index);
					if (nextPrintableSpan.IsEmpty)
					{
						continue;
					}
					if (nextPrintableSpan[0] == '&' && Index < length)
					{
						nextPrintableSpan = GraphemeSplitter.GetNextPrintableSpan(markupTextNode2.Text, ref Index);
						if (nextPrintableSpan[0] != '&')
						{
							SB.Append('&').Append(nextPrintableSpan);
							lastForeground = nextPrintableSpan[0];
							if (num3 == 0)
							{
								c = markupShader.GetForegroundColor(nextPrintableSpan[0], num3, textLength, num, num2);
							}
							continue;
						}
						c3 = '&';
					}
					else if (nextPrintableSpan[0] == '^' && Index < length)
					{
						nextPrintableSpan = GraphemeSplitter.GetNextPrintableSpan(markupTextNode2.Text, ref Index);
						if (nextPrintableSpan[0] != '^')
						{
							SB.Append('^').Append(nextPrintableSpan);
							lastBackground = nextPrintableSpan[0];
							if (num3 == 0)
							{
								c2 = markupShader.GetBackgroundColor(nextPrintableSpan[0], num3, textLength, num, num2);
							}
							continue;
						}
						c3 = '^';
					}
					char? foregroundColor = markupShader.GetForegroundColor(nextPrintableSpan[0], num3, textLength, num, num2);
					char? backgroundColor = markupShader.GetBackgroundColor(nextPrintableSpan[0], num3, textLength, num, num2);
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
					if (c3.HasValue)
					{
						SB.Append(c3);
						c3 = null;
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
			flag = Children[j] is MarkupControlNode;
			Children[j].ToStringBuilder(SB, RefreshAtNewline, ref lastForeground, ref lastBackground);
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

	public override void ToStringBuilder(StringBuilder SB, bool RefreshAtNewline)
	{
		char? lastForeground = null;
		char? lastBackground = 'k';
		ToStringBuilder(SB, RefreshAtNewline, ref lastForeground, ref lastBackground);
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
