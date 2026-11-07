using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Rules;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class SultanDungeonArgs : IComposite
{
	public static Dictionary<string, string> sultanArgumentClusters = new Dictionary<string, string>
	{
		{ "scholarship", "Scholarship" },
		{ "scholar", "Scholarship" },
		{ "academy", "Scholarship" },
		{ "scribe", "Scholarship" },
		{ "scriptorium", "Scholarship" },
		{ "philosopher", "Scholarship" },
		{ "college", "Scholarship" },
		{ "historian", "Scholarship" },
		{ "scientist", "Scholarship" },
		{ "laboratory", "Scholarship" },
		{ "tinker", "Circuitry" },
		{ "electrician", "Circuitry" },
		{ "circuitry", "Circuitry" },
		{ "travel", "Travel" },
		{ "explorer", "Travel" },
		{ "nomad", "Travel" },
		{ "might", "Warrior" },
		{ "soldier", "Warrior" },
		{ "barracks", "Warrior" },
		{ "arena", "Warrior" },
		{ "gladiator", "Warrior" },
		{ "stars", "Stars" },
		{ "astrologist", "Stars" },
		{ "observatory", "Stars" },
		{ "astronomer", "Stars" },
		{ "stargazer", "Stars" },
		{ "light", "Stars" },
		{ "glass", "Glass" },
		{ "window maker", "Glass" },
		{ "glassblower", "Glass" },
		{ "jewels", "Jewels" },
		{ "jeweler", "Jewels" },
		{ "geologist", "Jewels" },
		{ "dig site", "Jewels" },
		{ "time", "Time" },
		{ "salt", "Salt" },
		{ "cook", "Salt" },
		{ "tavern", "Salt" },
		{ "ice", "Ice" },
		{ "winter eremite", "Ice" },
		{ "hermitage", "Ice" },
		{ "chance", "Chance" },
		{ "gambler", "Chance" },
		{ "gambling hall", "Chance" },
		{ "gardens", "Plants" },
		{ "pools", "Liquids" },
		{ "museums", "Art" },
		{ "theaters", "Art" },
		{ "waste", "Waste" },
		{ "consulate", "Consulate" },
		{ "market", "Market" },
		{ "forums", "Forums" },
		{ "middle-class", "Residential" },
		{ "residential", "Residential" },
		{ "food storage", "Storage" },
		{ "pipe hub", "Pipes" },
		{ "prison", "Prison" },
		{ "temple", "Temple" },
		{ "city-state", "City-States" },
		{ "precinct", "Precincts" },
		{ "province", "Provinces" },
		{ "district", "Districts" },
		{ "districts", "Districts" },
		{ "quarter", "Quarters" },
		{ "quarters", "Quarters" },
		{ "poor", "Poor" },
		{ "rich", "Rich" },
		{ "godless", "Godless" },
		{ "sultans", "Sultans" }
	};

	public List<string> wallTypes = new List<string>();

	public List<string> primaryTemplate = new List<string>();

	public int primaryTemplateN = 3;

	public List<string> secondaryTemplate = new List<string>();

	public int secondaryTemplateN = 3;

	public List<string> detailTemplate = new List<string>();

	public int detailTemplateN = 3;

	public List<string> greenObjects = new List<string>();

	public List<string> yellowObjects = new List<string>();

	public List<string> blueObjects = new List<string>();

	public List<string> segmentations = new List<string>();

	public List<string> preconnectencounters = new List<string>();

	public List<string> furnishings = new List<string>();

	public List<string> encounters = new List<string>();

	public List<string> halls = new List<string>();

	public List<string> cubbies = new List<string>();

	public Dictionary<string, List<string>> additionalArgs = new Dictionary<string, List<string>>();

	public List<string> cultFactions = new List<string>();

	public List<string> enemyFactions = new List<string>();

	public List<string> properties = new List<string>();

	public string cultName;

	public string cultPeriod;

	public string cultFaction;

	public string tablePrefix = "SultanDungeons_";

	public static string translateArg(string input)
	{
		if (sultanArgumentClusters.ContainsKey(input))
		{
			return sultanArgumentClusters[input];
		}
		return input;
	}

	public void UpdateWalls(int Period, string tablePrefix = "SultanDungeons_")
	{
		wallTypes.Add(PopulationManager.RollOneFrom(tablePrefix + "Wall_Default_Period" + Period).Blueprint);
	}

	public void UpdateFromEntity(HistoricEntitySnapshot snap, string tablePrefix = "SultanDungeons_")
	{
		this.tablePrefix = tablePrefix;
		properties = new List<string>();
		if (snap.properties.ContainsKey("cultName"))
		{
			cultName = snap.properties["cultName"];
			cultPeriod = snap.properties["period"];
			cultFaction = Faction.GetSultanFactionName(cultPeriod);
			if (snap.listProperties.ContainsKey("likedFactions"))
			{
				cultFactions.AddRange(snap.listProperties["likedFactions"]);
			}
			if (snap.listProperties.ContainsKey("hatedFactions"))
			{
				enemyFactions.AddRange(snap.listProperties["hatedFactions"]);
			}
		}
		else
		{
			foreach (string value in snap.properties.Values)
			{
				string item = translateArg(value.ToLower());
				if (!properties.Contains(item))
				{
					properties.Add(item);
				}
			}
			foreach (List<string> value2 in snap.listProperties.Values)
			{
				foreach (string item3 in value2)
				{
					string item2 = translateArg(item3.ToLower());
					if (!properties.Contains(item2))
					{
						properties.Add(item2);
					}
				}
			}
			foreach (string property in properties)
			{
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "Wall_" + property))
				{
					wallTypes.Add(PopulationManager.RollOneFrom(tablePrefix + "Wall_" + property, null, "Fulcrete").Blueprint);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "PrimaryTemplate_" + property))
				{
					primaryTemplate.Add(PopulationManager.RollOneFrom(tablePrefix + "PrimaryTemplate_" + property, null, "smalltemple,W").Blueprint);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "SecondaryTemplate_" + property))
				{
					secondaryTemplate.Add(PopulationManager.RollOneFrom(tablePrefix + "SecondaryTemplate_" + property, null, "gardens").Blueprint);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "DetailTemplate_" + property))
				{
					detailTemplate.Add(PopulationManager.RollOneFrom(tablePrefix + "DetailTemplate_" + property, null, "gardens").Blueprint);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "Green_" + property))
				{
					greenObjects.Add(tablePrefix + "Green_" + property);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "Blue_" + property))
				{
					blueObjects.Add(tablePrefix + "Blue_" + property);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "Yellow_" + property))
				{
					yellowObjects.Add(tablePrefix + "Yellow_" + property);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "Segmentations_" + property))
				{
					segmentations.Add(tablePrefix + "Segmentations_" + property);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "Furnishings_" + property))
				{
					furnishings.Add(tablePrefix + "Furnishings_" + property);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "Halls_" + property))
				{
					halls.Add(tablePrefix + "Halls_" + property);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "Cubbies_" + property))
				{
					cubbies.Add(tablePrefix + "Cubbies_" + property);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "Encounters_" + property))
				{
					encounters.Add(tablePrefix + "Encounters_" + property);
				}
				if (PopulationManager.Populations.ContainsKey(tablePrefix + "PreconnectEncounters_" + property))
				{
					preconnectencounters.Add(tablePrefix + "PreconnectEncounters_" + property);
				}
			}
			snap.listProperties.ContainsKey("battles");
		}
		if (wallTypes.Count == 0)
		{
			wallTypes.Add(PopulationManager.RollOneFrom(tablePrefix + "Wall_*Default", null, "Fulcrete").Blueprint);
		}
		if (primaryTemplate.Count == 0)
		{
			int num = Stat.Random(1, 2);
			for (int i = 0; i < num; i++)
			{
				primaryTemplate.Add(PopulationManager.RollOneFrom(tablePrefix + "PrimaryTemplate_*Default", null, "smalltemple,W").Blueprint);
			}
		}
		if (detailTemplate.Count == 0)
		{
			int num2 = Stat.Random(1, 2);
			for (int j = 0; j < num2; j++)
			{
				detailTemplate.Add(PopulationManager.RollOneFrom(tablePrefix + "SecondaryTemplate_*Default", null, "gardens").Blueprint);
			}
		}
		if (secondaryTemplate.Count == 0)
		{
			int num3 = Stat.Random(1, 2);
			for (int k = 0; k < num3; k++)
			{
				secondaryTemplate.Add(PopulationManager.RollOneFrom(tablePrefix + "SecondaryTemplate_*Default", null, "gardens").Blueprint);
			}
		}
		if (greenObjects.Count == 0)
		{
			greenObjects.Add(tablePrefix + "Green_*Default");
		}
		if (yellowObjects.Count == 0)
		{
			yellowObjects.Add(tablePrefix + "Yellow_*Default");
		}
		if (blueObjects.Count == 0)
		{
			blueObjects.Add(tablePrefix + "Blue_*Default");
		}
		if (preconnectencounters.Count == 0)
		{
			preconnectencounters.Add(tablePrefix + "PreconnectEncounters_*Default");
		}
		if (encounters.Count == 0)
		{
			encounters.Add(tablePrefix + "Encounters_*Default");
		}
		if (furnishings.Count == 0)
		{
			furnishings.Add(tablePrefix + "Furnishings_*Default");
		}
		if (cubbies.Count == 0)
		{
			cubbies.Add(tablePrefix + "Cubbies_*Default");
		}
		if (halls.Count == 0)
		{
			halls.Add(tablePrefix + "Halls_*Default");
		}
	}

	public void Mutate()
	{
		if (Stat.Random(1, 100) <= 25)
		{
			primaryTemplate.Clear();
			primaryTemplate.Add(PopulationManager.RollOneFrom(tablePrefix + "PrimaryTemplate_*Default", null, "smalltemple,W").Blueprint);
		}
		if (Stat.Random(1, 100) <= 25)
		{
			secondaryTemplate.Clear();
			secondaryTemplate.Add(PopulationManager.RollOneFrom(tablePrefix + "SecondaryTemplate_*Default", null, "gardens").Blueprint);
		}
	}

	public SultanDungeonArgs clone()
	{
		SultanDungeonArgs sultanDungeonArgs = new SultanDungeonArgs();
		sultanDungeonArgs.wallTypes.AddRange(wallTypes);
		sultanDungeonArgs.primaryTemplate.AddRange(primaryTemplate);
		sultanDungeonArgs.primaryTemplateN = primaryTemplateN;
		sultanDungeonArgs.secondaryTemplate.AddRange(secondaryTemplate);
		sultanDungeonArgs.secondaryTemplateN = secondaryTemplateN;
		sultanDungeonArgs.detailTemplate.AddRange(detailTemplate);
		sultanDungeonArgs.detailTemplateN = detailTemplateN;
		sultanDungeonArgs.greenObjects.AddRange(greenObjects);
		sultanDungeonArgs.yellowObjects.AddRange(yellowObjects);
		sultanDungeonArgs.blueObjects.AddRange(blueObjects);
		sultanDungeonArgs.segmentations.AddRange(segmentations);
		sultanDungeonArgs.furnishings.AddRange(furnishings);
		sultanDungeonArgs.encounters.AddRange(encounters);
		sultanDungeonArgs.preconnectencounters.AddRange(preconnectencounters);
		sultanDungeonArgs.halls.AddRange(halls);
		sultanDungeonArgs.cubbies.AddRange(cubbies);
		sultanDungeonArgs.cultPeriod = cultPeriod;
		sultanDungeonArgs.cultName = cultName;
		sultanDungeonArgs.cultFaction = cultFaction;
		sultanDungeonArgs.cultFactions.AddRange(cultFactions);
		sultanDungeonArgs.enemyFactions.AddRange(enemyFactions);
		foreach (string key in additionalArgs.Keys)
		{
			sultanDungeonArgs.additionalArgs.Add(key, new List<string>());
			sultanDungeonArgs.additionalArgs[key].AddRange(additionalArgs[key]);
		}
		return sultanDungeonArgs;
	}
}
