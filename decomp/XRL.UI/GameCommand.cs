using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace XRL.UI;

public class GameCommand
{
	public class KeyboardBinding
	{
		public string Key;

		public string Modifier;

		public string Set;
	}

	public class MouseBinding
	{
		public string Button;

		public string Modifier;

		public string Set;
	}

	public class GamepadBinding
	{
		public string Button;

		public bool Alt;
	}

	public string ID;

	public string DisplayText;

	public string ConsoleDisplayText;

	public string Category;

	public string Layer;

	public string Display;

	public bool Bindable = true;

	public string InputModuleBind;

	public bool SkipUpgrade;

	public string UpgradeFrom;

	public string Auto;

	public string Instrument;

	public bool Required;

	public InputActionType Type = InputActionType.Button;

	public CapabilityManager.CapabilityType? RequiresCapability;

	public List<string> CanShareBindsWith;

	public List<KeyboardBinding> keyboardBindings = new List<KeyboardBinding>();

	public List<MouseBinding> mouseBindings = new List<MouseBinding>();

	public List<GamepadBinding> gamepadBindings = new List<GamepadBinding>();

	public bool IgnoreConflicts => Auto == "DownPass";

	public bool CanRemoveBinding()
	{
		if (!Required)
		{
			return true;
		}
		return CommandBindingManager.GetCommandBindings(ID).Count() > 1;
	}

	public bool CanRemoveBinding(ControlManager.InputDeviceType deviceType)
	{
		if (!Required)
		{
			return true;
		}
		return CommandBindingManager.GetCommandBindings(ID, deviceType).Count() > 1;
	}

	public bool SharesBindsWith(GameCommand with)
	{
		List<string> canShareBindsWith = CanShareBindsWith;
		if (canShareBindsWith == null || !canShareBindsWith.Contains(with?.ID))
		{
			return with?.CanShareBindsWith?.Contains(ID) == true;
		}
		return true;
	}
}
