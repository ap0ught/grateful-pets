using System;
using System.Collections.Generic;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class EitherOrPetSpawner : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public void spawn(Cell c, bool either)
	{
		GameObject gameObject = null;
		if (either)
		{
			gameObject = GameObject.Create("EitherPet");
		}
		if (!either)
		{
			gameObject = GameObject.Create("OrPet");
		}
		gameObject.Render.Tile = IComponent<GameObject>.ThePlayer.Render.Tile;
		gameObject.SetAlliedLeader<AllyPet>(IComponent<GameObject>.ThePlayer);
		gameObject.SetActive();
		c.AddObject(gameObject);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && IComponent<GameObject>.ThePlayer != null && IComponent<GameObject>.ThePlayer.CurrentCell != null)
		{
			List<Cell> list = new List<Cell>();
			IComponent<GameObject>.ThePlayer.CurrentCell.GetConnectedSpawnLocations(2, list);
			if (list.Count >= 2)
			{
				spawn(list[0], either: true);
				spawn(list[1], either: false);
				ParentObject.Destroy();
			}
		}
		return base.FireEvent(E);
	}
}
