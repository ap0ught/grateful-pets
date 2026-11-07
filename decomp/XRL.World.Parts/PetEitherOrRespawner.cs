using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class PetEitherOrRespawner : IPart
{
	public bool respawnEither;

	public bool respawnOr;

	public string lastZone = "";

	public override IPart DeepCopy(GameObject Parent)
	{
		PetEitherOrRespawner petEitherOrRespawner = (PetEitherOrRespawner)base.DeepCopy(Parent);
		petEitherOrRespawner.respawnEither = false;
		petEitherOrRespawner.respawnOr = false;
		return petEitherOrRespawner;
	}

	public override bool AllowStaticRegistration()
	{
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
		if ((respawnEither || respawnOr) && ParentObject.IsPlayer())
		{
			Respawn(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public void Respawn(Cell Cell)
	{
		Zone parentZone = Cell.ParentZone;
		if (lastZone != "" && parentZone.ZoneID != lastZone && !parentZone.IsWorldMap())
		{
			if (respawnEither)
			{
				Cell connectedSpawnLocation = Cell.GetConnectedSpawnLocation();
				if (connectedSpawnLocation != null)
				{
					GameObject gameObject = connectedSpawnLocation.AddObject("EitherPet");
					gameObject.SetActive();
					gameObject.SetAlliedLeader<AllyPet>(The.Player);
					gameObject.IsTrifling = true;
					respawnEither = false;
				}
			}
			if (respawnOr)
			{
				Cell connectedSpawnLocation2 = ParentObject.CurrentCell.GetConnectedSpawnLocation();
				if (connectedSpawnLocation2 != null)
				{
					GameObject gameObject2 = connectedSpawnLocation2.AddObject("OrPet");
					gameObject2.SetActive();
					gameObject2.SetAlliedLeader<AllyPet>(The.Player);
					gameObject2.IsTrifling = true;
					respawnOr = false;
				}
			}
		}
		lastZone = parentZone.ZoneID;
	}
}
