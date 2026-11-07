using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Wintellect.PowerCollections;

[JsonObject(MemberSerialization.OptIn)]
public class AchievementState
{
	[JsonProperty("Achievements")]
	public Dictionary<string, AchievementInfo> Achievements;

	[JsonProperty("Stats")]
	public Dictionary<string, StatInfo> Stats;

	public AchievementState()
	{
		Achievements = new Dictionary<string, AchievementInfo>();
		Stats = new Dictionary<string, StatInfo>();
	}

	public AchievementState(int Capacity)
	{
		Achievements = new Dictionary<string, AchievementInfo>(Capacity);
		Stats = new Dictionary<string, StatInfo>(Capacity);
	}

	public void AssignParents()
	{
		List<AchievementInfo> list = new List<AchievementInfo>(4);
		Comparison<AchievementInfo> comparison = (AchievementInfo a, AchievementInfo b) =>
		{
			int achievedAt = a.AchievedAt;
			return achievedAt.CompareTo(b.AchievedAt);
		};
		foreach (KeyValuePair<string, StatInfo> stat in Stats)
		{
			stat.Deconstruct(out var key, out var value);
			StatInfo statInfo = value;
			foreach (KeyValuePair<string, AchievementInfo> achievement in Achievements)
			{
				achievement.Deconstruct(out key, out var value2);
				AchievementInfo achievementInfo = value2;
				if (achievementInfo.Progress == statInfo)
				{
					list.Add(achievementInfo);
				}
			}
			statInfo.Achievements = list.ToArray();
			Algorithms.StableSortInPlace(statInfo.Achievements, comparison);
			list.Clear();
		}
	}
}
