using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using XRL.Core;
using XRL.UI;

namespace ConsoleLib.Console;

public static class Keyboard
{
	public class XRLKeyEvent
	{
		public UnityEngine.KeyCode keyCode;

		public char character;

		public bool shift;

		public bool control;

		public bool alt;

		public bool allowmap = true;

		public XRLKeyEvent(Event ke)
		{
			keyCode = ke.keyCode;
			character = MapKeyToChar(ke);
			shift = ke.shift || Input.GetKey(UnityEngine.KeyCode.RightShift) || Input.GetKey(UnityEngine.KeyCode.LeftShift);
			control = ke.control || Input.GetKey(UnityEngine.KeyCode.RightControl) || Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightMeta) || Input.GetKey(UnityEngine.KeyCode.LeftMeta);
			alt = ke.alt;
		}

		public XRLKeyEvent(UnityEngine.KeyCode code, char c = '\0', bool bShift = false, bool bControl = false, bool bAlt = false)
		{
			keyCode = code;
			character = c;
			shift = bShift;
			control = bControl;
			alt = bAlt;
		}

		public XRLKeyEvent(XRLKeyEvent ke)
		{
			keyCode = ke.keyCode;
			character = ke.character;
			shift = ke.shift;
			control = ke.control;
			alt = ke.alt;
		}

		public XRLKeyEvent()
		{
			keyCode = UnityEngine.KeyCode.None;
			character = '\0';
			shift = false;
			control = false;
			alt = false;
		}

