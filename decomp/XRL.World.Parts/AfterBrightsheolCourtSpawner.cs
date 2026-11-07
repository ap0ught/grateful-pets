using System;

namespace XRL.World.Parts;

[Serializable]
public class AfterBrightsheolCourtSpawner : IPart
{
	public bool Spawned;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneActivatedEvent.ID && ID != ZoneBuiltEvent.ID && ID != ZoneThawedEvent.ID)
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
			currentCell.ParentZone.GetCell(26, 14).AddObject("CourtOfTheSultans_ReshapingNook");
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

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		checkSpawn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		checkSpawn();
		return base.HandleEvent(E);
	}
}
