using System;

namespace XRL.World.Parts;

[Serializable]
public class HostileTurretBuilder : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<DidInitialEquipEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(DidInitialEquipEvent E)
	{
		ParentObject.FireEventOnBodyparts(Event.New("PrepIntegratedHostToReceiveAmmo", "Host", ParentObject));
		ParentObject.FireEventOnBodyparts(Event.New("GenerateIntegratedHostInitialAmmo", "Host", ParentObject));
		CommandReloadEvent.Execute(ParentObject, FreeAction: true);
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}
