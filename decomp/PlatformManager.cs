using System;
using System.Threading.Tasks;
using Steamworks;

public static class PlatformManager
{
	public enum TryGetNetworkResult
	{
		Success,
		Failure,
		Cancelled
	}

	public static bool m_inOfflineMode = false;

	public static readonly SteamManager Steam = new SteamManager();

	public static bool AreLeaderboardsOffline()
	{
		return m_inOfflineMode;
	}

	public static async Task<TryGetNetworkResult> TryGetLeaderboardsReady()
	{
		return (!Steam.Initialized) ? TryGetNetworkResult.Failure : TryGetNetworkResult.Success;
	}

	public static void TryGetLeaderboardsReady(Action<bool> onComplete)
	{
		onComplete?.Invoke(Steam.Initialized);
	}

	public static void SetOfflineMode(bool isOffline)
	{
		m_inOfflineMode = isOffline;
	}

	public static bool IsInitialized()
	{
		return Steam.Initialized;
	}

	public static void Awake()
	{
		Steam.Awake();
	}

	public static void UpdateAchievement(AchievementInfo Achievement)
	{
		if (Steam.Active)
		{
			Steam.UpdateAchievement(Achievement.ID, ref Achievement.Achieved, ref Achievement.TimeStamp);
		}
	}

	public static void UpdateStat(StatInfo Stat)
	{
		if (Steam.Active)
		{
			Steam.UpdateStat(Stat.ID, ref Stat.Value);
		}
	}

	public static bool GetAchievement(AchievementInfo Achievement)
	{
		if (Steam.Active)
		{
			return Steam.GetAchievement(Achievement.ID);
		}
		return false;
	}

	public static bool TrySetAchievement(AchievementInfo Achievement)
	{
		if (Steam.Active && Steam.SetAchievement(Achievement.ID))
		{
			return true;
		}
		return false;
	}

	public static int GetStat(StatInfo Stat)
	{
		if (Steam.Active)
		{
			return Steam.GetStat(Stat.ID);
		}
		return 0;
	}

	public static void SetStat(StatInfo Stat)
	{
		if (Steam.Active)
		{
			Steam.SetStat(Stat.ID, Stat.Value);
		}
	}

	public static void Synchronize()
	{
		if (Steam.Active)
		{
			Steam.Synchronize();
		}
	}

	public static void Update()
	{
		if (Steam.Initialized)
		{
			Steam.Update();
		}
	}

	public static void Shutdown()
	{
		MetricsManager.LogInfo("Platform - Shutdown Starting");
		Steam.Shutdown();
		MetricsManager.LogInfo("Platform - Shutdown Complete");
	}

	public static void ResetAchievements()
	{
		if (Steam.Active)
		{
			Steam.ResetAchievements();
		}
	}

	public static bool TryShowAchievementProgress(AchievementInfo Achievement)
	{
		if (Achievement.Progress == null)
		{
			return false;
		}
		int value = Achievement.Progress.Value;
		int achievedAt = Achievement.AchievedAt;
		if (Steam.Active && Steam.TryShowAchievementProgress(Achievement.ID, value, achievedAt))
		{
			return true;
		}
		return false;
	}

	public static bool IsOverlayEnabled()
	{
		return false;
	}

	public static PlatformControlType GetControlType()
	{
		if (Steam.Initialized)
		{
			return Steam.GetControlType();
		}
		return PlatformControlType.Undefined;
	}

	public static string GetOptionDefault(string OptionID)
	{
		string result = null;
		if (Steam.Initialized)
		{
			result = Steam.GetOptionDefault(OptionID);
		}
		return result;
	}

	public static bool TryShowVirtualKeyboard(VirtualKeyboardType Type = VirtualKeyboardType.SingleLine, int X = 0, int Y = 0, int Width = 80, int Height = 1)
	{
		if (Steam.Initialized && Steam.TryShowVirtualKeyboard(Type, X, Y, Width, Height))
		{
			return true;
		}
		return false;
	}

	public static ControlManager.ControllerFontType GetFontType()
	{
		if (Steam.Initialized)
		{
			return Steam.GetFontType();
		}
		return ControlManager.ControllerFontType.Default;
	}
}
