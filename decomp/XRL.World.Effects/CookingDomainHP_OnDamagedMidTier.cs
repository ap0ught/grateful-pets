using XRL.Rules;

namespace XRL.World.Effects;

public class CookingDomainHP_OnDamagedMidTier : CookingDomainHP_OnDamaged
{
	public override void Init(GameObject target)
	{
		Tier = Stat.Random(12, 15);
		base.Init(target);
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature take@s damage, there's a 12-15% chance";
	}
}
