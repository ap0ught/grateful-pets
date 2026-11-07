using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class GuardSpawner : IPart
{
	public string Faction = "Merchants";

	public int minLevel = 20;

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == BeforeObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		string populationName = "DynamicInheritsTable:Creature:Tier" + Tier.Constrain(ZoneManager.zoneGenerationContextTier + 1);
		GameObjectBlueprint gameObjectBlueprint = null;
		int num = 0;
		while (++num < 10)
		{
			GameObjectBlueprint gameObjectBlueprint2 = GameObjectFactory.Factory.Blueprints[PopulationManager.RollOneFrom(populationName).Blueprint];
			if (HasGuards.IsSuitableGuard(gameObjectBlueprint2))
			{
				gameObjectBlueprint = gameObjectBlueprint2;
				break;
			}
		}
		if (gameObjectBlueprint != null)
		{
			E.ReplacementObject = GameObjectFactory.Factory.CreateObject(gameObjectBlueprint);
			E.ReplacementObject.AddPart(new HiredGuard(null, Faction));
		}
		return base.HandleEvent(E);
	}
}