		public override string ToString()
		{
			return (alt ? "Alt+" : "") + (control ? "Control+" : "") + (shift ? "Shift+" : "") + keyCode;
		}
	}

	public class MouseEvent
	{
		public object Data;

		public string Event;

		public int x;

		public int y;

		public Vector2 Vector2 => new Vector2(x, y);

		public MouseEvent(string e, int x, int y, object data)
		{
			Event = e ?? "";
			this.x = x;
			this.y = y;
			Data = data;
		}

		public MouseEvent(string e)
			: this(e, 0, 0)
		{
		}

		public MouseEvent(string e, int x, int y)
			: this(e, x, y, null)
		{
		}

		public void CopyFrom(MouseEvent source)
		{
			Data = source?.Data;
			Event = source?.Event ?? "";
			x = source?.x ?? int.MinValue;
			y = source?.y ?? int.MinValue;
		}
	}

	public static CleanQueue<XRLKeyEvent> KeyQueue = new CleanQueue<XRLKeyEvent>();

	public static ManualResetEvent KeyEvent = new ManualResetEvent(initialState: false);

	public static bool Closed = false;

	public static Dictionary<Keys, UnityEngine.KeyCode> ReverseKeymap = null;

	public static Dictionary<UnityEngine.KeyCode, Keys> Keymap = null;

	public static Dictionary<UnityEngine.KeyCode, char> lccharmap = null;

	public static Dictionary<UnityEngine.KeyCode, char> uccharmap = null;

	public static Dictionary<char, List<UnityEngine.KeyCode>> reverselccharmap = null;

	public static Dictionary<char, List<UnityEngine.KeyCode>> reverseuccharmap = null;

	private static Dictionary<string, UnityEngine.KeyCode> parseUnityEngineKeyCodeCache = new Dictionary<string, UnityEngine.KeyCode>();

	public static readonly string COMMAND_EVENT_PREFIX = "Command:";

	private static Queue<MouseEvent> MouseEventQueue = new Queue<MouseEvent>();

	private static Queue<MouseEvent> MouseEventUsed = new Queue<MouseEvent>();

	private static Queue<MouseEvent> MouseEventPool = new Queue<MouseEvent>();

	public static Queue<MouseEvent> tempMouseEventQueue = new Queue<MouseEvent>();

	public static bool _bCtrl;

	public static bool _bShift;

	public static bool _bAlt;

	public static Keys vkCode = Keys.None;

	public static Keys RawCode = Keys.None;

	public static int Char = 0;

	public static int MetaKey = 0;

	public static int nRepeat = 0;

	public static MouseEvent CurrentMouseEvent = new MouseEvent("", 0, 0);

	public static Dictionary<string, Keys> metaMousecommands = new Dictionary<string, Keys>
	{
		{
			"Meta:NavigateNorth",
			Keys.NumPad8
		},
		{
			"Meta:NavigateN",
			Keys.NumPad8
		},
		{
			"Meta:NavigateSouth",
			Keys.NumPad2
		},
		{
			"Meta:NavigateS",
			Keys.NumPad2
		},
		{
			"Meta:NavigateEast",
			Keys.NumPad6
		},
		{
			"Meta:NavigateE",
			Keys.NumPad6
		},
		{
			"Meta:NavigateWest",
			Keys.NumPad4
		},
		{
			"Meta:NavigateW",
			Keys.NumPad4
		},
		{
			"Meta:NavigateNorthwest",
			Keys.NumPad7
		},
		{
			"Meta:NavigateNW",
			Keys.NumPad7
		},
		{
			"Meta:NavigateNortheast",
			Keys.NumPad9
		},
		{
			"Meta:NavigateNE",
			Keys.NumPad9
		},
		{
			"Meta:NavigateSouthwest",
			Keys.NumPad1
		},
		{
			"Meta:NavigateSW",
			Keys.NumPad1
		},
		{
			"Meta:NavigateSoutheast",
			Keys.NumPad3
		},
		{
			"Command:Page Left",
			Keys.NumPad7
		},
		{
			"Command:Page Right",
			Keys.NumPad9
		},
		{
			"Meta:NavigateSE",
			Keys.NumPad3
		},
		{
			"Meta:NavigateUp",
			Keys.Subtract
		},
		{
			"Meta:NavigateDown",
			Keys.Add
		},
		{
			"Meta:CharacterSheet",
			Keys.Tab
		},
		{
			"Meta:Walk",
			Keys.W
		},
		{
			"Meta:Abilities",
			Keys.A
		},
		{
			"Meta:Fire",
			Keys.F
		},
		{
			"Meta:Use",
			Keys.Space
		},
		{
			"Meta:Throw",
			Keys.T
		},
		{
			"Meta:Get",
			Keys.G
		},
		{
			"Meta:Autoexplore",
			Keys.NumPad0
		},
		{
			"Meta:Wait",
			Keys.NumPad5
		},
		{
			"Meta:Rest",
			Keys.Oemtilde
		},
		{
			"Meta:Accept",
			Keys.Enter
		},
		{
			"Meta:Cancel",
			Keys.Escape
		},
		{
			"Meta:System",
			Keys.Escape
		}
	};

	private static Dictionary<int, string> metaToString = new Dictionary<int, string>();

	public static bool bCtrl
	{
		get
		{
			return _bCtrl;
		}
		set
		{
			_bCtrl = value;
		}
	}

	public static bool bShift
	{
		get
		{
			return _bShift;
		}
		set
		{
			_bShift = value;
		}
	}

	public static bool bAlt
	{
		get
		{
			return _bAlt;
		}
		set
		{
			_bAlt = value;
		}
	}

	public static void Init()
	{
	}

	public static void ResetIfQueuesEmpty()
	{
		lock (MouseEventQueue)
		{
			lock (KeyQueue)
			{
				if (MouseEventQueue.Count == 0 && KeyQueue.Count == 0)
				{
					KeyEvent.Reset();
				}
			}
		}
	}

	public static char MapKeyToChar(Event ev)
	{
		if (Keymap == null)
		{
			InitKeymap();
		}
		if (ev.shift)
		{
			if (!uccharmap.ContainsKey(ev.keyCode))
			{
				return '\0';
			}
			return uccharmap[ev.keyCode];
		}
		if (!lccharmap.ContainsKey(ev.keyCode))
		{
			return '\0';
		}
		return lccharmap[ev.keyCode];
	}

	public static Keys ConvertKeyCodeToKeys(UnityEngine.KeyCode code)
	{
		if (!Keymap.TryGetValue(code, out var value))
		{
			return Keys.None;
		}
		return value;
	}

	public static char UcKeycodeMapper(UnityEngine.KeyCode code)
	{
		if (!uccharmap.ContainsKey(code))
		{
			return '\0';
		}
		return uccharmap[code];
	}

	public static UnityEngine.KeyCode ParseUnityEngineKeyCode(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return UnityEngine.KeyCode.None;
		}
		if (!parseUnityEngineKeyCodeCache.TryGetValue(key, out var value))
		{
			value = (UnityEngine.KeyCode)Enum.Parse(typeof(UnityEngine.KeyCode), key);
			parseUnityEngineKeyCodeCache.Add(key, value);
		}
		return value;
	}

	public static char ConvertKeycodeToLowercaseChar(UnityEngine.KeyCode code)
	{
		if (lccharmap == null)
		{
			InitKeymap();
		}
		if (lccharmap.TryGetValue(code, out var value))
		{
			return value;
		}
		return '\0';
	}

	public static void InitKeymap()
	{
		if (Keymap != null)
		{
			return;
		}
		Keymap = new Dictionary<UnityEngine.KeyCode, Keys>();
		ReverseKeymap = new Dictionary<Keys, UnityEngine.KeyCode>();
		lccharmap = new Dictionary<UnityEngine.KeyCode, char>();
		uccharmap = new Dictionary<UnityEngine.KeyCode, char>();
		reverselccharmap = new Dictionary<char, List<UnityEngine.KeyCode>>();
		reverseuccharmap = new Dictionary<char, List<UnityEngine.KeyCode>>();
		Keymap.Add(UnityEngine.KeyCode.Escape, Keys.Escape);
		Keymap.Add(UnityEngine.KeyCode.LeftWindows, Keys.LWin);
		Keymap.Add(UnityEngine.KeyCode.RightWindows, Keys.RWin);
		Keymap.Add(UnityEngine.KeyCode.Menu, Keys.Menu);
		Keymap.Add(UnityEngine.KeyCode.Numlock, Keys.NumLock);
		Keymap.Add(UnityEngine.KeyCode.F1, Keys.F1);
		Keymap.Add(UnityEngine.KeyCode.F2, Keys.F2);
		Keymap.Add(UnityEngine.KeyCode.F3, Keys.F3);
		Keymap.Add(UnityEngine.KeyCode.F4, Keys.F4);
		Keymap.Add(UnityEngine.KeyCode.F5, Keys.F5);
		Keymap.Add(UnityEngine.KeyCode.F6, Keys.F6);
		Keymap.Add(UnityEngine.KeyCode.F7, Keys.F7);
		Keymap.Add(UnityEngine.KeyCode.F8, Keys.F8);
		Keymap.Add(UnityEngine.KeyCode.F9, Keys.F9);
		Keymap.Add(UnityEngine.KeyCode.F10, Keys.F10);
		Keymap.Add(UnityEngine.KeyCode.F11, Keys.F11);
		Keymap.Add(UnityEngine.KeyCode.F12, Keys.F12);
		Keymap.Add(UnityEngine.KeyCode.F13, Keys.F13);
		Keymap.Add(UnityEngine.KeyCode.F14, Keys.F14);
		Keymap.Add(UnityEngine.KeyCode.F15, Keys.F15);
		Keymap.Add(UnityEngine.KeyCode.Help, Keys.F1);
		Keymap.Add(UnityEngine.KeyCode.Break, Keys.Pause);
		Keymap.Add(UnityEngine.KeyCode.Mouse0, Keys.XButton1);
		Keymap.Add(UnityEngine.KeyCode.Mouse1, Keys.XButton2);
		Keymap.Add(UnityEngine.KeyCode.Mouse2, Keys.MButton);
		Keymap.Add(UnityEngine.KeyCode.Joystick1Button0, Keys.XButton1);
		Keymap.Add(UnityEngine.KeyCode.Joystick1Button1, Keys.XButton2);
		Keymap.Add(UnityEngine.KeyCode.Joystick1Button2, Keys.MButton);
		Keymap.Add(UnityEngine.KeyCode.Print, Keys.PrintScreen);
		Keymap.Add(UnityEngine.KeyCode.ScrollLock, Keys.Scroll);
		Keymap.Add(UnityEngine.KeyCode.Pause, Keys.Pause);
		Keymap.Add(UnityEngine.KeyCode.BackQuote, Keys.Oemtilde);
		lccharmap.Add(UnityEngine.KeyCode.BackQuote, '~');
		uccharmap.Add(UnityEngine.KeyCode.BackQuote, '`');
		Keymap.Add(UnityEngine.KeyCode.Alpha0, Keys.D0);
		lccharmap.Add(UnityEngine.KeyCode.Alpha0, '0');
		uccharmap.Add(UnityEngine.KeyCode.Alpha0, ')');
		Keymap.Add(UnityEngine.KeyCode.Alpha1, Keys.D1);
		lccharmap.Add(UnityEngine.KeyCode.Alpha1, '1');
		uccharmap.Add(UnityEngine.KeyCode.Alpha1, '!');
		Keymap.Add(UnityEngine.KeyCode.Alpha2, Keys.D2);
		lccharmap.Add(UnityEngine.KeyCode.Alpha2, '2');
		uccharmap.Add(UnityEngine.KeyCode.Alpha2, '@');
		Keymap.Add(UnityEngine.KeyCode.Alpha3, Keys.D3);
		lccharmap.Add(UnityEngine.KeyCode.Alpha3, '3');
		uccharmap.Add(UnityEngine.KeyCode.Alpha3, '#');
		Keymap.Add(UnityEngine.KeyCode.Alpha4, Keys.D4);
		lccharmap.Add(UnityEngine.KeyCode.Alpha4, '4');
		uccharmap.Add(UnityEngine.KeyCode.Alpha4, '$');
		Keymap.Add(UnityEngine.KeyCode.Alpha5, Keys.D5);
		lccharmap.Add(UnityEngine.KeyCode.Alpha5, '5');
		uccharmap.Add(UnityEngine.KeyCode.Alpha5, '%');
		Keymap.Add(UnityEngine.KeyCode.Alpha6, Keys.D6);
		lccharmap.Add(UnityEngine.KeyCode.Alpha6, '6');
		uccharmap.Add(UnityEngine.KeyCode.Alpha6, '^');
		Keymap.Add(UnityEngine.KeyCode.Alpha7, Keys.D7);
		lccharmap.Add(UnityEngine.KeyCode.Alpha7, '7');
		uccharmap.Add(UnityEngine.KeyCode.Alpha7, '&');
		Keymap.Add(UnityEngine.KeyCode.Alpha8, Keys.D8);
		lccharmap.Add(UnityEngine.KeyCode.Alpha8, '8');
		uccharmap.Add(UnityEngine.KeyCode.Alpha8, '*');
		Keymap.Add(UnityEngine.KeyCode.Alpha9, Keys.D9);
		lccharmap.Add(UnityEngine.KeyCode.Alpha9, '9');
		uccharmap.Add(UnityEngine.KeyCode.Alpha9, '(');
		Keymap.Add(UnityEngine.KeyCode.Minus, Keys.OemMinus);
		lccharmap.Add(UnityEngine.KeyCode.Minus, '-');
		uccharmap.Add(UnityEngine.KeyCode.Minus, '_');
		Keymap.Add(UnityEngine.KeyCode.Plus, Keys.Oemplus);
		lccharmap.Add(UnityEngine.KeyCode.Plus, '+');
		uccharmap.Add(UnityEngine.KeyCode.Plus, '+');
		Keymap.Add(UnityEngine.KeyCode.Delete, Keys.Delete);
		Keymap.Add(UnityEngine.KeyCode.End, Keys.End);
		Keymap.Add(UnityEngine.KeyCode.Home, Keys.Home);
		Keymap.Add(UnityEngine.KeyCode.Insert, Keys.Insert);
		Keymap.Add(UnityEngine.KeyCode.PageUp, Keys.Prior);
		Keymap.Add(UnityEngine.KeyCode.PageDown, Keys.Next);
		Keymap.Add(UnityEngine.KeyCode.Backspace, Keys.Back);
		lccharmap.Add(UnityEngine.KeyCode.Backspace, '\b');
		uccharmap.Add(UnityEngine.KeyCode.Backspace, '\b');
		Keymap.Add(UnityEngine.KeyCode.Tab, Keys.Tab);
		lccharmap.Add(UnityEngine.KeyCode.Tab, '\t');
		uccharmap.Add(UnityEngine.KeyCode.Tab, '\t');
		Keymap.Add(UnityEngine.KeyCode.LeftBracket, Keys.Oem4);
		lccharmap.Add(UnityEngine.KeyCode.LeftBracket, '[');
		uccharmap.Add(UnityEngine.KeyCode.LeftBracket, '{');
		Keymap.Add(UnityEngine.KeyCode.RightBracket, Keys.Oem6);
		lccharmap.Add(UnityEngine.KeyCode.RightBracket, ']');
		uccharmap.Add(UnityEngine.KeyCode.RightBracket, '}');
		Keymap.Add(UnityEngine.KeyCode.Backslash, Keys.Oem102);
		lccharmap.Add(UnityEngine.KeyCode.Backslash, '\\');
		uccharmap.Add(UnityEngine.KeyCode.Backslash, '|');
		Keymap.Add(UnityEngine.KeyCode.CapsLock, Keys.CapsLock);
		Keymap.Add(UnityEngine.KeyCode.Semicolon, Keys.Oem1);
		lccharmap.Add(UnityEngine.KeyCode.Semicolon, ';');
		uccharmap.Add(UnityEngine.KeyCode.Semicolon, ':');
		Keymap.Add(UnityEngine.KeyCode.Quote, Keys.Oem7);
		lccharmap.Add(UnityEngine.KeyCode.Quote, '\'');
		uccharmap.Add(UnityEngine.KeyCode.Quote, '"');
		Keymap.Add(UnityEngine.KeyCode.Comma, Keys.Oemcomma);
		Keymap.Add(UnityEngine.KeyCode.Period, Keys.OemPeriod);
		Keymap.Add(UnityEngine.KeyCode.Question, Keys.OemQuestion);
		lccharmap.Add(UnityEngine.KeyCode.Question, '/');
		uccharmap.Add(UnityEngine.KeyCode.Question, '?');
		Keymap.Add(UnityEngine.KeyCode.Slash, Keys.OemQuestion);
		lccharmap.Add(UnityEngine.KeyCode.Slash, '/');
		uccharmap.Add(UnityEngine.KeyCode.Slash, '?');
		Keymap.Add(UnityEngine.KeyCode.Space, Keys.Space);
		Keymap.Add(UnityEngine.KeyCode.Return, Keys.Enter);
		Keymap.Add(UnityEngine.KeyCode.Keypad0, Keys.NumPad0);
		Keymap.Add(UnityEngine.KeyCode.Keypad1, Keys.NumPad1);
		Keymap.Add(UnityEngine.KeyCode.Keypad2, Keys.NumPad2);
		Keymap.Add(UnityEngine.KeyCode.Keypad3, Keys.NumPad3);
		Keymap.Add(UnityEngine.KeyCode.Keypad4, Keys.NumPad4);
		Keymap.Add(UnityEngine.KeyCode.Keypad5, Keys.NumPad5);
		Keymap.Add(UnityEngine.KeyCode.Keypad6, Keys.NumPad6);
		Keymap.Add(UnityEngine.KeyCode.Keypad7, Keys.NumPad7);
		Keymap.Add(UnityEngine.KeyCode.Keypad8, Keys.NumPad8);
		Keymap.Add(UnityEngine.KeyCode.Keypad9, Keys.NumPad9);
		Keymap.Add(UnityEngine.KeyCode.KeypadDivide, Keys.Divide);
		Keymap.Add(UnityEngine.KeyCode.KeypadMultiply, Keys.Multiply);
		Keymap.Add(UnityEngine.KeyCode.KeypadPlus, Keys.Add);
		Keymap.Add(UnityEngine.KeyCode.KeypadMinus, Keys.Subtract);
		Keymap.Add(UnityEngine.KeyCode.KeypadPeriod, Keys.Decimal);
		lccharmap.Add(UnityEngine.KeyCode.KeypadDivide, '/');
		uccharmap.Add(UnityEngine.KeyCode.KeypadDivide, '/');
		lccharmap.Add(UnityEngine.KeyCode.KeypadMultiply, '*');
		uccharmap.Add(UnityEngine.KeyCode.KeypadMultiply, '*');
		lccharmap.Add(UnityEngine.KeyCode.KeypadPlus, '+');
		uccharmap.Add(UnityEngine.KeyCode.KeypadPlus, '+');
		lccharmap.Add(UnityEngine.KeyCode.KeypadMinus, '-');
		uccharmap.Add(UnityEngine.KeyCode.KeypadMinus, '-');
		lccharmap.Add(UnityEngine.KeyCode.KeypadPeriod, '.');
		uccharmap.Add(UnityEngine.KeyCode.KeypadPeriod, '.');
		Keymap.Add(UnityEngine.KeyCode.LeftArrow, Keys.Left);
		Keymap.Add(UnityEngine.KeyCode.RightArrow, Keys.Right);
		Keymap.Add(UnityEngine.KeyCode.DownArrow, Keys.Down);
		Keymap.Add(UnityEngine.KeyCode.UpArrow, Keys.Up);
		Keymap.Add(UnityEngine.KeyCode.A, Keys.A);
		Keymap.Add(UnityEngine.KeyCode.B, Keys.B);
		Keymap.Add(UnityEngine.KeyCode.C, Keys.C);
		Keymap.Add(UnityEngine.KeyCode.D, Keys.D);
		Keymap.Add(UnityEngine.KeyCode.E, Keys.E);
		Keymap.Add(UnityEngine.KeyCode.F, Keys.F);
		Keymap.Add(UnityEngine.KeyCode.G, Keys.G);
		Keymap.Add(UnityEngine.KeyCode.H, Keys.H);
		Keymap.Add(UnityEngine.KeyCode.I, Keys.I);
		Keymap.Add(UnityEngine.KeyCode.J, Keys.J);
		Keymap.Add(UnityEngine.KeyCode.K, Keys.K);
		Keymap.Add(UnityEngine.KeyCode.L, Keys.L);
		Keymap.Add(UnityEngine.KeyCode.M, Keys.M);
		Keymap.Add(UnityEngine.KeyCode.N, Keys.N);
		Keymap.Add(UnityEngine.KeyCode.O, Keys.O);
		Keymap.Add(UnityEngine.KeyCode.P, Keys.P);
		Keymap.Add(UnityEngine.KeyCode.Q, Keys.Q);
		Keymap.Add(UnityEngine.KeyCode.R, Keys.R);
		Keymap.Add(UnityEngine.KeyCode.S, Keys.S);
		Keymap.Add(UnityEngine.KeyCode.T, Keys.T);
		Keymap.Add(UnityEngine.KeyCode.U, Keys.U);
		Keymap.Add(UnityEngine.KeyCode.V, Keys.V);
		Keymap.Add(UnityEngine.KeyCode.W, Keys.W);
		Keymap.Add(UnityEngine.KeyCode.X, Keys.X);
		Keymap.Add(UnityEngine.KeyCode.Y, Keys.Y);
		Keymap.Add(UnityEngine.KeyCode.Z, Keys.Z);
		Keymap.Add(UnityEngine.KeyCode.None, Keys.None);
		Keymap.Add(UnityEngine.KeyCode.Clear, Keys.Clear);
		Keymap.Add(UnityEngine.KeyCode.Exclaim, Keys.D1);
		Keymap.Add(UnityEngine.KeyCode.DoubleQuote, Keys.Oem7);
		Keymap.Add(UnityEngine.KeyCode.Hash, Keys.D3);
		Keymap.Add(UnityEngine.KeyCode.Dollar, Keys.D4);
		Keymap.Add(UnityEngine.KeyCode.Ampersand, Keys.D7);
		Keymap.Add(UnityEngine.KeyCode.LeftParen, Keys.D9);
		Keymap.Add(UnityEngine.KeyCode.RightParen, Keys.D0);
		Keymap.Add(UnityEngine.KeyCode.Asterisk, Keys.D8);
		Keymap.Add(UnityEngine.KeyCode.Colon, Keys.Oem1);
		Keymap.Add(UnityEngine.KeyCode.Less, Keys.Oemcomma);
		Keymap.Add(UnityEngine.KeyCode.Equals, Keys.Oemplus);
		Keymap.Add(UnityEngine.KeyCode.Greater, Keys.OemPeriod);
		Keymap.Add(UnityEngine.KeyCode.At, Keys.D2);
		Keymap.Add(UnityEngine.KeyCode.Caret, Keys.D6);
		Keymap.Add(UnityEngine.KeyCode.Underscore, Keys.OemMinus);
		Keymap.Add(UnityEngine.KeyCode.KeypadEnter, Keys.Enter);
		Keymap.Add(UnityEngine.KeyCode.KeypadEquals, Keys.Oemplus);
		lccharmap.Add(UnityEngine.KeyCode.KeypadEquals, '=');
		uccharmap.Add(UnityEngine.KeyCode.KeypadEquals, '=');
		lccharmap.Add(UnityEngine.KeyCode.Underscore, '_');
		uccharmap.Add(UnityEngine.KeyCode.Underscore, '_');
		lccharmap.Add(UnityEngine.KeyCode.Caret, '^');
		uccharmap.Add(UnityEngine.KeyCode.Caret, '^');
		lccharmap.Add(UnityEngine.KeyCode.At, '@');
		uccharmap.Add(UnityEngine.KeyCode.At, '@');
		lccharmap.Add(UnityEngine.KeyCode.Greater, '>');
		uccharmap.Add(UnityEngine.KeyCode.Greater, '>');
		lccharmap.Add(UnityEngine.KeyCode.Equals, '=');
		uccharmap.Add(UnityEngine.KeyCode.Equals, '=');
		lccharmap.Add(UnityEngine.KeyCode.Less, '<');
		uccharmap.Add(UnityEngine.KeyCode.Less, '<');
		lccharmap.Add(UnityEngine.KeyCode.Colon, ':');
		uccharmap.Add(UnityEngine.KeyCode.Colon, ':');
		lccharmap.Add(UnityEngine.KeyCode.Asterisk, '*');
		uccharmap.Add(UnityEngine.KeyCode.Asterisk, '*');
		lccharmap.Add(UnityEngine.KeyCode.RightParen, ')');
		uccharmap.Add(UnityEngine.KeyCode.RightParen, ')');
		lccharmap.Add(UnityEngine.KeyCode.LeftParen, '(');
		uccharmap.Add(UnityEngine.KeyCode.LeftParen, '(');
		lccharmap.Add(UnityEngine.KeyCode.Ampersand, '&');
		uccharmap.Add(UnityEngine.KeyCode.Ampersand, '&');
		lccharmap.Add(UnityEngine.KeyCode.Dollar, '$');
		uccharmap.Add(UnityEngine.KeyCode.Dollar, '$');
		lccharmap.Add(UnityEngine.KeyCode.Hash, '#');
		uccharmap.Add(UnityEngine.KeyCode.Hash, '#');
		lccharmap.Add(UnityEngine.KeyCode.Exclaim, '!');
		uccharmap.Add(UnityEngine.KeyCode.Exclaim, '!');
		lccharmap.Add(UnityEngine.KeyCode.Space, ' ');
		uccharmap.Add(UnityEngine.KeyCode.Space, ' ');
		lccharmap.Add(UnityEngine.KeyCode.KeypadEnter, '\r');
		uccharmap.Add(UnityEngine.KeyCode.KeypadEnter, '\r');
		lccharmap.Add(UnityEngine.KeyCode.Return, '\r');
		uccharmap.Add(UnityEngine.KeyCode.Return, '\r');
		lccharmap.Add(UnityEngine.KeyCode.DoubleQuote, '"');
		uccharmap.Add(UnityEngine.KeyCode.DoubleQuote, '"');
		lccharmap.Add(UnityEngine.KeyCode.A, 'a');
		uccharmap.Add(UnityEngine.KeyCode.A, 'A');
		lccharmap.Add(UnityEngine.KeyCode.B, 'b');
		uccharmap.Add(UnityEngine.KeyCode.B, 'B');
		lccharmap.Add(UnityEngine.KeyCode.C, 'c');
		uccharmap.Add(UnityEngine.KeyCode.C, 'C');
		lccharmap.Add(UnityEngine.KeyCode.D, 'd');
		uccharmap.Add(UnityEngine.KeyCode.D, 'D');
		lccharmap.Add(UnityEngine.KeyCode.E, 'e');
		uccharmap.Add(UnityEngine.KeyCode.E, 'E');
		lccharmap.Add(UnityEngine.KeyCode.F, 'f');
		uccharmap.Add(UnityEngine.KeyCode.F, 'F');
		lccharmap.Add(UnityEngine.KeyCode.G, 'g');
		uccharmap.Add(UnityEngine.KeyCode.G, 'G');
		lccharmap.Add(UnityEngine.KeyCode.H, 'h');
		uccharmap.Add(UnityEngine.KeyCode.H, 'H');
		lccharmap.Add(UnityEngine.KeyCode.I, 'i');
		uccharmap.Add(UnityEngine.KeyCode.I, 'I');
		lccharmap.Add(UnityEngine.KeyCode.J, 'j');
		uccharmap.Add(UnityEngine.KeyCode.J, 'J');
		lccharmap.Add(UnityEngine.KeyCode.K, 'k');
		uccharmap.Add(UnityEngine.KeyCode.K, 'K');
		lccharmap.Add(UnityEngine.KeyCode.L, 'l');
		uccharmap.Add(UnityEngine.KeyCode.L, 'L');
		lccharmap.Add(UnityEngine.KeyCode.M, 'm');
		uccharmap.Add(UnityEngine.KeyCode.M, 'M');
		lccharmap.Add(UnityEngine.KeyCode.N, 'n');
		uccharmap.Add(UnityEngine.KeyCode.N, 'N');
		lccharmap.Add(UnityEngine.KeyCode.O, 'o');
		uccharmap.Add(UnityEngine.KeyCode.O, 'O');
		lccharmap.Add(UnityEngine.KeyCode.P, 'p');
		uccharmap.Add(UnityEngine.KeyCode.P, 'P');
		lccharmap.Add(UnityEngine.KeyCode.Q, 'q');
		uccharmap.Add(UnityEngine.KeyCode.Q, 'Q');
		lccharmap.Add(UnityEngine.KeyCode.R, 'r');
		uccharmap.Add(UnityEngine.KeyCode.R, 'R');
		lccharmap.Add(UnityEngine.KeyCode.S, 's');
		uccharmap.Add(UnityEngine.KeyCode.S, 'S');
		lccharmap.Add(UnityEngine.KeyCode.T, 't');
		uccharmap.Add(UnityEngine.KeyCode.T, 'T');
		lccharmap.Add(UnityEngine.KeyCode.U, 'u');
		uccharmap.Add(UnityEngine.KeyCode.U, 'U');
		lccharmap.Add(UnityEngine.KeyCode.V, 'v');
		uccharmap.Add(UnityEngine.KeyCode.V, 'V');
		lccharmap.Add(UnityEngine.KeyCode.W, 'w');
		uccharmap.Add(UnityEngine.KeyCode.W, 'W');
		lccharmap.Add(UnityEngine.KeyCode.X, 'x');
		uccharmap.Add(UnityEngine.KeyCode.X, 'X');
		lccharmap.Add(UnityEngine.KeyCode.Y, 'y');
		uccharmap.Add(UnityEngine.KeyCode.Y, 'Y');
		lccharmap.Add(UnityEngine.KeyCode.Z, 'z');
		uccharmap.Add(UnityEngine.KeyCode.Z, 'Z');
		foreach (KeyValuePair<UnityEngine.KeyCode, Keys> item in Keymap)
		{
			if (!ReverseKeymap.ContainsKey(item.Value))
			{
				ReverseKeymap.Add(item.Value, item.Key);
			}
		}
		foreach (KeyValuePair<UnityEngine.KeyCode, char> item2 in lccharmap)
		{
			if (!reverselccharmap.ContainsKey(item2.Value))
			{
				reverselccharmap.Add(item2.Value, new List<UnityEngine.KeyCode>());
			}
			reverselccharmap[item2.Value].Add(item2.Key);
		}
		foreach (KeyValuePair<UnityEngine.KeyCode, char> item3 in uccharmap)
		{
			if (!reverseuccharmap.ContainsKey(item3.Value))
			{
				reverseuccharmap.Add(item3.Value, new List<UnityEngine.KeyCode>());
			}
			reverseuccharmap[item3.Value].Add(item3.Key);
		}
	}

	public static void PushCommand(string command, object data = null)
	{
		PushMouseEvent(COMMAND_EVENT_PREFIX + command, data);
	}

	public static bool IsCommandKey(string command)
	{
		if (command == "Page Up")
		{
			if (RawCode == Keys.MouseEvent && CurrentMouseEvent.Event == "Command:Page Up")
			{
				return true;
			}
			if (RawCode == Keys.Prior)
			{
				return true;
			}
		}
		if (command == "Page Down")
		{
			if (RawCode == Keys.MouseEvent && CurrentMouseEvent.Event == "Command:Page Down")
			{
				return true;
			}
			if (RawCode == Keys.Next)
			{
				return true;
			}
		}
		if (RawCode == Keys.MouseEvent)
		{
			if (CurrentMouseEvent.Event.StartsWith(COMMAND_EVENT_PREFIX))
			{
				return CurrentMouseEvent.Event.Substring(COMMAND_EVENT_PREFIX.Length) == command;
			}
			return false;
		}
		var (num, num2) = CommandBindingManager.GetAllKeysFromCommand(command);
		if (num != 0 && num == (int)RawCode)
		{
			return true;
		}
		if (num2 != 0 && num2 == (int)RawCode)
		{
			return true;
		}
		return false;
	}

	public static void PushKey(UnityEngine.KeyCode key)
	{
		PushKey(new XRLKeyEvent(key));
	}

	public static void PushKey(XRLKeyEvent ev, bool bAllowMap = false)
	{
		if (!TutorialManager.AllowPushKey(ev, bAllowMap))
		{
			return;
		}
		if (Keymap == null)
		{
			InitKeymap();
		}
		ev.allowmap = bAllowMap;
		if (KeyQueue.Count <= 2 || !GameManager.bCapInputBuffer)
		{
			lock (KeyQueue)
			{
				KeyQueue.Enqueue(ev);
			}
			KeyEvent.Set();
		}
	}

	public static bool HasKey()
	{
		if (Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
		return KeyQueue.Count > 0;
	}

	public static XRLKeyEvent PopKey()
	{
		lock (KeyQueue)
		{
			if (KeyQueue.Count == 0)
			{
				return null;
			}
			return KeyQueue.Dequeue();
		}
	}

	public static void PushMouseEvent(string ev, object data = null)
	{
		PushMouseEvent(ev, 0, 0, data);
	}

	public static void PushMouseEvent(string ev, int x, int y, object data = null)
	{
		if (!TutorialManager.AllowMouseEvent(ev, x, y, data) || (MouseEventQueue.Count > 2 && GameManager.bCapInputBuffer))
		{
			return;
		}
		lock (MouseEventQueue)
		{
			MouseEvent mouseEvent;
			if (MouseEventPool.Count > 0)
			{
				mouseEvent = MouseEventPool.Dequeue();
				mouseEvent.Event = ev;
				mouseEvent.x = x;
				mouseEvent.y = y;
				mouseEvent.Data = data;
			}
			else
			{
				mouseEvent = new MouseEvent(ev, x, y, data);
			}
			MouseEventQueue.Enqueue(mouseEvent);
			KeyEvent.Set();
		}
	}

	public static MouseEvent PopMouseEvent()
	{
		lock (MouseEventQueue)
		{
			if (MouseEventQueue.Count > 0)
			{
				MouseEvent mouseEvent = MouseEventQueue.Dequeue();
				MouseEventUsed.Enqueue(mouseEvent);
				return mouseEvent;
			}
		}
		return null;
	}

	public static bool HasMouseEvent(bool filterMetaCommands = false)
	{
		lock (MouseEventQueue)
		{
			if (filterMetaCommands)
			{
				return MouseEventQueue.Count > 0 && (MouseEventQueue.Peek().Event == null || !metaMousecommands.ContainsKey(MouseEventQueue.Peek().Event));
			}
			return MouseEventQueue.Count > 0;
		}
	}

	public static void ClearInput(bool LeaveMovementEvents = false, bool clearFrameDown = false)
	{
		lock (KeyQueue)
		{
			RawCode = Keys.None;
			vkCode = Keys.None;
			CurrentMouseEvent.Event = "";
			ClearMouseEvents(LeaveMovementEvents);
			KeyQueue.Clear();
		}
		if (clearFrameDown)
		{
			ControlManager.ConsumeCurrentInput();
		}
	}

	public static void ClearMouseEvents(bool LeaveMovementEvents = false)
	{
		if (LeaveMovementEvents)
		{
			tempMouseEventQueue.Clear();
		}
		lock (MouseEventQueue)
		{
			while (MouseEventQueue.Count > 0)
			{
				MouseEvent mouseEvent = MouseEventQueue.Dequeue();
				if (LeaveMovementEvents && mouseEvent.Event != null && (mouseEvent.Event.StartsWith("Meta:Navigate") || mouseEvent.Event.StartsWith("Command:CmdMove") || mouseEvent.Event.StartsWith("Command:CmdAutoMove")))
				{
					tempMouseEventQueue.Enqueue(mouseEvent);
					continue;
				}
				mouseEvent.Data = null;
				MouseEventPool.Enqueue(mouseEvent);
			}
			while (MouseEventUsed.Count > 0)
			{
				MouseEvent item = MouseEventUsed.Dequeue();
				MouseEventPool.Enqueue(item);
			}
			if (LeaveMovementEvents)
			{
				Queue<MouseEvent> mouseEventQueue = MouseEventQueue;
				MouseEventQueue = tempMouseEventQueue;
				tempMouseEventQueue = mouseEventQueue;
			}
		}
	}

	public static void IdleWait()
	{
		if (Options.DisableAllIdleTileAnimations)
		{
			KeyEvent.WaitOne();
		}
		else
		{
			KeyEvent.WaitOne(Options.ThrottleAnimationRate);
		}
	}

	public static bool kbhit()
	{
		if (XRLCore.bThreadFocus)
		{
			if (KeyQueue.Count <= 0)
			{
				return MouseEventQueue.Count > 0;
			}
			return true;
		}
		KeyEvent.WaitOne(200);
		if (KeyQueue.Count <= 0)
		{
			return MouseEventQueue.Count > 0;
		}
		return true;
	}

	public static void GetNextKey(bool MapDirectionsToArrows, bool pumpActions = false, bool waitForNext = true)
	{
		if (Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
		if (TextConsole.Mode != TerminalMode.Unity)
		{
			return;
		}
		while (true)
		{
			if (HasMouseEvent())
			{
				CurrentMouseEvent.CopyFrom(PopMouseEvent());
				if (CurrentMouseEvent.Event != null && metaMousecommands.ContainsKey(CurrentMouseEvent.Event))
				{
					vkCode = metaMousecommands[CurrentMouseEvent.Event];
					MetaKey = (int)vkCode;
					RawCode = vkCode;
					if (Keymap == null)
					{
						InitKeymap();
					}
					if (ReverseKeymap.TryGetValue(vkCode, out var value))
					{
						if (lccharmap.TryGetValue(value, out var value2))
						{
							Char = value2;
						}
						else
						{
							Char = 0;
						}
					}
					else
					{
						Char = 0;
					}
				}
				else
				{
					vkCode = Keys.MouseEvent;
					MetaKey = (int)vkCode;
					RawCode = vkCode;
					Char = (int)vkCode;
				}
				break;
			}
			if (!HasKey())
			{
				if (!waitForNext)
				{
					vkCode = Keys.None;
					Char = 0;
					MetaKey = 0;
					break;
				}
				if (XRLCore.bThreadFocus)
				{
					if (pumpActions)
					{
						if (!GameManager.Instance.gameQueue.HasTask())
						{
							KeyEvent.WaitOne(20);
						}
					}
					else
					{
						KeyEvent.WaitOne(20);
					}
				}
				else
				{
					KeyEvent.WaitOne(1000);
				}
			}
			ResetIfQueuesEmpty();
			XRLKeyEvent xRLKeyEvent = PopKey();
			if (xRLKeyEvent == null)
			{
				if (pumpActions)
				{
					GameManager.Instance.gameQueue.executeTasks();
				}
				continue;
			}
			Keys keys = Keys.None;
			if (Keymap == null)
			{
				Debug.Log("Null keymap??");
				break;
			}
			if (!Keymap.ContainsKey(xRLKeyEvent.keyCode))
			{
				Debug.LogWarning("Unknown key code " + xRLKeyEvent.keyCode);
			}
			else
			{
				keys = Keymap[xRLKeyEvent.keyCode];
			}
			if (keys == Keys.None)
			{
				break;
			}
			vkCode = keys;
			RawCode = vkCode;
			Char = xRLKeyEvent.character;
			if (xRLKeyEvent.shift)
			{
				vkCode += 65536;
			}
			if (xRLKeyEvent.shift)
			{
				RawCode += 65536;
			}
			if (xRLKeyEvent.control)
			{
				vkCode += 131072;
			}
			if (xRLKeyEvent.control)
			{
				RawCode += 131072;
			}
			if (xRLKeyEvent.alt)
			{
				vkCode += 262144;
			}
			if (xRLKeyEvent.alt)
			{
				RawCode += 262144;
			}
			if (vkCode == Keys.Oem1)
			{
				Char = 59;
			}
			if (vkCode == (Keys.Oem1 | Keys.Shift))
			{
				Char = 58;
			}
			if (vkCode == Keys.OemPeriod)
			{
				Char = 46;
			}
			if (vkCode == Keys.OemQuestion)
			{
				Char = 47;
			}
			if (vkCode == Keys.OemMinus)
			{
				Char = 45;
			}
			if (vkCode == Keys.Oem1)
			{
				Char = 59;
			}
			if (vkCode == Keys.Oemplus)
			{
				Char = 61;
			}
			if (vkCode == Keys.Oemcomma)
			{
				Char = 44;
			}
			if (vkCode == Keys.Oemtilde)
			{
				Char = 96;
			}
			if (vkCode == Keys.Oem4)
			{
				Char = 91;
			}
			if (vkCode == Keys.OemPipe)
			{
				Char = 124;
			}
			if (vkCode == Keys.Oem6)
			{
				Char = 93;
			}
			if (vkCode == Keys.Oem7)
			{
				Char = 39;
			}
			if (vkCode == Keys.Oem102)
			{
				Char = 92;
			}
			if (vkCode == (Keys.OemPeriod | Keys.Shift))
			{
				Char = 62;
			}
			if (vkCode == (Keys.OemQuestion | Keys.Shift))
			{
				Char = 63;
			}
			if (vkCode == (Keys.OemMinus | Keys.Shift))
			{
				Char = 95;
			}
			if (vkCode == (Keys.Oem1 | Keys.Shift))
			{
				Char = 58;
			}
			if (vkCode == (Keys.Oemplus | Keys.Shift))
			{
				Char = 43;
			}
			if (vkCode == (Keys.Oemcomma | Keys.Shift))
			{
				Char = 60;
			}
			if (vkCode == (Keys.Oemtilde | Keys.Shift))
			{
				Char = 126;
			}
			if (vkCode == (Keys.Oem4 | Keys.Shift))
			{
				Char = 123;
			}
			if (vkCode == (Keys.OemPipe | Keys.Shift))
			{
				Char = 124;
			}
			if (vkCode == (Keys.Oem6 | Keys.Shift))
			{
				Char = 125;
			}
			if (vkCode == (Keys.Oem7 | Keys.Shift))
			{
				Char = 34;
			}
			if (vkCode == (Keys.Oem102 | Keys.Shift))
			{
				Char = 124;
			}
			if (vkCode == Keys.Left)
			{
				vkCode = Keys.NumPad4;
				Char = 52;
			}
			if (vkCode == Keys.Right)
			{
				vkCode = Keys.NumPad6;
				Char = 54;
			}
			if (vkCode == Keys.Up)
			{
				vkCode = Keys.NumPad8;
				Char = 56;
			}
			if (vkCode == Keys.Down)
			{
				vkCode = Keys.NumPad2;
				Char = 50;
			}
			MetaKey = (int)vkCode;
			if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdIgnore")
			{
				continue;
			}
			if (MapDirectionsToArrows && xRLKeyEvent.allowmap)
			{
				if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdWait")
				{
					vkCode = Keys.NumPad5;
					Char = 53;
				}
				if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdMoveN")
				{
					vkCode = Keys.NumPad8;
					Char = 56;
				}
				if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdMoveS")
				{
					vkCode = Keys.NumPad2;
					Char = 50;
				}
				if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdMoveE")
				{
					vkCode = Keys.NumPad6;
					Char = 54;
				}
				if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdMoveW")
				{
					vkCode = Keys.NumPad4;
					Char = 52;
				}
				if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdMoveNW")
				{
					vkCode = Keys.NumPad7;
					Char = 55;
				}
				if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdMoveNE")
				{
					vkCode = Keys.NumPad9;
					Char = 57;
				}
				if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdMoveSW")
				{
					vkCode = Keys.NumPad1;
					Char = 49;
				}
				if (CommandBindingManager.MapKeyToCommand(MetaKey) == "CmdMoveSE")
				{
					vkCode = Keys.NumPad3;
					Char = 51;
				}
				if (IsCommandKey("Page Up"))
				{
					MetaKey = 33;
					vkCode = Keys.Prior;
				}
				if (IsCommandKey("Page Down"))
				{
					MetaKey = 34;
					vkCode = Keys.Next;
				}
			}
			if (Options.GetOption("OptionMapShiftDirectionToPage") == "Yes" && xRLKeyEvent.shift)
			{
				int num = MetaKey - 65536;
				if (CommandBindingManager.MapKeyToCommand(num) == "CmdWait")
				{
					vkCode = Keys.NumPad5;
					Char = 53;
				}
				if (CommandBindingManager.MapKeyToCommand(num) == "CmdMoveN")
				{
					vkCode = Keys.NumPad8;
					Char = 56;
				}
				if (CommandBindingManager.MapKeyToCommand(num) == "CmdMoveS")
				{
					vkCode = Keys.NumPad2;
					Char = 50;
				}
				if (CommandBindingManager.MapKeyToCommand(num) == "CmdMoveE")
				{
					vkCode = Keys.NumPad6;
					Char = 54;
				}
				if (CommandBindingManager.MapKeyToCommand(num) == "CmdMoveW")
				{
					vkCode = Keys.NumPad4;
					Char = 52;
				}
				if (CommandBindingManager.MapKeyToCommand(num) == "CmdMoveNW")
				{
					vkCode = Keys.NumPad7;
					Char = 55;
				}
				if (CommandBindingManager.MapKeyToCommand(num) == "CmdMoveNE")
				{
					vkCode = Keys.NumPad9;
					Char = 57;
				}
				if (CommandBindingManager.MapKeyToCommand(num) == "CmdMoveSW")
				{
					vkCode = Keys.NumPad1;
					Char = 49;
				}
				if (CommandBindingManager.MapKeyToCommand(num) == "CmdMoveSE")
				{
					vkCode = Keys.NumPad3;
					Char = 51;
				}
				if (vkCode == Keys.Up || vkCode == Keys.NumPad8 || num == 38 || num == 104)
				{
					MetaKey = 33;
					vkCode = Keys.Prior;
					xRLKeyEvent.shift = false;
				}
				if (vkCode == Keys.Down || vkCode == Keys.NumPad2 || num == 40 || num == 98)
				{
					MetaKey = 34;
					vkCode = Keys.Next;
					xRLKeyEvent.shift = false;
				}
				if (vkCode == Keys.Right || vkCode == Keys.NumPad6 || num == 39 || num == 102)
				{
					MetaKey = 105;
					vkCode = Keys.NumPad9;
					xRLKeyEvent.shift = false;
				}
				if (vkCode == Keys.Left || vkCode == Keys.NumPad4 || num == 37 || num == 100)
				{
					MetaKey = 103;
					vkCode = Keys.NumPad7;
					xRLKeyEvent.shift = false;
				}
			}
			MetaKey = (int)vkCode;
			break;
		}
	}

	public static Keys getvk(bool MapDirectionToArrows, bool pumpActions = false, bool wait = true)
	{
		GetNextKey(MapDirectionToArrows, pumpActions, wait);
		return vkCode;
	}

	public static int getchraw()
	{
		GetNextKey(MapDirectionsToArrows: false);
		return ((char)RawCode).ToString().ToLower()[0];
	}

	public static int getch()
	{
		GetNextKey(MapDirectionsToArrows: false);
		return Char;
	}

	public static int getmeta(bool MapDirectionToArrows)
	{
		GetNextKey(MapDirectionToArrows);
		return MetaKey;
	}

	public static string MetaToString(int MetaCode)
	{
		string text = "";
		if ((MetaCode & 0x20000) != 0)
		{
			text += "Ctrl-";
		}
		if ((MetaCode & 0x40000) != 0)
		{
			text += "Alt-";
		}
		if ((MetaCode & 0x10000) != 0)
		{
			text += "Shift-";
		}
		MetaCode &= 0xFFFF;
		string text2 = text;
		Keys keys = (Keys)MetaCode;
		return text2 + keys;
	}

	public static void MetaToString(int MetaCode, StringBuilder SB)
	{
		if ((MetaCode & 0x40000) != 0)
		{
			SB.Append("Alt+");
		}
		if ((MetaCode & 0x20000) != 0)
		{
			SB.Append("Ctrl+");
		}
		if ((MetaCode & 0x10000) != 0)
		{
			SB.Append("Shift+");
		}
		MetaCode &= 0xFFFF;
		if (!metaToString.ContainsKey(MetaCode))
		{
			Dictionary<int, string> dictionary = metaToString;
			int key = MetaCode;
			Keys keys = (Keys)MetaCode;
			dictionary.Add(key, keys.ToString());
		}
		SB.Append(metaToString[MetaCode]);
	}

	public static string MetaToStringWithLower(int MetaCode)
	{
		bool flag = false;
		string text = "";
		if ((MetaCode & 0x20000) != 0)
		{
			text += "Ctrl-";
			flag = true;
		}
		if ((MetaCode & 0x40000) != 0)
		{
			text += "Alt-";
			flag = true;
		}
		if ((MetaCode & 0x10000) != 0)
		{
			text += "Shift-";
			flag = true;
		}
		MetaCode &= 0xFFFF;
		Keys keys = (Keys)MetaCode;
		string text2 = keys.ToString();
		if (text2 != null)
		{
			text = ((flag || text2.Length != 1 || text2[0] < 'A' || text2[0] < 'Z') ? (text + text2) : (text + text2.ToLower()));
		}
		return text;
	}
}
