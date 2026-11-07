using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// <remarks>
/// This part is not used in the base game.
///
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is not by default, chance to activate is increased by a
/// percentage equal to ((power load - 100) / 10), i.e. 30% for
/// the standard overload power load of 400, and damage is increased
/// by the standard power load bonus, i.e. 2 for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class ModDefib : IModification
{
	public ModDefib()
	{
	}

	public ModDefib(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnEquipper = true;
		IsPowerLoadSensitive = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!IModification.CheckWornSlot(Object, "Body", "Back"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{love|defib}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckDefib(GetActivePartFirstSubject());
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Defib: When powered, if the wearer enters cardiac arrest, this armor will deliver an electrical shock to the wearer every turn until their heart restarts.";
	}

	public bool? CheckDefib(GameObject Subject)
	{
		if (!GameObject.Validate(ref Subject))
		{
			return false;
		}
		if (!Subject.HasEffect<CardiacArrest>())
		{
			return false;
		}
		int load = MyPowerLoadLevel();
		int num = Stat.Random(1, 4) + IComponent<GameObject>.PowerLoadBonus(load);
		Subject.TakeDamage(num, Owner: ParentObject, Message: "from an {{electrical|electrical shock}} delivered by " + Subject.poss(ParentObject) + ".", Attributes: "Electric", DeathReason: null, ThirdPersonDeathReason: null, Attacker: null, Source: null, Perspective: null, DescribeAsFrom: null, Accidental: true);
		if (GameObject.Validate(ref Subject) && Subject.HasEffect<CardiacArrest>())
		{
			int num2 = 30 + Tier * 5;
			int num3 = IComponent<GameObject>.PowerLoadBonus(load, 100, 10);
			if (num3 != 0)
			{
				num2 = num2 * (100 + num3) / 100;
			}
			if (num2.in100())
			{
				Subject.RemoveEffect<CardiacArrest>();
			}
		}
		if (!Subject.HasEffect<CardiacArrest>())
		{
			return true;
		}
		return null;
	}
}
