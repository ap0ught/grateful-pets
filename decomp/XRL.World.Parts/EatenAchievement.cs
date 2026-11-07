using System;

namespace XRL.World.Parts;

[Serializable]
public class EatenAchievement : IPart
{
	public string AchievementID = "";

	public bool Triggered;

	public EatenAchievement()
	{
	}

	public EatenAchievement(AchievementInfo Achievement)
	{
		AchievementID = Achievement.ID;
	}

	public EatenAchievement(string AchievementID)
	{
		this.AchievementID = AchievementID;
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Eaten");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Eaten" && !Triggered)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				AchievementManager.SetAchievement(AchievementID);
				Triggered = true;
			}
		}
		return base.FireEvent(E);
	}
}
