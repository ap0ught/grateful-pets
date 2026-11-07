using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainQuickness_UnitQuickness : ProceduralCookingEffectUnit
{
	public int Bonus;

	public override string GetDescription()
	{
		return Bonus.Signed() + " Quickness";
	}

	public override string GetTemplatedDescription()
	{
		return "+4-5 Quickness";
	}

	public override void Init(GameObject target)
	{
		Bonus = Stat.Random(4, 5);
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["Speed"].Bonus += Bonus;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["Speed"].Bonus -= Bonus;
	}
}
