using System;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class VehicleSeat : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeingSatOn");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeingSatOn")
		{
			AttemptPilot(E.GetGameObjectParameter("Object"));
		}
		return base.FireEvent(E);
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
		if (!vehicle.IsOperational())
		{
			return Object.ShowFailure(vehicle.ParentObject.T() + " is not operational.");
		}
		if (!vehicle.CanBePilotedBy(Object))
		{
			if (!Object.IsPlayer() || vehicle.BindBlueprint.IsNullOrEmpty())
			{
				return Object.ShowFailure(vehicle.ParentObject.T() + " is not bound to you.");
			}
			GameObject gameObject = Object.FindObjectInInventory(vehicle.BindBlueprint);
			if (gameObject != null)
			{
				if (Popup.ShowYesNo("Accessing the pilot console requires the permanent insertion of " + gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".\n\nAre you sure you want to proceed?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) != DialogResult.Yes)
				{
					return false;
				}
				gameObject.Destroy();
				vehicle.OwnerID = (Object.IsPlayer() ? "Player" : Object.ID);
				Popup.Show("Access diodes flash in the affirmative.");
			}
			else
			{
				string word = GameObjectFactory.Factory.GetBlueprint(vehicle.BindBlueprint).DisplayName();
				Popup.Show("Accessing the pilot console requires the permanent insertion of " + Grammar.A(word) + ".");
			}
		}
		vehicle.Pilot = Object;
		return true;
	}
}
