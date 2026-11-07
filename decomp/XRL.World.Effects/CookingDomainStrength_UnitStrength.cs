using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainStrength_UnitStrength : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " Strength";
	}

	public override string GetTemplatedDescription()
	{
		return "+4 Strength";
	}

	public override void Init(GameObject target)
	{
		Bonus = 4;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["Strength"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["Strength"].Bonus -= Bonus;
	}
}
