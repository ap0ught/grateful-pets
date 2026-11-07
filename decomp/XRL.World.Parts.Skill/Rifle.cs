using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle : BaseSkill
{
	public override int GetWeaponCriticalModifier(GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return 2;
	}
}
