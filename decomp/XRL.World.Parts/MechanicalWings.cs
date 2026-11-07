using System;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class MechanicalWings : IPoweredPart, IFlightSource
{
	public int Level = 1;

	public int BaseFallChance = 6;

	public int CrashBreakChance = 1;

	public bool RequiresOngoingEffort = true;

	public bool AbilitiesAppliedToEquipper;

	public string _FlightEvent = "ActivateMechanicalWings";

	public bool _FlightFlying;

	public Guid _FlightActivatedAbilityID = Guid.Empty;

	public string _FlightSourceDescription = "Mechanical Wings";

	public string _Type = "MechanicalWings";

	public int FlightLevel => Level;

	public int FlightBaseFallChance => BaseFallChance;

	public bool FlightRequiresOngoingEffort => RequiresOngoingEffort;

	public string FlightEvent => _FlightEvent;

	public string FlightActivatedAbilityClass => "Items";

	public bool FlightFlying
	{
		get
		{
			return _FlightFlying;
		}
		set
		{
			_FlightFlying = value;
		}
	}

	public Guid FlightActivatedAbilityID
	{
		get
		{
			return _FlightActivatedAbilityID;
		}
		set
		{
			_FlightActivatedAbilityID = value;
		}
	}

	public string FlightSourceDescription
	{
		get
		{
			return _FlightSourceDescription;
		}
		set
		{
			_FlightSourceDescription = value;
		}
	}

	public string Type
	{
		get
		{
			return _Type;
		}
		set
		{
			_Type = value;
			_FlightEvent = "Activate" + Type;
		}
	}

	public GameObject User => GetActivePartFirstSubject();

	public MechanicalWings()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		MustBeUnderstood = true;
		WorksOnWearer = true;
	}

	public override bool SameAs(IPart p)
	{
		MechanicalWings mechanicalWings = p as MechanicalWings;
		if (mechanicalWings.Level != Level)
		{
			return false;
		}
		if (mechanicalWings.BaseFallChance != BaseFallChance)
		{
			return false;
		}
		if (mechanicalWings.CrashBreakChance != CrashBreakChance)
		{
			return false;
		}
		if (mechanicalWings.RequiresOngoingEffort != RequiresOngoingEffort)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void CheckOperation()
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Flight.FailFlying(ParentObject, User, this);
		}
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (FlightFlying && !OnWorldMap)
		{
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Amount, null, UseChargeIfUnpowered: false, 0L))
			{
				Flight.MaintainFlight(ParentObject, User, this);
			}
			else
			{
				Flight.FailFlying(ParentObject, User, this);
			}
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != AIGetPassiveAbilityListEvent.ID && ID != PooledEvent<AttemptToLandEvent>.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID && ID != PooledEvent<BodyPositionChangedEvent>.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EquippedEvent.ID && ID != PooledEvent<ExamineSuccessEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetLostChanceEvent.ID && ID != GetMovementCapabilitiesEvent.ID && ID != InventoryActionEvent.ID && ID != PooledEvent<MovementModeChangedEvent>.ID && ID != ReplicaCreatedEvent.ID && ID != TravelSpeedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("CrashChance", Flight.GetMoveFallChance(User, this));
		stats.Set("SwoopCrashChance", Flight.GetSwoopFallChance(User, this));
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(FlightActivatedAbilityID, CollectStats, User);
		ActivatedAbilityEntry activatedAbilityEntry = User?.GetActivatedAbilityByCommand(Flight.SWOOP_ATTACK_COMMAND_NAME);
		if (activatedAbilityEntry != null)
		{
			DescribeMyActivatedAbility(activatedAbilityEntry.ID, CollectStats, User);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		if (IsObjectActivePartSubject(E.Actor))
		{
			ActivatedAbilityEntry activatedAbility = User.GetActivatedAbility(FlightActivatedAbilityID);
			E.Add(activatedAbility.DisplayName, FlightEvent, 20000, activatedAbility);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineSuccessEvent E)
	{
		if (E.Object == ParentObject)
		{
			ApplyOperations(User);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetPassiveAbilityListEvent E)
	{
		if (!FlightFlying && E.Actor == User && Flight.EnvironmentAllowsFlight(E.Actor) && Flight.IsAbilityAIUsable(this, E.Actor))
		{
			E.Add(FlightEvent);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (!FlightFlying && User == E.Actor && Flight.EnvironmentAllowsFlight(E.Actor) && Flight.IsAbilityAIUsable(this, E.Actor))
		{
			E.Add(FlightEvent);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == User)
		{
			Flight.SyncFlying(ParentObject, User, this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetLostChanceEvent E)
	{
		if (WasReady())
		{
			E.PercentageBonus += 36 + 4 * Level;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TravelSpeedEvent E)
	{
		if (WasReady())
		{
			E.PercentageBonus += 50 + 50 * Level;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BodyPositionChangedEvent E)
	{
		if (E.Object == User && FlightFlying)
		{
			if (E.Involuntary)
			{
				Flight.FailFlying(ParentObject, User, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, User, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MovementModeChangedEvent E)
	{
		if (E.Object == User && FlightFlying)
		{
			if (E.Involuntary)
			{
				Flight.FailFlying(ParentObject, User, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, User, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AttemptToLandEvent E)
	{
		if (FlightFlying && Flight.StopFlying(ParentObject, User, this))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		CheckOperation();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckOperation();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		ApplyOperations(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		Flight.FailFlying(ParentObject, E.Actor, this);
		UnapplyOperations(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) != ActivePartStatus.NeedsSubject)
		{
			E.AddAction("Activate", "activate", "ActivateMechanicalWings", null, 'a');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateMechanicalWings" && User != null && TryStartup())
		{
			bool flightFlying = FlightFlying;
			User.FireEvent(_FlightEvent);
			if (FlightFlying != flightFlying)
			{
				E.RequestInterfaceExit();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("travel", 2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckOperation();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(_FlightEvent);
		Registrar.Register("FlightFailed");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Flight.CheckFlight(ParentObject, User, this);
		}
		else if (E.ID == _FlightEvent)
		{
			if (User.IsActivatedAbilityToggledOn(FlightActivatedAbilityID))
			{
				if (User.IsPlayer() && currentCell != null && User.GetEffectCount(typeof(Flying)) <= 1)
				{
					int i = 0;
					for (int count = currentCell.Objects.Count; i < count; i++)
					{
						GameObject gameObject = currentCell.Objects[i];
						StairsDown part = gameObject.GetPart<StairsDown>();
						if (part != null && part.IsLongFall() && Popup.WarnYesNo("It looks like a long way down " + gameObject.the + gameObject.ShortDisplayName + " you're above. Are you sure you want to stop flying?") != DialogResult.Yes)
						{
							return false;
						}
					}
				}
				Flight.StopFlying(ParentObject, User, this);
			}
			else
			{
				if (!TryStartup())
				{
					return false;
				}
				Flight.StartFlying(ParentObject, User, this);
			}
		}
		else if (E.ID == "FlightFailed" && User != null && CrashBreakChance.in100())
		{
			ParentObject.ApplyEffect(new Broken());
		}
		return base.FireEvent(E);
	}

	public override bool IsActivePartEngaged()
	{
		if (!FlightFlying)
		{
			return false;
		}
		return base.IsActivePartEngaged();
	}

	private bool TryStartup()
	{
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			if (User != null && User.IsPlayer())
			{
				if (activePartStatus == ActivePartStatus.Booting && ParentObject.GetPart<BootSequence>().IsObvious())
				{
					Popup.ShowFail(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " still starting up.");
				}
				else
				{
					Popup.ShowFail(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " unresponsive.");
				}
			}
			return false;
		}
		PlayWorldSound("Sounds/Interact/sfx_interact_mechanicalWings_on");
		return true;
	}

	private void ApplyOperations(GameObject who)
	{
		if (GameObject.Validate(ref who) && !AbilitiesAppliedToEquipper && IsObjectActivePartSubject(who))
		{
			AbilitiesAppliedToEquipper = true;
			who.RegisterPartEvent(this, _FlightEvent);
			who.RegisterPartEvent(this, "EnteredCell");
			who.RegisterPartEvent(this, "FlightFailed");
			Flight.AbilitySetup(ParentObject, who, this);
		}
	}

	private void UnapplyOperations(GameObject who)
	{
		if (AbilitiesAppliedToEquipper)
		{
			AbilitiesAppliedToEquipper = false;
			if (GameObject.Validate(ref who))
			{
				who.UnregisterPartEvent(this, _FlightEvent);
				who.UnregisterPartEvent(this, "EnteredCell");
				who.UnregisterPartEvent(this, "FlightFailed");
				Flight.AbilityTeardown(ParentObject, who, this);
			}
		}
	}
}
