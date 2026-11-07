using System;

namespace XRL.World.Capabilities;

public static class SmartUse
{
	[NonSerialized]
	private static Event eCanSmartUse = new Event("CanSmartUse", "User", (object)null);

	[NonSerialized]
	private static Event ePreventSmartUse = new Event("PreventSmartUse", "User", (object)null);

	[NonSerialized]
	private static Event eCommandSmartUseEarly = new Event("CommandSmartUseEarly", "User", (object)null);

	[NonSerialized]
	private static Event eCommandSmartUse = new Event("CommandSmartUse", "User", (object)null);

	public static GameObject FindSmartUseObject(Cell TargetCell, GameObject who)
	{
		return TargetCell.GetHighestRenderLayerInteractableObjectFor(who, (GameObject o) => CanSmartUse(o, who));
	}

	public static GameObject FindPlayerSmartUseObject(Cell TargetCell, int MinPriority = 0)
	{
		return TargetCell.GetHighestRenderLayerInteractableObjectFor(The.Player, (GameObject o) => CanPlayerSmartUse(o, MinPriority));
	}

	public static bool CanSmartUse(GameObject GO, GameObject Actor, int MinPriority = 0)
	{
		if (GO == Actor || GO.HasTag("NoSmartUse"))
		{
			return false;
		}
		if (GO.Render != null && !GO.Render.Visible)
		{
			return false;
		}
		if (GO.HasTag("ForceSmartUse"))
		{
			return true;
		}
		bool flag = false;
		if (GO.HasRegisteredEvent(eCanSmartUse.ID))
		{
			eCanSmartUse.SetParameter("User", Actor);
			eCanSmartUse.SetParameter("MinPriority", MinPriority);
			if (!GO.FireEvent(eCanSmartUse))
			{
				flag = true;
			}
		}
		bool flag2 = false;
		if (GO.WantEvent(CanSmartUseEvent.ID, MinEvent.CascadeLevel) && !GO.HandleEvent(CanSmartUseEvent.FromPool(Actor, GO, MinPriority)))
		{
			flag2 = true;
		}
		if (!flag && !flag2)
		{
			return false;
		}
		if (GO.HasRegisteredEvent(ePreventSmartUse.ID))
		{
			ePreventSmartUse.SetParameter("User", Actor);
			if (!GO.FireEvent(ePreventSmartUse))
			{
				return false;
			}
		}
		if (GO.WantEvent(PreventSmartUseEvent.ID, MinEvent.CascadeLevel) && !GO.HandleEvent(PreventSmartUseEvent.FromPool(Actor, GO)))
		{
			return false;
		}
		Cell currentCell = GO.CurrentCell;
		if (currentCell != null && Actor.CurrentCell != currentCell && !GO.CanInteractInCellWithSolid(Actor) && currentCell.IsSolidFor(Actor))
		{
			return false;
		}
		return true;
	}

	public static bool CanPlayerSmartUse(GameObject GO, int MinPriority = 0)
	{
		return CanSmartUse(GO, The.Player, MinPriority);
	}

	public static bool CanTake(GameObject GO, GameObject Actor)
	{
		if (GO == Actor)
		{
			return false;
		}
		if (GO.Render != null && !GO.Render.Visible)
		{
			return false;
		}
		return GO.IsTakeable();
	}

	public static bool CanPlayerTake(GameObject GO)
	{
		return CanTake(GO, The.Player);
	}

	public static bool PerformSmartUse(GameObject GO, GameObject Actor, int MinPriority = 0)
	{
		if (GO == null)
		{
			return false;
		}
		if (GO.HasRegisteredEvent(eCommandSmartUseEarly.ID))
		{
			eCommandSmartUseEarly.SetParameter("User", Actor);
			if (!GO.FireEvent(eCommandSmartUseEarly))
			{
				return false;
			}
		}
		if (GO.WantEvent(CommandSmartUseEarlyEvent.ID, MinEvent.CascadeLevel) && !GO.HandleEvent(CommandSmartUseEarlyEvent.FromPool(Actor, GO)))
		{
			return false;
		}
		if (GO.HasRegisteredEvent(eCommandSmartUse.ID))
		{
			eCommandSmartUse.SetParameter("User", Actor);
			if (!GO.FireEvent(eCommandSmartUse))
			{
				return false;
			}
		}
		if (GO.WantEvent(CommandSmartUseEvent.ID, MinEvent.CascadeLevel) && !GO.HandleEvent(CommandSmartUseEvent.FromPool(Actor, GO, MinPriority)))
		{
			return false;
		}
		return true;
	}

	public static bool PlayerPerformSmartUse(GameObject GO, int MinPriority = 0)
	{
		return PerformSmartUse(GO, The.Player, MinPriority);
	}

	public static bool PlayerPerformSmartUse(Cell TargetCell)
	{
		return PlayerPerformSmartUse(FindPlayerSmartUseObject(TargetCell));
	}
}
