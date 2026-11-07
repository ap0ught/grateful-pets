using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConsoleLib.Console;
using UnityEngine.InputSystem;

namespace XRL.UI;

public static class InputSystemExtensions
{
	public class KeyboardKeyComparer : IComparer<string>, IEqualityComparer<string>
	{
		internal static readonly string keyboardPrefix = "<keyboard>/";

		private static KeyboardKeyComparer instance;

		public static KeyboardKeyComparer IgnoringKeyboard => instance ?? (instance = new KeyboardKeyComparer());

		public int Compare(string f1, string f2)
		{
			if (f1 != null && f1.StartsWith(keyboardPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				f1 = f1.Substring(keyboardPrefix.Length);
			}
			if (f2 != null && f2.StartsWith(keyboardPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				f2 = f2.Substring(keyboardPrefix.Length);
			}
			return StringComparer.InvariantCultureIgnoreCase.Compare(f1, f2);
		}

		public bool Equals(string f1, string f2)
		{
			if (f1 != null && f1.StartsWith(keyboardPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				f1 = f1.Substring(keyboardPrefix.Length);
			}
			if (f2 != null && f2.StartsWith(keyboardPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				f2 = f2.Substring(keyboardPrefix.Length);
			}
			return StringComparer.InvariantCultureIgnoreCase.Equals(f1, f2);
		}

		public int GetHashCode(string f1)
		{
			if (f1 != null && f1.StartsWith(keyboardPrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				f1 = f1.Substring(keyboardPrefix.Length);
			}
			return StringComparer.InvariantCultureIgnoreCase.GetHashCode(f1);
		}
	}

	public static readonly string COMPOSITE = "Composite";

	public static readonly string GAMEPADALT = "GamepadAlt";

	private static Dictionary<string, Keys> PathToKeysMap = new Dictionary<string, Keys>(KeyboardKeyComparer.IgnoringKeyboard)
	{
		{
			"space",
			Keys.Space
		},
		{
			"enter",
			Keys.Enter
		},
		{
			"tab",
			Keys.Tab
		},
		{
			"backquote",
			Keys.Oemtilde
		},
		{
			"quote",
			Keys.Oem7
		},
		{
			"semicolon",
			Keys.Oem1
		},
		{
			"comma",
			Keys.Oemcomma
		},
		{
			"period",
			Keys.OemPeriod
		},
		{
			"slash",
			Keys.OemQuestion
		},
		{
			"backslash",
			Keys.Oem102
		},
		{
			"leftbracket",
			Keys.Oem4
		},
		{
			"rightbracket",
			Keys.Oem6
		},
		{
			"minus",
			Keys.OemMinus
		},
		{
			"equals",
			Keys.Oemplus
		},
		{
			"a",
			Keys.A
		},
		{
			"b",
			Keys.B
		},
		{
			"c",
			Keys.C
		},
		{
			"d",
			Keys.D
		},
		{
			"e",
			Keys.E
		},
		{
			"f",
			Keys.F
		},
		{
			"g",
			Keys.G
		},
		{
			"h",
			Keys.H
		},
		{
			"i",
			Keys.I
		},
		{
			"j",
			Keys.J
		},
		{
			"k",
			Keys.K
		},
		{
			"l",
			Keys.L
		},
		{
			"m",
			Keys.M
		},
		{
			"n",
			Keys.N
		},
		{
			"o",
			Keys.O
		},
		{
			"p",
			Keys.P
		},
		{
			"q",
			Keys.Q
		},
		{
			"r",
			Keys.R
		},
		{
			"s",
			Keys.S
		},
		{
			"t",
			Keys.T
		},
		{
			"u",
			Keys.U
		},
		{
			"v",
			Keys.V
		},
		{
			"w",
			Keys.W
		},
		{
			"x",
			Keys.X
		},
		{
			"y",
			Keys.Y
		},
		{
			"z",
			Keys.Z
		},
		{
			"1",
			Keys.D1
		},
		{
			"2",
			Keys.D2
		},
		{
			"3",
			Keys.D3
		},
		{
			"4",
			Keys.D4
		},
		{
			"5",
			Keys.D5
		},
		{
			"6",
			Keys.D6
		},
		{
			"7",
			Keys.D7
		},
		{
			"8",
			Keys.D8
		},
		{
			"9",
			Keys.D9
		},
		{
			"0",
			Keys.D0
		},
		{
			"shift",
			Keys.Shift
		},
		{
			"leftshift",
			Keys.Shift
		},
		{
			"rightshift",
			Keys.Shift
		},
		{
			"alt",
			Keys.Alt
		},
		{
			"leftalt",
			Keys.Alt
		},
		{
			"rightalt",
			Keys.Alt
		},
		{
			"ctrl",
			Keys.Control
		},
		{
			"leftctrl",
			Keys.Control
		},
		{
			"rightctrl",
			Keys.Control
		},
		{
			"leftmeta",
			Keys.LWin
		},
		{
			"rightmeta",
			Keys.RWin
		},
		{
			"contextmenu",
			Keys.Menu
		},
		{
			"escape",
			Keys.Escape
		},
		{
			"leftarrow",
			Keys.Left
		},
		{
			"rightarrow",
			Keys.Right
		},
		{
			"uparrow",
			Keys.Up
		},
		{
			"downarrow",
			Keys.Down
		},
		{
			"backspace",
			Keys.Back
		},
		{
			"pagedown",
			Keys.Prior
		},
		{
			"pageup",
			Keys.Next
		},
		{
			"home",
			Keys.Home
		},
		{
			"end",
			Keys.End
		},
		{
			"insert",
			Keys.Insert
		},
		{
			"delete",
			Keys.Delete
		},
		{
			"capslock",
			Keys.CapsLock
		},
		{
			"numlock",
			Keys.NumLock
		},
		{
			"printscreen",
			Keys.PrintScreen
		},
		{
			"scrolllock",
			Keys.Scroll
		},
		{
			"pause",
			Keys.Pause
		},
		{
			"numpadenter",
			Keys.Enter
		},
		{
			"numpaddivide",
			Keys.Divide
		},
		{
			"numpadmultiply",
			Keys.Multiply
		},
		{
			"numpadplus",
			Keys.Oemplus
		},
		{
			"numpadminus",
			Keys.OemMinus
		},
		{
			"numpadperiod",
			Keys.OemPeriod
		},
		{
			"numpadequals",
			Keys.Oemplus
		},
		{
			"numpad0",
			Keys.NumPad0
		},
		{
			"numpad1",
			Keys.NumPad1
		},
		{
			"numpad2",
			Keys.NumPad2
		},
		{
			"numpad3",
			Keys.NumPad3
		},
		{
			"numpad4",
			Keys.NumPad4
		},
		{
			"numpad5",
			Keys.NumPad5
		},
		{
			"numpad6",
			Keys.NumPad6
		},
		{
			"numpad7",
			Keys.NumPad7
		},
		{
			"numpad8",
			Keys.NumPad8
		},
		{
			"numpad9",
			Keys.NumPad9
		},
		{
			"f1",
			Keys.F1
		},
		{
			"f2",
			Keys.F2
		},
		{
			"f3",
			Keys.F3
		},
		{
			"f4",
			Keys.F4
		},
		{
			"f5",
			Keys.F5
		},
		{
			"f6",
			Keys.F6
		},
		{
			"f7",
			Keys.F7
		},
		{
			"f8",
			Keys.F8
		},
		{
			"f9",
			Keys.F9
		},
		{
			"f10",
			Keys.F10
		},
		{
			"f11",
			Keys.F11
		},
		{
			"f12",
			Keys.F12
		},
		{
			"oem1",
			Keys.Oem1
		},
		{
			"oem2",
			Keys.OemQuestion
		},
		{
			"oem3",
			Keys.Oemtilde
		},
		{
			"oem4",
			Keys.Oem4
		},
		{
			"oem5",
			Keys.OemPipe
		}
	};

	private static FieldInfo _displayname;

	public static List<string> SerializedFormat(this InputAction action)
	{
		List<string> list = new List<string>();
		string text = ((action.name == GAMEPADALT) ? null : (CommandBindingManager.GetSerializedBindingsForCommand(GAMEPADALT)?.First() ?? "<Gamepad>/leftTrigger"));
		foreach (InputBinding binding in action.bindings)
		{
			if (binding.isComposite)
			{
				list.Add(COMPOSITE);
				list.Add(binding.GetNameOfComposite());
			}
			else if (binding.path == text)
			{
				list.Add(GAMEPADALT);
			}
			else
			{
				list.Add(binding.path);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list;
	}

	public static Keys PathToKeys(string path)
	{
		if (PathToKeysMap.TryGetValue(path, out var value))
		{
			return value;
		}
		if (!string.IsNullOrEmpty(path))
		{
			path.StartsWith("<Gampead>");
		}
		return Keys.None;
	}

	public static IEnumerable<int> GetConsoleKeycodes(this InputAction action)
	{
		int num = 0;
		for (int x = 0; x < action.bindings.Count; x++)
		{
			InputBinding bind = action.bindings[x];
			if (bind.isComposite)
			{
				if (num != 0)
				{
					yield return num;
					num = 0;
				}
				continue;
			}
			if (bind.isPartOfComposite)
			{
				if (bind.path != null)
				{
					num = (int)(num + PathToKeys(bind.path));
				}
				continue;
			}
			if (num != 0)
			{
				yield return num;
				num = 0;
			}
			if (bind.path != null)
			{
				num = (int)(num + PathToKeys(bind.path));
			}
		}
		if (num != 0)
		{
			yield return num;
		}
	}

	public static void LoadSerializedBindings(this InputAction action, List<string> bindings)
	{
		if (bindings == null)
		{
			return;
		}
		int num = 0;
		while (num < bindings.Count)
		{
			if (bindings[num] == COMPOSITE)
			{
				InputActionSetupExtensions.CompositeSyntax compositeSyntax = action.AddCompositeBinding(bindings[num + 1]);
				if (bindings[num + 1] == "OneModifier")
				{
					compositeSyntax.With("Binding", bindings[num + 2]);
					compositeSyntax.With("Modifier", resolveAlt(bindings[num + 3]));
					num += 4;
					continue;
				}
				if (!(bindings[num + 1] == "TwoModifiers"))
				{
					MetricsManager.LogError("Unknown composite type " + bindings[num + 1] + " - aborting load for this action");
					break;
				}
				compositeSyntax.With("Binding", bindings[num + 2]);
				compositeSyntax.With("Modifier1", resolveAlt(bindings[num + 3]));
				compositeSyntax.With("Modifier2", resolveAlt(bindings[num + 4]));
				num += 5;
			}
			else
			{
				action.AddBinding(bindings[num++]);
			}
		}
		static string resolveAlt(string binding)
		{
			if (binding == GAMEPADALT || binding.StartsWith("<Gamepad>"))
			{
				return CommandBindingManager.CurrentMap.ResolveGamepadAltBind();
			}
			return binding;
		}
	}

	public static void SetDisplayName(this InputControl control, string displayName)
	{
		_ = control.displayName;
		if ((object)_displayname == null)
		{
			_displayname = typeof(InputControl).GetField("m_DisplayName", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		_displayname.SetValue(control, displayName);
	}

	public static void SetDisplayName(this InputControlList<InputControl> controls, string displayName)
	{
		foreach (InputControl item in controls)
		{
			item.SetDisplayName(displayName);
		}
	}
}
