using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Platform.IO;
using UnityEngine;
using XRL;
using XRL.Messages;
using XRL.Wish;

public static class AchievementManager
{
	[HasWishCommand]
	public class AchievementWishes
	{
		[WishCommand("unlockanyachievement", null)]
		public static void DebugUnlockAnyAchievement()
		{
			foreach (var (_, achievementInfo2) in State.Achievements)
			{
				if (!achievementInfo2.Achieved)
				{
					MessageQueue.AddPlayerMessage("Unlocked Achievement: " + achievementInfo2.ID);
					achievementInfo2.Unlock();
					return;
				}
			}
			MessageQueue.AddPlayerMessage("All Achievements Already Unlocked");
		}
	}

	public static bool Write = false;

	public static bool Enabled = true;

	public static bool Active;

	public static AchievementState State = new AchievementState(100);

	public static void Awake()
	{
		if (Achievement.DIE != null)
		{
			State.AssignParents();
			Load().Wait();
			Active = true;
			PlatformManager.Synchronize();
		}
	}

	public static void Update()
	{
		if (Write)
		{
			Write = false;
			Task.Run((Action)Save);
		}
	}

	public static async Task Load()
	{
		string path = DataManager.SyncedPath("Achievements.json");
		JsonSerializerSettings settings = new JsonSerializerSettings
		{
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
		};
		if (!File.Exists(path))
		{
			return;
		}
		ObjectLoadResult<AchievementState> objectLoadResult = await Blob.ReadJsonAsync<AchievementState>(path, settings);
		if (!objectLoadResult.WasSuccessful())
		{
			MetricsManager.LogError("Error reading local achievement data", objectLoadResult.ToString());
			return;
		}
		AchievementState content = objectLoadResult.content;
		foreach (KeyValuePair<string, AchievementInfo> achievement in content.Achievements)
		{
			if (State.Achievements.TryGetValue(achievement.Key, out var value))
			{
				value.Achieved = achievement.Value.Achieved;
				value.TimeStamp = achievement.Value.TimeStamp;
			}
		}
		foreach (KeyValuePair<string, StatInfo> stat in content.Stats)
		{
			if (State.Stats.TryGetValue(stat.Key, out var value2))
			{
				value2.Value = stat.Value.Value;
			}
		}
	}

	public static void Save()
	{
		lock (State)
		{
			Blob.WriteAllJson(DataManager.SyncedPath("Achievements.json"), settings: new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				DefaultValueHandling = DefaultValueHandling.Ignore
			}, content: State).LogIfErrored();
		}
	}

	public static void Reset()
	{
		foreach (AchievementInfo value in State.Achievements.Values)
		{
			value.Achieved = false;
		}
		foreach (StatInfo value2 in State.Stats.Values)
		{
			value2.Value = 0;
		}
		PlatformManager.ResetAchievements();
	}

	public static bool GetAchievement(string ID)
	{
		if (State.Achievements.TryGetValue(ID, out var value))
		{
			return value.Achieved;
		}
		return false;
	}

	public static void SetAchievement(string ID)
	{
		if (Enabled)
		{
			if (!State.Achievements.TryGetValue(ID, out var value))
			{
				Debug.LogWarning("Unknown achievement ID: " + ID);
			}
			else
			{
				value.Unlock();
			}
		}
	}

	public static void IncrementAchievement(string ID, int Value = 1)
	{
		if (Enabled)
		{
			if (!State.Achievements.TryGetValue(ID, out var value))
			{
				Debug.LogWarning("Unknown stat ID: " + ID);
			}
			else if (value.Progress == null)
			{
				value.Unlock();
			}
			else
			{
				value.Progress.Increment(Value);
			}
		}
	}

	public static int GetStat(string ID)
	{
		if (!State.Stats.TryGetValue(ID, out var value))
		{
			Debug.LogWarning("Unknown stat ID: " + ID);
			return 0;
		}
		return value.Value;
	}

	public static void SetStat(string ID, int Value)
	{
		if (Enabled)
		{
			if (!State.Stats.TryGetValue(ID, out var value))
			{
				Debug.LogWarning("Unknown stat ID: " + ID);
			}
			else
			{
				value.SetValue(Value);
			}
		}
	}

	public static void IncrementStat(string ID)
	{
		if (Enabled)
		{
			if (!State.Stats.TryGetValue(ID, out var value))
			{
				Debug.LogWarning("Unknown stat ID: " + ID);
			}
			else
			{
				value.Increment();
			}
		}
	}
}
