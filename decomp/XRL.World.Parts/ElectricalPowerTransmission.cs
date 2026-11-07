using System;

namespace XRL.World.Parts;

[Serializable]
public class ElectricalPowerTransmission : IPowerTransmission
{
	public ElectricalPowerTransmission()
	{
		ChargeRate = 500;
		ChanceBreakConnectedOnDestroy = 100;
		Substance = "charge";
		Activity = "conducting";
		Constituent = "wiring";
		Assembly = "power grid";
		Unit = "amp";
		UnitFactor = 0.1;
		SparkWhenBrokenAndPowered = true;
	}

	public override string GetPowerTransmissionType()
	{
		return "electrical";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetElectricalConductivityEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 1 && E.Object == ParentObject)
		{
			E.MinValue(95);
		}
		return base.HandleEvent(E);
	}
}
