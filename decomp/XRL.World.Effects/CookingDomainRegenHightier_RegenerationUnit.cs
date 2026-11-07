using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainRegenHightier_RegenerationUnit : CookingDomainRegenLowtier_RegenerationUnit
{
	public override void Init(GameObject target)
	{
		Tier = Stat.Random(100, 100);
	}

	public override string GetTemplatedDescription()
	{
		return "+100% to natural healing rate";
	}
}
