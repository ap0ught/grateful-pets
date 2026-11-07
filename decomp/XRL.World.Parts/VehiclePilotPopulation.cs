using System;

namespace XRL.World.Parts;

[Serializable]
public class VehiclePilotPopulation : IPart
{
	public string Blueprint;

	public string Table;

	public GameObject Object;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDieEvent.ID && ID != ObjectCreatedEvent.ID && ID != PooledEvent<InteriorZoneBuiltEvent>.ID)
		{
			return ID == SingletonEvent<FindObjectByIdEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDieEvent E)
	{
		if (Object != null && ParentObject.TryGetPart<Interior>(out var Part) && Part.Zone == null && ParentObject.TryGetPart<Vehicle>(out var Part2) && Part2.Pilot == null)
		{
			ParentObject.CurrentCell.AddObject(Object);
			Object = null;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (ParentObject.TryGetPart<Vehicle>(out var Part))
		{
			if (!Blueprint.IsNullOrEmpty())
			{
				Object = GameObject.Create(Blueprint.GetRandomSubstring(','));
			}
			else if (!Table.IsNullOrEmpty())
			{
				Object = PopulationManager.CreateOneFrom(Table);
			}
			if (GameObject.Validate(ref Object))
			{
				if (!Part.CanBePilotedBy(Object))
				{
					Part.OwnerID = Object.ID;
				}
				Part.Pilot = Object;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InteriorZoneBuiltEvent E)
	{
		GameObject firstObjectWithPart = E.Zone.GetFirstObjectWithPart("VehicleSeat");
		if (firstObjectWithPart != null && GameObject.Validate(ref Object))
		{
			firstObjectWithPart.CurrentCell.AddObject(Object);
			firstObjectWithPart.GetPart<Chair>()?.SitDown(Object, E);
			Object = null;
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(FindObjectByIdEvent E)
	{
		if (GameObject.Validate(ref Object) && Object.IDMatch(E.FindID))
		{
			E.Object = Object;
			return false;
		}
		return base.HandleEvent(E);
	}
}
