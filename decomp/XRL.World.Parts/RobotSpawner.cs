using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

public class RobotSpawner : IPart
{
	public bool Village;

	public override bool WantEvent(int ID, int cascade)
	{
		return ID == BeforeObjectCreatedEvent.ID;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		List<GameObjectBlueprint> members = Faction.GetMembers("Robots", Village ? ((Predicate<GameObjectBlueprint>)((GameObjectBlueprint x) => !x.HasTag("ExcludeFromVillagePopulations"))) : null);
		E.ReplacementObject = GameObject.Create(members.GetRandomElement());
		return base.HandleEvent(E);
	}
}
