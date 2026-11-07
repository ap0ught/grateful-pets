using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class SingleWeaponFighting_WeaponMastery : ISingleWeaponFightingSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetWeaponMeleeAttacksEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetWeaponMeleeAttacksEvent E)
	{
		if (AbilityEntry.ToggleState && E.Intrinsic)
		{
			E.AddAttack(100, 0, 0, Primary: true, Source: typeof(SingleWeaponFighting), Type: "SingleWeaponFighting");
		}
		return base.HandleEvent(E);
	}
}
