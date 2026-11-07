using System;

namespace XRL.World.Parts;

[Serializable]
public class RegionalObject : IPart
{
	public string Table;

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == EnteredCellEvent.ID;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ParentObject.Destroy(null, Silent: true);
		PopulationInfo populationInfo = PopulationManager.ResolvePopulation(Table);
		string terrainRegion = E.Cell.ParentZone.GetTerrainRegion();
		if (!terrainRegion.IsNullOrEmpty() && populationInfo != null)
		{
			if (populationInfo.GroupLookup.TryGetValue(terrainRegion, out var value) || populationInfo.GroupLookup.TryGetValue("Default", out value))
			{
				PopulationResult populationResult = value.GenerateOne();
				E.Cell.AddObject(populationResult.Blueprint, populationResult.Number);
			}
			else
			{
				MetricsManager.LogError("RegionalObject: No matching or default region found in table '" + Table + "' for '" + terrainRegion + "'.");
			}
		}
		return base.HandleEvent(E);
	}
}
