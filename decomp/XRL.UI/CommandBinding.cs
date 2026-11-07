using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace XRL.UI;

public class CommandBinding : IDisposable
{
	public enum EVALUATE_MODE
	{
		IS_PRESSED,
		WAS_PERFORMED_THIS_FRAME,
		WAS_PRESSED_THIS_FRAME,
		WAS_RELEASED_THIS_FRAME
	}

	public bool enabled;

	public bool isDefault = true;

	public GameCommand command;

	public InputAction _inputAction;

	public List<InputAction>[] keyboardSubActions = new List<InputAction>[16];

	public List<InputAction>[] mouseSubActions = new List<InputAction>[16];

	public List<InputAction>[] gamepadSubActions = new List<InputAction>[16];

	public bool UseSubactions = true;

	public string name => _inputAction?.name ?? "(null)";

	public bool SharesBindingsWith(CommandBinding with)
	{
		if (command != null && with.command != null)
		{
			return command.SharesBindsWith(with.command);
		}
		return true;
	}

	public void Enable()
	{
		enabled = true;
		if (!_inputAction.enabled)
		{
			_inputAction.Enable();
		}
	}

	public void Disable()
	{
		enabled = false;
	}

	public bool IsMapped()
	{
		InputAction inputAction = _inputAction;
		if (inputAction == null)
		{
			return false;
		}
		return inputAction.bindings.Count > 0;
	}

	public IEnumerable<int> GetConsoleKeycodes()
	{
		foreach (int item in _inputAction?.GetConsoleKeycodes())
		{
			yield return item;
		}
	}

	public T ReadValue<T>() where T : struct
	{
		return _inputAction.ReadValue<T>();
	}

	public void InitSubactions()
	{
		if (_inputAction == null || _inputAction.bindings.Count <= 0)
		{
			return;
		}
		int num = 0;
		string text = null;
		int num2 = 0;
		for (int i = 0; i < _inputAction.bindings.Count; i++)
		{
			InputBinding inputBinding = _inputAction.bindings[i];
			if (inputBinding.isComposite)
			{
				if (inputBinding.path == "OneModifier")
				{
					num2 = 2;
				}
				else if (inputBinding.path == "TwoModifiers")
				{
					num2 = 3;
				}
				else
				{
					MetricsManager.LogError("Unknown composiite " + inputBinding.path);
				}
			}
			else if (inputBinding.isPartOfComposite)
			{
				CommandBindingManager.GetGamepadAltBindings();
				if (CommandBindingManager.keyboardModifierFlags.ContainsKey(inputBinding.path))
				{
					num = (int)(num + CommandBindingManager.keyboardModifierFlags[inputBinding.path]);
				}
				else if (CommandBindingManager.GetGamepadAltBindings() == inputBinding.path)
				{
					num += 8;
				}
				else
				{
					text = inputBinding.path;
				}
				num2--;
			}
			else
			{
				text = inputBinding.path;
			}
			if (inputBinding.isComposite || num2 != 0)
			{
				continue;
			}
			InputAction inputAction = new InputAction(_inputAction.name + "_subaction_" + num + "|" + text, _inputAction.type);
			inputAction.AddBinding(text);
			if (inputBinding.path.StartsWith("<Keyboard>"))
			{
				if (keyboardSubActions[num] == null)
				{
					keyboardSubActions[num] = new List<InputAction>();
				}
				keyboardSubActions[num].Add(inputAction);
			}
			else if (inputBinding.path.StartsWith("<Gamepad>"))
			{
				if (gamepadSubActions[num] == null)
				{
					gamepadSubActions[num] = new List<InputAction>();
				}
				gamepadSubActions[num].Add(inputAction);
			}
			else if (inputBinding.path.StartsWith("<Mouse>"))
			{
				if (mouseSubActions[num] == null)
				{
					mouseSubActions[num] = new List<InputAction>();
				}
				mouseSubActions[num].Add(inputAction);
			}
			else
			{
				MetricsManager.LogError("Unknown binding path while generating subactions " + inputBinding.path);
			}
			inputAction.Enable();
			num = 0;
		}
	}

