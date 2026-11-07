using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Expertise : BaseSkill
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
		if (E.Actor == ParentObject && E.Checking == "Actor" && E.Skill == "Cudgel" && E.Melee)
		{
			E.Modifier += HitBonus;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DealDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DealDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			if (gameObjectParameter != null)
			{
				MeleeWeapon part = gameObjectParameter.GetPart<MeleeWeapon>();
				if (part != null && part.Skill == "Cudgel" && ParentObject.HasSkill("Cudgel_Bonecrusher"))
				{
					(E.GetParameter("Damage") as Damage).Amount *= 2;
				}
			}
		}
		return base.FireEvent(E);
	}
}
