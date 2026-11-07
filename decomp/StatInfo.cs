using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class StatInfo
{
	public readonly string ID;

	[JsonProperty]
	public int Value;

	public int MaxValue;

	public StatInfo Parent;

	public AchievementInfo[] Achievements = Array.Empty<AchievementInfo>();

	public StatInfo[] Children = Array.Empty<StatInfo>();

	public StatInfo()
	{
	}

	private StatInfo(string ID, int MaxValue = 0)
	{
		this.ID = ID;
		this.MaxValue = MaxValue;
	}

	public static StatInfo Create(string ID, int MaxValue = 1)
	{
		Dictionary<string, StatInfo> stats = AchievementManager.State.Stats;
		if (!stats.TryGetValue(ID, out var value))
		{
			value = (stats[ID] = new StatInfo(ID, MaxValue));
		}
		else
		{
			value.MaxValue = Math.Max(MaxValue, value.MaxValue);
		}
		return value;
	}

	public static StatInfo Create(string ID, string[] Consumed)
	{
		Dictionary<string, StatInfo> stats = AchievementManager.State.Stats;
		if (stats.ContainsKey(ID))
		{
			throw new ArgumentException("Attempting to supply new children to existing StatInfo: " + ID);
		}
		int num = Consumed.Length;
		StatInfo statInfo = new StatInfo(ID, num);
		statInfo.Children = new StatInfo[num];
		for (int i = 0; i < num; i++)
		{
			statInfo.Children[i] = Create(Consumed[i]);
			statInfo.Children[i].Parent = statInfo;
		}
		stats[ID] = statInfo;
		return statInfo;
	}

	public void Increment(int Value = 1)
	{
		SetValue(this.Value + Value);
	}

	public void SetValue(int Value)
	{
		if (!AchievementManager.Enabled)
		{
			return;
		}
		Value = Math.Min(Value, MaxValue);
		if (Value == this.Value)
		{
			return;
		}
		this.Value = Value;
		AchievementManager.Write = true;
		PlatformManager.SetStat(this);
		if (Parent != null)
		{
			Parent.Consume();
		}
		if (Achievements.Length == 0)
		{
			return;
		}
		bool flag = false;
		int i = 0;
		for (int num = Achievements.Length; i < num; i++)
		{
			AchievementInfo achievementInfo = Achievements[i];
			if (!achievementInfo.Achieved && achievementInfo.AchievedAt <= Value)
			{
				achievementInfo.Unlock();
				flag = true;
			}
		}
		if (!flag && (MaxValue < 50 || Value % (MaxValue / 10) == 0))
		{
			Indicate();
		}
	}

	public void Consume()
	{
		if (!AchievementManager.Enabled)
		{
			return;
		}
		int num = Children.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			StatInfo statInfo = Children[i];
			if (statInfo.Value >= statInfo.MaxValue)
			{
				num2++;
			}
		}
		SetValue(num2);
	}

	public void Indicate()
	{
		if (AchievementManager.Enabled)
		{
			Array.Find(Achievements, (AchievementInfo x) => !x.Achieved)?.Indicate();
		}
	}
}
