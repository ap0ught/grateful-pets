using System;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Stuck : Effect, ITierInitialized
{
	public const string DEFAULT_ADJECTIVE = "stuck";

	public const string DEFAULT_PREPOSITION = "in";

	public const int DEFAULT_SAVE_TARGET = 15;

	public const string DEFAULT_SAVE_VS = "Stuck Restraint";

	public const string DEFAULT_DEFENDING_SAVE_BONUS_VS = "Move";

	public const float DEFAULT_DEFENDING_SAVE_BONUS_FACTOR = 0.25f;

	public const float DEFAULT_KINETIC_RESISTANCE_LINEAR_BONUS_FACTOR = 1f;

	public const float DEFAULT_KINETIC_RESISTANCE_PERCENTAGE_BONUS_FACTOR = 1f;

	public GameObject DestroyOnBreak;

	public string Adjective = "stuck";

	public string Preposition = "in";

	public int _SaveTarget = 15;

	public string SaveVs = "Stuck Restraint";

	public string DependsOn;

	public string DefendingSaveBonusVs = "Move";

	public float DefendingSaveBonusFactor = 0.25f;

	public float KineticResistanceLinearBonusFactor = 1f;

	public float KineticResistancePercentageBonusFactor = 1f;

	public int DefendingSaveBonus;

	public int KineticResistanceLinearBonus;

	public int KineticResistancePercentageBonus;

	public bool DependsOnMustBeFrozen;

	public bool DependsOnMustBeSolid;

	public int SaveTarget
	{
		get
		{
			return _SaveTarget;
		}
		set
		{
			_SaveTarget = value;
			BonusSetup();
		}
	}

	public Stuck()
	{
		DisplayName = Adjective;
		BonusSetup();
	}

	public Stuck(int Duration, int SaveTarget = 15, string SaveVs = "Stuck Restraint", GameObject DestroyOnBreak = null, string Adjective = "stuck", string Preposition = "in", string DependsOn = null, string DefendingSaveBonusVs = "Move", float DefendingSaveBonusFactor = 0.25f, float KineticResistanceLinearBonus = 1f, float KineticResistancePercentageBonus = 1f, bool DependsOnMustBeFrozen = false, bool DependsOnMustBeSolid = false)
		: this()
	{
		base.Duration = Duration;
		this.SaveTarget = SaveTarget;
		this.SaveVs = SaveVs;
		this.DestroyOnBreak = DestroyOnBreak;
		this.Adjective = Adjective;
		this.Preposition = Preposition;
		this.DependsOn = DependsOn;
		this.DefendingSaveBonusVs = DefendingSaveBonusVs;
		this.DefendingSaveBonusFactor = DefendingSaveBonusFactor;
		KineticResistanceLinearBonusFactor = KineticResistanceLinearBonus;
		KineticResistancePercentageBonusFactor = KineticResistancePercentageBonus;
		this.DependsOnMustBeFrozen = DependsOnMustBeFrozen;
		this.DependsOnMustBeSolid = DependsOnMustBeSolid;
		BonusSetup();
		DisplayName = GetStateDescription();
	}

	public void Initialize(int Tier)
	{
		Tier = Stat.Random(Tier - 2, Tier + 2);
		if (Tier < 1)
		{
			Tier = 1;
		}
		if (Tier > 8)
		{
			Tier = 8;
		}
		Duration = Stat.Random(12, 15);
		SaveTarget = 11 + Tier * 4;
	}

	private void BonusSetup()
	{
		DefendingSaveBonus = (int)Math.Round((float)SaveTarget * DefendingSaveBonusFactor);
		KineticResistanceLinearBonus = (int)Math.Round((float)SaveTarget * KineticResistanceLinearBonusFactor);
		KineticResistancePercentageBonus = (int)Math.Round((float)SaveTarget * KineticResistancePercentageBonusFactor);
	}

	public override int GetEffectType()
	{
		return 117440640;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		Stuck stuck = e as Stuck;
		if (stuck.DestroyOnBreak != DestroyOnBreak)
		{
			return false;
		}
		if (stuck.Adjective != Adjective)
		{
			return false;
		}
		if (stuck.Preposition != Preposition)
		{
			return false;
		}
		if (stuck.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (stuck.SaveVs != SaveVs)
		{
			return false;
		}
		if (stuck.DependsOn != DependsOn)
		{
			return false;
		}
		if (stuck.DefendingSaveBonusVs != DefendingSaveBonusVs)
		{
			return false;
		}
		if (stuck.DefendingSaveBonusFactor != DefendingSaveBonusFactor)
		{
			return false;
		}
		if (stuck.KineticResistanceLinearBonusFactor != KineticResistanceLinearBonusFactor)
		{
			return false;
		}
		if (stuck.KineticResistancePercentageBonusFactor != KineticResistancePercentageBonusFactor)
		{
			return false;
		}
		if (stuck.DefendingSaveBonusFactor != DefendingSaveBonusFactor)
		{
			return false;
		}
		if (stuck.KineticResistanceLinearBonusFactor != KineticResistanceLinearBonusFactor)
		{
			return false;
		}
		if (stuck.KineticResistancePercentageBonusFactor != KineticResistancePercentageBonusFactor)
		{
			return false;
		}
		if (stuck.DefendingSaveBonus != DefendingSaveBonus)
		{
			return false;
		}
		if (stuck.KineticResistanceLinearBonus != KineticResistanceLinearBonus)
		{
			return false;
		}
		if (stuck.KineticResistancePercentageBonus != KineticResistancePercentageBonus)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CanChangeMovementModeEvent>.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetCompanionStatusEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetKineticResistanceEvent>.ID && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (Duration > 0 && E.Object == Object)
		{
			if (E.ShowMessage)
			{
				E.Object.Fail("You are " + Adjective + "!");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("stuck", 60);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference)
		{
			E.AddTag("[{{B|" + DisplayName + "}}]", 20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!E.Flying)
		{
			E.Uncacheable = true;
			E.MinWeight(100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			CheckDependsOn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckDependsOn(Immediate: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckDependsOn(Immediate: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.Validate(ref DestroyOnBreak);
		if (Duration > 0)
		{
			CheckDependsOn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (DefendingSaveBonus != 0 && SavingThrows.Applicable(DefendingSaveBonusVs, E.Vs))
		{
			E.Roll += DefendingSaveBonus;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetKineticResistanceEvent E)
	{
		if (KineticResistanceLinearBonus > 0)
		{
			E.LinearIncrease += KineticResistanceLinearBonus;
		}
		else if (KineticResistanceLinearBonus < 0)
		{
			E.LinearReduction += -KineticResistanceLinearBonus;
		}
		if (KineticResistancePercentageBonus > 0)
		{
			E.PercentageIncrease += KineticResistancePercentageBonus;
		}
		else if (KineticResistancePercentageBonus < 0)
		{
			E.PercentageReduction += -KineticResistancePercentageBonus;
		}
		return base.HandleEvent(E);
	}

	public GameObject CheckDependsOn(bool Immediate = false, bool AllowDisplayNameUpdate = true)
	{
		GameObject Object = null;
		if (!DependsOn.IsNullOrEmpty())
		{
			Object = GameObject.FindByID(DependsOn);
			if (!GameObject.Validate(ref Object) || !Object.InSameOrAdjacentCellTo(base.Object) || (!Object.PhaseMatches(base.Object) && !Object.HasTagOrProperty("IgnorePhaseMatchForStuck")) || (DependsOnMustBeFrozen && !Object.IsFrozen()) || (DependsOnMustBeSolid && !Object.ConsiderSolidFor(base.Object)))
			{
				DependsOn = null;
				if (Immediate)
				{
					base.Object.RemoveEffect(this);
				}
				else
				{
					Duration = 0;
				}
			}
			else if (AllowDisplayNameUpdate)
			{
				DisplayName = GetStateDescription(Object);
			}
		}
		return Object;
	}

	public string GetStateDescription(GameObject From = null)
	{
		if (From == null && !DependsOn.IsNullOrEmpty())
		{
			From = GameObject.FindByID(DependsOn);
		}
		if (GameObject.Validate(ref From))
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append(Adjective);
			if (!Preposition.IsNullOrEmpty())
			{
				stringBuilder.Append(' ').Append(Preposition);
			}
			stringBuilder.Append(' ').Append(From.an());
			return stringBuilder.ToString();
		}
		return Adjective;
	}

	public GameObject CheckDependsOn(string Invalidate, bool Immediate = false, bool AllowDisplayNameUpdate = true)
	{
		if (!DependsOn.IsNullOrEmpty() && Invalidate == DependsOn)
		{
			if (Immediate)
			{
				Object.RemoveEffect(this);
			}
			else
			{
				Duration = 0;
			}
			return null;
		}
		return CheckDependsOn(Immediate, AllowDisplayNameUpdate);
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(Adjective.Capitalize()).Append(' ');
		GameObject gameObject = CheckDependsOn();
		if (gameObject != null)
		{
			stringBuilder.Append("in ").Append(gameObject.an());
		}
		else
		{
			stringBuilder.Append("where ").Append(Object.it).Append(Object.Is)
				.Append('.');
		}
		SavingThrows.AppendSaveBonusDescription(stringBuilder, DefendingSaveBonus, DefendingSaveBonusVs);
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasStat("Energy"))
		{
			return false;
		}
		if (Object.GetMatterPhase() > 1)
		{
			return false;
		}
		if (!Object.CanChangeMovementMode("Stuck", ShowMessage: false, Involuntary: true, AllowTelekinetic: false, FrozenOkay: true))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyStuck"))
		{
			return false;
		}
		CheckDependsOn();
		DidX("are", DisplayName, "!", null, null, null, Object);
		Object.ParticleText("*" + Adjective + "*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		Object.ForfeitTurn();
		Object.MovementModeChanged("Stuck", Involuntary: true);
		if (Object.IsFlying)
		{
			Flight.Fall(Object);
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.Validate(ref DestroyOnBreak))
		{
			DestroyOnBreak.Destroy();
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginAttack");
		Registrar.Register("BodyPositionChanged");
		Registrar.Register("CanChangeBodyPosition");
		Registrar.Register("CheckStuck");
		Registrar.Register("IsMobile");
		Registrar.Register("LeaveCell");
		Registrar.Register("MovementModeChanged");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile")
		{
			if (Duration > 0 && !Object.IsTryingToJoinPartyLeader())
			{
				return false;
			}
		}
		else if (E.ID == "LeaveCell" || E.ID == "BeginAttack")
		{
			if (E.ID == "LeaveCell" && (E.HasFlag("Forced") || E.GetStringParameter("Type") == "Teleporting" || Object.IsTryingToJoinPartyLeader()))
			{
				Object.RemoveEffect(this);
			}
			else if (Duration > 0)
			{
				if (Object.MakeSave("Strength", SaveTarget - Object.GetIntProperty("Stable"), null, null, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, GameObject.FindByID(DependsOn)))
				{
					DidX("break", "free", "!", null, null, Object);
					Object.RemoveEffect(this);
				}
				else
				{
					if (Object.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You are " + Adjective + "!");
					}
					if (!E.HasParameter("Dragging") && !E.HasFlag("Forced"))
					{
						Object.UseEnergy(1000);
					}
					if (E.ID == "LeaveCell")
					{
						return false;
					}
				}
			}
		}
		else if (E.ID == "CanChangeBodyPosition")
		{
			if (Duration > 0)
			{
				if (E.HasFlag("ShowMessage") && Object.IsPlayer())
				{
					Popup.Show("You are " + Adjective + "!");
				}
				return false;
			}
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			if (Duration > 0)
			{
				Object.RemoveEffect(this);
			}
		}
		else if (E.ID == "CheckStuck")
		{
			string stringParameter = E.GetStringParameter("Invalidate");
			if (!string.IsNullOrEmpty(stringParameter))
			{
				CheckDependsOn(stringParameter, Immediate: true);
			}
			else
			{
				CheckDependsOn(Immediate: true);
			}
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 50 && num < 60)
			{
				E.Tile = "terrain/sw_web.bmp";
				E.RenderString = "\u000f";
				E.ColorString = "&Y^K";
			}
		}
		return true;
	}
}
