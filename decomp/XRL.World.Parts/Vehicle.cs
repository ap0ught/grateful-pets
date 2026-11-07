using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Vehicle : IActivePart
{
	public const int FLAG_AUTONOMOUS = 1;

	public const int FLAG_FOLLOWER = 2;

	public const int FLAG_TRACKER = 4;

	public const int FLAG_INHERIT_PARTY = 8;

	public const int FLAG_EXIT_ABILITY = 16;

	public const string ABL_CMD = "CommandReleaseVehicle";

	public string Type;

	public string BindBlueprint;

	public string OwnerID;

	public string PilotID;

	public Guid AbilityID;

	public int Passengers = 2;

	public int Flags = 16;

	[NonSerialized]
	private Interior _Interior;

	[NonSerialized]
	private bool Failed;

	[NonSerialized]
	private GameObject _Pilot;

	[NonSerialized]
	private VehicleRecord _Record;

	public bool Autonomous
	{
		get
		{
			return Flags.HasBit(1);
		}
		set
		{
			Flags.SetBit(1, value);
		}
	}

	public bool Follower
	{
		get
		{
			return Flags.HasBit(2);
		}
		set
		{
			Flags.SetBit(2, value);
		}
	}

	public bool Tracker
	{
		get
		{
			return Flags.HasBit(4);
		}
		set
		{
			Flags.SetBit(4, value);
		}
	}

	public bool InheritParty
	{
		get
		{
			return Flags.HasBit(8);
		}
		set
		{
			Flags.SetBit(8, value);
		}
	}

	public bool ExitAbility
	{
		get
		{
			return Flags.HasBit(16);
		}
		set
		{
			Flags.SetBit(16, value);
		}
	}

	public Interior Interior => _Interior ?? (_Interior = ParentObject.GetPart<Interior>());

	public GameObject Pilot
	{
		get
		{
			if (!Failed && !GameObject.Validate(ref _Pilot) && !PilotID.IsNullOrEmpty())
			{
				if (_Pilot == null)
				{
					_Pilot = FindObjectByIdEvent.Find(ParentObject, PilotID);
				}
				if (_Pilot == null)
				{
					_Pilot = The.ZoneManager.peekCachedObject(PilotID);
				}
				if (_Pilot == null && Interior != null)
				{
					_Pilot = Interior.Zone?.FindObjectByID(PilotID);
				}
				if (_Pilot == null)
				{
					_Pilot = The.ZoneManager.FindObjectByID(PilotID);
				}
				if (_Pilot != null)
				{
					if (!_Pilot.TryGetEffect<Piloting>(out var Effect) || Effect.Vehicle != ParentObject)
					{
						_Pilot = null;
						PilotID = null;
						CheckPenalty(null);
					}
				}
				else
				{
					Failed = true;
				}
			}
			return _Pilot;
		}
		set
		{
			GameObject gameObject = Pilot;
			if (gameObject == value || (value != null && !CanBePilotedBy(value)) || !BeforePilotChangeEvent.Check(ParentObject, value, gameObject))
			{
				return;
			}
			try
			{
				if (InheritParty)
				{
					Zone currentZone = ParentObject.CurrentZone;
					if (currentZone != null)
					{
						Zone.ObjectEnumerator enumerator = currentZone.IterateObjects().GetEnumerator();
						while (enumerator.MoveNext())
						{
							GameObject current = enumerator.Current;
							if (!current.IsCombatObject() || current == ParentObject)
							{
								continue;
							}
							GameObject partyLeader = current.PartyLeader;
							if (partyLeader != null)
							{
								if (partyLeader == ParentObject)
								{
									current.PartyLeader = gameObject;
								}
								else if (partyLeader == value)
								{
									current.PartyLeader = ParentObject;
								}
							}
						}
					}
				}
				if (ParentObject.IsPlayer())
				{
					if (gameObject == null)
					{
						gameObject = The.ZoneManager.FindObjectByID("OriginalPlayer");
						if (gameObject == null)
						{
							return;
						}
					}
					The.Game.Player.Body = gameObject;
					if (!Follower)
					{
					}
				}
				else if (value != null && value.IsPlayer())
				{
					The.Game.Player.Body = ParentObject;
					The.Player.MakeActive();
					CheckAbility();
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("VehiclePilotEffects", x);
			}
			gameObject?.RemoveEffect(typeof(Piloting));
			value?.ApplyEffect(new Piloting(ParentObject));
			_Pilot = value;
			PilotID = value?.ID;
			Failed = false;
			CheckPenalty(value);
			AfterPilotChangeEvent.Send(ParentObject, value, gameObject);
		}
	}

	public VehicleRecord Record
	{
		get
		{
			if (!Tracker)
			{
				return null;
			}
			if (_Record == null && !VehicleRecord.All.TryGetValue(ParentObject.ID, out _Record) && !ParentObject.IsTemporary)
			{
				Dictionary<string, VehicleRecord> all = VehicleRecord.All;
				string iD = ParentObject.ID;
				VehicleRecord vehicleRecord = new VehicleRecord
				{
					ID = ParentObject.ID,
					OwnerID = OwnerID,
					Blueprint = ParentObject.Blueprint,
					Location = (ParentObject.CurrentCell?.GetGlobalLocation() ?? new GlobalLocation()),
					Type = Type
				};
				VehicleRecord value = vehicleRecord;
				_Record = vehicleRecord;
				all[iD] = value;
			}
			return _Record;
		}
	}

	[Obsolete("Use PilotID")]
	public string DriverID
	{
		get
		{
			return PilotID;
		}
		set
		{
			PilotID = value;
		}
	}

	[Obsolete("Use Pilot")]
	public GameObject Driver
	{
		get
		{
			return Pilot;
		}
		set
		{
			Pilot = value;
		}
	}

	public Vehicle()
	{
		WorksOnSelf = true;
		NameForStatus = "PrimeMover";
	}

	public override void Initialize()
	{
		CheckPenalty(Pilot);
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		Vehicle vehicle = (Vehicle)base.DeepCopy(Parent);
		vehicle.PilotID = null;
		vehicle.Tracker = false;
		return vehicle;
	}

	public virtual bool IsOperational()
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return IsVehicleOperationalEvent.Check(ParentObject);
		}
		return false;
	}

	public bool CanBePilotedBy(GameObject Actor)
	{
		if (Actor.Brain == null)
		{
			return false;
		}
		if (OwnerID.IsNullOrEmpty() && BindBlueprint.IsNullOrEmpty())
		{
			return true;
		}
		return IsOwnedBy(Actor);
	}

	public bool IsOwnedBy(GameObject Actor)
	{
		if (Actor.IsPlayer())
		{
			return OwnerID == "Player";
		}
		return Actor.IDMatch(OwnerID);
	}

	public void CheckAbility()
	{
		if (ExitAbility && AbilityID == Guid.Empty)
		{
			AbilityID = ParentObject.AddActivatedAbility("Exit pilot seat", "CommandReleaseVehicle", "Vehicle", null, "÷");
		}
	}

	public void CheckPenalty(GameObject Actor)
	{
		if (Actor != null)
		{
			ParentObject.RemoveEffect(typeof(Unpiloted));
		}
		else if (ParentObject.Brain != null && !ParentObject.HasEffect(typeof(Unpiloted)))
		{
			ParentObject.ApplyEffect(new Unpiloted(100, Autonomous));
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandReleaseVehicle");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandReleaseVehicle")
		{
			Pilot = null;
		}
		return base.FireEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != AfterDieEvent.ID && ID != ObjectCreatedEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && (!Tracker || ID != WasReplicatedEvent.ID) && (!Tracker || ID != BeforeDestroyObjectEvent.ID))
		{
			if (Tracker)
			{
				return ID == EnteredCellEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.IsPlayer())
		{
			E.AddAction("Exit", "exit", "ReleaseVehicle", null, 'x');
		}
		if (ParentObject.IsPlayerControlled())
		{
			E.AddAction("CompanionEnterVehicle", "direct follower to enter", "CompanionEnterVehicle", null, 'f', FireOnActor: false, 1000, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public bool IsValidPassenger(GameObject Object)
	{
		if (Object.PartyLeader == The.Player && Object != ParentObject && Object.IsMobile())
		{
			return !Object.HasPart<Vehicle>();
		}
		return false;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ReleaseVehicle")
		{
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.Fail("You cannot do that on the world map.");
			}
			Pilot = null;
			E.RequestInterfaceExit();
		}
		else if (E.Command == "CompanionEnterVehicle")
		{
			List<GameObject> list = Event.NewGameObjectList(The.ActiveZone.YieldObjects().Where(IsValidPassenger));
			if (list.Count == 0)
			{
				return E.Actor.Fail("You have no followers that can enter " + ParentObject.t() + ".");
			}
			GameObject gameObject = Popup.PickGameObject("Choose a follower", list, AllowEscape: true);
			if (gameObject == null)
			{
				return false;
			}
			if (Interior != null && !Interior.CanEnter(gameObject, Action: false, ShowMessage: true))
			{
				return false;
			}
			gameObject.Brain.Goals.Clear();
			gameObject.Brain.Staying = true;
			AIPassenger aIPassenger = new AIPassenger(ParentObject);
			gameObject.AddPart(aIPassenger);
			aIPassenger.CheckPassengerSeat();
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterDieEvent E)
	{
		ParentObject.PullDown();
		GameObject pilot = Pilot;
		if (pilot != null)
		{
			Pilot = null;
			if (pilot.CurrentCell == null && pilot.HasHitpoints())
			{
				pilot.SystemMoveTo(ParentObject.CurrentCell, 0, forced: true);
				StairsDown.InflictFallDamage(pilot, Interior?.FallDistance ?? 0);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		if (Tracker && !E.Obliterate && !ParentObject.IsPlayer())
		{
			ParentObject.Physics.TeardownForDestroy(MoveToGraveyard: false, Silent: true);
			The.ZoneManager.CachedObjects[ParentObject.ID] = ParentObject;
			foreach (Effect effect in ParentObject.Effects)
			{
				if (effect.IsOfType(67108864))
				{
					effect.Duration = 0;
				}
			}
			ParentObject.CleanEffects();
			ParentObject.ApplyEffect(new Invulnerable
			{
				Duration = 3
			});
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		switch (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
		case ActivePartStatus.Unpowered:
			if (!ParentObject.HasEffect(typeof(VehicleUnpowered)))
			{
				ParentObject.ApplyEffect(new VehicleUnpowered(Math.Max(ChargeUse, ChargeMinimum)));
			}
			break;
		default:
			return false;
		case ActivePartStatus.Operational:
			break;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		CheckPenalty(Pilot);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Tracker)
		{
			Record?.Location.SetCell(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WasReplicatedEvent E)
	{
		if (Tracker && !E.Temporary && E.Replica.TryGetPart<Vehicle>(out var Part))
		{
			Part.Tracker = true;
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ConsumeChargeIfOperational();
	}

	public static GameObject CreateOwnedBy(string Blueprint, GameObject Owner, bool? Follower = null, bool? Tracker = null, bool Clear = true)
	{
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(Blueprint);
		Vehicle part = gameObject.GetPart<Vehicle>();
		part.OwnerID = (Owner.IsPlayer() ? "Player" : Owner.ID);
		if (Follower.HasValue)
		{
			part.Follower = Follower.Value;
		}
		if (Tracker.HasValue)
		{
			part.Tracker = Tracker.Value;
		}
		if (Clear)
		{
			gameObject.Inventory.Clear();
		}
		return gameObject;
	}
}
