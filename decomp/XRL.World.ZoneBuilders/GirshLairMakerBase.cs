using System;
using System.Collections.Generic;
using Genkit;
using HistoryKit;
using UnityEngine;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public abstract class GirshLairMakerBase : ZoneBuilderSandbox
{
	public Zone Zone;

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

	public abstract string SecretID { get; }

	public abstract string CradleDisplayName { get; }

	public virtual bool IsCradle => true;

	public virtual int DiscoveryXP => 4000;

	public virtual int MutateChance => 75;

	public virtual bool BuildZone(Zone Z)
	{
		Zone = Z;
		bool isCradle = IsCradle;
		if (isCradle)
		{
			SetCradleProperties();
			AddLocationFinder();
		}
		if (!BuildLair(Z))
		{
			return false;
		}
		if (isCradle)
		{
			Zone.ObjectEnumerator enumerator = Z.IterateObjects().GetEnumerator();
			while (enumerator.MoveNext())
			{
				GameObject current = enumerator.Current;
				if (current.IsCombatObject() && !current.HasProperName && !current.IsPlayerControlled() && !current.IsFactionMember("Girsh") && Stat.Random(1, 100) <= MutateChance)
				{
					MutateObject(current);
					current.Brain.Allegiance["Girsh"] = 99;
				}
			}
			AddHolyPlace();
		}
		return true;
	}

	public virtual void SetCradleProperties()
	{
		Zone.SetMusic("Music/of Chrome and How");
		Zone.IncludeContextInZoneDisplay = true;
		Zone.HasProperName = true;
		Zone.BaseDisplayName = CradleDisplayName;
	}

	public virtual void AddLocationFinder()
	{
		if (Zone.GetCell(0, 0).AddObject("LocationFinder").TryGetPart<LocationFinder>(out var Part))
		{
			Part.ID = SecretID;
			Part.Value = DiscoveryXP;
		}
	}

	public virtual void AddHolyPlace()
	{
		Zone.GetCell(0, 0).RequireObject("HolyPlaceNephilimWidget");
	}

	public abstract bool BuildLair(Zone Z);

	public abstract void MutateObject(GameObject Object);

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

	public bool BuildZoneFromArgs(Zone Z, SultanDungeonArgs regionArgs, bool AddSurface, bool Clear = true)
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
		The.Game.sultanHistory.GetEntitiesWherePropertyEquals("name", locationName).entities.GetRandomElement()?.GetCurrentSnapshot();
		SultanDungeonArgs sultanDungeonArgs = regionArgs.clone();
		string randomElement = regionArgs.segmentations.GetRandomElement();
		string populationName = ResolveOneTableFromArgs(sultanDungeonArgs.greenObjects);
		string text3 = ResolveOneTableFromArgs(sultanDungeonArgs.yellowObjects);
		string text4 = ResolveOneTableFromArgs(sultanDungeonArgs.blueObjects);
		string text5 = ((text4 != null) ? (PopulationManager.RollOneFrom(text4, null, "SaltyWaterPuddle") ?? throw new Exception("Got no blue object result from " + text4)).Blueprint : "SaltyWaterPuddle");
		List<ISultanDungeonSegment> list = new List<ISultanDungeonSegment>();
		int num = 7;
		int num2 = 72;
		int num3 = 7;
		int num4 = 17;
		string[] array = randomElement.Split(';');
		foreach (string text6 in array)
		{
			if (text6 == "Full")
			{
				list.Add(new SultanRectDungeonSegment(new Rect2D(2, 2, 78, 22)));
			}
			else if (text6.StartsWith("BSP:"))
			{
				int nSegments = Convert.ToInt32(text6.Split(':')[1]);
				partition(new Rect2D(2, 2, 78, 24), ref nSegments, list);
			}
			else if (text6.StartsWith("Ring:"))
			{
				int num5 = Convert.ToInt32(text6.Split(':')[1]);
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
			else if (text6.StartsWith("Blocks"))
			{
				string[] array2 = text6.Split(':')[1].Split(',');
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
			else if (text6.StartsWith("Circle"))
			{
				string[] array3 = text6.Split(':')[1].Split(',');
				list.Add(new SultanTrueCircleDungeonSegment(Location2D.Get(array3[0].RollCached(), array3[1].RollCached()), array3[2].RollCached()));
			}
			else if (text6.StartsWith("Tower"))
			{
				string[] array4 = text6.Split(':')[1].Split(',');
				list.Add(new SultanTowerDungeonSegment(Location2D.Get(array4[0].RollCached(), array4[1].RollCached()), array4[2].RollCached(), array4[3].RollCached()));
			}
		}
		ColorOutputMap colorOutputMap = new ColorOutputMap(80, 25);
		for (int k = 0; k < list.Count; k++)
		{
			string text7 = "";
			string text8 = tablePrefix + "PrimaryTemplate_*Default";
			PopulationResult populationResult = PopulationManager.RollOneFrom(text8);
			if (populationResult == null)
			{
				throw new Exception("Got no pass 1 blueprint result from " + text8);
			}
			if (k == 0 || Stat.Random(1, 100) <= 75)
			{
				text7 = ResolveOneObjectFromArgs(sultanDungeonArgs.primaryTemplate, populationResult.Blueprint, Z.NewTier);
			}
			else
			{
				text7 = populationResult.Blueprint;
			}
			text7 = regionArgs.primaryTemplate.GetRandomElement();
			int primaryTemplateN = sultanDungeonArgs.primaryTemplateN;
			string text9 = "";
			string text10 = "";
			string text11 = tablePrefix + "SecondaryTemplate_*Default";
			PopulationResult populationResult2 = PopulationManager.RollOneFrom(text11);
			if (populationResult2 == null)
			{
				throw new Exception("Got no pass 2 blueprint result from " + text11);
			}
			if (k == 0 || Stat.Random(1, 100) <= 75)
			{
				text10 = ResolveOneObjectFromArgs(sultanDungeonArgs.secondaryTemplate, populationResult2.Blueprint, Z.NewTier);
			}
			else
			{
				text10 = populationResult2.Blueprint;
			}
			text10 = regionArgs.secondaryTemplate.GetRandomElement();
			int secondaryTemplateN = sultanDungeonArgs.secondaryTemplateN;
			if (text7.Contains(","))
			{
				string[] array5 = text7.Split(',');
				text7 = array5[0];
				text9 = array5[1];
			}
			if (text10.Contains(","))
			{
				text10 = text10.Split(',')[0];
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
			WaveCollapseFastModel waveCollapseFastModel = new WaveCollapseFastModel(text7, primaryTemplateN, list[k].width(), list[k].height(), periodicInput: true, periodicOutput: false, 8, 0);
			waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
			if (!string.IsNullOrEmpty(text9))
			{
				waveCollapseFastModel.ClearColors(text9);
			}
			waveCollapseFastModel.UpdateSample(text10, secondaryTemplateN, periodicInput: true, periodicOutput: false, 8, 0);
			waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
			ColorOutputMap map = new ColorOutputMap(waveCollapseFastModel);
			colorOutputMap.Paste(map, list[k].x1, list[k].y1);
			waveCollapseFastModel = null;
			MemoryHelper.GCCollect();
		}
		LogMem("step2");
		string defaultWall = GetDefaultWall(Z);
		if ((Z.Z > 10) & Clear)
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
			string text12 = ResolveOneObjectFromArgs(sultanDungeonArgs.wallTypes, "Fulcrete", Z.NewTier);
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
								cell3.AddObject(text12);
							}
							break;
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.BLUE))
						{
							if (Z.Z == 10)
							{
								cell3.Clear();
							}
							if (text5 != null && !cell3.HasObjectWithTag("NoRuinsRemove"))
							{
								cell3.AddObject(text5);
							}
							break;
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.YELLOW))
						{
							if (Z.Z == 10)
							{
								cell3.Clear();
							}
							PopulationResult populationResult3 = PopulationManager.RollOneFrom(text3, null, "Door");
							string blueprint2 = populationResult3?.Blueprint;
							if (GameObjectFactory.Factory.GetBlueprintIfExists(text12).HasTag("UsesGate"))
							{
								blueprint2 = GameObjectFactory.Factory.GetBlueprintIfExists(text12).GetTag("UsesGate");
							}
							if (populationResult3 == null)
							{
								throw new Exception("Got no yellow result from " + text3);
							}
							if (!cell3.HasObjectWithTag("NoRuinsRemove"))
							{
								cell3.AddObject(blueprint2);
							}
							break;
						}
						if (WaveCollapseTools.equals(a, ColorOutputMap.GREEN))
						{
							if (Z.Z == 10)
							{
								cell3.Clear();
							}
							PopulationResult populationResult4 = PopulationManager.RollOneFrom(populationName);
							if (populationResult4 != null && populationResult4.Number > 0 && !cell3.HasObjectWithTag("NoRuinsRemove"))
							{
								cell3.AddObject(populationResult4.Blueprint);
							}
						}
						else if (WaveCollapseTools.equals(a, ColorOutputMap.RED) || WaveCollapseTools.equals(a, ColorOutputMap.MAGENTA))
						{
							if (Clear && Z.Z == 10)
							{
								cell3.Clear();
							}
							if (Clear)
							{
								cell3.ClearWalls();
							}
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
		return true;
	}

	public void ConnectRegions(Zone Z, ColorOutputMap Output)
	{
		InfluenceMap influenceMap = new InfluenceMap(Output.width, Output.height);
		for (int i = 0; i < Output.height; i++)
		{
			for (int j = 0; j < Output.width; j++)
			{
				influenceMap.Walls[j, i] = (Z.GetCell(j, i).HasWall() ? 1 : 0);
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
		int num = 0;
		using Pathfinder pathfinder = Z.getPathfinder();
		for (int k = 0; k < Output.height; k++)
		{
			for (int l = 0; l < Output.width; l++)
			{
				if (influenceMap.Walls[l, k] > 0)
				{
					pathfinder.CurrentNavigationMap[l, k] = 999;
				}
				else
				{
					pathfinder.CurrentNavigationMap[l, k] = 0;
				}
			}
		}
		LogMem("step5");
		List<int> list = new List<int>();
		list.Add(num);
		for (int m = 0; m < influenceMap.Seeds.Count; m++)
		{
			if (list.Contains(m))
			{
				continue;
			}
			int num2 = influenceMap.FindClosestSeedToInList(m, list);
			if (num2 == -1)
			{
				num2 = num;
			}
			if (pathfinder.FindPath(influenceMap.Seeds[num2], influenceMap.Seeds[m]))
			{
				int num3 = 0;
				int num4 = 0;
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Cell cell = Z.GetCell(step.X, step.Y);
					if (cell.HasWall())
					{
						if (ConnectStep(cell, num3++, num4++))
						{
							pathfinder.CurrentNavigationMap[step.X, step.Y] = 0;
							step.weight = 0;
						}
					}
					else
					{
						num4 = 0;
						if (Z.Z == 10)
						{
							cell.Clear();
						}
					}
				}
			}
			list.Add(m);
		}
		foreach (ZoneConnection item in Z.EnumerateConnections())
		{
			Location2D location2D = Location2D.Get(item.X, item.Y);
			int num5 = influenceMap.FindClosestSeedToInList(location2D, list);
			if (num5 == -1)
			{
				num5 = num;
			}
			if (!pathfinder.FindPath(influenceMap.Seeds[num5], location2D))
			{
				continue;
			}
			int num6 = 0;
			int num7 = 0;
			foreach (PathfinderNode step2 in pathfinder.Steps)
			{
				Cell cell2 = Z.GetCell(step2.X, step2.Y);
				if (cell2.HasWall())
				{
					if (ConnectStep(cell2, num6++, num7++))
					{
						pathfinder.CurrentNavigationMap[step2.X, step2.Y] = 0;
						step2.weight = 0;
					}
				}
				else
				{
					num7 = 0;
					if (Z.Z == 10)
					{
						cell2.Clear();
					}
				}
			}
		}
		LogMem("step6");
		for (int n = 0; n < list.Count; n++)
		{
			if (!Z.IsReachable(influenceMap.Seeds[n].X, influenceMap.Seeds[n].Y))
			{
				Z.BuildReachableMap(influenceMap.Seeds[n].X, influenceMap.Seeds[n].Y);
			}
		}
	}

	public virtual bool ConnectStep(Cell Cell, int Step, int Consecutive)
	{
		Cell.ClearWalls();
		if (Step == 0)
		{
			if (Cell.HasObjectWithTag("UsesGate"))
			{
				string tag = Cell.GetObjectWithTag("UsesGate").GetTag("UsesGate", "Iron Gate");
				Cell.AddObject(tag);
			}
			else
			{
				Cell.AddObject("Door");
			}
		}
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

	public void CreateCyst(Zone Z, Location2D Location, string Blueprint, int MinRadius = 6, int MaxRadius = 8, System.Random Random = null, bool FullClear = false, bool Paint = false)
	{
		if (Random == null)
		{
			Random = Stat.Rnd;
		}
		int num = Random.Next(MinRadius, MaxRadius + 1);
		float num2 = (float)Random.Next(80, 91) / 75f;
		float num3 = (float)Random.Next(80, 91) / 75f;
		int num4 = (int)((float)num * num2);
		int num5 = (int)((float)num * num3);
		for (int i = -num4; i <= num4; i++)
		{
			for (int j = -num5; j <= num5; j++)
			{
				Cell cell = Z.GetCell(Location.X + i, Location.Y + j);
				if (cell == null || cell.HasStairs())
				{
					continue;
				}
				int num6 = (int)Math.Sqrt(num2 * (float)Math.Abs(i) * (num2 * (float)Math.Abs(i)) + (float)Math.Abs(j) * num3 * ((float)Math.Abs(j) * num3));
				if (num6 <= num)
				{
					if (FullClear)
					{
						cell.Clear();
					}
					else
					{
						cell.ClearWalls();
					}
					if (Paint)
					{
						cell.PaintTile = "Tiles/tile-dirt1.png";
						cell.PaintColorString = "&m";
					}
					if (num6 >= num - 2)
					{
						cell.RequireObject(Blueprint);
					}
				}
				else if (num6 < 3)
				{
					if (FullClear)
					{
						cell.Clear();
					}
					else
					{
						cell.ClearWalls();
					}
				}
			}
		}
	}
}
