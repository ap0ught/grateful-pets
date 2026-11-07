using System;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCathedra : IPart, IFlightSource
{
	public const string ABL_CMD = "CommandActivateCathedra";

	public const string ABL_CLASS = "Cybernetics";

	public int BaseLevel = 10;

	public int BaseCooldown = 100;

	public Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private GameObject _User;

	public int FlightLevel => GetLevel();

	public int FlightBaseFallChance => 6;

	public bool FlightRequiresOngoingEffort => true;

	public string FlightEvent => "CommandActivateCathedraFlight";

	public string FlightActivatedAbilityClass => "Cybernetics";

	public string FlightSourceDescription => "Cathedra";

	public bool FlightFlying { get; set; }

	public Guid FlightActivatedAbilityID { get; set; }

	public GameObject User => _User ?? (_User = ParentObject.Implantee);

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
		Writer.Write(FlightFlying);
		Writer.Write(FlightActivatedAbilityID);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		FlightFlying = Reader.ReadBoolean();
		FlightActivatedAbilityID = Reader.ReadGuid();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public virtual void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID, User), GetCooldown());
		stats.Set("CrashChance", Flight.GetMoveFallChance(User, this));
		stats.Set("SwoopCrashChance", Flight.GetSwoopFallChance(User, this));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AttemptToLandEvent>.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID && ID != PooledEvent<BodyPositionChangedEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != PooledEvent<MovementModeChangedEvent>.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.Type == "Inspect" || E.Type == "Disassemble")
		{
			int num = 5;
			int num2 = Math.Min(2 + num, 4);
			if (num2 != 0)
			{
				E.Bonus += num2;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, User);
		DescribeMyActivatedAbility(FlightActivatedAbilityID, CollectStats, User);
		ActivatedAbilityEntry activatedAbilityEntry = User?.GetActivatedAbilityByCommand(Flight.SWOOP_ATTACK_COMMAND_NAME);
		if (activatedAbilityEntry != null)
		{
			DescribeMyActivatedAbility(activatedAbilityEntry.ID, CollectStats, User);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		OnImplanted(E.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		OnUnimplanted(E.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandActivateCathedra")
		{
			if (!E.Actor.OnWorldMap())
			{
				Activate(E.Actor);
				ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
			}
		}
		else if (E.Command == FlightEvent)
		{
			if (E.Actor.IsActivatedAbilityToggledOn(FlightActivatedAbilityID))
			{
				if (E.Actor.IsPlayer() && E.Actor.CurrentCell != null && E.Actor.GetEffectCount(typeof(Flying)) <= 1)
				{
					foreach (GameObject @object in E.Actor.CurrentCell.Objects)
					{
						if (@object.TryGetPart<StairsDown>(out var Part) && Part.IsLongFall() && Popup.WarnYesNo("It looks like a long way down " + @object.the + @object.ShortDisplayName + " you're above. Are you sure you want to stop flying?") != DialogResult.Yes)
						{
							return false;
						}
					}
				}
				Flight.StopFlying(ParentObject, E.Actor, this);
			}
			else
			{
				Flight.StartFlying(ParentObject, E.Actor, this);
			}
			ParentObject.ModIntProperty("ActivatedAbilityCommandsProcessed", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (FlightFlying)
		{
			Flight.MaintainFlight(ParentObject, User, this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BodyPositionChangedEvent E)
	{
		if (FlightFlying && E.To != "Flying")
		{
			if (E.Involuntary)
			{
				Flight.FailFlying(ParentObject, E.Object, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, E.Object, this);
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
				Flight.FailFlying(ParentObject, E.Object, this);
			}
			else
			{
				Flight.StopFlying(ParentObject, E.Object, this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AttemptToLandEvent E)
	{
		if (FlightFlying && Flight.StopFlying(ParentObject, E.Actor, this))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		E.Weight += 100.0;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Flight.CheckFlight(ParentObject, E.Object, this);
		return base.HandleEvent(E);
	}

	public virtual void OnImplanted(GameObject Object)
	{
		_User = Object;
		RegisterImplantee(Object, EventRegistrar.Get(ParentObject, this));
		Flight.AbilitySetup(ParentObject, Object, this);
		CarryingCapacityChangedEvent.Send(Object);
	}

	public virtual void OnUnimplanted(GameObject Object)
	{
		_User = null;
		RegisterImplantee(Object, EventUnregistrar.Get(ParentObject, this));
		Object.RemoveActivatedAbility(ref ActivatedAbilityID);
		Flight.FailFlying(ParentObject, Object, this);
		Flight.AbilityTeardown(ParentObject, Object, this);
		CarryingCapacityChangedEvent.Send(Object);
	}

	public virtual void RegisterImplantee(GameObject Implantee, IEventRegistrar Registrar)
	{
		Registrar.Register(Implantee, EnteredCellEvent.ID);
		Registrar.Register(Implantee, GetMaxCarriedWeightEvent.ID);
		Registrar.Register(Implantee, GetTinkeringBonusEvent.ID);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
		GameObject implantee = ParentObject.Implantee;
		if (implantee.IsValid())
		{
			RegisterImplantee(implantee, Registrar);
		}
	}

	public virtual void Activate(GameObject Actor)
	{
		Actor.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown(Actor));
	}

	public int GetLevel(GameObject Actor = null)
	{
		return GetAvailableComputePowerEvent.AdjustUp(Actor ?? User, BaseLevel);
	}

	public int GetCooldown(GameObject Actor = null)
	{
		return GetAvailableComputePowerEvent.AdjustDown(Actor ?? User, BaseCooldown);
	}
}
