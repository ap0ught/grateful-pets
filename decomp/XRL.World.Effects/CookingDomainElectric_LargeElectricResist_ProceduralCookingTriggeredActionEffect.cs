using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainElectric_LargeElectricResist_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingStatEffect
{
	public CookingDomainElectric_LargeElectricResist_ProceduralCookingTriggeredActionEffect()
		: base("ElectricResistance", "125-175", 50)
	{
	}
}
