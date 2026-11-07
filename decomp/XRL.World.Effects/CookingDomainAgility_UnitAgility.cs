using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainAgility_UnitAgility : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " Agility";
	}

	public override string GetTemplatedDescription()
	{
		return "+4 Agility";
	}

	public override void Init(GameObject target)
	{
		Bonus = 4;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["Agility"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["Agility"].Bonus -= Bonus;
	}
}
