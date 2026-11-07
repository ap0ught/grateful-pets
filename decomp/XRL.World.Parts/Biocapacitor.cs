using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Biocapacitor : Capacitor
{
	public Biocapacitor()
	{
		ChargeRate = 3;
		MaxCharge = 5000;
		MinimumChargeToExplode = 2500;
		Description = "biocapacitor";
		ChargeDisplayStyle = "bio";
		AltChargeDisplayStyle = "percentage";
		AltChargeDisplayProperty = Scanning.GetScanPropertyName(Scanning.Scan.Bio);
		ChargeLossDisable = false;
		IsBioScannable = true;
	}

	public override bool InteractWithChargeEvent(IChargeEvent E)
	{
		if (!E.IncludeBiological)
		{
			return false;
		}
		return base.InteractWithChargeEvent(E);
	}
}
