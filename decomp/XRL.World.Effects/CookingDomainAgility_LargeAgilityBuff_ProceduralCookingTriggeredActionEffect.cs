using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainAgility_LargeAgilityBuff_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingStatEffect
{
	public CookingDomainAgility_LargeAgilityBuff_ProceduralCookingTriggeredActionEffect()
		: base("Agility", "8", 50)
	{
	}
}
