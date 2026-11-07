using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainBreathers_UnitBreatheFire10 : ProceduralCookingEffectUnitMutation<FireBreather>
{
	public CookingDomainBreathers_UnitBreatheFire10()
	{
		AddedTier = "10";
		BonusTier = "10";
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
	}
}
