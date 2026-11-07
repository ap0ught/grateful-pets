using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainLove_UnitEgo : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " Ego";
	}

	public override string GetTemplatedDescription()
	{
		return "+4 Ego";
	}

	public override void Init(GameObject target)
	{
		Bonus = 4;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["Ego"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["Ego"].Bonus -= Bonus;
	}
}
