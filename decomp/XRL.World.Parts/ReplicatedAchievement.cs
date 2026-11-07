using System;

namespace XRL.World.Parts;

[Serializable]
public class ReplicatedAchievement : IPart
{
	public bool Triggered;

	public string AchievementID;

	public override bool SameAs(IPart p)
	{
		ReplicatedAchievement replicatedAchievement = p as ReplicatedAchievement;
		if (replicatedAchievement.Triggered != Triggered)
		{
			return false;
		}
		if (replicatedAchievement.AchievementID != AchievementID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == WasReplicatedEvent.ID)
			{
				return !Triggered;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(WasReplicatedEvent E)
	{
		if (!Triggered && E.Actor != null && E.Actor.IsPlayer())
		{
			AchievementManager.SetAchievement(AchievementID);
			Triggered = true;
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}
}
