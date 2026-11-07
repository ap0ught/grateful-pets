using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class LongBlades : BaseSkill
{
	public override string GetWeaponCriticalDescription()
	{
		return "Long Blades (increased penetration on critical hit)";
	}

	public override int GetWeaponCriticalModifier(GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return 2;
	}
}
