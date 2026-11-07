using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using XRL;
using XRL.UI;

public class ControlManager : MonoBehaviour
{
	public class ControlInputContext
	{
		public Func<bool> isInInput = isFalse;

		private static bool isFalse()
		{
			return false;
		}
	}

	[Serializable]
	public enum ControllerFontType
	{
		XBox,
		PS5,
		Switch,
		Keyboard,
		Default,
		XBoxFilled,
		PS5Filled,
		ForceKBM,
		SwitchFilled
	}

	[Serializable]
	public struct ControllerFontSelection
	{
		public ControllerFontType controllerType;

		public TMP_FontAsset fontAsset;
	}

	public interface IControllerChangedEvent
	{
		void ControllerChanged();
	}

	public static class Keybinds
	{
		public static readonly string ACCEPT = "accept";
	}

	public struct LegacyKeyCode(UnityEngine.KeyCode code, bool bAlt, bool bCtrl, bool bShift)
	{
		public bool bCtrl = bCtrl;

		public bool bShift = bShift;

		public bool bAlt = bAlt;

		public UnityEngine.KeyCode code = code;

		public int rawCode = (int)(UnityKeycodeToConsoleKeycodeMap[code] + (bAlt ? 262144 : 0) + (bCtrl ? 131072 : 0) + (bShift ? 65536 : 0));
	}

	public enum InputDeviceType
	{
		Mouse,
		Keyboard,
		Gamepad,
		Unknown
	}

	public class FrameCommand
	{
		public CommandBinding binding;

		public string id;

		public List<string> aliases = new List<string>(8);

		public bool isRepeat;

		public bool ignore;

		public override string ToString()
		{
			string res = id;
			aliases.ForEach((string a) =>
			{
				res = res + " " + a;
			});
			return res;
		}

		public int match(string test)
		{
			bool flag = false;
			for (int i = 0; i < aliases.Count; i++)
			{
				if (aliases[i] == test)
				{
					flag = true;
					break;
				}
			}
			if ((id == test) | flag)
			{
				if (isRepeat)
				{
					return 2;
				}
				return 1;
			}
			return 0;
		}

		public void reset()
		{
			ignore = false;
			binding = null;
			id = null;
			if (aliases.Count > 0)
			{
				aliases.Clear();
			}
			isRepeat = false;
		}
	}

	public TMP_FontAsset keyboardGlyphFont;

	public List<ControllerFontSelection> controllerFonts = new List<ControllerFontSelection>();

	public ControllerFontType controllerFontType = ControllerFontType.Default;

	private ControllerFontType? lastControllerFontType;

	private InputDeviceType? lastControllerType;

	private Vector3 lastMousePosition;

	private double lastTimeChangedController;

	protected bool lastInputIsMouse;

	public static int SkipFrames = 0;

	private static List<string> _WaitForKeyupList = new List<string>();

	public static HashSet<string> CommandFramePressed = new HashSet<string>();

	public static bool ControllerChangedThisLateUpdate = false;

	public static bool updateFont = false;

	public static ControlInputContext currentContext = new ControlInputContext();

	public static ControlManager instance;

	public ControllerFontType lastControllerFont;

	public static InputDevice lastUsedDevice;

	public static InputControl lastControlPressed;

	public static bool SuspectSteamInput = false;

	public static List<string> EnabledLayers = new List<string>();

	public static Dictionary<string, string> _mapRewiredIDToLegacyID = new Dictionary<string, string>
	{
		{ "Accept", "Accept" },
		{ "Cancel", "Cancel" },
		{ "Interact", "CmdUse" },
		{ "Fire", "CmdFire" },
		{ "Reload", "CmdReload" },
		{ "Abilities", "CmdAbilities" },
		{ "Character Sheet", "CmdCharacter" },
		{ "Walk", "CmdWalk" },
		{ "Look", "CmdLook" },
		{ "ZoomIn", "CmdZoomIn" },
		{ "ZoomOut", "CmdZoomOut" },
		{ "Get", "CmdGet" },
		{ "GetNearby", "CmdGetFrom" },
		{ "InteractNearby", "CmdGetFrom" },
		{ "Autoexplore", "CmdAutoExplore" },
		{ "Wait", "CmdWait" },
		{ "Wait Menu", "CmdWaitMenu" },
		{ "Rest", "CmdWaitUntilHealed" },
		{ "Throw", "CmdThrow" },
		{ "Wish", "CmdWish" },
		{ "Quests", "CmdQuests" },
		{ "Save", "CmdSave" },
		{ "Load", "CmdLoad" },
		{ "AutoAttackNearest", "CmdAttackNearest" },
		{ "Move Up", "CmdMoveU" },
		{ "Move Down", "CmdMoveD" },
		{ "Move Northwest", "CmdMoveNW" },
		{ "Move North", "CmdMoveN" },
		{ "Move Northeast", "CmdMoveNE" },
		{ "Move East", "CmdMoveE" },
		{ "Move Southeast", "CmdMoveSE" },
		{ "Move South", "CmdMoveS" },
		{ "Move Southwest", "CmdMoveSW" },
		{ "Move West", "CmdMoveW" },
		{ "Attack Up", "CmdAttackN" },
		{ "Attack Down", "CmdAttackD" },
		{ "Attack Northwest", "CmdAttackNW" },
		{ "Attack North", "CmdAttackN" },
		{ "Attack Northeast", "CmdAttackNE" },
		{ "Attack East", "CmdAttackE" },
		{ "Attack Southeast", "CmdAttackSE" },
		{ "Attack South", "CmdAttackS" },
		{ "Attack Southwest", "CmdAttackSW" },
		{ "Attack West", "CmdAttackW" },
		{ "Toggle Message Log", "CmdShowSidebar" }
	};

	public static Dictionary<int, UnityEngine.KeyCode> _consoleKeycodeToUnityKeycode = null;

	public static Dictionary<UnityEngine.KeyCode, Keys> _UnityKeycodeToConsoleKeycodeMap = null;

	private static Dictionary<UnityEngine.KeyCode, KeyControl> _MacOS13UnityKeypadKeysDummy = new Dictionary<UnityEngine.KeyCode, KeyControl>();

	private static Dictionary<UnityEngine.KeyCode, KeyControl> _MacOS13UnityKeypadKeys = null;

	public static string frameString = null;

	public static List<UnityEngine.KeyCode> submitKeys = new List<UnityEngine.KeyCode>
	{
		UnityEngine.KeyCode.KeypadEnter,
		UnityEngine.KeyCode.Return,
		UnityEngine.KeyCode.Space
	};

	public static List<UnityEngine.KeyCode> keysIgnoredWhileInAnInputField = new List<UnityEngine.KeyCode>
	{
		UnityEngine.KeyCode.Backspace,
		UnityEngine.KeyCode.Keypad0,
		UnityEngine.KeyCode.Keypad1,
		UnityEngine.KeyCode.Keypad2,
		UnityEngine.KeyCode.Keypad3,
		UnityEngine.KeyCode.Keypad4,
		UnityEngine.KeyCode.Keypad5,
		UnityEngine.KeyCode.Keypad6,
		UnityEngine.KeyCode.Keypad7,
		UnityEngine.KeyCode.Keypad8,
		UnityEngine.KeyCode.Keypad9,
		UnityEngine.KeyCode.Alpha0,
		UnityEngine.KeyCode.Alpha1,
		UnityEngine.KeyCode.Alpha2,
		UnityEngine.KeyCode.Alpha3,
		UnityEngine.KeyCode.Alpha4,
		UnityEngine.KeyCode.Alpha5,
		UnityEngine.KeyCode.Alpha6,
		UnityEngine.KeyCode.Alpha7,
		UnityEngine.KeyCode.Alpha8,
		UnityEngine.KeyCode.Alpha9,
		UnityEngine.KeyCode.A,
		UnityEngine.KeyCode.B,
		UnityEngine.KeyCode.C,
		UnityEngine.KeyCode.D,
		UnityEngine.KeyCode.E,
		UnityEngine.KeyCode.F,
		UnityEngine.KeyCode.G,
		UnityEngine.KeyCode.H,
		UnityEngine.KeyCode.I,
		UnityEngine.KeyCode.J,
		UnityEngine.KeyCode.K,
		UnityEngine.KeyCode.L,
		UnityEngine.KeyCode.M,
		UnityEngine.KeyCode.N,
		UnityEngine.KeyCode.O,
		UnityEngine.KeyCode.P,
		UnityEngine.KeyCode.Q,
		UnityEngine.KeyCode.R,
		UnityEngine.KeyCode.S,
		UnityEngine.KeyCode.T,
		UnityEngine.KeyCode.U,
		UnityEngine.KeyCode.V,
		UnityEngine.KeyCode.W,
		UnityEngine.KeyCode.X,
		UnityEngine.KeyCode.Y,
		UnityEngine.KeyCode.Z,
		UnityEngine.KeyCode.Space,
		UnityEngine.KeyCode.Comma,
		UnityEngine.KeyCode.Colon,
		UnityEngine.KeyCode.Semicolon,
		UnityEngine.KeyCode.LeftArrow,
		UnityEngine.KeyCode.RightArrow,
		UnityEngine.KeyCode.Slash,
		UnityEngine.KeyCode.Quote,
		UnityEngine.KeyCode.DoubleQuote,
		UnityEngine.KeyCode.LeftBracket,
		UnityEngine.KeyCode.RightBracket
	};

