using System;

namespace XRL.World.Parts;

[Serializable]
public class AchievementOnConsume : IPart
{
	public string AchievementID;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == AfterConsumeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterConsumeEvent E)
	{
		if (!AchievementID.IsNullOrEmpty() && ParentObject == E.Object)
		{
			AchievementManager.IncrementAchievement(AchievementID);
		}
		return base.HandleEvent(E);
	}
}
