using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <summary>
///     The socket that holds the energy cell powering a device.
/// </summary>
[Serializable]
public class EnergyCellSocket : IPoweredPart
{
	public static readonly string REPLACE_CELL_INTERACTION = "ReplaceSocketCell";

	public string SlotType = "EnergyCell";

	public GameObject Cell;

	/// <summary>
	///     Chance in 100 to be slotted when created.
	/// </summary>
	public int ChanceSlotted = 50;

	/// <summary>
	///     If <see cref="F:XRL.World.Parts.EnergyCellSocket.ChanceSlotted" />, the cell that will be created and inserted into the socket.
	///     Uses <see cref="M:XRL.World.GameObjectFactory.ProcessSpecification(System.String,System.Action{XRL.World.GameObject},XRL.World.InventoryObject,System.Int32,System.Int32,System.Int32,System.String,System.String,System.Action{XRL.World.GameObject},XRL.World.GameObject,XRL.World.Parts.Inventory,System.Collections.Generic.List{XRL.World.GameObject})" /> format to create object.
	/// </summary>
	public string SlottedType = "#Chem Cell,Chem Cell,@DynamicObjectsTable:EnergyCells:Tier{zonetier}";

	/// <summary>
	///     Chance in 100 to fill the cell loaded via <see cref="M:XRL.World.Parts.IEnergyCell.MaximizeCharge" />
	/// </summary>
	public int ChanceFullCell = 10;

	public int ChanceDestroyCellOnForcedUnequip;

	public string CellStartChargePercentage;

	public string RemoveCellUnpoweredSound = "compartment_open";

	public string RemoveCellPoweredSound = "sfx_interact_artifact_windDown";

	public string SlotCellUnpoweredSound = "compartment_close";

	public string SlotCellPoweredSound = "sfx_interact_artifact_windup";

	public bool VisibleInDisplayName = true;

	public bool VisibleInDescription;

