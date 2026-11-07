using System;

namespace XRL.World.Effects;

[Serializable]
public class Hampered : Effect
{
	public int Penalty = 25;

	public Hampered()
	{
		DisplayName = "{{w|hampered}}";
		Duration = 1;
	}

	public Hampered(int Penalty = 25)
		: this()
	{
		this.Penalty = Penalty;
	}

	public override string GetDescription()
	{
		return DisplayName;
	}

	public override int GetEffectType()
	{
		return 50331776;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as Hampered).Penalty != Penalty)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "-" + Penalty + " move speed for wielding a heavy weapon.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Applicable(Object))
		{
			return false;
		}
		if (Object.HasEffect<Hampered>())
		{
			return false;
		}
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift(Object, "MoveSpeed", Penalty);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		CheckApplicable();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}

	public static bool Applicable(GameObject Subject)
	{
		if (Subject.HasSkill("HeavyWeapons_Tank"))
		{
			return false;
		}
		return Subject.HasHeavyWeaponEquipped();
	}

	public void CheckApplicable(bool Immediate = false)
	{
		if ((Immediate || Duration > 0) && !Applicable(Object))
		{
			if (Immediate)
			{
				Object.RemoveEffect(this);
			}
			else
			{
				Duration = 0;
			}
		}
	}
}
