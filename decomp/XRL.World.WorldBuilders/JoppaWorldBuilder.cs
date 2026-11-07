using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using Qud.API;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Annals;
using XRL.Core;
using XRL.EditorFormats.Map;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.World.Biomes;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.ZoneBuilders;

namespace XRL.World.WorldBuilders;

public class JoppaWorldBuilder : WorldBuilder
{
	public const string ID_JOPPA = "JoppaWorld.11.22.1.1.10";

	public const string ID_GRIT_GATE = "JoppaWorld.22.14.1.0.13";

	public const string ID_EZRA = "JoppaWorld.53.4.0.0.10";

	public const string ID_SPINDLE = "JoppaWorld.53.3.1.1.10";

	public const string ID_GOLGOTHA = "JoppaWorld.23.9.1.1.10";

	public const string ID_BETHESDA = "JoppaWorld.25.3.1.1.10";

	public const string ID_TEMPLE_ROCK = "JoppaWorld.25.3.1.1.26";

	public const string ID_COURT = "JoppaWorld.53.4.1.1.10";

	public MutabilityMap mutableMap = new MutabilityMap();

	public Dictionary<Location2D, string> terrainTypes = new Dictionary<Location2D, string>();

	public Dictionary<Location2D, TerrainTravel> terrainComponents = new Dictionary<Location2D, TerrainTravel>();

	public WorldInfo worldInfo;

	public Zone WorldZone;

	public List<IJoppaWorldBuilderExtension> extensions = new List<IJoppaWorldBuilderExtension>();

	private string World = "JoppaWorld";

	public int[,] Lairs;

	public static uint ROAD_NORTH = 1u;

	public static uint ROAD_SOUTH = 2u;

	public static uint ROAD_EAST = 4u;

	public static uint ROAD_WEST = 8u;

	public static uint ROAD_NONE = 16u;

	public static uint ROAD_START = 32u;

	public uint RIVER_NORTH = 1u;

	public uint RIVER_SOUTH = 2u;

	public uint RIVER_EAST = 4u;

	public uint RIVER_WEST = 8u;

	public uint RIVER_NONE = 16u;

	public uint RIVER_START = 32u;

	public uint[,] RoadSystem
	{
		get
		{
			return worldInfo.RoadSystem;
		}
		set
		{
			worldInfo.RoadSystem = value;
		}
	}

	public uint[,] RiverSystem
	{
		get
		{
			return worldInfo.RiverSystem;
		}
		set
		{
			worldInfo.RiverSystem = value;
		}
	}

	public void BuildMazes()
	{
		Maze maze = RecursiveBacktrackerMaze.Generate(240, 75, bShow: false, The.Game.GetWorldSeed("CanyonMaze"));
		maze.SetBorder(Value: true);
		The.Game.WorldMazes.Add("QudCanyonMaze", maze);
		Maze maze2 = RecursiveBacktrackerMaze.Generate(80, 25, bShow: false, The.Game.GetWorldSeed("WaterwayMaze"));
		maze2.Cell[25, 3].N = true;
		maze2.Cell[25, 3].S = true;
		maze2.Cell[25, 3].E = false;
		maze2.Cell[25, 3].W = false;
		maze2.Cell[24, 3].E = false;
		maze2.Cell[26, 3].W = false;
		maze2.Cell[25, 2].S = true;
		maze2.Cell[25, 4].N = true;
		The.Game.WorldMazes.Add("QudWaterwayMaze", maze2);
	}

