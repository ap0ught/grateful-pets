using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Conditions;
using AiUnity.NLog.Core.Config;
using AiUnity.NLog.Core.Internal;
using AiUnity.NLog.Core.LayoutRenderers;
using AiUnity.NLog.Core.LayoutRenderers.Wrappers;

namespace AiUnity.NLog.Core.Layouts;

internal sealed class LayoutParser
{
	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	internal static LayoutRenderer[] CompileLayout(ConfigurationItemFactory configurationItemFactory, SimpleStringReader sr, bool isNested, out string text)
	{
		List<LayoutRenderer> list = new List<LayoutRenderer>();
		StringBuilder stringBuilder = new StringBuilder();
		int position = sr.Position;
		int num;
		while ((num = sr.Peek()) != -1 && (!isNested || (num != 125 && num != 58)))
		{
			sr.Read();
			if (num == 36 && sr.Peek() == 123)
			{
				if (stringBuilder.Length > 0)
				{
					list.Add(new LiteralLayoutRenderer(stringBuilder.ToString()));
					stringBuilder.Length = 0;
				}
				LayoutRenderer layoutRenderer = ParseLayoutRenderer(configurationItemFactory, sr);
				if (CanBeConvertedToLiteral(layoutRenderer))
				{
					layoutRenderer = ConvertToLiteral(layoutRenderer);
				}
				list.Add(layoutRenderer);
			}
			else
			{
				stringBuilder.Append((char)num);
			}
		}
		if (stringBuilder.Length > 0)
		{
			list.Add(new LiteralLayoutRenderer(stringBuilder.ToString()));
			stringBuilder.Length = 0;
		}
		int position2 = sr.Position;
		MergeLiterals(list);
		text = sr.Substring(position, position2);
		return list.ToArray();
	}