	public static Dictionary<string, string> gamepadCommandInputsWithGlyphs = new Dictionary<string, string>();

	public static Dictionary<string, string> gamepadCommandInputsWithoutGlyphs = new Dictionary<string, string>();

	public static Dictionary<string, string> keyboardCommandInputsWithGlyphs = new Dictionary<string, string>();

	public static Dictionary<string, string> keyboardCommandInputsWithoutGlyphs = new Dictionary<string, string>();

	public static float delaytime = 0.5f;

	public static float repeattime = 0.1f;

	public static Dictionary<UnityEngine.KeyCode, float> delayTimers = new Dictionary<UnityEngine.KeyCode, float>();

	public static Dictionary<UnityEngine.KeyCode, float> repeatTimers = new Dictionary<UnityEngine.KeyCode, float>();

	public static Dictionary<int, float> delayTimers2 = new Dictionary<int, float>();

	public static Dictionary<int, float> repeatTimers2 = new Dictionary<int, float>();

	public static UnityEngine.KeyCode lastMacOS13KeyPress = UnityEngine.KeyCode.None;

	public static float MacOS13repeatTime = 0f;

	public static bool MacOS13repeating = false;

	private static List<string> scratch = new List<string>(64);

	private static List<string> layersToEnableAtEndOfFrame = null;

	public static InputDeviceType _activeControllerType;

	public static bool mappingMode = false;

	public static Dictionary<string, float> commandDelayTimers = new Dictionary<string, float>();

	public static Dictionary<string, float> commandRepeatTimers = new Dictionary<string, float>();

	private static Queue<FrameCommand> commandPool = new Queue<FrameCommand>(16);

	private static FrameCommand FrameDownCommand;

	private static Queue<FrameCommand> CommandQueue = new Queue<FrameCommand>();

	private static bool NeedsQueueUpdate = false;

	private static int lastFrame = 0;

	public static List<FrameCommand> currentFrameCommands = new List<FrameCommand>(8);

	private static bool PauseStickNTillReleased = false;

	private static int StickN = 0;

	private static bool PauseStickSTillReleased = false;

	private static int StickS = 0;

	private static bool PauseStickETillReleased = false;

	private static int StickE = 0;

	private static bool PauseStickWTillReleased = false;

	private static int StickW = 0;

	private static Dictionary<string, string> upActions = new Dictionary<string, string>();

	private static Dictionary<string, string> downActions = new Dictionary<string, string>();

	private static Dictionary<string, string> leftActions = new Dictionary<string, string>();

	private static Dictionary<string, string> rightActions = new Dictionary<string, string>();

	private static Dictionary<string, string> controllerGlyphs = new Dictionary<string, string>
	{
		{ "LS", "\ue904" },
		{ "Left Stick", "\ue904" },
		{ "RS", "\ue90f" },
		{ "Right Stick", "\ue90f" },
		{ "Cross", "\ue900" },
		{ "A", "\ue900" },
		{ "Button 0", "\ue900" },
		{ "Circle", "\ue901" },
		{ "B", "\ue901" },
		{ "Button 1", "\ue901" },
		{ "Square", "\ue902" },
		{ "X", "\ue902" },
		{ "Button 2", "\ue902" },
		{ "Triangle", "\ue903" },
		{ "Y", "\ue903" },
		{ "Button 3", "\ue903" },
		{ "Left Shoulder", "\ue915" },
		{ "L1", "\ue915" },
		{ "LB", "\ue915" },
		{ "Right Shoulder", "\ue917" },
		{ "R1", "\ue917" },
		{ "RB", "\ue917" },
		{ "Left Trigger", "\ue916" },
		{ "L2", "\ue916" },
		{ "LT", "\ue916" },
		{ "Right Trigger", "\ue918" },
		{ "R2", "\ue918" },
		{ "RT", "\ue918" },
		{ "D-Pad Up", "\ue90b" },
		{ "Up", "\ue90b" },
		{ "D-Pad Down", "\ue90d" },
		{ "Down", "\ue90d" },
		{ "D-Pad Left", "\ue90e" },
		{ "Left", "\ue90e" },
		{ "D-Pad Right", "\ue90c" },
		{ "Right", "\ue90c" },
		{ "D-Pad/Up", "\ue90b" },
		{ "D-Pad/Down", "\ue90d" },
		{ "D-Pad/Left", "\ue90e" },
		{ "D-Pad/Right", "\ue90c" },
		{ "Start", "\ue91a" },
		{ "Options", "\ue91a" },
		{ "Share", "\ue919" },
		{ "Select", "\ue919" },
		{ "Back", "\ue919" },
		{ "Create", "\ue919" },
		{ "Left Stick X", "\ue908\ue906" },
		{ "Left Stick Y", "\ue905\ue907" },
		{ "Right Stick X", "\ue913\ue911" },
		{ "Right Stick Y", "\ue910\ue912" },
		{ "Left Stick Press", "\ue909" },
		{ "Right Stick Press", "\ue914" },
		{ "L3", "\ue909" },
		{ "R3", "\ue914" }
	};

	public static bool LastInputFromMouse => instance?.lastInputIsMouse ?? false;

	public static GameManager gameManager => GameManager.Instance;

	public PlayerInputManager player => GameManager.Instance?.player;

	public static Dictionary<int, UnityEngine.KeyCode> consoleKeycodeToUnityKeycode
	{
		get
		{
			if (_consoleKeycodeToUnityKeycode == null)
			{
				ConsoleLib.Console.Keyboard.InitKeymap();
				_consoleKeycodeToUnityKeycode = new Dictionary<int, UnityEngine.KeyCode>();
				foreach (KeyValuePair<UnityEngine.KeyCode, Keys> item in ConsoleLib.Console.Keyboard.Keymap)
				{
					if (!_consoleKeycodeToUnityKeycode.ContainsKey((int)item.Value))
					{
						_consoleKeycodeToUnityKeycode.Add((int)item.Value, item.Key);
					}
				}
			}
			return _consoleKeycodeToUnityKeycode;
		}
	}

	public static Dictionary<UnityEngine.KeyCode, Keys> UnityKeycodeToConsoleKeycodeMap
	{
		get
		{
			if (_UnityKeycodeToConsoleKeycodeMap == null)
			{
				ConsoleLib.Console.Keyboard.InitKeymap();
				_UnityKeycodeToConsoleKeycodeMap = new Dictionary<UnityEngine.KeyCode, Keys>();
				foreach (KeyValuePair<UnityEngine.KeyCode, Keys> item in ConsoleLib.Console.Keyboard.Keymap)
				{
					if (!_UnityKeycodeToConsoleKeycodeMap.ContainsKey(item.Key))
					{
						_UnityKeycodeToConsoleKeycodeMap.Add(item.Key, item.Value);
					}
				}
			}
			return _UnityKeycodeToConsoleKeycodeMap;
		}
	}

