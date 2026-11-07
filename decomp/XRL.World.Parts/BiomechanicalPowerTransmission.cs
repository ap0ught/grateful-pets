using System;

namespace XRL.World.Parts;

[Serializable]
public class BiomechanicalPowerTransmission : IPowerTransmission
{
	public BiomechanicalPowerTransmission()
	{
		IsEMPSensitive = false;
		ChargeRate = 200;
		ChanceBreakConnectedOnDestroy = 30;
		ChanceBreakConnectedOnMove = 0;
		ChanceBreakOnMove = 0;
		Substance = "power";
		Activity = "carrying";
		Constituent = "biomachinery";
		Assembly = "biomechanical transmission system";
		Unit = "joule";
		UnitFactor = 0.1;
	}

	public override string GetPowerTransmissionType()
	{
		return "biomechanical";
	}
}
