using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsAirCurrentMicrosensor : IPart
{
	[NonSerialized]
	public Zone lastZone;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GenericDeepNotifyEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterEvent(this, EnteredCellEvent.ID, 0, Serialize: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterEvent(this, EnteredCellEvent.ID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericDeepNotifyEvent E)
	{
		if (E.Notify == "MemoriesEaten" || E.Notify == "AmnesiaTriggered")
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && E.Subject == implantee && implantee.IsPlayer())
			{
				RevealStairs();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (E.Actor != null && ShouldRevealStairs(E.Actor) && E.Cell.ParentZone != lastZone)
		{
			RevealStairs();
		}
		return base.HandleEvent(E);
	}

	public int RevealStairs()
	{
		int num = 0;
		GameObject implantee = ParentObject.Implantee;
		if (implantee != null && ShouldRevealStairs(implantee))
		{
			lastZone = implantee.CurrentZone;
			if (lastZone != null)
			{
				foreach (Cell cell in lastZone.GetCells())
				{
					if (cell.HasStairs() && !cell.IsExplored())
					{
						cell.SetExplored(State: true);
						num++;
					}
				}
			}
			if (num > 0)
			{
				implantee.PlayWorldSound("Sounds/Interact/sfx_interact_mapDevice_activate");
			}
		}
		return num;
	}

	public static bool ShouldRevealStairs(GameObject POV)
	{
		if (POV.IsPlayer())
		{
			return true;
		}
		if (POV.IsPlayerLed() && POV.IsAudible(The.Player))
		{
			return true;
		}
		return false;
	}
}
