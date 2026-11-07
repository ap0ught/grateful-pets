using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainCold_UnitResist : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " Cold Resistance";
	}

	public override string GetTemplatedDescription()
	{
		return "+10-15 Cold Resistance";
	}

	public override void Init(GameObject target)
	{
		Bonus = Stat.Random(10, 15);
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["ColdResistance"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["ColdResistance"].Bonus -= Bonus;
	}
}
