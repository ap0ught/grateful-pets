using System;

namespace XRL.World.Parts;

[Serializable]
public class InhabitAchievement : IPart
{
	public bool Triggered;

	public string AchievementID;

	public override bool SameAs(IPart p)
	{
		InhabitAchievement inhabitAchievement = p as InhabitAchievement;
		if (inhabitAchievement.Triggered != Triggered)
		{
			return false;
		}
		if (inhabitAchievement.AchievementID != AchievementID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && !Triggered && ParentObject != null && ParentObject.IsPlayer() && !string.IsNullOrEmpty(AchievementID))
		{
			AchievementManager.SetAchievement(AchievementID);
			Triggered = true;
		}
		return base.FireEvent(E);
	}
}