	private static Dictionary<UnityEngine.KeyCode, KeyControl> MacOS13UnityKeypadKeys
	{
		get
		{
			if (UnityEngine.InputSystem.Keyboard.current == null)
			{
				return _MacOS13UnityKeypadKeysDummy;
			}
			if (UnityEngine.InputSystem.Keyboard.current?.numpad0Key == null)
			{
				return _MacOS13UnityKeypadKeysDummy;
			}
			if (_MacOS13UnityKeypadKeys == null)
			{
				_MacOS13UnityKeypadKeys = new Dictionary<UnityEngine.KeyCode, KeyControl>
				{
					{
						UnityEngine.KeyCode.Keypad0,
						UnityEngine.InputSystem.Keyboard.current.numpad0Key
					},
					{
						UnityEngine.KeyCode.Keypad1,
						UnityEngine.InputSystem.Keyboard.current.numpad1Key
					},
					{
						UnityEngine.KeyCode.Keypad2,
						UnityEngine.InputSystem.Keyboard.current.numpad2Key
					},
					{
						UnityEngine.KeyCode.Keypad3,
						UnityEngine.InputSystem.Keyboard.current.numpad3Key
					},
					{
						UnityEngine.KeyCode.Keypad4,
						UnityEngine.InputSystem.Keyboard.current.numpad4Key
					},
					{
						UnityEngine.KeyCode.Keypad5,
						UnityEngine.InputSystem.Keyboard.current.numpad5Key
					},
					{
						UnityEngine.KeyCode.Keypad6,
						UnityEngine.InputSystem.Keyboard.current.numpad6Key
					},
					{
						UnityEngine.KeyCode.Keypad7,
						UnityEngine.InputSystem.Keyboard.current.numpad7Key
					},
					{
						UnityEngine.KeyCode.Keypad8,
						UnityEngine.InputSystem.Keyboard.current.numpad8Key
					},
					{
						UnityEngine.KeyCode.Keypad9,
						UnityEngine.InputSystem.Keyboard.current.numpad9Key
					},
					{
						UnityEngine.KeyCode.Alpha0,
						UnityEngine.InputSystem.Keyboard.current.digit0Key
					},
					{
						UnityEngine.KeyCode.Alpha1,
						UnityEngine.InputSystem.Keyboard.current.digit1Key
					},
					{
						UnityEngine.KeyCode.Alpha2,
						UnityEngine.InputSystem.Keyboard.current.digit2Key
					},
					{
						UnityEngine.KeyCode.Alpha3,
						UnityEngine.InputSystem.Keyboard.current.digit3Key
					},
					{
						UnityEngine.KeyCode.Alpha4,
						UnityEngine.InputSystem.Keyboard.current.digit4Key
					},
					{
						UnityEngine.KeyCode.Alpha5,
						UnityEngine.InputSystem.Keyboard.current.digit5Key
					},
					{
						UnityEngine.KeyCode.Alpha6,
						UnityEngine.InputSystem.Keyboard.current.digit6Key
					},
					{
						UnityEngine.KeyCode.Alpha7,
						UnityEngine.InputSystem.Keyboard.current.digit7Key
					},
					{
						UnityEngine.KeyCode.Alpha8,
						UnityEngine.InputSystem.Keyboard.current.digit8Key
					},
					{
						UnityEngine.KeyCode.Alpha9,
						UnityEngine.InputSystem.Keyboard.current.digit9Key
					},
					{
						UnityEngine.KeyCode.KeypadPeriod,
						UnityEngine.InputSystem.Keyboard.current.numpadPeriodKey
					},
					{
						UnityEngine.KeyCode.KeypadDivide,
						UnityEngine.InputSystem.Keyboard.current.numpadDivideKey
					},
					{
						UnityEngine.KeyCode.KeypadMultiply,
						UnityEngine.InputSystem.Keyboard.current.numpadMinusKey
					},
					{
						UnityEngine.KeyCode.KeypadPlus,
						UnityEngine.InputSystem.Keyboard.current.numpadPlusKey
					},
					{
						UnityEngine.KeyCode.KeypadEnter,
						UnityEngine.InputSystem.Keyboard.current.numpadEnterKey
					},
					{
						UnityEngine.KeyCode.KeypadEquals,
						UnityEngine.InputSystem.Keyboard.current.numpadEqualsKey
					}
				};
			}
			return _MacOS13UnityKeypadKeys;
		}
	}

	public static InputDeviceType activeControllerType
	{
		get
		{
			PlatformControlType controlType = PlatformManager.GetControlType();
			if (controlType == PlatformControlType.Deck || controlType == PlatformControlType.Controller)
			{
				return InputDeviceType.Gamepad;
			}
			return _activeControllerType;
		}
	}

	public static event Action<InputDevice> onActiveDeviceChanged;

	public void LateUpdate()
	{
		if (layersToEnableAtEndOfFrame != null)
		{
			List<string> list = layersToEnableAtEndOfFrame;
			layersToEnableAtEndOfFrame = null;
			list.ForEach((string l) =>
			{
				EnableLayer(l);
			});
		}
		CommandFramePressed.Clear();
		CommandBindingManager.LateUpdate();
		NeedsQueueUpdate = true;
	}

	public static bool GetButtonReleasedThisFrame(string Action)
	{
		if (!Application.isFocused || !Application.isPlaying)
		{
			return false;
		}
		return GameManager.Instance.player.GetButtonReleasedThisFrame(Action);
	}

	public static bool GetButtonPerformedThisFrame(string Action)
	{
		if (!Application.isFocused || !Application.isPlaying)
		{
			return false;
		}
		return GameManager.Instance.player.GetButtonPerformedThisFrame(Action);
	}

	public static bool GetButtonPressedThisFrame(string Action)
	{
		if (!Application.isFocused || !Application.isPlaying)
		{
			return false;
		}
		return GameManager.Instance.player.GetButtonPressedThisFrame(Action);
	}

	public static bool GetButtonDown(string Action, bool skipTutorialCheck = false)
	{
		if (!Application.isFocused || !Application.isPlaying)
		{
			return false;
		}
		return GameManager.Instance.player.GetButtonDown(Action, ignoreSkipframes: false, skipTutorialCheck);
	}

	public static bool GetButtonDownRepeating(string Action)
	{
		if (!Application.isFocused || !Application.isPlaying)
		{
			return false;
		}
		return GameManager.Instance.player.GetButtonDownRepeating(Action);
	}

	public static bool GetButton(string Action, bool forceEnable = false)
	{
		if (!Application.isFocused || !Application.isPlaying)
		{
			return false;
		}
		return GameManager.Instance.player.GetButton(Action, forceEnable);
	}

	public static void Skip(int min)
	{
		if (SkipFrames < min)
		{
			SkipFrames = min;
		}
	}

	public static void WaitForKeyup(string id)
	{
		lock (_WaitForKeyupList)
		{
			if (!_WaitForKeyupList.Contains(id))
			{
				_WaitForKeyupList.Add(id);
			}
		}
	}

	public static void AddCommandFramePressed(string command)
	{
		CommandFramePressed.Add(command);
	}

