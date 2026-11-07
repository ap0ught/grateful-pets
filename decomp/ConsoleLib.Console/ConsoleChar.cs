using System;
using System.Collections.Generic;
using UnityEngine;
using XRL;
using XRL.World;

namespace ConsoleLib.Console;

public class ConsoleChar
{
	public class IndexedProperty<TIndex, TValue>
	{
		private readonly Action<TIndex, TValue> SetAction;

		private readonly Func<TIndex, TValue> GetFunc;

		public TValue this[TIndex i]
		{
			get
			{
				return GetFunc(i);
			}
			set
			{
				SetAction(i, value);
			}
		}

		public IndexedProperty(Func<TIndex, TValue> getFunc, Action<TIndex, TValue> setAction)
		{
			GetFunc = getFunc;
			SetAction = setAction;
		}
	}

	[Obsolete("Use TileForeground instead.")]
	public IndexedProperty<int, Color> TileLayerForeground;

	[Obsolete("Use TileBackground instead")]
	public IndexedProperty<int, Color> TileLayerBackground;

	[Obsolete("Use Tile instead")]
	public IndexedProperty<int, string> TileLayer;

	public Color _Foreground = Color.grey;

	public Color _Background = Color.black;

	public Color _TileForeground = Color.grey;

	public Color _TileBackground = Color.black;

	public Color _Detail = Color.magenta;

	public bool HFlip;

	public bool VFlip;

	public bool BackdropBleedthrough;

	public string WantsBackdrop;

	public string _Tile;

	public char BackupChar;

	public char _Char;

	public ImposterExtra imposterExtra;

	public SoundExtra soundExtra;

	public List<IConsoleCharExtra> extras;

	private Dictionary<Type, IConsoleCharExtra> extraMap;

	public Color TileForeground
	{
		get
		{
			return _TileForeground;
		}
		set
		{
			_TileForeground = value;
		}
	}

	public Color TileBackground
	{
		get
		{
			return _TileBackground;
		}
		set
		{
			_TileBackground = value;
		}
	}

	public string Tile
	{
		get
		{
			return _Tile;
		}
		set
		{
			Char = '\0';
			_Tile = value;
			imposterExtra.Clear(overtyping: true);
			soundExtra.Clear(overtyping: true);
		}
	}

	public char Char
	{
		get
		{
			return _Char;
		}
		set
		{
			if (value != '\0')
			{
				HFlip = false;
				VFlip = false;
				imposterExtra.Clear(overtyping: true);
				soundExtra.Clear(overtyping: true);
			}
			_Char = value;
		}
	}

	public ushort Attributes
	{
		[Obsolete("SetColorsFromOldCharCode(value) instead")]
		set
		{
			SetColorsFromOldCharCode(value);
		}
	}

	public Color Foreground
	{
		get
		{
			return _Foreground;
		}
		set
		{
			_Foreground = value;
		}
	}

	public Color Detail
	{
		get
		{
			return _Detail;
		}
		set
		{
			_Detail = value;
		}
	}

	public Color Background
	{
		get
		{
			return _Background;
		}
		set
		{
			SetBackground(value);
		}
	}

	public char ForegroundCode
	{
		get
		{
			if (!ColorUtility.ColorToCharMap.TryGetValue(_Foreground, out var value))
			{
				return 'k';
			}
			return value;
		}
	}

	public char BackgroundCode
	{
		get
		{
			if (!ColorUtility.ColorToCharMap.TryGetValue(_Background, out var value))
			{
				return 'k';
			}
			return value;
		}
	}

	public char DetailCode
	{
		get
		{
			if (!ColorUtility.ColorToCharMap.TryGetValue(_Detail, out var value))
			{
				return 'k';
			}
			return value;
		}
	}

	public ConsoleChar()
	{
		requireExtra<ImposterExtra>();
		requireExtra<SoundExtra>();
		Clear();
		TileLayerForeground = new IndexedProperty<int, Color>((int i) => TileForeground, (int i, Color v) =>
		{
			TileForeground = v;
		});
		TileLayerBackground = new IndexedProperty<int, Color>((int i) => Detail, (int i, Color v) =>
		{
			Detail = v;
		});
		TileLayer = new IndexedProperty<int, string>((int i) => Tile, (int i, string v) =>
		{
			Tile = v;
		});
	}

