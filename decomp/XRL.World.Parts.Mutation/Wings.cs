using System;
using System.Text;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Wings : BaseDefaultEquipmentMutation, IFlightSource
{
	public static readonly string COMMAND_NAME = "CommandFlight";

	public GameObject WingsObject;

	public string BodyPartType = "Back";

	public int BaseFallChance = 6;

	public bool _FlightFlying;

	public Guid _FlightActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	protected GameObjectBlueprint _Blueprint;

	public int appliedChargeBonus;

	public int appliedJumpBonus;

	public int FlightLevel => Level;

	public int FlightBaseFallChance => BaseFallChance;

	public bool FlightRequiresOngoingEffort => true;

	public string FlightEvent => COMMAND_NAME;

	public string FlightActivatedAbilityClass => "Physical Mutation";

	public string FlightSourceDescription => null;

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

	public string ManagerID => ParentObject.ID + "::Wings";

	public string BlueprintName => Variant.Coalesce("Wings");

	public GameObjectBlueprint Blueprint => _Blueprint ?? (_Blueprint = GameObjectFactory.Factory.GetBlueprint(BlueprintName));

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Wings obj = base.DeepCopy(Parent, MapInv) as Wings;
		obj.WingsObject = null;
		return obj;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != AIGetPassiveAbilityListEvent.ID && ID != PooledEvent<AttemptToLandEvent>.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID && ID != PooledEvent<BodyPositionChangedEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EnteredCellEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != PooledEvent<GetJumpingBehaviorEvent>.ID && ID != GetLostChanceEvent.ID && ID != GetMovementCapabilitiesEvent.ID && ID != PooledEvent<GetPropertyModDescription>.ID && ID != PooledEvent<MovementModeChangedEvent>.ID && ID != ObjectStoppedFlyingEvent.ID && ID != ReplicaCreatedEvent.ID && ID != PooledEvent<TransparentToEMPEvent>.ID)
		{
			return ID == TravelSpeedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPropertyModDescription E)
	{
		if (E.Property == "ChargeRangeModifier")
		{
			E.AddLinearBonusModifier(appliedChargeBonus, "wings");
		}
		return base.HandleEvent(E);
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("CrashChance", Flight.GetMoveFallChance(ParentObject, this));
		stats.Set("SwoopCrashChance", Flight.GetSwoopFallChance(ParentObject, this));
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(FlightActivatedAbilityID, CollectStats, ParentObject);
		ActivatedAbilityEntry activatedAbilityEntry = ParentObject?.GetActivatedAbilityByCommand(Flight.SWOOP_ATTACK_COMMAND_NAME);
		if (activatedAbilityEntry != null)
		{
			DescribeMyActivatedAbility(activatedAbilityEntry.ID, CollectStats, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		if (E.Actor == ParentObject)
		{
			ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(FlightActivatedAbilityID);
			E.Add(activatedAbilityEntry.DisplayName, FlightEvent, 19000, activatedAbilityEntry);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (IsMyActivatedAbilityToggledOn(FlightActivatedAbilityID))
			{
				if (ParentObject.IsPlayer() && currentCell != null && ParentObject.GetEffectCount(typeof(Flying)) <= 1)
				{
					int i = 0;
					for (int count = currentCell.Objects.Count; i < count; i++)
					{
						GameObject gameObject = currentCell.Objects[i];
						StairsDown part = gameObject.GetPart<StairsDown>();
						if (part != null && part.IsLongFall() && Popup.WarnYesNo("It looks like a long way down " + gameObject.t() + " you're above. Are you sure you want to stop flying?") != DialogResult.Yes)
						{
							return false;
						}
					}
				}
				Flight.StopFlying(ParentObject, ParentObject, this);
			}
			else
			{
				if (ParentObject.IsEMPed() && MutationsSubjectToEMPEvent.Check(ParentObject))
				{
					return ParentObject.Fail(ParentObject.Poss(Blueprint.CachedDisplayNameStripped) + " will not move!");
				}
				Flight.StartFlying(ParentObject, ParentObject, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetJumpingBehaviorEvent E)
	{
		if (E.AbilityName == "Jump" && !E.CanJumpOverCreatures && IsMyActivatedAbilityUsable(FlightActivatedAbilityID) && !ParentObject.HasEffect<Grounded>())
		{
			E.CanJumpOverCreatures = true;
			E.Stats?.Set("CanJumpOverCreatures", "true");
			E.Stats?.AddLinearBonusModifier("Range", appliedJumpBonus, "wings");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectStoppedFlyingEvent E)
	{
		Acrobatics_Jump.SyncAbility(ParentObject);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetPassiveAbilityListEvent E)
	{
		if (!FlightFlying && E.Actor == ParentObject && Flight.EnvironmentAllowsFlight(E.Actor) && Flight.IsAbilityAIUsable(this, E.Actor) && (!E.Actor.IsEMPed() || !MutationsSubjectToEMPEvent.Check(E.Actor)))
		{
			E.Add(FlightEvent);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (!FlightFlying && Flight.EnvironmentAllowsFlight(E.Actor) && Flight.IsAbilityAIUsable(this, E.Actor) && (!E.Actor.IsEMPed() || !MutationsSubjectToEMPEvent.Check(E.Actor)))
		{
			E.Add(FlightEvent);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TransparentToEMPEvent E)
	{
		if (MutationsSubjectToEMPEvent.Check(ParentObject))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckEMP();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckEMP();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetLostChanceEvent E)
	{
		E.PercentageBonus += 36 + 4 * Level;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			Flight.SyncFlying(ParentObject, ParentObject, this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TravelSpeedEvent E)
	{
		E.PercentageBonus += 50 + 50 * Level;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BodyPositionChangedEvent E)
	{
		if (FlightFlying && E.To != "Flying")
		{
			if (E.Involuntary)
			{
				Flight.FailFlying(ParentObject, ParentObject, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, ParentObject, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MovementModeChangedEvent E)
	{
		if (FlightFlying && E.To != "Flying")
		{
			if (E.Involuntary)
			{
				Flight.FailFlying(ParentObject, ParentObject, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, ParentObject, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AttemptToLandEvent E)
	{
		if (FlightFlying && Flight.StopFlying(ParentObject, ParentObject, this))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckEMP();
		Flight.MaintainFlight(ParentObject, ParentObject, this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckEMP();
		Flight.CheckFlight(ParentObject, ParentObject, this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return Blueprint.GetTag("VariantDescription").Coalesce("You fly.");
	}

	public float SprintingMoveSpeedBonus(int Level)
	{
		return 0.1f + 0.1f * (float)Level;
	}

	public int GetJumpDistanceBonus(int Level)
	{
		return 1 + Level / 3;
	}

	public int GetChargeDistanceBonus(int Level)
	{
		return 2 + Level / 3;
	}

	public override string GetLevelText(int Level)
	{
		int num = Math.Max(0, FlightBaseFallChance - Level);
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("You travel on the world map at {{rules|").Append(1.5 + 0.5 * (double)Level).Append("x}} speed.\n");
		stringBuilder.Append("{{rules|" + (36 + Level * 4)).Append("%}} reduced chance of becoming lost\n");
		stringBuilder.Append("While outside, you may fly. You cannot be hit in melee by grounded creatures while flying.\n");
		stringBuilder.Append("{{rules|" + num).Append("%}} chance of falling clumsily to the ground\n");
		stringBuilder.Append("{{rules|" + ((int)(SprintingMoveSpeedBonus(Level) * 100f)).Signed() + "%}} move speed while sprinting\n");
		stringBuilder.Append("You can jump {{rules|" + GetJumpDistanceBonus(Level) + ((GetJumpDistanceBonus(Level) == 1) ? "}} square" : "}} squares") + " farther.\n");
		stringBuilder.Append("You can charge {{rules|" + GetChargeDistanceBonus(Level) + ((GetChargeDistanceBonus(Level) == 1) ? "}} square" : "}} squares") + " farther.\n");
		stringBuilder.Append("+300 reputation with {{w|birds}} and {{w|winged mammals}}");
		return stringBuilder.ToString();
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		string partParameter = Blueprint.GetPartParameter("Armor", "WornOn", "Back");
		if (!TryGetRegisteredSlot(body, partParameter, out var Part))
		{
			Part = body.GetFirstPart(partParameter) ?? AddBodyPart(body);
			if (Part != null)
			{
				RegisterSlot(partParameter, Part);
			}
		}
		if (Part != null)
		{
			Part.Description = Blueprint.GetTag("PartDescription", "Worn around Wings");
			Part.DescriptionPrefix = null;
			Part.DefaultBehavior = GameObject.Create(Blueprint);
			Part.DefaultBehavior.SetStringProperty("TemporaryDefaultBehavior", "Wings");
		}
	}

	public BodyPart AddBodyPart(Body Body)
	{
		BodyPart body = Body.GetBody();
		string partParameter = Blueprint.GetPartParameter("Armor", "WornOn", "Back");
		int? category = body.Category;
		string managerID = ManagerID;
		string tag = Blueprint.GetTag("InsertPartAfter", "Head");
		string[] orInsertBefore = new string[3] { "Arm", "Missile Weapon", "Hands" };
		return body.AddPartAt(partParameter, 0, null, null, null, null, managerID, category, null, null, null, null, null, null, null, null, null, null, null, null, tag, orInsertBefore);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		if (appliedChargeBonus > 0)
		{
			ParentObject.ModIntProperty("ChargeRangeModifier", -appliedChargeBonus);
		}
		if (appliedJumpBonus > 0)
		{
			ParentObject.ModIntProperty("JumpRangeModifier", -appliedJumpBonus);
		}
		appliedChargeBonus = GetChargeDistanceBonus(NewLevel);
		appliedJumpBonus = GetJumpDistanceBonus(NewLevel);
		ParentObject.ModIntProperty("ChargeRangeModifier", appliedChargeBonus);
		ParentObject.ModIntProperty("JumpRangeModifier", appliedJumpBonus);
		Acrobatics_Jump.SyncAbility(ParentObject);
		return base.ChangeLevel(NewLevel);
	}

	public override void SetVariant(string Variant)
	{
		base.SetVariant(Variant);
		_Blueprint = null;
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		Flight.AbilitySetup(GO, GO, this);
		Acrobatics_Jump.SyncAbility(GO, Silent: true);
		GO.WantToReequip();
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (FlightFlying)
		{
			Flight.FailFlying(GO, GO, this);
		}
		GO.ModIntProperty("ChargeRangeModifier", -appliedChargeBonus, RemoveIfZero: true);
		GO.ModIntProperty("JumpRangeModifier", -appliedJumpBonus, RemoveIfZero: true);
		GO.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		appliedChargeBonus = 0;
		appliedJumpBonus = 0;
		Flight.AbilityTeardown(GO, GO, this);
		Acrobatics_Jump.SyncAbility(GO);
		GO.WantToReequip();
		return base.Unmutate(GO);
	}

	public void CheckEMP()
	{
		if (FlightFlying && ParentObject.IsEMPed() && MutationsSubjectToEMPEvent.Check(ParentObject))
		{
			Flight.FailFlying(ParentObject, ParentObject, this);
		}
	}
}
