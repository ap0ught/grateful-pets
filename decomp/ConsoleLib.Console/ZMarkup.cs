using System;
using System.Collections.Generic;
using System.Diagnostics;
using XRL.World.Text;

namespace ConsoleLib.Console;

[DebuggerDisplay("Markup:{Action} {Children}")]
public class ZMarkup : ZMarkupControlNode
{
	public const char BASE_FOREGROUND = 'y';

	public const char BASE_BACKGROUND = 'k';

	public static readonly char[] NORMAL_FOREGROUNDS = new char[15]
	{
		'K', 'b', 'B', 'C', 'c', 'g', 'G', 'r', 'R', 'm',
		'M', 'W', 'w', 'Y', 'y'
	};

	private static string _BaseColorPattern;

	public static bool Enabled = true;

	private static Queue<ZMarkup> markupNodePool = new Queue<ZMarkup>();

	private static string BaseColorPattern
	{
		get
		{
			if (_BaseColorPattern == null)
			{
				_BaseColorPattern = "&y^k";
			}
			return _BaseColorPattern;
		}
	}

	public static char SignedValueTriColor(int value)
	{
		if (value <= 0)
		{
			if (value >= 0)
			{
				return 'c';
			}
			return 'R';
		}
		return 'G';
	}

	public ZMarkup()
	{
	}

	public ZMarkup(string text)
		: this()
	{
		set(text);
	}

	public new void set(ReadOnlySpan<char> text)
	{
		Action = BaseColorPattern;
		int pos = 0;
		ParseText(text, ref pos, this);
	}

	private static ZMarkup Parse(ReadOnlySpan<char> text)
	{
		ZMarkup zMarkup;
		lock (markupNodePool)
		{
			zMarkup = ((markupNodePool.Count > 0) ? markupNodePool.Dequeue() : null) ?? new ZMarkup();
		}
		zMarkup.set(text);
		return zMarkup;
	}

	public static void Transform(TextBuilder Input, TextBuilder Output, bool refreshAtNewline = false)
	{
		Span<char> span = Input.AsSpan();
		ZMarkup zMarkup = Parse(span);
		if (zMarkup.Children.Count == 1 && Input[0] != '{')
		{
			zMarkup.Children[0].ToTextBuilder(Output.Clear(), span, refreshAtNewline);
			return;
		}
		zMarkup.ToTextBuilder(Output.Clear(), span, refreshAtNewline);
		zMarkup.release();
	}

	public static string Strip(string Text)
	{
		if (Text.IsNullOrEmpty())
		{
			return Text;
		}
		using TextBuilder textBuilder = TextBuilder.Get(Text);
		Strip(textBuilder);
		return textBuilder.ToString();
	}

	public static void Strip(TextBuilder Builder)
	{
		char[] array = Builder.GetArray();
		int i = 0;
		int num = -1;
		int num2 = 0;
		for (int num3 = Builder.Length; i < num3; i++)
		{
			switch (array[i])
			{
			case '{':
				if (i < num3 - 1 && array[i + 1] == '{')
				{
					num = i++;
				}
				break;
			case '|':
				if (num != -1)
				{
					i++;
					Builder.Remove(num, i - num);
					num3 -= i - num;
					i = num - 1;
					num2++;
					num = -1;
				}
				break;
			case '}':
				if (num2 > 0 && i < num3 - 1 && array[i + 1] == '}')
				{
					Builder.Remove(i, 2);
					num3 -= 2;
					i--;
					num2--;
				}
				break;
			case '&':
			case '^':
				if (i < num3 - 1)
				{
					char c = array[i + 1];
					if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
					{
						Builder.Remove(i, 2);
						num3 -= 2;
						i--;
					}
				}
				break;
			}
		}
	}

	public static string Wrap(string Text)
	{
		if (Text != null && (Text.Contains("&") || Text.Contains("^")) && !Text.StartsWith("{{"))
		{
			Text = "{{|" + Text + "}}";
		}
		return Text;
	}

	public static string Color(string Color, string Text)
	{
		return "{{" + Color + "|" + Text + "}}";
	}

	private static void ParseText(ReadOnlySpan<char> text, ref int pos, ZMarkupControlNode parent)
	{
		int num = pos;
		while (pos < text.Length)
		{
			if (text[pos] == '{' && pos < text.Length - 1 && text[pos + 1] == '{')
			{
				if (pos > num)
				{
					parent.Add(text, num, pos - num);
				}
				pos += 2;
				ParseControl(text, ref pos, parent);
				num = pos;
			}
			else
			{
				pos++;
			}
		}
		if (pos > num)
		{
			parent.Add(text, num, pos - num);
		}
	}

	private static void ParseControl(ReadOnlySpan<char> text, ref int pos, ZMarkupControlNode parent)
	{
		int num = pos;
		ZMarkupControlNode zMarkupControlNode = ZMarkupControlNode.next();
		while (pos < text.Length)
		{
			char c = text[pos];
			if (c == '|' && zMarkupControlNode.Action == null)
			{
				zMarkupControlNode.Action = ((pos > num) ? text.Slice(num, pos - num).ToString() : "");
				pos++;
				num = pos;
				continue;
			}
			if (c == '}' && pos < text.Length - 1 && text[pos + 1] == '}')
			{
				if (zMarkupControlNode.Action != null)
				{
					zMarkupControlNode.Add(text, num, pos - num);
				}
				pos += 2;
				num = pos;
				break;
			}
			if (c == '{' && pos < text.Length - 1 && text[pos + 1] == '{')
			{
				if (zMarkupControlNode.Action == null)
				{
					zMarkupControlNode.Action = "";
				}
				if (pos > num)
				{
					zMarkupControlNode.Add(text, num, pos - num);
				}
				pos += 2;
				ParseControl(text, ref pos, zMarkupControlNode);
				num = pos;
			}
			else
			{
				pos++;
			}
		}
		if (pos > num)
		{
			zMarkupControlNode.Add(text, num, pos - num);
		}
		if (zMarkupControlNode.Children.Count > 0)
		{
			parent.Add(zMarkupControlNode);
		}
	}

	public new static ZMarkup next(ReadOnlySpan<char> text)
	{
		ZMarkup zMarkup;
		lock (markupNodePool)
		{
			zMarkup = ((markupNodePool.Count > 0) ? markupNodePool.Dequeue() : null) ?? new ZMarkup();
		}
		zMarkup.set(text);
		return zMarkup;
	}

	public override void release()
	{
		foreach (IZMarkupNode child in Children)
		{
			child.release();
		}
		Action = null;
		Children.Clear();
		lock (markupNodePool)
		{
			markupNodePool.Enqueue(this);
		}
	}
}
