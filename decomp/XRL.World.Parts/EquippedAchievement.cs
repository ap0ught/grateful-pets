using System;

namespace XRL.World.Parts;

[Serializable]
public class EquippedAchievement : IPart
{
	public string AchievementID = "";

	public bool Triggered;

	public EquippedAchievement()
	{
	}

	public EquippedAchievement(AchievementInfo Achievement)
		: this()
	{
		AchievementID = Achievement.ID;
	}

	public EquippedAchievement(string ID)
		: this()
	{
		AchievementID = ID;
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EquippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (!Triggered && !AchievementID.IsNullOrEmpty() && E.Actor.IsPlayer() && ParentObject.IsEquippedProperly())
		{
			AchievementManager.SetAchievement(AchievementID);
			Triggered = true;
		}
		return base.HandleEvent(E);
	}
}
