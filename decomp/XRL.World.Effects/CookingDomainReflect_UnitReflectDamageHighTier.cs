using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainReflect_UnitReflectDamageHighTier : CookingDomainReflect_UnitReflectDamage
{
	public override void Init(GameObject target)
	{
		Tier = Stat.Random(15, 18);
	}

	public override string GetTemplatedDescription()
	{
		return "Reflect 15-18% damage back at @their attackers, rounded up.";
	}
}
