using System;

namespace XRL.World.Parts;

[Serializable]
public class MechanicalPowerTransmission : IPowerTransmission
{
	public MechanicalPowerTransmission()
	{
		IsEMPSensitive = false;
		ChargeRate = 100;
		ChanceBreakConnectedOnDestroy = 100;
		Substance = "power";
		Activity = "carrying";
		Constituent = "machinery";
		Assembly = "mechanical transmission system";
		Unit = "joule";
		UnitFactor = 0.1;
	}

	public override string GetPowerTransmissionType()
	{
		return "mechanical";
	}
}
