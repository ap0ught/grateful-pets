using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHP_UnitHP : ProceduralCookingEffectUnit
{
	public int Tier;

	public int Bonus;

	public override string GetDescription()
	{
		return Tier.Signed() + "% max HP";
	}

	public override string GetTemplatedDescription()
	{
		return "+10-15% max HP";
	}

	public override void Init(GameObject target)
	{
		Bonus = 0;
		Tier = Stat.Random(10, 15);
		base.Init(target);
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Bonus = (int)Math.Ceiling((float)Tier * 0.01f * (float)Object.Statistics["Hitpoints"].BaseValue);
		Object.Statistics["Hitpoints"].BaseValue += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["Hitpoints"].BaseValue -= Bonus;
		Bonus = 0;
	}
}
