using System;

namespace XRL.World.Parts;

[Serializable]
public class IPoweredPart : IActivePart
{
	public IPoweredPart()
	{
		ChargeUse = 1;
		IsBootSensitive = true;
		IsEMPSensitive = true;
		IsPowerSwitchSensitive = true;
		IsTechScannable = true;
	}
}
