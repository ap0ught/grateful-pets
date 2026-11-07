using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class HindrenMysteryCriticalNPC : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		The.Game.SetStringGameState("HindrenMysteryCriticalNPCKilled", "1");
		if (The.Game.HasQuest("Kith and Kin") && !The.Game.FinishedQuest("Kith and Kin"))
		{
			Popup.Show("The death of " + ParentObject.BaseDisplayName + " means that the investigation can go no further.");
			The.Game.FailQuest("Kith and Kin");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
