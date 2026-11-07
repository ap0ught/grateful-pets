using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ForceProjector : IPoweredPart
{
	public string Blueprint = "ForceBarrier";

	public string KeyObject;

	public string PickerLabel;

	public string PsychometryAccessMessage = "You touch =subject.t= and recall =pronouns.possessive= passcode. =pronouns.Subjective= =verb:beep:afterpronoun= warmly.";

	public string AccessFailureMessage = "A loud buzz is emitted. The unauthorized glyph flashes on the display.";

	public int MaxProjections = 9;

	public int ChargePerProjection = 90;

	public bool MaintenanceRequiresContiguity = true;

	public bool StepDownCharge = true;

	public bool AddUserToAllowPassage = true;

	public bool FindInitialDeployment;

	public string SuspendedDeployment;

	[NonSerialized]
	public List<GameObject> CurrentField = new List<GameObject>(16);

	[NonSerialized]
	public List<GameObject> AllowPassage = new List<GameObject>();

	[NonSerialized]
	public List<string> AllowFactions = new List<string>();

	[NonSerialized]
	private int LastCurrentFieldCount = -1;

	[NonSerialized]
	private bool DelayMaintenance;

	public int _BaseOperatingCharge = 1;

	public string _AllowPassageTag;

	/// <summary>
	///     This is a workaround for a cycle discovered 8-7-2021 -BB
	///     at XRL.World.Parts.ForceProjector.SyncCharge () [0x00009] in G:\Workspace\caves-of-qud\Assets\XRL Application\World\Game Objects\Parts\Furniture\ForceProjector.cs:334
	///     at XRL.World.Parts.ForceProjector.ResyncCharge () [0x00007] in G:\Workspace\caves-of-qud\Assets\XRL Application\World\Game Objects\Parts\Furniture\ForceProjector.cs:342
	///     at XRL.World.Parts.ForceProjector.ShutDownProjections () [0x0006e] in G:\Workspace\caves-of-qud\Assets\XRL Application\World\Game Objects\Parts\Furniture\ForceProjector.cs:542
	///     at XRL.World.Parts.ForceProjector.ForceProjectorDeploy (System.Collections.Generic.List`1[T] TargetCells) [0x00000] in G:\Workspace\caves-of-qud\Assets\XRL Application\World\Game Objects\Parts\Furniture\ForceProjector.cs:584
	///     at XRL.World.Parts.ForceProjector.MaintainProjections (System.Boolean FromDeploy) [0x0061b] in G:\Workspace\caves-of-qud\Assets\XRL Application\World\Game Objects\Parts\Furniture\ForceProjector.cs:517
	///     at XRL.World.Parts.ForceProjector.SyncCharge () [0x00009] in G:\Workspace\caves-of-qud\Assets\XRL Application\World\Game Objects\Parts\Furniture\ForceProjector.cs:334
	/// </summary>
	private long lastResyncTurn = -1L;

	public int SecurityClearance
	{
		get
		{
			return XRL.World.Capabilities.SecurityClearance.GetSecurityClearanceByKeySpecification(KeyObject);
		}
		set
		{
			XRL.World.Capabilities.SecurityClearance.HandleSecurityClearanceSpecification(value, ref KeyObject);
		}
	}

	public int BaseOperatingCharge
	{
		get
		{
			return _BaseOperatingCharge;
		}
		set
		{
			if (_BaseOperatingCharge != value)
			{
				_BaseOperatingCharge = value;
				SyncCharge();
			}
		}
	}

	public string AllowPassageTag
	{
		get
		{
			return _AllowPassageTag;
		}
		set
		{
			if (!(_AllowPassageTag != value))
			{
				return;
			}
			_AllowPassageTag = value;
			foreach (GameObject item in CurrentField)
			{
				Forcefield part = item.GetPart<Forcefield>();
				if (part != null)
				{
					part.AllowPassageTag = _AllowPassageTag;
				}
			}
		}
	}

	public string AllowPassageFactions
	{
		set
		{
			AllowFactions.Clear();
			if (!value.IsNullOrEmpty())
			{
				AllowFactions.AddRange(value.Split(','));
			}
			foreach (GameObject item in CurrentField)
			{
				if (!item.TryGetPart<Forcefield>(out var Part))
				{
					continue;
				}
				if (value.IsNullOrEmpty())
				{
					Part.AllowFactions = null;
					continue;
				}
				Part.AllowFactions?.Clear();
				Forcefield forcefield = Part;
				if (forcefield.AllowFactions == null)
				{
					forcefield.AllowFactions = new List<string>(AllowFactions.Count);
				}
				Part.AllowFactions.AddRange(AllowFactions);
			}
		}
	}

	public ForceProjector()
	{
		ChargeUse = _BaseOperatingCharge;
		IsRealityDistortionBased = true;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteGameObjectList(CurrentField);
		Writer.WriteGameObjectList(AllowPassage);
		Writer.Write(AllowFactions);
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Reader.ReadGameObjectList(CurrentField);
		Reader.ReadGameObjectList(AllowPassage);
		AllowFactions = Reader.ReadList<string>();
		base.Read(Basis, Reader);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CheckExistenceSupportEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != LeftCellEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (CurrentField.Contains(E.Object))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (DelayMaintenance)
		{
			DelayMaintenance = false;
		}
		else
		{
			int num = MaintainProjections();
			if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && num > 0)
			{
				ShutDownProjections();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			if (ParentObject.FireEvent("ForceProjectorActive"))
			{
				E.AddAction("Activate", "activate", "ActivateForceProjector", null, 'a', FireOnActor: false, 100);
			}
			else
			{
				E.AddAction("Deactivate", "deactivate", "DeactivateForceProjector", null, 'a', FireOnActor: false, 50);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateForceProjector")
		{
			if (ForceProjectorActivate(E.Actor, E))
			{
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == "DeactivateForceProjector" && ForceProjectorDeactivate(E.Actor, E))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		ShutDownProjections();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		ShutDownProjections();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		if (FindInitialDeployment)
		{
			LookForInitialDeployment();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BootSequenceInitialized");
		Registrar.Register("CanSmartUse");
		Registrar.Register("CommandSmartUse");
		Registrar.Register("EffectApplied");
		Registrar.Register("EnteredCell");
		Registrar.Register("ForceProjectorActive");
		Registrar.Register("PowerSwitchDeactivated");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ForceProjectorActive")
		{
			if (LastCurrentFieldCount == -1)
			{
				MaintainProjections();
			}
			if (LastCurrentFieldCount > 0)
			{
				return false;
			}
		}
		else if (E.ID == "CanSmartUse")
		{
			if (!E.GetGameObjectParameter("User").IsPlayer() || ParentObject.Understood())
			{
				return false;
			}
		}
		else if (E.ID == "CommandSmartUse")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("User");
			if (!gameObjectParameter.IsPlayer() || ParentObject.Understood())
			{
				if (LastCurrentFieldCount == -1)
				{
					MaintainProjections();
				}
				if (LastCurrentFieldCount > 0)
				{
					ForceProjectorDeactivate(gameObjectParameter, E);
				}
				else
				{
					ForceProjectorActivate(gameObjectParameter, E);
				}
			}
		}
		else if (E.ID == "EffectApplied" || E.ID == "PowerSwitchDeactivated" || E.ID == "BootSequenceInitialized")
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				ShutDownProjections();
			}
		}
		else if (E.ID == "EnteredCell" && FindInitialDeployment && ParentObject.CurrentCell.ParentZone.Built)
		{
			LookForInitialDeployment();
		}
		return base.FireEvent(E);
	}

	public bool AddAllowPassage(GameObject Actor)
	{
		if (Actor != null && !AllowPassage.Contains(Actor))
		{
			AllowPassage.Add(Actor);
			foreach (GameObject item in CurrentField)
			{
				item.GetPart<Forcefield>()?.AddAllowPassage(Actor);
			}
			return true;
		}
		return false;
	}

	public bool RemoveAllowPassage(GameObject Actor)
	{
		if (Actor != null && AllowPassage.Contains(Actor))
		{
			AllowPassage.Remove(Actor);
			foreach (GameObject item in CurrentField)
			{
				item.GetPart<Forcefield>()?.RemoveAllowPassage(Actor);
			}
			return true;
		}
		return false;
	}

	public bool AddAllowPassage(string Faction)
	{
		if (!Faction.IsNullOrEmpty() && !AllowFactions.Contains(Faction))
		{
			AllowFactions.Add(Faction);
			foreach (GameObject item in CurrentField)
			{
				if (item.TryGetPart<Forcefield>(out var Part))
				{
					Part.AddAllowPassage(Faction);
				}
			}
			return true;
		}
		return false;
	}

	public bool RemoveAllowPassage(string Faction)
	{
		if (!Faction.IsNullOrEmpty() && AllowFactions.Remove(Faction))
		{
			foreach (GameObject item in CurrentField)
			{
				if (item.TryGetPart<Forcefield>(out var Part))
				{
					Part.RemoveAllowPassage(Faction);
				}
			}
			return true;
		}
		return false;
	}

	public void SyncCharge()
	{
		if (LastCurrentFieldCount == -1)
		{
			MaintainProjections();
		}
		ChargeUse = BaseOperatingCharge + ChargePerProjection * LastCurrentFieldCount;
	}

	public void ResyncCharge()
	{
		if (lastResyncTurn != The.Game.Turns)
		{
			lastResyncTurn = The.Game.Turns;
			LastCurrentFieldCount = -1;
			SyncCharge();
		}
	}

	public int MaintainProjections(bool FromDeploy = false)
	{
		int num = 0;
		List<GameObject> list = null;
		foreach (GameObject item in CurrentField)
		{
			if (item == null || item.IsInvalid())
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(item);
			}
			else
			{
				num++;
				Phase.sync(ParentObject, item);
			}
		}
		if (list != null)
		{
			foreach (GameObject item2 in list)
			{
				CurrentField.Remove(item2);
			}
			list.Clear();
		}
		if (MaintenanceRequiresContiguity && num > 0)
		{
			List<GameObject> list2 = Event.NewGameObjectList();
			List<GameObject> list3 = Event.NewGameObjectList();
			List<GameObject> list4 = Event.NewGameObjectList();
			List<Cell> localAdjacentCells = ParentObject.CurrentCell.GetLocalAdjacentCells();
			foreach (GameObject item3 in CurrentField)
			{
				if (localAdjacentCells.Contains(item3.CurrentCell))
				{
					list4.Add(item3);
				}
			}
			while (list4.Count > 0 && list2.Count + list3.Count + list4.Count < num)
			{
				list3.AddRange(list4);
				list4.Clear();
				foreach (GameObject item4 in list3)
				{
					foreach (Cell localAdjacentCell in item4.CurrentCell.GetLocalAdjacentCells())
					{
						foreach (GameObject @object in localAdjacentCell.Objects)
						{
							if (CurrentField.Contains(@object) && !list2.Contains(@object) && !list3.Contains(@object) && !list4.Contains(@object))
							{
								list4.Add(@object);
							}
						}
					}
				}
				list2.AddRange(list3);
				list3.Clear();
			}
			if (list4.Count > 0)
			{
				list2.AddRange(list4);
				list4.Clear();
			}
			if (list2.Count < num)
			{
				List<GameObject> list5 = Event.NewGameObjectList();
				foreach (GameObject item5 in CurrentField)
				{
					if (!list2.Contains(item5))
					{
						list5.Add(item5);
						num--;
					}
				}
				foreach (GameObject item6 in list5)
				{
					CurrentField.Remove(item6);
					item6.Obliterate();
				}
				list5.Clear();
			}
		}
		if (StepDownCharge)
		{
			ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
			if (CurrentField.Count > 0 && activePartStatus == ActivePartStatus.Unpowered)
			{
				int num2 = (ParentObject.QueryCharge(LiveOnly: false, 0L) - BaseOperatingCharge) / ChargePerProjection;
				if (num2 > 0)
				{
					for (int num3 = CurrentField.Count - 1; num3 >= num2; num3--)
					{
						Cell cell = CurrentField[num3].CurrentCell;
						if (cell != null)
						{
							string text = cell.X + "," + cell.Y;
							if (!SuspendedDeployment.IsNullOrEmpty())
							{
								SuspendedDeployment = text + ";" + SuspendedDeployment;
							}
							else
							{
								SuspendedDeployment = text;
							}
						}
						CurrentField[num3].Obliterate();
						CurrentField.RemoveAt(num3);
					}
					num = num2;
				}
				else
				{
					StringBuilder stringBuilder = Event.NewStringBuilder();
					foreach (GameObject item7 in CurrentField)
					{
						Cell cell2 = item7.CurrentCell;
						if (cell2 != null)
						{
							stringBuilder.Compound(cell2.X, ";").Append(',').Append(cell2.Y);
						}
					}
					if (!SuspendedDeployment.IsNullOrEmpty())
					{
						stringBuilder.Compound(SuspendedDeployment, ";");
					}
					SuspendedDeployment = ((stringBuilder.Length > 0) ? stringBuilder.ToString() : null);
					ShutDownProjections();
					num = 0;
				}
			}
			else if (!FromDeploy && activePartStatus == ActivePartStatus.Operational && !SuspendedDeployment.IsNullOrEmpty())
			{
				int num4 = (ParentObject.QueryCharge(LiveOnly: false, 0L) - BaseOperatingCharge) / ChargePerProjection - CurrentField.Count;
				if (num4 > 0)
				{
					Zone currentZone = ParentObject.CurrentZone;
					if (currentZone != null)
					{
						List<Cell> list6 = Event.NewCellList();
						foreach (GameObject item8 in CurrentField)
						{
							Cell cell3 = item8.CurrentCell;
							if (cell3 != null)
							{
								list6.Add(cell3);
							}
						}
						List<string> list7 = new List<string>(SuspendedDeployment.Split(';'));
						int i = 0;
						for (int num5 = Math.Min(num4, list7.Count); i < num5; i++)
						{
							string text2 = list7[0];
							list7.RemoveAt(0);
							string[] array = text2.Split(',');
							int x = Convert.ToInt32(array[0]);
							int y = Convert.ToInt32(array[1]);
							Cell cell4 = currentZone.GetCell(x, y);
							if (cell4 != null)
							{
								list6.Add(cell4);
							}
						}
						ForceProjectorDeploy(list6);
						SuspendedDeployment = ((list7.Count > 0) ? string.Join(";", list7.ToArray()) : null);
					}
				}
			}
		}
		LastCurrentFieldCount = num;
		SyncCharge();
		return num;
	}

	public void ShutDownProjections()
	{
		int lastCurrentFieldCount = LastCurrentFieldCount;
		foreach (GameObject item in CurrentField)
		{
			if (item != null && item.IsValid())
			{
				item.Obliterate();
			}
		}
		CurrentField.Clear();
		LastCurrentFieldCount = 0;
		if (lastCurrentFieldCount != LastCurrentFieldCount)
		{
			ResyncCharge();
		}
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

	private bool AccessCheck(GameObject Actor, bool Silent = false)
	{
		if (KeyObject.IsNullOrEmpty())
		{
			return true;
		}
		List<string> list = KeyObject.CachedCommaExpansion();
		if (Actor.FindContainedObjectByAnyBlueprint(list) != null)
		{
			return true;
		}
		if (list.Contains("*Psychometry") && UsePsychometry(Actor, ParentObject))
		{
			if (!Silent && !PsychometryAccessMessage.IsNullOrEmpty())
			{
				IComponent<GameObject>.EmitMessage(Actor, GameText.VariableReplace(PsychometryAccessMessage, ParentObject, null, StripColors: true), ' ', FromDialog: true);
			}
			return true;
		}
		if (!Silent && !AccessFailureMessage.IsNullOrEmpty())
		{
			IComponent<GameObject>.EmitMessage(Actor, GameText.VariableReplace(AccessFailureMessage, ParentObject, null, StripColors: true), ' ', FromDialog: true);
		}
		return false;
	}

	public void ForceProjectorDeploy(List<Cell> TargetCells, GameObject Actor = null)
	{
		ShutDownProjections();
		foreach (Cell TargetCell in TargetCells)
		{
			if (!IsRealityDistortionBased || IComponent<GameObject>.CheckRealityDistortionAccessibility(null, TargetCell, Actor, ParentObject))
			{
				GameObject gameObject = GameObject.Create(Blueprint);
				Forcefield part = gameObject.GetPart<Forcefield>();
				ProjectionSetup(gameObject, part);
				TargetCell.AddObject(gameObject);
				CurrentField.Add(gameObject);
			}
		}
		MaintainProjections(FromDeploy: true);
	}

	public bool ForceProjectorActivate(GameObject Actor, IEvent E = null)
	{
		Cell CC = ParentObject.CurrentCell;
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) || CC == null)
		{
			Actor.ShowFailure(ParentObject.Does("are") + " unresponsive.");
			return false;
		}
		if (!AccessCheck(Actor))
		{
			return false;
		}
		MaintainProjections();
		if (AddUserToAllowPassage)
		{
			AddAllowPassage(Actor);
		}
		Dictionary<GameObject, Cell> dictionary = ((CurrentField.Count > 0) ? new Dictionary<GameObject, Cell>(CurrentField.Count) : null);
		try
		{
			if (dictionary != null)
			{
				foreach (GameObject item in CurrentField)
				{
					dictionary.Add(item, item.CurrentCell);
					item.RemoveFromContext();
				}
			}
			if (!IsRealityDistortionBased || ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Device", ParentObject, "Operator", Actor), E))
			{
				List<Cell> TargetCells = null;
				if (Actor.IsPlayer())
				{
					TargetCells = PickTarget.ShowFieldPicker(MaxProjections, 1, ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, PickerLabel, StartAdjacent: true, ReturnNullForAbort: true, AllowDiagonals: false, AllowDiagonalStart: false);
				}
				else
				{
					Cell cell = CC.GetRandomLocalCardinalAdjacentCell() ?? CC;
					Cell PrevCell = null;
					Cell cell2 = null;
					TargetCells = Event.NewCellList();
					TargetCells.Add(cell);
					for (int i = 1; i < MaxProjections; TargetCells.Add(cell2), PrevCell = cell, cell = cell2, i++)
					{
						int num = 0;
						while (++num < 20)
						{
							cell2 = cell.GetRandomLocalCardinalAdjacentCell((Cell c) => c != PrevCell && c != CC && !TargetCells.Contains(c));
							if (cell2 == null)
							{
								break;
							}
							if (num > 10 || !cell2.IsSolid())
							{
								continue;
							}
							goto IL_0235;
						}
						break;
						IL_0235:;
					}
				}
				if (TargetCells != null)
				{
					Actor.UseEnergy(1000, "Equipment");
					dictionary = null;
					ForceProjectorDeploy(TargetCells, Actor);
				}
			}
		}
		finally
		{
			if (dictionary != null)
			{
				foreach (KeyValuePair<GameObject, Cell> item2 in dictionary)
				{
					GameObject key = item2.Key;
					if (key != null && key.IsValid() && CurrentField.Contains(key))
					{
						item2.Value.AddObject(key);
					}
				}
			}
		}
		return true;
	}

	public bool ForceProjectorDeactivate(GameObject Actor, IEvent E = null)
	{
		Cell cell = ParentObject.CurrentCell;
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) || cell == null)
		{
			Actor.ShowFailure(ParentObject.Does("are") + " unresponsive.");
			return false;
		}
		if (!AccessCheck(Actor))
		{
			return false;
		}
		ShutDownProjections();
		return true;
	}

	private void ProjectionSetup(GameObject obj, Forcefield pFF)
	{
		if (pFF != null)
		{
			pFF.Creator = ParentObject;
			pFF.AllowPassageTag = AllowPassageTag;
			foreach (GameObject item in AllowPassage)
			{
				pFF.AddAllowPassage(item);
			}
			foreach (string allowFaction in AllowFactions)
			{
				pFF.AddAllowPassage(allowFaction);
			}
		}
		Phase.carryOver(ParentObject, obj);
	}

	public void LookForInitialDeployment()
	{
		FindInitialDeployment = false;
		ShutDownProjections();
		List<GameObject> list = Event.NewGameObjectList();
		List<GameObject> list2 = Event.NewGameObjectList();
		foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells())
		{
			GameObject firstObject = localAdjacentCell.GetFirstObject(Blueprint);
			if (firstObject != null)
			{
				Forcefield part = firstObject.GetPart<Forcefield>();
				if (part == null || part.Creator == null || part.Creator.IsInvalid() || part.Creator == ParentObject)
				{
					ProjectionSetup(firstObject, part);
					list2.Add(firstObject);
				}
			}
		}
		while (list2.Count > 0)
		{
			list.AddRange(list2);
			list2.Clear();
			foreach (GameObject item in list)
			{
				foreach (Cell localAdjacentCell2 in item.CurrentCell.GetLocalAdjacentCells())
				{
					GameObject firstObject2 = localAdjacentCell2.GetFirstObject(Blueprint);
					if (firstObject2 == null || CurrentField.Contains(firstObject2) || list.Contains(firstObject2) || list2.Contains(firstObject2))
					{
						continue;
					}
					Forcefield part2 = firstObject2.GetPart<Forcefield>();
					if (part2 == null || part2.Creator == null || part2.Creator.IsInvalid() || part2.Creator == ParentObject)
					{
						ProjectionSetup(item, part2);
						list2.Add(firstObject2);
						if (CurrentField.Count + list.Count + list2.Count >= MaxProjections)
						{
							goto end_IL_01cf;
						}
					}
				}
			}
			CurrentField.AddRange(list);
			list.Clear();
			continue;
			end_IL_01cf:
			break;
		}
		CurrentField.AddRange(list);
		list.Clear();
		if (list2.Count > 0)
		{
			CurrentField.AddRange(list2);
			list2.Clear();
		}
		if (ParentObject.QueryCharge(LiveOnly: false, 0L) <= 0)
		{
			DelayMaintenance = true;
		}
	}
}
