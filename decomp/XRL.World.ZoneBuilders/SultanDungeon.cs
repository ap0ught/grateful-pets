using System;
using System.Collections.Generic;
using Genkit;
using HistoryKit;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class SultanDungeon : ZoneBuilderSandbox
{
	public string locationName;

	public string regionName;

	public string stairs = "";

	public string monstertable = "";

	public const int CHANCE_FOR_SPECIAL_HISTORIC_WALL = 25;

	public long lastmem;

	private static Dictionary<string, int> staticPerDungeon = new Dictionary<string, int> { { "*LightSource", 20 } };

	private int targetTier;

	private Zone targetZone;

	public Dictionary<string, int> memberTier;

	public Dictionary<string, List<string>> roleMembers;

	public List<string> allMembers;

	public Dictionary<string, bool> memberHasBody;

	public int clamp(int n, int low, int high)
	{
		if (n < low)
		{
			return low;
		}
		if (n > high)
		{
			return high;
		}
		return n;
	}

	public void partition(Rect2D rect, ref int nSegments, List<ISultanDungeonSegment> segments)
	{
		if (nSegments <= 0)
		{
			segments.Add(new SultanRectDungeonSegment(rect));
		}
		else if (Stat.Random(0, 1) == 0)
		{
			int num = rect.y2 - rect.y1;
			int num2 = (rect.y2 - rect.y1) / 2;
			int num3 = (rect.y2 - rect.y1) / 8;
			int num4 = clamp((int)Stat.GaussianRandom(0f, num3) + num2, rect.y1 + 4, rect.y2 - 4);
			if (num <= 8)
			{
				segments.Add(new SultanRectDungeonSegment(rect));
				return;
			}
			nSegments--;
			Rect2D rect2 = new Rect2D(rect.x1, rect.y1, rect.x2, num4);
			partition(rect2, ref nSegments, segments);
			nSegments--;
			Rect2D rect3 = new Rect2D(rect.x1, num4 + 1, rect.x2, rect.y2);
			partition(rect3, ref nSegments, segments);
		}
		else
		{
			int num5 = rect.x2 - rect.x1;
			int num6 = (rect.x2 - rect.x1) / 2;
			int num7 = (rect.x2 - rect.x1) / 8;
			int num8 = clamp((int)Stat.GaussianRandom(0f, num7) + num6, rect.x1 + 4, rect.x2 - 4);
			if (num5 <= 16)
			{
				segments.Add(new SultanRectDungeonSegment(rect));
				return;
			}
			nSegments--;
			Rect2D rect4 = new Rect2D(rect.x1, rect.y1, num8, rect.y2);
			partition(rect4, ref nSegments, segments);
			nSegments--;
			Rect2D rect5 = new Rect2D(num8 + 1, rect.y1, rect.x2, rect.y2);
			partition(rect5, ref nSegments, segments);
		}
	}

	public string ResolveOneObjectFromArgs(List<string> arg, string def = null, int tier = 1)
	{
		if (arg == null || arg.Count == 0)
		{
			return def;
		}
		string randomElement = arg.GetRandomElement();
		if (randomElement.Length == 0)
		{
			return def;
		}
		if (randomElement[0] == '$')
		{
			return PopulationManager.RollOneFrom(randomElement.Substring(1), new Dictionary<string, string> { 
			{
				"zonetier",
				tier.ToString()
			} }, "SmallBoulder").Blueprint;
		}
		return randomElement;
	}

	public List<PopulationResult> ResolveOnePopulationFromArgs(List<string> arg, string def = null, int Tier = 1)
	{
		if (arg == null || arg.Count == 0)
		{
			return PopulationManager.Generate(def);
		}
		return PopulationManager.Generate(arg.GetRandomElement(), "zonetier", Tier.ToString());
	}

	public string ResolveOneTableFromArgs(List<string> arg, string def = null)
	{
		if (arg == null || arg.Count == 0)
		{
			return def;
		}
		return arg.GetRandomElement();
	}

	public bool BuildRandomZoneWithArgs(Zone Z, int relicChance, bool bBuildSurface, string[] args, string stairs, string monstertable = null, string tablePrefix = "SultanDungeons_")
	{
		SultanDungeonArgs sultanDungeonArgs = new SultanDungeonArgs();
		this.monstertable = monstertable;
		this.stairs = stairs;
		HistoricEntitySnapshot historicEntitySnapshot = new HistoricEntitySnapshot(null);
		historicEntitySnapshot.properties.Clear();
		for (int i = 0; i < args.Length; i++)
		{
			historicEntitySnapshot.properties.Add("additionalArg" + Guid.NewGuid().ToString(), args[i]);
		}
		sultanDungeonArgs.UpdateFromEntity(historicEntitySnapshot, tablePrefix);
		bool flag = BuildZoneFromArgs(Z, sultanDungeonArgs, AddSurface: false);
		if (flag && Stat.Random(1, 100) <= relicChance)
		{
			new PlaceRelicBuilder().BuildZoneWithRelic(Z, RelicGenerator.GenerateRelic(historicEntitySnapshot, Z.NewTier, null, RandomName: true));
		}
		return flag;
	}

	public bool BuildRandomZone(Zone Z, int relicChance = 10, bool bBuildSurface = false, string tablePrefix = "SultanDungeons_")
	{
		History sultanHistory = The.Game.sultanHistory;
		SultanDungeonArgs sultanDungeonArgs = new SultanDungeonArgs();
		HistoricEntitySnapshot currentSnapshot = sultanHistory.entities.GetRandomElement().GetCurrentSnapshot();
		sultanDungeonArgs.UpdateFromEntity(currentSnapshot, tablePrefix);
		GameObject terrainObject = Z.GetTerrainObject();
		if (terrainObject.HasTag("RuinWalls") && Z.Z <= 15)
		{
			if (!The.ZoneManager.TryGetWorldCellProperty<string>(Z.ZoneID, "RuinWallType", out var Value))
			{
				Value = PopulationManager.RollOneFrom(terrainObject.GetTag("RuinWalls")).Blueprint;
				The.ZoneManager.SetWorldCellProperty(Z.ZoneID, "RuinWallType", Value);
			}
			sultanDungeonArgs.wallTypes.Clear();
			sultanDungeonArgs.wallTypes.Add(Value);
		}
		bool flag = BuildZoneFromArgs(Z, sultanDungeonArgs, AddSurface: false);
		if (flag && Stat.Random(1, 100) <= relicChance)
		{
			new PlaceRelicBuilder().BuildZoneWithRelic(Z, RelicGenerator.GenerateRelic(currentSnapshot, Z.NewTier, null, RandomName: true));
		}
		return flag;
	}

	public bool BuildZone(Zone Z)
	{
		return BuildZoneFromArgs(Z, (SultanDungeonArgs)The.Game.GetObjectGameState("sultanDungeonArgs_" + regionName), AddSurface: true);
	}

	public void LogMem(string step)
	{
	}

	public bool PointInSegments(List<ISultanDungeonSegment> segments, int x, int y)
	{
		for (int i = 0; i < segments.Count; i++)
		{
			if (segments[i].contains(Location2D.Get(x, y)))
			{
				return true;
			}
		}
		return false;
	}

	public GameObject sultanDungeonSemanticCreate(string blueprint)
	{
		if (!string.IsNullOrEmpty(blueprint) && blueprint[0] == '*')
		{
			int value = 0;
			string populationName = "DynamicSemanticTable:" + blueprint.Substring(1) + "::" + targetTier;
			if (staticPerDungeon.TryGetValue(blueprint, out value) && Stat.Random(1, 100) > value)
			{
				if (!HasZoneColumnValue(targetZone, "sultandungeon_semantic_result_" + blueprint))
				{
					string blueprint2 = PopulationManager.RollOneFrom(populationName).Blueprint;
					SetZoneColumnValue(targetZone, "sultandungeon_semantic_result_" + blueprint, blueprint2);
					return GameObject.Create(blueprint2);
				}
				return GameObject.Create(GetZoneColumnValue(targetZone, "sultandungeon_semantic_result_" + blueprint));
			}
			return GameObject.Create(PopulationManager.RollOneFrom(populationName).Blueprint);
		}
		return GameObject.Create(blueprint);
	}

	public bool BuildZoneFromArgs(Zone Z, SultanDungeonArgs regionArgs, bool AddSurface)
	{
		targetZone = Z;
		if (targetZone != null)
		{
			targetTier = targetZone.NewTier;
		}
		if (Z.Z > 10)
		{
			Z.AmbientBed = "Sounds/Ambiences/amb_bed_dungeon";
		}
		LogMem("step1");
		string text = "*Default";
		string tablePrefix = regionArgs.tablePrefix;
		string text2 = tablePrefix + "Floor_" + text;
		string blueprint = (PopulationManager.RollOneFrom(text2, null, "Rocky") ?? throw new Exception("Got no floor blueprint result from " + text2)).Blueprint;
		History sultanHistory = The.Game.sultanHistory;
		HistoricEntitySnapshot historicEntitySnapshot = null;
		HistoricEntity randomElement = sultanHistory.GetEntitiesWherePropertyEquals("name", locationName).entities.GetRandomElement();
		if (randomElement != null)
		{
			historicEntitySnapshot = randomElement.GetCurrentSnapshot();
		}
		SultanDungeonArgs sultanDungeonArgs = regionArgs.clone();
		sultanDungeonArgs.Mutate();
		if (historicEntitySnapshot != null)
		{
			sultanDungeonArgs.UpdateFromEntity(historicEntitySnapshot);
		}
		string text3 = tablePrefix + "Segmentation_*Default";
		string blueprint2 = (PopulationManager.RollOneFrom(text3, null, "Full") ?? throw new Exception("Got no segmentation blueprint result from " + text3)).Blueprint;
		string text4 = ResolveOneTableFromArgs(sultanDungeonArgs.greenObjects);
		string text5 = ResolveOneTableFromArgs(sultanDungeonArgs.yellowObjects);
		string text6 = ResolveOneTableFromArgs(sultanDungeonArgs.blueObjects);
		string blueprint3 = ((text6 != null) ? (PopulationManager.RollOneFrom(text6, null, "SaltyWaterPuddle") ?? throw new Exception("Got no blue object result from " + text6)).Blueprint : "SaltyWaterPuddle");
		List<ISultanDungeonSegment> list = new List<ISultanDungeonSegment>();
		int num = 7;
		int num2 = 72;
		int num3 = 7;
		int num4 = 17;
		string[] array = blueprint2.Split(';');
		foreach (string text7 in array)
		{
			if (text7 == "Zone")
			{
				list.Add(new SultanRectDungeonSegment(new Rect2D(0, 0, 80, 25)));
			}
			else if (text7 == "Full")
			{
				list.Add(new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22)));
			}
			else if (text7.StartsWith("BSP:"))
			{
				int nSegments = Convert.ToInt32(text7.Split(':')[1]);
				partition(new Rect2D(2, 2, 78, 24), ref nSegments, list);
			}
			else if (text7.StartsWith("Ring:"))
			{
				int num5 = Convert.ToInt32(text7.Split(':')[1]);
				list.Add(new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22)));
				if (num5 == 2)
				{
					list.Add(new SultanRectDungeonSegment(new Rect2D(20, 8, 60, 16)));
				}
				if (num5 == 3)
				{
					list.Add(new SultanRectDungeonSegment(new Rect2D(15, 8, 65, 16)));
					list.Add(new SultanRectDungeonSegment(new Rect2D(25, 10, 55, 14)));
				}
			}
			else if (text7.StartsWith("Blocks"))
			{
				string[] array2 = text7.Split(':')[1].Split(',');
				int num6 = array2[0].RollCached();
				for (int j = 0; j < num6; j++)
				{
					int num7 = array2[1].RollCached();
					int num8 = array2[2].RollCached();
					int num9 = Stat.Random(2, 78 - num7);
					int num10 = Stat.Random(2, 23 - num8);
					int num11 = num9 + num7;
					int num12 = num10 + num8;
					if (num < num9)
					{
						num = num9;
					}
					if (num2 > num11)
					{
						num2 = num11;
					}
					if (num3 < num10)
					{
						num3 = num10;
					}
					if (num4 > num12)
					{
						num4 = num12;
					}
					list.Add(new SultanRectDungeonSegment(new Rect2D(num9, num10, num9 + num7, num10 + num8)));
				}
			}
			else if (text7.StartsWith("Circle"))
			{
				string[] array3 = text7.Split(':')[1].Split(',');
				list.Add(new SultanCircleDungeonSegment(Location2D.Get(array3[0].RollCached(), array3[1].RollCached()), array3[2].RollCached()));
			}
			else if (text7.StartsWith("Tower"))
			{
				string[] array4 = text7.Split(':')[1].Split(',');
				list.Add(new SultanTowerDungeonSegment(Location2D.Get(array4[0].RollCached(), array4[1].RollCached()), array4[2].RollCached(), array4[3].RollCached()));
			}
		}
		ColorOutputMap colorOutputMap = new ColorOutputMap(80, 25);
		for (int k = 0; k < list.Count; k++)
		{
			string text8 = sultanDungeonArgs.primaryTemplate.GetRandomElement();
			int primaryTemplateN = sultanDungeonArgs.primaryTemplateN;
			string text9 = "";
			string randomElement2 = sultanDungeonArgs.secondaryTemplate.GetRandomElement();
			int secondaryTemplateN = sultanDungeonArgs.secondaryTemplateN;
			if (text8.Contains(","))
			{
				string[] array5 = text8.Split(',');
				text8 = array5[0];
				text9 = array5[1];
			}
			Cell cell = Z.GetCell(0, 0);
			if (cell == null)
			{
				throw new Exception("No cell at 0, 0");
			}
			if (Z.GetZoneZ() <= 10)
			{
				cell.AddObject("DaylightWidget");
			}
			if ((!AddSurface || Z.Z > 10 || string.IsNullOrEmpty(cell.PaintTile)) && !string.IsNullOrEmpty(blueprint))
			{
				cell.AddObject(blueprint);
			}
			WaveCollapseFastModel waveCollapseFastModel = new WaveCollapseFastModel(text8, primaryTemplateN, list[k].width(), list[k].height(), periodicInput: true, periodicOutput: false, 8, 0);
			waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
			if (!string.IsNullOrEmpty(text9))
			{
				waveCollapseFastModel.ClearColors(text9);
			}
			waveCollapseFastModel.UpdateSample(randomElement2, secondaryTemplateN, periodicInput: true, periodicOutput: false, 8, 0);
			waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
			ColorOutputMap colorOutputMap2 = new ColorOutputMap(waveCollapseFastModel);
			colorOutputMap2.ReplaceBorders(new Color32(byte.MaxValue, 0, 0, byte.MaxValue), new Color32(0, 0, 0, byte.MaxValue));
			colorOutputMap.Paste(colorOutputMap2, list[k].x1, list[k].y1);
			waveCollapseFastModel = null;
			MemoryHelper.GCCollect();
		}
		LogMem("step2");
		string defaultWall = GetDefaultWall(Z);
		if (Z.Z > 10)
		{
			for (int l = 0; l < Z.Height; l++)
			{
				for (int m = 0; m < Z.Width; m++)
				{
					bool flag = false;
					for (int n = 0; n < list.Count; n++)
					{
						if (list[n].contains(m, l))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						Cell cell2 = Z.GetCell(m, l);
						if (cell2 == null)
						{
							throw new Exception("No cell at " + m + ", " + l);
						}
						if (!cell2.HasWall())
						{
							cell2.AddObject(defaultWall);
						}
					}
				}
			}
		}
		if (AddSurface && Z.Z == 10)
		{
			string terrain = GetTerrain(Z);
			if (!string.IsNullOrEmpty(terrain))
			{
				if (terrain.Contains("Canyon") || terrain.Contains("Hills") || terrain.Contains("Mountains"))
				{
					new Hills().BuildZone(Z);
					Z.GetCell(0, 0).AddObject("Rocky");
				}
				else if (terrain.Contains("Wastes") || terrain.Contains("Desert") || terrain.Contains("dunes"))
				{
					new SaltDunes().BuildZone(Z);
				}
				else if (terrain.Contains("marsh") || terrain.Contains("Watervine"))
				{
					new Watervine().BuildZone(Z);
				}
				else if (terrain.Contains("River") || terrain.Contains("Water") || terrain.Contains("Opal") || terrain.Contains("Stream"))
				{
					new OverlandWater().BuildZone(Z);
				}
				else if (terrain.Contains("Ruins"))
				{
					new Ruins().BuildZone(Z);
				}
				else if (terrain.Contains("Flowerf"))
				{
					new Flowerfields().BuildZone(Z);
				}
				else if (terrain.Contains("Banana"))
				{
					new BananaGrove().BuildZone(Z);
				}
				else
				{
					Z.GetCell(0, 0).AddObject(blueprint);
				}
			}
		}
		int num13 = 0;
		int num14 = 0;
		for (int num15 = 0; num15 < list.Count; num15++)
		{
			string text10 = ResolveOneObjectFromArgs(sultanDungeonArgs.wallTypes, "Fulcrete", Z.NewTier);
			string period = ((!string.IsNullOrEmpty(regionArgs.cultPeriod)) ? regionArgs.cultPeriod : GetSultanPeriodFromTier(Z.NewTier).ToString());
			if (text10.Contains("^"))
			{
				string text11 = "";
				if (!string.IsNullOrEmpty(regionArgs.cultPeriod))
				{
					if (!The.Game.HasStringGameState(text10 + regionName))
					{
						The.Game.SetStringGameState(text10 + regionName, GetDefaultWallByPeriod(period));
					}
					text11 = The.Game.GetStringGameState(text10 + regionName);
				}
				else
				{
					text11 = GetDefaultWallByPeriod(period);
				}
				text10 = (25.in100() ? text10.Replace("^", "") : text11);
			}
			if (text10 == "*auto")
			{
				text10 = GetDefaultWall(Z);
			}
			if (text10.Contains("*SultanWall*"))
			{
				if (string.IsNullOrEmpty(regionArgs.cultPeriod))
				{
					Stat.Random(1, 5).ToString();
				}
				else
				{
					_ = regionArgs.cultPeriod;
				}
				text10 = text10.Replace("*SultanWall*", "InnerSultanWall_Period" + regionArgs.cultPeriod);
			}
			for (int num16 = list[num15].y1; num16 < list[num15].y2; num16++)
			{
				for (int num17 = list[num15].x1; num17 < list[num15].x2; num17++)
				{
					if (!list[num15].contains(num17, num16))
					{
						continue;
					}
					Cell cell3 = Z.GetCell(num17 + num13, num16 + num14);
					if (cell3 == null)
					{
						throw new Exception("No cell at " + (num17 + num13) + ", " + (num16 + num14));
					}
					int num18 = num15 + 1;
					while (true)
					{
						if (num18 < list.Count)
						{
							if (list[num18].contains(num17, num16))
							{
								break;
							}
							num18++;
							continue;
						}
						Color32 a = colorOutputMap.getPixel(num17, num16);
						if (list[num15].HasCustomColor(num17, num16))
						{
							a = list[num15].GetCustomColor(num17, num16);
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.BLACK))
						{
							if (Z.Z == 10)
							{
								cell3.Clear();
							}
							cell3.ClearWalls();
							if (!cell3.HasWall() && !cell3.HasObjectWithTag("NoRuinsRemove"))
							{
								cell3.AddObject(text10);
							}
							break;
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.BLUE))
						{
							if (Z.Z == 10)
							{
								cell3.Clear();
							}
							if (!cell3.HasObjectWithTag("NoRuinsRemove"))
							{
								cell3.AddObject(blueprint3);
							}
							break;
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.YELLOW))
						{
							if (Z.Z == 10)
							{
								cell3.Clear();
							}
							PopulationResult populationResult = PopulationManager.RollOneFrom(text5, null, "Door");
							string blueprint4 = populationResult?.Blueprint;
							if (GameObjectFactory.Factory.GetBlueprintIfExists(text10).HasTag("UsesGate"))
							{
								blueprint4 = GameObjectFactory.Factory.GetBlueprintIfExists(text10).GetTag("UsesGate");
							}
							if (populationResult == null)
							{
								throw new Exception("Got no yellow result from " + text5);
							}
							if (!cell3.HasObjectWithTag("NoRuinsRemove"))
							{
								cell3.AddObject(blueprint4);
							}
							break;
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.GREEN))
						{
							if (Z.Z == 10)
							{
								cell3.Clear();
							}
							PopulationResult populationResult2 = PopulationManager.RollOneFrom(text4, null, "Shimscale Mangrove Tree");
							if (populationResult2 == null)
							{
								throw new Exception("Got no green result from " + text4);
							}
							if (!cell3.HasObjectWithTag("NoRuinsRemove"))
							{
								cell3.AddObject(populationResult2.Blueprint);
							}
							break;
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.RED) || WaveCollapseTools.equals(a, ColorOutputMap.MAGENTA))
						{
							if (Z.Z == 10)
							{
								cell3.Clear();
							}
							cell3.ClearWalls();
							if (!cell3.HasObjectWithTag("NoRuinsRemove"))
							{
								Rocky.Paint(cell3);
							}
						}
						else if (WaveCollapseTools.equals(a, ColorOutputMap.WHITE))
						{
							if (blueprint == "Rocky")
							{
								Rocky.Paint(cell3);
							}
							if (blueprint == "Grassy")
							{
								Grassy.PaintCell(cell3);
							}
							if (blueprint == "Flowery")
							{
								Flowery.Paint(cell3);
							}
							if (blueprint == "Mushroomy")
							{
								Mushroomy.Paint(cell3);
							}
							if (blueprint == "Trashy")
							{
								Trashy.PaintCell(cell3);
							}
							if (blueprint == "Dirty")
							{
								Dirty.PaintCell(cell3);
							}
							if (blueprint == "UndergroundGrassy")
							{
								UndergroundGrassy.PaintCell(cell3);
							}
							if (blueprint == "GrassPaint")
							{
								GrassPaint.PaintCell(cell3);
							}
							if (blueprint == "UndergroundGrassPaint")
							{
								UndergroundGrassPaint.PaintCell(cell3);
							}
						}
						break;
					}
				}
			}
		}
		LogMem("step3a");
		foreach (PopulationResult item in ResolveOnePopulationFromArgs(sultanDungeonArgs.preconnectencounters, tablePrefix + "PreconnectEncounters_*Default", Z.NewTier))
		{
			for (int num19 = 0; num19 < item.Number; num19++)
			{
				ZoneBuilderSandbox.PlaceObjectInRect(Z, new Rect2D(5, 5, Z.Width - 5, Z.Height - 5), item.Blueprint, item.Hint, num19, sultanDungeonSemanticCreate);
			}
		}
		LogMem("step3b");
		if (Z.Z > 10)
		{
			int num20 = Stat.Random(num3, num3);
			int num21 = Stat.Random(num4, num4);
			for (int num22 = 0; num22 < Z.Width; num22++)
			{
				int num23 = 0;
				for (int num24 = num20 + Stat.Random(0, 2); num23 < num24; num23++)
				{
					Cell cell4 = Z.GetCell(num22, num23);
					if (cell4.HasWall())
					{
						if (PointInSegments(list, num22, num23))
						{
							break;
						}
					}
					else
					{
						cell4.AddObject(defaultWall);
					}
				}
				int num25 = Z.Height - 1;
				int num26 = num21 - Stat.Random(0, 2);
				while (num25 > num26)
				{
					Cell cell5 = Z.GetCell(num22, num25);
					if (cell5.HasWall())
					{
						if (PointInSegments(list, num22, num25))
						{
							break;
						}
					}
					else
					{
						cell5.AddObject(defaultWall);
					}
					num25--;
				}
			}
			int num27 = Stat.Random(num, num);
			int num28 = Stat.Random(num2, num2);
			for (int num29 = 1; num29 < Z.Height - 1; num29++)
			{
				int num30 = 0;
				for (int num31 = num27 + Stat.Random(0, 2); num30 < num31; num30++)
				{
					Cell cell6 = Z.GetCell(num30, num29);
					if (cell6.HasWall())
					{
						if (PointInSegments(list, num30, num29))
						{
							break;
						}
					}
					else
					{
						cell6.AddObject(defaultWall);
					}
				}
				int num32 = Z.Width - 1;
				int num33 = num28 - Stat.Random(0, 2);
				while (num32 > num33)
				{
					Cell cell7 = Z.GetCell(num32, num29);
					if (cell7.HasWall())
					{
						if (PointInSegments(list, num32, num29))
						{
							break;
						}
					}
					else
					{
						cell7.AddObject(defaultWall);
					}
					num32--;
				}
			}
		}
		LogMem("step4");
		InfluenceMap influenceMap = new InfluenceMap(colorOutputMap.width, colorOutputMap.height);
		for (int num34 = 0; num34 < colorOutputMap.height; num34++)
		{
			for (int num35 = 0; num35 < colorOutputMap.width; num35++)
			{
				influenceMap.Walls[num35, num34] = (Z.GetCell(num35, num34).HasWall() ? 1 : 0);
			}
		}
		influenceMap.SeedAllUnseeded();
		if (influenceMap.Seeds.Count == 0)
		{
			influenceMap.Walls[Stat.Random(10, 70), Stat.Random(5, 15)] = 0;
			influenceMap.Walls[Stat.Random(10, 70), Stat.Random(5, 15)] = 0;
			influenceMap.Walls[Stat.Random(10, 70), Stat.Random(5, 15)] = 0;
			influenceMap.SeedAllUnseeded();
		}
		influenceMap.MoveSeedsToCenters();
		int num36 = 0;
		using (Pathfinder pathfinder = Z.getPathfinder())
		{
			for (int num37 = 0; num37 < colorOutputMap.height; num37++)
			{
				for (int num38 = 0; num38 < colorOutputMap.width; num38++)
				{
					if (influenceMap.Walls[num38, num37] > 0)
					{
						pathfinder.CurrentNavigationMap[num38, num37] = 999;
					}
					else
					{
						pathfinder.CurrentNavigationMap[num38, num37] = 0;
					}
				}
			}
			LogMem("step5");
			List<int> list2 = new List<int>();
			list2.Add(num36);
			for (int num39 = 0; num39 < influenceMap.Seeds.Count; num39++)
			{
				if (list2.Contains(num39))
				{
					continue;
				}
				int num40 = influenceMap.FindClosestSeedToInList(num39, list2);
				if (num40 == -1)
				{
					num40 = num36;
				}
				if (pathfinder.FindPath(influenceMap.Seeds[num40], influenceMap.Seeds[num39]))
				{
					int num41 = 0;
					foreach (PathfinderNode step in pathfinder.Steps)
					{
						Cell cell8 = Z.GetCell(step.X, step.Y);
						if (cell8.HasWall())
						{
							if (num41 == 0)
							{
								if (cell8.HasObjectWithTag("UsesGate"))
								{
									string tag = cell8.GetObjectWithTag("UsesGate").GetTag("UsesGate", "Iron Gate");
									cell8.AddObject(tag);
								}
								else
								{
									cell8.AddObject("Door");
								}
								num41++;
							}
							cell8.ClearWalls();
							pathfinder.CurrentNavigationMap[step.X, step.Y] = 0;
							step.weight = 0;
						}
						else if (Z.Z == 10)
						{
							cell8.Clear();
						}
					}
				}
				list2.Add(num39);
			}
			foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(Z.ZoneID))
			{
				Location2D location2D = Location2D.Get(zoneConnection.X, zoneConnection.Y);
				int num42 = influenceMap.FindClosestSeedToInList(location2D, list2);
				if (num42 == -1)
				{
					num42 = num36;
				}
				if (!pathfinder.FindPath(influenceMap.Seeds[num42], location2D))
				{
					continue;
				}
				int num43 = 0;
				foreach (PathfinderNode step2 in pathfinder.Steps)
				{
					Cell cell9 = Z.GetCell(step2.X, step2.Y);
					if (cell9.HasWall())
					{
						if (num43 == 0)
						{
							if (cell9.HasObjectWithTag("UsesGate"))
							{
								string tag2 = cell9.GetObjectWithTag("UsesGate").GetTag("UsesGate", "Iron Gate");
								cell9.AddObject(tag2);
							}
							else
							{
								cell9.AddObject("Door");
							}
							num43++;
						}
						cell9.ClearWalls();
						pathfinder.CurrentNavigationMap[step2.X, step2.Y] = 0;
						step2.weight = 0;
					}
					else if (Z.Z == 10)
					{
						cell9.Clear();
					}
				}
			}
			LogMem("step6");
			for (int num44 = 0; num44 < list2.Count; num44++)
			{
				if (!Z.IsReachable(influenceMap.Seeds[num44].X, influenceMap.Seeds[num44].Y))
				{
					Z.BuildReachableMap(influenceMap.Seeds[num44].X, influenceMap.Seeds[num44].Y);
				}
			}
		}
		if (regionArgs.properties.Contains("Waste") || sultanDungeonArgs.properties.Contains("Waste"))
		{
			int oracleIntColumn = ZoneBuilderSandbox.GetOracleIntColumn(Z, 1, 4);
			bool chuteLevel = Stat.Random(1, 100) <= 30;
			new GenericChutes().BuildZone(Z, oracleIntColumn, chuteLevel);
		}
		if (stairs.Contains("U"))
		{
			new StairsUp().BuildZone(Z);
		}
		if (stairs.Contains("D"))
		{
			new StairsDown().BuildZone(Z);
		}
		List<Point2D> list3 = new List<Point2D>();
		if (!stairs.Contains("D") && influenceMap != null && influenceMap.Seeds.Count > 0)
		{
			list3.Add(influenceMap.Seeds[0].Point);
		}
		new ForceConnections()._BuildZone(Z, list3);
		LogMem("step7");
		for (int num45 = 0; num45 < 500; num45++)
		{
			bool flag2 = false;
			for (int num46 = 1; num46 < Z.Width - 1; num46++)
			{
				for (int num47 = 1; num47 < Z.Height - 1; num47++)
				{
					Cell cell10 = Z.GetCell(num46, num47);
					GameObject firstObjectWithPart = cell10.GetFirstObjectWithPart("Door");
					if (firstObjectWithPart != null && firstObjectWithPart.CanClear())
					{
						int num48 = 0;
						int num49 = 0;
						if (Z.GetCell(num46 - 1, num47).HasWall())
						{
							num49++;
						}
						if (Z.GetCell(num46 + 1, num47).HasWall())
						{
							num49++;
						}
						if (Z.GetCell(num46, num47 - 1).HasWall())
						{
							num48++;
						}
						if (Z.GetCell(num46, num47 + 1).HasWall())
						{
							num48++;
						}
						if (num48 + num49 != 2 || num48 == 1 || num49 == 1)
						{
							flag2 |= cell10.RemoveObject(firstObjectWithPart);
						}
					}
				}
			}
			if (!flag2)
			{
				break;
			}
		}
		LogMem("step8");
		GameObject gameObject = Z.FindObject("StairsUp");
		GameObject gameObject2 = Z.FindObject("StairsDown");
		List<Location2D> list4 = new List<Location2D>();
		if (gameObject != null)
		{
			list4.Add(gameObject.GetCurrentCell().Location);
		}
		if (gameObject2 != null)
		{
			list4.Add(gameObject2.GetCurrentCell().Location);
		}
		LogMem("step9");
		int num50 = 0;
		while (influenceMap.LargestSize() > 150)
		{
			influenceMap.AddSeedAtMaximaInLargestSeed();
			num50++;
			LogMem("adseed " + num50);
		}
		LogMem("step10");
		InfluenceMap influenceMap2 = new InfluenceMap(80, 25);
		influenceMap2.Seeds = new List<Location2D>(influenceMap.Seeds);
		Z.SetInfluenceMapWalls(influenceMap2.Walls);
		influenceMap2.Recalculate();
		LogMem("step11");
		int num51 = 0;
		if (80.in100())
		{
			num51++;
		}
		Dictionary<InfluenceMapRegion, Rect2D> dictionary = new Dictionary<InfluenceMapRegion, Rect2D>();
		Dictionary<InfluenceMapRegion, string> dictionary2 = new Dictionary<InfluenceMapRegion, string>();
		Dictionary<InfluenceMapRegion, string> dictionary3 = new Dictionary<InfluenceMapRegion, string>();
		LogMem("step12");
		List<InfluenceMapRegion> list5 = new List<InfluenceMapRegion>();
		if (Z.GetZoneProperty("Relicstyle") == "Vault")
		{
			List<InfluenceMapRegion> list6 = new List<InfluenceMapRegion>();
			foreach (InfluenceMapRegion item2 in influenceMap.Regions.Shuffle())
			{
				Rect2D rect2D = GridTools.MaxRectByArea(item2.GetGrid()).Translate(item2.BoundingBox.UpperLeft);
				dictionary.Add(item2, rect2D);
				if (rect2D.Width < 4 || rect2D.Height < 4 || item2.AnyPointsIn(list4) || item2.AdjacentRegions.Count != 1)
				{
					continue;
				}
				list6.Add(item2);
				ZoneBuilderSandbox.PlaceObjectInRegionRect(Z, item2, rect2D, "N", "RelicChest", null, null, null, null, sultanDungeonSemanticCreate);
				dictionary2.SetValue(item2, "vault");
				goto IL_1aed;
			}
			foreach (InfluenceMapRegion item3 in influenceMap.Regions.Shuffle())
			{
				Rect2D r = dictionary[item3];
				if (r.Width < 4 || r.Height < 4 || item3.AnyPointsIn(list4))
				{
					continue;
				}
				list6.Add(item3);
				ZoneBuilderSandbox.PlaceObjectInRegionRect(Z, item3, r, "N", "RelicChest", null, null, null, null, sultanDungeonSemanticCreate);
				dictionary2.SetValue(item3, "vault");
				goto IL_1aed;
			}
			if (list6.Count == 0)
			{
				foreach (InfluenceMapRegion region in influenceMap.Regions)
				{
					if (region.AdjacentRegions.Count == 1 && !region.AnyPointsIn(list4))
					{
						list6.Add(region);
					}
				}
			}
			if (list6.Count == 0)
			{
				list6.Add(influenceMap.Regions.GetRandomElement());
			}
			InfluenceMapRegion randomElement3 = list6.GetRandomElement();
			list5.Add(randomElement3);
			ZoneBuilderSandbox.PlaceObjectInRegion(Z, randomElement3, "RelicChest", 0, 0, null, null, bAllowCaching: false, null, sultanDungeonSemanticCreate);
			dictionary2.SetValue(randomElement3, "vault");
		}
		goto IL_1aed;
		IL_1aed:
		foreach (InfluenceMapRegion region2 in influenceMap.Regions)
		{
			if (list5.Contains(region2))
			{
				continue;
			}
			Rect2D value;
			if (!dictionary.ContainsKey(region2))
			{
				value = GridTools.MaxRectByArea(region2.GetGrid()).Translate(region2.BoundingBox.UpperLeft);
				dictionary.Add(region2, value);
			}
			else
			{
				value = dictionary[region2];
			}
			if (region2.BoundingBox.x1 > 0 && region2.BoundingBox.x2 < 79 && region2.BoundingBox.y1 > 0 && region2.BoundingBox.y2 < 24 && !region2.AnyPointsIn(list4))
			{
				if (value.Area >= 4)
				{
					if (regionArgs.furnishings.Count > 0)
					{
						ZoneBuilderSandbox.PlacePopulationInRegion(Z, region2, regionArgs.furnishings.GetRandomElement(), null, sultanDungeonSemanticCreate);
					}
					dictionary3.Add(region2, "room");
				}
				else if (influenceMap2.SeedToRegionMap[region2.Seed].AdjacentRegions.Count == 1)
				{
					if (num51 > 0)
					{
						num51--;
						ZoneBuilderSandbox.PlacePopulationInRegion(Z, region2, "RandomChest", null, sultanDungeonSemanticCreate);
					}
					else if (regionArgs.cubbies.Count > 0)
					{
						ZoneBuilderSandbox.PlacePopulationInRegion(Z, region2, regionArgs.cubbies.GetRandomElement(), null, sultanDungeonSemanticCreate);
					}
					dictionary3.Add(region2, "cubby");
				}
				else
				{
					if (regionArgs.halls.Count > 0)
					{
						ZoneBuilderSandbox.PlacePopulationInRegion(Z, region2, regionArgs.halls.GetRandomElement(), null, sultanDungeonSemanticCreate);
					}
					dictionary3.Add(region2, "hall");
				}
			}
			else
			{
				dictionary3.Add(region2, "connection");
			}
		}
		LogMem("step13");
		Queue<InfluenceMapRegion> queue = new Queue<InfluenceMapRegion>(Algorithms.RandomShuffle(influenceMap2.Regions));
		while (queue.Count > 0)
		{
			InfluenceMapRegion influenceMapRegion = queue.Dequeue();
			foreach (InfluenceMapRegion adjacentRegion in influenceMapRegion.AdjacentRegions)
			{
				if (!dictionary2.ContainsKey(influenceMap.SeedToRegionMap[adjacentRegion.Seed]) || !(dictionary2[influenceMap.SeedToRegionMap[adjacentRegion.Seed]] != "vault"))
				{
					continue;
				}
				if (!dictionary2.ContainsKey(influenceMap.SeedToRegionMap[influenceMapRegion.Seed]))
				{
					dictionary2.Add(influenceMap.SeedToRegionMap[influenceMapRegion.Seed], dictionary2[influenceMap.SeedToRegionMap[adjacentRegion.Seed]]);
				}
				goto IL_1e75;
			}
			if (influenceMapRegion.AdjacentRegions.Count <= 1)
			{
				if (!dictionary2.ContainsKey(influenceMap.SeedToRegionMap[influenceMapRegion.Seed]))
				{
					dictionary2.Add(influenceMap.SeedToRegionMap[influenceMapRegion.Seed], "abandoned");
				}
			}
			else if (!dictionary2.ContainsKey(influenceMap.SeedToRegionMap[influenceMapRegion.Seed]))
			{
				dictionary2.Add(influenceMap.SeedToRegionMap[influenceMapRegion.Seed], "cult");
			}
			IL_1e75:;
		}
		LogMem("step14");
		int num52 = Stat.Random(7, 10);
		int num53 = 0;
		LogMem("step15");
		foreach (InfluenceMapRegion item4 in influenceMap.Regions.Shuffle())
		{
			if (list5.Contains(item4))
			{
				continue;
			}
			if (dictionary2[item4] == "vault")
			{
				string text12 = ResolveCultRole("Hero", sultanDungeonArgs, Z.NewTier);
				if (!string.IsNullOrEmpty(text12))
				{
					GameObject gameObject3 = GameObject.Create(text12);
					while (gameObject3.Render == null)
					{
						Debug.LogError("generated cult hero with no render part from " + text12);
						text12 = ResolveCultRole("Hero", sultanDungeonArgs, Z.NewTier);
						gameObject3 = GameObject.Create(text12);
					}
					EnrollInCult(gameObject3, sultanDungeonArgs);
					if (!gameObject3.HasPart<GivesRep>())
					{
						gameObject3 = HeroMaker.MakeHero(gameObject3);
					}
					gameObject3.RequirePart<SocialRoles>().RemoveRole("member of the " + sultanDungeonArgs.cultName);
					gameObject3.RequirePart<SocialRoles>().RequireRole("{{M|leader of the " + sultanDungeonArgs.cultName + "}}");
					ZoneBuilderSandbox.PlaceObjectInArea(Z, item4, gameObject3);
				}
			}
			else if (dictionary2[item4] == "cult")
			{
				if (num53 > 0)
				{
					num53--;
				}
				else
				{
					if (num52 <= 0)
					{
						continue;
					}
					num52--;
					List<PopulationResult> list7 = ResolveOnePopulationFromArgs(sultanDungeonArgs.encounters, tablePrefix + "Encounters_*Default", Z.NewTier);
					int num54 = 0;
					foreach (PopulationResult item5 in list7)
					{
						num54++;
						string text13 = "snapjaw scavenger";
						bool flag3 = false;
						if (item5.Blueprint.StartsWith("="))
						{
							text13 = ResolveCultRole(item5.Blueprint.Substring(1), sultanDungeonArgs, Z.NewTier);
							flag3 = true;
						}
						else
						{
							text13 = item5.Blueprint;
						}
						if (string.IsNullOrEmpty(text13))
						{
							continue;
						}
						for (int num55 = 0; num55 < item5.Number; num55++)
						{
							GameObject gameObject4 = GameObject.Create(text13);
							if (flag3)
							{
								EnrollInCult(gameObject4, sultanDungeonArgs);
							}
							ZoneBuilderSandbox.PlaceObjectInArea(Z, item4, gameObject4, num54, num55);
						}
					}
				}
			}
			else if (dictionary2[item4] == "abandoned")
			{
				ZoneBuilderSandbox.PlacePopulationInRegionRect(Z, item4, dictionary[item4], "", "CaveDecoration", null, null, sultanDungeonSemanticCreate);
				ZoneBuilderSandbox.PlacePopulationInRegionRect(Z, item4, dictionary[item4], "", "Tier" + Z.NewTier + "CavePopulation", null, null, sultanDungeonSemanticCreate);
			}
		}
		LogMem("step16");
		MemoryHelper.GCCollect();
		return true;
	}

	public void EnrollInCult(GameObject GO, SultanDungeonArgs args)
	{
		if (!string.IsNullOrEmpty(args.cultFaction))
		{
			GO.RequirePart<SocialRoles>().RequireRole("member of the " + args.cultName);
			if (GO.Brain != null)
			{
				GO.Brain.FactionFeelings.Clear();
				GO.Brain.Allegiance.Clear();
				GO.Brain.Allegiance.Add(args.cultFaction, 100);
			}
		}
	}

	private void ProcessBlueprintForCultRole(GameObjectBlueprint B)
	{
		string key = "Minion";
		if (B.Tags.TryGetValue("Role", out var value) && !value.IsNullOrEmpty())
		{
			key = value;
		}
		if (B.Props.TryGetValue("Role", out value) && !value.IsNullOrEmpty())
		{
			key = value;
		}
		if (!roleMembers.TryGetValue(key, out var value2))
		{
			value2 = new List<string>();
			roleMembers.Add(key, value2);
		}
		if (!memberTier.ContainsKey(B.Name))
		{
			value2.Add(B.Name);
			allMembers.Add(B.Name);
			memberTier.Add(B.Name, B.Tier);
			memberHasBody.Add(B.Name, B.HasPart("Body"));
		}
	}

	public string ResolveCultRole(string Role, SultanDungeonArgs args, int targetTier)
	{
		if (roleMembers == null)
		{
			roleMembers = new Dictionary<string, List<string>>(16);
			allMembers = new List<string>(16);
			memberTier = new Dictionary<string, int>(16);
			memberHasBody = new Dictionary<string, bool>(16);
			if (args.cultFactions.Count > 0)
			{
				foreach (string cultFaction in args.cultFactions)
				{
					foreach (GameObjectBlueprint factionMember in GameObjectFactory.Factory.GetFactionMembers(cultFaction))
					{
						ProcessBlueprintForCultRole(factionMember);
					}
				}
			}
			if (!string.IsNullOrEmpty(monstertable))
			{
				foreach (string item in PopulationManager.GetEach(monstertable, new Dictionary<string, string> { 
				{
					"ZoneTier",
					targetTier.ToString()
				} }))
				{
					GameObjectBlueprint b = GameObjectFactory.Factory.Blueprints[item];
					ProcessBlueprintForCultRole(b);
				}
			}
		}
		bool flag = false;
		if (Role == "Hero")
		{
			flag = true;
			if (!roleMembers.ContainsKey("Hero"))
			{
				Role = "Leader";
			}
		}
		List<string> list = allMembers;
		if (roleMembers.TryGetValue(Role, out var value) && value.Count > 0)
		{
			list = value;
		}
		int num = 12;
		while (num.in100())
		{
			targetTier = ((!15.in100()) ? (targetTier + 1) : (targetTier - 1));
			num = Math.Max(num - 6, 1);
		}
		Tier.Constrain(ref targetTier);
		List<string> list2 = new List<string>();
		for (int i = 0; i < 8; i++)
		{
			if (flag)
			{
				list2.Clear();
				int j = 0;
				for (int count = list.Count; j < count; j++)
				{
					if ((memberTier[list[j]] - i == targetTier || memberTier[list[j]] + i == targetTier) && memberHasBody[list[j]])
					{
						list2.Add(list[j]);
					}
				}
				if (list2.Count > 0)
				{
					return list2.GetRandomElement();
				}
				list2.Clear();
				int k = 0;
				for (int count2 = allMembers.Count; k < count2; k++)
				{
					if ((memberTier[allMembers[k]] - i == targetTier || memberTier[allMembers[k]] + i == targetTier) && memberHasBody[allMembers[k]])
					{
						list2.Add(allMembers[k]);
					}
				}
				if (list2.Count > 0)
				{
					return list2.GetRandomElement();
				}
			}
			list2.Clear();
			int l = 0;
			for (int count3 = list.Count; l < count3; l++)
			{
				if (memberTier[list[l]] - i == targetTier || memberTier[list[l]] + i == targetTier)
				{
					list2.Add(list[l]);
				}
			}
			if (list2.Count > 0)
			{
				return list2.GetRandomElement();
			}
			list2.Clear();
			int m = 0;
			for (int count4 = allMembers.Count; m < count4; m++)
			{
				if (memberTier[allMembers[m]] - i == targetTier || memberTier[allMembers[m]] + i == targetTier)
				{
					list2.Add(allMembers[m]);
				}
			}
			if (list2.Count > 0)
			{
				return list2.GetRandomElement();
			}
		}
		if (value != null && value.Count > 0)
		{
			if (flag)
			{
				list2.Clear();
				int n = 0;
				for (int count5 = value.Count; n < count5; n++)
				{
					if (memberHasBody[value[n]])
					{
						list2.Add(value[n]);
					}
				}
				if (list2.Count > 0)
				{
					return list2.GetRandomElement();
				}
			}
			return value.GetRandomElement();
		}
		return null;
	}

	public static int GetSultanPeriodFromTier(int tier)
	{
		switch (tier)
		{
		case 0:
		case 1:
		case 2:
			return 5;
		case 3:
		case 4:
			return 4;
		case 5:
		case 6:
			return 3;
		case 7:
			return 2;
		case 8:
			return 1;
		default:
			return 5;
		}
	}

	public static string GetDefaultWallByPeriod(string period)
	{
		return PopulationManager.RollOneFrom("SultanDungeons_Wall_Default_Period" + period)?.Blueprint;
	}
}
