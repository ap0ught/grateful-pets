using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainArmor_UnitAV : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " AV";
	}

	public override string GetTemplatedDescription()
	{
		return "+2 AV";
	}

	public override void Init(GameObject target)
	{
		Bonus = 2;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["AV"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["AV"].Bonus -= Bonus;
	}
}
