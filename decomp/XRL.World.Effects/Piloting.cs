using System;
using System.Collections.Generic;
using XRL.World.AI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Piloting : Effect
{
	public GameObject Vehicle;

	public string VehicleFactions;

	public GameObjectReference VehicleLeader;

	public Dictionary<string, int> VehicleFeelings;

	public Dictionary<string, int> VehicleMembership;

	public Piloting()
	{
		DisplayName = "{{C|piloting}}";
		Duration = 1;
	}

	public Piloting(GameObject Vehicle)
	{
		this.Vehicle = Vehicle;
		DisplayName = "{{C|piloting " + Vehicle.an() + "}}";
		Duration = 1;
	}

	public override bool Apply(GameObject Object)
	{
		if (GameObject.Validate(ref Vehicle) && Vehicle.Brain != null)
		{
			Brain brain = Vehicle.Brain;
			VehicleLeader = brain.PartyLeader?.Reference();
			brain.PartyLeader = Object.Brain.PartyLeader;
			brain.TakeAllegiance<AllyPilot>(Object);
			brain.Goals.Clear();
			brain.ClearHostileMemory();
			brain.Target = null;
		}
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.Validate(ref Vehicle) && Vehicle.Brain != null)
		{
			Brain brain = Vehicle.Brain;
			brain.PartyLeader = VehicleLeader?.Object;
			brain.RemoveAllegiance<AllyPilot>(Object);
			brain.Goals.Clear();
			brain.ClearHostileMemory();
			brain.Target = null;
		}
		base.Remove(Object);
	}

	public void Stop()
	{
		if (GameObject.Validate(ref Vehicle))
		{
			Vehicle.GetPart<Vehicle>().Pilot = null;
		}
	}

	public override int GetEffectType()
	{
		return 83886208;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Can't take actions.";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != LeaveCellEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != TookDamageEvent.ID && ID != BeforeDeathRemovalEvent.ID && ID != GetZoneSuspendabilityEvent.ID)
		{
			return ID == GetZoneFreezabilityEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeaveCellEvent E)
	{
		if (E.Object == Object)
		{
			Stop();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (!GameObject.Validate(ref Vehicle) || Object.IsPlayer())
		{
			return Object.RemoveEffect(this);
		}
		return false;
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Object == Object)
		{
			Stop();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		Stop();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetZoneSuspendabilityEvent E)
	{
		if (GameObject.Validate(ref Vehicle) && Vehicle.IsPlayer())
		{
			E.Suspendability = Suspendability.InsideInterior;
		}
		return false;
	}

	public override bool HandleEvent(GetZoneFreezabilityEvent E)
	{
		if (GameObject.Validate(ref Vehicle) && Vehicle.IsPlayer())
		{
			E.Freezability = Freezability.FormerPlayerObject;
		}
		return false;
	}
}
