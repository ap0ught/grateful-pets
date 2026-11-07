using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class SingleWeaponFighting_PenetratingStrikes : ISingleWeaponFightingSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetAttackerMeleePenetrationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetAttackerMeleePenetrationEvent E)
	{
		if (AbilityEntry.ToggleState && (E.Defender == null || E.Defender.IsCombatObject() || !E.Defender.IsWall()))
		{
			E.Penetrations++;
		}
		return base.HandleEvent(E);
	}
}
