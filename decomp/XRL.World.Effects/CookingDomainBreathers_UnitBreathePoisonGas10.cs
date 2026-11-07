using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainBreathers_UnitBreathePoisonGas10 : ProceduralCookingEffectUnitMutation<PoisonBreather>
{
	public CookingDomainBreathers_UnitBreathePoisonGas10()
	{
		AddedTier = "10";
		BonusTier = "10";
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
	}
}
