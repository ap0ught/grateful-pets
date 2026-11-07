using System;
using Newtonsoft.Json;
using Qud.UI;
using UnityEngine;
using XRL;

[JsonObject(MemberSerialization.OptIn)]
public class AchievementInfo
{
	public readonly string ID;

	public readonly string Name;

	public readonly string Description;

	public readonly string IconUnlocked;

	public readonly string IconLocked;

	public readonly int AchievedAt;

	public readonly bool Hidden;

	public readonly StatInfo Progress;

	[JsonProperty]
	public bool Achieved;

	[JsonProperty]
	public DateTime TimeStamp;

	public AchievementInfo()
	{
	}

	public AchievementInfo(string ID, string Name, string Icon, string Description, bool Hidden = false)
	{
		this.ID = ID;
		this.Name = Name;
		this.Description = Description;
		this.Hidden = Hidden;
		IconUnlocked = (Icon.Contains('/') ? Icon : ("UI/Achievements/" + Icon));
		IconLocked = IconUnlocked.Replace(".", "_u.");
		AchievementManager.State.Achievements.Add(ID, this);
	}

	public AchievementInfo(string ID, string Name, string Icon, string Description, string Progress, int Value, int MaxValue = int.MaxValue, bool Hidden = false)
		: this(ID, Name, Icon, Description, Hidden)
	{
		AchievedAt = Value;
		this.Progress = StatInfo.Create(Progress, MaxValue);
	}

	public AchievementInfo(string ID, string Name, string Icon, string Description, string Progress, string[] Consumed, bool Hidden = false)
		: this(ID, Name, Icon, Description, Hidden)
	{
		AchievedAt = Consumed.Length;
		this.Progress = StatInfo.Create(Progress, Consumed);
	}

	public void Unlock()
	{
		if (!Achieved && AchievementManager.Enabled)
		{
			Achieved = true;
			TimeStamp = DateTime.Now;
			AchievementManager.Write = true;
			if (!PlatformManager.TrySetAchievement(this))
			{
				NotifyUnlock();
			}
		}
	}

	public void Indicate()
	{
		if (!Achieved && AchievementManager.Enabled && !PlatformManager.TryShowAchievementProgress(this))
		{
			NotifyProgress();
		}
	}

	public void NotifyProgress()
	{
		string name = Name;
		string iconLocked = IconLocked;
		int? value = Progress?.Value;
		int? maxValue = AchievedAt;
		Color? titleColor = The.Color.DarkCyan;
		Color? frameColor = The.Color.Black;
		Notification.Enqueue(name, "Achievement progress.", iconLocked, null, value, maxValue, titleColor, null, frameColor);
	}

	public void NotifyUnlock()
	{
		string name = Name;
		string iconUnlocked = IconUnlocked;
		Color? titleColor = The.Color.Yellow;
		Color? frameColor = The.Color.Yellow;
		Notification.Enqueue(name, "Achievement unlocked!", iconUnlocked, "Sounds/UI/ui_achievement_unlock", null, null, titleColor, null, frameColor);
	}
}
