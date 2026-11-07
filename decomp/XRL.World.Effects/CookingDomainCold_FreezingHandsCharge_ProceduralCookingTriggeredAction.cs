using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainCold_FreezingHandsCharge_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they emit a ray of frost per Freezing Hands at level 5-6.";
	}

	public override string GetNotification()
	{
		return "@they emit a powerful ray of frost.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer())
		{
			FreezingRay.Cast();
		}
	}
}
