using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHeat_FlamingHandsCharge_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they emit a ray of flame per Flaming Hands at level 5-6.";
	}

	public override string GetNotification()
	{
		return "@they emit a powerful ray of flame.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer())
		{
			FlamingRay.Cast();
		}
	}
}
