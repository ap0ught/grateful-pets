using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainLiquidSpitting_UnitSlimeGlands : ProceduralCookingEffectUnitMutation<SlimeGlands>
{
	public override void Init(GameObject target)
	{
		base.Init(target);
	}

	public CookingDomainLiquidSpitting_UnitSlimeGlands()
	{
		AddedTier = "1";
		BonusTier = "0";
	}
}
