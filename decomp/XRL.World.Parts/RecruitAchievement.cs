using System;

namespace XRL.World.Parts;

[Serializable]
public class RecruitAchievement : IPart
{
	public bool Triggered;

	public string AchievementID;

	public override bool SameAs(IPart p)
	{
		RecruitAchievement recruitAchievement = p as RecruitAchievement;
		if (recruitAchievement.Triggered != Triggered)
		{
			return false;
		}
		if (recruitAchievement.AchievementID != AchievementID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == PooledEvent<AfterChangePartyLeaderEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterChangePartyLeaderEvent E)
	{
		if (!Triggered && !E.Transient && E.NewLeader != null && E.NewLeader.IsPlayerControlled() && !AchievementID.IsNullOrEmpty())
		{
			AchievementManager.SetAchievement(AchievementID);
			Triggered = true;
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}
}
