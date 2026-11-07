using System;

namespace XRL.World.Parts;

[Serializable]
public class FoliageCamouflage : ICamouflage
{
	public FoliageCamouflage()
	{
		EffectClass = "FoliageCamouflaged";
		Description = "Foliage camouflage: This item grants the wearer +=level= DV in foliage.";
	}
}
