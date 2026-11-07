using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="F:XRL.World.Parts.IActivePart.IsPowerLoadSensitive" /> is true,
/// charge consumption is adjusted treating the power load as a percentage
/// (i.e. the standard overload power load of 400 multiplies charge
/// consumption by 4).
/// </remarks>
[Serializable]
public class IActivePart : IPart
{
	public int ChargeUse;

	public int ChargeMinimum;

	public int RequiresBodyPartCategoryCode;

	public int LiquidConsumptionAmount = 1;

	public int LiquidConsumptionChanceOneIn = 1;

	public bool IsBootSensitive;

	public bool IsBreakageSensitive = true;

	public bool IsEMPSensitive;

	public bool IsHangingSensitive;

	public bool IsPowerSwitchSensitive;

	public bool IsRealityDistortionBased;

	public bool IsRustSensitive = true;

	public bool IsPowerLoadSensitive;

	public bool MustBeUnderstood;

	public bool WorksOnAdjacentCellContents;

	public bool WorksOnCarrier;

	public bool WorksOnCellContents;

	public bool WorksOnEnclosed;

	public bool WorksOnEquipper;

	public bool WorksOnHolder;

	public bool WorksOnImplantee;

	public bool WorksOnInventory;

	public bool WorksOnSelf;

	public bool WorksOnWearer;

	public bool LiquidMustBePure = true;

	public string ConsumesLiquid;

	public string NeedsOtherActivePartOperational;

	public string NeedsOtherActivePartEngaged;

	public string DescribeStatusForProperty;

	public string NameForStatus;

	public string StatusStyle = "plain";

	public string ReadyColorString;

	public string ReadyDetailColor;

	public string DisabledColorString;

	public string DisabledDetailColor;

	[NonSerialized]
	public ActivePartStatus? LastStatus;

	[NonSerialized]
	public int? LastPowerLoadLevel;

	private static List<string> relationalItems = new List<string>();

	private static List<string> overallItems = new List<string>();

	public string RequiresBodyPartCategory
	{
		get
		{
			if (RequiresBodyPartCategoryCode == 0)
			{
				return null;
			}
			return BodyPartCategory.GetName(RequiresBodyPartCategoryCode);
		}
		set
		{
			if (value == null)
			{
				RequiresBodyPartCategoryCode = 0;
			}
			else
			{
				RequiresBodyPartCategoryCode = BodyPartCategory.GetCode(value);
			}
		}
	}

	public bool IsTechScannable
	{
		get
		{
			return DescribeStatusForProperty == Scanning.GetScanPropertyName(Scanning.Scan.Tech);
		}
		set
		{
			if (value)
			{
				DescribeStatusForProperty = Scanning.GetScanPropertyName(Scanning.Scan.Tech);
				StatusStyle = "tech";
			}
			else if (DescribeStatusForProperty == Scanning.GetScanPropertyName(Scanning.Scan.Tech))
			{
				DescribeStatusForProperty = null;
				StatusStyle = "plain";
			}
		}
	}

	public bool IsBioScannable
	{
		get
		{
			return DescribeStatusForProperty == Scanning.GetScanPropertyName(Scanning.Scan.Bio);
		}
		set
		{
			if (value)
			{
				DescribeStatusForProperty = Scanning.GetScanPropertyName(Scanning.Scan.Bio);
				StatusStyle = "bio";
			}
			else if (DescribeStatusForProperty == Scanning.GetScanPropertyName(Scanning.Scan.Bio))
			{
				DescribeStatusForProperty = null;
				StatusStyle = "plain";
			}
		}
	}

	public bool IsStructureScannable
	{
		get
		{
			return DescribeStatusForProperty == Scanning.GetScanPropertyName(Scanning.Scan.Structure);
		}
		set
		{
			if (value)
			{
				DescribeStatusForProperty = Scanning.GetScanPropertyName(Scanning.Scan.Structure);
				StatusStyle = "structure";
			}
			else if (DescribeStatusForProperty == Scanning.GetScanPropertyName(Scanning.Scan.Structure))
			{
				DescribeStatusForProperty = null;
				StatusStyle = "plain";
			}
		}
	}

	public bool IsPowered
	{
		get
		{
			if (ChargeUse <= 0)
			{
				return ChargeMinimum > 0;
			}
			return true;
		}
		set
		{
			if (value)
			{
				if (ChargeUse == 0 && ChargeMinimum == 0)
				{
					ChargeMinimum = 1;
				}
			}
			else
			{
				ChargeUse = 0;
				ChargeMinimum = 0;
			}
			IsBootSensitive = value;
			IsEMPSensitive = value;
			IsPowerSwitchSensitive = value;
			IsTechScannable = value;
		}
	}

