namespace XRL.World.Effects;

public class CookingDomainHP_OnLowHealthMidTier : CookingDomainHP_OnLowHealth
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature drop@s below 30% HP,";
	}

	public override void Init(GameObject target)
	{
		LowAmount = 0.3f;
		base.Init(target);
	}
}
