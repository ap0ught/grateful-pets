using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class BreakableInMelee : IPart
{
	public int Chance = 1;

	public bool WhileInInventory;

	public override bool SameAs(IPart Part)
	{
		BreakableInMelee breakableInMelee = Part as BreakableInMelee;
		if (breakableInMelee.Chance != Chance)
		{
			return false;
		}
		if (breakableInMelee.WhileInInventory != WhileInInventory)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			if (ID == PooledEvent<DefendMeleeHitEvent>.ID)
			{
				if (!WhileInInventory)
				{
					return ParentObject.Equipped != null;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(DefendMeleeHitEvent E)
	{
		if (E.Damage != null && E.Damage.Amount > 0 && (WhileInInventory || ParentObject.Equipped != null) && Chance.in100() && ParentObject.ApplyEffect(new Broken()))
		{
			DidX("break", null, null, null, null, null, ParentObject.Equipped);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
