using System;

namespace XRL.World.Parts;

[Serializable]
public class Teleporter : ITeleporter
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsObjectActivePartSubject(E.Actor))
		{
			E.AddAction("Activate", "activate", "ActivateTeleporter", null, 'a', FireOnActor: false, 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateTeleporter" && AttemptTeleport(E.Actor, E))
		{
			E.Actor.UseEnergy(1000, "Item Recoiler");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}
}