	public override bool SameAs(IPart p)
	{
		IActivePart activePart = p as IActivePart;
		if (activePart.ChargeUse != ChargeUse)
		{
			return false;
		}
		if (activePart.ChargeMinimum != ChargeMinimum)
		{
			return false;
		}
		if (activePart.RequiresBodyPartCategoryCode != RequiresBodyPartCategoryCode)
		{
			return false;
		}
		if (activePart.IsBootSensitive != IsBootSensitive)
		{
			return false;
		}
		if (activePart.IsBreakageSensitive != IsBreakageSensitive)
		{
			return false;
		}
		if (activePart.IsEMPSensitive != IsEMPSensitive)
		{
			return false;
		}
		if (activePart.IsPowerSwitchSensitive != IsPowerSwitchSensitive)
		{
			return false;
		}
		if (activePart.IsRealityDistortionBased != IsRealityDistortionBased)
		{
			return false;
		}
		if (activePart.IsRustSensitive != IsRustSensitive)
		{
			return false;
		}
		if (activePart.MustBeUnderstood != MustBeUnderstood)
		{
			return false;
		}
		if (activePart.WorksOnAdjacentCellContents != WorksOnAdjacentCellContents)
		{
			return false;
		}
		if (activePart.WorksOnCarrier != WorksOnCarrier)
		{
			return false;
		}
		if (activePart.WorksOnCellContents != WorksOnCellContents)
		{
			return false;
		}
		if (activePart.WorksOnEnclosed != WorksOnEnclosed)
		{
			return false;
		}
		if (activePart.WorksOnEquipper != WorksOnEquipper)
		{
			return false;
		}
		if (activePart.WorksOnHolder != WorksOnHolder)
		{
			return false;
		}
		if (activePart.WorksOnImplantee != WorksOnImplantee)
		{
			return false;
		}
		if (activePart.WorksOnInventory != WorksOnInventory)
		{
			return false;
		}
		if (activePart.WorksOnSelf != WorksOnSelf)
		{
			return false;
		}
		if (activePart.WorksOnWearer != WorksOnWearer)
		{
			return false;
		}
		if (activePart.NeedsOtherActivePartOperational != NeedsOtherActivePartOperational)
		{
			return false;
		}
		if (activePart.NeedsOtherActivePartEngaged != NeedsOtherActivePartEngaged)
		{
			return false;
		}
		if (activePart.ReadyColorString != ReadyColorString)
		{
			return false;
		}
		if (activePart.ReadyDetailColor != ReadyDetailColor)
		{
			return false;
		}
		if (activePart.DisabledColorString != DisabledColorString)
		{
			return false;
		}
		if (activePart.DisabledDetailColor != DisabledDetailColor)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AllowLiquidCollectionEvent.ID || ConsumesLiquid.IsNullOrEmpty()) && ID != BootSequenceAbortedEvent.ID && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != PooledEvent<CheckUsesChargeWhileEquippedEvent>.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && (ID != GetPreferredLiquidEvent.ID || ConsumesLiquid.IsNullOrEmpty()) && ID != PooledEvent<GetScanTypeEvent>.ID && ID != GetShortDescriptionEvent.ID && (ID != PooledEvent<IsOverloadableEvent>.ID || !IsPowerLoadSensitive) && (ID != PowerSwitchFlippedEvent.ID || !IsPowerSwitchSensitive) && ID != PooledEvent<QueryDrawEvent>.ID && ID != PooledEvent<SyncRenderEvent>.ID && (ID != PooledEvent<TransparentToEMPEvent>.ID || !IsEMPSensitive))
		{
			if (ID == WantsLiquidCollectionEvent.ID)
			{
				return !ConsumesLiquid.IsNullOrEmpty();
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		if (!ConsumesLiquid.IsNullOrEmpty() && !WantsLiquidCollection(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPreferredLiquidEvent E)
	{
		if (E.Liquid == null && !ConsumesLiquid.IsNullOrEmpty())
		{
			E.Liquid = ConsumesLiquid;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		if (WantsLiquidCollection(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetScanTypeEvent E)
	{
		if (E.ScanType == Scanning.Scan.Structure && E.Object == ParentObject)
		{
			if (IsBioScannable)
			{
				E.ScanType = Scanning.Scan.Bio;
			}
			else if (IsTechScannable)
			{
				E.ScanType = Scanning.Scan.Tech;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryDrawEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.Draw += GetDraw(E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckUsesChargeWhileEquippedEvent E)
	{
		if (ChargeUse > 0)
		{
			if (WorksOnEquipper || WorksOnHolder || WorksOnWearer)
			{
				return false;
			}
			if (WorksOnSelf && ParentObject.IsTakeable() && ParentObject.GetInventoryCategory() != "Ammo" && !ParentObject.HasPropertyOrTag("CannotEquip"))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!DescribeStatusForProperty.IsNullOrEmpty() && E.Context != "Tinkering" && The.Player != null && The.Player.GetIntProperty(DescribeStatusForProperty) > 0)
		{
			E.Infix.Append('\n').Append(GetStatusDescription());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TransparentToEMPEvent E)
	{
		if (IsEMPSensitive)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceAbortedEvent E)
	{
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SyncRenderEvent E)
	{
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsOverloadableEvent E)
	{
		if (IsPowerLoadSensitive)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		E.AddEntry(this, "Status", activePartStatus.ToString());
		if (activePartStatus == ActivePartStatus.LocallyDefinedFailure)
		{
			E.AddEntry(this, "LocallyDefinedFailureDescription", GetActivePartLocallyDefinedFailureDescription());
		}
		E.AddEntry(this, "FirstSubject", GetActivePartFirstSubject());
		E.AddEntry(this, "ChargeUse", ChargeUse);
		E.AddEntry(this, "ChargeMinimum", ChargeMinimum);
		E.AddEntry(this, "RequiresBodyPartCategory", RequiresBodyPartCategory);
		E.AddEntry(this, "IsBootSensitive", IsBootSensitive);
		E.AddEntry(this, "IsBreakageSensitive", IsBreakageSensitive);
		E.AddEntry(this, "IsEMPSensitive", IsEMPSensitive);
		E.AddEntry(this, "IsHangingSensitive", IsHangingSensitive);
		E.AddEntry(this, "IsPowerSwitchSensitive", IsPowerSwitchSensitive);
		E.AddEntry(this, "IsRealityDistortionBased", IsRealityDistortionBased);
		E.AddEntry(this, "IsRustSensitive", IsRustSensitive);
		E.AddEntry(this, "MustBeUnderstood", MustBeUnderstood);
		E.AddEntry(this, "WorksOnAdjacentCellContents", WorksOnAdjacentCellContents);
		E.AddEntry(this, "WorksOnCarrier", WorksOnCarrier);
		E.AddEntry(this, "WorksOnCellContents", WorksOnCellContents);
		E.AddEntry(this, "WorksOnEnclosed", WorksOnEnclosed);
		E.AddEntry(this, "WorksOnEquipper", WorksOnEquipper);
		E.AddEntry(this, "WorksOnHolder", WorksOnHolder);
		E.AddEntry(this, "WorksOnImplantee", WorksOnImplantee);
		E.AddEntry(this, "WorksOnInventory", WorksOnInventory);
		E.AddEntry(this, "WorksOnSelf", WorksOnSelf);
		E.AddEntry(this, "WorksOnWearer", WorksOnWearer);
		E.AddEntry(this, "NeedsOtherActivePartOperational", NeedsOtherActivePartOperational);
		E.AddEntry(this, "NeedsOtherActivePartEngaged", NeedsOtherActivePartEngaged);
		E.AddEntry(this, "DescribeStatusForProperty", DescribeStatusForProperty);
		E.AddEntry(this, "NameForStatus", NameForStatus);
		E.AddEntry(this, "StatusStyle", StatusStyle);
		E.AddEntry(this, "ReadyColorString", ReadyColorString);
		E.AddEntry(this, "ReadyDetailColor", ReadyDetailColor);
		E.AddEntry(this, "DisabledColorString", DisabledColorString);
		E.AddEntry(this, "DisabledDetailColor", DisabledDetailColor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject) && IsTechScannable)
		{
			E.Add("circuitry", 1);
		}
		return base.HandleEvent(E);
	}

	public void ResetSensitivity()
	{
		IsBootSensitive = false;
		IsBreakageSensitive = true;
		IsEMPSensitive = false;
		IsHangingSensitive = false;
		IsPowerSwitchSensitive = false;
		IsRealityDistortionBased = false;
		IsRustSensitive = true;
		IsPowerLoadSensitive = false;
	}

	public void ResetWorksOn()
	{
		WorksOnAdjacentCellContents = false;
		WorksOnCarrier = false;
		WorksOnCellContents = false;
		WorksOnEnclosed = false;
		WorksOnEquipper = false;
		WorksOnHolder = false;
		WorksOnImplantee = false;
		WorksOnInventory = false;
		WorksOnSelf = false;
		WorksOnWearer = false;
	}

	public void WorksOn(bool AdjacentCellContents = false, bool Carrier = false, bool CellContents = false, bool Enclosed = false, bool Equipper = false, bool Holder = false, bool Implantee = false, bool Inventory = false, bool Self = false, bool Wearer = false)
	{
		WorksOnAdjacentCellContents = AdjacentCellContents;
		WorksOnCarrier = Carrier;
		WorksOnCellContents = CellContents;
		WorksOnEnclosed = Enclosed;
		WorksOnEquipper = Equipper;
		WorksOnHolder = Holder;
		WorksOnImplantee = Implantee;
		WorksOnInventory = Inventory;
		WorksOnSelf = Self;
		WorksOnWearer = Wearer;
	}

	public void SyncWorksOn(IActivePart p)
	{
		WorksOnAdjacentCellContents = p.WorksOnAdjacentCellContents;
		WorksOnCarrier = p.WorksOnCarrier;
		WorksOnCellContents = p.WorksOnCellContents;
		WorksOnEnclosed = p.WorksOnEnclosed;
		WorksOnEquipper = p.WorksOnEquipper;
		WorksOnHolder = p.WorksOnHolder;
		WorksOnImplantee = p.WorksOnImplantee;
		WorksOnInventory = p.WorksOnInventory;
		WorksOnSelf = p.WorksOnSelf;
		WorksOnWearer = p.WorksOnWearer;
	}

	public bool HasAnyWorksOn()
	{
		if (!WorksOnAdjacentCellContents && !WorksOnCarrier && !WorksOnCellContents && !WorksOnEnclosed && !WorksOnEquipper && !WorksOnHolder && !WorksOnImplantee && !WorksOnInventory && !WorksOnSelf)
		{
			return WorksOnWearer;
		}
		return true;
	}

	public virtual void SyncRender()
	{
		if (LastStatus == ActivePartStatus.Operational)
		{
			if (!ReadyColorString.IsNullOrEmpty())
			{
				ParentObject.Render.ColorString = ReadyColorString;
			}
			if (!ReadyDetailColor.IsNullOrEmpty())
			{
				ParentObject.Render.DetailColor = ReadyDetailColor;
			}
		}
		else
		{
			if (!DisabledColorString.IsNullOrEmpty())
			{
				ParentObject.Render.ColorString = DisabledColorString;
			}
			if (!DisabledDetailColor.IsNullOrEmpty())
			{
				ParentObject.Render.DetailColor = DisabledDetailColor;
			}
		}
	}

	private bool Encloses(GameObject obj)
	{
		Enclosed effect = obj.GetEffect<Enclosed>();
		if (effect != null)
		{
			return effect.EnclosedBy == ParentObject;
		}
		return false;
	}

	public virtual bool WorksForEveryone()
	{
		return !MustBeUnderstood;
	}

	public virtual bool WorksFor(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (MustBeUnderstood)
		{
			if (obj.IsPlayer())
			{
				if (!ParentObject.Understood())
				{
					return false;
				}
			}
			else if (!obj.HasStat("Intelligence"))
			{
				return false;
			}
		}
		return true;
	}

	private bool EnclosesAndWorksFor(GameObject obj)
	{
		if (Encloses(obj))
		{
			return WorksFor(obj);
		}
		return false;
	}

	private bool NotSelf(GameObject obj)
	{
		return obj != ParentObject;
	}

	private bool WorksForAndNotSelf(GameObject obj)
	{
		if (obj != ParentObject)
		{
			return WorksFor(obj);
		}
		return false;
	}

	public virtual bool ActivePartNeedsSubject()
	{
		if (WorksOnSelf && WorksFor(ParentObject))
		{
			return false;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped))
		{
			return false;
		}
		if (WorksOnWearer && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped))
		{
			return false;
		}
		if (WorksOnImplantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && WorksFor(implantee))
			{
				return false;
			}
		}
		if (WorksOnEquipper && WorksFor(ParentObject?.EquippedProperlyBy()))
		{
			return false;
		}
		Cell cell = ParentObject?.CurrentCell;
		if (cell != null)
		{
			if (WorksOnCellContents)
			{
				int i = 0;
				for (int count = cell.Objects.Count; i < count; i++)
				{
					if (WorksForAndNotSelf(cell.Objects[i]))
					{
						return false;
					}
				}
			}
			if (WorksOnAdjacentCellContents)
			{
				foreach (Cell item in cell.IterateAdjacent())
				{
					int j = 0;
					for (int count2 = item.Objects.Count; j < count2; j++)
					{
						if (WorksFor(item.Objects[j]))
						{
							return false;
						}
					}
				}
			}
			if (WorksOnEnclosed)
			{
				int k = 0;
				for (int count3 = cell.Objects.Count; k < count3; k++)
				{
					if (EnclosesAndWorksFor(cell.Objects[k]))
					{
						return false;
					}
				}
			}
		}
		if (WorksOnCarrier && ((ParentObject.InInventory != null && WorksFor(ParentObject.InInventory)) || (ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped))))
		{
			return false;
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>() && ParentObject.Inventory.HasObject(WorksFor))
		{
			return false;
		}
		return true;
	}

	public virtual GameObject GetActivePartFirstSubject()
	{
		if (WorksOnSelf && WorksFor(ParentObject))
		{
			return ParentObject;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped))
		{
			return ParentObject.Equipped;
		}
		if (WorksOnWearer && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped))
		{
			return ParentObject.Equipped;
		}
		if (WorksOnImplantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && WorksFor(implantee))
			{
				return implantee;
			}
		}
		if (WorksOnEquipper)
		{
			GameObject gameObject = ParentObject.EquippedProperlyBy();
			if (WorksFor(gameObject))
			{
				return gameObject;
			}
		}
		if (WorksOnCarrier)
		{
			if (ParentObject.InInventory != null && WorksFor(ParentObject.InInventory))
			{
				return ParentObject.InInventory;
			}
			if (ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped))
			{
				return ParentObject.Equipped;
			}
		}
		if (WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			GameObject gameObject2 = (WorksForEveryone() ? ParentObject.CurrentCell.GetFirstObject(NotSelf) : ParentObject.CurrentCell.GetFirstObject(WorksForAndNotSelf));
			if (gameObject2 != null)
			{
				return gameObject2;
			}
		}
		if (WorksOnAdjacentCellContents && ParentObject.CurrentCell != null)
		{
			if (WorksForEveryone())
			{
				foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
				{
					GameObject firstObject = adjacentCell.GetFirstObject();
					if (firstObject != null)
					{
						return firstObject;
					}
				}
			}
			else
			{
				foreach (Cell adjacentCell2 in ParentObject.CurrentCell.GetAdjacentCells())
				{
					GameObject firstObject2 = adjacentCell2.GetFirstObject(WorksFor);
					if (firstObject2 != null)
					{
						return firstObject2;
					}
				}
			}
		}
		if (WorksOnEnclosed && ParentObject.CurrentCell != null)
		{
			GameObject firstObject3 = ParentObject.CurrentCell.GetFirstObject(WorksForEveryone() ? new Predicate<GameObject>(Encloses) : new Predicate<GameObject>(EnclosesAndWorksFor));
			if (firstObject3 != null)
			{
				return firstObject3;
			}
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>())
		{
			GameObject gameObject3 = (WorksForEveryone() ? ParentObject.Inventory.GetFirstObject() : ParentObject.Inventory.GetFirstObject(WorksFor));
			if (gameObject3 != null)
			{
				return gameObject3;
			}
		}
		return null;
	}

	public virtual GameObject GetActivePartFirstSubject(Predicate<GameObject> Filter)
	{
		if (WorksOnSelf && WorksFor(ParentObject) && Filter(ParentObject))
		{
			return ParentObject;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped) && Filter(ParentObject.Equipped))
		{
			return ParentObject.Equipped;
		}
		if (WorksOnWearer && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped) && Filter(ParentObject.Equipped))
		{
			return ParentObject.Equipped;
		}
		if (WorksOnImplantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && WorksFor(implantee) && Filter(implantee))
			{
				return implantee;
			}
		}
		if (WorksOnEquipper)
		{
			GameObject gameObject = ParentObject.EquippedProperlyBy();
			if (WorksFor(gameObject) && Filter(gameObject))
			{
				return gameObject;
			}
		}
		if (WorksOnCarrier)
		{
			if (ParentObject.InInventory != null && WorksFor(ParentObject.InInventory) && Filter(ParentObject.InInventory))
			{
				return ParentObject.InInventory;
			}
			if (ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped) && Filter(ParentObject.Equipped))
			{
				return ParentObject.Equipped;
			}
		}
		if (WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			foreach (GameObject @object in ParentObject.CurrentCell.Objects)
			{
				if (WorksFor(@object) && NotSelf(@object) && Filter(@object))
				{
					return @object;
				}
			}
		}
		if (WorksOnAdjacentCellContents && ParentObject.CurrentCell != null)
		{
			if (WorksForEveryone())
			{
				foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
				{
					GameObject firstObject = adjacentCell.GetFirstObject(Filter);
					if (firstObject != null)
					{
						return firstObject;
					}
				}
			}
			else
			{
				foreach (Cell adjacentCell2 in ParentObject.CurrentCell.GetAdjacentCells())
				{
					foreach (GameObject object2 in adjacentCell2.Objects)
					{
						if (WorksFor(object2) && Filter(object2))
						{
							return object2;
						}
					}
				}
			}
		}
		if (WorksOnEnclosed && ParentObject.CurrentCell != null)
		{
			GameObject firstObject2 = ParentObject.CurrentCell.GetFirstObject(WorksForEveryone() ? new Predicate<GameObject>(Encloses) : new Predicate<GameObject>(EnclosesAndWorksFor));
			if (firstObject2 != null && Filter(firstObject2))
			{
				return firstObject2;
			}
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>())
		{
			foreach (GameObject item in ParentObject.Inventory.GetObjectsDirect())
			{
				if (WorksFor(item) && Filter(item))
				{
					return item;
				}
			}
		}
		return null;
	}

	public virtual List<GameObject> GetActivePartSubjects()
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (WorksOnSelf && WorksFor(ParentObject))
		{
			list.Add(ParentObject);
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped) && !list.Contains(ParentObject.Equipped))
		{
			list.Add(ParentObject.Equipped);
		}
		if (WorksOnWearer && !list.Contains(ParentObject.Equipped) && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped))
		{
			list.Add(ParentObject.Equipped);
		}
		if (WorksOnImplantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && WorksFor(implantee))
			{
				list.Add(implantee);
			}
		}
		if (WorksOnEquipper)
		{
			GameObject gameObject = ParentObject.EquippedProperlyBy();
			if (gameObject != null && !list.Contains(gameObject) && WorksFor(gameObject))
			{
				list.Add(gameObject);
			}
		}
		if (WorksOnCarrier)
		{
			if (ParentObject.InInventory != null && WorksFor(ParentObject.InInventory))
			{
				list.Add(ParentObject.InInventory);
			}
			if (!list.Contains(ParentObject.Equipped) && ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped))
			{
				list.Add(ParentObject.Equipped);
			}
		}
		if (WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			List<GameObject> objectsViaEventList = ParentObject.CurrentCell.GetObjectsViaEventList();
			if (WorksForEveryone())
			{
				for (int i = 0; i < objectsViaEventList.Count; i++)
				{
					if (NotSelf(objectsViaEventList[i]))
					{
						list.Add(objectsViaEventList[i]);
					}
				}
			}
			else
			{
				for (int j = 0; j < objectsViaEventList.Count; j++)
				{
					if (WorksForAndNotSelf(objectsViaEventList[j]))
					{
						list.Add(objectsViaEventList[j]);
					}
				}
			}
		}
		if (WorksOnAdjacentCellContents && ParentObject.CurrentCell != null)
		{
			List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
			for (int k = 0; k < adjacentCells.Count; k++)
			{
				Cell cell = adjacentCells[k];
				list.AddRange(WorksForEveryone() ? cell.GetObjectsViaEventList() : cell.GetObjectsViaEventList(WorksFor));
			}
		}
		if (WorksOnEnclosed && ParentObject.CurrentCell != null)
		{
			List<GameObject> objectsViaEventList2 = ParentObject.CurrentCell.GetObjectsViaEventList();
			for (int l = 0; l < objectsViaEventList2.Count; l++)
			{
				GameObject gameObject2 = objectsViaEventList2[l];
				if (Encloses(gameObject2) && WorksFor(gameObject2) && !list.Contains(gameObject2))
				{
					list.Add(gameObject2);
				}
			}
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>())
		{
			list.AddRange(WorksForEveryone() ? ParentObject.Inventory.GetObjects() : ParentObject.Inventory.GetObjectsViaEventList(WorksFor));
		}
		return list;
	}

	public virtual List<GameObject> GetActivePartSubjects(Predicate<GameObject> Filter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (WorksOnSelf && WorksFor(ParentObject) && Filter(ParentObject))
		{
			list.Add(ParentObject);
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped) && !list.Contains(ParentObject.Equipped) && Filter(ParentObject.Equipped))
		{
			list.Add(ParentObject.Equipped);
		}
		if (WorksOnWearer && !list.Contains(ParentObject.Equipped) && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped) && Filter(ParentObject.Equipped))
		{
			list.Add(ParentObject.Equipped);
		}
		if (WorksOnImplantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && WorksFor(implantee) && Filter(implantee))
			{
				list.Add(implantee);
			}
		}
		if (WorksOnEquipper)
		{
			GameObject gameObject = ParentObject.EquippedProperlyBy();
			if (gameObject != null && !list.Contains(gameObject) && WorksFor(gameObject) && Filter(gameObject))
			{
				list.Add(gameObject);
			}
		}
		if (WorksOnCarrier)
		{
			if (ParentObject.InInventory != null && WorksFor(ParentObject.InInventory) && Filter(ParentObject.InInventory))
			{
				list.Add(ParentObject.InInventory);
			}
			if (!list.Contains(ParentObject.Equipped) && ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped) && Filter(ParentObject.Equipped))
			{
				list.Add(ParentObject.Equipped);
			}
		}
		if (WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			list.AddRange(WorksForEveryone() ? ParentObject.CurrentCell.GetObjects((GameObject obj) => NotSelf(obj) && Filter(obj)) : ParentObject.CurrentCell.GetObjects((GameObject obj) => WorksForAndNotSelf(obj) && Filter(obj)));
		}
		if (WorksOnAdjacentCellContents && ParentObject.CurrentCell != null)
		{
			foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
			{
				list.AddRange(WorksForEveryone() ? adjacentCell.GetObjects(Filter) : adjacentCell.GetObjects((GameObject obj) => WorksFor(obj) && Filter(obj)));
			}
		}
		if (WorksOnEnclosed && ParentObject.CurrentCell != null)
		{
			foreach (GameObject objectsViaEvent in ParentObject.CurrentCell.GetObjectsViaEventList())
			{
				if (Encloses(objectsViaEvent) && WorksFor(objectsViaEvent) && !list.Contains(objectsViaEvent) && Filter(objectsViaEvent))
				{
					list.Add(objectsViaEvent);
				}
			}
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>())
		{
			list.AddRange(WorksForEveryone() ? ParentObject.Inventory.GetObjects(Filter) : ParentObject.Inventory.GetObjects((GameObject obj) => WorksFor(obj) && Filter(obj)));
		}
		return list;
	}

	public virtual bool IsObjectActivePartSubject(GameObject Object)
	{
		if (Object == null)
		{
			return false;
		}
		if (!WorksFor(Object))
		{
			return false;
		}
		if (WorksOnSelf && ParentObject == Object)
		{
			return true;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && Object == ParentObject.Equipped)
		{
			return true;
		}
		if (WorksOnWearer && ParentObject.IsWorn() && Object == ParentObject.Equipped)
		{
			return true;
		}
		if (WorksOnImplantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && implantee == Object)
			{
				return true;
			}
		}
		if (WorksOnEquipper && Object == ParentObject.EquippedProperlyBy())
		{
			return true;
		}
		if (WorksOnCarrier)
		{
			if (ParentObject.InInventory != null && ParentObject.InInventory == Object)
			{
				return true;
			}
			if (ParentObject.IsEquippedAsThrownWeapon() && ParentObject.Equipped == Object)
			{
				return true;
			}
		}
		if (WorksOnCellContents && Object != ParentObject)
		{
			Cell cell = Object.CurrentCell;
			if (cell != null && cell == ParentObject.CurrentCell)
			{
				return true;
			}
		}
		if (WorksOnAdjacentCellContents && ParentObject.CurrentCell != null)
		{
			foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
			{
				if (adjacentCell.HasObject(Object))
				{
					return true;
				}
			}
		}
		if (WorksOnEnclosed && Encloses(Object))
		{
			return true;
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>() && ParentObject.Inventory.HasObject(Object))
		{
			return true;
		}
		return false;
	}

	public virtual bool ActivePartHasSingleSubject()
	{
		int num = 0;
		if (WorksOnSelf && WorksFor(ParentObject))
		{
			num++;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped))
		{
			if (++num > 1)
			{
				return false;
			}
		}
		else if (WorksOnWearer && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped))
		{
			if (++num > 1)
			{
				return false;
			}
		}
		else if (WorksOnEquipper && WorksFor(ParentObject.EquippedProperlyBy()))
		{
			if (++num > 1)
			{
				return false;
			}
		}
		else if (WorksOnCarrier && ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped) && ++num > 1)
		{
			return false;
		}
		if (WorksOnCarrier && ParentObject.InInventory != null && WorksFor(ParentObject.InInventory) && ++num > 1)
		{
			return false;
		}
		if (WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			num += ParentObject.CurrentCell.GetObjectCount(WorksForAndNotSelf);
			if (num > 1)
			{
				return false;
			}
		}
		if (WorksOnAdjacentCellContents && ParentObject.CurrentCell != null)
		{
			foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
			{
				num += adjacentCell.GetObjectCount(WorksFor);
				if (num > 1)
				{
					return false;
				}
			}
		}
		if (WorksOnEnclosed && !WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			foreach (GameObject objectsViaEvent in ParentObject.CurrentCell.GetObjectsViaEventList())
			{
				if (Encloses(objectsViaEvent) && WorksFor(objectsViaEvent) && NotSelf(objectsViaEvent) && ++num > 1)
				{
					return false;
				}
			}
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>())
		{
			num += ParentObject.Inventory.GetObjectCount(WorksFor);
			if (num > 1)
			{
				return false;
			}
		}
		return num == 1;
	}

	public virtual bool ActivePartHasMultipleSubjects()
	{
		int num = 0;
		if (WorksOnSelf && WorksFor(ParentObject))
		{
			num++;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped))
		{
			if (++num > 1)
			{
				return true;
			}
		}
		else if (WorksOnWearer && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped))
		{
			if (++num > 1)
			{
				return true;
			}
		}
		else if (WorksOnEquipper && WorksFor(ParentObject.EquippedProperlyBy()))
		{
			if (++num > 1)
			{
				return true;
			}
		}
		else if (WorksOnCarrier && ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped) && ++num > 1)
		{
			return true;
		}
		if (WorksOnCarrier && ParentObject.InInventory != null && WorksFor(ParentObject.InInventory) && ++num > 1)
		{
			return true;
		}
		if (WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			num += ParentObject.CurrentCell.GetObjectCount(WorksForAndNotSelf);
			if (num > 1)
			{
				return true;
			}
		}
		if (WorksOnAdjacentCellContents && ParentObject.CurrentCell != null)
		{
			foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
			{
				num += adjacentCell.GetObjectCount(WorksFor);
				if (num > 1)
				{
					return true;
				}
			}
		}
		if (WorksOnEnclosed && !WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			foreach (GameObject objectsViaEvent in ParentObject.CurrentCell.GetObjectsViaEventList())
			{
				if (Encloses(objectsViaEvent) && WorksFor(objectsViaEvent) && NotSelf(objectsViaEvent) && ++num > 1)
				{
					return true;
				}
			}
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>())
		{
			num += ParentObject.Inventory.GetObjectCount(WorksFor);
			if (num > 1)
			{
				return true;
			}
		}
		return num > 1;
	}

	public virtual int GetActivePartSubjectCount()
	{
		int num = 0;
		if (WorksOnSelf && WorksFor(ParentObject))
		{
			num++;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped))
		{
			num++;
		}
		else if (WorksOnWearer && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped))
		{
			num++;
		}
		else if (WorksOnEquipper && WorksFor(ParentObject.EquippedProperlyBy()))
		{
			num++;
		}
		else if (WorksOnCarrier && ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped))
		{
			num++;
		}
		if (WorksOnCarrier && ParentObject.InInventory != null && WorksFor(ParentObject.InInventory))
		{
			num++;
		}
		if (WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			num += ParentObject.CurrentCell.GetObjectCount(WorksForAndNotSelf);
		}
		if (WorksOnAdjacentCellContents && ParentObject.CurrentCell != null)
		{
			foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
			{
				num += adjacentCell.GetObjectCount(WorksFor);
			}
		}
		if (WorksOnEnclosed && !WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			foreach (GameObject objectsViaEvent in ParentObject.CurrentCell.GetObjectsViaEventList())
			{
				if (Encloses(objectsViaEvent) && WorksFor(objectsViaEvent) && NotSelf(objectsViaEvent))
				{
					num++;
				}
			}
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>())
		{
			num += ParentObject.Inventory.GetObjectCount(WorksFor);
		}
		return num;
	}

	/// <summary>
	/// A non-cancelable form of <c>ForeachActivePartSubjectWhile</c>, just for foreach over everthing.
	/// <b>PERFORMANCE NOTE:</b> If you are calling this from inside a tight loop on a bunch of things, please use
	/// <c>ForeachActivePartSubjectWhile</c> directly, this interface can create extra garbage.
	/// </summary>
	public virtual void ForeachActivePartSubject(Action<GameObject> Proc, bool MayMoveAddOrDestroy = false)
	{
		ForeachActivePartSubjectWhile((GameObject obj) =>
		{
			Proc(obj);
			return true;
		}, MayMoveAddOrDestroy);
	}

	/// <summary>
	/// The negated form of <c>ForeachActivePartSubjectWhile</c>, returning true will exit the foreach early,
	/// and cause the return value to be true.  It returns false on an empty set or if every predicate returns false.
	/// <b>PERFORMANCE NOTE:</b> If you are calling this from inside a tight loop on a bunch of things, please use
	/// <c>ForeachActivePartSubjectWhile</c> directly, this interface can create extra garbage.
	/// </summary>
	public bool ForeachActivePartSubjectUntil(Predicate<GameObject> Proc, bool MayMoveAddOrDestroy = false)
	{
		return !ForeachActivePartSubjectWhile((GameObject obj) => !Proc(obj));
	}

	private bool ForeachWhileReturnsTrue(GameObject o)
	{
		return true;
	}

	/// <summary>
	/// Finds the GameObjects that this part <c>WorksFor(GameObject)</c> in the ParentObjects's context,
	/// passing each to your predicate.  Returning false will exit the foreach early, and cause the return value to be false.
	/// It returns true on an empty set or if every predicate returns true.
	/// </summary>
	public virtual bool ForeachActivePartSubjectWhile(Predicate<GameObject> Proc, bool MayMoveAddOrDestroy = false)
	{
		if (Proc == null)
		{
			Proc = ForeachWhileReturnsTrue;
		}
		if (WorksOnSelf && WorksFor(ParentObject) && !Proc(ParentObject))
		{
			return false;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped) && !Proc(ParentObject.Equipped))
		{
			return false;
		}
		if (WorksOnWearer && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped) && !Proc(ParentObject.Equipped))
		{
			return false;
		}
		if (WorksOnEquipper)
		{
			GameObject obj = ParentObject.EquippedProperlyBy();
			if (WorksFor(obj) && !Proc(obj))
			{
				return false;
			}
		}
		if (WorksOnCarrier && ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped) && !Proc(ParentObject.Equipped))
		{
			return false;
		}
		if (WorksOnCarrier && ParentObject.InInventory != null && WorksFor(ParentObject.InInventory) && !Proc(ParentObject.InInventory))
		{
			return false;
		}
		if (WorksOnImplantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && WorksFor(implantee) && !Proc(implantee))
			{
				return false;
			}
		}
		if (WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			if (WorksForEveryone())
			{
				if (MayMoveAddOrDestroy)
				{
					if (!ParentObject.CurrentCell.SafeForeachObject(Proc, NotSelf))
					{
						return false;
					}
				}
				else if (!ParentObject.CurrentCell.ForeachObject(Proc, NotSelf))
				{
					return false;
				}
			}
			else if (MayMoveAddOrDestroy)
			{
				if (!ParentObject.CurrentCell.SafeForeachObject(Proc, WorksForAndNotSelf))
				{
					return false;
				}
			}
			else if (!ParentObject.CurrentCell.ForeachObject(Proc, WorksForAndNotSelf))
			{
				return false;
			}
		}
		if (WorksOnAdjacentCellContents && ParentObject.CurrentCell != null)
		{
			if (WorksForEveryone())
			{
				if (MayMoveAddOrDestroy)
				{
					foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
					{
						if (!adjacentCell.SafeForeachObject(Proc))
						{
							return false;
						}
					}
				}
				else
				{
					foreach (Cell adjacentCell2 in ParentObject.CurrentCell.GetAdjacentCells())
					{
						if (!adjacentCell2.ForeachObject(Proc))
						{
							return false;
						}
					}
				}
			}
			else if (MayMoveAddOrDestroy)
			{
				foreach (Cell adjacentCell3 in ParentObject.CurrentCell.GetAdjacentCells())
				{
					if (!adjacentCell3.SafeForeachObject(Proc, WorksFor))
					{
						return false;
					}
				}
			}
			else
			{
				foreach (Cell adjacentCell4 in ParentObject.CurrentCell.GetAdjacentCells())
				{
					if (!adjacentCell4.ForeachObject(Proc, WorksFor))
					{
						return false;
					}
				}
			}
		}
		if (WorksOnEnclosed && !WorksOnCellContents && ParentObject.CurrentCell != null)
		{
			if (MayMoveAddOrDestroy)
			{
				if (!ParentObject.CurrentCell.SafeForeachObject((GameObject obj2) => !Encloses(obj2) || !WorksFor(obj2) || !NotSelf(obj2) || Proc(obj2)))
				{
					return false;
				}
			}
			else
			{
				foreach (GameObject objectsViaEvent in ParentObject.CurrentCell.GetObjectsViaEventList())
				{
					if (Encloses(objectsViaEvent) && WorksFor(objectsViaEvent) && NotSelf(objectsViaEvent) && !Proc(objectsViaEvent))
					{
						return false;
					}
				}
			}
		}
		if (WorksOnInventory && ParentObject.HasPart<Inventory>())
		{
			if (WorksForEveryone())
			{
				if (MayMoveAddOrDestroy)
				{
					if (!ParentObject.Inventory.SafeForeachObject(Proc))
					{
						return false;
					}
				}
				else if (!ParentObject.Inventory.ForeachObject(Proc))
				{
					return false;
				}
			}
			else if (MayMoveAddOrDestroy)
			{
				if (!ParentObject.Inventory.SafeForeachObject(Proc, WorksFor))
				{
					return false;
				}
			}
			else if (!ParentObject.Inventory.ForeachObject(Proc, WorksFor))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool AnyActivePartSubjectWantsEvent(int ID, int cascade)
	{
		if (WorksOnSelf && WorksFor(ParentObject) && ParentObject.WantEvent(ID, cascade))
		{
			return true;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped) && ParentObject.Equipped.WantEvent(ID, cascade))
		{
			return true;
		}
		if (WorksOnWearer && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped) && ParentObject.Equipped.WantEvent(ID, cascade))
		{
			return true;
		}
		if (WorksOnEquipper)
		{
			GameObject gameObject = ParentObject.EquippedProperlyBy();
			if (WorksFor(gameObject) && gameObject.WantEvent(ID, cascade))
			{
				return true;
			}
		}
		if (WorksOnCarrier && ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped) && ParentObject.Equipped.WantEvent(ID, cascade))
		{
			return true;
		}
		if (WorksOnCarrier && ParentObject.InInventory != null && WorksFor(ParentObject.InInventory) && ParentObject.InInventory.WantEvent(ID, cascade))
		{
			return true;
		}
		if (WorksOnImplantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && WorksFor(implantee) && implantee.WantEvent(ID, cascade))
			{
				return true;
			}
		}
		if (WorksOnCellContents)
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				if (WorksForEveryone())
				{
					int i = 0;
					for (int count = cell.Objects.Count; i < count; i++)
					{
						if (cell.Objects[i] != ParentObject && cell.Objects[i].WantEvent(ID, cascade))
						{
							return true;
						}
					}
				}
				else
				{
					int j = 0;
					for (int count2 = cell.Objects.Count; j < count2; j++)
					{
						if (cell.Objects[j] != ParentObject && WorksFor(cell.Objects[j]) && cell.Objects[j].WantEvent(ID, cascade))
						{
							return true;
						}
					}
				}
			}
		}
		if (WorksOnAdjacentCellContents)
		{
			Cell cell2 = ParentObject.CurrentCell;
			if (cell2 != null)
			{
				List<Cell> adjacentCells = cell2.GetAdjacentCells();
				if (WorksForEveryone())
				{
					int k = 0;
					for (int count3 = adjacentCells.Count; k < count3; k++)
					{
						Cell cell3 = adjacentCells[k];
						int l = 0;
						for (int count4 = cell3.Objects.Count; l < count4; l++)
						{
							if (cell3.Objects[l] != ParentObject && cell3.Objects[l].WantEvent(ID, cascade))
							{
								return true;
							}
						}
					}
				}
				else
				{
					int m = 0;
					for (int count5 = adjacentCells.Count; m < count5; m++)
					{
						Cell cell4 = adjacentCells[m];
						int n = 0;
						for (int count6 = cell4.Objects.Count; n < count6; n++)
						{
							if (cell4.Objects[n] != ParentObject && WorksFor(cell4.Objects[n]) && cell4.Objects[n].WantEvent(ID, cascade))
							{
								return true;
							}
						}
					}
				}
			}
		}
		if (WorksOnEnclosed && !WorksOnCellContents)
		{
			Cell cell5 = ParentObject.CurrentCell;
			if (cell5 != null)
			{
				if (WorksForEveryone())
				{
					int num = 0;
					for (int count7 = cell5.Objects.Count; num < count7; num++)
					{
						if (cell5.Objects[num] != ParentObject && Encloses(cell5.Objects[num]) && cell5.Objects[num].WantEvent(ID, cascade))
						{
							return true;
						}
					}
				}
				else
				{
					int num2 = 0;
					for (int count8 = cell5.Objects.Count; num2 < count8; num2++)
					{
						if (cell5.Objects[num2] != ParentObject && WorksFor(cell5.Objects[num2]) && Encloses(cell5.Objects[num2]) && cell5.Objects[num2].WantEvent(ID, cascade))
						{
							return true;
						}
					}
				}
			}
		}
		if (WorksOnInventory)
		{
			Inventory inventory = ParentObject.Inventory;
			if (inventory != null)
			{
				List<GameObject> objects = inventory.GetObjects();
				if (WorksForEveryone())
				{
					int num3 = 0;
					for (int count9 = objects.Count; num3 < count9; num3++)
					{
						if (objects[num3] != ParentObject && objects[num3].WantEvent(ID, cascade))
						{
							return true;
						}
					}
				}
				else
				{
					int num4 = 0;
					for (int count10 = objects.Count; num4 < count10; num4++)
					{
						if (objects[num4] != ParentObject && WorksFor(objects[num4]) && objects[num4].WantEvent(ID, cascade))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public virtual bool ActivePartSubjectsHandleEvent(MinEvent E)
	{
		if (WorksOnSelf && WorksFor(ParentObject) && !ParentObject.HandleEvent(E))
		{
			return false;
		}
		if (WorksOnHolder && ParentObject.IsHeld() && WorksFor(ParentObject.Equipped) && !ParentObject.Equipped.HandleEvent(E))
		{
			return false;
		}
		if (WorksOnWearer && ParentObject.IsWorn() && WorksFor(ParentObject.Equipped) && !ParentObject.Equipped.HandleEvent(E))
		{
			return false;
		}
		if (WorksOnEquipper)
		{
			GameObject gameObject = ParentObject.EquippedProperlyBy();
			if (WorksFor(gameObject) && !gameObject.HandleEvent(E))
			{
				return false;
			}
		}
		if (WorksOnCarrier && ParentObject.IsEquippedAsThrownWeapon() && WorksFor(ParentObject.Equipped) && !ParentObject.Equipped.HandleEvent(E))
		{
			return false;
		}
		if (WorksOnCarrier && ParentObject.InInventory != null && WorksFor(ParentObject.InInventory) && !ParentObject.InInventory.HandleEvent(E))
		{
			return false;
		}
		if (WorksOnImplantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && WorksFor(implantee) && !implantee.HandleEvent(E))
			{
				return false;
			}
		}
		if (WorksOnCellContents)
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				if (WorksForEveryone())
				{
					int i = 0;
					for (int count = cell.Objects.Count; i < count; i++)
					{
						if (cell.Objects[i] != ParentObject && !cell.Objects[i].HandleEvent(E))
						{
							return false;
						}
					}
				}
				else
				{
					int j = 0;
					for (int count2 = cell.Objects.Count; j < count2; j++)
					{
						if (cell.Objects[j] != ParentObject && WorksFor(cell.Objects[j]) && !cell.Objects[j].HandleEvent(E))
						{
							return false;
						}
					}
				}
			}
		}
		if (WorksOnAdjacentCellContents)
		{
			Cell cell2 = ParentObject.CurrentCell;
			if (cell2 != null)
			{
				List<Cell> adjacentCells = cell2.GetAdjacentCells();
				if (WorksForEveryone())
				{
					int k = 0;
					for (int count3 = adjacentCells.Count; k < count3; k++)
					{
						Cell cell3 = adjacentCells[k];
						int l = 0;
						for (int count4 = cell3.Objects.Count; l < count4; l++)
						{
							if (cell3.Objects[l] != ParentObject && !cell3.Objects[l].HandleEvent(E))
							{
								return false;
							}
						}
					}
				}
				else
				{
					int m = 0;
					for (int count5 = adjacentCells.Count; m < count5; m++)
					{
						Cell cell4 = adjacentCells[m];
						int n = 0;
						for (int count6 = cell4.Objects.Count; n < count6; n++)
						{
							if (cell4.Objects[n] != ParentObject && WorksFor(cell4.Objects[n]) && !cell4.Objects[n].HandleEvent(E))
							{
								return false;
							}
						}
					}
				}
			}
		}
		if (WorksOnEnclosed && !WorksOnCellContents)
		{
			Cell cell5 = ParentObject.CurrentCell;
			if (cell5 != null)
			{
				if (WorksForEveryone())
				{
					int num = 0;
					for (int count7 = cell5.Objects.Count; num < count7; num++)
					{
						if (cell5.Objects[num] != ParentObject && Encloses(cell5.Objects[num]) && !cell5.Objects[num].HandleEvent(E))
						{
							return false;
						}
					}
				}
				else
				{
					int num2 = 0;
					for (int count8 = cell5.Objects.Count; num2 < count8; num2++)
					{
						if (cell5.Objects[num2] != ParentObject && WorksFor(cell5.Objects[num2]) && Encloses(cell5.Objects[num2]) && !cell5.Objects[num2].HandleEvent(E))
						{
							return false;
						}
					}
				}
			}
		}
		if (WorksOnInventory)
		{
			Inventory inventory = ParentObject.Inventory;
			if (inventory != null)
			{
				List<GameObject> objects = inventory.GetObjects();
				if (WorksForEveryone())
				{
					int num3 = 0;
					for (int count9 = objects.Count; num3 < count9; num3++)
					{
						if (objects[num3] != ParentObject && !objects[num3].HandleEvent(E))
						{
							return false;
						}
					}
				}
				else
				{
					int num4 = 0;
					for (int count10 = objects.Count; num4 < count10; num4++)
					{
						if (objects[num4] != ParentObject && WorksFor(objects[num4]) && !objects[num4].HandleEvent(E))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	public virtual bool GetActivePartLocallyDefinedFailure()
	{
		return false;
	}

	public virtual string GetActivePartLocallyDefinedFailureDescription()
	{
		return null;
	}

	private ActivePartStatus GetActivePartStatusCore(bool UseCharge = false, bool IgnoreCharge = false, bool IgnoreLiquid = false, bool IgnoreBootSequence = false, bool IgnoreBreakage = false, bool IgnoreRust = false, bool IgnoreEMP = false, bool IgnoreRealityStabilization = false, bool IgnoreSubject = false, bool IgnoreLocallyDefinedFailure = false, int MultipleCharge = 1, int? ChargeUse = null, bool UseChargeIfUnpowered = false, long GridMask = 0L, int? PowerLoadLevel = null)
	{
		if (!IgnoreSubject && ActivePartNeedsSubject())
		{
			return ActivePartStatus.NeedsSubject;
		}
		if (IsPowerSwitchSensitive)
		{
			PowerSwitch part = ParentObject.GetPart<PowerSwitch>();
			if (part != null && !part.Active)
			{
				return ActivePartStatus.SwitchedOff;
			}
		}
		if (IsEMPSensitive && !IgnoreEMP && IsEMPed())
		{
			return ActivePartStatus.EMP;
		}
		if (IsBreakageSensitive && !IgnoreBreakage && IsBroken())
		{
			return ActivePartStatus.Broken;
		}
		if (IsRustSensitive && !IgnoreRust && IsRusted())
		{
			return ActivePartStatus.Rusted;
		}
		if (IsBootSensitive && !IgnoreBootSequence)
		{
			BootSequence part2 = ParentObject.GetPart<BootSequence>();
			if (part2 != null && part2.BootTimeLeft > 0 && part2.WasReadyIfKnown())
			{
				return ActivePartStatus.Booting;
			}
		}
		if (IsHangingSensitive)
		{
			Hangable part3 = ParentObject.GetPart<Hangable>();
			if (part3 != null && !part3.Hanging)
			{
				return ActivePartStatus.NotHanging;
			}
		}
		if (RequiresBodyPartCategoryCode != 0 && !ParentObject.IsEquippedOnCategory(RequiresBodyPartCategoryCode))
		{
			return ActivePartStatus.LimbIncompatible;
		}
		if (IsRealityDistortionBased && !IgnoreRealityStabilization)
		{
			GameObject actor = ParentObject.Equipped ?? ParentObject.Implantee;
			if (!IComponent<GameObject>.CheckRealityDistortionUsability(ParentObject, null, actor, ParentObject))
			{
				return ActivePartStatus.RealityStabilized;
			}
			GameObject holder = ParentObject.Holder;
			if (holder != null && !IComponent<GameObject>.CheckRealityDistortionUsability(holder, null, actor, ParentObject))
			{
				return ActivePartStatus.RealityStabilized;
			}
		}
		if (!IgnoreLocallyDefinedFailure && GetActivePartLocallyDefinedFailure())
		{
			return ActivePartStatus.LocallyDefinedFailure;
		}
		if (NeedsOtherActivePartOperational != null && (!(ParentObject.GetPart(NeedsOtherActivePartOperational) is IActivePart activePart) || activePart.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L)))
		{
			return ActivePartStatus.PrimarySystemOffline;
		}
		if (NeedsOtherActivePartEngaged != null && (!(ParentObject.GetPart(NeedsOtherActivePartEngaged) is IActivePart activePart2) || !activePart2.IsActivePartEngaged()))
		{
			return ActivePartStatus.PrimarySystemOffline;
		}
		if (!IgnoreCharge)
		{
			if (!IgnoreLiquid && !ConsumesLiquid.IsNullOrEmpty())
			{
				LiquidVolume liquidVolume = ParentObject.LiquidVolume;
				if (liquidVolume == null || liquidVolume.Volume <= 0)
				{
					return ActivePartStatus.ProcessInputMissing;
				}
				if (LiquidConsumptionAmount > liquidVolume.Volume && (!LiquidMustBePure || liquidVolume.IsPureLiquid(ConsumesLiquid)))
				{
					if (UseCharge && (LiquidConsumptionChanceOneIn == 1 || Stat.Random(1, LiquidConsumptionChanceOneIn) == 1))
					{
						ConsumeLiquid(ConsumesLiquid, LiquidConsumptionAmount, !LiquidMustBePure);
					}
					return ActivePartStatus.ProcessInputMissing;
				}
				if (LiquidMustBePure && !liquidVolume.IsPureLiquid(ConsumesLiquid))
				{
					return ActivePartStatus.ProcessInputInvalid;
				}
			}
			int activeChargeUse = GetActiveChargeUse(ref PowerLoadLevel, ChargeUse);
			if (ChargeMinimum > activeChargeUse && !ParentObject.TestCharge(ChargeMinimum, LiveOnly: false, GridMask))
			{
				if (UseChargeIfUnpowered)
				{
					ParentObject.UseCharge(activeChargeUse, LiveOnly: false, GridMask, IncludeTransient: true, IncludeBiological: true, PowerLoadLevel);
				}
				return ActivePartStatus.Unpowered;
			}
			if (activeChargeUse > 0)
			{
				bool flag;
				if (!UseCharge)
				{
					flag = ParentObject.TestCharge(activeChargeUse * MultipleCharge, LiveOnly: false, GridMask);
					if (!flag & UseChargeIfUnpowered)
					{
						for (int i = 0; i < MultipleCharge; i++)
						{
							ParentObject.UseCharge(activeChargeUse, LiveOnly: false, GridMask, IncludeTransient: true, IncludeBiological: true, PowerLoadLevel);
						}
					}
				}
				else if (MultipleCharge == 1)
				{
					flag = ParentObject.UseCharge(activeChargeUse, LiveOnly: false, GridMask, IncludeTransient: true, IncludeBiological: true, PowerLoadLevel);
				}
				else
				{
					flag = ParentObject.UseCharge(activeChargeUse * MultipleCharge, LiveOnly: false, GridMask, IncludeTransient: true, IncludeBiological: true, PowerLoadLevel);
					if (!flag)
					{
						int j = 0;
						for (int num = MultipleCharge - 1; j < num; j++)
						{
							ParentObject.UseCharge(activeChargeUse, LiveOnly: false, GridMask, IncludeTransient: true, IncludeBiological: true, PowerLoadLevel);
						}
					}
				}
				if (!flag)
				{
					return ActivePartStatus.Unpowered;
				}
			}
			if (UseCharge && !IgnoreLiquid && !ConsumesLiquid.IsNullOrEmpty() && LiquidConsumptionChanceOneIn > 0 && LiquidConsumptionAmount > 0 && (LiquidConsumptionChanceOneIn == 1 || Stat.Random(1, LiquidConsumptionChanceOneIn) == 1))
			{
				ConsumeLiquid(ConsumesLiquid, LiquidConsumptionAmount, !LiquidMustBePure);
			}
		}
		return ActivePartStatus.Operational;
	}

	public ActivePartStatus GetActivePartStatus(bool UseCharge = false, bool IgnoreCharge = false, bool IgnoreLiquid = false, bool IgnoreBootSequence = false, bool IgnoreBreakage = false, bool IgnoreRust = false, bool IgnoreEMP = false, bool IgnoreRealityStabilization = false, bool IgnoreSubject = false, bool IgnoreLocallyDefinedFailure = false, int MultipleCharge = 1, int? ChargeUse = null, bool UseChargeIfUnpowered = false, long GridMask = 0L, int? PowerLoadLevel = null)
	{
		if (IgnoreCharge | IgnoreLiquid | IgnoreBootSequence | IgnoreBreakage | IgnoreRust | IgnoreEMP | IgnoreRealityStabilization | IgnoreSubject)
		{
			return GetActivePartStatusCore(UseCharge, IgnoreCharge, IgnoreLiquid, IgnoreBootSequence, IgnoreBreakage, IgnoreRust, IgnoreEMP, IgnoreRealityStabilization, IgnoreSubject, IgnoreLocallyDefinedFailure, MultipleCharge, ChargeUse, UseChargeIfUnpowered, GridMask, PowerLoadLevel);
		}
		ActivePartStatus? lastStatus = LastStatus;
		ActivePartStatus activePartStatusCore = GetActivePartStatusCore(UseCharge, IgnoreCharge, IgnoreLiquid, IgnoreBootSequence, IgnoreBreakage, IgnoreRust, IgnoreEMP, IgnoreRealityStabilization, IgnoreSubject, IgnoreLocallyDefinedFailure, MultipleCharge, ChargeUse, UseChargeIfUnpowered, GridMask, PowerLoadLevel);
		LastStatus = activePartStatusCore;
		if (LastStatus != lastStatus)
		{
			SyncRender();
		}
		return activePartStatusCore;
	}

	public bool IsDisabled(bool UseCharge = false, bool IgnoreCharge = false, bool IgnoreLiquid = false, bool IgnoreBootSequence = false, bool IgnoreBreakage = false, bool IgnoreRust = false, bool IgnoreEMP = false, bool IgnoreRealityStabilization = false, bool IgnoreSubject = false, bool IgnoreLocallyDefinedFailure = false, int MultipleCharge = 1, int? ChargeUse = null, bool UseChargeIfUnpowered = false, long GridMask = 0L, int? PowerLoadLevel = null)
	{
		return GetActivePartStatus(UseCharge, IgnoreCharge, IgnoreLiquid, IgnoreBootSequence, IgnoreBreakage, IgnoreRust, IgnoreEMP, IgnoreRealityStabilization, IgnoreSubject, IgnoreLocallyDefinedFailure, MultipleCharge, ChargeUse, UseChargeIfUnpowered, GridMask, PowerLoadLevel) != ActivePartStatus.Operational;
	}

	public bool IsReady(bool UseCharge = false, bool IgnoreCharge = false, bool IgnoreLiquid = false, bool IgnoreBootSequence = false, bool IgnoreBreakage = false, bool IgnoreRust = false, bool IgnoreEMP = false, bool IgnoreRealityStabilization = false, bool IgnoreSubject = false, bool IgnoreLocallyDefinedFailure = false, int MultipleCharge = 1, int? ChargeUse = null, bool UseChargeIfUnpowered = false, long GridMask = 0L, int? PowerLoadLevel = null)
	{
		return !IsDisabled(UseCharge, IgnoreCharge, IgnoreLiquid, IgnoreBootSequence, IgnoreBreakage, IgnoreRust, IgnoreEMP, IgnoreRealityStabilization, IgnoreSubject, IgnoreLocallyDefinedFailure, MultipleCharge, ChargeUse, UseChargeIfUnpowered, GridMask, PowerLoadLevel);
	}

	public ActivePartStatus GetLastActivePartStatus()
	{
		if (!LastStatus.HasValue)
		{
			return GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		}
		return LastStatus.Value;
	}

	public int GetLastPowerLoadLevel()
	{
		if (!LastPowerLoadLevel.HasValue)
		{
			LastPowerLoadLevel = ParentObject.GetPowerLoadLevel();
		}
		return LastPowerLoadLevel.Value;
	}

	public bool WasReady()
	{
		return GetLastActivePartStatus() == ActivePartStatus.Operational;
	}

	public bool WasReadyIfKnown()
	{
		return LastStatus == ActivePartStatus.Operational;
	}

	public bool WasDisabled()
	{
		return !WasReady();
	}

	public int GetActiveChargeUse(ref int? PowerLoadLevel, int? ChargeUse = null)
	{
		int num = ChargeUse ?? this.ChargeUse;
		if (IsPowerLoadSensitive)
		{
			int num2 = PowerLoadLevel ?? ParentObject.GetPowerLoadLevel();
			if (num2 != 100)
			{
				num = num * num2 / 100;
			}
			LastPowerLoadLevel = num2;
		}
		return num;
	}

	public int GetActiveChargeUse(int? ChargeUse = null, int? PowerLoadLevel = null)
	{
		return GetActiveChargeUse(ref PowerLoadLevel, ChargeUse);
	}

	public bool ConsumeCharge(int? ChargeUse = null, int? PowerLoadLevel = null)
	{
		int activeChargeUse = GetActiveChargeUse(ChargeUse, PowerLoadLevel);
		if (activeChargeUse <= 0)
		{
			return true;
		}
		return ParentObject.UseCharge(activeChargeUse, LiveOnly: false, 0L);
	}

	public bool ConsumeCharge(int MultipleCharge, int? ChargeUse = null, int? PowerLoadLevel = null)
	{
		int activeChargeUse = GetActiveChargeUse(ChargeUse, PowerLoadLevel);
		if (activeChargeUse <= 0)
		{
			return true;
		}
		return ParentObject.UseCharge(activeChargeUse * MultipleCharge, LiveOnly: false, 0L);
	}

	public bool ConsumeChargeIfOperational(bool IgnoreLiquid = false, bool IgnoreBootSequence = false, bool IgnoreBreakage = false, bool IgnoreRust = false, bool IgnoreEMP = false, bool IgnoreRealityStabilization = false, bool IgnoreSubject = false, bool IgnoreLocallyDefinedFailure = false, bool IgnoreWorldMap = false, int MultipleCharge = 1, int? ChargeUse = null, bool UseChargeIfUnpowered = false, int GridMask = 0, bool NeedStatusUpdate = false, int? PowerLoadLevel = null)
	{
		if (!IgnoreWorldMap && IsWorldMapActive)
		{
			return false;
		}
		if (!NeedStatusUpdate && GetActiveChargeUse(ChargeUse, PowerLoadLevel) <= 0)
		{
			return true;
		}
		return GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid, IgnoreBootSequence, IgnoreBreakage, IgnoreRust, IgnoreEMP, IgnoreRealityStabilization, IgnoreSubject, IgnoreLocallyDefinedFailure, MultipleCharge, ChargeUse, UseChargeIfUnpowered, GridMask, PowerLoadLevel) == ActivePartStatus.Operational;
	}

	public virtual int GetDraw(QueryDrawEvent E = null)
	{
		if (ChargeUse <= 0 || !IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return 0;
		}
		return ChargeUse;
	}

	public virtual bool IsActivePartEngaged()
	{
		return IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
	}

	public string GetStatusDescription()
	{
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		string text = null;
		if (activePartStatus == ActivePartStatus.LocallyDefinedFailure)
		{
			text = GetActivePartLocallyDefinedFailureDescription();
		}
		if (text == null)
		{
			text = activePartStatus.ToString();
		}
		return StyledStatus.Format(NameForStatus ?? Name, text, StatusStyle);
	}

	public virtual string GetStatusSummary(ActivePartStatus Status)
	{
		switch (Status)
		{
		case ActivePartStatus.EMP:
			return "{{W|EMP}}";
		case ActivePartStatus.Unpowered:
			return "{{K|unpowered}}";
		case ActivePartStatus.ProcessInputMissing:
			return "{{K|unfueled}}";
		case ActivePartStatus.ProcessInputInvalid:
			return "{{g|fuel contaminated}}";
		case ActivePartStatus.SwitchedOff:
			return "{{K|switched off}}";
		case ActivePartStatus.Booting:
		{
			BootSequence part = ParentObject.GetPart<BootSequence>();
			if (part == null || part.IsObvious())
			{
				return "{{b|warming up}}";
			}
			break;
		}
		}
		if (Status != ActivePartStatus.Operational && Status != ActivePartStatus.NeedsSubject)
		{
			return "{{r|nonfunctional}}";
		}
		return null;
	}

	public string GetStatusSummary()
	{
		return GetStatusSummary(GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L));
	}

	public void AddStatusSummary(StringBuilder SB, string Summary)
	{
		if (!Summary.IsNullOrEmpty())
		{
			SB.Append(" (").Append(Summary).Append(')');
		}
	}

	public void AddStatusSummary(StringBuilder SB)
	{
		AddStatusSummary(SB, GetStatusSummary());
	}

	public Action<StringBuilder> GetEventSensitiveAddStatusSummary(IEvent E)
	{
		if ((E as GetShortDescriptionEvent)?.Context == "Tinkering")
		{
			return (StringBuilder SB) =>
			{
			};
		}
		return AddStatusSummary;
	}

	public virtual string GetStatusPhrase(ActivePartStatus Status)
	{
		switch (Status)
		{
		case ActivePartStatus.EMP:
			return "{{W|disabled by electromagnetic pulse}}";
		case ActivePartStatus.Unpowered:
			return "{{K|unpowered}}";
		case ActivePartStatus.ProcessInputMissing:
			return "{{K|unfueled}}";
		case ActivePartStatus.ProcessInputInvalid:
			return "{{g|disabled by fuel contamination}}";
		case ActivePartStatus.SwitchedOff:
			return "{{K|switched off}}";
		case ActivePartStatus.Booting:
		{
			BootSequence part = ParentObject.GetPart<BootSequence>();
			if (part == null || part.IsObvious())
			{
				return "{{b|still warming up}}";
			}
			break;
		}
		}
		return "{{r|nonfunctional}}";
	}

	public string GetStatusPhrase()
	{
		return GetStatusPhrase(GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L));
	}

	public string GetOperationalScopeDescription()
	{
		relationalItems.Clear();
		overallItems.Clear();
		if (WorksOnEquipper || WorksOnImplantee)
		{
			relationalItems.Add("user");
		}
		else
		{
			if (WorksOnHolder)
			{
				relationalItems.Add("wielder");
			}
			if (WorksOnWearer)
			{
				relationalItems.Add("wearer");
			}
		}
		if (WorksOnCarrier)
		{
			relationalItems.Add("carrier");
		}
		if (WorksOnInventory)
		{
			relationalItems.Add("contents");
		}
		if (WorksOnAdjacentCellContents && WorksOnCellContents)
		{
			relationalItems.Add("vicinity");
		}
		else if (WorksOnCellContents)
		{
			relationalItems.Add("immediate vicinity");
		}
		else if (WorksOnAdjacentCellContents)
		{
			relationalItems.Add("nearby vicinity");
		}
		if (relationalItems.Count > 0)
		{
			overallItems.Add(ParentObject.its + " " + Grammar.MakeOrList(relationalItems));
		}
		if (WorksOnSelf)
		{
			overallItems.Add(ParentObject.itself);
		}
		if (WorksOnEnclosed)
		{
			overallItems.Add("someone enclosed within " + ParentObject.them);
		}
		if (overallItems.Count <= 0)
		{
			return ParentObject.its + " operating area";
		}
		return Grammar.MakeOrList(overallItems);
	}

	public override int MyPowerLoadBonus(int Load = int.MinValue, int Baseline = 100, int Divisor = 150)
	{
		if (!IsPowerLoadSensitive)
		{
			return 0;
		}
		return base.MyPowerLoadBonus(Load, Baseline, Divisor);
	}

	public override int MyPowerLoadLevel()
	{
		if (!IsPowerLoadSensitive)
		{
			return 100;
		}
		return base.MyPowerLoadLevel();
	}

	public virtual bool WantsLiquidCollection(string LiquidType)
	{
		if (ConsumesLiquid != null)
		{
			if (ConsumesLiquid == LiquidType)
			{
				return true;
			}
			if (!LiquidMustBePure && LiquidType.IndexOf(',') != -1 && LiquidType.IndexOf(ConsumesLiquid) != -1)
			{
				foreach (string item in LiquidType.CachedCommaExpansion())
				{
					if (item.Split('-')[0] == ConsumesLiquid)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
