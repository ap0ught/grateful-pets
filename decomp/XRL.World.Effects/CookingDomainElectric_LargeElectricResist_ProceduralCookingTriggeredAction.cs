using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainElectric_LargeElectricResist_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain 125-175 Electric Resist for 50 turns.";
	}

	public override string GetNotification()
	{
		return "@they become impervious to electrical damage.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainElectric_LargeElectricResist_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainElectric_LargeElectricResist_ProceduralCookingTriggeredActionEffect());
		}
	}
}
