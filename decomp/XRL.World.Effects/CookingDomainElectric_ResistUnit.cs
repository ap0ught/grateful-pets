using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainElectric_ResistUnit : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " Electric Resistance";
	}

	public override string GetTemplatedDescription()
	{
		return "+10-15 Electric Resist";
	}

	public override void Init(GameObject target)
	{
		Bonus = Stat.Random(10, 15);
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.GetStat("ElectricResistance").Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.GetStat("ElectricResistance").Bonus -= Bonus;
	}
}
