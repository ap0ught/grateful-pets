using System;
using XRL.World.AI;

namespace XRL.World.Effects;

[Serializable]
public class ResummonGloaming : Effect
{
	public string currentZone;

	public ResummonGloaming()
	{
		Duration = 1;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<ResummonGloaming>())
		{
			return false;
		}
		if (Object.CurrentZone != null)
		{
			currentZone = Object.CurrentZone.ZoneID;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Duration > 0 && Object.CurrentZone?.ZoneID != currentZone && !Object.OnWorldMap())
		{
			Cell connectedSpawnLocation = Object.CurrentCell.GetConnectedSpawnLocation();
			if (connectedSpawnLocation != null)
			{
				GameObject gameObject = connectedSpawnLocation.AddObject("Gloaming");
				gameObject.SetAlliedLeader<AllyPet>(Object);
				Duration = 0;
				IComponent<GameObject>.XDidY(gameObject, "reappear");
			}
		}
		return base.HandleEvent(E);
	}
}
