using System;

namespace XRL.World.Parts;

[Serializable]
public class PassiveFactionDeed : IPart
{
	public string Faction = "";

	public string Amount = "500";

	public override bool SameAs(IPart p)
	{
		return false;
	}

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
		E.AddAction("Activate", "activate", "ActivatePassiveFactionDeed", null, 'a');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivatePassiveFactionDeed" && string.IsNullOrEmpty(Faction))
		{
			The.Game.PlayerReputation.Modify(Faction, Amount.RollCached(), "PassiveFactionDeed");
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
