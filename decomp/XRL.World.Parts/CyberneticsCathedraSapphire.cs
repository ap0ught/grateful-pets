using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCathedraSapphire : CyberneticsCathedra
{
	public override void OnImplanted(GameObject Object)
	{
		base.OnImplanted(Object);
		ActivatedAbilityID = Object.AddActivatedAbility("Stunning Force", "CommandActivateCathedra", "Cybernetics", null, "#", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, "CommandCathedraSapphire");
	}

	public override void CollectStats(Templates.StatCollector stats)
	{
		base.CollectStats(stats);
		StunningForce.CollectProxyStats(stats, GetLevel());
	}

	public override void Activate(GameObject Actor)
	{
		StunningForce.Concussion(Actor.CurrentCell, Actor, GetLevel(Actor), 3, Actor.GetPhase());
		base.Activate(Actor);
	}
}
