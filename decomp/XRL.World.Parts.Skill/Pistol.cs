using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Pistol : BaseSkill
{
	public override int GetWeaponCriticalModifier(GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		int num = 2;
		if (Attacker != null && Attacker.HasSkill("Pistol_DeadShot"))
		{
			num += 2;
		}
		return num;
	}
}
