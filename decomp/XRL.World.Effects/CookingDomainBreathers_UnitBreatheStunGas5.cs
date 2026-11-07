using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainBreathers_UnitBreatheStunGas5 : ProceduralCookingEffectUnitMutation<StunBreather>
{
	public CookingDomainBreathers_UnitBreatheStunGas5()
	{
		AddedTier = "5";
		BonusTier = "5";
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
	}
}
