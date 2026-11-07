using System;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainDissociation_UnitOtherBodyBonus : ProceduralCookingEffectUnit
{
	public int Bonus = 30;

	public bool Applied;

	public override string GetDescription()
	{
		return "As long as your body isn't your own, you gain +" + Bonus + " Quickness.";
	}

	public override string GetTemplatedDescription()
	{
		return "As long as your body isn't your own, you gain +40-70 Quickness.";
	}

	public override void Init(GameObject target)
	{
		Bonus = Stat.Random(40, 70);
		Applied = false;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		if (Object.IsPlayer() && !Object.IsOriginalPlayerBody() && (Object.HasEffect<Dominated>() || Object.HasEffect<WakingDream>()))
		{
			Object.GetStat("Speed").BaseValue += Bonus;
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		if (Applied)
		{
			Object.GetStat("Speed").BaseValue -= Bonus;
		}
	}
}