	private static string ParseLayoutRendererName(SimpleStringReader sr)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num;
		while ((num = sr.Peek()) != -1 && num != 58 && num != 125)
		{
			stringBuilder.Append((char)num);
			sr.Read();
		}
		return stringBuilder.ToString();
	}

	private static string ParseParameterName(SimpleStringReader sr)
	{
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		int num2;
		while ((num2 = sr.Peek()) != -1 && ((num2 != 61 && num2 != 125 && num2 != 58) || num != 0))
		{
			switch (num2)
			{
			case 36:
				sr.Read();
				stringBuilder.Append('$');
				if (sr.Peek() == 123)
				{
					stringBuilder.Append('{');
					num++;
					sr.Read();
				}
				continue;
			case 125:
				num--;
				break;
			}
			if (num2 == 92)
			{
				sr.Read();
				stringBuilder.Append((char)sr.Read());
			}
			else
			{
				stringBuilder.Append((char)num2);
				sr.Read();
			}
		}
		return stringBuilder.ToString();
	}

	private static string ParseParameterValue(SimpleStringReader sr)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num;
		while ((num = sr.Peek()) != -1)
		{
			switch (num)
			{
			case 92:
				sr.Read();
				switch ((char)(ushort)sr.Peek())
				{
				case ':':
					sr.Read();
					stringBuilder.Append(':');
					break;
				case '{':
					sr.Read();
					stringBuilder.Append('{');
					break;
				case '}':
					sr.Read();
					stringBuilder.Append('}');
					break;
				case '\'':
					sr.Read();
					stringBuilder.Append('\'');
					break;
				case '"':
					sr.Read();
					stringBuilder.Append('"');
					break;
				case '\\':
					sr.Read();
					stringBuilder.Append('\\');
					break;
				case '0':
					sr.Read();
					stringBuilder.Append('\0');
					break;
				case 'a':
					sr.Read();
					stringBuilder.Append('\a');
					break;
				case 'b':
					sr.Read();
					stringBuilder.Append('\b');
					break;
				case 'f':
					sr.Read();
					stringBuilder.Append('\f');
					break;
				case 'n':
					sr.Read();
					stringBuilder.Append('\n');
					break;
				case 'r':
					sr.Read();
					stringBuilder.Append('\r');
					break;
				case 't':
					sr.Read();
					stringBuilder.Append('\t');
					break;
				case 'u':
				{
					sr.Read();
					char unicode3 = GetUnicode(sr, 4);
					stringBuilder.Append(unicode3);
					break;
				}
				case 'U':
				{
					sr.Read();
					char unicode2 = GetUnicode(sr, 8);
					stringBuilder.Append(unicode2);
					break;
				}
				case 'x':
				{
					sr.Read();
					char unicode = GetUnicode(sr, 4);
					stringBuilder.Append(unicode);
					break;
				}
				case 'v':
					sr.Read();
					stringBuilder.Append('\v');
					break;
				}
				continue;
			default:
				stringBuilder.Append((char)num);
				sr.Read();
				continue;
			case 58:
			case 125:
				break;
			}
			break;
		}
		return stringBuilder.ToString();
	}

	private static char GetUnicode(SimpleStringReader sr, int maxDigits)
	{
		int num = 0;
		for (int i = 0; i < maxDigits; i++)
		{
			int num2 = sr.Peek();
			if (num2 >= 48 && num2 <= 57)
			{
				num2 -= 48;
			}
			else if (num2 >= 97 && num2 <= 102)
			{
				num2 = num2 - 97 + 10;
			}
			else
			{
				if (num2 < 65 || num2 > 70)
				{
					break;
				}
				num2 = num2 - 65 + 10;
			}
			sr.Read();
			num = num * 16 + num2;
		}
		return (char)num;
	}

	private static LayoutRenderer ParseLayoutRenderer(ConfigurationItemFactory configurationItemFactory, SimpleStringReader sr)
	{
		int num = sr.Read();
		Logger.Assert(num == 123, "'{' expected in layout specification");
		string itemName = ParseLayoutRendererName(sr);
		LayoutRenderer layoutRenderer = configurationItemFactory.LayoutRenderers.CreateInstance(itemName);
		Dictionary<Type, LayoutRenderer> dictionary = new Dictionary<Type, LayoutRenderer>();
		List<LayoutRenderer> list = new List<LayoutRenderer>();
		num = sr.Read();
		while (num != -1 && num != 125)
		{
			string text = ParseParameterName(sr).Trim();
			PropertyInfo result3;
			if (sr.Peek() == 61)
			{
				sr.Read();
				LayoutRenderer layoutRenderer2 = layoutRenderer;
				if (!PropertyHelper.TryGetPropertyInfo(layoutRenderer, text, out var result) && configurationItemFactory.AmbientProperties.TryGetDefinition(text, out var result2))
				{
					if (!dictionary.TryGetValue(result2, out var value))
					{
						value = (dictionary[result2] = configurationItemFactory.AmbientProperties.CreateInstance(text));
						list.Add(value);
					}
					if (!PropertyHelper.TryGetPropertyInfo(value, text, out result))
					{
						result = null;
					}
					else
					{
						layoutRenderer2 = value;
					}
				}
				if (result == null)
				{
					ParseParameterValue(sr);
				}
				else if (typeof(Layout).IsAssignableFrom(result.PropertyType))
				{
					SimpleLayout simpleLayout = new SimpleLayout();
					LayoutRenderer[] renderers = CompileLayout(configurationItemFactory, sr, isNested: true, out var text2);
					simpleLayout.SetRenderers(renderers, text2);
					result.SetValue(layoutRenderer2, simpleLayout, null);
				}
				else if (typeof(ConditionExpression).IsAssignableFrom(result.PropertyType))
				{
					ConditionExpression value2 = ConditionParser.ParseExpression(sr, configurationItemFactory);
					result.SetValue(layoutRenderer2, value2, null);
				}
				else
				{
					string value3 = ParseParameterValue(sr);
					PropertyHelper.SetPropertyFromString(layoutRenderer2, text, value3, configurationItemFactory);
				}
			}
			else if (PropertyHelper.TryGetPropertyInfo(layoutRenderer, string.Empty, out result3))
			{
				if (typeof(SimpleLayout) == result3.PropertyType)
				{
					result3.SetValue(layoutRenderer, new SimpleLayout(text), null);
				}
				else
				{
					string value4 = text;
					PropertyHelper.SetPropertyFromString(layoutRenderer, result3.Name, value4, configurationItemFactory);
				}
			}
			else
			{
				Logger.Warn("{0} has no default property", layoutRenderer.GetType().FullName);
			}
			num = sr.Read();
		}
		return ApplyWrappers(configurationItemFactory, layoutRenderer, list);
	}

	private static LayoutRenderer ApplyWrappers(ConfigurationItemFactory configurationItemFactory, LayoutRenderer lr, List<LayoutRenderer> orderedWrappers)
	{
		for (int num = orderedWrappers.Count - 1; num >= 0; num--)
		{
			WrapperLayoutRendererBase wrapperLayoutRendererBase = (WrapperLayoutRendererBase)orderedWrappers[num];
			Logger.Trace("Wrapping {0} with {1}", lr.GetType().Name, wrapperLayoutRendererBase.GetType().Name);
			if (CanBeConvertedToLiteral(lr))
			{
				lr = ConvertToLiteral(lr);
			}
			wrapperLayoutRendererBase.Inner = new SimpleLayout(new LayoutRenderer[1] { lr }, string.Empty, configurationItemFactory);
			lr = wrapperLayoutRendererBase;
		}
		return lr;
	}

	private static bool CanBeConvertedToLiteral(LayoutRenderer lr)
	{
		IRenderable[] array = ObjectGraphScanner.FindReachableObjects<IRenderable>(new object[1] { lr });
		foreach (IRenderable renderable in array)
		{
			if (!(renderable.GetType() == typeof(SimpleLayout)) && !renderable.GetType().IsDefined(typeof(AppDomainFixedOutputAttribute), inherit: false))
			{
				return false;
			}
		}
		return true;
	}

	private static void MergeLiterals(List<LayoutRenderer> list)
	{
		int num = 0;
		while (num + 1 < list.Count)
		{
			LiteralLayoutRenderer literalLayoutRenderer = list[num] as LiteralLayoutRenderer;
			LiteralLayoutRenderer literalLayoutRenderer2 = list[num + 1] as LiteralLayoutRenderer;
			if (literalLayoutRenderer != null && literalLayoutRenderer2 != null)
			{
				literalLayoutRenderer.Text += literalLayoutRenderer2.Text;
				list.RemoveAt(num + 1);
			}
			else
			{
				num++;
			}
		}
	}

	private static LayoutRenderer ConvertToLiteral(LayoutRenderer renderer)
	{
		return new LiteralLayoutRenderer(renderer.Render(LogEventInfo.CreateNullEvent()));
	}
}
