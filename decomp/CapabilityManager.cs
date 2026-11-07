using System;
using System.Collections.Generic;
using LaundryBear.PlatformServices;
using UnityEngine;
using XRL.UI;

public class CapabilityManager : MonoBehaviour
{
	public enum CapabilityType
	{
		Unknown,
		OnscreenKeyboard,
		CanQuit,
		CanModifyWindowSettings,
		SupportsSystemUsers,
		SupportsSystemAchievements,
		IsDesktop,
		IsSwitch,
		Mouse,
		HardwareKeyboard,
		Mods,
		Analytics,
		SteamInput,
		UnlimitedStorage,
		LimitedWriteFrequency,
		RequiresStorageExpansion
	}

	public enum PlatformClassification
	{
		PC,
		Console
	}

	public static CapabilityManager instance;

	private static Dictionary<string, CapabilityType> StringToCapability;

	public static GameManager gameManager => GameManager.Instance;

	public static bool AllowClipboardFunctionality => true;

	public static bool AllowKeyboardHotkeys => true;

	public static void SuggestOnscreenKeyboard()
	{
		if (!(Options.GetOption("OptionFloatingKeyboard") != "Yes") && PlatformManager.GetControlType() == PlatformControlType.Deck)
		{
			PlatformManager.TryShowVirtualKeyboard();
		}
	}

	public static CapabilityType CapabilityEnumFromString(string capability)
	{
		if (StringToCapability == null)
		{
			StringToCapability = new Dictionary<string, CapabilityType>();
			foreach (CapabilityType value3 in Enum.GetValues(typeof(CapabilityType)))
			{
				StringToCapability.Add(value3.ToString(), value3);
			}
		}
		if (StringToCapability.TryGetValue(capability, out var value2))
		{
			return value2;
		}
		return CapabilityType.Unknown;
	}

	public static bool HasCapability(string capability)
	{
		return HasCapability(CapabilityEnumFromString(capability));
	}

	public static bool HasCapability(CapabilityType? capability)
	{
		switch (capability)
		{
		case null:
			return true;
		case CapabilityType.OnscreenKeyboard:
			return PlatformFeatureGameObjectSwitcher.CurrentPlatformSupportsFeature(PlatformFeature.SupportsOnScreenKeyboard);
		case CapabilityType.CanQuit:
			return PlatformFeatureGameObjectSwitcher.CurrentPlatformSupportsFeature(PlatformFeature.CanQuit);
		case CapabilityType.CanModifyWindowSettings:
			return PlatformFeatureGameObjectSwitcher.CurrentPlatformSupportsFeature(PlatformFeature.CanModifyWindowSettings);
		case CapabilityType.SupportsSystemUsers:
			return PlatformFeatureGameObjectSwitcher.CurrentPlatformSupportsFeature(PlatformFeature.SupportsSystemUsers);
		case CapabilityType.SupportsSystemAchievements:
			return PlatformFeatureGameObjectSwitcher.CurrentPlatformSupportsFeature(PlatformFeature.SupportsSystemAchievements);
		case CapabilityType.Mouse:
			return PlatformFeatureGameObjectSwitcher.CurrentPlatformSupportsFeature(PlatformFeature.SupportsMouse);
		case CapabilityType.HardwareKeyboard:
			return PlatformFeatureGameObjectSwitcher.CurrentPlatformSupportsFeature(PlatformFeature.SupportsHardwareKeyboard);
		case CapabilityType.IsDesktop:
		case CapabilityType.Mods:
		case CapabilityType.Analytics:
		case CapabilityType.SteamInput:
		case CapabilityType.UnlimitedStorage:
			return GetStandaloneOnlyCapability(capability);
		case CapabilityType.IsSwitch:
		case CapabilityType.LimitedWriteFrequency:
		case CapabilityType.RequiresStorageExpansion:
			return GetSwitchOnlyCapability(capability);
		default:
			return false;
		}
	}

	private static bool GetStandaloneOnlyCapability(CapabilityType? capability)
	{
		if (capability == CapabilityType.OnscreenKeyboard)
		{
			return PlatformManager.GetControlType() == PlatformControlType.Deck;
		}
		return true;
	}

	private static bool GetSwitchOnlyCapability(CapabilityType? capability)
	{
		return false;
	}

	public static PlatformClassification CurrentPlatformClassification()
	{
		return PlatformClassification.PC;
	}

	private void Awake()
	{
	}

	public string GetDefaultOptionOverrideForCapabilities(string Option, string Default)
	{
		return PlatformManager.GetOptionDefault(Option) ?? Default;
	}

	public void Init()
	{
		instance = this;
	}
}
