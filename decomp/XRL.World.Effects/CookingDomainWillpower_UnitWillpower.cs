using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainWillpower_UnitWillpower : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " Willpower";
	}

	public override string GetTemplatedDescription()
	{
		return "+4 Willpower";
	}

	public override void Init(GameObject target)
	{
		Bonus = 4;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["Willpower"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["Willpower"].Bonus -= Bonus;
	}
}
