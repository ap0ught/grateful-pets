using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: the effective PushLevel and PushDistance are
/// increased by the standard power load bonus, i.e. 2 for the standard
/// overload power load of 400, which means an additional 1000 lbs. of
/// push force and two cells of push distance, and reality stabilization
/// penetration is increased by ((power load - 100) / 10), i.e. 30 for
/// the standard overload power load of 400 (affecting only the capacity
/// of the device to activate within reality stabilization, not the
/// ability of emitted force fields to resist reality stabilization).
/// </remarks>
[Serializable]
public class ForceEmitter : IPoweredPart
{
	public static readonly string COMMAND_NAME = "ToggleForceEmitter";

	public string Blueprint = "Forcefield";

	public int PushLevel = 5;

	public int PushDistance = 4;

	public bool StartActive;

	public int RenewChance = 10;

	public Guid ActivatedAbilityID;

	[NonSerialized]
	public Dictionary<string, GameObject> CurrentField = new Dictionary<string, GameObject>(8);

	[NonSerialized]
	public static List<string> ToRemove = new List<string>();

	public ForceEmitter()
	{
		ChargeUse = 500;
		IsPowerLoadSensitive = true;
		IsRealityDistortionBased = true;
		WorksOnHolder = true;
		WorksOnWearer = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public string MyCommandName()
	{
		if (ActivatedAbilityID == Guid.Empty)
		{
			return COMMAND_NAME;
		}
		return (ParentObject.Equipped ?? ParentObject).GetActivatedAbility(ActivatedAbilityID)?.Command ?? COMMAND_NAME;
	}

	public void Validate()
	{
		if (CurrentField.Count <= 0)
		{
			return;
		}
		ToRemove.Clear();
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			if (item.Value == null || item.Value.IsInvalid())
			{
				ToRemove.Add(item.Key);
			}
		}
		foreach (string item2 in ToRemove)
		{
			CurrentField.Remove(item2);
		}
		if (CurrentField.Count <= 0)
		{
			SyncActivatedAbilityName();
		}
	}

