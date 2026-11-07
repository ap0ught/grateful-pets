using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFear_UnitBonusMA : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " MA";
	}

	public override string GetTemplatedDescription()
	{
		return "+2 MA";
	}

	public override void Init(GameObject target)
	{
		Bonus = 2;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["MA"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["MA"].Bonus -= Bonus;
		Bonus = 0;
	}
}
