using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Cysharp.Text;
using Platform.IO;
using SimpleJSON;
using UnityEngine;
using XRL;
using XRL.World.Text;

namespace ConsoleLib.Console;

[HasModSensitiveStaticCache]
public static class ColorUtility
{
	public class ColorCollection
	{
		public Color DarkBlack { get; private set; }

		public Color DarkBlue { get; private set; }

		public Color DarkGreen { get; private set; }

		public Color DarkCyan { get; private set; }

		public Color DarkRed { get; private set; }

		public Color DarkMagenta { get; private set; }

		public Color Brown { get; private set; }

		public Color Gray { get; private set; }

		public Color DarkOrange { get; private set; }

		public Color Black { get; private set; }

		public Color Blue { get; private set; }

		public Color Green { get; private set; }

		public Color Cyan { get; private set; }

		public Color Red { get; private set; }

		public Color Magenta { get; private set; }

		public Color Yellow { get; private set; }

		public Color White { get; private set; }

		public Color Orange { get; private set; }

		public Color k { get; private set; }

		public Color b { get; private set; }

		public Color g { get; private set; }

		public Color c { get; private set; }

		public Color r { get; private set; }

		public Color m { get; private set; }

		public Color w { get; private set; }

		public Color y { get; private set; }

		public Color o { get; private set; }

		public Color K { get; private set; }

		public Color B { get; private set; }

		public Color G { get; private set; }

		public Color C { get; private set; }

		public Color R { get; private set; }

		public Color M { get; private set; }

		public Color W { get; private set; }

		public Color Y { get; private set; }

		public Color O { get; private set; }

		public Color DefaultForeground { get; private set; }

		public Color DefaultBackground { get; private set; }

		public Color DefaultDetail { get; private set; }

		public Color CameraBackground { get; private set; }

		public Color this[char Key] => ColorMap[Key];

		private void Load()
		{
			Color darkBlack = (k = ColorMap['k']);
			DarkBlack = darkBlack;
			darkBlack = (b = ColorMap['b']);
			DarkBlue = darkBlack;
			darkBlack = (g = ColorMap['g']);
			DarkGreen = darkBlack;
			darkBlack = (c = ColorMap['c']);
			DarkCyan = darkBlack;
			darkBlack = (r = ColorMap['r']);
			DarkRed = darkBlack;
			darkBlack = (m = ColorMap['m']);
			DarkMagenta = darkBlack;
			darkBlack = (w = ColorMap['w']);
			Brown = darkBlack;
			darkBlack = (y = ColorMap['y']);
			Gray = darkBlack;
			darkBlack = (o = ColorMap['o']);
			DarkOrange = darkBlack;
			darkBlack = (K = ColorMap['K']);
			Black = darkBlack;
			darkBlack = (B = ColorMap['B']);
			Blue = darkBlack;
			darkBlack = (G = ColorMap['G']);
			Green = darkBlack;
			darkBlack = (C = ColorMap['C']);
			Cyan = darkBlack;
			darkBlack = (R = ColorMap['R']);
			Red = darkBlack;
			darkBlack = (M = ColorMap['M']);
			Magenta = darkBlack;
			darkBlack = (W = ColorMap['W']);
			Yellow = darkBlack;
			darkBlack = (Y = ColorMap['Y']);
			White = darkBlack;
			darkBlack = (O = ColorMap['O']);
			Orange = darkBlack;
			DefaultForeground = Gray;
			DefaultBackground = DarkBlack;
			DefaultDetail = Yellow;
			CameraBackground = ColorAliasMap.GetValue(CAMERA_BACKGROUND);
		}

		public static void Load(ColorCollection Colors)
		{
			Colors.Load();
		}
	}

	public static readonly string DEFAULT_FOREGROUND = "default foreground";

	public static readonly string DEFAULT_BACKGROUND = "default background";

	public static readonly string DEFAULT_DETAIL = "default detail";

	public static readonly string CAMERA_BACKGROUND = "camera background";

	public static ColorCollection Colors = new ColorCollection();

	public static Dictionary<char, ushort> CharToColorMap = new Dictionary<char, ushort>();

	public static Dictionary<ushort, char> ColorAttributeToCharMap = new Dictionary<ushort, char>();

	public static Dictionary<Color, char> ColorToCharMap = new Dictionary<Color, char>();

	public static Dictionary<string, Color> ColorAliasMap = new Dictionary<string, Color>();

	public static Dictionary<char, Color> _ColorMap = new Dictionary<char, Color>();

	public static Color[] usfColorMap = new Color[32];

	private static bool BaseInitialized;

	private static bool ModInitialized;

	public static Color[] FastColorMap = new Color[257];

	private static StringBuilder StripSB = new StringBuilder(256);

	private static Dictionary<string, string> StripCache = new Dictionary<string, string>();

	private static Dictionary<char, int> ForegroundColorCounts = new Dictionary<char, int>();

	private static StringBuilder ClipSB = new StringBuilder(256);

	private static StringBuilder EscSB = new StringBuilder(256);

	private static StringBuilder ColorSB = new StringBuilder(256);

	private static Dictionary<string, Color> webColors = new Dictionary<string, Color>();

	private static Dictionary<string, List<string>> CachedForegroundExpansions = new Dictionary<string, List<string>>(16);

	private static string LastForegroundExpansionRequest;

	private static List<string> LastForegroundExpansionResult;

	private static Dictionary<string, List<string>> CachedBackgroundExpansions = new Dictionary<string, List<string>>(16);

	private static string LastBackgroundExpansionRequest;

	private static List<string> LastBackgroundExpansionResult;

	public static Dictionary<char, Color> ColorMap
	{
		get
		{
			if (_ColorMap == null)
			{
				Init();
			}
			return _ColorMap;
		}
		set
		{
			_ColorMap = value;
		}
	}