	public static Color GetColor(char code)
	{
		if (!ColorUtility.ColorMap.TryGetValue(code, out var value))
		{
			MetricsManager.LogError("unknown color code " + code);
			return Color.magenta;
		}
		return value;
	}

	public static Color GetColor(string code)
	{
		if (!ColorUtility.ColorAliasMap.TryGetValue(code, out var value))
		{
			MetricsManager.LogError("unknown color code " + code);
			return Color.magenta;
		}
		return value;
	}

	public void Clear()
	{
		_Char = ' ';
		_Tile = null;
		ColorUtility.ColorCollection color = The.Color;
		_TileForeground = (_Foreground = color.DefaultForeground);
		_TileBackground = (_Background = color.DefaultBackground);
		_Detail = color.DefaultDetail;
		HFlip = false;
		VFlip = false;
		BackdropBleedthrough = false;
		WantsBackdrop = null;
		if (extras != null)
		{
			for (int i = 0; i < extras.Count; i++)
			{
				extras[i].Clear();
			}
		}
	}

	public override bool Equals(object obj)
	{
		ConsoleChar consoleChar = obj as ConsoleChar;
		if (consoleChar == null)
		{
			return false;
		}
		if (consoleChar.Char != Char)
		{
			return false;
		}
		if (consoleChar._Tile != _Tile)
		{
			return false;
		}
		if (consoleChar._Foreground != _Foreground)
		{
			return false;
		}
		if (consoleChar._Background != _Background)
		{
			return false;
		}
		if (consoleChar._Detail != _Detail)
		{
			return false;
		}
		if (consoleChar._TileBackground != _TileBackground)
		{
			return false;
		}
		if (consoleChar._TileForeground != _TileForeground)
		{
			return false;
		}
		if (consoleChar.HFlip != HFlip)
		{
			return false;
		}
		if (consoleChar.VFlip != VFlip)
		{
			return false;
		}
		if (consoleChar.BackdropBleedthrough != BackdropBleedthrough)
		{
			return false;
		}
		if (consoleChar.WantsBackdrop != WantsBackdrop)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return new { Char, _Foreground, _Background, _Detail, _TileBackground, _TileForeground }.GetHashCode();
	}

