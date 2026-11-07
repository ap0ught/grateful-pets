using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades : BaseSkill
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetWeaponCriticalDescription()
	{
		return "Short Blades (causes bleeding on critical hit)";
	}

	public override void WeaponMadeCriticalHit(GameObject Attacker, GameObject Defender, GameObject Weapon, string Properties)
	{
		Defender.ApplyEffect(new Bleeding("1d2-1", 20 + Attacker.StatMod("Agility"), Attacker, Stack: false));
		base.WeaponMadeCriticalHit(Attacker, Defender, Weapon, Properties);
	}
}