	public static Color ColorFromString(string str)
	{
		if (str.IsNullOrEmpty() || !ColorMap.TryGetValue(str[0], out var value))
		{
			return Colors.Gray;
		}
		return value;
	}

	public static Color colorFromTextColor(TextColor C)
	{
		return usfColorMap[(uint)C];
	}

	public static Color colorFromChar(char c)
	{
		if (c == '\0')
		{
			return Color.clear;
		}
		if (ColorMap.TryGetValue(c, out var value))
		{
			return value;
		}
		MetricsManager.LogError(new Exception($"Invalid color char '{c}'"));
		if (c == 'g')
		{
			return new Color(0f, 0.578f, 0.117f);
		}
		return Colors.Gray;
	}

	public static string StripBackgroundFormatting(string s)
	{
		StringBuilder stringBuilder = The.StringBuilder;
		int i = 0;
		for (int length = s.Length; i < length; i++)
		{
			if (s[i] != '^')
			{
				stringBuilder.Append(s[i]);
			}
			else if (s[i] == '^')
			{
				i++;
				if (s[i] == '^')
				{
					stringBuilder.Append('^');
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static StringBuilder StripFormatting(StringBuilder Text)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		StripSB.Clear();
		StripSB.Append(Text);
		Text.Clear();
		bool Found;
		do
		{
			GetNext(StripSB, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found);
			if (Found)
			{
				Text.Append(Ch);
			}
		}
		while (Found);
		StripSB.Clear();
		return Text;
	}

	public static string StripFormatting(string Text)
	{
		if (Text == null)
		{
			return "";
		}
		lock (StripCache)
		{
			if (StripCache.TryGetValue(Text, out var value))
			{
				return value;
			}
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		StripSB.Clear();
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found);
			if (Found)
			{
				StripSB.Append(Ch);
			}
		}
		while (Found);
		string text = StripSB.ToString();
		StripSB.Clear();
		lock (StripCache)
		{
			if (StripCache.Count > 50)
			{
				StripCache.Clear();
			}
			StripCache[Text] = text;
			return text;
		}
	}

	public static string GetMainForegroundColor(string s)
	{
		if (s == null)
		{
			return null;
		}
		s = Markup.Transform(s);
		ForegroundColorCounts.Clear();
		char c = 'y';
		int value = 0;
		char c2 = c;
		int num = 0;
		int i = 0;
		for (int length = s.Length; i < length; i++)
		{
			bool flag = false;
			if (s[i] == '&')
			{
				i++;
				if (i < length)
				{
					if (s[i] == '&')
					{
						flag = true;
					}
					else if (s[i] != c)
					{
						ForegroundColorCounts[c] = value;
						c = s[i];
						ForegroundColorCounts.TryGetValue(c, out value);
					}
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i < length && s[i] == '^')
				{
					flag = true;
				}
			}
			else if (s[i] != ' ')
			{
				flag = true;
			}
			if (flag)
			{
				value++;
				if (c == c2)
				{
					num++;
				}
				else if (value > num)
				{
					c2 = c;
					num = value;
				}
			}
		}
		return c2.ToString() ?? "";
	}

	public static string GetMainForegroundColor(StringBuilder s)
	{
		return GetMainForegroundColor(s.ToString());
	}

	public static bool HasFormatting(string Text)
	{
		if (Text.Contains("{{"))
		{
			return true;
		}
		int i = 0;
		for (int num = Text.Length - 1; i < num; i++)
		{
			if (Text[i] == '&')
			{
				i++;
				if (i < Text.Length && Text[i] != '&')
				{
					return true;
				}
			}
			else if (Text[i] == '^')
			{
				i++;
				if (i < Text.Length && Text[i] != '^')
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasFormatting(StringBuilder Text)
	{
		if (Text.Contains("{{"))
		{
			return true;
		}
		int i = 0;
		for (int num = Text.Length - 1; i < num; i++)
		{
			if (Text[i] == '&')
			{
				i++;
				if (i < Text.Length && Text[i] != '&')
				{
					return true;
				}
			}
			else if (Text[i] == '^')
			{
				i++;
				if (i < Text.Length && Text[i] != '^')
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void GetNext(string Text, int Len, int LenM1, ref int Pos, ref bool InControl, ref int ControlDepth, out char Ch, out bool Found, StringBuilder ControlStore = null)
	{
		Ch = '\0';
		Found = false;
		while (Pos < Len)
		{
			char c = Text[Pos];
			if (InControl)
			{
				ControlStore?.Append(c);
				if (c == '|')
				{
					InControl = false;
				}
				Pos++;
				continue;
			}
			if (Pos < LenM1)
			{
				if (c == '{' && Text[Pos + 1] == '{')
				{
					ControlStore?.Append("{{");
					InControl = true;
					ControlDepth++;
					Pos++;
					Pos++;
					continue;
				}
				if (ControlDepth > 0 && c == '}' && Text[Pos + 1] == '}')
				{
					ControlStore?.Append("}}");
					ControlDepth--;
					Pos++;
					Pos++;
					continue;
				}
			}
			if (Pos < LenM1)
			{
				switch (c)
				{
				case '&':
					ControlStore?.Append(c);
					Pos++;
					c = Text[Pos];
					if (c != '&')
					{
						ControlStore?.Append(c);
						Pos++;
						continue;
					}
					break;
				case '^':
					ControlStore?.Append(c);
					Pos++;
					c = Text[Pos];
					if (c != '^')
					{
						ControlStore?.Append(c);
						Pos++;
						continue;
					}
					break;
				}
			}
			Ch = c;
			Found = true;
			Pos++;
			break;
		}
	}

	private static void GetNext(StringBuilder Text, int Len, int LenM1, ref int Pos, ref bool InControl, ref int ControlDepth, out char Ch, out bool Found, StringBuilder ControlStore = null)
	{
		Ch = '\0';
		Found = false;
		while (Pos < Len)
		{
			char c = Text[Pos];
			if (InControl)
			{
				ControlStore?.Append(c);
				if (c == '|')
				{
					InControl = false;
				}
				Pos++;
				continue;
			}
			if (Pos < LenM1)
			{
				if (c == '{' && Text[Pos + 1] == '{')
				{
					ControlStore?.Append("{{");
					InControl = true;
					ControlDepth++;
					Pos++;
					Pos++;
					continue;
				}
				if (ControlDepth > 0 && c == '}' && Text[Pos + 1] == '}')
				{
					ControlStore?.Append("}}");
					ControlDepth--;
					Pos++;
					Pos++;
					continue;
				}
			}
			if (Pos < LenM1)
			{
				switch (c)
				{
				case '&':
					ControlStore?.Append(c);
					Pos++;
					c = Text[Pos];
					if (c != '&')
					{
						ControlStore?.Append(c);
						Pos++;
						continue;
					}
					break;
				case '^':
					ControlStore?.Append(c);
					Pos++;
					c = Text[Pos];
					if (c != '^')
					{
						ControlStore?.Append(c);
						Pos++;
						continue;
					}
					break;
				}
			}
			Ch = c;
			Found = true;
			Pos++;
			break;
		}
	}

	public static char FirstCharacterExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return '\0';
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out var _);
		return Ch;
	}

	public static char LastCharacterExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return '\0';
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		char result = '\0';
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found);
			if (Found)
			{
				result = Ch;
			}
		}
		while (Found);
		return result;
	}

	public static int LengthExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return 0;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		int num = 0;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var _, out Found);
			if (Found)
			{
				num++;
			}
		}
		while (Found);
		return num;
	}