	public static bool operator ==(ConsoleChar a, ConsoleChar b)
	{
		if ((object)a == null)
		{
			if ((object)b == null)
			{
				return true;
			}
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(ConsoleChar a, ConsoleChar b)
	{
		return !(a == b);
	}

	public ConsoleChar GetCopy()
	{
		ConsoleChar consoleChar = new ConsoleChar();
		consoleChar.Copy(this);
		return consoleChar;
	}

	public void Copy(ConsoleChar C)
	{
		_Char = C._Char;
		_Tile = C._Tile;
		_Foreground = C._Foreground;
		_Background = C._Background;
		_TileForeground = C._TileForeground;
		_TileBackground = C._TileBackground;
		_Detail = C._Detail;
		HFlip = C.HFlip;
		VFlip = C.VFlip;
		BackdropBleedthrough = C.BackdropBleedthrough;
		WantsBackdrop = C.WantsBackdrop;
		soundExtra.CopyFrom(C.soundExtra);
		imposterExtra.CopyFrom(C.imposterExtra);
	}

	public void SetColorsFromOldCharCode(ushort color)
	{
		foreach (KeyValuePair<ushort, char> item in ColorUtility.ColorAttributeToCharMap)
		{
			if ((color & item.Key) == item.Key)
			{
				Foreground = ColorUtility.colorFromChar(item.Value);
			}
			if ((color & (ushort)(item.Key << 5)) == (ushort)(item.Key << 5))
			{
				Background = ColorUtility.colorFromChar(item.Value);
			}
		}
	}

	public void Set(RenderEvent Event)
	{
		_Tile = Event.Tile;
		Foreground = Event.GetForegroundColor();
		Background = Event.GetBackgroundColor();
		Detail = Event.GetDetailColor();
		WantsBackdrop = Event.WantsBackdrop;
		BackdropBleedthrough = Event.BackdropBleedthrough;
		List<ImposterExtra.ImposterInfo> imposters = imposterExtra.imposters;
		imposters.Clear();
		soundExtra.Clear(overtyping: true);
		if (_Tile != null)
		{
			_Char = '\0';
			BackupChar = Event.RenderString[0];
			HFlip = Event.HFlip;
			VFlip = Event.VFlip;
			if (Event.Imposters.Count > 0)
			{
				imposters.AddRange(Event.Imposters);
			}
		}
		else
		{
			_Char = Event.RenderString[0];
			BackupChar = '\0';
			HFlip = false;
			VFlip = false;
		}
		if (!Event.DisableFullscreenColorEffects && Event.Lit == LightLevel.Darkvision)
		{
			_Background = new Color(_Background.r, _Background.g + 0.025f, _Background.b);
		}
	}

	public void SetDetail(char colorCode)
	{
		_Detail = ColorUtility.ColorMap[colorCode];
	}

	public void SetDetail(Color color)
	{
		_Detail = color;
	}

	public void SetForeground(char colorCode)
	{
		_Foreground = ColorUtility.ColorMap[colorCode];
	}

	public void SetForeground(Color color)
	{
		_Foreground = color;
	}

	public void SetBackground(Color color)
	{
		_Background = color;
	}

	public void SetBackground(char colorCode)
	{
		_Background = ColorUtility.ColorMap[colorCode];
	}

	public void SetColors(string Color)
	{
		char key = '\0';
		bool flag = true;
		bool flag2 = true;
		Dictionary<char, Color> colorMap = ColorUtility.ColorMap;
		int num = Color.Length - 1;
		while ((num >= 0) & flag & flag2)
		{
			char c = Color[num];
			switch (c)
			{
			case '^':
			{
				if (flag2 && colorMap.TryGetValue(key, out var value2))
				{
					_Background = value2;
					flag2 = false;
				}
				break;
			}
			case '&':
			{
				if (flag && colorMap.TryGetValue(key, out var value))
				{
					_Foreground = value;
					flag = false;
				}
				break;
			}
			}
			key = c;
			num--;
		}
	}

	public void SetColors(RenderEvent E)
	{
		char c = '\0';
		char key = '\0';
		bool flag = true;
		bool flag2 = true;
		Dictionary<char, Color> colorMap = ColorUtility.ColorMap;
		if (E.BackgroundString != null)
		{
			int num = E.BackgroundString.Length - 1;
			while ((num >= 0) & flag2)
			{
				c = E.BackgroundString[num];
				if (c == '^' && colorMap.TryGetValue(key, out var value))
				{
					_Background = value;
					flag2 = false;
					break;
				}
				key = c;
				num--;
			}
			key = '0';
		}
		if (E.ColorString == null)
		{
			return;
		}
		int num2 = E.ColorString.Length - 1;
		while (num2 >= 0 && (flag | flag2))
		{
			c = E.ColorString[num2];
			switch (c)
			{
			case '^':
			{
				if (flag2 && colorMap.TryGetValue(key, out var value3))
				{
					_Background = value3;
					flag2 = false;
				}
				break;
			}
			case '&':
			{
				if (flag && colorMap.TryGetValue(key, out var value2))
				{
					_Foreground = value2;
					flag = false;
				}
				break;
			}
			}
			key = c;
			num2--;
		}
	}

	public T requireExtra<T>() where T : IConsoleCharExtra, new()
	{
		if (extras == null)
		{
			extras = new List<IConsoleCharExtra>();
			extraMap = new Dictionary<Type, IConsoleCharExtra>();
		}
		if (extraMap.TryGetValue(typeof(T), out var value))
		{
			return value as T;
		}
		T val = new T();
		extras.Add(val);
		extraMap.Add(typeof(T), val);
		if (typeof(T) == typeof(ImposterExtra))
		{
			imposterExtra = val as ImposterExtra;
		}
		if (typeof(T) == typeof(SoundExtra))
		{
			soundExtra = val as SoundExtra;
		}
		return val;
	}

	public void BeforeRender(int x, int y, ex3DSprite2 sprite, ScreenBuffer buffer)
	{
		if (extras != null)
		{
			for (int i = 0; i < extras.Count; i++)
			{
				extras[i].BeforeRender(x, y, this, sprite, buffer);
			}
		}
	}

	public void AfterRender(int x, int y, ex3DSprite2 sprite, ScreenBuffer buffer)
	{
		if (extras != null)
		{
			for (int i = 0; i < extras.Count; i++)
			{
				extras[i].AfterRender(x, y, this, sprite, buffer);
			}
		}
	}
}
