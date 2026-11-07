using System;
using System.Linq;

namespace XRL.World.Parts;

[Serializable]
public class EskhindSpawner : IPart
{
	public bool spawned;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!spawned && The.Game.HasQuest("Kith and Kin") && !The.Game.HasGameState("EskhindKilled"))
		{
			spawned = true;
			GameObject gameObject = The.ZoneManager.FindObjectsReadonly((GameObject x) => x.Blueprint == "Eskhind").FirstOrDefault();
			if (gameObject == null)
			{
				gameObject = GameObject.Create("Eskhind");
			}
			if (!gameObject.IsPlayer())
			{
				ParentObject.CurrentCell.GetCellOrFirstConnectedSpawnLocation().AddObject(gameObject);
				gameObject.Brain.StartingCell = null;
				gameObject.Brain.Goals.Clear();
			}
			ParentObject.CurrentCell.GetCellOrFirstConnectedSpawnLocation().AddObject("Meyehind");
			ParentObject.CurrentCell.GetCellOrFirstConnectedSpawnLocation().AddObject("Liihart");
		}
		return base.HandleEvent(E);
	}
}
