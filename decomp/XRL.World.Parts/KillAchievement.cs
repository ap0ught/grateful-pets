using System;

namespace XRL.World.Parts;

[Serializable]
public class KillAchievement : IPart
{
	public string AchievementID = "";

	public string Category = "";

	public int TargetAmount;

	public int IndicateIncrement = 10;

	public bool Triggered;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDie");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDie" && !Triggered)
		{
			try
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Killer");
				if (gameObjectParameter != null && gameObjectParameter.IsPlayerControlled())
				{
					Triggered = true;
					AchievementManager.IncrementAchievement(AchievementID);
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Kill achievement", x);
			}
		}
		return base.FireEvent(E);
	}
}
