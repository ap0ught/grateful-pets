using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Genkit;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;

namespace ConsoleLib.Console;

public class ScreenBuffer
{
	public string ViewTag;

	public Point2D focusPosition = Point2D.invalid;

	public ConsoleChar[,] Buffer;

	public int _Width;

	public int _Height;

	public int _X;

	public int _Y;

	public StringBuilder TSB = new StringBuilder(2000);

	public static bool bLowContrast = false;

	public static bool[,] ImposterSuppression = new bool[80, 25];

	public ConsoleChar CurrentChar => Buffer[_X, _Y];

	public int X
	{
		get
		{
			return _X;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (value >= _Width)
			{
				value %= _Width;
			}
			_X = value;
		}
	}

	public int Y
	{
		get
		{
			return _Y;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (value >= _Width)
			{
				value %= _Width;
			}
			_Y = value;
		}
	}

	public int Width
	{
		get
		{
			return _Width;
		}
		set
		{
			_Width = value;
		}
	}

	public int Height
	{
		get
		{
			return _Height;
		}
		set
		{
			_Height = value;
		}
	}

	public ConsoleChar this[int r, int c]
	{
		get
		{
			return Buffer[r, c];
		}
		set
		{
			if (r < Width && c < Height && r >= 0 && c >= 0)
			{
				Buffer[r, c] = value;
			}
		}
	}

	public ConsoleChar this[Cell C]
	{
		get
		{
			return Buffer[C.X, C.Y];
		}
		set
		{
			Buffer[C.X, C.Y] = value;
		}
	}

	public ConsoleChar this[Location2D L]
	{
		get
		{
			return Buffer[L.X, L.Y];
		}
		set
		{
			Buffer[L.X, L.Y] = value;
		}
	}

	public ConsoleChar get(int x, int y)
	{
		if (x < 0)
		{
			return null;
		}
		if (y < 0)
		{
			return null;
		}
		if (x >= _Width)
		{
			return null;
		}
		if (y >= _Height)
		{
			return null;
		}
		return Buffer[x, y];
	}

	public ScreenBuffer WithBase()
	{
		XRLCore.Core.RenderBaseToBuffer(this);
		return this;
	}

	public ScreenBuffer WithMap()
	{
		XRLCore.Core.RenderMapToBuffer(this);
		return this;
	}

	public ScreenBuffer RenderBase()
	{
		return WithBase();
	}

	public ScreenBuffer Draw()
	{
		XRLCore._Console.DrawBuffer(this);
		return this;
	}

