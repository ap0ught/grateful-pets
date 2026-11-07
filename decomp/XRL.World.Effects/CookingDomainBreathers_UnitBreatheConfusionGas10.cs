using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainBreathers_UnitBreatheConfusionGas10 : ProceduralCookingEffectUnitMutation<ConfusionBreather>
{
	public CookingDomainBreathers_UnitBreatheConfusionGas10()
	{
		AddedTier = "10";
		BonusTier = "10";
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
	}
}
