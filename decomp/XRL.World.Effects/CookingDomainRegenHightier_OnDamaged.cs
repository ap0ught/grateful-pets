using XRL.Rules;

namespace XRL.World.Effects;

public class CookingDomainRegenHightier_OnDamaged : CookingDomainRegenLowtier_OnDamaged
{
	public override void Init(GameObject target)
	{
		Tier = Stat.Random(30, 40);
		base.Init(target);
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature take@s damage, there's a 30-40% chance";
	}
}
