using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainBurrowing_UnitBurrowingClaws : ProceduralCookingEffectUnitMutation<BurrowingClaws>
{
	public override void Init(GameObject target)
	{
		base.Init(target);
	}

	public CookingDomainBurrowing_UnitBurrowingClaws()
	{
		AddedTier = "5-6";
		BonusTier = "3-4";
	}
}
