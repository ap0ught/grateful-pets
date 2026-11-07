using System;
using Qud.UI;
using UnityEngine;
using XRL.Collections;

namespace Steamworks;

public class SteamManager
{
	public const uint APP_ID = 333640u;

	public bool Active;

	public bool Initialized;

	public bool Synchronized;

	private bool Deck;

	private bool Store = true;

	internal Rack<IDisposable> Disposables = new Rack<IDisposable>();

	public void ResetAchievements()
	{
		Debug.Log("STEAM - Resetting achievements.");
		SteamUserStats.ResetAllStats(bAchievementsToo: true);
		Store = true;
	}

	public void Synchronize()
	{
		if (!AchievementManager.Active || Synchronized)
		{
			return;
		}
		Synchronized = true;
		AchievementState state = AchievementManager.State;
		bool flag = false;
		foreach (AchievementInfo value in state.Achievements.Values)
		{
			if (!value.ID.IsNullOrEmpty())
			{
				flag |= UpdateAchievement(value.ID, ref value.Achieved, ref value.TimeStamp);
			}
		}
		foreach (StatInfo value2 in state.Stats.Values)
		{
			if (!value2.ID.IsNullOrEmpty())
			{
				flag |= UpdateStat(value2.ID, ref value2.Value);
			}
		}
		AchievementManager.Write |= flag;
	}

	public bool TryShowVirtualKeyboard(VirtualKeyboardType Type = VirtualKeyboardType.SingleLine, int X = 0, int Y = 0, int Width = 80, int Height = 1)
	{
		try
		{
			return SteamUtils.ShowFloatingGamepadTextInput(Type switch
			{
				VirtualKeyboardType.MultiLine => EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeMultipleLines, 
				VirtualKeyboardType.Numeric => EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeNumeric, 
				_ => EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine, 
			}, X, Y, Width, Height);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Showing onscreen keyboard", x);
		}
		return false;
	}

	public PlatformControlType GetControlType()
	{
		if (Deck)
		{
			return PlatformControlType.Deck;
		}
		return PlatformControlType.Undefined;
	}

	public ControlManager.ControllerFontType GetFontType()
	{
		return ControlManager.ControllerFontType.Default;
	}

	public bool GetAchievement(string ID)
	{
		if (SteamUserStats.GetAchievement(ID, out var pbAchieved))
		{
			return pbAchieved;
		}
		return false;
	}

	public bool SetAchievement(string ID)
	{
		if (Active && SteamUserStats.SetAchievement(ID))
		{
			Store = true;
			return true;
		}
		return false;
	}

	public bool UpdateAchievement(string ID, ref bool Achieved, ref DateTime TimeStamp)
	{
		if (!Initialized)
		{
			return false;
		}
		if (!SteamUserStats.GetAchievementAndUnlockTime(ID, out var pbAchieved, out var punUnlockTime))
		{
			return false;
		}
		bool result = false;
		if (punUnlockTime != 0 && TimeStamp == DateTime.MinValue)
		{
			TimeStamp = DateTimeOffset.FromUnixTimeSeconds(punUnlockTime).DateTime;
			result = true;
		}
		if (Achieved && !pbAchieved)
		{
			SteamUserStats.SetAchievement(ID);
			Store = true;
			return result;
		}
		if (!Achieved & pbAchieved)
		{
			Achieved = true;
			return true;
		}
		return result;
	}

	public int GetStat(string ID)
	{
		if (SteamUserStats.GetStat(ID, out int pData))
		{
			return pData;
		}
		return 0;
	}

	public void SetStat(string ID, int Value)
	{
		Store = SteamUserStats.SetStat(ID, Value);
	}

	public bool UpdateStat(string ID, ref int Value)
	{
		if (!SteamUserStats.GetStat(ID, out int pData))
		{
			return false;
		}
		if (Value > pData)
		{
			SetStat(ID, Value);
			return false;
		}
		if (Value < pData)
		{
			Value = pData;
			return true;
		}
		return false;
	}

	public bool TryShowAchievementProgress(string ID, int Value, int Max)
	{
		if (SteamUtils.IsOverlayEnabled())
		{
			return SteamUserStats.IndicateAchievementProgress(ID, (uint)Value, (uint)Max);
		}
		return false;
	}

