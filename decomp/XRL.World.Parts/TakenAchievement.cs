using System;

namespace XRL.World.Parts;

[Serializable]
public class TakenAchievement : IPart
{
	public string AchievementID = "";

	public bool Triggered;

	public override bool SameAs(IPart p)
	{
		TakenAchievement takenAchievement = p as TakenAchievement;
		if (takenAchievement.AchievementID != AchievementID)
		{
			return false;
		}
		if (takenAchievement.Triggered != Triggered)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		Trigger(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		Trigger(E.Actor);
		return base.HandleEvent(E);
	}

	public void Trigger(GameObject who)
	{
		if (!Triggered && who != null && who.IsPlayer())
		{
			AchievementManager.SetAchievement(AchievementID);
			Triggered = true;
		}
	}
}