	public static int LengthExceptFormatting(StringBuilder Text)
	{
		if (Text == null)
		{
			return 0;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		int num = 0;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var _, out Found);
			if (Found)
			{
				num++;
			}
		}
		while (Found);
		return num;
	}

	public static string ClipExceptFormatting(string Text, int Want)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		ClipSB.Clear();
		int num = 0;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found, ClipSB);
			if (Found)
			{
				ClipSB.Append(Ch);
				num++;
			}
		}
		while (Found && num < Want);
		for (int i = 0; i < ControlDepth; i++)
		{
			ClipSB.Append("}}");
		}
		return ClipSB.ToString();
	}

	public static string ClipExceptFormatting(StringBuilder Text, int Want)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		ClipSB.Clear();
		int num = 0;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found, ClipSB);
			if (Found)
			{
				ClipSB.Append(Ch);
				num++;
			}
		}
		while (Found && num < Want);
		for (int i = 0; i < ControlDepth; i++)
		{
			ClipSB.Append("}}");
		}
		return ClipSB.ToString();
	}

	public static string ClipToFirstExceptFormatting(string Text, char Ch)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		ClipSB.Clear();
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch2, out Found, ClipSB);
			if (Found)
			{
				if (Ch2 == Ch)
				{
					break;
				}
				ClipSB.Append(Ch2);
			}
		}
		while (Found);
		for (int i = 0; i < ControlDepth; i++)
		{
			ClipSB.Append("}}");
		}
		return ClipSB.ToString();
	}

	public static string ClipToFirstExceptFormatting(StringBuilder Text, char Ch)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		ClipSB.Clear();
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch2, out Found, ClipSB);
			if (Found)
			{
				if (Ch2 == Ch)
				{
					break;
				}
				ClipSB.Append(Ch2);
			}
		}
		while (Found);
		for (int i = 0; i < ControlDepth; i++)
		{
			ClipSB.Append("}}");
		}
		return ClipSB.ToString();
	}

	public static string EscapeFormatting(string s)
	{
		if (s == null)
		{
			return null;
		}
		EscSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			EscSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					EscSB.Append("\\{");
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					EscSB.Append("\\}");
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				EscSB.Append('&');
			}
			else if (s[i] == '^')
			{
				EscSB.Append('^');
			}
		}
		return EscSB.ToString();
	}

	public static string EscapeFormatting(StringBuilder s)
	{
		if (s == null)
		{
			return null;
		}
		EscSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			EscSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					EscSB.Append("\\{");
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					EscSB.Append("\\}");
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				EscSB.Append('&');
			}
			else if (s[i] == '^')
			{
				EscSB.Append('^');
			}
		}
		return EscSB.ToString();
	}

	public static string EscapeNonMarkupFormatting(string s)
	{
		if (s == null)
		{
			return null;
		}
		EscSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			EscSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					EscSB.Append('{');
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					EscSB.Append('}');
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				EscSB.Append('&');
			}
			else if (s[i] == '^')
			{
				EscSB.Append('^');
			}
		}
		return EscSB.ToString();
	}

	public static string EscapeNonMarkupFormatting(StringBuilder s)
	{
		if (s == null)
		{
			return null;
		}
		EscSB.Clear();
		bool flag = false;
		int num = 0;
		int i = 0;
		int length = s.Length;
		int num2 = length - 1;
		for (; i < length; i++)
		{
			EscSB.Append(s[i]);
			if (flag)
			{
				if (s[i] == '|')
				{
					flag = false;
				}
				continue;
			}
			if (i < num2)
			{
				if (s[i] == '{' && s[i + 1] == '{')
				{
					EscSB.Append('{');
					flag = true;
					num++;
					i++;
					continue;
				}
				if (num > 0 && s[i] == '}' && s[i + 1] == '}')
				{
					EscSB.Append('}');
					num--;
					i++;
					continue;
				}
			}
			if (s[i] == '&')
			{
				EscSB.Append('&');
			}
			else if (s[i] == '^')
			{
				EscSB.Append('^');
			}
		}
		return EscSB.ToString();
	}

	public static string ToUpperExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		StringBuilder stringBuilder = The.StringBuilder;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found, stringBuilder);
			if (Found)
			{
				stringBuilder.Append(char.ToUpper(Ch));
			}
		}
		while (Found);
		return stringBuilder.ToString();
	}

	public static string ToLowerExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		StringBuilder stringBuilder = The.StringBuilder;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found, stringBuilder);
			if (Found)
			{
				stringBuilder.Append(char.ToLower(Ch));
			}
		}
		while (Found);
		return stringBuilder.ToString();
	}

	public static string ReplaceExceptFormatting(string Text, char Search, char Replace)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		StringBuilder stringBuilder = The.StringBuilder;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found, stringBuilder);
			if (Found)
			{
				if (Ch == Search)
				{
					Ch = Replace;
				}
				stringBuilder.Append(Ch);
			}
		}
		while (Found);
		return stringBuilder.ToString();
	}

	public static string CapitalizeExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		bool flag = true;
		StringBuilder stringBuilder = The.StringBuilder;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found, stringBuilder);
			if (Found)
			{
				if (flag)
				{
					Ch = char.ToUpper(Ch);
					flag = false;
				}
				stringBuilder.Append(Ch);
			}
		}
		while (Found);
		return stringBuilder.ToString();
	}

	public static string UncapitalizeExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return null;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		bool flag = true;
		StringBuilder stringBuilder = The.StringBuilder;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found, stringBuilder);
			if (!Found)
			{
				continue;
			}
			if (flag)
			{
				if (char.IsUpper(Ch))
				{
					Ch = char.ToLower(Ch);
					flag = false;
				}
				else if (char.IsLower(Ch) || char.IsDigit(Ch))
				{
					flag = false;
				}
			}
			stringBuilder.Append(Ch);
		}
		while (Found);
		return stringBuilder.ToString();
	}

	public static string ApplyColor(string String, string Color)
	{
		if (Color.IsNullOrEmpty())
		{
			return String;
		}
		using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
		utf16ValueStringBuilder.Append("{{");
		utf16ValueStringBuilder.Append(Color);
		utf16ValueStringBuilder.Append('|');
		utf16ValueStringBuilder.Append(String);
		utf16ValueStringBuilder.Append("}}");
		return utf16ValueStringBuilder.ToString();
	}

	public static string ApplyColor(string String, char Color)
	{
		if (Color == ' ')
		{
			return String;
		}
		return ColorSB.Clear().Append("{{").Append(Color)
			.Append('|')
			.Append(String)
			.Append("}}")
			.ToString();
	}

	public static void SortExceptFormatting(List<string> List)
	{
		List.Sort(CompareExceptFormatting);
	}

	public static int CompareExceptFormattingNoCase(string A, string B)
	{
		if (A == null)
		{
			if (B == null)
			{
				return 0;
			}
			return -1;
		}
		if (B == null)
		{
			return 1;
		}
		bool InControl = false;
		int ControlDepth = 0;
		bool InControl2 = false;
		int ControlDepth2 = 0;
		int length = A.Length;
		int lenM = length - 1;
		int length2 = B.Length;
		int lenM2 = length2 - 1;
		int Pos = 0;
		int Pos2 = 0;
		char Ch = ' ';
		char Ch2 = ' ';
		bool Found = false;
		bool Found2 = false;
		while (Pos < length || Pos2 < length2)
		{
			GetNext(A, length, lenM, ref Pos, ref InControl, ref ControlDepth, out Ch, out Found);
			GetNext(B, length2, lenM2, ref Pos2, ref InControl2, ref ControlDepth2, out Ch2, out Found2);
			if (Found & Found2)
			{
				int num = char.ToUpperInvariant(Ch).CompareTo(char.ToUpperInvariant(Ch2));
				if (num != 0)
				{
					return num;
				}
				continue;
			}
			if (Found)
			{
				return 1;
			}
			if (Found2)
			{
				return -1;
			}
			return 0;
		}
		return 0;
	}

	public static int CompareExceptFormatting(string A, string B)
	{
		if (A == null)
		{
			if (B == null)
			{
				return 0;
			}
			return -1;
		}
		if (B == null)
		{
			return 1;
		}
		bool InControl = false;
		int ControlDepth = 0;
		bool InControl2 = false;
		int ControlDepth2 = 0;
		int length = A.Length;
		int lenM = length - 1;
		int length2 = B.Length;
		int lenM2 = length2 - 1;
		int Pos = 0;
		int Pos2 = 0;
		char Ch = ' ';
		char Ch2 = ' ';
		bool Found = false;
		bool Found2 = false;
		while (Pos < length || Pos2 < length2)
		{
			GetNext(A, length, lenM, ref Pos, ref InControl, ref ControlDepth, out Ch, out Found);
			GetNext(B, length2, lenM2, ref Pos2, ref InControl2, ref ControlDepth2, out Ch2, out Found2);
			if (Found & Found2)
			{
				int num = Ch.CompareTo(Ch2);
				if (num != 0)
				{
					return num;
				}
				continue;
			}
			if (Found)
			{
				return 1;
			}
			if (Found2)
			{
				return -1;
			}
			return 0;
		}
		return 0;
	}

	public static void SortExceptFormattingAndCase(List<string> List)
	{
		List?.Sort(CompareExceptFormattingAndCase);
	}

	public static int CompareExceptFormattingAndCase(string A, string B)
	{
		if (A == null)
		{
			if (B == null)
			{
				return 0;
			}
			return -1;
		}
		if (B == null)
		{
			return 1;
		}
		bool InControl = false;
		int ControlDepth = 0;
		bool InControl2 = false;
		int ControlDepth2 = 0;
		int length = A.Length;
		int lenM = length - 1;
		int length2 = B.Length;
		int lenM2 = length2 - 1;
		int Pos = 0;
		int Pos2 = 0;
		char Ch = ' ';
		char Ch2 = ' ';
		bool Found = false;
		bool Found2 = false;
		while (Pos < length || Pos2 < length2)
		{
			GetNext(A, length, lenM, ref Pos, ref InControl, ref ControlDepth, out Ch, out Found);
			GetNext(B, length2, lenM2, ref Pos2, ref InControl2, ref ControlDepth2, out Ch2, out Found2);
			if (Found & Found2)
			{
				if (char.IsLetter(Ch) && char.IsLetter(Ch2))
				{
					Ch = char.ToUpper(Ch);
					Ch2 = char.ToUpper(Ch2);
				}
				int num = Ch.CompareTo(Ch2);
				if (num != 0)
				{
					return num;
				}
				continue;
			}
			if (Found)
			{
				return 1;
			}
			if (Found2)
			{
				return -1;
			}
			return 0;
		}
		return 0;
	}

	public static bool EqualsExceptFormatting(string A, string B)
	{
		return CompareExceptFormatting(A, B) == 0;
	}

	public static bool EqualsExceptFormattingAndCase(string A, string B)
	{
		return CompareExceptFormattingAndCase(A, B) == 0;
	}

	public static bool BeginsWithExceptFormatting(string A, string B)
	{
		if (A == null)
		{
			return false;
		}
		if (B == null)
		{
			return false;
		}
		if (B == "")
		{
			return true;
		}
		if (A == "")
		{
			return false;
		}
		bool InControl = false;
		int ControlDepth = 0;
		bool InControl2 = false;
		int ControlDepth2 = 0;
		int length = A.Length;
		int lenM = length - 1;
		int length2 = B.Length;
		int lenM2 = length2 - 1;
		int Pos = 0;
		int Pos2 = 0;
		char Ch = ' ';
		char Ch2 = ' ';
		bool Found = false;
		bool Found2 = false;
		bool result = false;
		while (Pos < length || Pos2 < length2)
		{
			GetNext(A, length, lenM, ref Pos, ref InControl, ref ControlDepth, out Ch, out Found);
			GetNext(B, length2, lenM2, ref Pos2, ref InControl2, ref ControlDepth2, out Ch2, out Found2);
			if (Found & Found2)
			{
				if (Ch != Ch2)
				{
					return false;
				}
				result = true;
				continue;
			}
			if (Found && !Found2)
			{
				result = true;
			}
			break;
		}
		return result;
	}

	public static bool HasUpperExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return false;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		char Ch = ' ';
		bool Found = false;
		while (Pos < length)
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out Ch, out Found);
			if (!Found)
			{
				break;
			}
			if (char.IsUpper(Ch))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasLowerExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return false;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		char Ch = ' ';
		bool Found = false;
		while (Pos < length)
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out Ch, out Found);
			if (!Found)
			{
				break;
			}
			if (char.IsLower(Ch))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAllUpperExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return false;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		char Ch = ' ';
		bool Found = false;
		bool result = false;
		while (Pos < length)
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out Ch, out Found);
			if (!Found)
			{
				break;
			}
			if (char.IsUpper(Ch))
			{
				result = true;
			}
			else if (char.IsLower(Ch))
			{
				return false;
			}
		}
		return result;
	}

	public static bool IsAllLowerExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return false;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		char Ch = ' ';
		bool Found = false;
		bool result = false;
		while (Pos < length)
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out Ch, out Found);
			if (!Found)
			{
				break;
			}
			if (char.IsLower(Ch))
			{
				result = true;
			}
			else if (char.IsUpper(Ch))
			{
				return false;
			}
		}
		return result;
	}

	public static bool IsFirstUpperExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return false;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out var Found);
		if (Found)
		{
			return char.IsUpper(Ch);
		}
		return false;
	}

	public static bool IsFirstLowerExceptFormatting(string Text)
	{
		if (Text == null)
		{
			return false;
		}
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out var Found);
		if (Found)
		{
			return char.IsLower(Ch);
		}
		return false;
	}

	public static void ComponentsFromAttributeColor(ushort Color, out float R, out float G, out float B)
	{
		R = 0f;
		G = 0f;
		B = 0f;
		switch (Color)
		{
		case 1:
			B = 128f;
			break;
		case 2:
			G = 128f;
			break;
		case 3:
			B = 128f;
			G = 128f;
			break;
		case 4:
			R = 128f;
			break;
		case 5:
			R = 128f;
			B = 128f;
			break;
		case 6:
			G = 128f;
			R = 128f;
			break;
		case 7:
			R = 192f;
			G = 192f;
			B = 192f;
			break;
		case 8:
			R = 128f;
			G = 128f;
			B = 128f;
			break;
		case 9:
			B = 255f;
			break;
		case 10:
			G = 255f;
			break;
		case 11:
			B = 255f;
			G = 255f;
			break;
		case 12:
			R = 255f;
			break;
		case 13:
			R = 255f;
			B = 255f;
			break;
		case 14:
			G = 255f;
			R = 255f;
			break;
		case 15:
			R = 255f;
			G = 255f;
			B = 255f;
			break;
		}
	}

	public static void ForegroundFromAttribute(ushort A, out float R, out float G, out float B)
	{
		ComponentsFromAttributeColor(GetForeground(A), out R, out G, out B);
	}

	public static void BackgroundFromAttribute(ushort A, out float R, out float G, out float B)
	{
		ComponentsFromAttributeColor(GetBackground(A), out R, out G, out B);
	}

	public static Color FromWebColor(string s)
	{
		if (s.StartsWith("#"))
		{
			s = s.Substring(1);
		}
		if (webColors.TryGetValue(s, out var value))
		{
			return value;
		}
		float num = (int)byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
		float num2 = (int)byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
		float num3 = (int)byte.Parse(s.Substring(4, 2), NumberStyles.HexNumber);
		webColors.Add(s, new Color(num / 255f, num2 / 255f, num3 / 255f));
		return webColors[s];
	}

	[ModSensitiveCacheInit]
	public static void Reinit()
	{
		if (!ModInitialized)
		{
			Init(!BaseInitialized, Mod: true, Clear: false);
		}
		else
		{
			Init();
		}
	}

	private static List<(string path, ModInfo mod)> GetPaths(bool Base, bool Mod)
	{
		List<(string path, ModInfo mod)> paths = new List<(string, ModInfo)>();
		if (Base)
		{
			paths.Add((DataManager.FilePath("Display.txt"), null));
		}
		if (Mod)
		{
			ModManager.ForEachFile("Display.txt", (string path, ModInfo mod) =>
			{
				paths.Add((path, mod));
			});
		}
		try
		{
			string text = DataManager.SavePath("Display.txt");
			if (File.Exists(text))
			{
				paths.Add((text, null));
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Error getting save path for Display.txt", x);
		}
		return paths;
	}

	private static void LoadDisplaySettings(string Path, GameManager Manager, GameObject MainCamera)
	{
		JSONClass jSONClass = JSON.Parse(File.ReadAllText(Path)) as JSONClass;
		if (jSONClass["colors"] is JSONClass jSONClass2)
		{
			foreach (KeyValuePair<string, JSONNode> item in jSONClass2)
			{
				Color color = FromWebColor(item.Value);
				if (ColorMap.ContainsKey(item.Key[0]))
				{
					ColorToCharMap.Remove(ColorMap[item.Key[0]]);
					ColorMap.Remove(item.Key[0]);
				}
				FastColorMap[(uint)item.Key[0]] = color;
				ColorMap.Add(item.Key[0], color);
				ColorToCharMap.Add(color, item.Key[0]);
			}
		}
		if (!MainCamera)
		{
			return;
		}
		if (jSONClass["camera"] is JSONClass jSONClass3 && jSONClass3["background"] != null)
		{
			Camera component = MainCamera.GetComponent<Camera>();
			component.backgroundColor = FromWebColor(jSONClass3["background"]);
			ColorAliasMap[CAMERA_BACKGROUND] = component.backgroundColor;
		}
		if (jSONClass["tiles"] is JSONClass jSONClass4)
		{
			if (jSONClass4["width"] != null)
			{
				Manager.tileWidth = Convert.ToInt32(jSONClass4["width"]);
				LetterboxCamera component2 = MainCamera.GetComponent<LetterboxCamera>();
				component2.InitialDesiredWidth = Manager.tileWidth * 80;
				component2.DesiredWidth = Manager.tileWidth * 80;
				component2.Refresh();
			}
			if (jSONClass4["height"] != null)
			{
				Manager.tileHeight = Convert.ToInt32(jSONClass4["height"]);
			}
		}
		if (!(jSONClass["shaders"] is JSONClass jSONClass5))
		{
			return;
		}
		if (jSONClass5["scanlines"] != null)
		{
			CC_AnalogTV component3 = MainCamera.GetComponent<CC_AnalogTV>();
			if (jSONClass5["scanlines"]["enable"] != null)
			{
				if (jSONClass5["scanlines"]["enable"].Value.EqualsNoCase("true"))
				{
					component3.enabled = true;
				}
				else
				{
					component3.enabled = false;
				}
			}
			if (jSONClass5["scanlines"]["greyscale"] != null)
			{
				component3.grayscale = jSONClass5["scanlines"]["greyscale"].Value.EqualsNoCase("true");
			}
			if (jSONClass5["scanlines"]["noise"] != null)
			{
				component3.noiseIntensity = Convert.ToSingle(jSONClass5["scanlines"]["noise"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["intensity"] != null)
			{
				component3.scanlinesIntensity = Convert.ToSingle(jSONClass5["scanlines"]["intensity"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["count"] != null)
			{
				component3.scanlinesCount = Convert.ToSingle(jSONClass5["scanlines"]["count"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["offset"] != null)
			{
				component3.scanlinesOffset = Convert.ToSingle(jSONClass5["scanlines"]["offset"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["distortion"] != null)
			{
				component3.distortion = Convert.ToSingle(jSONClass5["scanlines"]["distortion"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["cubicdistortion"] != null)
			{
				component3.cubicDistortion = Convert.ToSingle(jSONClass5["scanlines"]["cubicdistortion"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["scanlines"]["zoom"] != null)
			{
				component3.scale = Convert.ToSingle(jSONClass5["scanlines"]["zoom"].Value, CultureInfo.InvariantCulture);
			}
		}
		if (jSONClass5["vignette"] != null)
		{
			CC_FastVignette component4 = MainCamera.GetComponent<CC_FastVignette>();
			if (jSONClass5["vignette"]["enable"] != null)
			{
				if (jSONClass5["vignette"]["enable"].Value.EqualsNoCase("true"))
				{
					component4.enabled = true;
				}
				else
				{
					component4.enabled = false;
				}
			}
			if (jSONClass5["vignette"]["sharpness"] != null)
			{
				component4.sharpness = Convert.ToSingle(jSONClass5["vignette"]["sharpness"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["vignette"]["darkness"] != null)
			{
				component4.darkness = Convert.ToSingle(jSONClass5["vignette"]["darkness"].Value, CultureInfo.InvariantCulture);
			}
			if (jSONClass5["vignette"]["desaturate"] != null)
			{
				component4.desaturate = jSONClass5["vignette"]["desaturate"].Value.EqualsNoCase("true");
			}
		}
		if (!(jSONClass5["settings"] != null))
		{
			return;
		}
		CC_BrightnessContrastGamma component5 = MainCamera.GetComponent<CC_BrightnessContrastGamma>();
		if (jSONClass5["settings"]["enable"] != null)
		{
			if (jSONClass5["settings"]["enable"].Value.EqualsNoCase("true"))
			{
				component5.enabled = true;
			}
			else
			{
				component5.enabled = false;
			}
		}
		if (jSONClass5["settings"]["brightness"] != null)
		{
			component5.brightness = Convert.ToSingle(jSONClass5["settings"]["brightness"].Value, CultureInfo.InvariantCulture);
		}
		if (jSONClass5["settings"]["contrast"] != null)
		{
			component5.contrast = Convert.ToSingle(jSONClass5["settings"]["contrast"].Value, CultureInfo.InvariantCulture);
		}
		if (jSONClass5["settings"]["gamma"] != null)
		{
			component5.gamma = Convert.ToSingle(jSONClass5["settings"]["gamma"].Value, CultureInfo.InvariantCulture);
		}
	}

	internal static void LoadBaseColors()
	{
		if (!BaseInitialized)
		{
			Init(Base: true, Mod: false, Clear: false);
			BaseInitialized = true;
		}
	}

	internal static void LoadModColors()
	{
		if (!ModInitialized)
		{
			Init(Base: false, Mod: true, Clear: false);
			ModInitialized = true;
		}
	}

	private static void ClearMaps()
	{
		ColorMap.Clear();
		ColorToCharMap.Clear();
		CharToColorMap.Clear();
		ColorAttributeToCharMap.Clear();
		ColorAliasMap.Clear();
		((IList<Color>)usfColorMap).Fill(default(Color));
	}

	private static void Init(bool Base = true, bool Mod = true, bool Clear = true)
	{
		if (Clear)
		{
			ClearMaps();
		}
		GameManager manager = GameObject.Find("GameManager")?.GetComponent<GameManager>();
		GameObject mainCamera = GameObject.Find("Main Camera");
		foreach (var (text, modInfo) in GetPaths(Base, Mod))
		{
			try
			{
				LoadDisplaySettings(text, manager, mainCamera);
			}
			catch (Exception ex)
			{
				if (modInfo != null)
				{
					modInfo.Error($"Error Loading {text}\n{ex}");
				}
				else
				{
					MetricsManager.LogException("Error loading " + text, ex);
				}
			}
		}
		ColorCollection.Load(Colors);
		usfColorMap[0] = Colors.DarkBlack;
		usfColorMap[1] = Colors.DarkBlue;
		usfColorMap[4] = Colors.DarkRed;
		usfColorMap[2] = Colors.DarkGreen;
		usfColorMap[3] = Colors.DarkCyan;
		usfColorMap[5] = Colors.DarkMagenta;
		usfColorMap[6] = Colors.Brown;
		usfColorMap[7] = Colors.Gray;
		usfColorMap[8] = Colors.DarkOrange;
		usfColorMap[16] = Colors.Black;
		usfColorMap[17] = Colors.Blue;
		usfColorMap[20] = Colors.Red;
		usfColorMap[18] = Colors.Green;
		usfColorMap[19] = Colors.Cyan;
		usfColorMap[21] = Colors.Magenta;
		usfColorMap[22] = Colors.Yellow;
		usfColorMap[23] = Colors.White;
		usfColorMap[24] = Colors.Orange;
		CharToColorMap['k'] = 0;
		CharToColorMap['b'] = 1;
		CharToColorMap['r'] = 4;
		CharToColorMap['g'] = 2;
		CharToColorMap['c'] = 3;
		CharToColorMap['m'] = 5;
		CharToColorMap['w'] = 6;
		CharToColorMap['y'] = 7;
		CharToColorMap['o'] = 8;
		CharToColorMap['K'] = Bright((ushort)0);
		CharToColorMap['B'] = Bright(1);
		CharToColorMap['R'] = Bright(4);
		CharToColorMap['G'] = Bright(2);
		CharToColorMap['C'] = Bright(3);
		CharToColorMap['M'] = Bright(5);
		CharToColorMap['W'] = Bright(6);
		CharToColorMap['Y'] = Bright(7);
		CharToColorMap['O'] = Bright(8);
		ColorAttributeToCharMap[0] = 'k';
		ColorAttributeToCharMap[1] = 'b';
		ColorAttributeToCharMap[4] = 'r';
		ColorAttributeToCharMap[2] = 'g';
		ColorAttributeToCharMap[3] = 'c';
		ColorAttributeToCharMap[5] = 'm';
		ColorAttributeToCharMap[6] = 'w';
		ColorAttributeToCharMap[7] = 'y';
		ColorAttributeToCharMap[8] = 'o';
		ColorAttributeToCharMap[Bright((ushort)0)] = 'K';
		ColorAttributeToCharMap[Bright(1)] = 'B';
		ColorAttributeToCharMap[Bright(4)] = 'R';
		ColorAttributeToCharMap[Bright(2)] = 'G';
		ColorAttributeToCharMap[Bright(3)] = 'C';
		ColorAttributeToCharMap[Bright(5)] = 'M';
		ColorAttributeToCharMap[Bright(6)] = 'W';
		ColorAttributeToCharMap[Bright(7)] = 'Y';
		ColorAttributeToCharMap[Bright(8)] = 'O';
		ColorAliasMap[DEFAULT_FOREGROUND] = Colors.DefaultForeground;
		ColorAliasMap[DEFAULT_BACKGROUND] = Colors.DefaultBackground;
		ColorAliasMap[DEFAULT_DETAIL] = Colors.DefaultDetail;
	}

	public static ushort MakeColor(ushort foreground, ushort background)
	{
		return (ushort)(foreground + (background << 5));
	}

	public static ushort MakeColor(TextColor foreground, TextColor background)
	{
		return (ushort)((uint)foreground + ((uint)background << 5));
	}

	public static ushort MakeColor(ushort foreground, TextColor background)
	{
		return (ushort)(foreground + ((uint)background << 5));
	}

	public static ushort MakeColor(TextColor foreground, ushort background)
	{
		return (ushort)((uint)foreground + (uint)(background << 5));
	}

	public static ushort Bright(ushort c)
	{
		return (ushort)(c | 0x10);
	}

	public static ushort Bright(TextColor c)
	{
		return (ushort)(c | (TextColor)0x10);
	}

	public static ushort MakeBackgroundColor(ushort c)
	{
		return (ushort)(c << 5);
	}

	public static ushort MakeBackgroundColor(TextColor c)
	{
		return (ushort)((uint)c << 5);
	}

	public static char ParseForegroundColor(string str, char defaultColor = 'y')
	{
		if (string.IsNullOrEmpty(str))
		{
			return 'y';
		}
		char result = defaultColor;
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] == '&')
			{
				i++;
				if (i < str.Length && str[i] != '&')
				{
					result = str[i];
				}
			}
		}
		return result;
	}

	public static char? FindLastForeground(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		char? result = null;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '&')
			{
				i++;
				if (i < length && text[i] != '&')
				{
					result = text[i];
				}
			}
		}
		return result;
	}

	public static void FindLastForeground(string text, ref char? result)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '&')
			{
				i++;
				if (i < length && text[i] != '&')
				{
					result = text[i];
				}
			}
		}
	}

	public static char? FindLastBackground(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		char? result = null;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '^')
			{
				i++;
				if (i < length && text[i] != '^')
				{
					result = text[i];
				}
			}
		}
		return result;
	}

	public static void FindLastBackground(string text, ref char? result)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '^')
			{
				i++;
				if (i < length && text[i] != '^')
				{
					result = text[i];
				}
			}
		}
	}

	public static void FindLastForegroundAndBackground(ReadOnlySpan<char> text, ref char? foreground, ref char? background)
	{
		if (text.IsEmpty)
		{
			return;
		}
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			if (text[i] == '&')
			{
				i++;
				if (i < length && text[i] != '&')
				{
					foreground = text[i];
				}
			}
			else if (text[i] == '^')
			{
				i++;
				if (i < length && text[i] != '^')
				{
					background = text[i];
				}
			}
		}
	}

	public static ushort GetForeground(ushort c)
	{
		return (ushort)(c & 0x1F);
	}

	public static ushort GetBackground(ushort c)
	{
		return (ushort)(c >> 5);
	}

	public static List<string> CachedForegroundExpansion(string Text, char Separator = ',')
	{
		if (Text == null)
		{
			return null;
		}
		if (Text == LastForegroundExpansionRequest)
		{
			return LastForegroundExpansionResult;
		}
		if (!CachedForegroundExpansions.TryGetValue(Text, out var value))
		{
			string[] array = Text.Split(Separator);
			value = new List<string>(array.Length);
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				value.Add("&" + array[i]);
			}
			CachedForegroundExpansions.Add(Text, value);
		}
		LastForegroundExpansionRequest = Text;
		return LastForegroundExpansionResult = value;
	}

	public static List<string> CachedBackgroundExpansion(string Text, char Separator = ',')
	{
		if (Text == null)
		{
			return null;
		}
		if (Text == LastBackgroundExpansionRequest)
		{
			return LastBackgroundExpansionResult;
		}
		if (!CachedBackgroundExpansions.TryGetValue(Text, out var value))
		{
			string[] array = Text.Split(Separator);
			value = new List<string>(array.Length);
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				value.Add("^" + array[i]);
			}
			CachedBackgroundExpansions.Add(Text, value);
		}
		LastBackgroundExpansionRequest = Text;
		return LastBackgroundExpansionResult = value;
	}

	public static void StripFormatting(ReadOnlySpan<char> Input, TextBuilder Output)
	{
		bool InControl = false;
		int ControlDepth = 0;
		int length = Input.Length;
		int lenM = length - 1;
		int Pos = 0;
		Output.Clear();
		bool Found;
		do
		{
			GetNext(Input, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var Ch, out Found);
			if (Found)
			{
				Output.Append(Ch);
			}
		}
		while (Found);
	}

	public static int LengthExceptFormatting(ReadOnlySpan<char> Text)
	{
		bool InControl = false;
		int ControlDepth = 0;
		int length = Text.Length;
		int lenM = length - 1;
		int Pos = 0;
		int num = 0;
		bool Found;
		do
		{
			GetNext(Text, length, lenM, ref Pos, ref InControl, ref ControlDepth, out var _, out Found);
			if (Found)
			{
				num++;
			}
		}
		while (Found);
		return num;
	}

	private static void GetNext(ReadOnlySpan<char> Text, int Len, int LenM1, ref int Pos, ref bool InControl, ref int ControlDepth, out ReadOnlySpan<char> Ch, out bool Found, StringBuilder ControlStore = null)
	{
		Ch = null;
		Found = false;
		while (Pos < Len)
		{
			ReadOnlySpan<char> readOnlySpan = Text.Slice(Pos++, 1);
			if (InControl)
			{
				ControlStore?.Append(readOnlySpan);
				if (readOnlySpan[0] == '|')
				{
					InControl = false;
				}
				continue;
			}
			if (Pos < Len)
			{
				if (readOnlySpan[0] == '{' && Text[Pos] == '{')
				{
					ControlStore?.Append("{{");
					InControl = true;
					ControlDepth++;
					Pos++;
					continue;
				}
				if (ControlDepth > 0 && readOnlySpan[0] == '}' && Text[Pos] == '}')
				{
					ControlStore?.Append("}}");
					ControlDepth--;
					Pos++;
					continue;
				}
			}
			if (Pos < Len)
			{
				if (readOnlySpan[0] == '&')
				{
					ControlStore?.Append(readOnlySpan);
					readOnlySpan = Text.Slice(Pos++, 1);
					if (readOnlySpan[0] != '&')
					{
						ControlStore?.Append(readOnlySpan);
						continue;
					}
				}
				else if (readOnlySpan[0] == '^')
				{
					ControlStore?.Append(readOnlySpan);
					readOnlySpan = Text.Slice(Pos++, 1);
					if (readOnlySpan[0] != '^')
					{
						ControlStore?.Append(readOnlySpan);
						continue;
					}
				}
			}
			Ch = readOnlySpan;
			Found = true;
			break;
		}
	}
}
