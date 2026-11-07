using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Expertise : BaseSkill
{
	public int HitBonus = 2;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetToHitModifierEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Actor == ParentObject && E.Checking == "Actor" && E.Skill == "Axe" && E.Melee)
		{
			E.Modifier += HitBonus;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
