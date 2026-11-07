using System;

namespace XRL.World.Parts;

[Serializable]
public class Fur : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID)
		{
			return ID == PooledEvent<GetSpringinessEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageIncrease += 100;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSpringinessEvent E)
	{
		E.LinearIncrease += ParentObject.GetKineticResistance() * 2 / 3;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject)
		{
			if (E.Damage.IsColdDamage())
			{
				E.Damage.Amount /= 4;
			}
			if (E.Damage.IsHeatDamage())
			{
				E.Damage.Amount = E.Damage.Amount * 3 / 2;
			}
		}
		return base.HandleEvent(E);
	}
}
