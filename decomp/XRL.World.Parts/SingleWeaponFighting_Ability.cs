using System;

namespace XRL.World.Parts;

[Serializable]
public class SingleWeaponFighting_Ability : ISupportAbilitySkillPart
{
	public override string AbilityCommand => "CommandSingleWeaponFighting";

	public override ActivatedAbilityEntry RequireAbility()
	{
		if (AbilityEntry != null)
		{
			return AbilityEntry;
		}
		Guid iD = ParentObject.AddActivatedAbility("Single Weapon Fighting", AbilityCommand, "Skills", null, "ô", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		return ParentObject.GetActivatedAbility(iD);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(PooledEvent<GetMeleeAttackChanceEvent>.ID, EventOrder.VERY_EARLY);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == AbilityCommand)
		{
			ParentObject.ToggleActivatedAbility(AbilityEntry.ID);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (AbilityEntry.ToggleState && E.Intrinsic && !E.Primary)
		{
			E.Multiplier = 0.0;
			if (E.Inherited)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}
}
