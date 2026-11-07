using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHightierRegen_Heal40to50_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they heal 40-50 HP.";
	}

	public override string GetNotification()
	{
		return "@their wounds heal significantly.";
	}

	public override void Apply(GameObject go)
	{
		go.Statistics["Hitpoints"].Penalty -= Stat.Roll("40-50");
	}
}
