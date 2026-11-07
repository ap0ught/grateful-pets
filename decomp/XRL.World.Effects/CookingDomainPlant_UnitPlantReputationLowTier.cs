using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPlant_UnitPlantReputationLowTier : ProceduralCookingEffectUnit
{
	public int Tier;

	private bool Applied;

	public override string GetDescription()
	{
		return "+100 reputation with flowers, roots, succulents, trees, vines, and the Consortium of Phyta";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		if (Object.IsPlayer())
		{
			The.Game.PlayerReputation.Modify("Flowers", 100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Roots", 100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Succulents", 100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Trees", 100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Vines", 100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Consortium", 100, "Cooking", null, null, Silent: true, Transient: true);
			Applied = true;
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		if (Applied)
		{
			The.Game.PlayerReputation.Modify("Flowers", -100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Roots", -100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Succulents", -100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Trees", -100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Vines", -100, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Consortium", -100, "Cooking", null, null, Silent: true, Transient: true);
		}
	}
}