	public bool IsActive()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			return CurrentField.Count > 0;
		}
		return false;
	}

	public bool IsSuspended()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count > 0)
			{
				foreach (GameObject value in CurrentField.Values)
				{
					if (value.CurrentCell != null)
					{
						return false;
					}
				}
				return true;
			}
		}
		return false;
	}

	public bool IsAnySuspended()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count > 0)
			{
				foreach (GameObject value in CurrentField.Values)
				{
					if (value.CurrentCell == null)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void DestroyBubble()
	{
		Validate();
		foreach (GameObject value in CurrentField.Values)
		{
			value.Obliterate();
		}
		CurrentField.Clear();
		SyncActivatedAbilityName();
	}

	public int CreateBubble(bool Renew = false)
	{
		Validate();
		int num = 0;
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject == null)
		{
			return num;
		}
		Cell cell = activePartFirstSubject.CurrentCell;
		if (cell == null)
		{
			return num;
		}
		if (cell.ParentZone.IsWorldMap())
		{
			return num;
		}
		if (Renew && RenewChance <= 0)
		{
			return num;
		}
		int powerLoad = MyPowerLoadLevel();
		int pushForce = GetPushForce(powerLoad);
		int pushDistance = GetPushDistance(powerLoad);
		string[] directionList = Directions.DirectionList;
		foreach (string text in directionList)
		{
			Cell cellFromDirection = cell.GetCellFromDirection(text, BuiltOnly: false);
			if (cellFromDirection == null)
			{
				continue;
			}
			if (CurrentField.ContainsKey(text))
			{
				GameObject gameObject = CurrentField[text];
				if (gameObject.CurrentCell == cellFromDirection)
				{
					continue;
				}
				gameObject.Obliterate();
				CurrentField.Remove(text);
			}
			if ((IsRealityDistortionBased && !IComponent<GameObject>.CheckRealityDistortionAccessibility(null, cellFromDirection, activePartFirstSubject, ParentObject)) || (Renew && !RenewChance.in100()))
			{
				continue;
			}
			GameObject gameObject2 = GameObject.Create(Blueprint);
			Forcefield part = gameObject2.GetPart<Forcefield>();
			if (part != null)
			{
				part.Creator = activePartFirstSubject;
				part.MovesWithOwner = true;
				part.RejectOwner = false;
			}
			ExistenceSupport existenceSupport = gameObject2.RequirePart<ExistenceSupport>();
			existenceSupport.SupportedBy = ParentObject;
			existenceSupport.ValidateEveryTurn = true;
			Phase.carryOver(activePartFirstSubject, gameObject2);
			cellFromDirection.AddObject(gameObject2);
			if (gameObject2.CurrentCell == cellFromDirection)
			{
				CurrentField[text] = gameObject2;
				num++;
				foreach (GameObject item in cellFromDirection.GetObjectsWithPartReadonly("Physics"))
				{
					if (item != gameObject2 && !item.HasPart<Forcefield>() && !item.HasPart<HologramMaterial>() && item.ConsiderSolidFor(gameObject2) && gameObject2.ConsiderSolidFor(item))
					{
						item.Push(text, pushForce, pushDistance);
					}
				}
				foreach (GameObject item2 in cellFromDirection.GetObjectsWithPartReadonly("Combat"))
				{
					if (item2 != gameObject2 && !item2.HasPart<Forcefield>() && !item2.HasPart<HologramMaterial>() && (part == null || !part.CanPass(item2)) && item2.PhaseMatches(gameObject2))
					{
						item2.Push(text, pushForce, pushDistance);
					}
				}
			}
			else
			{
				gameObject2.Obliterate();
			}
		}
		return num;
	}

	public int GetPushForce(int PowerLoad)
	{
		return 5000 + (PushLevel + MyPowerLoadBonus(PowerLoad)) * 500;
	}

	public int GetPushDistance(int PowerLoad)
	{
		return PushDistance + MyPowerLoadBonus(PowerLoad);
	}

	public void SuspendBubble()
	{
		Validate();
		foreach (GameObject value in CurrentField.Values)
		{
			value.RemoveFromContext();
		}
	}

	public void DesuspendBubble(bool Validated = false)
	{
		if (!Validated)
		{
			Validate();
		}
		Cell cell = GetActivePartFirstSubject()?.CurrentCell;
		if (cell == null || cell.ParentZone == null || cell.OnWorldMap())
		{
			DestroyBubble();
			return;
		}
		ToRemove.Clear();
		int powerLoad = MyPowerLoadLevel();
		int pushForce = GetPushForce(powerLoad);
		int pushDistance = GetPushDistance(powerLoad);
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			string key = item.Key;
			GameObject value = item.Value;
			if (value.CurrentCell != null)
			{
				continue;
			}
			Cell cellFromDirection = cell.GetCellFromDirection(key, BuiltOnly: false);
			if (cellFromDirection == null)
			{
				value.Obliterate();
				ToRemove.Add(key);
				continue;
			}
			cellFromDirection.AddObject(value);
			Forcefield part = value.GetPart<Forcefield>();
			if (value.CurrentCell == cellFromDirection)
			{
				foreach (GameObject item2 in cellFromDirection.GetObjectsWithPartReadonly("Physics"))
				{
					if (item2 != value && !item2.HasPart<Forcefield>() && !item2.HasPart<HologramMaterial>() && item2.ConsiderSolidFor(value) && value.ConsiderSolidFor(item2))
					{
						item2.Push(key, pushForce, pushDistance);
					}
				}
				foreach (GameObject item3 in cellFromDirection.GetObjectsWithPartReadonly("Combat"))
				{
					if (item3 != value && !item3.HasPart<Forcefield>() && !item3.HasPart<HologramMaterial>() && (part == null || !part.CanPass(item3)) && item3.PhaseMatches(value))
					{
						item3.Push(key, pushForce, pushDistance);
					}
				}
			}
			else
			{
				value.Obliterate();
				ToRemove.Add(key);
			}
		}
		foreach (string item4 in ToRemove)
		{
			CurrentField.Remove(item4);
		}
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(CurrentField.Count);
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			Writer.Write(item.Key);
			Writer.WriteGameObject(item.Value);
		}
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		CurrentField.Clear();
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadString();
			GameObject value = Reader.ReadGameObject("forcebubble");
			CurrentField[key] = value;
		}
		base.Read(Basis, Reader);
	}

	public void CollectStats(Templates.StatCollector stats)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveItemListEvent.ID && ID != PooledEvent<CheckExistenceSupportEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EquippedEvent.ID && ID != PooledEvent<ExamineSuccessEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetPartyLeaderFollowDistanceEvent>.ID && ID != PooledEvent<GetRealityStabilizationPenetrationEvent>.ID && ID != InventoryActionEvent.ID && ID != ObjectCreatedEvent.ID && ID != OnDestroyObjectEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineSuccessEvent E)
	{
		if (E.Object == ParentObject && E.Complete)
		{
			SetUpActivatedAbility(ParentObject.Equipped);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		if (!IsActive() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ForceFieldAdvisable() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !ParentObject.CurrentCell.AnyAdjacentCell(ContainsAlly))
		{
			E.Add(MyCommandName(), 1, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPartyLeaderFollowDistanceEvent E)
	{
		if (IsActive())
		{
			E.MinDistance(2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (WorksOnSelf)
		{
			SetUpActivatedAbility(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			DestroyBubble();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			DestroyBubble();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (CurrentField.ContainsValue(E.Object))
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null && cell.DistanceTo(E.Object) <= 1)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (IsActive())
		{
			if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				DestroyBubble();
			}
			else if (RenewChance > 0 && CurrentField.Count < 8)
			{
				CreateBubble(Renew: true);
			}
			else
			{
				MaintainBubble();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRealityStabilizationPenetrationEvent E)
	{
		E.Penetration += MyPowerLoadBonus(int.MinValue, 100, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "BeginMove");
		E.Actor.RegisterPartEvent(this, "EffectApplied");
		E.Actor.RegisterPartEvent(this, "EnteredCell");
		E.Actor.RegisterPartEvent(this, "MoveFailed");
		if (ParentObject.Understood())
		{
			SetUpActivatedAbility(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "BeginMove");
		E.Actor.UnregisterPartEvent(this, "EffectApplied");
		E.Actor.UnregisterPartEvent(this, "EnteredCell");
		E.Actor.UnregisterPartEvent(this, "MoveFailed");
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		DestroyBubble();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject != null && activePartFirstSubject.IsPlayer())
			{
				if (IsActive())
				{
					E.AddAction("Deactivate", "deactivate", MyCommandName(), null, 'a', FireOnActor: false, 10);
				}
				else
				{
					E.AddAction("Activate", "activate", MyCommandName(), null, 'a', FireOnActor: false, 10);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == MyCommandName() && ActivateForceEmitter(E))
		{
			E.RequestInterfaceExit();
			E.Actor.UseEnergy(1000, "Item Force Bracelet");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == MyCommandName() && ActivateForceEmitter(E))
		{
			E.RequestInterfaceExit();
			E.Actor.UseEnergy(1000, "Item Force Bracelet");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		DestroyBubble();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnterCell");
		if (WorksOnSelf)
		{
			Registrar.Register("BeginMove");
			Registrar.Register("EnteredCell");
			Registrar.Register("MoveFailed");
		}
		base.Register(Object, Registrar);
	}

	private bool ContainsAlly(Cell Cell)
	{
		int i = 0;
		for (int count = Cell.Objects.Count; i < count; i++)
		{
			if (Cell.Objects[i].IsCombatObject() && ParentObject.IsAlliedTowards(Cell.Objects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove")
		{
			SuspendBubble();
		}
		else if (E.ID == "EnteredCell" || E.ID == "MoveFailed")
		{
			DesuspendBubble();
		}
		else if (E.ID == "EffectApplied")
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				DestroyBubble();
			}
		}
		else if (E.ID == "EnterCell")
		{
			if (StartActive && !IsActive())
			{
				ActivateForceEmitter();
			}
			ParentObject.UnregisterPartEvent(this, "EnterCell");
		}
		return base.FireEvent(E);
	}

	private bool ForceFieldAdvisable()
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject == null)
		{
			return false;
		}
		return IComponent<GameObject>.CheckRealityDistortionAdvisability(activePartFirstSubject, null, activePartFirstSubject, ParentObject);
	}

	public bool ActivateForceEmitter(IEvent E = null)
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject == null)
		{
			return false;
		}
		Cell cell = activePartFirstSubject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (!cell.BroadcastEvent(Event.New("InitiateForceBubble", "Object", activePartFirstSubject, "Device", ParentObject, "Operator", activePartFirstSubject), E))
		{
			return false;
		}
		if (!cell.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", activePartFirstSubject, "Device", ParentObject, "Operator", activePartFirstSubject), E))
		{
			return false;
		}
		if (IsActive())
		{
			if (IsSuspended())
			{
				activePartFirstSubject.Fail(ParentObject.Does("vibrate") + " slightly.");
			}
			else if (activePartFirstSubject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("The {{B|force bubble}} snaps off.");
			}
			else if (IComponent<GameObject>.Visible(activePartFirstSubject))
			{
				IComponent<GameObject>.AddPlayerMessage("The {{B|force bubble}} around " + activePartFirstSubject.t() + " snaps off.");
			}
			DestroyBubble();
			return true;
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus == ActivePartStatus.Unpowered)
		{
			activePartFirstSubject.Fail(ParentObject.Does("don't") + " have enough charge to sustain the field!");
		}
		else
		{
			if (activePartStatus == ActivePartStatus.Operational && CreateBubble() > 0)
			{
				activePartFirstSubject.PlayWorldSound("Sounds/Interact/sfx_interact_forceProject_activate");
				if (activePartFirstSubject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("{{G|A {{B|force bubble}} pops into being around you.}}");
				}
				else if (IComponent<GameObject>.Visible(activePartFirstSubject))
				{
					IComponent<GameObject>.AddPlayerMessage("A {{B|force bubble}} pops into being around " + activePartFirstSubject.t() + ".");
				}
				SyncActivatedAbilityName(activePartFirstSubject);
				return true;
			}
			activePartFirstSubject.Fail("Nothing happens.");
		}
		return false;
	}

	public void SetUpActivatedAbility(GameObject Actor)
	{
		if (Actor != null)
		{
			string text = default;
			ActivatedAbilityID = Actor.AddDynamicCommand(out text, Name: GetActivatedAbilityName(Actor), CommandForDescription: COMMAND_NAME, Class: (Actor == ParentObject) ? "Maneuvers" : "Items", Description: null, Icon: "è");
		}
	}

	public string GetActivatedAbilityName(GameObject Actor = null)
	{
		if (Actor == null)
		{
			Actor = ParentObject.Equipped ?? ParentObject;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(IsActive() ? "Deactivate" : "Activate").Append(' ').Append((Actor == null || Actor == ParentObject) ? "Force Emitter" : Grammar.MakeTitleCase(ParentObject.BaseDisplayNameStripped));
		return stringBuilder.ToString();
	}

	public void SyncActivatedAbilityName(GameObject Actor = null)
	{
		if (!(ActivatedAbilityID == Guid.Empty))
		{
			if (Actor == null)
			{
				Actor = ParentObject.Equipped ?? ParentObject;
			}
			Actor.SetActivatedAbilityDisplayName(ActivatedAbilityID, GetActivatedAbilityName(Actor));
		}
	}

	public void MaintainBubble()
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject == null)
		{
			return;
		}
		Phase.syncPrep(activePartFirstSubject, out var FX, out var FX2);
		foreach (GameObject value in CurrentField.Values)
		{
			Phase.sync(activePartFirstSubject, value, FX, FX2);
		}
	}
}
