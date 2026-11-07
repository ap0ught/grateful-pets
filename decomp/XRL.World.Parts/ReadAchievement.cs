using System;

namespace XRL.World.Parts;

[Serializable]
public class ReadAchievement : IPart
{
	public bool Triggered;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			if (ID == InventoryActionEvent.ID)
			{
				return !Triggered;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read")
		{
			Achievement.STAT_BOOKS_READ.Increment();
			Triggered = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("scholarship", 2);
		}
		return base.HandleEvent(E);
	}
}
