using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainElectric_ExtraResistUnit : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " Electric Resistance";
	}

	public override string GetTemplatedDescription()
	{
		return "+50-75 Electric Resist";
	}

	public override void Init(GameObject target)
	{
		Bonus = Stat.Random(50, 75);
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.GetStat("ElectricResistance").Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.GetStat("ElectricResistance").Bonus -= Bonus;
		Bonus = 0;
	}
}