	public ScreenBuffer ScrollUp()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (i == Height - 1)
				{
					Buffer[j, i].Char = ' ';
					Buffer[j, i].SetForeground('y');
					Buffer[j, i].SetBackground('k');
				}
				else
				{
					Buffer[j, i].Copy(Buffer[j, i + 1]);
				}
			}
		}
		return this;
	}

	public override string ToString()
	{
		TSB.Length = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				TSB.Append(Buffer[j, i].Char.ToString());
			}
			TSB.Append("\n");
		}
		return TSB.ToString();
	}

	public static ScreenBuffer create(int w, int h)
	{
		return new ScreenBuffer(w, h);
	}

	public static ScreenBuffer GetScrapBuffer1(bool bLoadFromCurrent = false)
	{
		return TextConsole.GetScrapBuffer1(bLoadFromCurrent);
	}

	public static ScreenBuffer GetScrapBuffer2(bool bLoadFromCurrent = false)
	{
		return TextConsole.GetScrapBuffer2(bLoadFromCurrent);
	}

	private ScreenBuffer(int w, int h)
	{
		Width = w;
		Height = h;
		Buffer = new ConsoleChar[w, h];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Buffer[i, j] = new ConsoleChar();
			}
		}
	}

	public void Shake(int MS, int Duration, TextConsole Console)
	{
		int num = MS / Duration;
		ScreenBuffer screenBuffer = new ScreenBuffer(Width, Height);
		for (int i = 0; i < num; i++)
		{
			int num2 = Stat.Random(-1, 1);
			int num3 = Stat.Random(-1, 1);
			for (int j = 0; j < Height; j++)
			{
				for (int k = 0; k < Width; k++)
				{
					int num4 = k + num2;
					int num5 = j + num3;
					if (num4 >= 0 && num4 < Width && num5 >= 0 && num5 < Height)
					{
						ConsoleChar c = this[k, j];
						screenBuffer[num4, num5].Copy(c);
					}
					else
					{
						screenBuffer[k, j].Char = ' ';
					}
				}
			}
			Console.DrawBuffer(screenBuffer);
			Thread.Sleep(Duration);
		}
	}

	public void Clear()
	{
		focusPosition = Point2D.invalid;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Buffer[j, i].Clear();
			}
		}
	}

	public void Clear(int X1, int Y1, int X2, int Y2)
	{
		for (int i = X1; i <= X2; i++)
		{
			for (int j = Y1; j <= Y2; j++)
			{
				Buffer[i, j].Clear();
			}
		}
	}

	public ScreenBuffer Copy(ScreenBuffer Source)
	{
		if (Source != null)
		{
			int num = Math.Min(Height, Source.Height);
			int num2 = Math.Min(Width, Source.Width);
			ConsoleChar[,] buffer = Buffer;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					buffer[j, i].Copy(Source[j, i]);
				}
			}
			focusPosition = Source.focusPosition;
		}
		return this;
	}

	public ScreenBuffer Goto(int x, int y)
	{
		X = x;
		Y = y;
		return this;
	}

	public ScreenBuffer Goto(Cell C)
	{
		if (C != null)
		{
			X = C.X;
			Y = C.Y;
		}
		return this;
	}

	public ScreenBuffer Goto(XRL.World.GameObject obj)
	{
		if (obj != null)
		{
			Goto(obj.CurrentCell);
		}
		return this;
	}

	public void Write(ushort c, ushort f, ushort b)
	{
		ushort colorsFromOldCharCode = ColorUtility.MakeColor(f, b);
		if (_X < Width && _X >= 0 && _Y < Height && _Y >= 0)
		{
			this[_X, _Y].Char = (char)c;
			this[_X, _Y].SetColorsFromOldCharCode(colorsFromOldCharCode);
		}
		if (X != Width - 1)
		{
			X++;
			if (X >= Width)
			{
				X = 0;
				Y++;
			}
			if (Y >= Height)
			{
				Y = 0;
			}
		}
	}

	public void Write(short c)
	{
		if (_X < Width && _X >= 0 && _Y < Height && _Y >= 0)
		{
			ConsoleChar consoleChar = this[_X, _Y];
			consoleChar.Char = (char)c;
			consoleChar.Foreground = The.Color.Gray;
			consoleChar.Background = The.Color.Black;
		}
		X++;
		if (X >= Width)
		{
			X = 0;
			Y++;
		}
		if (Y >= Height)
		{
			Y = 0;
		}
	}

	public void Write(string Tile, string RenderString, string ColorString, string TileColor, char DetailColor, bool SuppressImposters = true, bool HFlip = false, bool VFlip = false)
	{
		if (_X < 0 || _X >= Width || _Y < 0 || _Y >= Height)
		{
			return;
		}
		ConsoleChar consoleChar = this[_X, _Y];
		string text = ColorString;
		consoleChar.HFlip = HFlip;
		consoleChar.VFlip = VFlip;
		if (!string.IsNullOrEmpty(Tile) && Options.UseTiles)
		{
			consoleChar.Tile = Tile;
			if (SuppressImposters)
			{
				ImposterSuppression[_X, _Y] = true;
			}
			Color value;
			if (DetailColor == '\0')
			{
				consoleChar.Detail = The.Color.DarkBlack;
			}
			else if (ColorUtility.ColorMap.TryGetValue(DetailColor, out value))
			{
				consoleChar.Detail = value;
			}
			text = TileColor ?? text;
		}
		else if (!string.IsNullOrEmpty(RenderString))
		{
			consoleChar.Char = RenderString[0];
		}
		if (!string.IsNullOrEmpty(text))
		{
			int num = text.LastIndexOf('&');
			if (num != -1 && ColorUtility.ColorMap.TryGetValue(text[num + 1], out var value2))
			{
				consoleChar.Foreground = value2;
			}
			else
			{
				consoleChar.Foreground = The.Color.DarkBlack;
			}
			num = text.LastIndexOf('^');
			if (num != -1 && ColorUtility.ColorMap.TryGetValue(text[num + 1], out value2))
			{
				consoleChar.Background = value2;
			}
			else
			{
				consoleChar.Background = The.Color.DarkBlack;
			}
		}
		X++;
		if (X >= Width)
		{
			X = 0;
			Y++;
		}
		if (Y >= Height)
		{
			Y = 0;
		}
	}

	public void Write(string Tile, string RenderString, string ColorString, string TileColor, string DetailColor, bool SuppressImposters = true, bool HFlip = false, bool VFlip = false)
	{
		Write(Tile, RenderString, ColorString, TileColor, string.IsNullOrEmpty(DetailColor) ? DetailColor[0] : '\0', SuppressImposters, HFlip, VFlip);
	}

	public void Write(IRenderable r)
	{
		if (r == null)
		{
			Write("{{M|?}}");
		}
		else
		{
			Write(r.getTile(), r.getRenderString(), r.getColorString(), r.getTileColor(), r.getDetailColor(), SuppressImposters: true, r.getHFlip(), r.getVFlip());
		}
	}

	public void Write(IRenderable r, string Tile = null, string RenderString = null, string ColorString = null, string TileColor = null, char? DetailColor = null, bool SuppressImposters = true, bool HFlip = false, bool VFlip = false)
	{
		if (r == null)
		{
			Write("{{M|?}}");
		}
		else
		{
			Write(Tile ?? r.getTile(), RenderString ?? r.getRenderString(), ColorString ?? r.getColorString(), TileColor ?? r.getTileColor(), DetailColor ?? r.getDetailColor(), SuppressImposters, HFlip, VFlip);
		}
	}

	public static Color GetColorFromChar(char c)
	{
		Color value = Color.grey;
		if (ColorUtility.ColorMap.TryGetValue(c, out value))
		{
			return value;
		}
		return Color.grey;
	}

	public static Color GetForeground(string s)
	{
		Color result = The.Color.Gray;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] != '&')
			{
				continue;
			}
			i++;
			if (i >= s.Length)
			{
				break;
			}
			if (s[i] != '&')
			{
				try
				{
					result = GetColorFromChar(s[i]);
				}
				catch
				{
				}
			}
		}
		return result;
	}

	public static Color GetBackground(string s)
	{
		Color result = The.Color.DarkBlack;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] != '^')
			{
				continue;
			}
			i++;
			if (i >= s.Length)
			{
				break;
			}
			if (s[i] != '^')
			{
				try
				{
					result = GetColorFromChar(s[i]);
				}
				catch
				{
				}
			}
		}
		return result;
	}

	public int WriteBlockWithNewlines(string[] strings, int MaxLines = 999, int StartLine = 0, bool drawIndicators = false)
	{
		return WriteBlockWithNewlines(string.Join("\n", strings), MaxLines, StartLine, drawIndicators);
	}

	public int WriteBlockWithNewlines(string s, int MaxLines = 999, int StartLine = 0, bool drawIndicators = false)
	{
		if (string.IsNullOrEmpty(s))
		{
			return 1;
		}
		Color value = The.Color.Gray;
		Color value2 = The.Color.DarkBlack;
		int x = _X;
		int num = 1;
		int num2 = 0;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '&')
			{
				i++;
				if (i >= s.Length)
				{
					break;
				}
				if (s[i] == '&')
				{
					if (_X >= 0 && _Y >= 0 && _X < Width && _X >= 0 && _Y < Height && _Y >= 0 && num2 >= StartLine)
					{
						this[_X, _Y].Char = '&';
						this[_X, _Y].Foreground = value;
						this[_X, _Y].Background = value2;
						this[_X, _Y].HFlip = false;
						this[_X, _Y].VFlip = false;
					}
					if (X == Width - 1)
					{
						return num;
					}
					X++;
					if (X >= Width)
					{
						X = 0;
						if (num2 >= StartLine)
						{
							Y++;
						}
						break;
					}
					if (Y >= Height)
					{
						Y = 0;
						break;
					}
				}
				else
				{
					try
					{
						ColorUtility.ColorMap.TryGetValue(s[i], out value);
					}
					catch
					{
					}
				}
				continue;
			}
			if (s[i] == '^')
			{
				i++;
				if (i >= s.Length)
				{
					break;
				}
				if (s[i] == '^')
				{
					if (_X >= 0 && _Y >= 0 && _X < Width && _X >= 0 && _Y < Height && _Y >= 0 && num2 >= StartLine)
					{
						this[_X, _Y].Char = '^';
						this[_X, _Y].Foreground = value;
						this[_X, _Y].Background = value2;
						this[_X, _Y].HFlip = false;
						this[_X, _Y].VFlip = false;
					}
					if (X == Width - 1)
					{
						return num;
					}
					X++;
					if (X >= Width)
					{
						X = 0;
						if (num2 >= StartLine)
						{
							Y++;
						}
						break;
					}
					if (Y >= Height)
					{
						Y = 0;
						break;
					}
				}
				else
				{
					try
					{
						ColorUtility.ColorMap.TryGetValue(s[i], out value2);
					}
					catch
					{
					}
				}
				continue;
			}
			if (s[i] == '\n')
			{
				if (num2 >= StartLine)
				{
					Y++;
				}
				X = x;
				if (num2 >= StartLine)
				{
					num++;
				}
				num2++;
				if (num >= MaxLines)
				{
					return num;
				}
				continue;
			}
			if (_X < Width && _X >= 0 && _Y < Height && _Y >= 0 && num2 >= StartLine)
			{
				this[_X, _Y].Char = s[i];
				this[_X, _Y].Foreground = value;
				this[_X, _Y].Background = value2;
				this[_X, _Y].HFlip = false;
				this[_X, _Y].VFlip = false;
			}
			if (X == Width - 1)
			{
				return num;
			}
			X++;
			if (X >= Width)
			{
				X = 0;
				if (num2 >= StartLine)
				{
					Y++;
				}
			}
			if (Y >= Height)
			{
				Y = 0;
			}
		}
		return num;
	}

	public ScreenBuffer WriteAt(int x, int y, string s, bool processMarkup = true)
	{
		int x2 = X;
		int y2 = Y;
		Goto(x, y);
		Write(s, processMarkup);
		Goto(x2, y2);
		return this;
	}

	public ScreenBuffer WriteAt(Cell C, string s, bool processMarkup = true)
	{
		int x = X;
		int y = Y;
		if (C != null)
		{
			Goto(C);
			Write(s, processMarkup);
		}
		Goto(x, y);
		return this;
	}

	public ScreenBuffer WriteAt(XRL.World.GameObject obj, string s, bool processMarkup = true)
	{
		int x = X;
		int y = Y;
		if (obj != null)
		{
			WriteAt(obj.CurrentCell, s, processMarkup);
		}
		Goto(x, y);
		return this;
	}

	public ScreenBuffer Write(string s, bool processMarkup = true, bool HFlip = false, bool VFlip = false, List<string> imposters = null, int maxCharsWritten = -1)
	{
		if (string.IsNullOrEmpty(s))
		{
			return this;
		}
		if (processMarkup)
		{
			s = Markup.Transform(s);
		}
		Color foreground = The.Color.DefaultForeground;
		Color background = The.Color.DefaultBackground;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '&')
			{
				i++;
				if (i >= s.Length)
				{
					break;
				}
				Color value;
				if (s[i] == '&')
				{
					if (_X >= 0 && _Y >= 0 && _X < Width && _X >= 0 && _Y < Height && _Y >= 0)
					{
						ConsoleChar consoleChar = this[_X, _Y];
						consoleChar.Char = '&';
						consoleChar.Foreground = foreground;
						consoleChar.Background = background;
						consoleChar.HFlip = HFlip;
						consoleChar.VFlip = VFlip;
					}
					if (X == Width - 1)
					{
						return this;
					}
					if (--maxCharsWritten == 0)
					{
						return this;
					}
					X++;
					if (X >= Width)
					{
						X = 0;
						Y++;
						break;
					}
					if (Y >= Height)
					{
						Y = 0;
						break;
					}
				}
				else if (ColorUtility.ColorMap.TryGetValue(s[i], out value))
				{
					foreground = value;
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i >= s.Length)
				{
					break;
				}
				Color value2;
				if (s[i] == '^')
				{
					if (_X >= 0 && _Y >= 0 && _X < Width && _X >= 0 && _Y < Height && _Y >= 0)
					{
						ConsoleChar consoleChar2 = this[_X, _Y];
						consoleChar2.Char = '^';
						consoleChar2.Foreground = foreground;
						consoleChar2.Background = background;
						consoleChar2.HFlip = HFlip;
						consoleChar2.VFlip = VFlip;
					}
					if (X == Width - 1)
					{
						return this;
					}
					if (--maxCharsWritten == 0)
					{
						return this;
					}
					X++;
					if (X >= Width)
					{
						X = 0;
						Y++;
						break;
					}
					if (Y >= Height)
					{
						Y = 0;
						break;
					}
				}
				else if (ColorUtility.ColorMap.TryGetValue(s[i], out value2))
				{
					background = value2;
				}
			}
			else if (s[i] == '\n')
			{
				Y++;
				X = 0;
			}
			else
			{
				if (_X < Width && _X >= 0 && _Y < Height && _Y >= 0)
				{
					ConsoleChar consoleChar3 = this[_X, _Y];
					consoleChar3.Char = s[i];
					consoleChar3.Foreground = foreground;
					consoleChar3.Background = background;
					consoleChar3.HFlip = HFlip;
					consoleChar3.VFlip = VFlip;
				}
				if (X == Width - 1)
				{
					return this;
				}
				if (--maxCharsWritten == 0)
				{
					return this;
				}
				X++;
				if (X >= Width)
				{
					X = 0;
					Y++;
				}
				if (Y >= Height)
				{
					Y = 0;
				}
			}
		}
		return this;
	}

	public ScreenBuffer Write(StringBuilder s, int maxCharsWritten = -1)
	{
		if (s.Length == 0)
		{
			return this;
		}
		Markup.Transform(s);
		Color foreground = The.Color.DefaultForeground;
		Color background = The.Color.DefaultBackground;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == '&')
			{
				i++;
				if (i >= s.Length)
				{
					break;
				}
				Color value;
				if (s[i] == '&')
				{
					if (_X >= 0 && _Y >= 0 && _X < Width && _X >= 0 && _Y < Height && _Y >= 0)
					{
						ConsoleChar consoleChar = this[_X, _Y];
						consoleChar.Char = '&';
						consoleChar.Foreground = foreground;
						consoleChar.Background = background;
						consoleChar.HFlip = false;
						consoleChar.VFlip = false;
					}
					if (--maxCharsWritten == 0)
					{
						return this;
					}
					if (X == Width - 1)
					{
						return this;
					}
					X++;
					if (X >= Width)
					{
						X = 0;
						Y++;
						break;
					}
					if (Y >= Height)
					{
						Y = 0;
						break;
					}
				}
				else if (ColorUtility.ColorMap.TryGetValue(s[i], out value))
				{
					foreground = value;
				}
			}
			else if (s[i] == '^')
			{
				i++;
				if (i >= s.Length)
				{
					break;
				}
				Color value2;
				if (s[i] == '^')
				{
					if (_X >= 0 && _Y >= 0 && _X < Width && _X >= 0 && _Y < Height && _Y >= 0)
					{
						ConsoleChar consoleChar2 = this[_X, _Y];
						consoleChar2.Char = '^';
						consoleChar2.Foreground = foreground;
						consoleChar2.Background = background;
						consoleChar2.HFlip = false;
						consoleChar2.VFlip = false;
					}
					if (--maxCharsWritten == 0)
					{
						return this;
					}
					if (X == Width - 1)
					{
						return this;
					}
					X++;
					if (X >= Width)
					{
						X = 0;
						Y++;
						break;
					}
					if (Y >= Height)
					{
						Y = 0;
						break;
					}
				}
				else if (ColorUtility.ColorMap.TryGetValue(s[i], out value2))
				{
					background = value2;
				}
			}
			else if (s[i] == '\n')
			{
				Y++;
				X = 0;
			}
			else
			{
				if (_X < Width && _X >= 0 && _Y < Height && _Y >= 0)
				{
					ConsoleChar consoleChar3 = this[_X, _Y];
					consoleChar3.Char = s[i];
					consoleChar3.Foreground = foreground;
					consoleChar3.Background = background;
					consoleChar3.HFlip = false;
					consoleChar3.VFlip = false;
				}
				if (--maxCharsWritten == 0)
				{
					return this;
				}
				if (X == Width - 1)
				{
					return this;
				}
				X++;
				if (X >= Width)
				{
					X = 0;
					Y++;
				}
				if (Y >= Height)
				{
					Y = 0;
				}
			}
		}
		return this;
	}

	public void Fill(int x1, int y1, int x2, int y2, ushort Char, ushort Color, bool HFlip = false, bool VFlip = false)
	{
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (x2 >= Width)
		{
			x2 = Width - 1;
		}
		if (y2 >= Height)
		{
			y2 = Height - 1;
		}
		if (y2 < 0 || x2 < 0 || x1 >= Width || y1 >= Height)
		{
			return;
		}
		for (int i = x1; i <= x2; i++)
		{
			for (int j = y1; j <= y2; j++)
			{
				if (i >= 0 && j >= 0 && i < 80 && j < 25)
				{
					Buffer[i, j].Char = (char)Char;
					Buffer[i, j].SetColorsFromOldCharCode(Color);
					Buffer[i, j].HFlip = HFlip;
					Buffer[i, j].VFlip = VFlip;
				}
			}
		}
	}

	public void BeveledBox(int x1, int y1, int x2, int y2, ushort ForeColor, ushort BackColor)
	{
		for (int i = x1 + 1; i < x2; i++)
		{
			Buffer[i, y1].Char = 'Ü';
			Buffer[i, y1].SetColorsFromOldCharCode(ColorUtility.MakeColor(BackColor, ForeColor));
			Buffer[i, y1].HFlip = false;
			Buffer[i, y1].VFlip = false;
			Buffer[i, y2].Char = 'Ü';
			Buffer[i, y2].SetColorsFromOldCharCode(ColorUtility.MakeColor(ForeColor, BackColor));
			Buffer[i, y2].HFlip = false;
			Buffer[i, y2].VFlip = false;
		}
		for (int j = y1 + 1; j < y2; j++)
		{
			Buffer[x1, j].Char = 'Ý';
			Buffer[x1, j].SetColorsFromOldCharCode(ColorUtility.MakeColor(ForeColor, BackColor));
			Buffer[x1, j].HFlip = false;
			Buffer[x1, j].VFlip = false;
			Buffer[x2, j].Char = 'Ý';
			Buffer[x2, j].SetColorsFromOldCharCode(ColorUtility.MakeColor(BackColor, ForeColor));
			Buffer[x2, j].HFlip = false;
			Buffer[x2, j].VFlip = false;
		}
		Buffer[x1, y1].Char = 'Ú';
		Buffer[x1, y1].SetColorsFromOldCharCode(ColorUtility.MakeColor(BackColor, ForeColor));
		Buffer[x1, y1].HFlip = false;
		Buffer[x1, y1].VFlip = false;
		Buffer[x1, y2].Char = 'À';
		Buffer[x1, y2].SetColorsFromOldCharCode(ColorUtility.MakeColor(BackColor, ForeColor));
		Buffer[x1, y2].HFlip = false;
		Buffer[x1, y2].VFlip = false;
		Buffer[x2, y1].Char = '¿';
		Buffer[x2, y1].SetColorsFromOldCharCode(ColorUtility.MakeColor(BackColor, ForeColor));
		Buffer[x2, y1].HFlip = false;
		Buffer[x2, y1].VFlip = false;
		Buffer[x2, y2].Char = 'Ù';
		Buffer[x2, y2].SetColorsFromOldCharCode(ColorUtility.MakeColor(BackColor, ForeColor));
		Buffer[x2, y2].HFlip = false;
		Buffer[x2, y2].VFlip = false;
	}

	public void ThickSingleBox(int x1, int y1, int x2, int y2, ushort Color)
	{
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (x2 >= Width)
		{
			x2 = Width - 1;
		}
		if (y2 >= Height)
		{
			y2 = Height - 1;
		}
		if (y2 >= 0 && x2 >= 0 && x1 < Width && y1 < Height)
		{
			for (int i = x1 + 1; i < x2; i++)
			{
				Buffer[i, y1].Char = 'ß';
				Buffer[i, y1].SetColorsFromOldCharCode(Color);
				Buffer[i, y1].HFlip = false;
				Buffer[i, y1].VFlip = false;
				Buffer[i, y2].Char = 'Ü';
				Buffer[i, y2].SetColorsFromOldCharCode(Color);
				Buffer[i, y2].HFlip = false;
				Buffer[i, y2].VFlip = false;
			}
			for (int j = y1 + 1; j < y2; j++)
			{
				Buffer[x1, j].Char = 'Ý';
				Buffer[x1, j].SetColorsFromOldCharCode(Color);
				Buffer[x1, j].HFlip = false;
				Buffer[x1, j].VFlip = false;
				Buffer[x2, j].Char = 'Þ';
				Buffer[x2, j].SetColorsFromOldCharCode(Color);
				Buffer[x2, j].HFlip = false;
				Buffer[x2, j].VFlip = false;
			}
			Buffer[x1, y1].Char = 'Û';
			Buffer[x1, y1].SetColorsFromOldCharCode(Color);
			Buffer[x1, y1].HFlip = false;
			Buffer[x1, y1].VFlip = false;
			Buffer[x2, y1].Char = 'Û';
			Buffer[x2, y1].SetColorsFromOldCharCode(Color);
			Buffer[x2, y1].HFlip = false;
			Buffer[x2, y1].VFlip = false;
			Buffer[x1, y2].Char = 'Û';
			Buffer[x1, y2].SetColorsFromOldCharCode(Color);
			Buffer[x1, y2].HFlip = false;
			Buffer[x1, y2].VFlip = false;
			Buffer[x2, y2].Char = 'Û';
			Buffer[x2, y2].SetColorsFromOldCharCode(Color);
			Buffer[x2, y2].HFlip = false;
			Buffer[x2, y2].VFlip = false;
		}
	}

	public void SingleBox(int x1, int y1, int x2, int y2, ushort Color)
	{
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (x2 >= Width)
		{
			x2 = Width - 1;
		}
		if (y2 >= Height)
		{
			y2 = Height - 1;
		}
		if (y2 < 0 || x2 < 0 || x1 >= Width || y1 >= Height)
		{
			return;
		}
		for (int i = x1 + 1; i < x2; i++)
		{
			if (i >= 0 && i < 80 && y1 >= 0 && y1 < 25)
			{
				Buffer[i, y1].Char = 'Ä';
				Buffer[i, y1].SetColorsFromOldCharCode(Color);
				Buffer[i, y1].HFlip = false;
				Buffer[i, y1].VFlip = false;
			}
			if (i >= 0 && i < 80 && y2 >= 0 && y2 < 25)
			{
				Buffer[i, y2].Char = 'Ä';
				Buffer[i, y2].SetColorsFromOldCharCode(Color);
				Buffer[i, y2].HFlip = false;
				Buffer[i, y2].VFlip = false;
			}
		}
		for (int j = y1 + 1; j < y2; j++)
		{
			if (x1 >= 0 && x1 < 80 && j >= 0 && j < 25)
			{
				Buffer[x1, j].Char = '³';
				Buffer[x1, j].SetColorsFromOldCharCode(Color);
				Buffer[x1, j].HFlip = false;
				Buffer[x1, j].VFlip = false;
			}
			if (x2 >= 0 && x2 < 80 && j >= 0 && j < 25)
			{
				Buffer[x2, j].Char = '³';
				Buffer[x2, j].SetColorsFromOldCharCode(Color);
				Buffer[x2, j].HFlip = false;
				Buffer[x2, j].VFlip = false;
			}
		}
		if (x1 >= 0 && x1 < 80 && y1 >= 0 && y1 < 25)
		{
			Buffer[x1, y1].Char = 'Ú';
			Buffer[x1, y1].SetColorsFromOldCharCode(Color);
			Buffer[x1, y1].HFlip = false;
			Buffer[x1, y1].VFlip = false;
		}
		if (x2 >= 0 && x2 < 80 && y1 >= 0 && y1 < 25)
		{
			Buffer[x2, y1].Char = '¿';
			Buffer[x2, y1].SetColorsFromOldCharCode(Color);
			Buffer[x2, y1].HFlip = false;
			Buffer[x2, y1].VFlip = false;
		}
		if (x1 >= 0 && x1 < 80 && y2 >= 0 && y2 < 25)
		{
			Buffer[x1, y2].Char = 'À';
			Buffer[x1, y2].SetColorsFromOldCharCode(Color);
			Buffer[x1, y2].HFlip = false;
			Buffer[x1, y2].VFlip = false;
		}
		if (x2 >= 0 && x2 < 80 && y2 >= 0 && y2 < 25)
		{
			Buffer[x2, y2].Char = 'Ù';
			Buffer[x2, y2].SetColorsFromOldCharCode(Color);
			Buffer[x2, y2].HFlip = false;
			Buffer[x2, y2].VFlip = false;
		}
	}

	public void DoubleBox(int x1, int y1, int x2, int y2, ushort Color)
	{
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (x2 >= Width)
		{
			x2 = Width - 1;
		}
		if (y2 >= Height)
		{
			y2 = Height - 1;
		}
		if (y2 >= 0 && x2 >= 0 && x1 < Width && y1 < Height)
		{
			for (int i = x1 + 1; i < x2; i++)
			{
				Buffer[i, y1].Char = 'Í';
				Buffer[i, y1].SetColorsFromOldCharCode(Color);
				Buffer[i, y1].HFlip = false;
				Buffer[i, y1].VFlip = false;
				Buffer[i, y2].Char = 'Í';
				Buffer[i, y2].SetColorsFromOldCharCode(Color);
				Buffer[i, y2].HFlip = false;
				Buffer[i, y2].VFlip = false;
			}
			for (int j = y1 + 1; j < y2; j++)
			{
				Buffer[x1, j].Char = 'º';
				Buffer[x1, j].SetColorsFromOldCharCode(Color);
				Buffer[x1, y1].HFlip = false;
				Buffer[x1, y1].VFlip = false;
				Buffer[x2, j].Char = 'º';
				Buffer[x2, j].SetColorsFromOldCharCode(Color);
				Buffer[x2, y1].HFlip = false;
				Buffer[x2, y1].VFlip = false;
			}
			Buffer[x1, y1].Char = 'É';
			Buffer[x1, y1].SetColorsFromOldCharCode(Color);
			Buffer[x1, y1].HFlip = false;
			Buffer[x1, y1].VFlip = false;
			Buffer[x2, y1].Char = '»';
			Buffer[x2, y1].SetColorsFromOldCharCode(Color);
			Buffer[x2, y1].HFlip = false;
			Buffer[x2, y1].VFlip = false;
			Buffer[x1, y2].Char = 'È';
			Buffer[x1, y2].SetColorsFromOldCharCode(Color);
			Buffer[x1, y2].HFlip = false;
			Buffer[x1, y2].VFlip = false;
			Buffer[x2, y2].Char = '¼';
			Buffer[x2, y2].SetColorsFromOldCharCode(Color);
			Buffer[x2, y2].HFlip = false;
			Buffer[x2, y2].VFlip = false;
		}
	}

	public void Constrain(ref int X, ref int Y)
	{
		if (X < 0)
		{
			X = 0;
		}
		else if (X >= Width)
		{
			X = Width - 1;
		}
		if (Y < 0)
		{
			Y = 0;
		}
		else if (Y >= Height)
		{
			Y = Height - 1;
		}
	}

	public void Constrain(ref int Left, ref int Right, ref int Top, ref int Bottom)
	{
		if (Left < 0)
		{
			Left = 0;
		}
		if (Right >= Width)
		{
			Right = Width - 1;
		}
		if (Top < 0)
		{
			Top = 0;
		}
		if (Bottom >= Height)
		{
			Bottom = Height - 1;
		}
	}

	public static void ClearImposterSuppression()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 80; j++)
			{
				ImposterSuppression[j, i] = false;
			}
		}
	}
}
