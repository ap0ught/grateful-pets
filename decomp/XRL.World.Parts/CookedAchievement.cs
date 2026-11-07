using System;

namespace XRL.World.Parts;

[Serializable]
public class CookedAchievement : IPart
{
	public string AchievementID = "";

	public bool Triggered;

	public CookedAchievement()
	{
	}

	public CookedAchievement(AchievementInfo Achievement)
	{
		AchievementID = Achievement.ID;
	}

	public CookedAchievement(string AchievementID)
	{
		this.AchievementID = AchievementID;
	}

	public override bool SameAs(IPart p)
	{
		if (p is CookedAchievement cookedAchievement)
		{
			return AchievementID == cookedAchievement.AchievementID;
		}
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("UsedAsIngredient");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "UsedAsIngredient" && !Triggered)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				AchievementManager.SetAchievement(AchievementID);
				Triggered = true;
				ParentObject.RemovePart(this);
			}
		}
		return base.FireEvent(E);
	}
}
