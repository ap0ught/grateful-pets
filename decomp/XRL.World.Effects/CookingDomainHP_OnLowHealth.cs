namespace XRL.World.Effects;

public class CookingDomainHP_OnLowHealth : ProceduralCookingEffectWithTrigger
{
	public bool currentlyBelow;

	protected float LowAmount = 0.2f;

	public override void Init(GameObject target)
	{
		base.Init(target);
		currentlyBelow = false;
	}

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature drop@s below 20% HP,";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<StatChangeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (E.Name == "Hitpoints")
		{
			if (currentlyBelow)
			{
				if (!IsLow())
				{
					currentlyBelow = false;
				}
			}
			else if (IsLow())
			{
				currentlyBelow = true;
				Trigger();
			}
		}
		return base.HandleEvent(E);
	}

	public bool IsLow()
	{
		if (Object == null)
		{
			return false;
		}
		if (!Object.HasStat("Hitpoints"))
		{
			return false;
		}
		return Object.isDamaged(LowAmount, inclusive: true);
	}
}
