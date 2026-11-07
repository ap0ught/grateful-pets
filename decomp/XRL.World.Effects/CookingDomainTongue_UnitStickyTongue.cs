using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainTongue_UnitStickyTongue : ProceduralCookingEffectUnitMutation<StickyTongue>
{
	public override void Init(GameObject target)
	{
		base.Init(target);
	}

	public CookingDomainTongue_UnitStickyTongue()
	{
		AddedTier = "4-5";
		BonusTier = "4-5";
	}
}
