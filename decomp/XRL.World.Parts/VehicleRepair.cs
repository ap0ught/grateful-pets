using System;

namespace XRL.World.Parts;

[Serializable]
public class VehicleRepair : IPoweredPart
{
	public string AdjacentBlueprint;

	public string RequiredBlueprint;

	public string RequiredType;

	[NonSerialized]
	private string _VehicleTerm;

	public string VehicleTerm
	{
		get
		{
			if (_VehicleTerm == null)
			{
				if (ParentObject.Property.TryGetValue("VehicleTerm", out _VehicleTerm))
				{
					return _VehicleTerm;
				}
				if (!RequiredBlueprint.IsNullOrEmpty() && GameObjectFactory.Factory.Blueprints.TryGetValue(RequiredBlueprint, out var value))
				{
					_VehicleTerm = value.CachedDisplayNameStripped;
				}
				else
				{
					_VehicleTerm = "vehicle";
				}
			}
			return _VehicleTerm;
		}
	}

	public VehicleRepair()
	{
		WorksOnAdjacentCellContents = true;
	}

	public override bool WorksForEveryone()
	{
		return false;
	}

	public override bool WorksFor(GameObject Object)
	{
		return Object.Blueprint == AdjacentBlueprint;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != CanSmartUseEvent.ID)
		{
			return ID == CommandSmartUseEarlyEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			ParentObject.Twiddle();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("RepairVehicle", "repair " + VehicleTerm, "RepairVehicle", null, 'r');
		return base.HandleEvent(E);
	}

	public bool IsValidVehicle(GameObject Object)
	{
		if (Object.TryGetPart<Vehicle>(out var Part) && (RequiredBlueprint == null || Object.Blueprint == RequiredBlueprint))
		{
			if (RequiredType != null)
			{
				return Part.Type == RequiredType;
			}
			return true;
		}
		return false;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "RepairVehicle")
		{
			switch (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
			case ActivePartStatus.Unpowered:
				return E.Actor.ShowFailure(ParentObject.Does("do") + " not have enough charge to operate.");
			default:
				return E.Actor.ShowFailure(ParentObject.T() + " merely" + ParentObject.GetVerb("click") + ".");
			case ActivePartStatus.Operational:
				break;
			}
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject == null)
			{
				return false;
			}
			GameObject firstObject = activePartFirstSubject.CurrentCell.GetFirstObject(IsValidVehicle);
			if (firstObject == null)
			{
				return E.Actor.ShowFailure("There is no " + VehicleTerm + " on " + activePartFirstSubject.t() + ".");
			}
			if (!IsRepairableEvent.Check(E.Actor, firstObject, ParentObject) && ParentObject.Body.DismemberedParts.IsNullOrEmpty() && (ParentObject.HasPart(typeof(Stomach)) || ParentObject.GetPercentDamaged() == 0))
			{
				return E.Actor.ShowFailure(firstObject.Does("are") + " in no need of repairs.");
			}
			if (!E.Actor.UseDrams(1, "sunslag"))
			{
				return E.Actor.ShowFailure("You do not have 1 dram of " + LiquidVolume.GetLiquid("sunslag").GetName() + ".");
			}
			if (ConsumeCharge())
			{
				E.Actor.ShowSuccess("Claystuff is reprinted across the fissures and scars on " + firstObject.t() + ".");
				E.RequestInterfaceExit();
				firstObject.RestorePristineHealth(UseHeal: true);
				firstObject.FireEvent(Event.New("Regenera", "Level", 10, "Source", ParentObject));
				firstObject.DustPuff();
				RepairedEvent.Send(E.Actor, firstObject, ParentObject);
			}
		}
		return base.HandleEvent(E);
	}
}
