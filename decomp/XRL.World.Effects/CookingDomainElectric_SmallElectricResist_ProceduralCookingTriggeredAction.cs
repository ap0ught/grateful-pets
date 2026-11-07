using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainElectric_SmallElectricResist_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain 40-50 Electric Resist for 6 hours.";
	}

	public override string GetNotification()
	{
		return "@they become fortified against electrical damage.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainElectric_SmallElectricResist_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainElectric_SmallElectricResist_ProceduralCookingTriggeredActionEffect());
		}
	}
}
