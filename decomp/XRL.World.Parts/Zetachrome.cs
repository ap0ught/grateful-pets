using System;

namespace XRL.World.Parts;

[Serializable]
public class Zetachrome : IPart
{
	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		ParentObject.MakeNonflammable();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != PooledEvent<GetElectricalConductivityEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == PooledEvent<TransparentToEMPEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 2 && E.Object == ParentObject)
		{
			E.MinValue(60);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction += 30;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && !ParentObject.HasTag("Creature"))
		{
			if (E.Damage.IsHeatDamage() || E.Damage.IsColdDamage())
			{
				NotifyTargetImmuneEvent.Send(E.Weapon, E.Object, E.Actor, E.Damage, this);
				E.Damage.Amount = 0;
			}
			else if (E.Damage.IsAcidDamage())
			{
				E.Damage.Amount /= 4;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("jewels", 1);
			E.Add("stars", 1);
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TransparentToEMPEvent E)
	{
		return false;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.MakeImperviousToHeat();
		ParentObject.SetStringProperty("EquipmentFrameColors", "mCmC");
		return base.HandleEvent(E);
	}
}
