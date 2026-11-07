using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainBreathers_UnitBreatheCorrosiveGas5 : ProceduralCookingEffectUnitMutation<CorrosiveBreather>
{
	public CookingDomainBreathers_UnitBreatheCorrosiveGas5()
	{
		AddedTier = "5";
		BonusTier = "5";
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
	}
}
