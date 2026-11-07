using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public abstract class ISingleWeaponFightingSkill : ISupportAbilitySkill
{
	public const string ABL_CMD = "CommandSingleWeaponFighting";

	public override string AbilityCommand => "CommandSingleWeaponFighting";

	public override ActivatedAbilityEntry RequireAbility()
	{
		return ParentObject.RequirePart<SingleWeaponFighting_Ability>().AbilityEntry;
	}
}
