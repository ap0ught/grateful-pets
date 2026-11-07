using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

public class VillageUnder : ZoneBuilderSandbox
{
	public string VillageEntityID;

	[NonSerialized]
	private HistoricEntity _VillageEntity;

	public HistoricEntity villageEntity
	{
		get
		{
			if (_VillageEntity == null && !VillageEntityID.IsNullOrEmpty())
			{
				_VillageEntity = The.Game.sultanHistory.GetEntity(VillageEntityID);
			}
			return _VillageEntity;
		}
		set
		{
			_VillageEntity = value;
			VillageEntityID = value?.id;
		}
	}

	public bool BuildZone(Zone Z)
	{
		zone = Z;
		Z.SetZoneProperty("DisableForcedConnections", "Yes");
		Z.SetZoneProperty("relaxedbiomes", "true");
		List<Location2D> list = new List<Location2D>();
		foreach (Cell cell in Z.GetCells())
		{
			cell.ClearObjectsWithPart("Combat");
		}
		string blueprint = Z.GetAnyObjectWithTag("Wall")?.Blueprint ?? "Shale";
		List<string> list2 = new List<string>();
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-" && item.Type.StartsWith("burrow"))
			{
				list.Add(Location2D.Get(item.X, item.Y));
				string[] array = item.Type.Split(',');
				if (array.Length > 1 && !string.IsNullOrEmpty(array[1]))
				{
					list2.Add(array[1]);
				}
				else
				{
					list2.Add("Villages_BuildingContents_Dwelling_*Default");
				}
			}
		}
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (zoneConnection.Type == "burrow")
			{
				list.Add(Location2D.Get(zoneConnection.X, zoneConnection.Y));
				string[] array2 = zoneConnection.Type.Split(',');
				if (array2.Length > 1 && !string.IsNullOrEmpty(array2[1]))
				{
					list2.Add(array2[1]);
				}
				else
				{
					list2.Add("Villages_BuildingContents_Dwelling_*Default");
				}
			}
		}
		if (list.Count > 0)
		{
			List<Box> list3 = new List<Box>();
			foreach (Location2D item2 in list)
			{
				int num = Stat.Random(4, 6);
				Box box = new Box(item2.X - num, item2.Y - num, item2.X + num, item2.Y + num).clamp(1, 1, 78, 23);
				list3.Add(box);
				Z.ClearBox(box);
			}
			InfluenceMap influenceMap = new InfluenceMap(Z.Width, Z.Height);
			Z.SetInfluenceMapWalls(influenceMap.Walls);
			foreach (Box item3 in list3)
			{
				influenceMap.AddSeed(item3.center, bRecalculate: false);
			}
			influenceMap.Recalculate();
			influenceMap.SeedAllUnseeded();
			while (influenceMap.LargestSeed() > 100)
			{
				influenceMap.AddSeedAtMaxima();
			}
			for (int i = 0; i < list3.Count; i++)
			{
				foreach (Location2D item4 in influenceMap.Regions[i].getBorder(2))
				{
					Z.GetCell(item4).AddObject(blueprint);
				}
				List<PopulationResult> list4 = PopulationManager.Generate(list2[i]);
				PopulationLayout populationLayout = new PopulationLayout(zone, influenceMap.Regions[i], list3[i].rect, list[i]);
				populationLayout.inside.AddRange(influenceMap.Regions[i].reducyBy(2));
				populationLayout.insideWall.AddRange(influenceMap.Regions[i].reducyBy(2).Except(influenceMap.Regions[i].reducyBy(3)));
				populationLayout.insideCorner.AddRange(influenceMap.Regions[i].reducyBy(2).Except(influenceMap.Regions[i].reducyBy(3)));
				foreach (PopulationResult item5 in list4)
				{
					PlaceObjectInBuilding(GameObject.Create(item5.Blueprint), populationLayout);
				}
				Z.GetCell(list[i]).AddObject("StairsUp").SetIntProperty("IdleStairs", 1);
			}
		}
		HistoricEntitySnapshot currentSnapshot = villageEntity.GetCurrentSnapshot();
		string damageChance = ((currentSnapshot.GetProperty("abandoned") == "true") ? Stat.Random(5, 25).ToString() : (10 - currentSnapshot.TechTier).ToString());
		PowerGrid powerGrid = new PowerGrid();
		powerGrid.DamageChance = damageChance;
		if ((10 + currentSnapshot.TechTier * 3).in100())
		{
			powerGrid.MissingConsumers = "1d6";
			powerGrid.MissingProducers = "1d3";
		}
		powerGrid.BuildZone(Z);
		Hydraulics hydraulics = new Hydraulics();
		hydraulics.DamageChance = damageChance;
		if ((10 + currentSnapshot.TechTier * 3).in100())
		{
			hydraulics.MissingConsumers = "1d6";
			hydraulics.MissingProducers = "1d3";
		}
		hydraulics.BuildZone(Z);
		MechanicalPower mechanicalPower = new MechanicalPower();
		mechanicalPower.DamageChance = damageChance;
		if ((20 - currentSnapshot.TechTier).in100())
		{
			mechanicalPower.MissingConsumers = "1d6";
			mechanicalPower.MissingProducers = "1d3";
		}
		mechanicalPower.BuildZone(Z);
		return true;
	}
}
