using System;
using XRL.World.Parts;

namespace XRL.World.Units;

[Serializable]
public class GameObjectCloneUnit : GameObjectUnit
{
	public override void Apply(GameObject Object)
	{
		CloneOnPlace cloneOnPlace = Object.RequirePart<CloneOnPlace>();
		cloneOnPlace.Amount++;
		cloneOnPlace.Context = "Unit";
		cloneOnPlace.Force = true;
	}

	public override string GetDescription(bool Inscription = false)
	{
		return "Spawns with a copy in a nearby cell";
	}
}