	public void Update()
	{
		if (NeedsQueueUpdate)
		{
			UpdateTheCommandQueue();
		}
		ControllerChangedThisLateUpdate = false;
		CommandFramePressed.Clear();
		bool flag = false;
		if (SkipFrames > 0 && !mappingMode)
		{
			SkipFrames--;
		}
		if (Application.isPlaying && (activeControllerType != lastControllerType || updateFont))
		{
			if (activeControllerType == InputDeviceType.Keyboard)
			{
				controllerFontType = ControllerFontType.Keyboard;
			}
			else
			{
				controllerFontType = Options.GetOption("OptionControllerFont") switch
				{
					"KBM" => ControllerFontType.ForceKBM, 
					"XBox" => ControllerFontType.XBox, 
					"PS" => ControllerFontType.PS5, 
					"XBox Filled" => ControllerFontType.XBoxFilled, 
					"PS Filled" => ControllerFontType.PS5Filled, 
					"Switch" => ControllerFontType.Switch, 
					"Switch Filled" => ControllerFontType.SwitchFilled, 
					_ => lastControllerFont, 
				};
			}
			lastControllerType = activeControllerType;
			updateFont = false;
			flag = true;
		}
		if (Application.isPlaying)
		{
			if (Input.anyKeyDown)
			{
				lastInputIsMouse = false;
			}
			if (Options.MouseInput && (Input.mousePosition != lastMousePosition || Input.GetMouseButton(1) || Input.GetMouseButton(2) || Input.GetMouseButton(3)))
			{
				lastMousePosition = Input.mousePosition;
				lastInputIsMouse = true;
			}
		}
		if (controllerFontType != lastControllerFontType)
		{
			lastControllerFontType = controllerFontType;
			TMP_Settings.fallbackFontAssets.RemoveAll((TMP_FontAsset f) => controllerFonts.Any((ControllerFontSelection cf) => cf.fontAsset == f) || f == keyboardGlyphFont);
			TMP_FontAsset fontAsset = controllerFonts.Find((ControllerFontSelection cf) => cf.controllerType == controllerFontType).fontAsset;
			if ((object)fontAsset != null)
			{
				TMP_Settings.fallbackFontAssets.Add(fontAsset);
			}
			TMP_Settings.fallbackFontAssets.Add(keyboardGlyphFont);
			TextMeshProUGUI[] array = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
			for (int num = 0; num < array.Length; num++)
			{
				array[num].ForceMeshUpdate(ignoreActiveState: false, forceTextReparsing: true);
			}
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		ControllerChangedThisLateUpdate = true;
		foreach (IControllerChangedEvent item in UnityEngine.Object.FindObjectsByType(typeof(MonoBehaviour), FindObjectsSortMode.None).OfType<IControllerChangedEvent>())
		{
			if (!(item is MonoBehaviour { isActiveAndEnabled: false }))
			{
				item.ControllerChanged();
			}
		}
	}

	private void Awake()
	{
		instance = this;
	}

	private void SetupControllerGlyphs(InputDevice device)
	{
	}

	private void setLastUsedDevice(InputDevice device)
	{
		if (device is Mouse || lastUsedDevice == device)
		{
			return;
		}
		lastUsedDevice = device;
		if (lastUsedDevice == Gamepad.current)
		{
			_activeControllerType = InputDeviceType.Gamepad;
			InputDevice inputDevice = lastUsedDevice;
			if (inputDevice != null && inputDevice.description.manufacturer.Contains("Sony"))
			{
				lastControllerFont = ControllerFontType.PS5;
			}
			else
			{
				lastControllerFont = ControllerFontType.XBox;
			}
			OverrideInputControlNames(lastUsedDevice);
		}
		else
		{
			_activeControllerType = InputDeviceType.Keyboard;
		}
		onActiveDeviceChanged?.Invoke(device);
	}

	private static void OverrideInputControlNames(InputDevice device)
	{
	}

	private void OnAnyButtonPress(InputControl control)
	{
		lastControlPressed = control;
	}

	private void OnInputSystemEvent(InputEventPtr eventPtr, InputDevice device)
	{
		if (lastUsedDevice == device)
		{
			return;
		}
		if (eventPtr.type == 1398030676)
		{
			using InputControlExtensions.InputEventControlEnumerator inputEventControlEnumerator = eventPtr.EnumerateChangedControls(device, 0.0001f).GetEnumerator();
			if (!inputEventControlEnumerator.MoveNext())
			{
				return;
			}
			_ = inputEventControlEnumerator.Current;
		}
		setLastUsedDevice(device);
	}

	private void OnInputSystemDeviceChange(InputDevice device, InputDeviceChange change)
	{
		if (lastUsedDevice != device)
		{
			if (device != null && !string.IsNullOrEmpty(device.name) && device.name.Contains("XInputControllerWindows"))
			{
				SuspectSteamInput = true;
			}
			setLastUsedDevice(device);
		}
	}

	public void Init()
	{
		ControllerFontType controllerFontType = PlatformManager.GetFontType();
		ControllerFontType controllerFontType2 = Options.GetOption("OptionControllerFont") switch
		{
			"XBox" => ControllerFontType.XBox, 
			"PS" => ControllerFontType.PS5, 
			"XBox Filled" => ControllerFontType.XBoxFilled, 
			"PS Filled" => ControllerFontType.PS5Filled, 
			"KBM" => ControllerFontType.ForceKBM, 
			_ => ControllerFontType.Default, 
		};
		if (controllerFontType2 != ControllerFontType.Default)
		{
			controllerFontType = controllerFontType2;
		}
		if (controllerFontType != ControllerFontType.Default)
		{
			this.controllerFontType = controllerFontType;
			Update();
		}
		InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
		InputSystem.onEvent += new Action<InputEventPtr, InputDevice>(OnInputSystemEvent);
		InputSystem.onDeviceChange += OnInputSystemDeviceChange;
		try
		{
			if (!(Options.GetOption("OptionDisableSteamInputWarning") == "No"))
			{
				return;
			}
			foreach (InputDevice device in InputSystem.devices)
			{
				MetricsManager.LogInfo("Detected device: " + device.name);
				if (!string.IsNullOrEmpty(device.name) && device.name.Contains("XInputControllerWindows"))
				{
					SuspectSteamInput = true;
				}
			}
		}
		catch
		{
		}
	}

	public static void EnableOnlyLayers(List<string> layersToEnable)
	{
		EnabledLayers.Clear();
		EnabledLayers.AddRange(layersToEnable);
		ResetInput();
		foreach (KeyValuePair<string, CommandBindingLayer> commandBindingLayer in CommandBindingManager.CommandBindingLayers)
		{
			if (!layersToEnable.Contains(commandBindingLayer.Key))
			{
				DisableLayer(commandBindingLayer.Key);
			}
		}
		foreach (string item in layersToEnable)
		{
			EnableLayer(item);
		}
	}

	public static void OnUpdate()
	{
	}

	public static void DisableLayer(CommandBindingLayer map)
	{
		if (GameManager.IsOnUIContext())
		{
			map.Disable();
		}
	}

	public static void EnableLayer(CommandBindingLayer map)
	{
		if (GameManager.IsOnUIContext())
		{
			map.Enable();
		}
	}

	public static void DisableAllLayers()
	{
		foreach (KeyValuePair<string, CommandBindingLayer> commandBindingLayer in CommandBindingManager.CommandBindingLayers)
		{
			DisableLayer(commandBindingLayer.Value);
		}
	}

	public static void EnableLayer(string layer)
	{
		if (layersToEnableAtEndOfFrame != null)
		{
			layersToEnableAtEndOfFrame = null;
		}
		if (CommandBindingManager.CommandBindingLayers.TryGetValue(layer, out var value))
		{
			EnableLayer(value);
		}
	}

	public static bool IsLayerEnabled(string layer)
	{
		if (CommandBindingManager.CommandBindingLayers.TryGetValue(layer, out var value))
		{
			return value.enabled;
		}
		return false;
	}

	public static void DisableLayer(string layer)
	{
		if (layersToEnableAtEndOfFrame != null)
		{
			MetricsManager.LogError("Trying to disable a layer in the same frame as a resetinput this might be a problem.");
			layersToEnableAtEndOfFrame = null;
		}
		if (CommandBindingManager.CommandBindingLayers.TryGetValue(layer, out var value))
		{
			DisableLayer(value);
		}
	}

	public static string mapRewiredIDToLegacyID(string commandId)
	{
		if (_mapRewiredIDToLegacyID.TryGetValue(commandId, out var value))
		{
			return value;
		}
		return commandId;
	}

	public static List<UnityEngine.KeyCode> GetHotkeySpread(List<string> layersToInclude = null)
	{
		if (layersToInclude == null)
		{
			layersToInclude = EnabledLayers;
		}
		List<UnityEngine.KeyCode> list = new List<UnityEngine.KeyCode>();
		for (int i = 97; i <= 122; i++)
		{
			if (!isKeyMapped((UnityEngine.KeyCode)i, layersToInclude))
			{
				list.Add((UnityEngine.KeyCode)i);
			}
		}
		for (int j = 48; j <= 57; j++)
		{
			if (!isKeyMapped((UnityEngine.KeyCode)j, layersToInclude))
			{
				list.Add((UnityEngine.KeyCode)j);
			}
		}
		return list;
	}

	public static char GetHotkeyCharFor(string Name, IReadOnlyList<char> Hotkeys)
	{
		if (Name.IsNullOrEmpty())
		{
			return '\0';
		}
		bool flag = false;
		int i = 0;
		int length = Name.Length;
		int count = Hotkeys.Count;
		for (; i < length; i++)
		{
			char c = (flag ? char.ToUpper(Name[i]) : char.ToLower(Name[i]));
			if (!char.IsLetter(c))
			{
				continue;
			}
			bool flag2 = false;
			for (int j = 0; j < count; j++)
			{
				if (Hotkeys[j] == c)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				return c;
			}
			if (!flag && i == length - 1)
			{
				flag = true;
				i = 0;
			}
		}
		return '\0';
	}

	public static LegacyKeyCode mapCommandToPrimaryLegacyKeycode(string id)
	{
		string key = mapRewiredIDToLegacyID(id);
		foreach (KeyValuePair<string, Dictionary<string, int>> item in CommandBindingManager.CurrentMap.PrimaryMapCommandToKeyLayer)
		{
			if (item.Value.TryGetValue(key, out var value) && consoleKeycodeToUnityKeycode.TryGetValue(value & 0xFFFF, out var value2))
			{
				return new LegacyKeyCode
				{
					rawCode = value,
					code = value2,
					bCtrl = ((value & 0x20000) > 0),
					bShift = ((value & 0x10000) > 0),
					bAlt = ((value & 0x40000) > 0)
				};
			}
		}
		return new LegacyKeyCode
		{
			code = UnityEngine.KeyCode.None
		};
	}

	public static LegacyKeyCode mapCommandToSecondaryLegacyKeycode(string id)
	{
		string key = mapRewiredIDToLegacyID(id);
		foreach (KeyValuePair<string, Dictionary<string, int>> item in CommandBindingManager.CurrentMap.SecondaryMapCommandToKeyLayer)
		{
			if (item.Value.TryGetValue(key, out var value) && consoleKeycodeToUnityKeycode.TryGetValue(value & 0xFFFF, out var value2))
			{
				return new LegacyKeyCode
				{
					rawCode = value,
					code = value2,
					bCtrl = ((value & 0x20000) > 0),
					bShift = ((value & 0x10000) > 0),
					bAlt = ((value & 0x40000) > 0)
				};
			}
		}
		return new LegacyKeyCode
		{
			rawCode = 0,
			code = UnityEngine.KeyCode.None
		};
	}

	public static bool isKeyMapped(UnityEngine.KeyCode keycode, bool bAlt, bool bControl, bool bShift)
	{
		ConsoleLib.Console.Keyboard.InitKeymap();
		if (!ConsoleLib.Console.Keyboard.Keymap.ContainsKey(keycode))
		{
			return false;
		}
		return isKeyMapped(ConsoleLib.Console.Keyboard.Keymap[keycode] + (bAlt ? 262144 : 0) + (bControl ? 131072 : 0) + (bShift ? 65536 : 0));
	}

	public static bool isKeyMapped(char c, List<string> layersToInclude = null)
	{
		if (char.IsUpper(c))
		{
			return isKeyMapped((Keys)(65536 + char.ToUpperInvariant(c)), layersToInclude);
		}
		return isKeyMapped((Keys)char.ToUpperInvariant(c), layersToInclude);
	}

	public static bool isCommandMapped(string id)
	{
		return CommandBindingManager.CommandBindings[id].IsMapped();
	}

	public static bool isKeyMapped(UnityEngine.KeyCode key, List<string> layersToInclude = null)
	{
		ConsoleLib.Console.Keyboard.InitKeymap();
		if (layersToInclude == null)
		{
			layersToInclude = EnabledLayers;
		}
		return isKeyMapped(ConsoleLib.Console.Keyboard.Keymap[key], layersToInclude);
	}

	public static bool isKeyMapped(Keys key, List<string> layersToInclude = null)
	{
		try
		{
			return CommandBindingManager.DoesAnyLayerConsumeKeycode(key, layersToInclude);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("isKeyMapped", x);
		}
		return false;
	}

	public static bool isKey(UnityEngine.KeyCode key)
	{
		if (mappingMode)
		{
			return false;
		}
		if (currentContext.isInInput() && keysIgnoredWhileInAnInputField.Any(Input.GetKey))
		{
			return false;
		}
		return Input.GetKey(key);
	}

	public static bool isHotkeyDown(UnityEngine.KeyCode key)
	{
		if (mappingMode)
		{
			return false;
		}
		if (key == UnityEngine.KeyCode.None)
		{
			return false;
		}
		if (currentContext.isInInput() && keysIgnoredWhileInAnInputField.Any(Input.GetKey))
		{
			return false;
		}
		if (isKeyMapped(key))
		{
			return false;
		}
		return Input.GetKeyDown(key);
	}

	public static bool isKeyDown(UnityEngine.KeyCode key)
	{
		if (mappingMode)
		{
			return false;
		}
		if (currentContext.isInInput() && keysIgnoredWhileInAnInputField.Any(Input.GetKey))
		{
			return false;
		}
		return Input.GetKeyDown(key);
	}

	public static bool isCharDown(char c)
	{
		if (Input.GetKey(UnityEngine.KeyCode.LeftAlt) || Input.GetKey(UnityEngine.KeyCode.RightAlt) || Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightControl))
		{
			return false;
		}
		if (mappingMode)
		{
			return false;
		}
		if (currentContext.isInInput())
		{
			for (int i = 0; i < keysIgnoredWhileInAnInputField.Count; i++)
			{
				if (Input.GetKey(keysIgnoredWhileInAnInputField[i]))
				{
					return false;
				}
			}
		}
		ConsoleLib.Console.Keyboard.InitKeymap();
		if (!char.IsNumber(c) && Input.inputString != null && Input.inputString.Contains(c))
		{
			return true;
		}
		bool flag = false;
		if (char.IsLetter(c) && char.IsUpper(c))
		{
			flag = true;
		}
		if (ConsoleLib.Console.Keyboard.reverselccharmap.TryGetValue(c, out var value))
		{
			for (int j = 0; j < value.Count; j++)
			{
				if (!isKeyDown(value[j]))
				{
					continue;
				}
				if (!flag)
				{
					if (!Input.GetKey(UnityEngine.KeyCode.LeftShift))
					{
						return !Input.GetKey(UnityEngine.KeyCode.RightShift);
					}
					return false;
				}
				return true;
			}
			return false;
		}
		if (ConsoleLib.Console.Keyboard.reverseuccharmap.TryGetValue(c, out value))
		{
			for (int k = 0; k < value.Count; k++)
			{
				if (!isKeyDown(value[k]))
				{
					continue;
				}
				if (flag)
				{
					if (!Input.GetKey(UnityEngine.KeyCode.LeftShift))
					{
						return Input.GetKey(UnityEngine.KeyCode.RightShift);
					}
					return true;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public static void ResetBindingCaches()
	{
		gamepadCommandInputsWithGlyphs?.Clear();
		gamepadCommandInputsWithoutGlyphs?.Clear();
		keyboardCommandInputsWithGlyphs?.Clear();
		keyboardCommandInputsWithoutGlyphs?.Clear();
		if (CommandBindingManager.CommandBindings == null)
		{
			return;
		}
		foreach (KeyValuePair<string, CommandBinding> commandBinding in CommandBindingManager.CommandBindings)
		{
			keyboardCommandInputsWithGlyphs?.Add(commandBinding.Key, getCommandInputDescription(commandBinding.Key, mapGlyphs: true, allowAlt: false, InputDeviceType.Keyboard));
			keyboardCommandInputsWithoutGlyphs?.Add(commandBinding.Key, getCommandInputDescription(commandBinding.Key, mapGlyphs: false, allowAlt: false, InputDeviceType.Keyboard));
			gamepadCommandInputsWithGlyphs?.Add(commandBinding.Key, getCommandInputDescription(commandBinding.Key, mapGlyphs: true, allowAlt: false, InputDeviceType.Gamepad));
			gamepadCommandInputsWithoutGlyphs?.Add(commandBinding.Key, getCommandInputDescription(commandBinding.Key, mapGlyphs: false, allowAlt: false, InputDeviceType.Gamepad));
		}
	}

	public static string getCommandInputFormatted(string id, bool mapGlyphs = true, bool allBinds = false)
	{
		string commandInputDescription = getCommandInputDescription(id, mapGlyphs, allowAlt: false, InputDeviceType.Unknown, allBinds);
		if (string.IsNullOrEmpty(commandInputDescription))
		{
			return "";
		}
		return "{{hotkey|" + commandInputDescription + "}}";
	}

	public static string getCommandInputDescription(string id, bool mapGlyphs = true, bool allowAlt = false, InputDeviceType deviceType = InputDeviceType.Unknown, bool allBinds = false)
	{
		if (deviceType == InputDeviceType.Unknown)
		{
			deviceType = activeControllerType;
			if (instance.controllerFontType == ControllerFontType.ForceKBM)
			{
				deviceType = InputDeviceType.Keyboard;
			}
		}
		if (mapGlyphs && !Options.ModernUI)
		{
			mapGlyphs = false;
		}
		if (id == "NavigationXYAxis")
		{
			if (deviceType != InputDeviceType.Gamepad)
			{
				return "\ue80a";
			}
			return "\ue90a";
		}
		if (!allBinds)
		{
			if (mapGlyphs)
			{
				if (deviceType == InputDeviceType.Gamepad)
				{
					if (gamepadCommandInputsWithGlyphs.ContainsKey(id))
					{
						return gamepadCommandInputsWithGlyphs[id];
					}
				}
				else if (keyboardCommandInputsWithGlyphs.ContainsKey(id))
				{
					return keyboardCommandInputsWithGlyphs[id];
				}
			}
			else if (deviceType == InputDeviceType.Gamepad)
			{
				if (gamepadCommandInputsWithoutGlyphs.ContainsKey(id))
				{
					return gamepadCommandInputsWithoutGlyphs[id];
				}
			}
			else if (keyboardCommandInputsWithoutGlyphs.ContainsKey(id))
			{
				return keyboardCommandInputsWithoutGlyphs[id];
			}
		}
		if (CommandBindingManager.CommandBindings == null)
		{
			return "";
		}
		if (!CommandBindingManager.CommandBindings.ContainsKey(id))
		{
			return "";
		}
		if (!GameManager.IsOnUIContext())
		{
			return "NEEDUICONTEXT";
		}
		if (CommandBindingManager.CommandBindings.ContainsKey(id) && CommandBindingManager.CommandBindings[id].IsMapped())
		{
			if (allBinds)
			{
				using (Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder())
				{
					foreach (string commandBinding in CommandBindingManager.GetCommandBindings(id, (deviceType != InputDeviceType.Gamepad) ? InputDeviceType.Keyboard : InputDeviceType.Gamepad))
					{
						string value = ((!mapGlyphs) ? (commandBinding ?? "") : ConvertBindingTextToGlyphs(commandBinding ?? "", deviceType));
						if (!string.IsNullOrEmpty(value))
						{
							if (utf16ValueStringBuilder.Length > 0)
							{
								utf16ValueStringBuilder.Append(" {{y|or}} ");
							}
							utf16ValueStringBuilder.Append(value);
						}
					}
					if (utf16ValueStringBuilder.Length == 0)
					{
						return "{{K|<nothing bound to this command>}}";
					}
					return utf16ValueStringBuilder.ToString();
				}
			}
			if (mapGlyphs)
			{
				string text = CommandBindingManager.GetCommandBindings(id, (deviceType != InputDeviceType.Gamepad) ? InputDeviceType.Keyboard : InputDeviceType.Gamepad).FirstOrDefault() ?? "";
				string text2 = ConvertBindingTextToGlyphs(text, deviceType);
				if (text != text2)
				{
					return text2;
				}
			}
			return CommandBindingManager.GetCommandBindings(id, (deviceType != InputDeviceType.Gamepad) ? InputDeviceType.Keyboard : InputDeviceType.Gamepad, forGlyph: false).FirstOrDefault() ?? "";
		}
		switch (id)
		{
		case "NavigationXYAxis":
			return "\ue90a";
		case "Page Up":
			return "PgUp";
		case "Page Left":
			return "Home";
		case "Page Down":
			return "PgDown";
		case "Page Right":
			return "End";
		case "U Positive":
			return "Insert";
		case "U Negative":
			return "Delete";
		case "V Positive":
			return "KeyPad+";
		case "V Negative":
			return "KeyPad-";
		default:
		{
			string text3 = ConsoleLib.Console.Keyboard.MetaToString(CommandBindingManager.GetKeyFromCommand(id));
			if (text3 == "None")
			{
				if (id == "Cancel")
				{
					return "Esc";
				}
				if (id == "Accept")
				{
					return "Space";
				}
				return null;
			}
			return ConvertModifierGlyphs(text3);
		}
		}
	}

	private static void enqueueCommand(FrameCommand cmd)
	{
		if (cmd != null)
		{
			commandPool.Enqueue(cmd);
		}
	}

	public static void ConsumeAllInput()
	{
		if (FrameDownCommand != null)
		{
			enqueueCommand(FrameDownCommand);
		}
		FrameDownCommand = null;
		CommandQueue?.Clear();
	}

	public static void ConsumeCurrentInput()
	{
		if (FrameDownCommand != null)
		{
			enqueueCommand(FrameDownCommand);
		}
		FrameDownCommand = null;
	}

	public static void ConsumeAllInput(string cmd)
	{
		foreach (FrameCommand item in CommandQueue)
		{
			if (item.match(cmd) > 0)
			{
				item.ignore = true;
			}
		}
	}

	public static void ResetInput(bool disableLayers = false, bool LeaveMovementEvents = true)
	{
		SkipFrames = 2;
		ConsoleLib.Console.Keyboard.ClearInput(LeaveMovementEvents);
		StickN = 0;
		StickS = 0;
		StickE = 0;
		StickW = 0;
		PauseStickNTillReleased = true;
		PauseStickSTillReleased = true;
		PauseStickETillReleased = true;
		PauseStickWTillReleased = true;
		if (!GameManager.IsOnUIContext())
		{
			return;
		}
		if (disableLayers && layersToEnableAtEndOfFrame == null && CommandBindingManager.CommandBindingLayers != null)
		{
			layersToEnableAtEndOfFrame = (from kv in CommandBindingManager.CommandBindingLayers
				where kv.Value.enabled
				select kv.Key).ToList();
			DisableAllLayers();
		}
		Input.ResetInputAxes();
	}

	public static bool GetLegacyKeyDown(UnityEngine.KeyCode button, bool repeat = false)
	{
		if (mappingMode)
		{
			return false;
		}
		if (GameManager.IsOnUIContext() && !Application.isPlaying)
		{
			return false;
		}
		if (button == UnityEngine.KeyCode.None)
		{
			return false;
		}
		if (isKeyDown(button))
		{
			delayTimers.Set(button, delaytime);
			repeatTimers.Remove(button);
			return true;
		}
		if (!repeat)
		{
			return false;
		}
		if (isKey(button))
		{
			if (!delayTimers.ContainsKey(button))
			{
				delayTimers.Add(button, delaytime);
			}
			else if (delayTimers[button] <= 0f)
			{
				if (!repeatTimers.ContainsKey(button))
				{
					repeatTimers.Add(button, repeattime);
				}
				else
				{
					repeatTimers[button] -= Time.deltaTime;
					if (repeatTimers[button] <= 0f)
					{
						repeatTimers[button] = repeattime;
						return true;
					}
				}
			}
			else
			{
				delayTimers[button] -= Time.deltaTime;
			}
		}
		else
		{
			delayTimers.Remove(button);
			repeatTimers.Remove(button);
		}
		return false;
	}

	public static bool GetLegacyKeyCodeDown(LegacyKeyCode button, bool repeat = false)
	{
		if (mappingMode)
		{
			return false;
		}
		if (GameManager.IsOnUIContext() && !Application.isPlaying)
		{
			return false;
		}
		if (button.code == UnityEngine.KeyCode.None)
		{
			return false;
		}
		if (isKeyDown(button.code) && (!button.bCtrl || isKey(UnityEngine.KeyCode.LeftControl) || isKey(UnityEngine.KeyCode.RightControl)) && (!button.bAlt || isKey(UnityEngine.KeyCode.LeftShift) || isKey(UnityEngine.KeyCode.RightShift)) && (!button.bShift || isKey(UnityEngine.KeyCode.LeftAlt) || isKey(UnityEngine.KeyCode.RightAlt)))
		{
			delayTimers2.Set(button.rawCode, delaytime);
			repeatTimers2.Remove(button.rawCode);
			return true;
		}
		if (!repeat)
		{
			return false;
		}
		if (isKey(button.code) && (!button.bCtrl || isKey(UnityEngine.KeyCode.LeftControl) || isKey(UnityEngine.KeyCode.RightControl)) && (!button.bAlt || isKey(UnityEngine.KeyCode.LeftShift) || isKey(UnityEngine.KeyCode.RightShift)) && (!button.bShift || isKey(UnityEngine.KeyCode.LeftAlt) || isKey(UnityEngine.KeyCode.RightAlt)))
		{
			if (!delayTimers2.ContainsKey(button.rawCode))
			{
				delayTimers2.Add(button.rawCode, delaytime);
			}
			else if (delayTimers2[button.rawCode] <= 0f)
			{
				if (!repeatTimers2.ContainsKey(button.rawCode))
				{
					repeatTimers2.Add(button.rawCode, repeattime);
				}
				else
				{
					repeatTimers2[button.rawCode] -= Time.deltaTime;
					if (repeatTimers2[button.rawCode] <= 0f)
					{
						repeatTimers2[button.rawCode] = repeattime;
						return true;
					}
				}
			}
			else
			{
				delayTimers2[button.rawCode] -= Time.deltaTime;
			}
		}
		else
		{
			delayTimers2.Remove(button.rawCode);
			repeatTimers2.Remove(button.rawCode);
		}
		return false;
	}

	public static bool isCommandPressed(string id)
	{
		if (NeedsQueueUpdate)
		{
			UpdateTheCommandQueue();
		}
		if (mappingMode)
		{
			return false;
		}
		if (SkipFrames > 0)
		{
			return false;
		}
		if (SynchronizationContext.Current == The.UiContext && !Application.isPlaying)
		{
			return false;
		}
		if (CommandBindingManager.CommandBindings.TryGetValue(id, out var value))
		{
			return value.IsPressed();
		}
		return false;
	}

	public static bool isCommandReleasedThisFrame(string id)
	{
		if (NeedsQueueUpdate)
		{
			UpdateTheCommandQueue();
		}
		if (mappingMode)
		{
			return false;
		}
		if (SkipFrames > 0)
		{
			return false;
		}
		if (SynchronizationContext.Current == The.UiContext && !Application.isPlaying)
		{
			return false;
		}
		if (CommandBindingManager.CommandBindings.TryGetValue(id, out var value))
		{
			return value.WasReleasedThisFrame();
		}
		return false;
	}

	public static bool isCommandPerformedThisFrame(string id)
	{
		if (NeedsQueueUpdate)
		{
			UpdateTheCommandQueue();
		}
		if (mappingMode)
		{
			return false;
		}
		if (SkipFrames > 0)
		{
			return false;
		}
		if (SynchronizationContext.Current == The.UiContext && !Application.isPlaying)
		{
			return false;
		}
		if (CommandBindingManager.CommandBindings.TryGetValue(id, out var value))
		{
			return value.WasPerformedThisFrame();
		}
		return false;
	}

	public static bool isCommandPressedThisFrame(string id)
	{
		if (NeedsQueueUpdate)
		{
			UpdateTheCommandQueue();
		}
		if (mappingMode)
		{
			return false;
		}
		if (SkipFrames > 0)
		{
			return false;
		}
		if (SynchronizationContext.Current == The.UiContext && !Application.isPlaying)
		{
			return false;
		}
		if (CommandBindingManager.CommandBindings.TryGetValue(id, out var value))
		{
			return value.WasPressedThisFrame();
		}
		return false;
	}

	public static bool isCommandDown(string id, bool repeat = true, bool ignoreSkipframes = false, bool skipTutorialCheck = false)
	{
		return isCommandDownValue(id, repeat, ignoreSkipframes, skipTutorialCheck) > 0;
	}

	public static bool isConsoleCommandDown(string id)
	{
		switch (id)
		{
		case "Accept":
			return ConsoleLib.Console.Keyboard.Char == 32;
		case "Cancel":
			return ConsoleLib.Console.Keyboard.Char == 3;
		case "Page Left":
			return ConsoleLib.Console.Keyboard.Char == 103;
		case "Page Right":
			return ConsoleLib.Console.Keyboard.Char == 105;
		case "Page Up":
			return ConsoleLib.Console.Keyboard.Char == 33;
		case "Page Down":
			return ConsoleLib.Console.Keyboard.Char == 34;
		case "V Positive":
			return ConsoleLib.Console.Keyboard.Char == 107;
		case "V Negative":
			return ConsoleLib.Console.Keyboard.Char == 109;
		case "CmdMoveN":
			return ConsoleLib.Console.Keyboard.Char == 98;
		case "CmdMoveS":
			return ConsoleLib.Console.Keyboard.Char == 102;
		case "CmdMoveE":
			return ConsoleLib.Console.Keyboard.Char == 100;
		case "CmdMoveW":
			return ConsoleLib.Console.Keyboard.Char == 102;
		case "CmdMoveNE":
			return ConsoleLib.Console.Keyboard.Char == 105;
		case "CmdMoveSE":
			return ConsoleLib.Console.Keyboard.Char == 99;
		case "CmdMoveSW":
			return ConsoleLib.Console.Keyboard.Char == 97;
		case "CmdMoveNW":
			return ConsoleLib.Console.Keyboard.Char == 103;
		default:
		{
			string text = "Command:" + id;
			if (ConsoleLib.Console.Keyboard.Char == 252)
			{
				return ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == text;
			}
			return false;
		}
		}
	}

	private static FrameCommand nextFrameCommand()
	{
		if (commandPool.Count > 0)
		{
			return commandPool.Dequeue() ?? new FrameCommand();
		}
		return new FrameCommand();
	}

	public static void UpdateTheCommandQueue()
	{
		if (_WaitForKeyupList.Count > 0)
		{
			lock (_WaitForKeyupList)
			{
				for (int num = _WaitForKeyupList.Count - 1; num >= 0; num--)
				{
					if (!CommandBindingManager.CommandBindings[_WaitForKeyupList[num]].IsPressed())
					{
						_WaitForKeyupList.RemoveAt(num);
					}
				}
			}
		}
		if (lastFrame == Time.frameCount)
		{
			Debug.LogError("updating the command queue twice in the same frame! " + Environment.StackTrace.ToString());
		}
		else
		{
			if (!NeedsQueueUpdate)
			{
				return;
			}
			lastFrame = Time.frameCount;
			NeedsQueueUpdate = false;
			if (FrameDownCommand != null)
			{
				commandPool.Enqueue(FrameDownCommand);
			}
			FrameDownCommand = null;
			currentFrameCommands.Clear();
			string resolvedDirection = ResolveAxisDirection("UI:Navigate");
			StickN = DoStick("UI:Navigate", "N", resolvedDirection);
			if (PauseStickNTillReleased && StickN == 0 && !commandDelayTimers.ContainsKey("__STICKNAV_N") && !commandRepeatTimers.ContainsKey("__STICKNAV_N") && !commandDelayTimers.ContainsKey("CmdMoveN") && !commandRepeatTimers.ContainsKey("CmdMoveN"))
			{
				PauseStickNTillReleased = false;
			}
			StickS = DoStick("UI:Navigate", "S", resolvedDirection);
			if (PauseStickSTillReleased && StickS == 0 && !commandDelayTimers.ContainsKey("__STICKNAV_S") && !commandRepeatTimers.ContainsKey("__STICKNAV_S") && !commandDelayTimers.ContainsKey("CmdMoveS") && !commandRepeatTimers.ContainsKey("CmdMoveS"))
			{
				PauseStickSTillReleased = false;
			}
			StickE = DoStick("UI:Navigate", "E", resolvedDirection);
			if (PauseStickETillReleased && StickE == 0 && !commandDelayTimers.ContainsKey("__STICKNAV_E") && !commandRepeatTimers.ContainsKey("__STICKNAV_E") && !commandDelayTimers.ContainsKey("CmdMoveE") && !commandRepeatTimers.ContainsKey("CmdMoveE"))
			{
				PauseStickETillReleased = false;
			}
			StickW = DoStick("UI:Navigate", "W", resolvedDirection);
			if (PauseStickWTillReleased && StickW == 0 && !commandDelayTimers.ContainsKey("__STICKNAV_W") && !commandRepeatTimers.ContainsKey("__STICKNAV_W") && !commandDelayTimers.ContainsKey("CmdMoveW") && !commandRepeatTimers.ContainsKey("CmdMoveW"))
			{
				PauseStickWTillReleased = false;
			}
			for (int i = 0; i < CommandBindingManager.CommandBindingLayersList.Count; i++)
			{
				CommandBindingLayer commandBindingLayer = CommandBindingManager.CommandBindingLayersList[i];
				for (int j = 0; j < commandBindingLayer.actions.Count; j++)
				{
					CommandBinding commandBinding = commandBindingLayer.actions[j];
					if (_WaitForKeyupList.Count > 0)
					{
						lock (_WaitForKeyupList)
						{
							for (int num2 = 0; num2 < _WaitForKeyupList.Count; num2++)
							{
								if (_WaitForKeyupList[num2] == commandBinding.name)
								{
									goto IL_0618;
								}
							}
						}
					}
					int num3;
					int num4;
					if (commandBinding.WasPressedThisFrame())
					{
						if (currentFrameCommands.Count > 0)
						{
							num3 = 0;
							while (num3 < currentFrameCommands.Count)
							{
								if (!currentFrameCommands[num3].binding.SharesBindingsWith(commandBinding))
								{
									num3++;
									continue;
								}
								goto IL_0353;
							}
						}
						FrameCommand frameCommand = nextFrameCommand();
						frameCommand.reset();
						frameCommand.binding = commandBinding;
						frameCommand.id = commandBinding.name;
						frameCommand.isRepeat = false;
						currentFrameCommands.Add(frameCommand);
						CommandQueue.Enqueue(frameCommand);
						commandRepeatTimers.Remove(commandBinding.name);
						commandDelayTimers[commandBinding.name] = 0f;
					}
					else if (commandBinding.IsPressed() && commandBinding.command != null && commandBinding.name != null)
					{
						if (commandRepeatTimers.ContainsKey(commandBinding.name))
						{
							commandRepeatTimers[commandBinding.name] += Time.deltaTime;
							if (!(commandRepeatTimers[commandBinding.name] >= repeattime))
							{
								continue;
							}
							commandRepeatTimers[commandBinding.name] -= repeattime;
							if (currentFrameCommands.Count > 0)
							{
								num4 = 0;
								while (num4 < currentFrameCommands.Count)
								{
									if (!currentFrameCommands[num4].binding.SharesBindingsWith(commandBinding))
									{
										num4++;
										continue;
									}
									goto IL_04cd;
								}
							}
							FrameCommand frameCommand2 = nextFrameCommand();
							frameCommand2.reset();
							frameCommand2.binding = commandBinding;
							frameCommand2.id = commandBinding.name;
							frameCommand2.isRepeat = true;
							currentFrameCommands.Add(frameCommand2);
							CommandQueue.Enqueue(frameCommand2);
						}
						else
						{
							if (!commandDelayTimers.ContainsKey(commandBinding.name))
							{
								commandDelayTimers.Add(commandBinding.name, 0f);
							}
							commandDelayTimers[commandBinding.name] += Time.deltaTime;
							if (commandDelayTimers[commandBinding.name] >= delaytime)
							{
								commandRepeatTimers.Add(commandBinding.name, 0f);
							}
						}
					}
					else
					{
						if (commandDelayTimers.ContainsKey(commandBinding.name))
						{
							commandDelayTimers.Remove(commandBinding.name);
						}
						if (commandRepeatTimers.ContainsKey(commandBinding.name))
						{
							commandRepeatTimers.Remove(commandBinding.name);
						}
					}
					continue;
					IL_0353:
					currentFrameCommands[num3].aliases.Add(commandBinding.name);
					continue;
					IL_04cd:
					currentFrameCommands[num4].aliases.Add(commandBinding.name);
					IL_0618:;
				}
			}
			if (SkipFrames > 0)
			{
				return;
			}
			while (CommandQueue.Count > 0)
			{
				FrameCommand frameCommand3 = CommandQueue.Dequeue();
				if (frameCommand3.ignore)
				{
					commandPool.Enqueue(frameCommand3);
				}
				else
				{
					if (CommandBindingManager.CommandsByID.ContainsKey(frameCommand3.id) && !CommandBindingManager.CommandBindingLayers[CommandBindingManager.CommandsByID[frameCommand3.id].Layer].enabled)
					{
						continue;
					}
					if (frameCommand3?.id != "IndicateDirection")
					{
						if (FrameDownCommand != null)
						{
							commandPool.Enqueue(FrameDownCommand);
						}
						FrameDownCommand = frameCommand3;
						ConsoleLib.Console.Keyboard.KeyEvent.Set();
					}
					return;
				}
			}
			if (FrameDownCommand == null)
			{
				ConsoleLib.Console.Keyboard.ResetIfQueuesEmpty();
			}
		}
	}

	private static int DoStick(string axis, string direction, string resolvedDirection)
	{
		string key = null;
		if (direction == "N")
		{
			key = "__STICKNAV_N";
		}
		if (direction == "S")
		{
			key = "__STICKNAV_S";
		}
		if (direction == "E")
		{
			key = "__STICKNAV_E";
		}
		if (direction == "W")
		{
			key = "__STICKNAV_W";
		}
		if (resolvedDirection == direction)
		{
			if (!commandDelayTimers.ContainsKey(key))
			{
				commandDelayTimers.Add(key, 0f);
				return 1;
			}
			if (!commandRepeatTimers.ContainsKey(key))
			{
				commandDelayTimers[key] += Time.deltaTime;
				if (commandDelayTimers[key] >= delaytime)
				{
					commandRepeatTimers.Add(key, 0f);
					return 2;
				}
			}
			else if (commandRepeatTimers.ContainsKey(key))
			{
				commandRepeatTimers[key] += Time.deltaTime;
				if (commandRepeatTimers[key] >= repeattime)
				{
					commandRepeatTimers[key] -= repeattime;
					return 2;
				}
			}
		}
		else
		{
			if (commandDelayTimers.ContainsKey(key))
			{
				commandDelayTimers.Remove(key);
			}
			if (commandRepeatTimers.ContainsKey(key))
			{
				commandRepeatTimers.Remove(key);
			}
		}
		return 0;
	}

	public static int isCommandDownValue(string id, bool repeat = true, bool ignoreSkipframes = false, bool skipTutorialCheck = false)
	{
		if (mappingMode)
		{
			return 0;
		}
		if (NeedsQueueUpdate)
		{
			UpdateTheCommandQueue();
		}
		if (!ignoreSkipframes && SkipFrames > 0)
		{
			return 0;
		}
		if (_WaitForKeyupList.Count > 0)
		{
			lock (_WaitForKeyupList)
			{
				for (int i = 0; i < _WaitForKeyupList.Count; i++)
				{
					if (_WaitForKeyupList[i] == id)
					{
						return 0;
					}
				}
			}
		}
		if (SynchronizationContext.Current == The.UiContext && !Application.isPlaying)
		{
			return 0;
		}
		if (currentContext.isInInput() && keysIgnoredWhileInAnInputField.Any(Input.GetKey))
		{
			return 0;
		}
		if (id == "Navigate Up")
		{
			if (PauseStickNTillReleased)
			{
				return 0;
			}
			if (StickN > 0)
			{
				return StickN;
			}
		}
		if (id == "Navigate Down")
		{
			if (PauseStickSTillReleased)
			{
				return 0;
			}
			if (StickS > 0)
			{
				return StickS;
			}
		}
		if (id == "Navigate Left")
		{
			if (PauseStickWTillReleased)
			{
				return 0;
			}
			if (StickW > 0)
			{
				return StickW;
			}
		}
		if (id == "Navigate Right")
		{
			if (PauseStickETillReleased)
			{
				return 0;
			}
			if (StickE > 0)
			{
				return StickE;
			}
		}
		if (FrameDownCommand == null)
		{
			return 0;
		}
		int num = FrameDownCommand.match(id);
		if (num != 0 && !skipTutorialCheck && !TutorialManager.AllowCommand(id))
		{
			return 0;
		}
		return num;
	}

	public static string ResolveAxisDirection(string Axis)
	{
		if (!CommandBindingManager.CommandBindings.ContainsKey(Axis))
		{
			return null;
		}
		Vector2 vector = CommandBindingManager.CommandBindings[Axis].ReadValue<Vector2>();
		float num = vector.x;
		float num2 = vector.y;
		if (!upActions.TryGetValue(Axis, out var value))
		{
			upActions.Add(Axis, Axis + "/up");
			value = upActions[Axis];
		}
		if (!downActions.TryGetValue(Axis, out var value2))
		{
			downActions.Add(Axis, Axis + "/down");
			value2 = downActions[Axis];
		}
		if (!leftActions.TryGetValue(Axis, out var value3))
		{
			leftActions.Add(Axis, Axis + "/left");
			value3 = leftActions[Axis];
		}
		if (!rightActions.TryGetValue(Axis, out var value4))
		{
			rightActions.Add(Axis, Axis + "/right");
			value4 = rightActions[Axis];
		}
		if (isCommandPressed(value) || (Axis == "UI:Navigate" && isCommandPressed("CmdMoveN")))
		{
			num2 = 1f;
		}
		if (isCommandPressed(value2) || (Axis == "UI:Navigate" && isCommandPressed("CmdMoveS")))
		{
			num2 = -1f;
		}
		if (isCommandPressed(value3) || (Axis == "UI:Navigate" && isCommandPressed("CmdMoveW")))
		{
			num = -1f;
		}
		if (isCommandPressed(value4) || (Axis == "UI:Navigate" && isCommandPressed("CmdMoveE")))
		{
			num = 1f;
		}
		float num3 = 0.4f;
		if (num > num3 && num2 > num3)
		{
			return "NE";
		}
		if (num > num3 && num2 < 0f - num3)
		{
			return "SE";
		}
		if (num < 0f - num3 && num2 > num3)
		{
			return "NW";
		}
		if (num < 0f - num3 && num2 < 0f - num3)
		{
			return "SW";
		}
		if (num < 0f - num3)
		{
			return "W";
		}
		if (num > num3)
		{
			return "E";
		}
		if (num2 < 0f - num3)
		{
			return "S";
		}
		if (num2 > num3)
		{
			return "N";
		}
		return null;
	}

	public static bool GetLegacyCommandDown(string id, bool repeat = true)
	{
		LegacyKeyCode button = mapCommandToPrimaryLegacyKeycode(id);
		LegacyKeyCode button2 = mapCommandToSecondaryLegacyKeycode(id);
		if (currentContext.isInInput())
		{
			if (button.code == UnityEngine.KeyCode.Backspace)
			{
				button.code = UnityEngine.KeyCode.None;
			}
			if (button2.code == UnityEngine.KeyCode.Backspace)
			{
				button2.code = UnityEngine.KeyCode.None;
			}
		}
		if (!GetLegacyKeyCodeDown(button, repeat))
		{
			return GetLegacyKeyCodeDown(button2, repeat);
		}
		return true;
	}

	public static async Task<T> SuspendControlsWhile<T>(Func<Task<T>> action)
	{
		List<string> activeLayers = (from kv in CommandBindingManager.CommandBindingLayers
			where kv.Value.enabled
			select kv.Key).ToList();
		mappingMode = true;
		DisableAllLayers();
		try
		{
			return await action();
		}
		finally
		{
			DisableAllLayers();
			activeLayers.ForEach(EnableLayer);
			ResetInput(disableLayers: true);
			mappingMode = false;
		}
	}

	public static string _ConvertBindingTextToGlyphs(string input, InputDeviceType type)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		if (type == InputDeviceType.Keyboard || type == InputDeviceType.Mouse)
		{
			return Sidebar.ToCP437(ConvertModifierGlyphs(input));
		}
		if (controllerGlyphs.TryGetValue(input, out var value))
		{
			return value;
		}
		value = CommandBindingManager.GetOverrideNameFromBinding(input);
		if (!string.IsNullOrEmpty(value))
		{
			return value;
		}
		return Sidebar.ToCP437(input);
	}

	public static string ConvertBindingTextToGlyphs(string input, InputDeviceType type)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		if (input.Contains("+") && !input.StartsWith("+") && !input.EndsWith("+") && !input.Contains("++"))
		{
			string text = "";
			string[] array = input.Split("+");
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					text += "+";
				}
				text += _ConvertBindingTextToGlyphs(array[i], type);
			}
			return text;
		}
		return _ConvertBindingTextToGlyphs(input, type);
	}

	public static string ConvertModifierGlyphs(string input)
	{
		return input?.Replace("Control", "\ue816").Replace("Ctrl", "\ue816").Replace("Alt", "\ue818")
			.Replace("Shift", "\ue802")
			.Replace("LMB", "\ue809")
			.Replace("RMB", "\ue814");
	}
}
