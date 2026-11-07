using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Stopsvaalinn : IPoweredPart
{
	public string Blueprint = "Forcefield";

	public int PushLevel = 5;

	public int RenewChance = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public string FieldDirection;

	public bool RobotStopApplied;

	[NonSerialized]
	public Dictionary<string, GameObject> CurrentField = new Dictionary<string, GameObject>(3);

	[NonSerialized]
	public static List<string> toRemove = new List<string>();

	public Stopsvaalinn()
	{
		ChargeUse = 500;
		IsRealityDistortionBased = true;
		WorksOnWearer = true;
		WorksOnEquipper = true;
		NameForStatus = "ForceEmitter";
	}

	private void ApplyRobotStop(GameObject Subject = null)
	{
		if (!RobotStopApplied)
		{
			if (Subject == null)
			{
				Subject = GetActivePartFirstSubject();
			}
			if (Subject != null)
			{
				Subject.SetStringProperty("RobotStop", "true");
				RobotStopApplied = true;
			}
		}
	}

	private void UnapplyRobotStop(GameObject Subject = null)
	{
		if (RobotStopApplied)
		{
			if (Subject == null)
			{
				Subject = GetActivePartFirstSubject();
			}
			if (Subject != null)
			{
				Subject.RemoveStringProperty("RobotStop");
				RobotStopApplied = false;
			}
		}
	}

	public void Validate()
	{
		toRemove.Clear();
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			if (item.Value == null || item.Value.IsInvalid())
			{
				toRemove.Add(item.Key);
			}
		}
		if (toRemove.Count <= 0)
		{
			return;
		}
		foreach (string item2 in toRemove)
		{
			CurrentField.Remove(item2);
		}
		if (CurrentField.Count == 0)
		{
			UnapplyRobotStop();
		}
	}

	public bool IsActive()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count > 0)
			{
				return true;
			}
		}
		UnapplyRobotStop();
		return false;
	}

	public bool IsSuspended()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count <= 0)
			{
				return false;
			}
			foreach (KeyValuePair<string, GameObject> item in CurrentField)
			{
				if (item.Value.CurrentCell != null)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool IsAnySuspended()
	{
		if (CurrentField.Count > 0)
		{
			Validate();
			if (CurrentField.Count <= 0)
			{
				return false;
			}
			foreach (GameObject value in CurrentField.Values)
			{
				if (value.CurrentCell == null)
				{
					return true;
				}
			}
			return false;
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
		UnapplyRobotStop();
		FieldDirection = null;
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
		if (FieldDirection.IsNullOrEmpty() || (CurrentField.Count == 0 && !Renew))
		{
			FieldDirection = PickDirectionS("Activate Stopsvaalinn");
			if (FieldDirection.IsNullOrEmpty())
			{
				return num;
			}
			if (FieldDirection == ".")
			{
				FieldDirection = Directions.GetRandomDirection();
			}
		}
		foreach (KeyValuePair<string, Cell> item in cell.GetAdjacentDirectionCellMap(FieldDirection, BuiltOnly: false))
		{
			string key = item.Key;
			Cell value = item.Value;
			if (value == null || value == cell)
			{
				continue;
			}
			if (CurrentField.ContainsKey(key))
			{
				GameObject gameObject = CurrentField[key];
				if (gameObject.CurrentCell == value)
				{
					continue;
				}
				gameObject.Obliterate();
				CurrentField.Remove(key);
			}
			if ((IsRealityDistortionBased && !IComponent<GameObject>.CheckRealityDistortionAccessibility(value)) || (Renew && !RenewChance.in100()))
			{
				continue;
			}
			GameObject gameObject2 = GameObject.Create(Blueprint);
			Forcefield part = gameObject2.GetPart<Forcefield>();
			part.Creator = activePartFirstSubject;
			part.MovesWithOwner = true;
			part.RejectOwner = false;
			gameObject2.RequirePart<ExistenceSupport>().SupportedBy = ParentObject;
			Phase.carryOver(activePartFirstSubject, gameObject2);
			AnimatedMaterialForcefield part2 = gameObject2.GetPart<AnimatedMaterialForcefield>();
			part2.Color = "Red";
			gameObject2.SetStringProperty("RobotStop", "true");
			value.AddObject(gameObject2);
			if (gameObject2.CurrentCell == value)
			{
				CurrentField.Add(key, gameObject2);
				num++;
				foreach (GameObject item2 in value.GetObjectsWithPart("Physics"))
				{
					if (item2 != gameObject2 && item2 != activePartFirstSubject && item2.ConsiderSolidFor(gameObject2) && (part == null || !part.CanPass(item2)) && !item2.HasPart<Forcefield>() && !item2.HasPart<HologramMaterial>() && item2.PhaseMatches(gameObject2))
					{
						item2.Physics.Push(key, 5000 + 500 * PushLevel, 4);
					}
				}
				foreach (GameObject item3 in value.GetObjectsWithPart("Combat"))
				{
					if (item3 != gameObject2 && item3 != activePartFirstSubject && item3.Physics != null && (part == null || !part.CanPass(item3)) && !item3.HasPart<HologramMaterial>() && item3.PhaseMatches(gameObject2))
					{
						item3.Physics.Push(key, 5000 + 500 * PushLevel, 4);
					}
				}
			}
			else
			{
				gameObject2.Obliterate();
			}
		}
		if (num > 0)
		{
			ApplyRobotStop(activePartFirstSubject);
		}
		return num;
	}

	public void SuspendBubble()
	{
		Validate();
		foreach (KeyValuePair<string, GameObject> item in CurrentField)
		{
			item.Value.RemoveFromContext();
		}
	}

	public void DesuspendBubble(bool Validated = false)
	{
		if (!Validated)
		{
			Validate();
		}
		Cell cell = GetActivePartFirstSubject()?.CurrentCell;
		if (cell == null || cell.ParentZone == null || cell.ParentZone.IsWorldMap())
		{
			DestroyBubble();
			return;
		}
		toRemove.Clear();
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
				toRemove.Add(key);
				continue;
			}
			cellFromDirection.AddObject(value);
			Forcefield part = value.GetPart<Forcefield>();
			if (value.CurrentCell == cellFromDirection)
			{
				foreach (GameObject item2 in cellFromDirection.GetObjectsWithPart("Physics"))
				{
					if (item2 != value && item2.Physics.Solid && (part == null || !part.CanPass(item2)) && !item2.HasPart<Forcefield>() && !item2.HasPart<HologramMaterial>() && item2.PhaseMatches(value))
					{
						item2.Physics.Push(key, 5000 + 500 * PushLevel, 4);
					}
				}
				foreach (GameObject item3 in cellFromDirection.GetObjectsWithPart("Combat"))
				{
					if (item3 != value && item3.Physics != null && (part == null || !part.CanPass(item3)) && !item3.HasPart<HologramMaterial>() && item3.PhaseMatches(value))
					{
						item3.Physics.Push(key, 5000 + 500 * PushLevel, 4);
					}
				}
			}
			else
			{
				value.Obliterate();
				toRemove.Add(key);
			}
		}
		foreach (string item4 in toRemove)
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
			GameObject value = Reader.ReadGameObject("stopsvalinn");
			CurrentField.Add(key, value);
		}
		base.Read(Basis, Reader);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Duration", "3");
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveItemListEvent.ID && ID != PooledEvent<CheckExistenceSupportEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != EffectAppliedEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EquippedEvent.ID && ID != PooledEvent<ExamineSuccessEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != OnDestroyObjectEvent.ID && ID != UnequippedEvent.ID)
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
		if (E.Object == ParentObject)
		{
			CheckApplyPowers(ParentObject.EquippedOn());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetDefensiveItemListEvent E)
	{
		if (!IsActive() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ForceFieldAdvisable() && E.Actor == ParentObject.Equipped && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("ActivateStopsvalinn", 1, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "ActivateStopsvalinn" && E.Actor == ParentObject.Equipped && ActivateStopsvalinn(E))
		{
			E.RequestInterfaceExit();
			E.Actor?.UseEnergy(1000, "Item Stopsvalinn");
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

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (CurrentField.ContainsValue(E.Object))
		{
			return false;
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

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApplyPowers(E.Part);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		UnapplyPowers(E.Actor);
		return base.HandleEvent(E);
	}

	private void CheckApplyPowers(BodyPart Limb)
	{
		if (ActivatedAbilityID == Guid.Empty && Limb != null && ParentObject.IsEquippedProperly(Limb))
		{
			GameObject parentObject = Limb.ParentBody.ParentObject;
			if (parentObject != null && (!parentObject.IsPlayer() || ParentObject.Understood()))
			{
				ApplyPowers(parentObject);
			}
		}
	}

	private void ApplyPowers(GameObject who)
	{
		StatShifter.SetStatShift(who, "Ego", 1);
		who.RegisterPartEvent(this, "BeginMove");
		who.RegisterPartEvent(this, "EnteredCell");
		who.RegisterPartEvent(this, "MoveFailed");
		ActivatedAbilityID = who.AddActivatedAbility("Activate Stopsvalinn", "ActivateStopsvalinn", "Items", null, "é", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased);
	}

	private void CheckUnapplyPowers(GameObject who = null)
	{
		if (ActivatedAbilityID != Guid.Empty)
		{
			UnapplyPowers(who);
		}
	}

	private void UnapplyPowers(GameObject who)
	{
		StatShifter.RemoveStatShifts(who);
		who.UnregisterPartEvent(this, "BeginMove");
		who.UnregisterPartEvent(this, "EnteredCell");
		who.UnregisterPartEvent(this, "MoveFailed");
		who.RemoveActivatedAbility(ref ActivatedAbilityID);
		DestroyBubble();
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood() && IsObjectActivePartSubject(E.Actor))
		{
			if (IsActive())
			{
				E.AddAction("Deactivate", "deactivate", "ActivateStopsvalinn", null, 'a');
			}
			else
			{
				E.AddAction("Activate", "activate", "ActivateStopsvalinn", null, 'a');
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateStopsvalinn" && ActivateStopsvalinn(E))
		{
			E.RequestInterfaceExit();
			E.Actor.UseEnergy(1000, "Item Stopsvalinn");
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

	public bool ActivateStopsvalinn(IEvent E = null)
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
		if (!cell.BroadcastEvent(Event.New("InitiateForceBubble", "Object", activePartFirstSubject, "Device", ParentObject), E))
		{
			return false;
		}
		if (!cell.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", activePartFirstSubject, "Device", ParentObject), E))
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
				IComponent<GameObject>.AddPlayerMessage("The {{R|force bubble}} snaps off.");
			}
			else if (IComponent<GameObject>.Visible(activePartFirstSubject))
			{
				IComponent<GameObject>.AddPlayerMessage("The {{R|force bubble}} in front of " + activePartFirstSubject.t() + " snaps off.");
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
				activePartFirstSubject.PlayWorldSound("Sounds/Interact/sfx_interact_legendaryForcefieldShield_activate");
				if (activePartFirstSubject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("A {{R|force bubble}} pops into being in front of you!");
				}
				else if (IComponent<GameObject>.Visible(activePartFirstSubject))
				{
					IComponent<GameObject>.AddPlayerMessage("A {{R|force bubble}} pops into being in front of " + activePartFirstSubject.the + activePartFirstSubject.ShortDisplayName + ".");
				}
				return true;
			}
			activePartFirstSubject.Fail("Nothing happens.");
		}
		return false;
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
