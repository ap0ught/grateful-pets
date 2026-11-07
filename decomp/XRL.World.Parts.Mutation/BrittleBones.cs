using System;
using XRL.Messages;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class BrittleBones : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeApplyDamage");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "Your bones are brittle.\n\nYou suffer 50% more damage from bludgeoning attacks, falling, and other sources of concussive damage.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if ((damage.HasAttribute("Crushing") || damage.HasAttribute("Cudgel") || damage.HasAttribute("Concussion")) && damage.Amount > 0)
			{
				if (ParentObject.IsPlayer())
				{
					MessageQueue.AddPlayerMessage("You feel your bones fracture.", 'r');
				}
				damage.Amount = (int)((double)damage.Amount * 1.5);
				E.SetParameter("Damage", damage);
			}
		}
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
