using System;

namespace XRL.World.Parts;

[Serializable]
public class AfterBrightsheolStudySpawner : IPart
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
		if (!Spawned && The.Game.HasBooleanGameState("Recame"))
		{
			Spawned = true;
			currentCell.ParentZone.GetCell(18, 6).AddObject("BarathrumsStudy_Klanq");
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
