using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainLowtierRegen_HealToFull_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they heal 15-20 HP.";
	}

	public override string GetNotification()
	{
		return "@their wounds heal a bit.";
	}

	public override void Apply(GameObject go)
	{
		go.Statistics["Hitpoints"].Penalty -= Stat.Roll("15-20");
	}
}
