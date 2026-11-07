using System;

namespace XRL.World.Parts;

[Serializable]
public class AfterEarlSpawner : IPart
{
	public bool Spawned;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneActivatedEvent.ID)
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public void checkSpawn()
	{
		if (!Spawned && (The.Game.HasIntGameState("ForcePostEarlSpawn") || (The.Game.GetQuestFinishTime("The Earl of Omonporch") > 0 && Calendar.TotalTimeTicks - The.Game.GetQuestFinishTime("The Earl of Omonporch") > 16800)))
		{
			Spawned = true;
			currentCell.ParentZone.GetCell(45, 16).AddObject("OmonporchBarathrumiteCamp1");
			currentCell.ParentZone.GetCell(51, 6).AddObject("OmonporchBarathrumiteCamp2");
			currentCell.ParentZone.GetCell(62, 2).AddObject("OmonporchBarathrumiteCamp3");
		}
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		checkSpawn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		checkSpawn();
		return base.HandleEvent(E);
	}
}
