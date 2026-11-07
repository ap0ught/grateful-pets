using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades_Jab : BaseSkill
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(PooledEvent<GetMeleeAttackChanceEvent>.ID, EventOrder.EARLY);
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (!E.Primary && E.Weapon?.GetWeaponSkill() == "ShortBlades" && !E.Properties.HasDelimitedSubstring(',', "Flurrying"))
		{
			E.Attempts++;
		}
		return base.HandleEvent(E);
	}
}
