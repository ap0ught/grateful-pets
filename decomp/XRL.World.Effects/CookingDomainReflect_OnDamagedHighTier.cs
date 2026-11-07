using XRL.Rules;

namespace XRL.World.Effects;

public class CookingDomainReflect_OnDamagedHighTier : CookingDomainReflect_OnDamaged
{
	public override void Init(GameObject target)
	{
		base.Init(target);
		Tier = Stat.Random(16, 20);
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature take@s damage, there's a 16-20% chance";
	}
}
