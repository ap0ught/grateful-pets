using System;

namespace XRL.World.Parts;

[Serializable]
public class PoweredMissilePerformance : MissilePerformance
{
	public PoweredMissilePerformance()
	{
		ChargeUse = 100;
		IsBootSensitive = true;
		IsEMPSensitive = true;
		IsTechScannable = true;
		NameForStatus = "PerformanceEnhancementSystems";
	}
}