	public void Awake()
	{
		Active = false;
		Initialized = false;
		Synchronized = false;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		int i = 0;
		for (int num = commandLineArgs.Length; i < num; i++)
		{
			if (commandLineArgs[i].EqualsNoCase("STEAM:NO"))
			{
				Debug.Log("STEAM - Disabled via command line argument.");
				return;
			}
		}
		if (!Packsize.Test())
		{
			Debug.LogError("STEAM - Packsize test failed, the wrong version of Steamworks.NET is being run on this platform.");
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("STEAM - Dll test failed, one or more of the Steamworks binaries seems to be the wrong version.");
		}
		ESteamAPIInitResult eSteamAPIInitResult = SteamAPI.InitEx(out var OutSteamErrMsg);
		Initialized = eSteamAPIInitResult == ESteamAPIInitResult.k_ESteamAPIInitResult_OK;
		if (!Initialized)
		{
			Debug.Log($"STEAM - could not be initalized: {eSteamAPIInitResult} ({OutSteamErrMsg})");
			return;
		}
		Deck = SteamUtils.IsSteamRunningOnSteamDeck();
		Debug.Log("STEAM - Requesting user stats.");
		Disposables.Add(Callback<UserStatsReceived_t>.Create(OnUserStatsReceived));
		SteamUserStats.RequestCurrentStats();
		CheckEntitlements();
	}

	public void CheckEntitlements()
	{
		MetricsManager.LogInfo("Checking entitlements");
		uint value = 3252660u;
		uint value2 = 1247760u;
		uint value3 = 5249200u;
		bool flag = SteamApps.BIsDlcInstalled(new AppId_t(value)) || SteamApps.BIsSubscribedApp(new AppId_t(value)) || SteamApps.BIsAppInstalled(new AppId_t(value));
		MetricsManager.LogInfo(flag ? "has Pets Pack 1" : "does not have Pets Pack 1");
		bool flag2 = SteamApps.BIsDlcInstalled(new AppId_t(value2)) || SteamApps.BIsAppInstalled(new AppId_t(value2)) || SteamApps.BIsAppInstalled(new AppId_t(value2));
		MetricsManager.LogInfo(flag2 ? "has OST" : "does not have OST");
		bool num = SteamApps.BIsDlcInstalled(new AppId_t(value3)) || SteamApps.BIsAppInstalled(new AppId_t(value3)) || SteamApps.BIsAppInstalled(new AppId_t(value3));
		MetricsManager.LogInfo(flag2 ? "has Diaspora" : "does not have Diaspora");
		if (flag & flag2)
		{
			MainMenu.HasDromadDeluxeEntitlement = true;
		}
		if (num)
		{
			MainMenu.HasDiasporaEntitlement = true;
		}
	}

	private void OnUserStatsReceived(UserStatsReceived_t Callback)
	{
		if (Callback.m_eResult == EResult.k_EResultOK)
		{
			Debug.Log("STEAM - User stats retrieved.");
			Active = true;
			Synchronize();
		}
		else
		{
			Debug.Log($"STEAM - Failed to retrieve user stats ({Callback.m_eResult}).");
			Active = false;
		}
	}

	public void Shutdown()
	{
		if (!Initialized)
		{
			return;
		}
		Debug.Log("STEAM - Shutting down.");
		Initialized = false;
		Active = false;
		foreach (IDisposable disposable in Disposables)
		{
			disposable.Dispose();
		}
		Disposables.Clear();
		SteamAPI.Shutdown();
	}

	public void Update()
	{
		SteamAPI.RunCallbacks();
		if (Store)
		{
			SteamUserStats.StoreStats();
			Store = false;
		}
	}

	public string GetOptionDefault(string OptionID)
	{
		if (Deck)
		{
			switch (OptionID)
			{
			case "OptionBugDeckKeyboardShift":
				return "Yes";
			case "OptionsPrereleaseInputManager":
				return "Yes";
			case "OptionPrereleaseStageScale":
				return "1.25";
			case "OptionDisplayFramerate":
				return "120";
			}
		}
		return null;
	}
}