	public bool WorldCellHasTerrain(string World, int x, int y, string[] T)
	{
		WorldZone = The.ZoneManager.GetZone(World);
		GameObject firstObjectWithPart = WorldZone.GetCell(x, y).GetFirstObjectWithPart("TerrainTravel");
		if (firstObjectWithPart != null)
		{
			GameObjectBlueprint blueprint = firstObjectWithPart.GetBlueprint();
			for (int i = 0; i < T.Length; i++)
			{
				if (blueprint.DescendsFrom(T[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void BuildMutableEncounters()
	{
		try
		{
			History sultanHistory = The.Game.sultanHistory;
			MetricsManager.rngCheckpoint("init " + World);
			mutableMap = new MutabilityMap();
			mutableMap.Init(240, 75);
			terrainComponents = new Dictionary<Location2D, TerrainTravel>();
			terrainTypes = new Dictionary<Location2D, string>();
			WorldZone = The.ZoneManager.GetZone(World);
			extensions.ForEach((IJoppaWorldBuilderExtension e) =>
			{
				e.OnBeforeMutableInit(this);
			});
			for (int num = 0; num < 25; num++)
			{
				for (int num2 = 0; num2 < 80; num2++)
				{
					Event.ResetPool();
					string zoneID = World + "." + num2 + "." + num + ".";
					List<CellBlueprint> cellBlueprints = The.ZoneManager.GetCellBlueprints(zoneID);
					int value = 1;
					string text = "None";
					int num3 = 1;
					TerrainTravel terrainTravel = null;
					GameObject firstObjectWithPart = WorldZone.GetCell(num2, num).GetFirstObjectWithPart("TerrainTravel");
					if (firstObjectWithPart != null)
					{
						terrainTravel = firstObjectWithPart.GetPart<TerrainTravel>();
						text = firstObjectWithPart.GetTag("Terrain", firstObjectWithPart.Blueprint);
						num3 = int.Parse(firstObjectWithPart.GetTag("RegionTier", "1"));
						if (terrainTravel == null)
						{
							MetricsManager.LogError($"Terrain object {firstObjectWithPart.Blueprint} is missing the TerrainTravel part in world map cell [{num2},{num}]");
						}
					}
					else
					{
						MetricsManager.LogError($"Missing Terrain object in world map cell [{num2},{num}]");
						value = 0;
					}
					foreach (CellBlueprint item in cellBlueprints)
					{
						if (!item.Mutable)
						{
							value = 0;
						}
						else if (firstObjectWithPart != null && firstObjectWithPart.GetBlueprint().DescendsFrom("TerrainWater"))
						{
							value = 1;
						}
					}
					int key = num3;
					terrainTypes.Add(Location2D.Get(num2, num), text);
					terrainComponents.Add(Location2D.Get(num2, num), terrainTravel);
					if (!worldInfo.terrainLocations.ContainsKey(text))
					{
						worldInfo.terrainLocations.Add(text, new List<Location2D>());
					}
					worldInfo.terrainLocations[text].Add(Location2D.Get(num2, num));
					if (!worldInfo.tierLocations.ContainsKey(key))
					{
						worldInfo.tierLocations.Add(key, new List<Location2D>());
					}
					worldInfo.tierLocations[key].Add(Location2D.Get(num2, num));
					for (int num4 = 0; num4 < 3; num4++)
					{
						for (int num5 = 0; num5 < 3; num5++)
						{
							int x = num2 * 3 + num4;
							int y = num * 3 + num5;
							mutableMap.AddMutableLocation(Location2D.Get(x, y), text, value);
							mutableMap.SetMutable(Location2D.Get(x, y), value);
						}
					}
				}
			}
			mutableMap.RemoveMutableLocation("JoppaWorld.53.4.1.0.10");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.4.1.0.11");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.4.0.0.11");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.4.0.0.10");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.3.1.2.10");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.3.2.2.10");
			mutableMap.RemoveMutableLocation("JoppaWorld.53.3.2.0.10");
			extensions.ForEach((IJoppaWorldBuilderExtension e) =>
			{
				e.OnAfterMutableInit(this);
			});
			RiverSystem = new uint[240, 75];
			MetricsManager.rngCheckpoint("mamon");
			if (World == "JoppaWorld")
			{
				AddMamonVillage();
				AddYonderPath(World);
			}
			MetricsManager.rngCheckpoint("biomes");
			for (int num6 = 0; num6 < 80; num6++)
			{
				if (num6 % 2 == 0)
				{
					WorldCreationProgress.StepProgress("Generating lairs...");
				}
				for (int num7 = 0; num7 < 25; num7++)
				{
					string text2 = World + "." + num6 + "." + num7 + ".";
					string a = terrainTypes[Location2D.Get(num6, num7)];
					TerrainTravel terrainTravel2 = terrainComponents[Location2D.Get(num6, num7)];
					for (int num8 = 0; num8 < 3; num8++)
					{
						for (int num9 = 0; num9 < 3; num9++)
						{
							string text3 = text2 + num8 + "." + num9 + ".10";
							The.ZoneManager.AdjustZoneGenerationTierTo(text3);
							if (Options.ShowOverlandRegions)
							{
								if (BiomeManager.Biomes["Slimy"].GetBiomeValue(text3) > 0)
								{
									MarkCell("JoppaWorld", num6, num7, BiomeManager.Biomes["Slimy"].GetBiomeValue(text3).ToString());
								}
								if (BiomeManager.Biomes["Tarry"].GetBiomeValue(text3) > 0)
								{
									MarkCell("JoppaWorld", num6, num7, BiomeManager.Biomes["Tarry"].GetBiomeValue(text3).ToString());
								}
								if (BiomeManager.Biomes["Rusty"].GetBiomeValue(text3) > 0)
								{
									MarkCell("JoppaWorld", num6, num7, BiomeManager.Biomes["Rusty"].GetBiomeValue(text3).ToString());
								}
								if (BiomeManager.Biomes["Fungal"].GetBiomeValue(text3) > 0)
								{
									MarkCell("JoppaWorld", num6, num7, BiomeManager.Biomes["Fungal"].GetBiomeValue(text3).ToString());
								}
							}
							if (!(World == "JoppaWorld") || mutableMap.GetMutable(num6 * 3 + num8, num7 * 3 + num9) != 1)
							{
								continue;
							}
							if (3.in100())
							{
								if (5.in100())
								{
									The.ZoneManager.SetZoneProperty(text3, "ambient_bed_3", "Sounds/Ambiences/amb_bed_ruins");
									The.ZoneManager.AddZoneMidBuilder(text3, "OverlandRuins");
									The.ZoneManager.AddZoneMidBuilder(text3, "InsertPresetFromPopulation", "Population", "CyberStations", "Important", true);
									string name = QudHistoryFactory.NameRuinsSite(sultanHistory, out var Proper, out var nameRoot);
									The.ZoneManager.SetZoneName(text3, name, null, null, null, null, Proper);
									string text4 = AddSecret(text3, name, new string[3] { "ruins", "tech", "cybernetics" }, "Ruins with Becoming Nooks");
									game.SetStringGameState(text4 + "_NameRoot", nameRoot);
									The.ZoneManager.SetZoneProperty(text3, "TeleportGateCandidateNameRoot", nameRoot);
									terrainTravel2.AddEncounter(new EncounterEntry("You notice some ruins nearby. Would you like to investigate?", text3, "", text4, Optional: true));
									AddLocationFinder(text3, text4);
									if (Options.ShowOverlandEncounters)
									{
										The.ZoneManager.GetZone("JoppaWorld").GetCell(num6, num7).GetObjectInCell(0)
											.Render.RenderString = "#";
									}
									GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
									generatedLocationInfo.name = name;
									generatedLocationInfo.targetZone = text3;
									generatedLocationInfo.zoneLocation = Location2D.Get(num6 * 3 + num8, num7 * 3 + num9);
									generatedLocationInfo.secretID = text4;
									worldInfo.ruins.Add(generatedLocationInfo);
									mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
								}
								else
								{
									The.ZoneManager.SetZoneProperty(text3, "ambient_bed_3", "Sounds/Ambiences/amb_bed_ruins");
									The.ZoneManager.AddZoneMidBuilder(text3, "OverlandRuins");
									string name2 = QudHistoryFactory.NameRuinsSite(sultanHistory, out var Proper2, out var nameRoot2);
									The.ZoneManager.SetZoneName(text3, name2, null, null, null, null, Proper2);
									string text5 = AddSecret(text3, name2, new string[2] { "ruins", "tech" }, "Ruins");
									game.SetStringGameState(text5 + "_NameRoot", nameRoot2);
									The.ZoneManager.SetZoneProperty(text3, "TeleportGateCandidateNameRoot", nameRoot2);
									terrainTravel2.AddEncounter(new EncounterEntry("You notice some ruins nearby. Would you like to investigate?", text3, "", text5, Optional: true));
									AddLocationFinder(text3, text5);
									if (Options.ShowOverlandEncounters)
									{
										The.ZoneManager.GetZone("JoppaWorld").GetCell(num6, num7).GetObjectInCell(0)
											.Render.RenderString = "#";
									}
									GeneratedLocationInfo generatedLocationInfo2 = new GeneratedLocationInfo();
									generatedLocationInfo2.name = name2;
									generatedLocationInfo2.targetZone = text3;
									generatedLocationInfo2.zoneLocation = Location2D.Get(num6 * 3 + num8, num7 * 3 + num9);
									generatedLocationInfo2.secretID = text5;
									worldInfo.ruins.Add(generatedLocationInfo2);
									mutableMap.SetMutable(generatedLocationInfo2.zoneLocation, 0);
								}
								continue;
							}
							if (string.Equals(a, "Jungle") && 3.in100())
							{
								The.ZoneManager.AddZoneBuilder(text3, 6000, "GoatfolkYurts");
								string name3 = SettlementNames.GenerateGoatfolkVillageName(sultanHistory) + ", goatfolk village";
								The.ZoneManager.SetZoneName(text3, name3, null, null, null, null, Proper: true);
								string text6 = AddSecret(text3, name3, new string[3] { "settlement", "goatfolk", "humanoid" }, "Settlements");
								terrainTravel2.AddEncounter(new EncounterEntry("You smell roasted boar nearby. Would you like to investigate?", text3, "", text6, Optional: true));
								AddLocationFinder(text3, text6);
								if (Options.ShowOverlandEncounters)
								{
									The.ZoneManager.GetZone("JoppaWorld").GetCell(num6, num7).GetObjectInCell(0)
										.Render.RenderString = "Y";
								}
								GeneratedLocationInfo generatedLocationInfo3 = new GeneratedLocationInfo();
								generatedLocationInfo3.name = name3;
								generatedLocationInfo3.targetZone = text3;
								generatedLocationInfo3.zoneLocation = Zone.zoneIDTo240x72Location(text3);
								generatedLocationInfo3.secretID = text6;
								worldInfo.enemySettlements.Add(generatedLocationInfo3);
								mutableMap.SetMutable(generatedLocationInfo3.zoneLocation, 0);
							}
							if (string.Equals(a, "DeepJungle") && 15.in1000())
							{
								The.ZoneManager.AddZoneBuilder(text3, 6000, "GoatfolkQlippothYurts");
								string name4 = SettlementNames.GenerateGoatfolkQlippothVillageName(sultanHistory) + ", goatfolk haunt";
								The.ZoneManager.SetZoneName(text3, name4, null, null, null, null, Proper: true);
								string text7 = AddSecret(text3, name4, new string[3] { "settlement", "goatfolk", "humanoid" }, "Settlements");
								terrainTravel2.AddEncounter(new EncounterEntry("You experience a sense memory of roasted boar smell. Would you like to investigate?", text3, "", text7, Optional: true));
								AddLocationFinder(text3, text7);
								if (Options.ShowOverlandEncounters)
								{
									The.ZoneManager.GetZone("JoppaWorld").GetCell(num6, num7).GetObjectInCell(0)
										.Render.RenderString = "Y";
								}
								GeneratedLocationInfo generatedLocationInfo4 = new GeneratedLocationInfo();
								generatedLocationInfo4.name = name4;
								generatedLocationInfo4.targetZone = text3;
								generatedLocationInfo4.zoneLocation = Zone.zoneIDTo240x72Location(text3);
								generatedLocationInfo4.secretID = text7;
								worldInfo.enemySettlements.Add(generatedLocationInfo4);
								mutableMap.SetMutable(generatedLocationInfo4.zoneLocation, 0);
							}
						}
					}
				}
			}
			MetricsManager.rngCheckpoint("paths");
			BuildStep("Placing canyons", BuildCanyonSystems);
			BuildStep("Placing rivers", BuildRiverSystems);
			BuildStep("Placing roads", BuildRoadSystems);
			MetricsManager.rngCheckpoint("forts");
			BuildStep("Placing forts", BuildForts);
			MetricsManager.rngCheckpoint("farms");
			BuildStep("Placing farms", BuildFarms);
			MetricsManager.rngCheckpoint("statics");
			BuildStep("Placing static encounters", AddStaticEncounters);
			BuildStep("Placing Bey Lah", AddHindrenVillage);
			BuildStep("Placing Hydropon", AddHydropon);
			MetricsManager.rngCheckpoint("oboroqoru");
			if (World == "JoppaWorld")
			{
				BuildStep("Placing Oboroqoru's lair", AddOboroqorusLair);
			}
			MetricsManager.rngCheckpoint("waterway");
			BuildStep("Placing waterways", AddWaterway);
			MetricsManager.rngCheckpoint("sultans");
			if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.IsSwitch))
			{
				WorldCreationProgress.StepProgress("Organizing cults...");
			}
			BuildStep("Creating sultan entries", JournalAPI.InitializeSultanEntries);
			BuildStep("Recording sultan aliases", RecordSultanAliases);
			BuildStep("Placing historic sites", AddSultanHistoryLocations);
			BuildStep("Renaming sultan tombs", RenameSultanTombs);
			if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.IsSwitch))
			{
				WorldCreationProgress.StepProgress("Mythologizing creatures...");
			}
			MetricsManager.rngCheckpoint("lairs");
			BuildStep("Placing lairs", BuildLairs);
			BuildStep("Placing lairs", AddNephilimLairs);
			if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.IsSwitch))
			{
				WorldCreationProgress.StepProgress("Founding villages...");
			}
			MetricsManager.rngCheckpoint("villages");
			BuildStep("Placing villages", AddVillages);
			MetricsManager.rngCheckpoint("secrets");
			BuildStep("Placing secrets", BuildSecrets);
			if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.IsSwitch))
			{
				WorldCreationProgress.StepProgress("Seeding personalities...");
			}
			MetricsManager.rngCheckpoint("quests");
			BuildStep("Generating dynamic quests", BuildDynamicQuests);
			if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.IsSwitch))
			{
				WorldCreationProgress.StepProgress("Shucking clams...");
			}
			MetricsManager.rngCheckpoint("clams");
			BuildStep("Placing clams", PlaceClams);
			MetricsManager.rngCheckpoint("heirlooms");
			BuildStep("Requiring faction heirlooms", Factions.RequireCachedHeirlooms);
			if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.IsSwitch))
			{
				WorldCreationProgress.StepProgress("Spreading gossip...");
			}
			MetricsManager.rngCheckpoint("gossip");
			BuildStep("Creating gossip", JournalAPI.InitializeGossip);
			BuildStep("Creating observations", JournalAPI.InitializeObservations);
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("BuildMutableEncounters", x2);
		}
	}

	public void BuildStep(string Context, Action<string> Action)
	{
		BuildStep(Context, () =>
		{
			Action(World);
		});
	}

	public void BuildStep(string Context, Action Action)
	{
		MetricsManager.LogInfo(Context);
		try
		{
			Action();
		}
		catch (Exception x)
		{
			MetricsManager.LogException(Context, x);
		}
	}

	public Location2D getLocationWithinNFromTerrainType(int min, int max, string terrainType)
	{
		List<Location2D> list = new List<Location2D>();
		foreach (Location2D item in worldInfo.terrainLocations[terrainType])
		{
			for (int i = item.X - max; i < item.X + max; i++)
			{
				for (int j = item.Y - max; j < item.Y + max; j++)
				{
					if (i > 0 && i < 80 && j > 0 && j < 25)
					{
						Location2D location2D = Location2D.Get(i, j);
						int num = location2D.Distance(item);
						if (num >= min && num <= max && !list.Contains(location2D) && mutableMap.GetMutable(i * 3 + 1, j * 3 + 1) > 0)
						{
							list.Add(Location2D.Get(i, j));
						}
					}
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		Location2D randomElement = list.GetRandomElement();
		mutableMap.SetMutable(randomElement, 0);
		return randomElement;
	}

	public Location2D getLocationWithinNFromTerrainTypeTier(int min, int max, string terrainType, int tier)
	{
		List<Location2D> list = new List<Location2D>();
		using (List<Location2D>.Enumerator enumerator = worldInfo.terrainLocations[terrainType].Shuffle().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Location2D current = enumerator.Current;
				list.Clear();
				for (int i = current.X - max; i < current.X + max; i++)
				{
					for (int j = current.Y - max; j < current.Y + max; j++)
					{
						if (i > 0 && i < 80 && j > 0 && j < 25)
						{
							Location2D location2D = Location2D.Get(i, j);
							int num = location2D.Distance(current);
							if (num >= min && num <= max && !list.Contains(location2D) && mutableMap.GetMutable(i * 3 + 1, j * 3 + 1) > 0 && worldInfo.tierLocations[tier].Contains(Location2D.Get(i, j)))
							{
								list.Add(Location2D.Get(i, j));
							}
						}
					}
				}
				if (list.Count == 0)
				{
					return null;
				}
				Location2D randomElement = list.GetRandomElement();
				mutableMap.SetMutable(Location2D.Get(randomElement.X * 3 + 1, randomElement.Y * 3 + 1), 0);
				return randomElement;
			}
		}
		return null;
	}

	public Location2D getLocationWithinNFromTerrainBlueprintTier(int min, int max, string terrainBlueprint, int tier)
	{
		List<Location2D> list = new List<Location2D>();
		Location2D location = game.ZoneManager.GetZone("JoppaWorld").GetFirstObject((GameObject o) => o.Blueprint == terrainBlueprint).GetCurrentCell()
			.Location;
		list.Clear();
		for (int num = location.X - max; num < location.X + max; num++)
		{
			for (int num2 = location.Y - max; num2 < location.Y + max; num2++)
			{
				if (num > 0 && num < 80 && num2 > 0 && num2 < 25)
				{
					Location2D location2D = Location2D.Get(num, num2);
					int num3 = location2D.Distance(location);
					if (num3 >= min && num3 <= max && !list.Contains(location2D) && mutableMap.GetMutable(num * 3 + 1, num2 * 3 + 1) > 0 && worldInfo.tierLocations[tier].Contains(Location2D.Get(num, num2)))
					{
						list.Add(Location2D.Get(num, num2));
					}
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		Location2D randomElement = list.GetRandomElement();
		mutableMap.SetMutable(Location2D.Get(randomElement.X * 3 + 1, randomElement.Y * 3 + 1), 0);
		return randomElement;
	}

	public string GetZoneIdOfTerrain(string Terrain, string z = "10")
	{
		Location2D randomElement = worldInfo.terrainLocations[Terrain].GetRandomElement();
		string[] array = new string[10] { "JoppaWorld.", null, null, null, null, null, null, null, null, null };
		int x = randomElement.X;
		array[1] = x.ToString();
		array[2] = ".";
		x = randomElement.Y;
		array[3] = x.ToString();
		array[4] = ".";
		array[5] = Stat.Random(0, 2).ToString();
		array[6] = ".";
		array[7] = Stat.Random(0, 2).ToString();
		array[8] = ".";
		array[9] = z;
		return string.Concat(array);
	}

	public Location2D popMutableBlockOfTerrain(string Terrain)
	{
		List<Location2D> list = new List<Location2D>();
		foreach (Location2D item in worldInfo.terrainLocations[Terrain].Shuffle())
		{
			list.Clear();
			bool flag = true;
			int num = 0;
			while (true)
			{
				if (num <= 2)
				{
					int num2 = 0;
					while (num2 <= 2)
					{
						if (mutableMap.GetMutable(item.X * 3 + num, item.Y * 3 + num2) > 0)
						{
							num2++;
							continue;
						}
						goto IL_0066;
					}
					num++;
					continue;
				}
				if (!flag)
				{
					break;
				}
				for (int i = 0; i <= 2; i++)
				{
					for (int j = 0; j <= 2; j++)
					{
						mutableMap.RemoveMutableLocation(Location2D.Get(item.X * 3 + i, item.Y * 3 + j));
					}
				}
				return Location2D.Get(item.X * 3 + 1, item.Y * 3 + 1);
				IL_0066:
				flag = false;
				break;
			}
		}
		return null;
	}

	/// <summary>
	/// Finds a radius x radius square of terrain and returns the center zone
	/// </summary>
	/// <param name="Terrain" />
	/// <param name="where" />
	/// <param name="radius" />
	/// <returns />
	public Location2D popMutableLocationBlockOfTerrain(string Terrain, Predicate<Location2D> where = null, int radius = 1)
	{
		List<Location2D> list = new List<Location2D>();
		if (!worldInfo.terrainLocations.ContainsKey(Terrain))
		{
			Debug.LogError("Couldn't find terrain: " + Terrain);
		}
		foreach (Location2D item in worldInfo.terrainLocations[Terrain].Shuffle())
		{
			list.Clear();
			int num = -radius;
			while (true)
			{
				if (num <= radius)
				{
					for (int i = -radius; i <= radius; i++)
					{
						if (mutableMap.GetMutable(item.X * 3 + num, item.Y * 3 + i) <= 0)
						{
							goto end_IL_00c7;
						}
						Location2D location2D = Location2D.Get(item.X * 3 + num, item.Y * 3 + i);
						if (where != null && !where(location2D))
						{
							goto end_IL_00c7;
						}
						list.Add(location2D);
					}
					num++;
					continue;
				}
				if (list.Count != 9)
				{
					break;
				}
				Location2D result = list[4];
				list.ForEach((Location2D r) =>
				{
					mutableMap.RemoveMutableLocation(r);
				});
				return result;
				continue;
				end_IL_00c7:
				break;
			}
		}
		return null;
	}

	public IEnumerable<Location2D> YieldBlocksWithin(string Terrain, Box Box = null, bool Mutable = true)
	{
		if (worldInfo.terrainLocations.TryGetValue(Terrain, out var value))
		{
			value = value.Shuffle();
			foreach (Location2D item in value)
			{
				if ((Box == null || Box.contains(item)) && (!Mutable || mutableMap.GetWorldBlockMutable(item)))
				{
					yield return item;
				}
			}
		}
		else
		{
			Debug.LogError("Couldn't find terrain: " + Terrain);
		}
	}

	public Location2D peekMutableLocation(Predicate<Location2D> where = null, bool centerOnly = true)
	{
		List<Location2D> list = new List<Location2D>();
		foreach (KeyValuePair<string, List<Location2D>> item in worldInfo.terrainLocations.Where((KeyValuePair<string, List<Location2D>> l) => l.Value.Count > 0).ToList().Shuffle())
		{
			foreach (Location2D item2 in item.Value.Shuffle())
			{
				list.Clear();
				for (int num = 0; num <= 2; num++)
				{
					for (int num2 = 0; num2 <= 2; num2++)
					{
						if ((!centerOnly || (num == 1 && num2 == 1)) && mutableMap.GetMutable(item2.X * 3 + num, item2.Y * 3 + num2) > 0)
						{
							Location2D location2D = Location2D.Get(item2.X * 3 + num, item2.Y * 3 + num2);
							if (where == null || where(location2D))
							{
								list.Add(location2D);
							}
						}
					}
				}
				if (list.Count > 0)
				{
					return list.GetRandomElement();
				}
			}
		}
		return null;
	}

	public Location2D popMutableLocation(Predicate<Location2D> where = null, bool centerOnly = true)
	{
		List<Location2D> list = new List<Location2D>();
		foreach (KeyValuePair<string, List<Location2D>> item in worldInfo.terrainLocations.Where((KeyValuePair<string, List<Location2D>> l) => l.Value.Count > 0).ToList().Shuffle())
		{
			foreach (Location2D item2 in item.Value.Shuffle())
			{
				list.Clear();
				for (int num = 0; num <= 2; num++)
				{
					for (int num2 = 0; num2 <= 2; num2++)
					{
						if ((!centerOnly || (num == 1 && num2 == 1)) && mutableMap.GetMutable(item2.X * 3 + num, item2.Y * 3 + num2) > 0)
						{
							Location2D location2D = Location2D.Get(item2.X * 3 + num, item2.Y * 3 + num2);
							if (where == null || where(location2D))
							{
								list.Add(location2D);
							}
						}
					}
				}
				if (list.Count > 0)
				{
					Location2D randomElement = list.GetRandomElement();
					mutableMap.RemoveMutableLocation(randomElement);
					return randomElement;
				}
			}
		}
		return null;
	}

	public Location2D popMutableLocationOfTerrain(string Terrain, Predicate<Location2D> where = null, bool centerOnly = true)
	{
		List<Location2D> list = new List<Location2D>();
		if (!worldInfo.terrainLocations.ContainsKey(Terrain))
		{
			Debug.LogError("Couldn't find terrain: " + Terrain);
		}
		foreach (Location2D item in worldInfo.terrainLocations[Terrain].Shuffle())
		{
			list.Clear();
			for (int i = 0; i <= 2; i++)
			{
				for (int j = 0; j <= 2; j++)
				{
					if ((!centerOnly || (i == 1 && j == 1)) && mutableMap.GetMutable(item.X * 3 + i, item.Y * 3 + j) > 0)
					{
						Location2D location2D = Location2D.Get(item.X * 3 + i, item.Y * 3 + j);
						if (where == null || where(location2D))
						{
							list.Add(location2D);
						}
					}
				}
			}
			if (list.Count > 0)
			{
				Location2D randomElement = list.GetRandomElement();
				mutableMap.RemoveMutableLocation(randomElement);
				return randomElement;
			}
		}
		return null;
	}

	public Location2D getLocationOfTier(int tier)
	{
		int num = 0;
		List<Location2D> value;
		while (!worldInfo.tierLocations.TryGetValue(tier, out value))
		{
			Debug.LogWarning("Couldn't find location of tier " + tier);
			tier--;
			if (tier < 1)
			{
				tier = 8;
			}
			num++;
			if (num > 9)
			{
				return null;
			}
		}
		foreach (Location2D item in value.Shuffle())
		{
			if (mutableMap.GetMutable(item.X * 3 + 1, item.Y * 3 + 1) > 0)
			{
				mutableMap.SetMutable(Location2D.Get(item.X * 3 + 1, item.Y * 3 + 1), 0);
				return item;
			}
		}
		return null;
	}

	public Location2D getLocationOfTier(int minTier, int maxTier)
	{
		List<int> list = new List<int>();
		for (int i = minTier; i <= maxTier; i++)
		{
			list.Add(i);
		}
		list.ShuffleInPlace();
		foreach (int item in list)
		{
			if (!worldInfo.tierLocations.ContainsKey(item))
			{
				continue;
			}
			foreach (Location2D item2 in worldInfo.tierLocations[item].Shuffle())
			{
				if (mutableMap.GetMutable(item2.X * 3 + 1, item2.Y * 3 + 1) > 0)
				{
					mutableMap.SetMutable(Location2D.Get(item2.X * 3 + 1, item2.Y * 3 + 1), 0);
					return item2;
				}
			}
		}
		return null;
	}

	public void BuildDynamicQuests(string WorldID)
	{
		foreach (HistoricEntity entitiesWherePropertyEqual in The.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "village"))
		{
			if (!entitiesWherePropertyEqual.GetCurrentSnapshot().hasProperty("zoneID"))
			{
				continue;
			}
			int num = 1;
			if (entitiesWherePropertyEqual.HasEntityProperty("isVillageZero", -1L))
			{
				num = 2;
			}
			for (int i = 0; i < num; i++)
			{
				VillageDynamicQuestContext villageDynamicQuestContext = new VillageDynamicQuestContext(entitiesWherePropertyEqual);
				villageDynamicQuestContext.questNumber = i;
				string populationName = "Dynamic Village Quests";
				try
				{
					DynamicQuestFactory.fabricateQuestTemplate(PopulationManager.RollOneFrom(populationName).Blueprint, villageDynamicQuestContext);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("DynamicVillageQuestFab", x);
				}
			}
		}
	}

	public void AddVillages()
	{
		History sultanHistory = The.Game.sultanHistory;
		HistoricEntityList entitiesWherePropertyEquals = sultanHistory.GetEntitiesWherePropertyEquals("type", "village");
		bool flag = false;
		int num = 0;
		foreach (HistoricEntity item in entitiesWherePropertyEquals)
		{
			item.events.Find((HistoricEvent historicEvent) => historicEvent is InitializeVillage);
			HistoricEntitySnapshot currentSnapshot = item.GetCurrentSnapshot();
			string property = currentSnapshot.GetProperty("name");
			string property2 = currentSnapshot.GetProperty("region");
			bool flag2 = false;
			if (currentSnapshot.GetProperty("villageZero", "false") == "true")
			{
				if (flag)
				{
					sultanHistory.entities.Remove(item);
					continue;
				}
				if (The.Game.GetStringGameState("VillageZeroRegion", "&YJoppa") == property2)
				{
					flag2 = true;
					flag = true;
				}
				if (!flag2)
				{
					sultanHistory.entities.Remove(item);
					continue;
				}
			}
			int num2;
			int num3;
			if (flag2)
			{
				num2 = 1;
				num3 = 1;
			}
			else
			{
				try
				{
					num2 = currentSnapshot.Tier;
				}
				catch
				{
					num2 = Tier.Constrain(num);
				}
				try
				{
					num3 = currentSnapshot.TechTier;
				}
				catch
				{
					num3 = Tier.Constrain(num);
				}
			}
			Location2D location2D = null;
			if (flag2)
			{
				Box box = new Box(0, 15, 26, 25);
				location2D = YieldBlocksWithin(property2, box).FirstOrDefault();
			}
			if (location2D == null)
			{
				location2D = YieldBlocksWithin(property2).FirstOrDefault();
			}
			if (location2D == null)
			{
				MetricsManager.LogError($"Unable to find map cell for village in {property2} (V0: {flag2})");
				sultanHistory.entities.Remove(item);
				continue;
			}
			mutableMap.SetWorldBlockMutable(location2D, 0);
			Location2D location2D2 = Location2D.Get(location2D.X * 3 + 1, location2D.Y * 3 + 1);
			GameObject firstObjectWithPart = WorldZone.GetCell(location2D.X, location2D.Y).GetFirstObjectWithPart("TerrainTravel");
			VillageTerrain villageTerrain = new VillageTerrain(item);
			firstObjectWithPart.AddPart(villageTerrain);
			string[] array = new string[5] { "JoppaWorld.", null, null, null, null };
			int x = location2D.X;
			array[1] = x.ToString();
			array[2] = ".";
			x = location2D.Y;
			array[3] = x.ToString();
			array[4] = ".1.1.10";
			string text = string.Concat(array);
			The.ZoneManager.AdjustZoneGenerationTierTo(text);
			if (flag2)
			{
				game.SetStringGameState("villageZeroStartingLocation", text + "@37,22");
				item.SetEntityPropertyAtCurrentYear("isVillageZero", "true");
			}
			item.SetEntityPropertyAtCurrentYear("zoneID", text);
			currentSnapshot.setProperty("zoneID", text);
			if (currentSnapshot.hasProperty("worships_creature"))
			{
				Location2D villageLocation = Zone.zoneIDTo240x72Location(text);
				GeneratedLocationInfo randomElement = worldInfo.lairs.Where((GeneratedLocationInfo l) => l != null && villageLocation.ManhattanDistance(l.zoneLocation) >= 0 && villageLocation.ManhattanDistance(l.zoneLocation) <= 18).GetRandomElement();
				if (randomElement == null)
				{
					randomElement = worldInfo.lairs.GetRandomElement();
				}
				GameObject cachedObjects = The.ZoneManager.GetCachedObjects(randomElement.ownerID);
				Worships.PostProcessEvent(item, cachedObjects.GetReferenceDisplayName(int.MaxValue, null, "Worship", NoColor: false, Stripped: true), cachedObjects.ID);
			}
			if (currentSnapshot.hasProperty("despises_creature"))
			{
				Location2D villageLocation2 = Zone.zoneIDTo240x72Location(text);
				GeneratedLocationInfo randomElement2 = worldInfo.lairs.Where((GeneratedLocationInfo l) => l != null && villageLocation2.ManhattanDistance(l.zoneLocation) >= 0 && villageLocation2.ManhattanDistance(l.zoneLocation) <= 18).GetRandomElement();
				if (randomElement2 == null)
				{
					randomElement2 = worldInfo.lairs.GetRandomElement();
				}
				GameObject cachedObjects2 = The.ZoneManager.GetCachedObjects(randomElement2.ownerID);
				Despises.PostProcessEvent(item, cachedObjects2.GetReferenceDisplayName(int.MaxValue, null, "Despise", NoColor: false, Stripped: true), cachedObjects2.ID);
			}
			TerrainTravel part = WorldZone.GetCell(location2D.X, location2D.Y).GetFirstObjectWithPart("TerrainTravel").GetPart<TerrainTravel>();
			Faction faction = VillageBase.CreateVillageFaction(currentSnapshot);
			string text2 = AddSecret(text, property, new string[3] { "settlement", "villages", "humanoid" }, "Settlements", item.id, flag2, flag2);
			JournalAPI.GetMapNote(text2).Tradable = !flag2;
			JournalAPI.GetMapNote(text2).Attributes.Add("nobuy:" + faction.Name);
			JournalAPI.GetMapNote(text2).Attributes.Add("nosell:" + faction.Name);
			JournalMapNote mapNote = JournalAPI.GetMapNote(text2);
			mapNote.History = mapNote.History + " {{K|-learned from " + Faction.GetFormattedName(faction.Name) + "}}";
			JournalAPI.GetMapNote(text2).LearnedFrom = Faction.GetFormattedName(faction.Name);
			if (currentSnapshot.GetProperty("abandoned") == "true")
			{
				part.AddEncounter(new EncounterEntry("You discover an abandoned village. Would you like to investigate?", text, "", text2, Optional: true));
			}
			else
			{
				part.AddEncounter(new EncounterEntry("You discover a village. Would you like to investigate?", text, "", text2, Optional: true));
			}
			villageTerrain.secretId = text2;
			The.ZoneManager.AddZoneBuilder(text, 6000, "Village", "VillageEntityID", item.id, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name);
			The.ZoneManager.SetZoneName(text, property, null, null, null, null, Proper: true);
			The.ZoneManager.SetZoneIncludeStratumInZoneDisplay(text, false);
			string[] directionList = Directions.DirectionList;
			foreach (string d in directionList)
			{
				Location2D location2D3 = location2D2.FromDirection(d);
				string zoneID = Zone.XYToID("JoppaWorld", location2D3.X, location2D3.Y, 10);
				The.ZoneManager.AdjustZoneGenerationTierTo(zoneID);
				The.ZoneManager.AddZoneBuilder(zoneID, 6000, "VillageOutskirts", "VillageEntityID", item.id, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name);
				The.ZoneManager.SetZoneProperty(zoneID, "NoBiomes", "Yes");
				The.ZoneManager.SetZoneName(zoneID, "outskirts", property, "some");
				The.ZoneManager.SetZoneIncludeStratumInZoneDisplay(zoneID, false);
				string zoneID2 = Zone.XYToID("JoppaWorld", location2D3.X, location2D3.Y, 9);
				The.ZoneManager.AdjustZoneGenerationTierTo(zoneID2);
				The.ZoneManager.AddZoneBuilder(zoneID2, 6000, "VillageOver", "VillageEntityID", item.id, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name);
				The.ZoneManager.SetZoneName(zoneID2, "sky", property, null, null, "the");
				string zoneID3 = Zone.XYToID("JoppaWorld", location2D3.X, location2D3.Y, 11);
				The.ZoneManager.AddZoneBuilder(zoneID3, 6000, "VillageUnder", "VillageEntityID", item.id, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name);
			}
			for (int num4 = 9; num4 >= 0; num4--)
			{
				string[] array2 = new string[6] { "JoppaWorld.", null, null, null, null, null };
				x = location2D.X;
				array2[1] = x.ToString();
				array2[2] = ".";
				x = location2D.Y;
				array2[3] = x.ToString();
				array2[4] = ".1.1.";
				array2[5] = num4.ToString();
				string zoneID4 = string.Concat(array2);
				The.ZoneManager.AdjustZoneGenerationTierTo(zoneID4);
				The.ZoneManager.AddZoneBuilder(zoneID4, 6000, "VillageOver", "VillageEntityID", item.id, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name);
				The.ZoneManager.SetZoneName(zoneID4, "sky", property, null, null, "the");
			}
			string[] array3 = new string[5] { "JoppaWorld.", null, null, null, null };
			x = location2D.X;
			array3[1] = x.ToString();
			array3[2] = ".";
			x = location2D.Y;
			array3[3] = x.ToString();
			array3[4] = ".1.1.11";
			string zoneID5 = string.Concat(array3);
			The.ZoneManager.AddZoneBuilder(zoneID5, 6000, "VillageUnder", "VillageEntityID", item.id, "villageTier", num2, "villageTechTier", num3, "villageFaction", faction.Name);
			The.ZoneManager.SetZoneName(zoneID5, "undervillage", property, null, null, "the");
			GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
			generatedLocationInfo.name = property;
			generatedLocationInfo.targetZone = text;
			generatedLocationInfo.zoneLocation = location2D2;
			generatedLocationInfo.secretID = text2;
			worldInfo.villages.Add(generatedLocationInfo);
			mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
			num++;
		}
		JournalAPI.InitializeVillageEntries();
	}

	public void AddSultanHistoryLocations()
	{
		History sultanHistory = The.Game.sultanHistory;
		HistoricEntityList entitiesWherePropertyEquals = sultanHistory.GetEntitiesWherePropertyEquals("type", "region");
		HistoricEntityList entitiesWherePropertyEquals2 = sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan");
		for (int i = 0; i < 8; i++)
		{
			int num = i + 1;
			int tier = num;
			bool flag = false;
			Location2D location2D;
			if (i != 0)
			{
				location2D = ((i >= 2) ? getLocationOfTier(tier) : getLocationWithinNFromTerrainBlueprintTier(1, 29, "TerrainJoppa", tier));
			}
			else
			{
				location2D = getLocationWithinNFromTerrainBlueprintTier(3, 8, "TerrainJoppa", tier);
			}
			int num2 = 0;
			if (i == 0)
			{
				num2 = 5;
			}
			if (i == 1)
			{
				num2 = 5;
			}
			if (i == 2)
			{
				num2 = 4;
			}
			if (i == 3)
			{
				num2 = 4;
			}
			if (i == 4)
			{
				num2 = 3;
			}
			if (i == 5)
			{
				num2 = 3;
			}
			if (i == 6)
			{
				num2 = 2;
			}
			if (i == 7)
			{
				num2 = 1;
			}
			HistoricEntity historicEntity = null;
			HistoricEntitySnapshot historicEntitySnapshot = null;
			for (int j = 0; j < entitiesWherePropertyEquals.entities.Count; j++)
			{
				HistoricEntitySnapshot currentSnapshot = entitiesWherePropertyEquals.entities[j].GetCurrentSnapshot();
				if (Convert.ToInt32(currentSnapshot.GetProperty("period", "-1")) == num2 && currentSnapshot.hasListProperty("items"))
				{
					historicEntity = entitiesWherePropertyEquals.entities[j];
					historicEntitySnapshot = currentSnapshot;
					entitiesWherePropertyEquals.entities.RemoveAt(j);
					break;
				}
			}
			if (historicEntity == null)
			{
				for (int k = 0; k < entitiesWherePropertyEquals.entities.Count; k++)
				{
					HistoricEntitySnapshot regionSnap = entitiesWherePropertyEquals.entities[k].GetCurrentSnapshot();
					if (Convert.ToInt32(regionSnap.GetProperty("period", "-1")) != num2)
					{
						continue;
					}
					bool flag2 = false;
					for (int l = 0; l < entitiesWherePropertyEquals2.entities.Count; l++)
					{
						if (entitiesWherePropertyEquals2.entities[l].GetRandomEventWhereDelegate((HistoricEvent ev) => ev.HasEventProperty("revealsRegion") && ev.GetEventProperty("revealsRegion") != null && ev.GetEventProperty("revealsRegion") == regionSnap.GetProperty("newName"), Stat.Rnd) != null)
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						historicEntity = entitiesWherePropertyEquals.entities[k];
						historicEntitySnapshot = regionSnap;
						entitiesWherePropertyEquals.entities.RemoveAt(k);
						break;
					}
				}
			}
			if (historicEntity == null)
			{
				int index = Stat.Random(0, entitiesWherePropertyEquals.entities.Count - 1);
				historicEntity = entitiesWherePropertyEquals.entities[index];
				historicEntitySnapshot = historicEntity.GetCurrentSnapshot();
				entitiesWherePropertyEquals.entities.RemoveAt(index);
			}
			int num3 = 1;
			switch (num)
			{
			case 0:
				num3 += 4;
				break;
			case 1:
				num3 += 4;
				break;
			case 2:
				num3 += Stat.Random(4, 5);
				break;
			case 3:
				num3 += Stat.Random(4, 5);
				break;
			case 4:
				num3 += Stat.Random(4, 5);
				break;
			case 5:
				num3 += Stat.Random(4, 5);
				break;
			case 6:
				num3 += Stat.Random(4, 6);
				break;
			case 7:
				num3 += Stat.Random(5, 6);
				break;
			case 8:
				num3 += Stat.Random(5, 7);
				break;
			}
			string property = historicEntitySnapshot.GetProperty("newName");
			The.Game.SetStringGameState("SultanDungeonPlacementOrder_" + i, property);
			The.Game.SetStringGameState("SultanDungeonPlaced_" + property, i.ToString());
			The.Game.SetObjectGameState("sultanRegionPosition_" + property, location2D.Vector2i);
			The.Game.SetObjectGameState("sultanRegionPosition_" + historicEntitySnapshot.GetProperty("name"), location2D.Vector2i);
			SultanRegion sultanRegion = WorldZone.GetCell(location2D.X, location2D.Y).GetFirstObjectWithPart("TerrainTravel").AddPart(new SultanRegion(historicEntity));
			GameObject gameObject = GameObjectFactory.Factory.CreateObject("SultanRegionSurface");
			SultanRegionSurface part = gameObject.GetPart<SultanRegionSurface>();
			part.RegionName = property;
			part.RevealKey = "sultanRegionReveal_" + property;
			part.RevealLocation = new Vector2i(location2D.X, location2D.Y);
			part.RevealString = "You discover " + property + ".";
			string[] array = new string[5] { "JoppaWorld.", null, null, null, null };
			int x = location2D.X;
			array[1] = x.ToString();
			array[2] = ".";
			x = location2D.Y;
			array[3] = x.ToString();
			array[4] = ".1.1.10";
			string text = string.Concat(array);
			The.ZoneManager.AdjustZoneGenerationTierTo(text);
			TerrainTravel part2 = WorldZone.GetCell(location2D.X, location2D.Y).GetFirstObjectWithPart("TerrainTravel").GetPart<TerrainTravel>();
			string text2 = AddSecret(text, property, new string[3] { "historic", "tech", "ruins" }, "Historic Sites");
			string property2 = historicEntitySnapshot.GetProperty("nameRoot");
			game.SetStringGameState(text2 + "_NameRoot", property2);
			The.ZoneManager.SetZoneProperty(text, "TeleportGateCandidateNameRoot", property2);
			if (i == 0)
			{
				JournalAPI.GetMapNote(text2).Attributes.Add("nobuy:Joppa");
				JournalMapNote mapNote = JournalAPI.GetMapNote(text2);
				mapNote.History = mapNote.History + " {{K|-known by " + Faction.GetFormattedName("Joppa") + "}}";
			}
			part2.AddEncounter(new EncounterEntry("You discover some historic ruins. Would you like to investigate?", text, "", text2, Optional: true));
			part.RevealSecret = text2;
			sultanRegion.secretId = text2;
			HistoricEntitySnapshot currentSnapshot2 = sultanHistory.GetEntitiesWherePropertyEquals("newName", property).entities[0].GetCurrentSnapshot();
			List<string> locationsInRegion = QudHistoryHelpers.GetLocationsInRegion(sultanHistory, currentSnapshot2.GetProperty("name"));
			if (num3 < locationsInRegion.Count)
			{
				num3 = locationsInRegion.Count;
			}
			SultanDungeonArgs sultanDungeonArgs = new SultanDungeonArgs();
			if (num2 > 0)
			{
				HistoricEntity randomElement = entitiesWherePropertyEquals2.GetEntitiesWherePropertyEquals("period", num2.ToString()).GetRandomElement();
				if (randomElement == null)
				{
					randomElement = entitiesWherePropertyEquals2.GetRandomElement();
				}
				if (randomElement != null)
				{
					sultanDungeonArgs.UpdateFromEntity(randomElement.GetCurrentSnapshot());
				}
			}
			sultanDungeonArgs.UpdateWalls(num2);
			sultanDungeonArgs.UpdateFromEntity(currentSnapshot2);
			if (50.in100())
			{
				sultanDungeonArgs.wallTypes.Add("*SultanWall*");
			}
			Faction faction = Factions.Get("SultanCult" + sultanDungeonArgs.cultPeriod);
			The.Game.SetObjectGameState("sultanDungeonArgs_" + property, sultanDungeonArgs);
			List<string> list = new List<string>();
			for (int num4 = 0; num4 < locationsInRegion.Count; num4++)
			{
				list.Add(locationsInRegion[num4]);
			}
			for (int num5 = 0; num5 < num3 - locationsInRegion.Count; num5++)
			{
				list.Add(null);
			}
			Algorithms.RandomShuffle(list);
			if (list[num3 - 1] == null)
			{
				for (int num6 = 0; num6 < num3; num6++)
				{
					if (list[num6] != null)
					{
						list[num3 - 1] = list[num6];
						list[num6] = null;
						break;
					}
				}
			}
			for (int num7 = 0; num7 < num3; num7++)
			{
				bool flag3 = false;
				string text3 = list[num7];
				string[] array2 = new string[6] { "JoppaWorld.", null, null, null, null, null };
				x = location2D.X;
				array2[1] = x.ToString();
				array2[2] = ".";
				x = location2D.Y;
				array2[3] = x.ToString();
				array2[4] = ".1.1.";
				array2[5] = (num7 + 10).ToString();
				string text4 = string.Concat(array2);
				The.ZoneManager.AdjustZoneGenerationTierTo(text4);
				if (num7 > 0)
				{
					faction.HolyPlaces.Add(text4);
				}
				if (text3 == null)
				{
					flag3 = false;
					string text5 = Grammar.MakeTitleCase(property);
					if (num7 == 0)
					{
						The.ZoneManager.SetZoneName(text4, text5, null, null, null, null, Proper: true);
					}
					else
					{
						The.ZoneManager.SetZoneName(text4, "liminal floor", text5);
					}
					text3 = ((locationsInRegion.Count <= 0) ? sultanHistory.GetEntitiesWherePropertyEquals("type", "location").entities.GetRandomElement().GetCurrentSnapshot().GetProperty("name") : locationsInRegion.GetRandomElement());
				}
				else
				{
					flag3 = true;
					string context = Grammar.MakeTitleCase(property);
					The.ZoneManager.SetZoneName(text4, Grammar.MakeTitleCase(text3), context, null, null, null, Proper: true);
				}
				The.ZoneManager.SetZoneProperty(text4, "HistoricSite", property);
				The.ZoneManager.SetZoneProperty(text4, "TeleportGateCandidateNameRoot", property2);
				HistoricEntitySnapshot currentSnapshot3 = sultanHistory.GetEntitiesWherePropertyEquals("name", text3).entities[0].GetCurrentSnapshot();
				string text6 = "";
				if (num7 < num3 - 1)
				{
					text6 += "D";
				}
				if (num7 > 0)
				{
					text6 += "U";
				}
				if (num7 != 0)
				{
					The.ZoneManager.ClearZoneBuilders(text4);
				}
				if (num7 != 0)
				{
					The.ZoneManager.SetZoneProperty(text4, "SkipTerrainBuilders", true);
				}
				The.ZoneManager.AddZoneBuilder(text4, 6000, "SultanDungeon", "locationName", text3, "regionName", property, "stairs", text6);
				The.ZoneManager.AddZoneBuilder(text4, 6000, "AddWidgetBuilder", "Object", The.ZoneManager.CacheObject(gameObject, cacheTwiceOk: true));
				The.ZoneManager.AddZoneBuilder(text4, 6000, "Music", "Track", "Music/of Chrome and How");
				The.ZoneManager.SetZoneProperty(text4, "ZoneTierOverride", num.ToString());
				if (!flag3)
				{
					continue;
				}
				if (currentSnapshot3.listProperties.ContainsKey("items"))
				{
					foreach (string item in currentSnapshot3.listProperties["items"])
					{
						HistoricEntityList entitiesWherePropertyEquals3 = sultanHistory.GetEntitiesWherePropertyEquals("name", item);
						if (entitiesWherePropertyEquals3.Count > 0)
						{
							GameObject gameObject2 = RelicGenerator.GenerateRelic(entitiesWherePropertyEquals3.entities[0].GetCurrentSnapshot(), tier);
							gameObject2.AddPart(new TakenAchievement
							{
								AchievementID = "ACH_RECOVER_RELIC"
							});
							The.ZoneManager.SetZoneProperty(text4, "Relicstyle", "Vault");
							if (num7 == num3 - 1)
							{
								The.ZoneManager.AddZoneBuilder(text4, 6000, "PlaceRelicBuilder", "Relic", The.ZoneManager.CacheObject(gameObject2));
								flag = true;
							}
							else
							{
								The.ZoneManager.AddZoneBuilder(text4, 6000, "PlaceRelicBuilder", "Relic", The.ZoneManager.CacheObject(gameObject2), "AddCreditWedges", false);
							}
						}
						else
						{
							Debug.LogError("Unknown relic: " + item);
						}
					}
				}
				else if (3.in1000() && num7 != num3 - 1)
				{
					GameObject gameObject3 = RelicGenerator.GenerateRelic(currentSnapshot3, tier);
					gameObject3.AddPart(new TakenAchievement
					{
						AchievementID = "ACH_RECOVER_RELIC"
					});
					The.ZoneManager.AddZoneBuilder(text4, 6000, "PlaceRelicBuilder", "Relic", The.ZoneManager.CacheObject(gameObject3), "AddCreditWedges", false);
				}
			}
			if (!flag)
			{
				string[] array3 = new string[6] { "JoppaWorld.", null, null, null, null, null };
				x = location2D.X;
				array3[1] = x.ToString();
				array3[2] = ".";
				x = location2D.Y;
				array3[3] = x.ToString();
				array3[4] = ".1.1.";
				array3[5] = (10 + num3 - 1).ToString();
				string zoneID = string.Concat(array3);
				The.ZoneManager.AdjustZoneGenerationTierTo(zoneID);
				GameObject gameObject4 = RelicGenerator.GenerateRelic(currentSnapshot2, tier, null, RandomName: true);
				gameObject4.AddPart(new TakenAchievement
				{
					AchievementID = "ACH_RECOVER_RELIC"
				});
				The.ZoneManager.SetZoneProperty(zoneID, "Relicstyle", "Vault");
				The.ZoneManager.AddZoneBuilder(zoneID, 6000, "PlaceRelicBuilder", "Relic", The.ZoneManager.CacheObject(gameObject4));
			}
			mutableMap.SetMutable(Location2D.Get(location2D.X * 3 + 1, location2D.Y * 3 + 1), 0);
			for (int num8 = 0; num8 < entitiesWherePropertyEquals.entities.Count; num8++)
			{
				HistoricEntitySnapshot currentSnapshot4 = entitiesWherePropertyEquals.entities[num8].GetCurrentSnapshot();
				if (Convert.ToInt32(currentSnapshot4.GetProperty("period", "-1")) == num2 && i != 0 && currentSnapshot4.hasListProperty("items"))
				{
					i--;
					break;
				}
			}
		}
		for (int num9 = 1; num9 <= 6; num9++)
		{
			HistoricEntity periodSultan = QudHistoryHelpers.GetPeriodSultan(sultanHistory, num9);
			string sultanFactionName = Faction.GetSultanFactionName(num9);
			Faction faction2 = Factions.Get(sultanFactionName);
			faction2.Emblem = new FactionEmblem();
			faction2.Emblem = new FactionEmblem();
			faction2.Emblem.ColorString = "&y";
			string property3 = periodSultan.GetCurrentSnapshot().GetProperty("name", "<unknown>");
			faction2.Emblem.Tile = SultanShrine.GetStatueForSultan(property3, num9);
			faction2.Emblem.RenderString = "8";
			faction2.Emblem.TileColor = "&y";
			faction2.Emblem.DetailColor = 'g';
			foreach (string sultanLikedFaction in HistoryAPI.GetSultanLikedFactions(periodSultan))
			{
				faction2.ApplyWorshipAttitude(sultanLikedFaction, 50);
				Factions.Get(sultanLikedFaction).ApplyWorshipAttitude(sultanFactionName, 50);
			}
			foreach (string sultanHatedFaction in HistoryAPI.GetSultanHatedFactions(periodSultan))
			{
				faction2.ApplyWorshipAttitude(sultanHatedFaction, -100);
				Factions.Get(sultanHatedFaction).ApplyWorshipAttitude(sultanFactionName, -100);
			}
		}
	}

	public void RecordSultanAliases()
	{
		foreach (HistoricEntity entitiesWherePropertyEqual in The.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan"))
		{
			string property = entitiesWherePropertyEqual.GetCurrentSnapshot().GetProperty("period");
			game.SetStringGameState("*Sultan" + property + "Name*", entitiesWherePropertyEqual.GetCurrentSnapshot().GetProperty("name"));
		}
	}

	public void RenameSultanTombs()
	{
		int num = 5;
		int num2 = 1;
		while (num >= 1)
		{
			for (int i = 0; i <= 2; i++)
			{
				for (int j = 0; j <= 2; j++)
				{
					string zoneID = ZoneID.Assemble("JoppaWorld", 53, 3, i, j, num);
					The.ZoneManager.AdjustZoneGenerationTierTo(zoneID);
					The.ZoneManager.SetZoneBaseDisplayName(zoneID, The.ZoneManager.GetZoneBaseDisplayName(zoneID).Replace("*Sultan" + num2 + "Name*", game.GetStringGameState("*Sultan" + num2 + "Name*")));
				}
			}
			num--;
			num2++;
		}
	}

	public void AddMutableEncounterToTerrainRect(string Terrain, int n, Action<string, Location2D, TerrainTravel> encounter, bool unflagAsMutable = true)
	{
		for (int i = 0; i < n; i++)
		{
			Location2D mutableLocationWithTerrain = mutableMap.GetMutableLocationWithTerrain(Terrain);
			if (mutableLocationWithTerrain != null)
			{
				int x = mutableLocationWithTerrain.X;
				int y = mutableLocationWithTerrain.Y;
				int x2 = mutableLocationWithTerrain.X / 3;
				int y2 = mutableLocationWithTerrain.Y / 3;
				string text = Zone.XYToID("JoppaWorld", x, y, 10);
				The.ZoneManager?.AdjustZoneGenerationTierTo(text);
				encounter(text, mutableLocationWithTerrain, terrainComponents[Location2D.Get(x2, y2)]);
				if (unflagAsMutable)
				{
					mutableMap.RemoveMutableLocation(mutableLocationWithTerrain);
				}
			}
		}
	}

	public void AddMutableEncounterToTerrain(string Terrain, int n, Action<string, Location2D, TerrainTravel> encounter, bool unflagAsMutable = true)
	{
		for (int i = 0; i < n; i++)
		{
			Location2D mutableLocationWithTerrain = mutableMap.GetMutableLocationWithTerrain(Terrain);
			if (mutableLocationWithTerrain != null)
			{
				int x = mutableLocationWithTerrain.X;
				int y = mutableLocationWithTerrain.Y;
				int x2 = mutableLocationWithTerrain.X / 3;
				int y2 = mutableLocationWithTerrain.Y / 3;
				string text = Zone.XYToID("JoppaWorld", x, y, 10);
				The.ZoneManager?.AdjustZoneGenerationTierTo(text);
				encounter(text, mutableLocationWithTerrain, terrainComponents[Location2D.Get(x2, y2)]);
				if (unflagAsMutable)
				{
					mutableMap.RemoveMutableLocation(mutableLocationWithTerrain);
				}
			}
		}
	}

	public void BuildForts(string WorldID)
	{
		if (!(WorldID == "JoppaWorld"))
		{
			return;
		}
		WorldCreationProgress.StepProgress("Building forts...");
		AddMutableEncounterToTerrain("DesertCanyon", 1, (string zoneID, Location2D location, TerrainTravel pTravel) =>
		{
			if (Options.ShowOverlandEncounters && pTravel != null)
			{
				pTravel.ParentObject.Render.RenderString = "]";
				pTravel.ParentObject.Render.SetForegroundColor('m');
			}
			GameObject gameObject = GameObject.Create("Snapjaw Hero Stopsvaalinn");
			gameObject.SetIntProperty("RequireVillagePlacement", 1);
			The.ZoneManager.AddZonePostBuilder(zoneID, "AddObjectBuilder", "Object", The.ZoneManager.CacheObject(gameObject));
			string secretID = AddSecret(zoneID, "the snapjaw who wields {{R-r-K-y-Y sequence|Stopsvalinn}}", new string[4] { "artifact", "tech", "stopsvalinn", "old" }, "Artifacts", "$stopsvalinn");
			AddLocationFinder(zoneID, secretID);
		});
		AddMutableEncounterToTerrain("DesertCanyon", 25, (string zoneID, Location2D location, TerrainTravel pTravel) =>
		{
			if (Options.ShowOverlandEncounters && pTravel != null)
			{
				pTravel.ParentObject.Render.RenderString = "F";
				pTravel.ParentObject.Render.SetForegroundColor('w');
			}
			The.ZoneManager.AddZoneBuilder(zoneID, 6000, "SnapjawStockadeMaker");
			string secretID = AddSecret(zoneID, "a snapjaw fort", new string[3] { "snapjaw", "settlement", "humanoid" }, "Settlements");
			AddLocationFinder(zoneID, secretID);
			GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo
			{
				name = "a snapjaw fort",
				targetZone = zoneID,
				zoneLocation = location,
				secretID = secretID
			};
			worldInfo.enemySettlements.Add(generatedLocationInfo);
			mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
		});
		if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.IsSwitch))
		{
			WorldCreationProgress.StepProgress("Planting orchards...");
		}
		AddMutableEncounterToTerrain("DesertCanyon", 25, (string zoneID, Location2D location, TerrainTravel pTravel) =>
		{
			if (Options.ShowOverlandEncounters && pTravel != null)
			{
				pTravel.ParentObject.Render.RenderString = "&RF";
			}
			The.ZoneManager.AddZoneBuilder(zoneID, 6000, "StarappleFarmMaker");
			string name = SettlementNames.GenerateStarappleFarmName(The.Game.sultanHistory);
			The.ZoneManager.SetZoneName(zoneID, name, null, null, null, null, Proper: true);
			string secretID = AddSecret(zoneID, name, new string[3] { "apple", "settlement", "humanoid" }, "Settlements");
			AddLocationFinder(zoneID, secretID);
			The.ZoneManager.AddZonePostBuilder(zoneID, "IsCheckpoint", "Key", zoneID);
			GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo
			{
				name = name,
				targetZone = zoneID,
				zoneLocation = location,
				secretID = secretID
			};
			worldInfo.friendlySettlements.Add(generatedLocationInfo);
			mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
		});
		WorldCreationProgress.StepProgress("Settling farms...");
		AddMutableEncounterToTerrain("DesertCanyon", 25, (string zoneID, Location2D location, TerrainTravel pTravel) =>
		{
			if (Options.ShowOverlandEncounters && pTravel != null)
			{
				pTravel.ParentObject.Render.RenderString = "&RF";
			}
			The.ZoneManager.AddZoneBuilder(zoneID, 6000, "PigFarmMaker");
			string name = SettlementNames.GeneratePigFarmName(The.Game.sultanHistory);
			The.ZoneManager.SetZoneName(zoneID, name, null, null, null, null, Proper: true);
			string secretID = AddSecret(zoneID, name, new string[3] { "pig", "settlement", "humanoid" }, "Settlements");
			AddLocationFinder(zoneID, secretID);
			The.ZoneManager.AddZonePostBuilder(zoneID, "IsCheckpoint", "Key", zoneID);
			The.ZoneManager.SetZoneProperty(zoneID, "ambient_bed_2", "Sounds/Ambiences/amb_creature_snapjaw");
			GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo
			{
				name = name,
				targetZone = zoneID,
				zoneLocation = location,
				secretID = secretID
			};
			worldInfo.friendlySettlements.Add(generatedLocationInfo);
			mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
		});
	}

	public void BuildFarms(string WorldID)
	{
		if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.IsSwitch))
		{
			WorldCreationProgress.StepProgress("Fencing pigs...");
		}
	}

	public void AddWaterway()
	{
		for (int i = 60; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				if (mutableMap.GetMutable(i, j) == 1)
				{
					string zoneID = Zone.XYToID("JoppaWorld", i, j, 11);
					The.ZoneManager.AddZonePostBuilder(zoneID, "Waterway");
				}
			}
		}
	}

	public void AddOboroqorusLair()
	{
		Location2D location2D = mutableMap.popMutableLocationInArea(81, 54, 94, 68);
		if (location2D == null)
		{
			XRLCore.LogError("worldgen", "no position for oboroqoru's lair");
			return;
		}
		int x = location2D.X;
		int y = location2D.Y;
		Faction faction = Factions.Get("Apes");
		string text = ZoneIDFromXY("JoppaWorld", x, y);
		The.ZoneManager?.AdjustZoneGenerationTierTo(text);
		string text2 = AddSecret(text, "the lair of {{M|Oboroqoru, Ape God}}", new string[3] { "lair", "oboroqoru", "ape" }, "Lairs", "$oboroqorulair");
		(The.ZoneManager.GetZone("JoppaWorld").GetCell(x / 3, y / 3).GetFirstObjectWithPart("TerrainTravel")?.GetPart<TerrainTravel>()).AddEncounter(new EncounterEntry("You discover a lair. Would you like to investigate?", text, "", text2, Optional: true));
		if (Options.ShowOverlandEncounters)
		{
			Render render = The.ZoneManager.GetZone("JoppaWorld").GetCell(x / 3, y / 3).GetObjectInCell(0)
				.Render;
			if (render != null)
			{
				render.RenderString = "A";
				render.ColorString = (render.TileColor = "&M");
			}
		}
		for (int i = -1; i < 1; i++)
		{
			for (int j = -1; j < 1; j++)
			{
				string zoneID = Zone.XYToID("JoppaWorld", x + i, y + j, 10);
				if ((i == 0 && j == 0) || 75.in100())
				{
					The.ZoneManager.AddZoneBuilder(zoneID, 6000, "Torchposts");
				}
			}
		}
		string value = "the lair of Oboroqoru, Ape God";
		for (int k = 0; k < 10; k++)
		{
			string text3 = Zone.XYToID("JoppaWorld", x, y, 10 + k);
			The.ZoneManager.SetZoneNameContext(text3, value);
			faction.HolyPlaces.Add(text3);
			switch (k)
			{
			case 0:
				The.ZoneManager.AddZonePostBuilder(text3, "WallOutcrop", "Blueprint", "Shale");
				AddLocationFinder(text3, text2, 3000);
				continue;
			case 9:
			{
				The.ZoneManager.AddZoneBuilder(text3, -1000, "BasicRoomHall");
				The.ZoneManager.AddZoneBuilder(text3, -1000, "StairsUp");
				GameObject gameObject = GameObject.Create("Oboroqoru");
				gameObject.SetStringProperty("nosecret", text2);
				The.ZoneManager.AddZoneBuilder(text3, -1000, "AddObjectBuilder", "Object", The.ZoneManager.CacheObject(gameObject));
				The.ZoneManager.AddZoneBuilder(text3, -1000, "AddBlueprintBuilder", "Object", "Chest6");
				break;
			}
			default:
				The.ZoneManager.AddZoneBuilder(text3, -1000, "BasicRoomHall");
				The.ZoneManager.AddZoneBuilder(text3, -1000, "StairsUp");
				The.ZoneManager.AddZoneBuilder(text3, -1000, "StairsDown");
				break;
			}
			if (45.in100())
			{
				The.ZoneManager.AddZoneBuilder(text3, -1000, "AddBlueprintBuilder", "Object", "Chest7");
			}
			The.ZoneManager.AddZoneBuilder(text3, -1000, "ApegodCave");
			if (k < 3)
			{
				The.ZoneManager.AddZoneBuilder(text3, -1000, "Population", "Table", "ApeGodLair1");
			}
			else if (k < 6)
			{
				The.ZoneManager.AddZoneBuilder(text3, -1000, "Population", "Table", "ApeGodLair2");
			}
			else
			{
				The.ZoneManager.AddZoneBuilder(text3, -1000, "Population", "Table", "ApeGodLair3");
			}
			AddLocationFinder(text3, text2, 3000, -1000);
		}
	}

	public void AddNephilimLairs()
	{
		AddRermadonLair();
		AddQasQonLair();
		AddShugruithLair();
		AddAgolgotLairSecret();
		AddBethsaidaLairSecret();
	}

	public void AddShugruithLair()
	{
		ZoneManager zoneManager = The.ZoneManager;
		Location2D location2D = peekMutableLocation(null, centerOnly: false);
		int zp = 10;
		string text = ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp);
		The.ZoneManager?.AdjustZoneGenerationTierTo(text);
		string text2 = AddSecret(text, "the mouth of {{M|Shug'ruith the Burrower}}", new string[3] { "lair", "shugruith", "nephilim" }, "Lairs", "$shugruithmouth");
		AddLocationFinder(text, text2, 2000);
		if (zoneManager.GetZone("JoppaWorld").GetCell(location2D.X / 3, location2D.Y / 3).GetFirstObjectWithPart("TerrainTravel")
			.TryGetPart<TerrainTravel>(out var Part))
		{
			Part.AddEncounter(new EncounterEntry("You come upon a yawning pit. Would you like to investigate?", text, "", text2, Optional: true));
		}
		string[] cardinalDirectionList = Directions.CardinalDirectionList;
		string text3 = "D";
		int num = Stat.Random(40, 47);
		int num2 = 0;
		int num3 = 0;
		num2 = Stat.Random(5, 75);
		num3 = Stat.Random(5, 19);
		The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowDescendingMouth", "X", num2, "Y", num3);
		The.ZoneManager.AddZonePostBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowBuilder");
		zp = 11;
		while (zp < num)
		{
			Stat.Random(0, 3);
			if (text3 == "D")
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowAscendingMouth", "X", num2, "Y", num3);
			}
			if (text3 == "S")
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowNorthMouth");
			}
			if (text3 == "N")
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowSouthMouth");
			}
			if (text3 == "W")
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowEastMouth");
			}
			if (text3 == "E")
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowWestMouth");
			}
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowBuilder");
			text3 = ((Stat.Random(1, 100) > 50 && zp != num - 1) ? cardinalDirectionList.GetRandomElement() : "D");
			while (true)
			{
				if (text3 == "D")
				{
					num2 = Stat.Random(5, 75);
					num3 = Stat.Random(5, 19);
					The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowDescendingMouth", "X", num2, "Y", num3);
					zp++;
					break;
				}
				Location2D location2D2 = location2D.FromDirection(text3);
				if (location2D2 == null || location2D2.X < 0 || location2D2.Y < 0 || location2D2.X >= 240 || location2D2.Y >= 75 || mutableMap.GetMutable(location2D2.X, location2D2.Y) != 1)
				{
					text3 = "D";
					continue;
				}
				if (text3 == "N")
				{
					The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowNorthMouth");
				}
				if (text3 == "S")
				{
					The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowSouthMouth");
				}
				if (text3 == "E")
				{
					The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowEastMouth");
				}
				if (text3 == "W")
				{
					The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowWestMouth");
				}
				location2D = location2D2;
				break;
			}
		}
		The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowAscendingMouth");
		The.ZoneManager.AddZonePostBuilder(ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp), "ShugBurrowBuilder");
		string text4 = ZoneIDFromXYz("JoppaWorld", location2D.X, location2D.Y, zp);
		The.ZoneManager?.AdjustZoneGenerationTierTo(text4);
		AddSecret(text4, "the cradle of {{M|Shug'ruith the Burrower}}", new string[3] { "lair", "shugruith", "nephilim" }, "Lairs", "$shugruithlair");
		for (int i = 0; i <= 3; i++)
		{
			string zoneID = Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, zp + i);
			zoneManager.AddZonePostBuilder(zoneID, "ShugLair", "l", i);
			zoneManager.SetZoneProperty(zoneID, "ChasmColor", "&m");
		}
	}

	public void AddQasQonLair()
	{
		ZoneManager zoneManager = The.ZoneManager;
		Location2D location2D = popMutableLocationOfTerrain("MoonStair", null, centerOnly: false);
		int x = location2D.X;
		int y = location2D.Y;
		string text = ZoneIDFromXY("JoppaWorld", x, y);
		The.ZoneManager?.AdjustZoneGenerationTierTo(text);
		string secret = AddSecret(text, "the chuppah of {{M|Girsh Qas and Girsh Qon}}", new string[3] { "lair", "qasqon", "nephilim" }, "Lairs", "$qasqonlair");
		if (zoneManager.GetZone("JoppaWorld").GetCell(x / 3, y / 3).GetFirstObjectWithPart("TerrainTravel")
			.TryGetPart<TerrainTravel>(out var Part))
		{
			Part.AddEncounter(new EncounterEntry("You discover a lair. Would you like to investigate?", text, "", secret, Optional: true));
		}
		for (int i = 0; i <= 3; i++)
		{
			string zoneID = Zone.XYToID("JoppaWorld", x, y, 10 + i);
			if (i == 3)
			{
				zoneManager.AddZoneBuilder(zoneID, 6000, "QasQonLair", "Nephal", true);
			}
			else
			{
				zoneManager.AddZoneBuilder(zoneID, 6000, "QasQonLair");
			}
			zoneManager.SetZoneProperty(zoneID, "ChasmColor", "&m");
		}
	}

	public void AddRermadonLair()
	{
		ZoneManager zoneManager = The.ZoneManager;
		Location2D location2D = popMutableLocationOfTerrain("PalladiumReef", null, centerOnly: false);
		int x = location2D.X;
		int y = location2D.Y;
		string text = ZoneIDFromXY("JoppaWorld", x, y);
		The.ZoneManager?.AdjustZoneGenerationTierTo(text);
		string secret = AddSecret(text, "the cradle of {{M|Girsh Rermadon}}", new string[3] { "lair", "rermadon", "nephilim" }, "Lairs", "$rermadonlair");
		if (zoneManager.GetZone("JoppaWorld").GetCell(x / 3, y / 3).GetFirstObjectWithPart("TerrainTravel")
			.TryGetPart<TerrainTravel>(out var Part))
		{
			Part.AddEncounter(new EncounterEntry("You discover a lair. Would you like to investigate?", text, "", secret, Optional: true));
		}
		for (int i = 0; i <= 3; i++)
		{
			string zoneID = Zone.XYToID("JoppaWorld", x, y, 10 + i);
			if (i == 3)
			{
				zoneManager.AddZoneBuilder(zoneID, 6000, "RermadonLair", "Nephal", true);
			}
			else
			{
				zoneManager.AddZoneBuilder(zoneID, 6000, "RermadonLair");
			}
			zoneManager.SetZoneProperty(zoneID, "ChasmColor", "&m");
		}
	}

	public void AddAgolgotLairSecret()
	{
		Cell cell = WorldZone.FindObject("TerrainGolgotha")?.CurrentCell;
		if (cell == null)
		{
			MetricsManager.LogError("Unable to find world map terrain of Golgotha.");
			return;
		}
		int oracleIntColumn = ZoneBuilderSandbox.GetOracleIntColumn(cell.X, cell.Y, 1, 1, 40, 45);
		string text = ZoneID.Assemble(World, cell.X, cell.Y, 1, 1, oracleIntColumn - 3 + 1);
		The.ZoneManager?.AdjustZoneGenerationTierTo(text);
		AddSecret(text, "the cradle of {{M|Girsh Agolgot}}", new string[3] { "lair", "agolgot", "nephilim" }, "Lairs", "$agolgotlair");
	}

	public void AddBethsaidaLairSecret()
	{
		Cell cell = WorldZone.FindObject("TerrainBethesdaSusa")?.CurrentCell;
		if (cell == null)
		{
			MetricsManager.LogError("Unable to find world map terrain of Bethesda Susa.");
			return;
		}
		int oracleIntColumn = ZoneBuilderSandbox.GetOracleIntColumn(cell.X, cell.Y, 1, 1, 40, 45);
		string text = ZoneID.Assemble(World, cell.X, cell.Y, 1, 1, oracleIntColumn - 3 + 1);
		The.ZoneManager?.AdjustZoneGenerationTierTo(text);
		AddSecret(text, "the cradle of {{M|Girsh Bethsaida}}", new string[3] { "lair", "bethsaida", "nephilim" }, "Lairs", "$bethsaidalair");
	}

	public void AddMamonVillage()
	{
		List<Location2D> list = BuildMamonRiver("JoppaWorld", 81, 62, 1, 8, 20, 3);
		while (list.Count <= 18)
		{
			list = BuildMamonRiver("JoppaWorld", 81, 62, 1, 8, 20, 3);
		}
		int num = Stat.Random(3, 5);
		int x = list[num].X;
		int y = list[num].Y;
		string zoneID = ZoneIDFromXY("JoppaWorld", x, y);
		The.ZoneManager.AddZonePostBuilderAfterTerrain(zoneID, "IdolFight");
		num += Stat.Random(3, 5);
		x = list[num].X;
		y = list[num].Y;
		zoneID = ZoneIDFromXY("JoppaWorld", x, y);
		The.ZoneManager.AddZonePostBuilderAfterTerrain(zoneID, "MinorRazedGoatfolkVillage");
		num += Stat.Random(3, 5);
		x = list[num].X;
		y = list[num].Y;
		zoneID = ZoneIDFromXY("JoppaWorld", x, y);
		The.ZoneManager.AddZonePostBuilderAfterTerrain(zoneID, "WildWatervineMerchant");
		num++;
		if (list[num - 1].X < list[num].X)
		{
			The.ZoneManager.AddZonePostBuilderAfterTerrain(zoneID, "SmokingAreaE");
		}
		if (list[num - 1].Y < list[num].Y)
		{
			The.ZoneManager.AddZonePostBuilderAfterTerrain(zoneID, "SmokingAreaS");
		}
		if (list[num - 1].Y > list[num].Y)
		{
			The.ZoneManager.AddZonePostBuilderAfterTerrain(zoneID, "SmokingAreaN");
		}
		x = list[num].X;
		y = list[num].Y;
		zoneID = ZoneIDFromXY("JoppaWorld", x, y);
		The.ZoneManager.AddZonePostBuilderAfterTerrain(zoneID, "RazedGoatfolkVillage");
		if (Options.ShowOverlandEncounters)
		{
			The.ZoneManager.GetZone("JoppaWorld").GetCell(x / 3, y / 3).GetObjectInCell(0)
				.GetPart<Render>()
				.RenderString = "&Mg";
		}
		AddSecret(Zone.XYToID("JoppaWorld", x, y, 10), "the village lair of {{M|Mamon Souldrinker}}", new string[4] { "lair", "mammon", "goatfolk", "humanoid" }, "Lairs", "$mamonvillage");
		AddLocationFinder(zoneID, "$mamonvillage");
	}

	public string ZoneIDFromXYz(string World, int xp, int yp, int zp)
	{
		int parasangX = (int)Math.Floor((float)xp / 3f);
		int parasangY = (int)Math.Floor((float)yp / 3f);
		return ZoneID.Assemble(World, parasangX, parasangY, xp % 3, yp % 3, zp);
	}

	public string ZoneIDFromXY(string World, int xp, int yp)
	{
		int parasangX = (int)Math.Floor((float)xp / 3f);
		int parasangY = (int)Math.Floor((float)yp / 3f);
		return ZoneID.Assemble(World, parasangX, parasangY, xp % 3, yp % 3, 10);
	}

	public string AddSecret(string secretZone, string name, string[] adj, string category, string secretid = null, bool revealed = false, bool silent = false)
	{
		if (secretid != null && JournalAPI.GetMapNote(secretid) != null)
		{
			Debug.LogWarning("dupe secret: " + secretid);
			return secretid;
		}
		string objectTypeForZone = ZoneManager.GetObjectTypeForZone(secretZone);
		if (objectTypeForZone != "")
		{
			List<string> list = new List<string>(adj);
			string tag = GameObjectFactory.Factory.Blueprints[objectTypeForZone].GetTag("SecretAttributes");
			if (tag != "")
			{
				list.AddRange(tag.Split(','));
			}
			else
			{
				list.Add(objectTypeForZone.Replace("Terrain", "").Replace("Watervine", "Saltmarsh").Replace("1", "")
					.Replace("2", "")
					.Replace("3", "")
					.Replace("4", "")
					.Replace("5", "")
					.Replace("6", "")
					.Replace("7", "")
					.Replace("8", "")
					.Replace("9", "")
					.Replace("0", "")
					.ToLower());
			}
			adj = list.ToArray();
		}
		if (secretid == null)
		{
			secretid = Guid.NewGuid().ToString();
		}
		JournalAPI.AddMapNote(secretZone, name, category, adj, secretid, revealed, sold: false, 0L, silent);
		return secretid;
	}

	public void AddHindrenVillage(string WorldID)
	{
		Location2D location2D = popMutableBlockOfTerrain("Flowerfields");
		string text = Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, 10);
		Cell cell = The.ZoneManager.GetZone(WorldID).GetCell(location2D.X / 3, location2D.Y / 3);
		The.ZoneManager.SetZoneProperty(text, "NoBiomes", "Yes");
		cell.GetFirstObjectWithPart("TerrainTravel").AddPart(new BeyLahTerrain());
		The.ZoneManager.ClearZoneBuilders(text);
		The.ZoneManager.AddZoneBuilder(text, 4900, "ClearAll");
		The.ZoneManager.AddZonePostBuilder(text, "MapBuilder", "FileName", "BeyLah.rpm");
		The.ZoneManager.AddZonePostBuilder(text, "HindrenClues");
		The.ZoneManager.AddZonePostBuilder(text, "Music", "Track", "Music/Bey Lah Heritage II");
		The.ZoneManager.AddZonePostBuilder(text, "AddWidgetBuilder", "Blueprint", "BeyLahSurface");
		The.ZoneManager.AddZonePostBuilder(text, "IsCheckpoint", "Key", text);
		The.ZoneManager.SetZoneName(text, "Bey Lah", null, null, null, null, Proper: true);
		The.ZoneManager.SetZoneIncludeStratumInZoneDisplay(text, false);
		The.ZoneManager.SetZoneProperty(text, "SkipTerrainBuilders", true);
		string[] directionList = Directions.DirectionList;
		foreach (string d in directionList)
		{
			Location2D location2D2 = location2D.FromDirection(d);
			string zoneID = Zone.XYToID("JoppaWorld", location2D2.X, location2D2.Y, 10);
			The.ZoneManager.AdjustZoneGenerationTierTo(zoneID);
			The.ZoneManager.AddZoneBuilder(zoneID, 4900, "ClearAll");
			The.ZoneManager.AddZoneBuilder(zoneID, 5000, "BeyLahOutskirts");
			The.ZoneManager.AddZoneBuilder(zoneID, 5000, "Music", "Track", "Music/Bey Lah Heritage II");
			The.ZoneManager.SetZoneName(zoneID, "outskirts", "Bey Lah", "some");
			The.ZoneManager.SetZoneIncludeStratumInZoneDisplay(zoneID, false);
		}
		List<Location2D> source = new List<Location2D>();
		List<int> list = new List<int> { 0, 1, 2, 3 };
		list.ShuffleInPlace();
		int num = 4;
		bool flag = false;
		while (true)
		{
			foreach (int item in list)
			{
				source = BuildEskhindRoad("JoppaWorld", location2D.X, location2D.Y, item, 8, 4, -1, layRoad: false);
				Location2D location2D3 = source.Last();
				if (location2D3.Distance(location2D) >= num && mutableMap.GetMutable(Location2D.Get(location2D3.X, location2D3.Y)) > 0)
				{
					flag = true;
					source = BuildEskhindRoad("JoppaWorld", location2D.X, location2D.Y, item, 8, 4, -1, layRoad: true);
					break;
				}
			}
			if (flag)
			{
				break;
			}
			num--;
		}
		string text2 = Zone.XYToID("JoppaWorld", source.Last().X, source.Last().Y, 10);
		The.ZoneManager.AdjustZoneGenerationTierTo(text2);
		The.ZoneManager.ClearZoneBuilders(text2);
		The.ZoneManager.AddZoneBuilder(text2, 6000, "ClearAll");
		The.ZoneManager.AddZoneBuilder(text2, 6000, "MapBuilder", "FileName", "HollowTree.rpm");
		The.ZoneManager.AddZoneBuilder(text2, 6000, "Music", "Track", "Music/Overworld");
		game.SetStringGameState("HollowTreeZoneId", text2);
		The.ZoneManager.SetZoneProperty(text2, "NoBiomes", "Yes");
		mutableMap.SetMutable(Location2D.Get(source.Last().X, source.Last().Y), 0);
		GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
		generatedLocationInfo.name = "Bey Lah";
		generatedLocationInfo.targetZone = text;
		generatedLocationInfo.zoneLocation = Location2D.Get(location2D.X, location2D.Y);
		generatedLocationInfo.secretID = null;
		worldInfo.villages.Add(generatedLocationInfo);
		mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
		game.SetStringGameState("BeyLahZoneID", Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, 10));
		AddSecret(Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, 10), "Bey Lah", new string[2] { "settlement", "hindren" }, "Settlements", "$beylah");
		JournalAPI.GetMapNote("$beylah").Attributes.Add("nobuy:Hindren");
	}

	public void AddHydropon(string WorldID)
	{
		Location2D location2D = popMutableLocationOfTerrain("PalladiumReef", null, centerOnly: false);
		string text = Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, 10);
		Cell cell = The.ZoneManager.GetZone(WorldID).GetCell(location2D.X / 3, location2D.Y / 3);
		The.ZoneManager.SetZoneProperty(text, "NoBiomes", "Yes");
		The.ZoneManager.SetZoneProperty(text, "NoSvardymStorm", "Yes");
		cell.GetFirstObjectWithPart("TerrainTravel").AddPart(new HydroponTerrain());
		The.ZoneManager.ClearZoneBuilders(text);
		The.ZoneManager.AddZonePostBuilder(text, "ClearAll");
		The.ZoneManager.AddZonePostBuilder(text, "Reef");
		The.ZoneManager.AddZonePostBuilder(text, "MapBuilder", "FileName", "Hydropon.rpm", "ClearBeforePlace", true);
		The.ZoneManager.SetZoneName(text, "Hydropon", null, null, null, "the", Proper: true);
		The.ZoneManager.SetZoneIncludeStratumInZoneDisplay(text, false);
		The.ZoneManager.SetZoneProperty(text, "SkipTerrainBuilders", true);
		GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
		generatedLocationInfo.name = "Hydropon";
		generatedLocationInfo.targetZone = text;
		generatedLocationInfo.zoneLocation = location2D;
		generatedLocationInfo.secretID = null;
		worldInfo.villages.Add(generatedLocationInfo);
		mutableMap.SetMutable(generatedLocationInfo.zoneLocation, 0);
		game.SetStringGameState("HydroponZoneID", Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, 10));
		string text2 = Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, 10);
		The.ZoneManager.AdjustZoneGenerationTierTo(text2);
		AddSecret(text2, "the Hydropon", new string[1] { "settlement" }, "Settlements", "$hydropon");
		AddLocationFinder(text2, "$hydropon");
	}

	public void AddHamilcrabSecret()
	{
		Cell cell = WorldZone.GetFirstObject("TerrainAsphaltMines")?.CurrentCell;
		if (cell != null)
		{
			string secretZone = Zone.XYToID(WorldZone.ZoneWorld, cell.X * 3 + 1, cell.Y * 3 + 1, 40);
			AddSecret(secretZone, "Hamilcrab's shop", new string[3] { "special", "oddity", "merchant" }, "Merchants", "$hamilcrab");
		}
	}

	public void AddGreatMachineArtifact()
	{
		Location2D locationOfTier = getLocationOfTier(1, 8);
		string text = Zone.XYToID("JoppaWorld", locationOfTier.X * 3 + 1, locationOfTier.Y * 3 + 1, Stat.Random(40, 50));
		The.ZoneManager.AdjustZoneGenerationTierTo(text);
		string blueprint = PopulationManager.RollOneFrom("StaticObjectsTable:GreatMachine_Artifact").Blueprint;
		GameObjectBlueprint blueprint2 = GameObjectFactory.Factory.GetBlueprint(blueprint);
		The.ZoneManager.AddZonePostBuilder(text, "PlaceRelicBuilder", "Blueprint", blueprint);
		AddSecret(text, blueprint2.CachedDisplayNameStripped, new string[1] { "artifact" }, "Artifacts", "$greatmachineartifact");
	}

	public List<Location2D> BuildEskhindRoad(string WorldID, int StartX, int StartY, int Direction, int Bias, int MinimumLength, int ExcludeDirection, bool layRoad)
	{
		if (layRoad)
		{
			string zoneID = ZoneIDFromXY(WorldID, StartX, StartY);
			switch (Direction)
			{
			case 0:
				game.SetStringGameState("EskhindRoadDirection", "north");
				The.ZoneManager.AddZoneConnection(zoneID, "-", 43, 12, "Road");
				RoadSystem[StartX, StartY] |= ROAD_NORTH;
				break;
			case 1:
				game.SetStringGameState("EskhindRoadDirection", "east");
				The.ZoneManager.AddZoneConnection(zoneID, "-", 72, 16, "Road");
				RoadSystem[StartX, StartY] |= ROAD_EAST;
				break;
			case 2:
				game.SetStringGameState("EskhindRoadDirection", "south");
				The.ZoneManager.AddZoneConnection(zoneID, "-", 59, 19, "Road");
				RoadSystem[StartX, StartY] |= ROAD_SOUTH;
				break;
			case 3:
				game.SetStringGameState("EskhindRoadDirection", "west");
				The.ZoneManager.AddZoneConnection(zoneID, "-", 20, 9, "Road");
				RoadSystem[StartX, StartY] |= ROAD_WEST;
				break;
			}
			if ((RoadSystem[StartX, StartY] & ROAD_NORTH) != 0)
			{
				The.ZoneManager.AddZoneConnection(zoneID, "-", 36, 0, "RoadNorthMouth");
				The.ZoneManager.AddZoneConnection(zoneID, "n", 36, 24, "RoadSouthMouth");
			}
			if ((RoadSystem[StartX, StartY] & ROAD_EAST) != 0)
			{
				The.ZoneManager.AddZoneConnection(zoneID, "-", 79, 13, "RoadEastMouth");
				The.ZoneManager.AddZoneConnection(zoneID, "e", 0, 13, "RoadWestMouth");
			}
			if ((RoadSystem[StartX, StartY] & ROAD_SOUTH) != 0)
			{
				The.ZoneManager.AddZoneConnection(zoneID, "-", 57, 24, "RoadSouthMouth");
				The.ZoneManager.AddZoneConnection(zoneID, "s", 57, 0, "RoadNorthMouth");
			}
			if ((RoadSystem[StartX, StartY] & ROAD_WEST) != 0)
			{
				The.ZoneManager.AddZoneConnection(zoneID, "-", 0, 8, "RoadWestMouth");
				The.ZoneManager.AddZoneConnection(zoneID, "w", 79, 8, "RoadEastMouth");
			}
			The.ZoneManager.AddZonePostBuilder(zoneID, "RoadBuilder", "ClearSolids", false, "Noise", false);
			RoadSystem[StartX, StartY] |= ROAD_START;
		}
		List<Location2D> list = new List<Location2D>();
		ContinueEskhindRoad(StartX, StartY, Direction, 0, Bias, MinimumLength, ExcludeDirection, list, layRoad);
		return list;
	}

	public void ContinueEskhindRoad(int StartX, int StartY, int Direction, int Depth, int Bias, int MinimumLength, int ExcludeDirection, List<Location2D> Points, bool layRoad)
	{
		int num = StartX;
		int num2 = StartY;
		if (Direction == 0)
		{
			num2--;
		}
		if (Direction == 2)
		{
			num2++;
		}
		if (Direction == 1)
		{
			num++;
		}
		if (Direction == 3)
		{
			num--;
		}
		if (num < 0 || num2 < 0 || num >= 240 || num2 >= 75)
		{
			return;
		}
		if (layRoad)
		{
			mutableMap.RemoveMutableLocation(Location2D.Get(num, num2));
		}
		Points.Add(Location2D.Get(num, num2));
		if (Depth > MinimumLength && Stat.Random(0, 100) < 50 + (Depth - MinimumLength) * 25)
		{
			if (layRoad)
			{
				The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadStartMouth");
				RoadSystem[num, num2] |= ROAD_START;
				The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
			}
			return;
		}
		int num3 = Direction;
		if (Bias <= 0 && 50.in100())
		{
			int num4 = Stat.Random(0, 1);
			if (num4 == 0)
			{
				num3--;
			}
			if (num4 == 1)
			{
				num3++;
			}
			if (num3 < 0)
			{
				num3 = 3;
			}
			if (num3 > 3)
			{
				num3 = 0;
			}
			if (num3 == ExcludeDirection)
			{
				num3 = Direction;
			}
		}
		if ((Direction == 0) & layRoad)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadSouthMouth");
			RoadSystem[num, num2] |= ROAD_SOUTH;
		}
		if ((Direction == 1) & layRoad)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadWestMouth");
			RoadSystem[num, num2] |= ROAD_WEST;
		}
		if ((Direction == 2) & layRoad)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadNorthMouth");
			RoadSystem[num, num2] |= ROAD_NORTH;
		}
		if ((Direction == 3) & layRoad)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadEastMouth");
			RoadSystem[num, num2] |= ROAD_EAST;
		}
		int num5 = num;
		int num6 = num2;
		if (num3 == 0)
		{
			num6--;
		}
		if (num3 == 2)
		{
			num6++;
		}
		if (num3 == 1)
		{
			num5++;
		}
		if (num3 == 3)
		{
			num5--;
		}
		if (num6 < 0 || num5 < 0 || num6 == 75 || num5 == 240 || (Depth > 0 && RoadSystem[num5, num6] != 0))
		{
			if (layRoad)
			{
				RoadSystem[num, num2] |= ROAD_START;
				The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadStartMouth");
			}
			return;
		}
		if ((num3 == 0) & layRoad)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadNorthMouth");
			RoadSystem[num, num2] |= ROAD_NORTH;
		}
		if ((num3 == 1) & layRoad)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadEastMouth");
			RoadSystem[num, num2] |= ROAD_EAST;
		}
		if ((num3 == 2) & layRoad)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadSouthMouth");
			RoadSystem[num, num2] |= ROAD_SOUTH;
		}
		if ((num3 == 3) & layRoad)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadWestMouth");
			RoadSystem[num, num2] |= ROAD_WEST;
		}
		bool flag = false;
		if (layRoad)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
		}
		if (layRoad && Options.ShowOverlandEncounters)
		{
			Render part = The.ZoneManager.GetZone("JoppaWorld").GetCell(num / 3, num2 / 3).GetObjectInCell(0)
				.GetPart<Render>();
			part.RenderString = "=";
			if (flag)
			{
				part.RenderString = "t";
			}
		}
		ContinueEskhindRoad(num, num2, num3, Depth + 1, Bias - 1, MinimumLength, ExcludeDirection, Points, layRoad);
	}

	public void AddYonderPath(string WorldID)
	{
		Location2D location = The.ZoneManager.GetZone(WorldID).GetFirstObject("TerrainFungalCenter").CurrentCell.Location;
		Location2D location2D = Location2D.Get(location.X * 3 + 1, location.Y * 3 + 1);
		string text = ZoneIDFromXY(WorldID, location2D.X, location2D.Y);
		The.ZoneManager.AddZoneBuilder(text, 6000, "FungalTrailExileCorpse");
		The.ZoneManager.AddZoneConnection(text, "-", Stat.Random(20, 50), Stat.Random(10, 15), "FungalTrailStart");
		mutableMap.SetMutable(location2D, 0);
		List<Location2D> list = new List<Location2D>(4) { location2D };
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num < 3 && num4 < 100)
		{
			string randomCardinalDirection = Directions.GetRandomCardinalDirection();
			num2 = list[num].X;
			num3 = list[num].Y;
			Directions.ApplyDirection(randomCardinalDirection, ref num2, ref num3);
			Location2D location2D2 = Location2D.Get(num2, num3);
			if (!list.Contains(location2D2) && mutableMap.GetMutable(location2D2) > 0)
			{
				int x = Stat.Random(10, 70);
				int y = Stat.Random(5, 20);
				switch (randomCardinalDirection)
				{
				case "N":
					The.ZoneManager.AddZoneConnection(text, "-", x, 0, "FungalTrailNorthMouth");
					The.ZoneManager.AddZoneConnection(text, randomCardinalDirection, x, 24, "FungalTrailSouthMouth");
					break;
				case "S":
					The.ZoneManager.AddZoneConnection(text, "-", x, 24, "FungalTrailSouthMouth");
					The.ZoneManager.AddZoneConnection(text, randomCardinalDirection, x, 0, "FungalTrailNorthMouth");
					break;
				case "E":
					The.ZoneManager.AddZoneConnection(text, "-", 79, y, "FungalTrailEastMouth");
					The.ZoneManager.AddZoneConnection(text, randomCardinalDirection, 0, y, "FungalTrailWestMouth");
					break;
				case "W":
					The.ZoneManager.AddZoneConnection(text, "-", 0, y, "FungalTrailWestMouth");
					The.ZoneManager.AddZoneConnection(text, randomCardinalDirection, 79, y, "FungalTrailEastMouth");
					break;
				}
				list.Add(location2D2);
				The.ZoneManager.AddZoneBuilder(text, 6000, "FungalTrailBuilder");
				mutableMap.SetMutable(location2D2, 0);
				text = ZoneIDFromXY(WorldID, num2, num3);
				num++;
			}
			num4++;
		}
		The.ZoneManager.AddZoneBuilder(text, 6000, "FungalTrailKlanqHut");
		The.ZoneManager.AddZoneBuilder(text, 6000, "FungalTrailBuilder");
		The.ZoneManager.AddZoneConnection(text, "-", Stat.Random(20, 50), Stat.Random(10, 15), "FungalTrailStart");
		game.SetStringGameState("FungalTrailEnd", text);
	}

	public void AddStaticEncounters(string WorldID)
	{
		The.ZoneManager.GetZone("JoppaWorld");
		WorldCreationProgress.StepProgress("Generating village history...");
		if (!(WorldID == "JoppaWorld"))
		{
			return;
		}
		Location2D location2D = popMutableLocationOfTerrain("Saltmarsh");
		string text = Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, 10);
		GameObject gameObject = The.ZoneManager.GetZone(WorldID).GetCell(location2D.X / 3, location2D.Y / 3).Objects[0];
		The.ZoneManager.AddZonePostBuilder(text, "AddBlueprintBuilder", "Object", "SkrefCorpse");
		if (Options.ShowOverlandEncounters)
		{
			gameObject.Render.RenderString = "&r%";
		}
		AddSecret(text, "some flattened remains", new string[3] { "encounter", "special", "oddity" }, "Oddities", "$skrefcorpse");
		Location2D location2D2 = popMutableLocationOfTerrain("DesertCanyon");
		string text2 = Zone.XYToID("JoppaWorld", location2D2.X, location2D2.Y, 10);
		gameObject = The.ZoneManager.GetZone(WorldID).GetCell(location2D2.X / 3, location2D2.Y / 3).Objects[0];
		The.Game.SetStringGameState("$TrembleEntranceEncounter", text2);
		The.ZoneManager.AddZonePostBuilder(text2, "TrembleEntrance");
		if (Options.ShowOverlandEncounters)
		{
			gameObject.Render.RenderString = "&Wx";
		}
		Location2D location2D3 = popMutableLocationOfTerrain("Saltmarsh");
		string text3 = Zone.XYToID("JoppaWorld", location2D3.X, location2D3.Y, 10);
		gameObject = The.ZoneManager.GetZone(WorldID).GetCell(location2D3.X / 3, location2D3.Y / 3).Objects[0];
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				string zoneID = Zone.XYToID("JoppaWorld", location2D3.X + i, location2D3.Y + j, 10);
				The.ZoneManager.AddZonePostBuilder(zoneID, "DenseBrinestalk");
			}
		}
		The.ZoneManager.AddZonePostBuilder(text3, "AddBlueprintBuilder", "Object", "OasisGlowpad");
		if (Options.ShowOverlandEncounters)
		{
			gameObject.Render.RenderString = "&W$";
		}
		AddSecret(text3, "a secluded merchant from the Consortium of Phyta", new string[4] { "encounter", "special", "oddity", "consortium" }, "Oddities", "$glowpadmerchant");
		Location2D location2D4 = mutableMap.popMutableLocationInArea(0, 0, 119, 74);
		int num = location2D4.X / 3;
		int num2 = location2D4.Y / 3;
		string text4 = Zone.XYToID("JoppaWorld", location2D4.X, location2D4.Y, 10);
		The.ZoneManager.AdjustZoneGenerationTierTo(text4);
		The.Game.SetIntGameState("RuinofHouseIsner_xCoordinate", num);
		The.Game.SetIntGameState("RuinofHouseIsner_yCoordinate", num2);
		Event.ResetPool();
		The.ZoneManager.AddZonePostBuilder(text4, "AddBlueprintBuilder", "Object", "ChestIsner");
		if (Options.ShowOverlandEncounters)
		{
			The.ZoneManager.GetZone(WorldID).GetCell(num, num2).Objects[0].Render.RenderString = "&M*";
		}
		AddSecret(text4, "the Ruin of House Isner", new string[7] { "artifact", "special", "tech", "saltmarsh", "saltdunes", "desertcanyons", "flowerfields" }, "Artifacts", "$ruinofhouseisner");
		Location2D location2D5 = popMutableLocationOfTerrain("LakeHinnom");
		string text5 = Zone.XYToID("JoppaWorld", location2D5.X, location2D5.Y, 10);
		The.ZoneManager.AdjustZoneGenerationTierTo(text5);
		The.Game.SetStringGameState("Recorporealization_ZoneID", text5);
		The.ZoneManager.AddZonePostBuilder(text5, "AddWidgetBuilder", "Blueprint", "RecorporealizationBoothSpawner");
		The.ZoneManager.SetZoneName(text5, "Gyl", null, null, null, null, Proper: true);
		AddSecret(text5, "Recoming nook at Gyl", new string[2] { "oddity", "ruins" }, "Oddities", "$recomingnook");
		AddLocationFinder(text5, "$recomingnook");
		WorldZone.GetCell(location2D5.X / 3, location2D5.Y / 3).GetFirstObjectWithPart("TerrainTravel").GetPart<TerrainTravel>()
			.AddEncounter(new EncounterEntry("You notice some ruins nearby. Would you like to investigate?", text5, "", "$recomingnook", Optional: true));
		AddHamilcrabSecret();
	}

	public void BuildSecrets(string WorldID)
	{
		if (!(WorldID == "JoppaWorld"))
		{
			return;
		}
		Location2D location2D;
		string text;
		for (int i = 0; i < 32; i++)
		{
			location2D = mutableMap.popMutableLocationInArea(0, 0, 119, 74);
			text = Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, 10);
			The.ZoneManager.AdjustZoneGenerationTierTo(text);
			AddSecret(text, "a {{w|dromad}} caravan", new string[2] { "dromad", "merchant" }, "Merchants", "DromadMerchant_" + text);
			The.ZoneManager.AddZonePostBuilder(text, "AddBlueprintBuilder", "Object", "DromadTrader1");
		}
		location2D = getLocationOfTier(4, 6);
		location2D = Location2D.Get(location2D.X * 3 + 1, location2D.Y * 3 + 1);
		text = Zone.XYToID("JoppaWorld", location2D.X, location2D.Y, Stat.Random(10, 19));
		The.ZoneManager.AdjustZoneGenerationTierTo(text);
		The.ZoneManager.AddZonePostBuilder(text, "PlaceRelicBuilder", "Relic", The.ZoneManager.CacheObject(GameObject.Create("Kindrish")));
		AddSecret(text, "Kindrish, the ancestral bracelet of the hindren", new string[2] { "artifact", "kindrish" }, "Artifacts", "~kindrish");
		AddGreatMachineArtifact();
		foreach (KeyValuePair<string, IBiome> biome in BiomeManager.Biomes)
		{
			List<Point2D> list = new List<Point2D>();
			for (int j = 0; j < 240; j++)
			{
				for (int k = 0; k < 75; k++)
				{
					text = Zone.XYToID("JoppaWorld", j, k, 10);
					if (The.ZoneManager.CheckBiomesAllowed(text) && biome.Value.IsNotable(text))
					{
						list.Add(new Point2D(j, k));
					}
				}
			}
			foreach (Point2D item in list)
			{
				text = Zone.XYToID("JoppaWorld", item.x, item.y, 10);
				string name;
				if (string.Equals(biome.Key, "slimy", StringComparison.CurrentCultureIgnoreCase))
				{
					name = "a {{g|slime}} bog";
				}
				else if (string.Equals(biome.Key, "tarry", StringComparison.CurrentCultureIgnoreCase))
				{
					name = "some {{fiery|flaming}} {{K|tar}} pits";
				}
				else if (string.Equals(biome.Key, "rusty", StringComparison.CurrentCultureIgnoreCase))
				{
					name = "a {{rusty|rust}} bog";
				}
				else
				{
					name = ((!string.Equals(biome.Key, "fungal", StringComparison.CurrentCultureIgnoreCase)) ? (Grammar.A(biome.Key) + " region") : "a {{m|fungus}} forest");
				}
				string secretID = AddSecret(text, name, new string[2]
				{
					"biome",
					biome.Key.ToLower()
				}, "Natural Features", "Biome_" + text);
				AddLocationFinder(text, secretID);
			}
		}
		List<Point3D> list2 = new List<Point3D>(800);
		for (int l = 0; l < 240; l++)
		{
			for (int m = 0; m < 75; m++)
			{
				for (int n = 10; n <= 29; n++)
				{
					int num = ((FungalBiome.BiomeLevels != null) ? FungalBiome.BiomeLevels[l, m, n % 10] : BiomeManager.BiomeValue("Fungal", Zone.XYToID("JoppaWorld", l, m, n)));
					if (num >= 1)
					{
						if (n == 10)
						{
							list2.Add(new Point3D(l, m, n));
						}
						if (n > 10 && 5.in100())
						{
							list2.Add(new Point3D(l, m, n));
						}
					}
				}
			}
		}
		string[] array = new string[14]
		{
			"waterLichen Minor", "honeyLichen Minor", "lavaLichen Minor", "acidLichen Minor", "wineLichen Minor", "slimeLichen Minor", "ciderLichen Minor", "gelLichen Minor", "asphaltLichen Minor", "saltLichen Minor",
			"oilLichen Minor", "sapLichen Minor", "waxLichen Minor", "inkLichen Minor"
		};
		list2.ShuffleInPlace();
		int num2 = 0;
		string[] array2 = array;
		foreach (string objectBlueprint in array2)
		{
			int num4 = Stat.Random(3, 4);
			for (int num5 = 0; num5 < num4; num5++)
			{
				if (num2 >= list2.Count)
				{
					break;
				}
				text = Zone.XYToID("JoppaWorld", list2[num2].x, list2[num2].y, list2[num2].z);
				The.ZoneManager.AdjustZoneGenerationTierTo(text);
				GameObject gameObject = GameObjectFactory.Factory.CreateObject(objectBlueprint);
				string text2 = "surface";
				if (list2[num2].z > 10)
				{
					text2 = "underground";
				}
				The.ZoneManager.AddZonePostBuilder(text, "AddObjectBuilder", "Object", The.ZoneManager.CacheObject(gameObject));
				AddSecret(text, gameObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false), new string[3]
				{
					"weep",
					text2,
					gameObject.GetPart<LiquidFont>().Liquid
				}, "Natural Features", gameObject.GetPart<SecretObject>().id);
				num2++;
			}
		}
	}

	public void BuildLairs(string WorldID)
	{
		if (WorldID != "JoppaWorld")
		{
			return;
		}
		MetricsManager.rngCheckpoint("%LAIRS 1");
		Lairs = new int[240, 75];
		List<Location2D> list = new List<Location2D>();
		for (int i = 0; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				if (mutableMap.GetMutable(i, j) > 0 && ((RoadSystem[i, j] & ROAD_START) != 0 || (RiverSystem[i, j] & RIVER_START) != 0))
				{
					list.Add(Location2D.Get(i, j));
				}
			}
		}
		int num = 125;
		int num2 = 300;
		int num3 = num - list.Count + num2;
		MetricsManager.rngCheckpoint("%LAIRS 2");
		for (int k = 0; k < num3; k++)
		{
			list.Add(mutableMap.popMutableLocation());
		}
		MetricsManager.rngCheckpoint("%LAIRS 3");
		Coach.StartSection("Generate Lairs");
		list.ShuffleInPlace();
		MetricsManager.rngCheckpoint("%LAIRS 4");
		Event.PinCurrentPool();
		for (int l = 0; l < num; l++)
		{
			Event.ResetPool();
			try
			{
				if (AddLairAt(list[l], l))
				{
					mutableMap.SetMutable(list[l], 0);
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException($"AddLairAt::{list[l]}", x);
			}
			Event.ResetToPin();
		}
		Coach.EndSection();
		MetricsManager.rngCheckpoint("%LAIRS 5");
	}

	public void PlaceClams()
	{
		MapFile mapFile = MapFile.Resolve("Tzimtzlum");
		BallBag<string> ballBag = new BallBag<string>
		{
			{ "reef_surface", 15 },
			{ "reef_cave", 5 },
			{ "lake_surface", 7 },
			{ "lake_cave", 3 }
		};
		ClamSystem clamSystem = The.Game.RequireSystem(() => new ClamSystem());
		foreach (MapFileCellReference item in mapFile.Cells.AllCells())
		{
			if (item.cell.Objects.Any((MapFileObjectBlueprint o) => o.Name == "Giant Clam"))
			{
				string text = ballBag.PeekOne();
				string text2 = null;
				switch (text)
				{
				case "reef_surface":
					text2 = GetZoneIdOfTerrain("PalladiumReef");
					break;
				case "reef_cave":
					text2 = GetZoneIdOfTerrain("PalladiumReef", Stat.Random(11, 15).ToString());
					break;
				case "lake_surface":
					text2 = GetZoneIdOfTerrain("LakeHinnom");
					break;
				case "lake_cave":
					text2 = GetZoneIdOfTerrain("LakeHinnom", Stat.Random(11, 15).ToString());
					break;
				}
				The.Game.ZoneManager.AddZonePostBuilder(text2, "PlaceAClam", "clamNumber", clamSystem.clamJoppaZone.Count);
				clamSystem.clamJoppaZone.Add(text2);
			}
		}
		clamSystem.clamJoppaZone.Add("JoppaWorld.67.17.1.1.10");
	}

	public void AddLocationFinder(string ZoneID, string SecretID, int Value = 0, int Priority = 5000)
	{
		if (Value > 0)
		{
			The.ZoneManager.AddZoneBuilder(ZoneID, Priority, "AddLocationFinder", "SecretID", SecretID, "Value", Value);
		}
		else
		{
			The.ZoneManager.AddZoneBuilder(ZoneID, Priority, "AddLocationFinder", "SecretID", SecretID);
		}
	}

	public static GameObject GenerateLairOwner(string TerrainBlueprint, int ZoneTier, int GenericChance = 10)
	{
		string tag = GameObjectFactory.Factory.Blueprints[TerrainBlueprint].GetTag("LairOwnerTable", "GenericLairOwner");
		Dictionary<string, string> vars = new Dictionary<string, string> { 
		{
			"zonetier",
			ZoneTier.ToString()
		} };
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(PopulationManager.RollOneFrom(GenericChance.in100() ? "GenericLairOwner" : tag, vars).Blueprint);
		gameObject.SetIntProperty("LairOwner", 1);
		gameObject.SetStringProperty("Role", "Hero");
		gameObject.RemovePart<AIPilgrim>();
		if (!gameObject.HasPart<GivesRep>())
		{
			gameObject = HeroMaker.MakeHero(gameObject, Array.Empty<string>(), Array.Empty<string>(), ZoneTier, "Lair");
			if (gameObject == null)
			{
				return null;
			}
		}
		if (!gameObject.HasTag("Merchant"))
		{
			if (gameObject.HasStat("Strength"))
			{
				gameObject.GetStat("Strength").BoostStat(1);
			}
			if (gameObject.HasStat("Intelligence"))
			{
				gameObject.GetStat("Intelligence").BoostStat(1);
			}
			if (gameObject.HasStat("Toughness"))
			{
				gameObject.GetStat("Toughness").BoostStat(1);
			}
			if (gameObject.HasStat("Willpower"))
			{
				gameObject.GetStat("Willpower").BoostStat(1);
			}
			if (gameObject.HasStat("Ego"))
			{
				gameObject.GetStat("Ego").BoostStat(1);
			}
			if (gameObject.HasStat("Agility"))
			{
				gameObject.GetStat("Agility").BoostStat(1);
			}
			if (gameObject.HasStat("Hitpoints"))
			{
				gameObject.GetStat("Hitpoints").BaseValue *= 2;
			}
		}
		else
		{
			if (gameObject.HasStat("Willpower"))
			{
				gameObject.GetStat("Willpower").BoostStat(1);
			}
			if (gameObject.HasStat("Hitpoints"))
			{
				gameObject.GetStat("Hitpoints").BaseValue = Math.Max(50 * ZoneTier, gameObject.GetStat("Hitpoints").BaseValue);
			}
			if (gameObject.HasStat("Level"))
			{
				gameObject.GetStat("Level").BaseValue = Math.Max(ZoneTier * 5, gameObject.GetStat("Level").BaseValue);
			}
			if (gameObject.HasStat("XP") && gameObject.HasStat("Level"))
			{
				gameObject.GetStat("XP").BaseValue = Leveler.GetXPForLevel(gameObject.GetStat("Level").Value);
			}
			if (gameObject.HasStat("XPValue") && gameObject.HasStat("Level"))
			{
				float num = gameObject.GetStat("Level").Value;
				num /= 2f;
				gameObject.GetStat("XPValue").BaseValue = (int)(num * 200f);
			}
		}
		if (gameObject.HasPart<Inventory>() && gameObject.HasTag("LairInventory"))
		{
			if (gameObject.HasTag("LairAddMakersMark"))
			{
				HasMakersMark hasMakersMark = gameObject.RequirePart<HasMakersMark>();
				if (hasMakersMark.Mark == null)
				{
					hasMakersMark.Mark = MakersMark.Generate();
				}
				gameObject.EquipFromPopulationTable(gameObject.GetTag("LairInventory"), gameObject.GetTier(), GenericInventoryRestocker.GetCraftmarkApplication(gameObject));
			}
			else
			{
				gameObject.EquipFromPopulationTable(gameObject.GetTag("LairInventory"), ZoneTier);
			}
		}
		return gameObject;
	}

	public bool AddLairAt(Location2D pos, int nLair)
	{
		MetricsManager.rngCheckpoint("LAIR " + nLair + " start");
		string text = Zone.XYToID("JoppaWorld", pos.X, pos.Y, 10);
		int num = 0;
		if (25.in100())
		{
			num += Stat.Random(1, 25);
		}
		string text2 = Zone.XYToID("JoppaWorld", pos.X, pos.Y, 10 + num);
		string text3 = null;
		int zoneTier = The.ZoneManager.GetZoneTier(text);
		string objectTypeForZone = ZoneManager.GetObjectTypeForZone(pos.X / 3, pos.Y / 3, "JoppaWorld");
		GeneratedLocationInfo generatedLocationInfo = new GeneratedLocationInfo();
		GameObject gameObject = GenerateLairOwner(objectTypeForZone, zoneTier);
		if (gameObject == null)
		{
			MetricsManager.LogError("AddLairAt: Couldn't get a lair monster for " + text2 + " in a " + objectTypeForZone + ".");
			return false;
		}
		if (gameObject.HasTag("LairMinionsInherit"))
		{
			text3 = "DynamicInheritsTable:" + gameObject.GetTag("LairMinionsInherit") + ":Tier" + The.ZoneManager.GetZoneTier(text);
		}
		else if (gameObject.HasTag("LairMinions"))
		{
			text3 = gameObject.GetTag("LairMinions");
		}
		else
		{
			string text4 = gameObject.GetBlueprint().Inherits;
			string text5 = text4;
			while (text5 != null && ((!text5.StartsWith("Base") && !GameObjectFactory.Factory.Blueprints[text5].Tags.ContainsKey("BaseObject")) || GameObjectFactory.Factory.Blueprints[text5].Tags.ContainsKey("SkipAsLairBaseCreatureType")))
			{
				text5 = GameObjectFactory.Factory.Blueprints[text5].Inherits;
			}
			if (text5 != null)
			{
				text4 = text5;
			}
			if (text3 == null)
			{
				text3 = "DynamicInheritsTable:" + text4 + ":Tier" + The.ZoneManager.GetZoneTier(text);
			}
		}
		int value = gameObject.Stat("Level") * 75;
		int num2 = gameObject.Stat("Level") / 5 + 1;
		int num3 = num2 - 2;
		if (num2 < 2)
		{
			num2 = 2;
		}
		if (num2 > 8)
		{
			num2 = 8;
		}
		if (num3 < 1)
		{
			num3 = 1;
		}
		if (num3 > 8)
		{
			num3 = 8;
		}
		string text6 = Guid.NewGuid().ToString();
		gameObject.SetStringProperty("nosecret", text6);
		string text7 = gameObject.GetPropertyOrTag("LairAdjectives", "");
		if (text7.Length > 0)
		{
			text7 += ",";
		}
		text7 += GameObjectFactory.Factory.Blueprints[objectTypeForZone].GetTag("LairAdjectives", "lair");
		GameObjectBlueprint blueprint = gameObject.GetBlueprint();
		string referenceDisplayName = gameObject.GetReferenceDisplayName(int.MaxValue, null, "LairName");
		string text8 = Grammar.MakePossessive(referenceDisplayName);
		string newValue = referenceDisplayName.Strip();
		string newValue2 = text8.Strip();
		string tag = gameObject.GetTag("LairDepth", "2-4");
		string name = "the %l of %o".Replace("%l", blueprint.GetTag("LairName", "lair")).Replace("%o", referenceDisplayName).Replace("%p", text8);
		string propertyOrTag = gameObject.GetPropertyOrTag("LairAmbientBed");
		int num4 = tag.RollCached();
		MetricsManager.rngCheckpoint("LAIR " + nLair + " lairstats");
		for (int i = 0; i < num4; i++)
		{
			string newValue3 = i.ToString();
			string zoneID = Zone.XYToID("JoppaWorld", pos.X, pos.Y, 10 + i);
			if (!propertyOrTag.IsNullOrEmpty())
			{
				The.ZoneManager.SetZoneProperty(zoneID, "ambient_bed_2", propertyOrTag);
			}
			string tag2 = blueprint.GetTag("LairNameContext", "the lair of %o");
			string text9 = null;
			bool proper = false;
			if (i != num4 - 1)
			{
				text9 = ((i != 0) ? blueprint.GetTag("LairLevelNameSubsurface") : blueprint.GetTag("LairLevelNameSurface"));
			}
			else
			{
				text9 = blueprint.GetTag("LairLevelNameFinal");
				if (!text9.IsNullOrEmpty())
				{
					proper = true;
				}
			}
			if (text9 != null)
			{
				text9 = text9.Replace("%o", newValue).Replace("%n", newValue3).Replace("%p", newValue2);
			}
			tag2 = tag2.Replace("%o", newValue).Replace("%n", newValue3).Replace("%p", newValue2);
			The.ZoneManager.SetZoneName(zoneID, text9, tag2, null, null, null, proper);
			if (i == 0)
			{
				The.ZoneManager.AddZonePostBuilder(zoneID, "BasicLair", "Table", text3, "Adjectives", text7, "Stairs", "D");
			}
			else if (i == num4 - 1)
			{
				The.ZoneManager.ClearZoneBuilders(zoneID);
				The.ZoneManager.SetZoneProperty(zoneID, "SkipTerrainBuilders", true);
				The.ZoneManager.AddZonePostBuilder(zoneID, "BasicLair", "Table", text3, "Adjectives", text7, "Stairs", "U");
				The.ZoneManager.AddZonePostBuilder(zoneID, "AddObjectBuilder", "Object", The.ZoneManager.CacheObject(gameObject));
				The.ZoneManager.AddZonePostBuilder(zoneID, "AddBlueprintBuilder", "Object", "Chest" + num2);
			}
			else
			{
				The.ZoneManager.ClearZoneBuilders(zoneID);
				The.ZoneManager.SetZoneProperty(zoneID, "SkipTerrainBuilders", true);
				The.ZoneManager.AddZonePostBuilder(zoneID, "BasicLair", "Table", text3, "Adjectives", text7, "Stairs", "UD");
			}
			AddLocationFinder(zoneID, text6, value);
		}
		MetricsManager.rngCheckpoint("LAIR " + nLair + " levelgen");
		string secretZone = Zone.XYToID("JoppaWorld", pos.X, pos.Y, 10);
		List<string> list = new List<string>();
		string tag3 = gameObject.GetTag("LairCategory", "Lairs");
		string tag4 = gameObject.GetTag("LairName", "lair");
		if (tag3 == "Lairs")
		{
			list.Add("lair");
		}
		if (gameObject.HasTag("LairAdjectives"))
		{
			list.AddRange(gameObject.GetTag("LairAdjectives").Split(','));
		}
		if (gameObject.HasTag("SecretAdjectives"))
		{
			list.AddRange(gameObject.GetTag("SecretAdjectives").Split(','));
		}
		list.Add(gameObject.GetPropertyOrTag("Species") ?? gameObject.GetBlueprint().GetBaseTypeName().ToLower());
		string propertyOrTag2 = gameObject.GetPropertyOrTag("Class");
		if (!propertyOrTag2.IsNullOrEmpty())
		{
			list.Add(propertyOrTag2);
		}
		AddSecret(secretZone, "the " + tag4 + " of " + gameObject.GetReferenceDisplayName(int.MaxValue, null, "LairName"), list.ToArray(), tag3, text6);
		MetricsManager.rngCheckpoint("LAIR " + nLair + " secretgen");
		Coach.EndSection();
		Coach.StartSection("Add pulldowns");
		Zone zone = The.ZoneManager.GetZone("JoppaWorld");
		TerrainTravel terrainTravel = null;
		Render render = null;
		GameObject firstObjectWithPart = zone.GetCell(pos.X / 3, pos.Y / 3).GetFirstObjectWithPart("TerrainTravel");
		if (firstObjectWithPart != null)
		{
			terrainTravel = firstObjectWithPart.GetPart<TerrainTravel>();
			render = firstObjectWithPart.GetPart<Render>();
		}
		if (Options.ShowOverlandEncounters && render != null)
		{
			render.RenderString = "&W*";
			render.ParentObject.SetStringProperty("OverlayColor", "&M");
		}
		terrainTravel?.AddEncounter(new EncounterEntry(blueprint.GetTag("LairPulldownMessage", "You discover a lair. Would you like to investigate?"), text, "", text6, Optional: true));
		generatedLocationInfo.targetZone = Zone.XYToID("JoppaWorld", pos.X, pos.Y, 10 + num4 - 1);
		generatedLocationInfo.zoneLocation = pos;
		generatedLocationInfo.name = name;
		generatedLocationInfo.ownerID = gameObject.ID;
		generatedLocationInfo.secretID = text6;
		worldInfo.lairs.Add(generatedLocationInfo);
		MetricsManager.rngCheckpoint("LAIR " + nLair + " travelgen");
		Coach.EndSection();
		return true;
	}

	public Location2D GetRoadHead()
	{
		int num = Stat.Random(0, 239);
		int num2 = Stat.Random(0, 74);
		while (RoadSystem[num, num2] != 0)
		{
			num = Stat.Random(0, 239);
			num2 = Stat.Random(0, 74);
		}
		return Location2D.Get(num, num2);
	}

	public void BuildCanyonSystems(string WorldID)
	{
		if (!(WorldID == "JoppaWorld"))
		{
			return;
		}
		Zone zone = The.ZoneManager.GetZone("JoppaWorld");
		Maze maze = The.Game.WorldMazes["QudCanyonMaze"];
		for (int i = 0; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				string zoneID = Zone.XYToID("JoppaWorld", i, j, 10);
				int x = i / 3;
				int y = j / 3;
				GameObject firstObjectWithPart = zone.GetCell(x, y).GetFirstObjectWithPart("TerrainTravel");
				if (firstObjectWithPart != null && (firstObjectWithPart.Blueprint.Contains("TerrainJoppaRedrockChannel") || firstObjectWithPart.Blueprint.Contains("Canyon") || firstObjectWithPart.Blueprint.Contains("Hills") || firstObjectWithPart.Blueprint.Contains("Asphalt") || firstObjectWithPart.Blueprint.Contains("TerrainRustedArchway") || firstObjectWithPart.Blueprint.Contains("TerrainBethesdaSusa")))
				{
					MazeCell mazeCell = maze.Cell[i, j];
					The.ZoneManager.AddZoneMidBuilder(zoneID, "CanyonStartMouth");
					if (mazeCell.N)
					{
						The.ZoneManager.AddZoneMidBuilder(zoneID, "CanyonNorthMouth");
					}
					if (mazeCell.S)
					{
						The.ZoneManager.AddZoneMidBuilder(zoneID, "CanyonSouthMouth");
					}
					if (mazeCell.E)
					{
						The.ZoneManager.AddZoneMidBuilder(zoneID, "CanyonEastMouth");
					}
					if (mazeCell.W)
					{
						The.ZoneManager.AddZoneMidBuilder(zoneID, "CanyonWestMouth");
					}
					The.ZoneManager.AddZoneMidBuilder(zoneID, "CanyonBuilder");
					The.ZoneManager.AddZonePostBuilder(zoneID, "CanyonReacher");
				}
			}
		}
	}

	public void BuildRoadSystems(string WorldID)
	{
		RoadSystem = new uint[240, 75];
		for (int i = 0; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				if (mutableMap.GetMutable(i, j) == 1)
				{
					RoadSystem[i, j] = 0u;
				}
				else
				{
					RoadSystem[i, j] = ROAD_NONE;
				}
			}
		}
		if (WorldID == "JoppaWorld")
		{
			for (int k = 0; k < 160; k++)
			{
				Location2D roadHead = GetRoadHead();
				BuildRoad(WorldID, roadHead.X, roadHead.Y);
			}
		}
	}

	public void BuildRoad(string WorldID, int StartX, int StartY)
	{
		int num = Stat.Random(0, 3);
		The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadStartMouth");
		RoadSystem[StartX, StartY] |= ROAD_START;
		if (num == 0)
		{
			RoadSystem[StartX, StartY] |= ROAD_NORTH;
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadNorthMouth");
		}
		if (num == 1)
		{
			RoadSystem[StartX, StartY] |= ROAD_EAST;
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadEastMouth");
		}
		if (num == 2)
		{
			RoadSystem[StartX, StartY] |= ROAD_SOUTH;
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadSouthMouth");
		}
		if (num == 3)
		{
			RoadSystem[StartX, StartY] |= ROAD_WEST;
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadWestMouth");
		}
		The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RoadBuilder");
		ContinueRoad(StartX, StartY, num, 0);
	}

	public void ContinueRoad(int StartX, int StartY, int Direction, int Depth)
	{
		int num = StartX;
		int num2 = StartY;
		if (Direction == 0)
		{
			num2--;
		}
		if (Direction == 2)
		{
			num2++;
		}
		if (Direction == 1)
		{
			num++;
		}
		if (Direction == 3)
		{
			num--;
		}
		if (num < 0 || num2 < 0 || num >= 240 || num2 >= 75)
		{
			return;
		}
		if (Stat.Random(0, 100) < 2 + Depth * 5)
		{
			RoadSystem[num, num2] |= ROAD_START;
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadStartMouth");
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
			return;
		}
		int num3 = Direction;
		if (50.in100())
		{
			int num4 = Stat.Random(0, 1);
			if (num4 == 0)
			{
				num3--;
			}
			if (num4 == 1)
			{
				num3++;
			}
			if (num3 < 0)
			{
				num3 = 3;
			}
			if (num3 > 3)
			{
				num3 = 0;
			}
		}
		if (Direction == 0)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadSouthMouth");
			RoadSystem[num, num2] |= ROAD_SOUTH;
		}
		if (Direction == 1)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadWestMouth");
			RoadSystem[num, num2] |= ROAD_WEST;
		}
		if (Direction == 2)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadNorthMouth");
			RoadSystem[num, num2] |= ROAD_NORTH;
		}
		if (Direction == 3)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadEastMouth");
			RoadSystem[num, num2] |= ROAD_EAST;
		}
		int num5 = num;
		int num6 = num2;
		if (num3 == 0)
		{
			num6--;
		}
		if (num3 == 2)
		{
			num6++;
		}
		if (num3 == 1)
		{
			num5++;
		}
		if (num3 == 3)
		{
			num5--;
		}
		if (num6 < 0 || num5 < 0 || num6 == 75 || num5 == 240 || RoadSystem[num5, num6] != 0)
		{
			RoadSystem[num, num2] |= ROAD_START;
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadStartMouth");
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
			return;
		}
		if (num3 == 0)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadNorthMouth");
			RoadSystem[num, num2] |= ROAD_NORTH;
		}
		if (num3 == 1)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadEastMouth");
			RoadSystem[num, num2] |= ROAD_EAST;
		}
		if (num3 == 2)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadSouthMouth");
			RoadSystem[num, num2] |= ROAD_SOUTH;
		}
		if (num3 == 3)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadWestMouth");
			RoadSystem[num, num2] |= ROAD_WEST;
		}
		if (Stat.Random(0, 100) < 5)
		{
			int num7 = Direction;
			if (num7 == num3)
			{
				int num8 = Stat.Random(0, 1);
				if (num8 == 0)
				{
					num7--;
				}
				if (num8 == 1)
				{
					num7++;
				}
				if (num7 < 0)
				{
					num7 = 3;
				}
				if (num7 > 3)
				{
					num7 = 0;
				}
			}
			if (num7 == 0)
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RoadNorthMouth");
				RoadSystem[StartX, StartY] |= ROAD_NORTH;
			}
			if (num7 == 1)
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RoadEastMouth");
				RoadSystem[StartX, StartY] |= ROAD_EAST;
			}
			if (num7 == 2)
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RoadSouthMouth");
				RoadSystem[StartX, StartY] |= ROAD_SOUTH;
			}
			if (num7 == 3)
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RoadWestMouth");
				RoadSystem[StartX, StartY] |= ROAD_WEST;
			}
			ContinueRoad(num, num2, num7, Depth + 1);
		}
		The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RoadBuilder");
		if (Options.ShowOverlandEncounters)
		{
			The.ZoneManager.GetZone("JoppaWorld").GetCell(num / 3, num2 / 3).GetObjectInCell(0)
				.Render.RenderString = ".";
		}
		ContinueRoad(num, num2, num3, Depth);
	}

	public Location2D GetRiverHead()
	{
		int num = Stat.Random(0, 239);
		int num2 = Stat.Random(0, 74);
		string[] t = new string[3] { "TerrainWater", "TerrainSaltmarsh", "TerrainSaltmarsh2" };
		while (RiverSystem[num, num2] != 0 || !WorldCellHasTerrain("JoppaWorld", num / 3, num2 / 3, t))
		{
			num = Stat.Random(0, 239);
			num2 = Stat.Random(0, 74);
		}
		return Location2D.Get(num, num2);
	}

	public void BuildRiverSystems(string WorldID)
	{
		for (int i = 0; i < 240; i++)
		{
			for (int j = 0; j < 75; j++)
			{
				if (mutableMap.GetMutable(i, j) == 1)
				{
					RiverSystem[i, j] = 0u;
				}
				else
				{
					RiverSystem[i, j] = RIVER_NONE;
				}
			}
		}
		if (WorldID == "JoppaWorld")
		{
			for (int k = 0; k < 160; k++)
			{
				Location2D riverHead = GetRiverHead();
				BuildRiver(WorldID, riverHead.X, riverHead.Y);
			}
		}
	}

	public void BuildRiver(string WorldID, int StartX, int StartY)
	{
		int num = Stat.Random(0, 3);
		The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverStartMouth");
		RiverSystem[StartX, StartY] |= RIVER_START;
		if (num == 0)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverNorthMouth");
			RiverSystem[StartX, StartY] |= RIVER_NORTH;
		}
		if (num == 1)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverEastMouth");
			RiverSystem[StartX, StartY] |= RIVER_EAST;
		}
		if (num == 2)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverSouthMouth");
			RiverSystem[StartX, StartY] |= RIVER_SOUTH;
		}
		if (num == 3)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverWestMouth");
			RiverSystem[StartX, StartY] |= RIVER_WEST;
		}
		The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverBuilder");
		ContinueRiver(StartX, StartY, num, 0);
	}

	public List<Location2D> BuildMamonRiver(string WorldID, int StartX, int StartY, int Direction, int Bias, int MinimumLength, int ExcludeDirection)
	{
		The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverStartMouth");
		RiverSystem[StartX, StartY] |= RIVER_START;
		if (Direction == 0)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverNorthMouth");
			RiverSystem[StartX, StartY] |= RIVER_NORTH;
		}
		if (Direction == 1)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverEastMouth");
			RiverSystem[StartX, StartY] |= RIVER_EAST;
		}
		if (Direction == 2)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverSouthMouth");
			RiverSystem[StartX, StartY] |= RIVER_SOUTH;
		}
		if (Direction == 3)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverWestMouth");
			RiverSystem[StartX, StartY] |= RIVER_WEST;
		}
		The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY(WorldID, StartX, StartY), "RiverBuilder");
		List<Location2D> list = new List<Location2D>();
		ContinueMamonRiver(StartX, StartY, Direction, 0, Bias, MinimumLength, ExcludeDirection, list);
		return list;
	}

	public void ContinueRiver(int StartX, int StartY, int Direction, int Depth)
	{
		int num = StartX;
		int num2 = StartY;
		if (Direction == 0)
		{
			num2--;
		}
		if (Direction == 2)
		{
			num2++;
		}
		if (Direction == 1)
		{
			num++;
		}
		if (Direction == 3)
		{
			num--;
		}
		if (num < 0 || num2 < 0 || num >= 240 || num2 >= 75)
		{
			return;
		}
		if (Stat.Random(0, 100) < Depth - 8)
		{
			RiverSystem[num, num2] |= RIVER_START;
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverStartMouth");
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
			return;
		}
		int num3 = Direction;
		if (Stat.Random(1, 100) < 20)
		{
			int num4 = Stat.Random(0, 1);
			if (num4 == 0)
			{
				num3--;
			}
			if (num4 == 1)
			{
				num3++;
			}
			if (num3 < 0)
			{
				num3 = 3;
			}
			if (num3 > 3)
			{
				num3 = 0;
			}
		}
		if (Direction == 0)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverSouthMouth");
			RiverSystem[num, num2] |= RIVER_SOUTH;
		}
		if (Direction == 1)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverWestMouth");
			RiverSystem[num, num2] |= RIVER_WEST;
		}
		if (Direction == 2)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverNorthMouth");
			RiverSystem[num, num2] |= RIVER_NORTH;
		}
		if (Direction == 3)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverEastMouth");
			RiverSystem[num, num2] |= RIVER_EAST;
		}
		int num5 = num;
		int num6 = num2;
		if (num3 == 0)
		{
			num6--;
		}
		if (num3 == 2)
		{
			num6++;
		}
		if (num3 == 1)
		{
			num5++;
		}
		if (num3 == 3)
		{
			num5--;
		}
		if (num6 < 0 || num5 < 0 || num6 >= 75 || num5 >= 240 || RiverSystem[num5, num6] != 0)
		{
			RiverSystem[num, num2] |= RIVER_START;
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverStartMouth");
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
			return;
		}
		if (num3 == 0)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverNorthMouth");
			RiverSystem[num, num2] |= RIVER_NORTH;
		}
		if (num3 == 1)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverEastMouth");
			RiverSystem[num, num2] |= RIVER_EAST;
		}
		if (num3 == 2)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverSouthMouth");
			RiverSystem[num, num2] |= RIVER_SOUTH;
		}
		if (num3 == 3)
		{
			The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverWestMouth");
			RiverSystem[num, num2] |= RIVER_WEST;
		}
		bool flag = false;
		if (5.in100())
		{
			flag = true;
			int num7 = Direction;
			if (num7 == num3)
			{
				int num8 = Stat.Random(0, 1);
				if (num8 == 0)
				{
					num7--;
				}
				if (num8 == 1)
				{
					num7++;
				}
				if (num7 < 0)
				{
					num7 = 3;
				}
				if (num7 > 3)
				{
					num7 = 0;
				}
			}
			if (num7 == 0)
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RiverNorthMouth");
			}
			if (num7 == 1)
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RiverEastMouth");
			}
			if (num7 == 2)
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RiverSouthMouth");
			}
			if (num7 == 3)
			{
				The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", StartX, StartY), "RiverWestMouth");
			}
			ContinueRiver(num, num2, num7, Depth + 1);
		}
		The.ZoneManager.AddZoneMidBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
		if (Options.ShowOverlandEncounters)
		{
			Render render = The.ZoneManager.GetZone("JoppaWorld").GetCell(num / 3, num2 / 3).GetObjectInCell(0)
				.Render;
			render.RenderString = (flag ? "T" : "~");
			render.Tile = null;
		}
		ContinueRiver(num, num2, num3, Depth + 1);
	}

	public void ContinueMamonRiver(int StartX, int StartY, int Direction, int Depth, int Bias, int MinimumLength, int ExcludeDirection, List<Location2D> Points)
	{
		int num = StartX;
		int num2 = StartY;
		if (Direction == 0)
		{
			num2--;
		}
		if (Direction == 2)
		{
			num2++;
		}
		if (Direction == 1)
		{
			num++;
		}
		if (Direction == 3)
		{
			num--;
		}
		if (num < 0 || num2 < 0 || num >= 240 || num2 >= 75)
		{
			return;
		}
		mutableMap.RemoveMutableLocation(Location2D.Get(num, num2));
		Points.Add(Location2D.Get(num, num2));
		if (Depth > MinimumLength && Stat.Random(0, 100) < 2 + Depth * 5)
		{
			RiverSystem[num, num2] |= RIVER_START;
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverStartMouth");
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
			return;
		}
		int num3 = Direction;
		if (Bias <= 0 && 50.in100())
		{
			int num4 = Stat.Random(0, 1);
			if (num4 == 0)
			{
				num3--;
			}
			if (num4 == 1)
			{
				num3++;
			}
			if (num3 < 0)
			{
				num3 = 3;
			}
			if (num3 > 3)
			{
				num3 = 0;
			}
			if (num3 == ExcludeDirection)
			{
				num3 = Direction;
			}
		}
		if (Direction == 0)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverSouthMouth");
			RiverSystem[num, num2] |= RIVER_SOUTH;
		}
		if (Direction == 1)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverWestMouth");
			RiverSystem[num, num2] |= RIVER_WEST;
		}
		if (Direction == 2)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverNorthMouth");
			RiverSystem[num, num2] |= RIVER_NORTH;
		}
		if (Direction == 3)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverEastMouth");
			RiverSystem[num, num2] |= RIVER_EAST;
		}
		int num5 = num;
		int num6 = num2;
		if (num3 == 0)
		{
			num6--;
		}
		if (num3 == 2)
		{
			num6++;
		}
		if (num3 == 1)
		{
			num5++;
		}
		if (num3 == 3)
		{
			num5--;
		}
		if (num6 < 0 || num5 < 0 || num6 == 75 || num5 == 240 || RiverSystem[num5, num6] != 0)
		{
			RiverSystem[num, num2] |= RIVER_START;
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverStartMouth");
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
			return;
		}
		if (num3 == 0)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverNorthMouth");
			RiverSystem[num, num2] |= RIVER_NORTH;
		}
		if (num3 == 1)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverEastMouth");
			RiverSystem[num, num2] |= RIVER_EAST;
		}
		if (num3 == 2)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverSouthMouth");
			RiverSystem[num, num2] |= RIVER_SOUTH;
		}
		if (num3 == 3)
		{
			The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverWestMouth");
			RiverSystem[num, num2] |= RIVER_WEST;
		}
		bool flag = false;
		The.ZoneManager.AddZonePostBuilder(ZoneIDFromXY("JoppaWorld", num, num2), "RiverBuilder");
		if (Options.ShowOverlandEncounters)
		{
			Render part = The.ZoneManager.GetZone("JoppaWorld").GetCell(num / 3, num2 / 3).GetObjectInCell(0)
				.GetPart<Render>();
			part.RenderString = "=";
			if (flag)
			{
				part.RenderString = "t";
			}
		}
		ContinueMamonRiver(num, num2, num3, Depth, Bias - 1, MinimumLength, ExcludeDirection, Points);
	}

	public override bool BuildWorld(string worldName)
	{
		extensions = ModManager.GetInstancesWithAttribute<IJoppaWorldBuilderExtension>(typeof(JoppaWorldBuilderExtension));
		World = worldName;
		worldInfo = new WorldInfo();
		The.Game.ObjectGameState.Add("JoppaWorldInfo", worldInfo);
		WorldZone = The.ZoneManager.GetZone(World);
		extensions.ForEach((IJoppaWorldBuilderExtension e) =>
		{
			e.OnBeforeBuild(this);
		});
		MetricsManager.rngCheckpoint("JWB start");
		JournalAPI.SuspendSorting();
		MetricsManager.rngCheckpoint("mazes");
		WorldCreationProgress.NextStep("Generating deep history...", 1);
		BuildMazes();
		MetricsManager.rngCheckpoint("mutable");
		WorldCreationProgress.NextStep("Generating historical sites...", WorldFactory.Factory.countWorlds() * 80);
		BuildMutableEncounters();
		if (The.Game.AlternateStart && TutorialManager.currentStep == null)
		{
			Factions.Get("Joppa").Visible = false;
		}
		game.SetStringGameState("BlackOrbLocation", "OrbWorld.40.15.1.1.12@19,7");
		JournalAPI.ResumeSorting();
		MetricsManager.rngCheckpoint("embark");
		if (game.GetStringGameState("RuinedJoppa", "No") == "Yes")
		{
			Factions.Get("Joppa").Visible = false;
			Cell currentCell = game.ZoneManager.GetZone("JoppaWorld").GetFirstObject((GameObject o) => o.Blueprint == "TerrainJoppa").GetCurrentCell();
			currentCell.Clear();
			currentCell.AddObject("TerrainJoppaRuins");
			game.ZoneManager.SetZoneName("JoppaWorld.11.22.1.1.10", "abandoned village");
			for (int num = 0; num <= 2; num++)
			{
				for (int num2 = 0; num2 <= 2; num2++)
				{
					if (num != 1 || num2 != 1)
					{
						game.ZoneManager.SetZoneName("JoppaWorld.11.22." + num + "." + num2 + ".10", "salt marsh", "");
					}
				}
			}
		}
		extensions.ForEach((IJoppaWorldBuilderExtension e) =>
		{
			e.OnAfterBuild(this);
		});
		return true;
	}
}
