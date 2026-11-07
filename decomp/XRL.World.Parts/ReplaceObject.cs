using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ReplaceObject : IPart
{
	public string Table;

	public bool SeedWithZoneID = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		if (!Table.IsNullOrEmpty())
		{
			if (SeedWithZoneID && !ZoneManager.zoneGenerationContextZoneID.IsNullOrEmpty())
			{
				List<string> each = PopulationManager.GetEach(Table);
				int index = Stat.SeededRandom(ZoneManager.zoneGenerationContextZoneID, 0, each.Count - 1);
				E.ReplacementObject = GameObject.Create(each[index]);
			}
			else
			{
				PopulationResult populationResult = PopulationManager.GenerateOne(Table);
				E.ReplacementObject = GameObject.Create(populationResult.Blueprint);
			}
		}
		return base.HandleEvent(E);
	}
}
