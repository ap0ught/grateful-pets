using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPlant_UnitPlantReputationHighTier : ProceduralCookingEffectUnit
{
	public int Tier;

	private bool Applied;

	public override void Init(GameObject target)
	{
		base.Init(target);
		Applied = false;
	}

	public override string GetDescription()
	{
		return "+200 reputation with flowers, roots, succulents, trees, vines, and the Consortium of Phyta";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		if (Object.IsPlayer())
		{
			The.Game.PlayerReputation.Modify("Flowers", 200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Roots", 200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Succulents", 200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Trees", 200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Vines", 200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Consortium", 200, "Cooking", null, null, Silent: true, Transient: true);
			Applied = true;
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		if (Applied)
		{
			The.Game.PlayerReputation.Modify("Flowers", -200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Roots", -200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Succulents", -200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Trees", -200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Vines", -200, "Cooking", null, null, Silent: true, Transient: true);
			The.Game.PlayerReputation.Modify("Consortium", -200, "Cooking", null, null, Silent: true, Transient: true);
		}
	}
}
