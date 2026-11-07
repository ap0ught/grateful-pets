using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainBreathers_UnitBreatheSleepGas10 : ProceduralCookingEffectUnitMutation<SleepBreather>
{
	public CookingDomainBreathers_UnitBreatheSleepGas10()
	{
		AddedTier = "10";
		BonusTier = "10";
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
	}
}
