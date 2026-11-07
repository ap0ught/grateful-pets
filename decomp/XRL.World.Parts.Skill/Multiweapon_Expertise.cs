using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Multiweapon_Expertise : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetMeleeAttackChanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (E.Intrinsic && !E.Primary)
		{
			E.Chance += 15;
		}
		return base.HandleEvent(E);
	}
}