	public bool Evaluate(EVALUATE_MODE mode)
	{
		if (UseSubactions)
		{
			int frameKeyboardModifier = CommandBindingManager.GetFrameKeyboardModifier();
			List<InputAction> list;
			if (keyboardSubActions != null && (list = keyboardSubActions[frameKeyboardModifier]) != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (mode == EVALUATE_MODE.IS_PRESSED && list[i].IsPressed())
					{
						return true;
					}
					if (mode == EVALUATE_MODE.WAS_PERFORMED_THIS_FRAME && list[i].WasPerformedThisFrame())
					{
						return true;
					}
					if (mode == EVALUATE_MODE.WAS_PRESSED_THIS_FRAME && list[i].WasPressedThisFrame())
					{
						return true;
					}
					if (mode == EVALUATE_MODE.WAS_RELEASED_THIS_FRAME && list[i].WasReleasedThisFrame())
					{
						return true;
					}
				}
			}
			if (mouseSubActions != null && (list = mouseSubActions[frameKeyboardModifier]) != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (mode == EVALUATE_MODE.IS_PRESSED && list[j].IsPressed())
					{
						return true;
					}
					if (mode == EVALUATE_MODE.WAS_PERFORMED_THIS_FRAME && list[j].WasPerformedThisFrame())
					{
						return true;
					}
					if (mode == EVALUATE_MODE.WAS_PRESSED_THIS_FRAME && list[j].WasPressedThisFrame())
					{
						return true;
					}
					if (mode == EVALUATE_MODE.WAS_RELEASED_THIS_FRAME && list[j].WasReleasedThisFrame())
					{
						return true;
					}
				}
			}
			int frameGamepadModifier = CommandBindingManager.GetFrameGamepadModifier();
			if (gamepadSubActions != null && (list = gamepadSubActions[frameGamepadModifier]) != null)
			{
				for (int k = 0; k < list.Count; k++)
				{
					if (mode == EVALUATE_MODE.IS_PRESSED && list[k].IsPressed())
					{
						return true;
					}
					if (mode == EVALUATE_MODE.WAS_PERFORMED_THIS_FRAME && list[k].WasPerformedThisFrame())
					{
						return true;
					}
					if (mode == EVALUATE_MODE.WAS_PRESSED_THIS_FRAME && list[k].WasPressedThisFrame())
					{
						return true;
					}
					if (mode == EVALUATE_MODE.WAS_RELEASED_THIS_FRAME && list[k].WasReleasedThisFrame())
					{
						return true;
					}
				}
			}
			return false;
		}
		return mode switch
		{
			EVALUATE_MODE.IS_PRESSED => _inputAction.IsPressed(), 
			EVALUATE_MODE.WAS_PERFORMED_THIS_FRAME => _inputAction.WasPerformedThisFrame(), 
			EVALUATE_MODE.WAS_PRESSED_THIS_FRAME => _inputAction.WasPressedThisFrame(), 
			EVALUATE_MODE.WAS_RELEASED_THIS_FRAME => _inputAction.WasReleasedThisFrame(), 
			_ => false, 
		};
	}

	private bool _IsPressed(InputAction a)
	{
		return a.IsPressed();
	}

	public bool IsPressed(bool forceEnable = false)
	{
		if (!forceEnable && !enabled)
		{
			return false;
		}
		return Evaluate(EVALUATE_MODE.IS_PRESSED);
	}

	private bool _WasPerformedThisFrame(InputAction a)
	{
		return a.WasPerformedThisFrame();
	}

	public bool WasPerformedThisFrame(bool ignoreEnabled = false)
	{
		if (!enabled && !ignoreEnabled)
		{
			return false;
		}
		return Evaluate(EVALUATE_MODE.WAS_PERFORMED_THIS_FRAME);
	}

	public bool WasReleasedThisFrame(bool ignoreEnabled = false)
	{
		if (!enabled && !ignoreEnabled)
		{
			return false;
		}
		return Evaluate(EVALUATE_MODE.WAS_RELEASED_THIS_FRAME);
	}

	private bool _WasPressedThisFrame(InputAction a)
	{
		return a.WasPressedThisFrame();
	}

	public bool WasPressedThisFrame(bool ignoreEnabled = false)
	{
		if (!enabled && !ignoreEnabled)
		{
			return false;
		}
		return Evaluate(EVALUATE_MODE.WAS_PRESSED_THIS_FRAME);
	}

	public List<string> SerializedFormat()
	{
		if (isDefault)
		{
			return null;
		}
		return _inputAction.SerializedFormat();
	}

	public void Dispose()
	{
		if (keyboardSubActions != null)
		{
			List<InputAction>[] array = keyboardSubActions;
			foreach (List<InputAction> list in array)
			{
				if (list == null)
				{
					continue;
				}
				foreach (InputAction item in list)
				{
					item?.Dispose();
				}
			}
		}
		if (mouseSubActions != null)
		{
			List<InputAction>[] array = mouseSubActions;
			foreach (List<InputAction> list2 in array)
			{
				if (list2 == null)
				{
					continue;
				}
				foreach (InputAction item2 in list2)
				{
					item2?.Dispose();
				}
			}
		}
		if (gamepadSubActions != null)
		{
			List<InputAction>[] array = gamepadSubActions;
			foreach (List<InputAction> list3 in array)
			{
				if (list3 == null)
				{
					continue;
				}
				foreach (InputAction item3 in list3)
				{
					item3?.Dispose();
				}
			}
		}
		for (int j = 0; j < 16; j++)
		{
			mouseSubActions[j]?.Clear();
			keyboardSubActions[j]?.Clear();
			gamepadSubActions[j]?.Clear();
		}
		_inputAction?.Dispose();
		_inputAction = null;
	}

	public static CommandBinding FromSerializedFormat(InputAction action, GameCommand cmd, List<string> bindings, bool AllowLegacyUpgrade, string targetSet)
	{
		CommandBinding commandBinding = new CommandBinding();
		commandBinding.command = cmd;
		commandBinding._inputAction = action;
		if (bindings != null)
		{
			commandBinding.isDefault = false;
			int num = 0;
			while (num < bindings.Count)
			{
				if (bindings[num] == InputSystemExtensions.COMPOSITE)
				{
					InputActionSetupExtensions.CompositeSyntax compositeSyntax = action.AddCompositeBinding(bindings[num + 1]);
					if (bindings[num + 1] == "OneModifier")
					{
						compositeSyntax.With("Binding", bindings[num + 2]);
						compositeSyntax.With("Modifier", resolveAlt(bindings[num + 3]));
						num += 4;
					}
					else if (bindings[num + 1] == "TwoModifiers")
					{
						compositeSyntax.With("Binding", bindings[num + 2]);
						compositeSyntax.With("Modifier1", resolveAlt(bindings[num + 3]));
						compositeSyntax.With("Modifier2", resolveAlt(bindings[num + 4]));
						num += 5;
					}
					else
					{
						MetricsManager.LogError("Unknown composite type " + bindings[num + 1] + " - aborting load for this action");
						num++;
					}
				}
				else
				{
					action.AddBinding(bindings[num++]);
				}
			}
		}
		else
		{
			KeyMap keyMap = (AllowLegacyUpgrade ? CommandBindingManager.GetLegacyKeymap() : null);
			if ((cmd.keyboardBindings.Count > 0) | AllowLegacyUpgrade)
			{
				bool flag = false;
				try
				{
					try
					{
						if (keyMap != null && !cmd.SkipUpgrade)
						{
							List<int> list = new List<int>();
							try
							{
								list.Add(keyMap.PrimaryMapCommandToKeyLayer.Where((KeyValuePair<string, Dictionary<string, int>> l) => l.Value.Any((KeyValuePair<string, int> legacyCmd) => legacyCmd.Key.ToLower() == cmd.UpgradeFrom.ToLower())).FirstOrDefault().Value.Where((KeyValuePair<string, int> v) => v.Key.ToLower() == cmd.ID.ToLower())?.FirstOrDefault().Value ?? 0);
							}
							catch
							{
							}
							try
							{
								IEnumerable<KeyValuePair<string, int>> source = keyMap.SecondaryMapCommandToKeyLayer.Where((KeyValuePair<string, Dictionary<string, int>> l) => l.Value.Any((KeyValuePair<string, int> legacyCmd) => legacyCmd.Key.ToLower() == cmd.UpgradeFrom.ToLower())).FirstOrDefault().Value.Where((KeyValuePair<string, int> v) => v.Key.ToLower() == cmd.ID.ToLower());
								list.Add(source.FirstOrDefault().Value);
							}
							catch
							{
							}
							foreach (int item in list)
							{
								if (CommandBindingManager.AddKeysValueToActionAsBinding(item, action))
								{
									flag = true;
									commandBinding.isDefault = false;
								}
							}
						}
					}
					catch (Exception)
					{
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("KeyMapping upgrade old keymap", x);
				}
				if (!flag)
				{
					foreach (GameCommand.KeyboardBinding keyboardBinding in cmd.keyboardBindings)
					{
						if (!string.IsNullOrEmpty(keyboardBinding.Set) && !(keyboardBinding.Set == targetSet) && (!(keyboardBinding.Set == "default") || !string.IsNullOrEmpty(targetSet)))
						{
							continue;
						}
						List<string> list2 = keyboardBinding.Modifier?.CachedCommaExpansion();
						if (list2 == null || list2.Count == 0)
						{
							action.AddBinding("<Keyboard>/" + keyboardBinding.Key);
							continue;
						}
						if (list2.Count == 1)
						{
							action.AddCompositeBinding("OneModifier").With("Binding", "<Keyboard>/" + keyboardBinding.Key).With("Modifier", "<Keyboard>/" + list2[0]);
							continue;
						}
						if (list2.Count == 2)
						{
							action.AddCompositeBinding("TwoModifiers").With("Binding", "<Keyboard>/" + keyboardBinding.Key).With("Modifier1", "<Keyboard>/" + list2[0])
								.With("Modifier2", "<Keyboard>/" + list2[1]);
							continue;
						}
						throw new Exception("Invalid or too many modifiers on " + cmd.ID + "!");
					}
				}
			}
			foreach (GameCommand.MouseBinding mouseBinding in cmd.mouseBindings)
			{
				if (!string.IsNullOrEmpty(mouseBinding.Set) && !(mouseBinding.Set == targetSet) && (!(mouseBinding.Set == "default") || !string.IsNullOrEmpty(targetSet)))
				{
					continue;
				}
				List<string> list3 = mouseBinding.Modifier?.CachedCommaExpansion();
				if (list3 == null || list3.Count == 0)
				{
					action.AddBinding("<Mouse>/" + mouseBinding.Button);
					continue;
				}
				if (list3.Count == 1)
				{
					action.AddCompositeBinding("OneModifier").With("Binding", "<Mouse>/" + mouseBinding.Button).With("Modifier", "<Keyboard>/" + list3[0]);
					continue;
				}
				if (list3.Count == 2)
				{
					action.AddCompositeBinding("TwoModifiers").With("Binding", "<Mouse>/" + mouseBinding.Button).With("Modifier1", "<Keyboard>/" + list3[0])
						.With("Modifier2", "<Keyboard>/" + list3[1]);
					continue;
				}
				throw new Exception("Invalid or too many modifiers on " + cmd.ID + "!");
			}
			if (cmd.gamepadBindings.Count > 0)
			{
				foreach (GameCommand.GamepadBinding gamepadBinding in cmd.gamepadBindings)
				{
					if (!gamepadBinding.Alt)
					{
						action.AddBinding("<Gamepad>/" + gamepadBinding.Button);
					}
					else
					{
						action.AddCompositeBinding("OneModifier").With("Binding", "<Gamepad>/" + gamepadBinding.Button).With("Modifier", CommandBindingManager.CurrentMap.ResolveGamepadAltBind());
					}
				}
			}
		}
		if (action.name == "GamepadAlt")
		{
			commandBinding.UseSubactions = false;
			CommandBindingManager.gamepadAltAction = commandBinding._inputAction;
		}
		else
		{
			commandBinding.InitSubactions();
		}
		return commandBinding;
		static string resolveAlt(string binding)
		{
			if (binding == InputSystemExtensions.GAMEPADALT || binding.StartsWith("<Gamepad>"))
			{
				return CommandBindingManager.CurrentMap.ResolveGamepadAltBind();
			}
			return binding;
		}
	}

	public static CommandBinding FromInputAction(InputAction action)
	{
		CommandBinding commandBinding = new CommandBinding();
		commandBinding._inputAction = action;
		if (action.name == "GamepadAlt")
		{
			commandBinding.UseSubactions = false;
			CommandBindingManager.gamepadAltAction = commandBinding._inputAction;
		}
		else
		{
			commandBinding.InitSubactions();
		}
		return commandBinding;
	}
}
