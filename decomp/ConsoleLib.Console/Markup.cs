using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using XRL.World.Text;

namespace ConsoleLib.Console;

[DebuggerDisplay("Markup:{Action} {Children}")]
public class Markup : MarkupControlNode
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

	private static Queue<Markup> markupNodePool = new Queue<Markup>();

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

	public Markup()
	{
	}

	public Markup(string text)
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

	private static Markup Parse(ReadOnlySpan<char> text)
	{
		Markup markup;
		lock (markupNodePool)
		{
			markup = ((markupNodePool.Count > 0) ? markupNodePool.Dequeue() : null) ?? new Markup();
		}
		markup.set(text);
		return markup;
	}

	public static string Transform(ReadOnlySpan<char> text, bool refreshAtNewline = false)
	{
		if (Enabled && text != null && text.Contains("{{"))
		{
			Markup markup = Parse(text);
			string result = markup.ToString(refreshAtNewline);
			markup.release();
			return result;
		}
		return text.ToString();
	}

	public static void Transform(TextBuilder text, bool refreshAtNewline = false)
	{
		if (Enabled && text != null && text.Contains("{{"))
		{
			Markup markup = Parse(text.AsSpan());
			string value = markup.ToString(refreshAtNewline);
			text.Length = 0;
			text.Append(value);
			markup.release();
		}
	}

	public static void Transform(StringBuilder text, bool refreshAtNewline = false)
	{
		if (Enabled && text != null && text.Contains("{{"))
		{
			Markup markup = Parse(text.ToString());
			string value = markup.ToString(refreshAtNewline);
			text.Length = 0;
			text.Append(value);
			markup.release();
		}
	}

	public static List<string> Transform(List<string> text, bool refreshAtNewline = false)
	{
		return new List<string>(Transform(string.Join("\n", text.ToArray())).Split('\n'));
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

	private static void ParseText(ReadOnlySpan<char> text, ref int pos, MarkupControlNode parent)
	{
		int num = pos;
		while (pos < text.Length)
		{
			if (text[pos] == '{' && pos < text.Length - 1 && text[pos + 1] == '{')
			{
				if (pos > num)
				{
					parent.Add(text.Slice(num, pos - num));
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
			parent.Add(text.Slice(num, pos - num));
		}
	}

	private static void ParseControl(ReadOnlySpan<char> text, ref int pos, MarkupControlNode parent)
	{
		int num = pos;
		MarkupControlNode markupControlNode = MarkupControlNode.next();
		while (pos < text.Length)
		{
			char c = text[pos];
			if (c == '|' && markupControlNode.Action == null)
			{
				markupControlNode.Action = ((pos > num) ? text.Slice(num, pos - num).ToString() : "");
				pos++;
				num = pos;
				continue;
			}
			if (c == '}' && pos < text.Length - 1 && text[pos + 1] == '}')
			{
				if (markupControlNode.Action != null)
				{
					markupControlNode.Add(text.Slice(num, pos - num));
				}
				pos += 2;
				num = pos;
				break;
			}
			if (c == '{' && pos < text.Length - 1 && text[pos + 1] == '{')
			{
				if (markupControlNode.Action == null)
				{
					markupControlNode.Action = "";
				}
				if (pos > num)
				{
					markupControlNode.Add(text.Slice(num, pos - num));
				}
				pos += 2;
				ParseControl(text, ref pos, markupControlNode);
				num = pos;
			}
			else
			{
				pos++;
			}
		}
		if (pos > num)
		{
			markupControlNode.Add(text.Slice(num, pos - num));
		}
		if (markupControlNode.Children.Count > 0)
		{
			parent.Add(markupControlNode);
		}
	}

	public new static Markup next(ReadOnlySpan<char> text)
	{
		Markup markup;
		lock (markupNodePool)
		{
			markup = ((markupNodePool.Count > 0) ? markupNodePool.Dequeue() : null) ?? new Markup();
		}
		markup.set(text);
		return markup;
	}

	public override void release()
	{
		foreach (IMarkupNode child in Children)
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
