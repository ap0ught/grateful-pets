using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainBreathers_UnitBreatheCorrosiveGas10 : ProceduralCookingEffectUnitMutation<CorrosiveBreather>
{
	public CookingDomainBreathers_UnitBreatheCorrosiveGas10()
	{
		AddedTier = "10";
		BonusTier = "10";
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
	}
}
