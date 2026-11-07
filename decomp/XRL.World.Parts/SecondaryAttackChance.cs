using System;

namespace XRL.World.Parts;

[Serializable]
public class SecondaryAttackChance : IPart
{
	public int Chance;

	public double Multiplier = 1.0;

	public bool Additive;

	public bool Final;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetMeleeAttackChanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (E.Intrinsic && !E.Primary && E.Weapon == ParentObject)
		{
			if (Additive)
			{
				E.Chance += Chance;
				E.Multiplier += Multiplier - 1.0;
			}
			else
			{
				if (E.Chance < Chance)
				{
					E.Chance = Chance;
				}
				E.Multiplier *= Multiplier;
			}
			if (Final)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}
}
