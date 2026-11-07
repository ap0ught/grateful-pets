using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainDarkness_UnitDV : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " DV";
	}

	public override string GetTemplatedDescription()
	{
		return "+4 DV";
	}

	public override void Init(GameObject target)
	{
		Bonus = 4;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["DV"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["DV"].Bonus -= Bonus;
	}
}
