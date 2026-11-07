using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainElectric_EMPUnit : ProceduralCookingEffectUnitMutation<ElectromagneticPulse>
{
	public override void Init(GameObject target)
	{
		base.Init(target);
	}

	public CookingDomainElectric_EMPUnit()
	{
		AddedTier = "2-3";
		BonusTier = "3-4";
	}
}
