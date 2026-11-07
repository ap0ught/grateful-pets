using System;

namespace XRL.World.Parts;

[Serializable]
public class Medication : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == AfterInventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		int num = 0;
		if (E.Object.HasPart<Empty_Tonic_Applicator>())
		{
			num = -100;
		}
		else if (E.Object.Equipped == E.Actor || E.Object.InInventory == E.Actor)
		{
			num = ((!E.Object.IsImportant()) ? 100 : (-1));
		}
		E.AddAction("Apply", "apply", "Apply", null, 'a', FireOnActor: false, num);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterInventoryActionEvent E)
	{
		if (E.Command == "Apply")
		{
			E.Actor?.UseEnergy(1000, "Item Medication");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}
}
