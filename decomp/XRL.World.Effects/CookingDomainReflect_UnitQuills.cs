using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainReflect_UnitQuills : ProceduralCookingEffectUnitMutation<Quills>
{
	public override void Init(GameObject target)
	{
		base.Init(target);
	}

	public CookingDomainReflect_UnitQuills()
	{
		AddedTier = "5-6";
		BonusTier = "3-4";
	}
}
