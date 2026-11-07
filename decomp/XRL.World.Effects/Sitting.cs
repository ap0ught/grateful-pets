using System;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Sitting : Effect, ITierInitialized
{
	public int HealCounter;

	public string DamageAttributes;

	public GameObject SittingOn;

	public int Level;

	public Sitting()
	{
		Duration = 1;
		DisplayName = "{{C|sitting}}";
	}

	public Sitting(int Level)
		: this()
	{
		this.Level = Level;
	}

	public Sitting(int Level, string DamageAttributes)
		: this(Level)
	{
		this.DamageAttributes = DamageAttributes;
	}

	public Sitting(GameObject SittingOn)
	{
		this.SittingOn = SittingOn;
		Duration = 1;
		DisplayName = "{{C|sitting on " + SittingOn.an() + "}}";
	}

	public Sitting(GameObject SittingOn, int Level)
		: this(SittingOn)
	{
		this.Level = Level;
	}

	public Sitting(GameObject SittingOn, int Level, string DamageAttributes)
		: this(SittingOn, Level)
	{
		this.DamageAttributes = DamageAttributes;
	}

	public void Initialize(int Tier)
	{
		Level = Stat.Random(-5, 5);
	}

	public override int GetEffectType()
	{
		return 83886208;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		if (Level >= 5)
		{
			return "Improves natural healing rate.\nAids in examining and disassembling artifacts.\n-6 DV.\nMust spend a turn to stand up before moving.";
		}
		if (Level > -2)
		{
			return "Slightly improves natural healing rate.\nAids in examining and disassembling artifacts.\n-6 DV.\nMust spend a turn to stand up before moving.";
		}
		if (Level == -2)
		{
			return "Slightly improves natural healing rate.\n-6 DV.\nMust spend a turn to stand up before moving.";
		}
		if (Level > -10)
		{
			return "Slightly improves natural healing rate.\nDistracts from examining and disassembling artifacts.\n-6 DV.\nMust spend a turn to stand up before moving.";
		}
		return "Inflicts ongoing damage.\nDistracts from examining and disassembling artifacts.\n-6 DV.\nMust spend a turn to stand up before moving.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.CanChangeBodyPosition("Sitting"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplySitting"))
		{
			return false;
		}
		if (Object.CurrentCell != null)
		{
			Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_plop");
			Object.CurrentCell.FireEvent(Event.New("ObjectSitting", "Object", Object, "SittingOn", SittingOn));
		}
		Object.BodyPositionChanged("Sitting");
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Duration = 0;
		Object.BodyPositionChanged("NotSitting");
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_standUp");
		UnapplyStats();
		base.Remove(Object);
	}

	public bool IsSittingOnValid()
	{
		if (!GameObject.Validate(ref SittingOn))
		{
			return false;
		}
		if (SittingOn.CurrentCell == null)
		{
			return false;
		}
		if (!GameObject.Validate(Object))
		{
			return false;
		}
		if (Object.CurrentCell == null)
		{
			return false;
		}
		if (SittingOn.CurrentCell != Object.CurrentCell)
		{
			return false;
		}
		if (!SittingOn.PhaseAndFlightMatches(Object))
		{
			return false;
		}
		return true;
	}

	public bool CheckSittingOn()
	{
		if (!IsSittingOnValid())
		{
			SittingOn = null;
			Level = 0;
			DamageAttributes = null;
			return false;
		}
		if (SittingOn != null)
		{
			DisplayName = "{{C|sitting on " + SittingOn.an() + "}}";
		}
		return true;
	}

	private void ApplyStats()
	{
		StatShifter.SetStatShift("DV", -6);
	}

	private void UnapplyStats()
	{
		StatShifter.RemoveStatShifts();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetTinkeringBonusEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.Type == "Inspect" || E.Type == "Disassemble")
		{
			int num = Math.Min(2 + Level, 4);
			if (num != 0)
			{
				E.Bonus += num;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (CheckSittingOn())
		{
			if (Level > -10)
			{
				if (++HealCounter >= 10 - Level)
				{
					Object.Heal(1);
					HealCounter = 0;
				}
			}
			else if (Object.TakeDamage(-Level - 9, Attributes: DamageAttributes, Attacker: SittingOn, Message: (SittingOn == null) ? "from sitting!" : "from %O!") && 50.in100())
			{
				Object.Bloodsplatter();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference)
		{
			CheckSittingOn();
			if (SittingOn != null)
			{
				E.AddTag("[{{B|sitting on " + SittingOn.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true) + "}}]", 20);
			}
			else
			{
				E.AddTag("[{{B|sitting}}]", 20);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("BodyPositionChanged");
		Registrar.Register("LeaveCell");
		Registrar.Register("MovementModeChanged");
		base.Register(Object, Registrar);
	}

	private void StandUp(Event E = null)
	{
		CheckSittingOn();
		if (SittingOn != null)
		{
			SittingOn.GetPart<Chair>().StandUp(Object, E, this);
			return;
		}
		DidX("stand", "up", null, null, null, Object);
		Object.UseEnergy(1000, "Position");
		Object.RemoveEffect(this);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LeaveCell")
		{
			if (Duration > 0)
			{
				Cell cell = E.GetParameter("Cell") as Cell;
				Chair chair = SittingOn?.GetPart<Chair>();
				if (cell == null || chair == null || !chair.Securing || cell.HasObject(SittingOn))
				{
					StandUp(E);
				}
			}
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			if (Duration > 0 && E.GetStringParameter("To") != "Asleep")
			{
				StandUp(E);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
