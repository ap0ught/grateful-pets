using System;

namespace XRL.World.Parts;

[Serializable]
public class HydraulicPowerTransmission : IPowerTransmission
{
	public HydraulicPowerTransmission()
	{
		IsEMPSensitive = false;
		ChargeRate = 2000;
		ChanceBreakConnectedOnDestroy = 100;
		Substance = "power";
		Activity = "carrying";
		Constituent = "plumbing";
		Assembly = "hydraulic transmission system";
		Unit = "joule";
		UnitFactor = 0.1;
		DependsOnLiquid = "water";
		WrongLiquidFactor = 0.2;
	}

	public override string GetPowerTransmissionType()
	{
		return "hydraulic";
	}
}
