using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainSelfHarm_UnitSelfPoison : ProceduralCookingEffectUnit
{
	public GameObject Object;

	public override string GetDescription()
	{
		return "Poisons @thisCreature.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.ApplyEffect(new Poisoned(Stat.Roll("1d4+4"), Stat.Roll("1d2+2") + "d2", 10));
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