	public EnergyCellSocket()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		WorksOnSelf = true;
	}

	public override bool CanGenerateStacked()
	{
		if (ChanceSlotted != 0)
		{
			if (ChanceSlotted < 100)
			{
				return false;
			}
			if (SlottedType.Contains("#") || SlottedType.Contains("@") || SlottedType.Contains("*"))
			{
				return false;
			}
		}
		return base.CanGenerateStacked();
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		EnergyCellSocket energyCellSocket = new EnergyCellSocket();
		energyCellSocket.SlotType = SlotType;
		energyCellSocket.ChanceSlotted = ChanceSlotted;
		if (GameObject.Validate(ref Cell))
		{
			energyCellSocket.Cell = MapInv?.Invoke(Cell) ?? Cell.DeepCopy(CopyEffects: false, CopyID: false, MapInv);
			if (energyCellSocket.Cell != null)
			{
				energyCellSocket.Cell.ForeachPartDescendedFrom((IEnergyCell p) =>
				{
					p.SlottedIn = Parent;
				});
			}
		}
		energyCellSocket.SlottedType = SlottedType;
		energyCellSocket.ChanceFullCell = ChanceFullCell;
		energyCellSocket.ChanceDestroyCellOnForcedUnequip = ChanceDestroyCellOnForcedUnequip;
		energyCellSocket.RemoveCellUnpoweredSound = RemoveCellUnpoweredSound;
		energyCellSocket.RemoveCellPoweredSound = RemoveCellPoweredSound;
		energyCellSocket.SlotCellUnpoweredSound = SlotCellUnpoweredSound;
		energyCellSocket.SlotCellPoweredSound = SlotCellPoweredSound;
		energyCellSocket.ParentObject = Parent;
		return energyCellSocket;
	}

	public override bool SameAs(IPart Part)
	{
		EnergyCellSocket energyCellSocket = Part as EnergyCellSocket;
		if (energyCellSocket.SlotType != SlotType)
		{
			return false;
		}
		if (energyCellSocket.Cell != null || Cell != null)
		{
			return false;
		}
		if (energyCellSocket.ChanceDestroyCellOnForcedUnequip != ChanceDestroyCellOnForcedUnequip)
		{
			return false;
		}
		if (energyCellSocket.RemoveCellUnpoweredSound != RemoveCellUnpoweredSound)
		{
			return false;
		}
		if (energyCellSocket.RemoveCellPoweredSound != RemoveCellPoweredSound)
		{
			return false;
		}
		if (energyCellSocket.SlotCellUnpoweredSound != SlotCellUnpoweredSound)
		{
			return false;
		}
		if (energyCellSocket.SlotCellPoweredSound != SlotCellPoweredSound)
		{
			return false;
		}
		if (energyCellSocket.VisibleInDisplayName != VisibleInDisplayName)
		{
			return false;
		}
		if (energyCellSocket.VisibleInDescription != VisibleInDescription)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	private bool CellWantsEvent(int ID, int cascade)
	{
		if (!GameObject.Validate(Cell))
		{
			return false;
		}
		if (!MinEvent.CascadeTo(cascade, 4))
		{
			return false;
		}
		return Cell.WantEvent(ID, cascade);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeginBeingUnequippedEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetContentsEvent>.ID && (ID != PooledEvent<GetDisplayNameEvent>.ID || !VisibleInDisplayName) && (ID != GetShortDescriptionEvent.ID || !VisibleInDescription) && ID != GetExtrinsicValueEvent.ID && ID != GetExtrinsicWeightEvent.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetReplaceCellInteractionsEvent>.ID && ID != InventoryActionEvent.ID && (ID != ObjectCreatedEvent.ID || ChanceSlotted <= 0) && ID != PooledEvent<StripContentsEvent>.ID)
		{
			return CellWantsEvent(ID, cascade);
		}
		return true;
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(4) && GameObject.Validate(Cell) && !E.Dispatch(Cell))
		{
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetReplaceCellInteractionsEvent E)
	{
		E.Add(ParentObject, GetReplaceCellPriority(), REPLACE_CELL_INTERACTION);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginBeingUnequippedEvent E)
	{
		if (E.Forced && ChanceDestroyCellOnForcedUnequip > 0)
		{
			GameObject Object = Cell;
			if (GameObject.Validate(ref Object) && ChanceDestroyCellOnForcedUnequip.in100() && Object.FireEvent("CanForcedUnequipDestroy"))
			{
				SetCell(null);
				Object.Destroy();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (VisibleInDisplayName && !E.Reference && E.Understood())
		{
			if (!GameObject.Validate(ref Cell))
			{
				E.AddTag("{{y|[{{K|no cell}}]}}", -5);
			}
			else
			{
				E.AddTag("{{y|[" + Cell.DisplayName + "]}}", -5);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (VisibleInDescription && E.Understood() && GameObject.Validate(ref Cell) && Cell.TryGetPartDescendedFrom<IEnergyCell>(out var Part))
		{
			E.Postfix.Append("\n{{rules|").AppendCase(Cell.ShortDisplayNameStripped, Capitalize: true).Append(": ")
				.Append(Part.ChargeStatus())
				.Append("}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (GameObject.Validate(ref Cell) && E.CascadeTo(4) && !E.Dispatch(Cell))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (GameObject.Validate(ref Cell))
		{
			E.AddAction("Replace Cell", "replace cell", REPLACE_CELL_INTERACTION, "cell", 'c');
			GetSlottedInventoryActionsEvent.Send(Cell, E);
		}
		else
		{
			E.AddAction("Replace Cell", "install cell", REPLACE_CELL_INTERACTION, null, 'c', FireOnActor: false, 15);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == REPLACE_CELL_INTERACTION)
		{
			return AttemptReplaceCell(E.Actor, E, E.MinimumCharge, E.ObjectTarget);
		}
		if (E.Command == "EmptyForDisassemble" && GameObject.Validate(ref Cell))
		{
			ParentObject.GetContext(out var ObjectContext, out var CellContext);
			if (ObjectContext != null)
			{
				ObjectContext.ReceiveObject(Cell);
			}
			else if (CellContext != null)
			{
				CellContext.AddObject(Cell);
			}
			else
			{
				E.Generated.Add(Cell);
				E.Actor.ReceiveObject(Cell);
			}
			SetCell(null);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		if (GameObject.Validate(ref Cell))
		{
			E.Value += Cell.Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		if (GameObject.Validate(ref Cell))
		{
			E.Weight += Cell.GetWeight();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StripContentsEvent E)
	{
		if (GameObject.Validate(ref Cell) && (!E.KeepNatural || !Cell.IsNatural()))
		{
			GameObject cell = Cell;
			SetCell(null);
			cell.Obliterate();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContentsEvent E)
	{
		if (GameObject.Validate(ref Cell))
		{
			E.Objects.Add(Cell);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (ChanceSlotted.in100())
		{
			GameObjectFactory.ProcessSpecification(SlottedType, LoadCell);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("InductionCharge");
		Registrar.Register("QueryCharge");
		Registrar.Register("QueryChargeStorage");
		Registrar.Register("QueryRechargeStorage");
		Registrar.Register("RechargeAvailable");
		Registrar.Register("TestCharge");
		Registrar.Register("UseCharge");
		base.Register(Object, Registrar);
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (GameObject.Validate(ref Cell))
		{
			Cell.TurnTick(TimeTick, Amount);
		}
	}

	public void SetCell(GameObject Object)
	{
		if (Object == Cell)
		{
			return;
		}
		if (Cell != null)
		{
			Cell.ForeachPartDescendedFrom((IEnergyCell p) =>
			{
				p.SlottedIn = null;
			});
		}
		Cell = Object;
		Object?.ForeachPartDescendedFrom((IEnergyCell p) =>
		{
			p.SlottedIn = ParentObject;
		});
		if (Sidebar.CurrentTarget == Object)
		{
			Sidebar.CurrentTarget = null;
		}
		FlushTransientCaches();
	}

	private void LoadCell(GameObject Object)
	{
		if (Object == null)
		{
			Debug.LogError("Unknown cell type: " + SlottedType);
			return;
		}
		IEnergyCell partDescendedFrom = Object.GetPartDescendedFrom<IEnergyCell>();
		if (partDescendedFrom == null)
		{
			Debug.LogError("Cell type has no IEnergyCell part: " + Object.Blueprint);
			return;
		}
		if (Cell != null)
		{
			Debug.LogError("Multiple cells generated: " + Cell.Blueprint + ", " + Object.Blueprint);
			return;
		}
		SetCell(Object);
		if (ChanceFullCell.in100())
		{
			partDescendedFrom.MaximizeCharge();
		}
		else if (!CellStartChargePercentage.IsNullOrEmpty())
		{
			partDescendedFrom.SetChargePercentage(CellStartChargePercentage.RollCached());
		}
		else
		{
			partDescendedFrom.RandomizeCharge();
		}
	}

	public bool AttemptRemoveCell(GameObject Owner, InventoryActionEvent E, bool CheckStack = true)
	{
		GameObject cell = Cell;
		bool flag = cell.GetIntProperty("NeverStack") > 0;
		if (!flag)
		{
			cell.SetIntProperty("NeverStack", 1);
		}
		try
		{
			Event obj = Event.New("BeforeRemoveCell", "Object", cell);
			bool flag2 = ParentObject.FireEvent(obj);
			obj.ID = "CommandTakeObject";
			if (flag2 && E.OverrideEnergyCost)
			{
				obj.SetParameter("EnergyCost", E.EnergyCostOverride);
			}
			if (!flag2 || Owner.Inventory == null || !Owner.Inventory.FireEvent(obj))
			{
				Owner.Fail("You can't remove " + Cell.t() + "!");
				return false;
			}
			Owner.PlayWorldOrUISound("Sounds/Missile/Special/sfx_missile_energyWeapon_cellRemoved");
			IComponent<GameObject>.WDidXToYWithZ(Owner, "pop", cell, "out of", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: false, PossessiveIndirectObject: false, null, null, (ParentObject.Equipped == Owner || ParentObject.InInventory == Owner) ? Owner : null);
		}
		finally
		{
			if (!flag)
			{
				cell.RemoveIntProperty("NeverStack");
			}
		}
		cell.RemoveIntProperty("StoredByPlayer");
		SetCell(null);
		if (CheckStack)
		{
			cell.CheckStack();
		}
		return true;
	}

	public bool AttemptReplaceCell(GameObject Actor, InventoryActionEvent E, int MinimumCharge = 0, GameObject NewCell = null)
	{
		GameObject.Validate(ref Cell);
		if (Actor == null)
		{
			Actor = ParentObject.Holder;
			if (Actor == null)
			{
				return false;
			}
		}
		if (ParentObject.HasRegisteredEvent("BeforeReplaceCell"))
		{
			Event e = Event.New("BeforeReplaceCell", "Actor", Actor, "Cell", Cell, "NewCell", NewCell);
			if (!ParentObject.FireEvent(e, E))
			{
				return false;
			}
		}
		bool flag = false;
		if (Cell != null && Cell.GetIntProperty("StoredByPlayer") < 1 && Actor.IsPlayer() && ParentObject.Owner != null && ParentObject.Equipped != Actor && ParentObject.InInventory != Actor)
		{
			if (Popup.ShowYesNoCancel(ParentObject.IndicativeDistal + ParentObject.Is + " not owned by you. Are you sure you want to access " + ParentObject.its + " energy cell?") != DialogResult.Yes)
			{
				return false;
			}
			flag = true;
		}
		Inventory inventory = Actor.Inventory;
		if (inventory == null)
		{
			return false;
		}
		GameObject cell = Cell;
		bool flag2 = Cell != null && Cell.QueryCharge(LiveOnly: false, 0L) > 0 && !IsBroken() && !IsRusted() && !IsEMPed();
		bool flag3 = false;
		bool flag4 = false;
		int num = 0;
		if (NewCell == null)
		{
			bool flag5 = ParentObject.GetObjectContext() == Actor;
			List<string> list = new List<string>(16);
			List<object> list2 = new List<object>(16);
			List<char> list3 = new List<char>(16);
			if (Cell != null)
			{
				list.Add("remove cell: " + cell.DisplayName);
				list2.Add(null);
				list3.Add('-');
				if (flag5)
				{
					num++;
				}
				TinkerItem part = Cell.GetPart<TinkerItem>();
				if (part != null && part.CanBeDisassembled(Actor))
				{
					list.Add("disassemble cell");
					list2.Add(-1);
					list3.Add('/');
					if (flag5)
					{
						num++;
					}
				}
			}
			List<GameObject> objs = Event.NewGameObjectList();
			Dictionary<GameObject, int> charge = new Dictionary<GameObject, int>();
			Dictionary<GameObject, string> names = new Dictionary<GameObject, string>();
			inventory.ForeachObject((GameObject GO) =>
			{
				if (!Actor.IsPlayer() || GO.Understood())
				{
					GO.ForeachPartDescendedFrom((IEnergyCell P) =>
					{
						if (P.SlotType == SlotType && CanBeSlottedEvent.Check(ParentObject, GO, P))
						{
							objs.Add(GO);
							charge[GO] = P.GetCharge();
							names[GO] = GO.DisplayName;
							return false;
						}
						return true;
					});
				}
			});
			if (objs.Count > 1)
			{
				objs.Sort((GameObject a, GameObject b) =>
				{
					int num2 = charge[a].CompareTo(charge[b]);
					return (num2 != 0) ? (-num2) : names[a].CompareTo(names[b]);
				});
			}
			else if (objs.Count == 0)
			{
				num = 0;
			}
			char c = 'a';
			foreach (GameObject item in objs)
			{
				if (CompatibleCell(item))
				{
					list.Add(names[item]);
					list2.Add(item);
					if (c <= 'z')
					{
						list3.Add(c++);
					}
					else
					{
						list3.Add(' ');
					}
				}
			}
			if (list2.Count == 0)
			{
				Actor.Fail("You have no cells that fit.");
				return false;
			}
			int choice = -1;
			if (Actor.IsPlayer())
			{
				choice = Popup.PickOption("Choose a cell for " + ParentObject.t(), null, "", "Sounds/UI/ui_notification", list.ToArray(), list3.ToArray(), null, null, null, null, null, 0, 60, num, -1, AllowEscape: true);
			}
			else
			{
				Cell?.ForeachPartDescendedFrom((IEnergyCell P) =>
				{
					MinimumCharge = Math.Max(MinimumCharge, P.GetCharge() + 1);
				});
				int maxCharge = -1;
				int x;
				for (x = 0; x < list2.Count; x++)
				{
					if (!(list2[x] is GameObject gameObject))
					{
						continue;
					}
					gameObject.ForeachPartDescendedFrom((IEnergyCell P) =>
					{
						int charge2 = P.GetCharge();
						if (charge2 > maxCharge && charge2 > 0 && charge2 >= MinimumCharge)
						{
							choice = x;
							maxCharge = charge2;
						}
					});
				}
			}
			if (choice < 0)
			{
				return false;
			}
			if (list2[choice] == null)
			{
				if (AttemptRemoveCell(Actor, E))
				{
					flag3 = true;
				}
			}
			else if (list2[choice] as int? == -1)
			{
				Cell cell2 = ParentObject.GetCellContext() ?? ParentObject.GetObjectContext()?.GetCellContext();
				GameObject Object = Cell;
				if (AttemptRemoveCell(Actor, E, CheckStack: false))
				{
					flag3 = true;
					if (cell2 != null)
					{
						Object.RemoveFromContext();
						cell2.AddObject(Object, Forced: false, System: false, IgnoreGravity: true);
					}
					InventoryActionEvent.Check(Object, Actor, Object, "Disassemble");
					FlushWeightCaches();
					if (GameObject.Validate(ref Object))
					{
						Object.CheckStack();
					}
				}
			}
			else
			{
				NewCell = list2[choice] as GameObject;
			}
		}
		else if (!CompatibleCell(NewCell))
		{
			Actor.Fail("That won't fit!");
			return false;
		}
		if (NewCell != null)
		{
			bool flag6 = false;
			if (Cell == null)
			{
				Actor.UseEnergy(E.OverrideEnergyCost ? E.EnergyCostOverride : 1000, "Reload Energy Cell");
				flag6 = true;
			}
			else if (AttemptRemoveCell(Actor, E))
			{
				flag6 = true;
				flag3 = true;
			}
			if (flag6)
			{
				ParentObject.SplitFromStack();
				NewCell = NewCell.RemoveOne();
				Event obj = Event.New("CommandRemoveObject");
				obj.SetParameter("Object", NewCell);
				obj.SetFlag("ForEquip", State: true);
				obj.SetSilent(Silent: true);
				if (!inventory.FireEvent(obj))
				{
					Actor.Fail("You can't take the new cell out of your inventory!");
					NewCell.CheckStack();
				}
				else
				{
					flag4 = true;
					NewCell.RemoveFromContext();
					SetCell(NewCell);
					IComponent<GameObject>.WDidXToYWithZ(Actor, "slot", Cell, "into", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: true, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: false, PossessiveIndirectObject: false, null, null, (ParentObject.Equipped == Actor || ParentObject.InInventory == Actor) ? Actor : null);
				}
			}
			else
			{
				Actor.Fail("You can't remove the old cell!");
			}
		}
		bool flag7 = Cell != null && Cell.QueryCharge(LiveOnly: false, 0L) > 0 && !IsBroken() && !IsRusted() && !IsEMPed();
		if (flag3 | flag4)
		{
			string tag = ParentObject.GetTag("ReloadSound");
			if (flag4 && !tag.IsNullOrEmpty())
			{
				PlayWorldSound(tag);
			}
			else if (flag4 && !(flag7 ? SlotCellPoweredSound : SlotCellUnpoweredSound).IsNullOrEmpty())
			{
				PlayWorldSound(flag7 ? SlotCellPoweredSound : SlotCellUnpoweredSound);
			}
			else if (flag3 && !(flag2 ? RemoveCellPoweredSound : RemoveCellUnpoweredSound).IsNullOrEmpty())
			{
				PlayWorldSound(flag2 ? RemoveCellPoweredSound : RemoveCellUnpoweredSound);
			}
			CellChangedEvent.Send(Actor, ParentObject, cell, Cell);
		}
		if (Cell != null && Actor.IsPlayer())
		{
			Cell.SetIntProperty("StoredByPlayer", 1);
		}
		if (flag3 & flag)
		{
			ParentObject.Physics.BroadcastForHelp(Actor);
		}
		ParentObject.CheckStack();
		return flag3 | flag4;
	}

	public bool CompatibleCell(GameObject Object)
	{
		if (GameObject.Validate(ref Object))
		{
			int i = 0;
			for (int count = Object.PartsList.Count; i < count; i++)
			{
				if (Object.PartsList[i] is IEnergyCell energyCell && energyCell.SlotType == SlotType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public IEnergyCell CompatibleCellPart(GameObject Object)
	{
		if (GameObject.Validate(ref Object))
		{
			int i = 0;
			for (int count = Object.PartsList.Count; i < count; i++)
			{
				if (Object.PartsList[i] is IEnergyCell energyCell && energyCell.SlotType == SlotType)
				{
					return energyCell;
				}
			}
		}
		return null;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TestCharge" || E.ID == "QueryCharge" || E.ID == "RechargeAvailable" || E.ID == "InductionCharge" || E.ID == "QueryRechargeStorage" || E.ID == "QueryChargeStorage")
		{
			if (GameObject.Validate(ref Cell) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && !Cell.FireEvent(E))
			{
				return false;
			}
		}
		else if (E.ID == "UseCharge" && GameObject.Validate(ref Cell) && IsReady(E.GetIntParameter("Charge") > 0, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && !Cell.FireEvent(E))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public int GetReplaceCellPriority()
	{
		int num = 100;
		IEnergyCell energyCell = CompatibleCellPart(Cell);
		if (energyCell != null)
		{
			num -= energyCell.GetChargePercentage();
		}
		if (ParentObject.Equipped != null || ParentObject.Implantee != null)
		{
			num += 200;
		}
		if ((ParentObject.HasPart<EnergyAmmoLoader>() || ParentObject.HasPart<ElectricalDischargeLoader>()) && !ParentObject.HasPart<PointDefense>())
		{
			num -= 100;
		}
		if (ParentObject.IsBroken() || ParentObject.IsRusted() || ParentObject.IsEMPed())
		{
			num -= 100;
		}
		if (ParentObject.IsImportant())
		{
			num += 100;
		}
		long longProperty = ParentObject.GetLongProperty("LastInventoryActionTurn", 0L);
		if (longProperty > 0 && The.CurrentTurn - longProperty < 10)
		{
			num += 100;
		}
		return num;
	}

	public string GetSlotTypeName()
	{
		return EnergyStorage.GetSlotTypeName(SlotType);
	}

	public string GetSlotTypeShortName()
	{
		return EnergyStorage.GetSlotTypeShortName(SlotType);
	}
}
