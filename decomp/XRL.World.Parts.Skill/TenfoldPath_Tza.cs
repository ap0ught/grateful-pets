using System;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Tza : BaseInitiatorySkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetCooldownEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCooldownEvent E)
	{
		if (E.Ability.Class == "Mental Mutations")
		{
			E.PercentageReduction += 10;
		}
		return base.HandleEvent(E);
	}
}
