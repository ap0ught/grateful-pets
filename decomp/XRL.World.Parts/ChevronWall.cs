using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ChevronWall : IPart
{
	public const int REFLECTION_CHANCE = 3;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterDieEvent.ID;
		}
		return true;
	}

	public bool IsValidTarget(GameObject Object)
	{
		if (GameObject.Validate(Object) && Object.IsCreature)
		{
			return !Object.HasPart<MentalShield>();
		}
		return false;
	}

	public override bool HandleEvent(AfterDieEvent E)
	{
		PlayWorldSound("shatter");
		ParentObject.CrystalSpray();
		if (IsValidTarget(E.Killer) && 3.in100())
		{
			Cell cell = ParentObject.CurrentCell;
			int num = Stat.Random(1, 6);
			int i = 0;
			for (int num2 = num; i < num2; i++)
			{
				Cell closestPassableCellFor = cell.getClosestPassableCellFor(E.Killer);
				if (closestPassableCellFor != null)
				{
					EvilTwin.CreateEvilTwin(E.Killer, "anti-", closestPassableCellFor, null, "&K", null, null, MakeExtras: true, "It's negative you.");
				}
			}
			if (num == 1)
			{
				E.Killer.EmitMessage("As the prism shatters, a reflection of " + E.Killer.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: true) + " is caught on the limen of realities and appears out of nowhere.");
			}
			else
			{
				E.Killer.EmitMessage("As the prism shatters, reflections of " + E.Killer.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: true) + " are caught on the limen of realities and appear out of nowhere.");
			}
		}
		return base.HandleEvent(E);
	}
}
