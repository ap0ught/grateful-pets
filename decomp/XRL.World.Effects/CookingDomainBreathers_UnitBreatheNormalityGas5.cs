using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainBreathers_UnitBreatheNormalityGas5 : ProceduralCookingEffectUnitMutation<NormalityBreather>
{
	public CookingDomainBreathers_UnitBreatheNormalityGas5()
	{
		AddedTier = "5";
		BonusTier = "5";
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
	}
}
