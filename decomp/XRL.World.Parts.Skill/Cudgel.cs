using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel : BaseSkill
{
	public override int Priority => int.MinValue;

	public override string GetWeaponCriticalDescription()
	{
		return "Cudgel (dazes on critical hit)";
	}

	public override void WeaponMadeCriticalHit(GameObject Attacker, GameObject Defender, GameObject Weapon, string Properties)
	{
		Defender.ApplyEffect(new Dazed(Stat.Random(1, 4), DontStunIfPlayer: true));
		base.WeaponMadeCriticalHit(Attacker, Defender, Weapon, Properties);
	}
}
