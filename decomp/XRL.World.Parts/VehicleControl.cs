using System;

namespace XRL.World.Parts;

[Serializable]
public class VehicleControl : IPart
{
	public override bool WantEvent(int ID, int Cascade)
	{
		if (ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != CanSmartUseEvent.ID)
		{
			return ID == CommandSmartUseEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Control", "control", "VehicleControl", null, 'c');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "VehicleControl" && AttemptPilot(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		AttemptPilot(E.Actor);
		return false;
	}

	public bool AttemptPilot(GameObject Object)
	{
		InteriorZone interiorZone = ParentObject.CurrentZone as InteriorZone;
		Interior interior = interiorZone?.ParentObject?.GetPart<Interior>();
		if (interior != null && interior.HasRequired(ParentObject.Blueprint) && interior.GetRequired(ParentObject) < 0)
		{
			return false;
		}
		Vehicle vehicle = interiorZone?.ParentObject.GetPart<Vehicle>();
		if (vehicle == null)
		{
			return false;
		}
		if (vehicle.Pilot != null)
		{
			return Object.ShowFailure(ParentObject.T() + " is already in use by " + vehicle.Pilot.t() + ".");
		}
		vehicle.Pilot = Object;
		return true;
	}
}
