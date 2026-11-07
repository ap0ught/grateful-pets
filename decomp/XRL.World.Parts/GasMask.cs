using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class GasMask : IPart
{
	public int Power = 10;

	public override bool SameAs(IPart p)
	{
		if ((p as GasMask).Power != Power)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != PooledEvent<GetRespiratoryAgentPerformanceEvent>.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetRespiratoryAgentPerformanceEvent E)
	{
		if (!E.WillAllowSave && E.Object == ParentObject.Equipped)
		{
			E.LinearAdjustment -= Power * 5;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Inhaled Gas", E.Vs))
		{
			E.Roll += Power;
			E.IgnoreNatural1 = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Damage != null && E.Damage.HasAttribute("Gas") && E.Object == ParentObject.Equipped)
		{
			E.Damage.Amount = E.Damage.Amount * (100 - Power) / 100;
		}
		return base.HandleEvent(E);
	}
}
