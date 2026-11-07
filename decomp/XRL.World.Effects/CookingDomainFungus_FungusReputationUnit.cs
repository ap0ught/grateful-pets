using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFungus_FungusReputationUnit : ProceduralCookingEffectUnit
{
	public int Tier;

	private bool Applied;

	public override string GetDescription()
	{
		return "+300 reputation with fungi";
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
		Applied = false;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		if (Object.IsPlayer())
		{
			XRLCore.Core.Game.PlayerReputation.Modify("Fungi", 300, "Cooking", null, null, Silent: true, Transient: true);
			Applied = true;
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		if (Applied)
		{
			XRLCore.Core.Game.PlayerReputation.Modify("Fungi", -300, "Cooking", null, null, Silent: true, Transient: true);
		}
	}
}
