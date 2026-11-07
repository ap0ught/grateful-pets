using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_ShatteringBlows : BaseSkill
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DealDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DealDamage")
		{
			MeleeWeapon meleeWeapon = E.GetGameObjectParameter("Weapon")?.GetPart<MeleeWeapon>();
			if (meleeWeapon != null && meleeWeapon.Skill == "Cudgel" && 10.in100())
			{
				E.GetGameObjectParameter("Defender").ApplyEffect(new ShatterArmor(2000));
			}
		}
		return base.FireEvent(E);
	}
}
