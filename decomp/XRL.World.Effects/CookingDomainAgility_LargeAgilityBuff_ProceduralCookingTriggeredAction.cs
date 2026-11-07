using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainAgility_LargeAgilityBuff_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain@s +8 Agility for 50 turns.";
	}

	public override string GetNotification()
	{
		return "@they assume a limber pose.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainAgility_LargeAgilityBuff_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainAgility_LargeAgilityBuff_ProceduralCookingTriggeredActionEffect());
		}
	}
}
