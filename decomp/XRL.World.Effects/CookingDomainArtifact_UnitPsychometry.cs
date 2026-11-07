using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainArtifact_UnitPsychometry : ProceduralCookingEffectUnitMutation<Psychometry>
{
	public CookingDomainArtifact_UnitPsychometry()
	{
		AddedTier = "1-2";
		BonusTier = "3-4";
	}
}
