using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainEgo_UnitEgoProjection : ProceduralCookingEffectUnitMutation<WillForce>
{
	public override void Init(GameObject target)
	{
		base.Init(target);
	}

	public CookingDomainEgo_UnitEgoProjection()
	{
		AddedTier = "4-5";
		BonusTier = "4-5";
	}
}
