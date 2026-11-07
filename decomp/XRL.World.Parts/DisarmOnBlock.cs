using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class DisarmOnBlock : IPart
{
	public string SaveStat;

	public string DisarmerStat;

	public int SaveTarget = 40;

	public int Chance = 100;

	public DisarmOnBlock()
	{
	}

	public DisarmOnBlock(int Chance)
		: this()
	{
		this.Chance = Chance;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == PooledEvent<AfterShieldBlockEvent>.ID;
	}

	public override bool HandleEvent(AfterShieldBlockEvent E)
	{
		if ((E.Shield == ParentObject || E.Defender == ParentObject) && GetSpecialEffectChanceEvent.GetFor(E.Defender, ParentObject, "Part DisarmOnBlock Activation", Subject: E.Attacker, Chance: Chance).in100() && GameObject.Validate(E.Weapon))
		{
			Disarming.Disarm(E.Attacker, E.Defender, SaveTarget, SaveStat.Coalesce("Strength"), DisarmerStat.Coalesce("Agility"), E.Weapon, ParentObject);
		}
		return base.HandleEvent(E);
	}
}
