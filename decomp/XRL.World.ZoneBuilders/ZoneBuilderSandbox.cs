using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConsoleLib.Console;
using Genkit;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.EditorFormats.Map;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI.Pathfinding;
using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class ZoneBuilderSandbox
{
	public enum BridgeDirection
	{
		Horizontal,
		Vertical,
		Random
	}

	public enum PlacePrefabAlign
	{
		NW,
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		Center
	}

	protected Zone zone;

	protected static List<Location2D> placementCells = new List<Location2D>();

	protected static List<Location2D> Points = new List<Location2D>();

	private FastNoise simplexFastNoise;

	public static FastNoise pathNoise = new FastNoise();

	[NonSerialized]
	public Dictionary<string, List<ColorOutputMap>> templates;

	public static int currentpop = 0;

	private static Dictionary<string, List<Location2D>> ObjectPlacementCache = new Dictionary<string, List<Location2D>>();

	private static List<List<Location2D>> hintFields = new List<List<Location2D>>();

	private static Dictionary<Location2D, int> hintFieldtotals = new Dictionary<Location2D, int>();

	private static List<Location2D> workingSet = new List<Location2D>();

	private static Dictionary<string, MethodInfo> templateApplicators = new Dictionary<string, MethodInfo>();

	protected static string PopulationOr(string Population, string Default)
	{
		if (!PopulationManager.HasPopulation(Population))
		{
			return Default;
		}
		return Population;
	}

	public Location2D getMatchedEdgeConnectionLocation(Zone zone, string label, string dir, int margin = 3)
	{
		dir = dir.ToLower();
		string text = "";
		if (dir == "n")
		{
			text = "ns";
		}
		if (dir == "s")
		{
			text = "ns";
		}
		if (dir == "e")
		{
			text = "ew";
		}
		if (dir == "w")
		{
			text = "ew";
		}
		if (dir == "n")
		{
			label = label + (zone.Y - 1) + zone.X;
		}
		if (dir == "s")
		{
			label = label + zone.Y + zone.X;
		}
		if (dir == "e")
		{
			label = label + zone.Y + zone.X;
		}
		if (dir == "w")
		{
			label = label + zone.Y + (zone.X - 1);
		}
		int worldSeed = XRLCore.Core.Game.GetWorldSeed(label + text);
		return dir switch
		{
			"n" => Location2D.Get(GetSeededRange(worldSeed, margin, 79 - margin), 0), 
			"s" => Location2D.Get(GetSeededRange(worldSeed, margin, 79 - margin), 24), 
			"e" => Location2D.Get(79, GetSeededRange(worldSeed, margin, 24 - margin)), 
			"w" => Location2D.Get(0, GetSeededRange(worldSeed, margin, 24 - margin)), 
			_ => Location2D.Get(0, 0), 
		};
	}

	public double sampleSimplexNoise(string seedid, int x, int y, int z, float low, float high, float frequency = 0.1f, int octaves = 5, float lacunarity = 1.1f, float gain = 1f)
	{
		if (simplexFastNoise == null)
		{
			simplexFastNoise = new FastNoise();
			simplexFastNoise.SetSeed(GetSeedValue(seedid));
			simplexFastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
			simplexFastNoise.SetFrequency(frequency);
			simplexFastNoise.SetFractalType(FastNoise.FractalType.FBM);
			simplexFastNoise.SetFractalOctaves(octaves);
			simplexFastNoise.SetFractalLacunarity(lacunarity);
			simplexFastNoise.SetFractalGain(gain);
		}
		return (simplexFastNoise.GetNoise(x, y, z) + 1f) / 2f * (high - low) + low;
	}

	public void EnsureCellReachable(Zone zone, Cell cell)
	{
		List<Cell> list = (from c in zone.GetCells()
			where c.IsReachable()
			select c).ToList();
		list.Sort((Cell a, Cell b) => a.PathDistanceTo(cell).CompareTo(b.PathDistanceTo(cell)));
		Grid<int> grid = new Grid<int>(zone.Width, zone.Height);
		grid.regionalize((int x, int y, int c) =>
		{
			if (zone.GetCell(x, y).HasWall())
			{
				return 80;
			}
			return zone.GetCell(x, y).HasObject("InfluenceMapBlocker") ? 80 : 0;
		});
		using Pathfinder pathfinder = new Pathfinder(240, 75);
		pathfinder.setWeightsFromGrid(grid, (int x, int y, int c) =>
		{
			if (zone.GetCell(x, y).HasWall())
			{
				return 80;
			}
			return zone.GetCell(x, y).HasObject("InfluenceMapBlocker") ? 80 : 0;
		});
		if (!pathfinder.FindPath(cell.Location, list[0].Location, Display: false, CardinalDirectionsOnly: true, 24300))
		{
			return;
		}
		foreach (PathfinderNode step in pathfinder.Steps)
		{
			zone.GetCell(step.X, step.Y)?.ClearWalls();
		}
	}

	public static void TunnelTo(Zone zone, Location2D start, Location2D end, bool pathWithNoise = true, float pathWindyness = 0.2f, int emptyBase = 0, Action<Cell> action = null, Func<int, int, int, int> weightFunc = null)
	{
		using Pathfinder pathfinder = zone.getPathfinder();
		if (pathWithNoise)
		{
			pathNoise.SetSeed(Stat.Random(int.MinValue, int.MaxValue));
			pathNoise.SetNoiseType(FastNoise.NoiseType.Simplex);
			pathNoise.SetFractalOctaves(2);
			pathNoise.SetFrequency(pathWindyness);
		}
		Grid<int> grid = new Grid<int>(zone.Width, zone.Height);
		zone.GetCells().ForEach((Cell c) =>
		{
			if (c.HasWall())
			{
				grid.set(c.X, c.Y, 80);
			}
		});
		if (weightFunc == null)
		{
			weightFunc = (int x, int y, int c) =>
			{
				int num = 0;
				if (x == 0)
				{
					return 2000;
				}
				if (y == 0)
				{
					return 2000;
				}
				if (x == zone.Width - 1)
				{
					return 2000;
				}
				if (y == zone.Height - 1)
				{
					return 2000;
				}
				if (pathWithNoise)
				{
					num = (int)(Math.Abs(pathNoise.GetNoise((x + zone.wX * 80) / 3, y + zone.wY * 25)) * 160f);
				}
				if (zone.GetCell(x, y).HasWall())
				{
					return 80 + num;
				}
				return zone.GetCell(x, y).HasObject("InfluenceMapBlocker") ? (80 + num) : (num + emptyBase);
			};
		}
		pathfinder.setWeightsFromGrid(grid, weightFunc);
		if (!pathfinder.FindPath(start, end, Display: false, CardinalDirectionsOnly: true, 24300, ShuffleDirections: true))
		{
			return;
		}
		foreach (PathfinderNode step in pathfinder.Steps)
		{
			Cell cell = zone.GetCell(step.X, step.Y);
			if (cell != null)
			{
				if (action == null)
				{
					cell.ClearWalls();
				}
				else
				{
					action(cell);
				}
			}
		}
	}

	public static void EnsureAllVoidsConnected(Zone zone, bool pathWithNoise = false, float pathWindyness = 0.2f, Action<GameObject> ClearAction = null)
	{
		Grid<int> grid = new Grid<int>(zone.Width, zone.Height);
		InfluenceMap regions = grid.regionalize((int x, int y, int c) =>
		{
			if (zone.GetCell(x, y).HasWall())
			{
				return 80;
			}
			if (zone.GetCell(x, y).HasObjectWithTag("EnsureVoidBlocker"))
			{
				return 80;
			}
			if (zone.GetCell(x, y).HasObjectWithTag("InfluenceMapBlocker"))
			{
				return 80;
			}
			return zone.GetCell(x, y).HasObject((GameObject o) => o.Physics != null && o.Physics.Solid && !o.HasPart<Door>()) ? 80 : 0;
		});
		List<InfluenceMapRegion> list = new List<InfluenceMapRegion>();
		using Pathfinder pathfinder = zone.getPathfinder();
		regions.Regions.Sort((InfluenceMapRegion a, InfluenceMapRegion b) => a.Center.SquareDistance(regions.Regions.First().Center).CompareTo(b.Center.SquareDistance(regions.Regions.First().Center)));
		if (pathWithNoise)
		{
			pathNoise.SetSeed(Stat.Random(int.MinValue, int.MaxValue));
			pathNoise.SetNoiseType(FastNoise.NoiseType.Simplex);
			pathNoise.SetFractalOctaves(2);
			pathNoise.SetFrequency(pathWindyness);
		}
		foreach (InfluenceMapRegion region in regions.Regions)
		{
			if (list.Count > 0)
			{
				list.Sort((InfluenceMapRegion a, InfluenceMapRegion b) => a.Center.SquareDistance(region.Center).CompareTo(b.Center.SquareDistance(region.Center)));
				InfluenceMapRegion influenceMapRegion = list.First();
				Location2D randomElement = region.Cells.GetRandomElement();
				Location2D randomElement2 = influenceMapRegion.Cells.GetRandomElement();
				pathfinder.setWeightsFromGrid(grid, (int x, int y, int c) =>
				{
					int num = 0;
					if (x == 0)
					{
						return 2000;
					}
					if (y == 0)
					{
						return 2000;
					}
					if (x == zone.Width - 1)
					{
						return 2000;
					}
					if (y == zone.Height - 1)
					{
						return 2000;
					}
					if (pathWithNoise)
					{
						num = (int)(Math.Abs(pathNoise.GetNoise((x + zone.wX * 80) / 3, y + zone.wY * 25)) * 160f);
					}
					if (zone.GetCell(x, y).HasWall())
					{
						return 80 + num;
					}
					if (zone.GetCell(x, y).HasObject("InfluenceMapBlocker"))
					{
						return 1000 + num;
					}
					if (zone.GetCell(x, y).HasObject("EnsureVoidBlocker"))
					{
						return 1000 + num;
					}
					return zone.GetCell(x, y).HasObject((GameObject o) => o.Physics != null && o.Physics.Solid) ? 1000 : num;
				});
				if (!pathfinder.FindPath(randomElement, randomElement2, Display: false, CardinalDirectionsOnly: true, 24300, ShuffleDirections: true))
				{
					continue;
				}
				foreach (PathfinderNode step in pathfinder.Steps)
				{
					Cell cell = zone.GetCell(step.X, step.Y);
					if (cell == null)
					{
						continue;
					}
					cell.ClearWalls();
					if (!cell.HasObject((GameObject o) => o.Physics != null && o.Physics.Solid))
					{
						continue;
					}
					GameObject firstObject = cell.GetFirstObject((GameObject o) => o.Physics != null && o.Physics.Solid);
					if (!firstObject.IsImportant())
					{
						if (ClearAction != null)
						{
							ClearAction(firstObject);
						}
						else
						{
							firstObject.Obliterate();
						}
					}
				}
				list.Add(region);
			}
			else
			{
				list.Add(region);
			}
		}
		regions.RecalculateCostOnly();
	}

	public List<Location2D> CreateConveyor(Location2D start, Location2D end, Zone zone, Func<int, int, int> getExtraWeight = null, string forceFinalDirection = null, string padObject = "ConveyorPad")
	{
		Pathfinder pathfinder = zone.getPathfinder((int x, int y, Cell cell2) =>
		{
			int num3 = ((getExtraWeight != null) ? getExtraWeight(x, y) : 0);
			if (cell2.HasObject("ConveyorPad") || cell2.HasObject(padObject) || cell2.HasObject("ConveyorDrive"))
			{
				return int.MaxValue;
			}
			return cell2.HasWall() ? (40 + num3) : num3;
		});
		try
		{
			int num = 10;
			while (true)
			{
				pathfinder.FindPath(start, end, Display: false, CardinalDirectionsOnly: true, 9999, ShuffleDirections: true);
				if (!pathfinder.Steps.Any((PathfinderNode s) => pathfinder.Steps.Where((PathfinderNode t) => t == s).Count() > 1) || num <= 0)
				{
					break;
				}
				num--;
				MetricsManager.LogWarning("overlapping conveyor path detected");
			}
			for (int num2 = 0; num2 < pathfinder.Steps.Count; num2++)
			{
				Cell cell = zone.GetCell(pathfinder.Steps[num2].pos);
				cell.ClearWalls();
				GameObject gameObject = GameObject.Create(padObject);
				ConveyorPad part = gameObject.GetPart<ConveyorPad>();
				if (part != null)
				{
					ref string connections = ref part.Connections;
					ref string direction = ref part.Direction;
					string text;
					if (num2 < pathfinder.Steps.Count - 1)
					{
						text = pathfinder.Directions[num2];
					}
					else
					{
						text = ((forceFinalDirection == null) ? pathfinder.Directions[num2 - 1] : forceFinalDirection);
					}
					connections = (direction = text);
					if (num2 > 0)
					{
						part.Connections += Directions.GetOppositeDirection(pathfinder.Directions[num2 - 1]);
					}
				}
				else
				{
					MetricsManager.LogError("built " + gameObject.DebugName + " for conveyer belt but it has no ConveyorPad");
				}
				cell.AddObject(gameObject);
			}
			return new List<Location2D>(pathfinder.Steps.Select((PathfinderNode s) => s.pos));
		}
		finally
		{
			if (pathfinder != null)
			{
				((IDisposable)pathfinder).Dispose();
			}
		}
	}

	public List<ColorOutputMap> getWfcBuildingTemplate(string template)
	{
		if (templates == null)
		{
			templates = new Dictionary<string, List<ColorOutputMap>>();
		}
		if (!templates.ContainsKey(template))
		{
			ColorOutputMap colorOutputMap = new ColorOutputMap(80, 25);
			WaveCollapseFastModel waveCollapseFastModel = new WaveCollapseFastModel(template, 3, 78, 23, periodicInput: true, periodicOutput: false, 8, 0);
			waveCollapseFastModel.Run(Stat.Random(int.MinValue, 2147483646), 0);
			ColorOutputMap colorOutputMap2 = new ColorOutputMap(waveCollapseFastModel);
			colorOutputMap2.ReplaceBorders(new Color32(byte.MaxValue, 0, 0, byte.MaxValue), new Color32(0, 0, 0, byte.MaxValue));
			colorOutputMap.Paste(colorOutputMap2, 1, 1);
			List<ColorOutputMap> list = colorOutputMap.CarveSubmaps(new List<Color32>
			{
				ColorOutputMap.BLACK,
				ColorOutputMap.RED
			});
			list.RemoveAll((ColorOutputMap o) => o.extrawidth < 4 || o.extraheight < 4 || o.extrawidth * o.extraheight <= 16 || o.CountColor(ColorOutputMap.BLACK) < 7);
			templates.Add(template, list);
			colorOutputMap2 = null;
			list = null;
			MemoryHelper.GCCollect();
		}
		return templates[template];
	}

	public string GetDefaultWall(Zone Z)
	{
		return Z.GetDefaultWall();
	}

	public GameObject GetTerrainObject(Zone Z)
	{
		return Z.GetTerrainObject();
	}

	public string GetTerrain(Zone Z)
	{
		string[] array = Z.ZoneID.Split('.');
		int wx = Convert.ToInt32(array[1]);
		int wy = Convert.ToInt32(array[2]);
		return ZoneManager.GetObjectTypeForZone(wx, wy, "JoppaWorld");
	}

	public int GetSeedValue(string id)
	{
		return XRLCore.Core.Game.GetWorldSeed(id);
	}

	public static int GetOracleIntFromString(string s, int Low, int High)
	{
		return new System.Random(Hash.String(s)).Next(Low, High);
	}

	public T GetOracleListEntryFromInt<T>(int i, List<T> list)
	{
		return list[GetOracleIntFromString(i.ToString(), 0, list.Count - 1)];
	}

	public int GetOracleIntZone(Zone Z, int Low, int High)
	{
		return new System.Random(Hash.String(Z.ZoneID)).Next(Low, High);
	}

	public static List<T> GetOracleNFromList<T>(List<T> list, int n, string key)
	{
		System.Random r = new System.Random(Hash.String(key + XRLCore.Core.Game.GetWorldSeed(key)));
		List<T> list2 = new List<T>();
		for (int i = 0; i < n; i++)
		{
			list2.Add(list.RemoveRandomElement(r));
		}
		return list2;
	}

	public static Location2D GetOracleLocationForZone(Zone Z, string seed, int ZOffset = 0, Predicate<Location2D> filter = null)
	{
		return GetOracleLocation(Z.wX + "." + Z.wY + "." + Z.X + "." + Z.Y + "." + (Z.Z + ZOffset) + seed, filter);
	}

	public static Location2D GetOracleLocation(string seed, Predicate<Location2D> filter = null)
	{
		int num = 0;
		Location2D location2D;
		do
		{
			location2D = Location2D.Get(GetOracleIntFromString(seed + num, 0, 80), GetOracleIntFromString(seed + num, 0, 24));
			num++;
		}
		while (filter != null && !filter(location2D));
		return location2D;
	}

	public bool HasZoneColumnValue(Zone Z, string key)
	{
		return The.Game.ZoneManager.HasZoneColumnProperty(Z.ZoneID, key);
	}

	public string GetZoneColumnValue(Zone Z, string key, string defaultValue = null)
	{
		return (string)The.Game.ZoneManager.GetZoneColumnProperty(Z.ZoneID, key, defaultValue);
	}

	public void SetZoneColumnValue(Zone Z, string key, object value)
	{
		The.Game.ZoneManager.SetZoneColumnProperty(Z.ZoneID, key, value);
	}

	public static void BridgeOver(Zone Z, Location2D start, BridgeDirection dir = BridgeDirection.Random, string OverObject = "Pit")
	{
		if (dir == BridgeDirection.Random)
		{
			dir = ((Stat.Random(0, 1) != 0) ? BridgeDirection.Vertical : BridgeDirection.Horizontal);
		}
		if (dir == BridgeDirection.Horizontal)
		{
			HBridgeOver(Z, start, OverObject);
		}
		if (dir == BridgeDirection.Vertical)
		{
			VBridgeOver(Z, start, OverObject);
		}
	}

	public static void VBridgeOver(Zone Z, Location2D start, string OverObject = "Pit")
	{
		int num = start.Y;
		while (num > 0 && Z.GetCell(start.X, num).HasObject(OverObject))
		{
			Z.GetCell(start.X, num).RequireObject("Bridge");
			num--;
		}
		for (int i = start.Y; i < Z.Height && Z.GetCell(start.X, i).HasObject(OverObject); i++)
		{
			Z.GetCell(start.X, i).RequireObject("Bridge");
		}
	}

	public static void HBridgeOver(Zone Z, Location2D start, string OverObject = "Pit")
	{
		int num = start.X;
		while (num > 0 && Z.GetCell(num, start.Y).HasObject(OverObject))
		{
			Z.GetCell(num, start.Y).RequireObject("Bridge");
			num--;
		}
		for (int i = start.X; i < Z.Width && Z.GetCell(i, start.Y).HasObject(OverObject); i++)
		{
			Z.GetCell(i, start.Y).RequireObject("Bridge");
		}
	}

	public static int GetOracleIntColumn(Zone Z, string seed, int Low, int High)
	{
		return GetOracleIntColumn(Z.wX, Z.wY, Z.X, Z.Y, seed, Low, High);
	}

	public static int GetOracleIntColumn(Zone Z, int Low, int High)
	{
		return GetOracleIntColumn(Z.wX, Z.wY, Z.X, Z.Y, null, Low, High);
	}

	public static int GetOracleIntColumn(int ParasangX, int ParasangY, int ZoneX, int ZoneY, int Low, int High)
	{
		return GetOracleIntColumn(ParasangX, ParasangY, ZoneX, ZoneY, null, Low, High);
	}

	public static int GetOracleIntColumn(int ParasangX, int ParasangY, int ZoneX, int ZoneY, string Seed, int Low, int High)
	{
		Span<int> span = stackalloc int[4];
		span[0] = ParasangX;
		span[1] = ParasangY;
		span[2] = ZoneX;
		span[3] = ZoneY;
		uint hash = Hash.FNV1A32(span);
		if (Seed != null)
		{
			hash = Hash.FNV1A32(Seed, hash);
		}
		return new System.Random((int)Hash.FNV1A32(The.Game?.GetWorldSeed() ?? 0, hash)).Next(Low, High);
	}

	public System.Random GetSeededRand(string val)
	{
		return new System.Random(Hash.String(val + XRLCore.Core.Game.GetWorldSeed(val)));
	}

	public int GetSeededRange(string val, int Low = int.MinValue, int High = int.MaxValue)
	{
		return GetSeededRange(Hash.String(val + XRLCore.Core.Game.GetWorldSeed(val)), Low, High);
	}

	public int GetSeededRange(int Seed, int Low = int.MinValue, int High = int.MaxValue)
	{
		return new System.Random(Seed).Next(Low, High);
	}

	public IEnumerable<Location2D> YieldGaussianCellCluster(float XP, float YP, float StdDeviationX = 3f, float StdDeviationY = 5f)
	{
		while (true)
		{
			int num = (int)Stat.GaussianRandom(XP, StdDeviationX);
			int num2 = (int)Stat.GaussianRandom(YP, StdDeviationY);
			if (num < 0)
			{
				num = 0;
			}
			if (num > 79)
			{
				num = 79;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num2 > 24)
			{
				num2 = 24;
			}
			yield return Location2D.Get(num, num2);
		}
	}

	public Cell GetCell(Zone Z, Location2D P)
	{
		return Z.GetCell(P.X, P.Y);
	}

	public Cell GetCell(Zone Z, Point2D P)
	{
		return Z.GetCell(P.x, P.y);
	}

	public void PlaceHut(Zone Z, Rect2D R, string Floor = "DirtPath", string Wall = "Shale", string Table = null, bool Round = false)
	{
		Rect2D r = R.ReduceBy(1, 1);
		ClearRect(Z, r);
		for (int i = r.y1; i <= r.y2; i++)
		{
			for (int j = r.x1; j <= r.x2; j++)
			{
				Z.GetCell(j, i).AddObject(Floor);
			}
		}
		if (Round)
		{
			for (int k = R.x1 + 1; k <= R.x2 - 1; k++)
			{
				Z.GetCell(k, R.y1).ClearAndAddObject(Wall);
				Z.GetCell(k, R.y2).ClearAndAddObject(Wall);
			}
			for (int l = R.y1 + 1; l <= R.y2 - 1; l++)
			{
				Z.GetCell(R.x1, l).ClearAndAddObject(Wall);
				Z.GetCell(R.x2, l).ClearAndAddObject(Wall);
			}
			Z.GetCell(R.x1 + 1, R.y1 + 1).ClearAndAddObject(Wall);
			Z.GetCell(R.x2 - 1, R.y1 + 1).ClearAndAddObject(Wall);
			Z.GetCell(R.x1 + 1, R.y2 - 1).ClearAndAddObject(Wall);
			Z.GetCell(R.x2 - 1, R.y2 - 1).ClearAndAddObject(Wall);
		}
		else
		{
			for (int m = R.x1; m <= R.x2; m++)
			{
				Z.GetCell(m, R.y1).ClearAndAddObject(Wall);
				Z.GetCell(m, R.y2).ClearAndAddObject(Wall);
			}
			for (int n = R.y1; n <= R.y2; n++)
			{
				Z.GetCell(R.x1, n).ClearAndAddObject(Wall);
				Z.GetCell(R.x2, n).ClearAndAddObject(Wall);
			}
		}
		Z.GetCell(R.Door)?.ClearWalls();
		foreach (PopulationResult item in PopulationManager.Generate(Table, "zonetier", Z.NewTier.ToString()))
		{
			for (int num = 0; num < item.Number; num++)
			{
				PlaceObjectInRect(Z, r, item.Blueprint, item.Hint);
			}
		}
	}

	public void PaintZone(Zone Z, string PaintTile, string PaintRender, string PaintColor, string PaintTileColor, string PaintDetail, bool bForce = false)
	{
		foreach (Cell cell in Z.GetCells())
		{
			if (bForce || string.IsNullOrEmpty(cell.PaintTile))
			{
				cell.PaintTile = PaintTile;
				cell.PaintColorString = PaintColor;
				cell.PaintTileColor = PaintTileColor;
				cell.PaintDetailColor = PaintDetail;
				cell.PaintRenderString = PaintRender;
			}
		}
	}

	public List<Location2D> BuildPathWithObject(Zone zone, List<Location2D> Segments, string PaintObject, int Width, bool Noise, Func<int, int, int, int> weightFunc = null)
	{
		List<Location2D> list = new List<Location2D>();
		HashSet<Cell> hashSet = new HashSet<Cell>();
		using Pathfinder pathfinder = new Pathfinder(240, 75);
		Grid<int> grid = new Grid<int>(zone.Width, zone.Height);
		grid.regionalize((int x, int y, int c) =>
		{
			if (weightFunc != null)
			{
				return weightFunc(x, y, c);
			}
			if (zone.GetCell(x, y).HasWall())
			{
				return 80;
			}
			return zone.GetCell(x, y).HasObject("InfluenceMapBlocker") ? 80 : 0;
		});
		pathfinder.setWeightsFromGrid(grid, (int x, int y, int c) =>
		{
			if (weightFunc != null)
			{
				return weightFunc(x, y, c);
			}
			if (zone.GetCell(x, y).HasWall())
			{
				return 80;
			}
			return zone.GetCell(x, y).HasObject("InfluenceMapBlocker") ? 80 : 0;
		});
		for (int num = 0; num < Segments.Count - 1; num++)
		{
			pathfinder.FindPath(Segments[num], Segments[num + 1], Display: false, CardinalDirectionsOnly: true);
			if (!pathfinder.Found)
			{
				continue;
			}
			foreach (PathfinderNode step in pathfinder.Steps)
			{
				if (zone.GetCell(step.pos) == null)
				{
					continue;
				}
				foreach (Cell localAdjacentCell in zone.GetCell(step.pos).GetLocalAdjacentCells(Width, IncludeSelf: true))
				{
					if (!hashSet.Contains(localAdjacentCell))
					{
						hashSet.Add(localAdjacentCell);
						localAdjacentCell.AddObject(PaintObject);
						Location2D location = localAdjacentCell.Location;
						list.Add(location);
					}
				}
			}
		}
		return list;
	}

	public List<Location2D> BuildSimplePathWithObject(Zone Z, List<Location2D> Segments, string Blueprint, int Width, bool Noise)
	{
		List<Location2D> list = new List<Location2D>();
		GameObject looker = null;
		for (int i = 0; i < Segments.Count; i += 2)
		{
			FindPath findPath = new FindPath(Z, Segments[i].X, Segments[i].Y, Z, Segments[i + 1].X, Segments[i + 1].Y, PathGlobal: false, PathUnlimited: true, looker, Noise);
			if (!findPath.Usable)
			{
				continue;
			}
			foreach (Cell step in findPath.Steps)
			{
				foreach (Cell localAdjacentCell in step.GetLocalAdjacentCells())
				{
					if (!localAdjacentCell.HasObject(Blueprint))
					{
						localAdjacentCell.AddObject(Blueprint);
						Location2D location = localAdjacentCell.Location;
						if (!list.Contains(location))
						{
							list.Add(location);
						}
					}
				}
			}
		}
		return list;
	}

	public List<Location2D> BuildPath(Zone Z, List<Location2D> Segments, string PaintTile, string PaintRender, string PaintColor, string PaintDetail, int Width, bool Noise)
	{
		List<Location2D> list = new List<Location2D>();
		GameObject looker = null;
		for (int i = 0; i < Segments.Count; i += 2)
		{
			FindPath findPath = new FindPath(Z, Segments[i].X, Segments[i].Y, Z, Segments[i + 1].X, Segments[i + 1].Y, PathGlobal: false, PathUnlimited: true, looker, Noise);
			if (!findPath.Usable)
			{
				continue;
			}
			foreach (Cell step in findPath.Steps)
			{
				foreach (Cell localAdjacentCell in step.GetLocalAdjacentCells())
				{
					localAdjacentCell.PaintTile = PaintTile;
					localAdjacentCell.PaintColorString = PaintColor;
					localAdjacentCell.PaintDetailColor = PaintDetail;
					localAdjacentCell.PaintRenderString = PaintRender;
					Location2D location = localAdjacentCell.Location;
					if (!list.Contains(location))
					{
						list.Add(location);
					}
				}
			}
		}
		return list;
	}

	public static void PlacePopulationInCells(Zone Z, List<Location2D> Cells, string Table)
	{
		List<Location2D> list = new List<Location2D>(Cells);
		Algorithms.RandomShuffleInPlace(list, Stat.Rand);
		List<PopulationResult> list2 = PopulationManager.Generate(Table, "zonetier", Z.NewTier.ToString());
		for (int i = 0; i < list2.Count; i++)
		{
			for (int j = 0; j < list2[i].Number; j++)
			{
				if (list.Count == 0)
				{
					return;
				}
				Cell cell = Z.GetCell(list[list.Count - 1]);
				if (!cell.HasObject(list2[i].Blueprint))
				{
					cell.AddObject(list2[i].Blueprint);
				}
				list.RemoveAt(list.Count - 1);
			}
		}
	}

	public static void PlacePopulationInRegion(Zone Z, IEnumerable<Location2D> R, string Table, string DefaultHint = null, Func<string, GameObject> customFabricator = null)
	{
		PlacePopulationInRegion(Z, new LocationList(R), Table, DefaultHint, customFabricator);
	}

	public static void PlacePopulationInRegion(Zone Z, IEnumerable<Cell> R, string Table, string DefaultHint = null, Func<string, GameObject> customFabricator = null)
	{
		PlacePopulationInRegion(Z, new LocationList(R.Select((Cell c) => c.Location)), Table, DefaultHint, customFabricator);
	}

	public static void PlacePopulationInRegion(Zone Z, ILocationArea R, string Table, string DefaultHint = null, Func<string, GameObject> customFabricator = null)
	{
		List<PopulationResult> list = PopulationManager.Generate(Table, "zonetier", Z.NewTier.ToString());
		ObjectPlacementCache.Clear();
		currentpop++;
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].Number; j++)
			{
				if (list[i].Blueprint == "*Fracti")
				{
					string blueprint;
					if (Stat.Random(0, 2) != 0)
					{
						blueprint = ((Stat.Random(0, 2) != 1) ? "Fracti" : "Fracti3");
					}
					else
					{
						blueprint = "Fracti2";
					}
					CellularGrid cellularGrid = new CellularGrid(Stat.Rand, 1, 80, 25);
					cellularGrid.BornList = new int[4] { 2, 4, 5, 6 };
					cellularGrid.SurviveList = new int[6] { 2, 3, 4, 5, 7, 8 };
					cellularGrid.Passes = 8;
					cellularGrid.SeedChance = 3;
					cellularGrid.SeedBorders = false;
					cellularGrid.BorderDepth = 0;
					cellularGrid.Generate(Stat.Rand, Z.Width, Z.Height);
					for (int k = 1; k < Z.Width - 1; k++)
					{
						for (int l = 1; l < Z.Height - 1; l++)
						{
							if (cellularGrid.cells[k, l] == 1 && R.EnumerateNonBorderLocations().Contains(Location2D.Get(k, l)) && !Z.GetCell(Location2D.Get(k, l)).HasObjectWithTag("NoFractus"))
							{
								Z.GetCell(Location2D.Get(k, l)).AddObject(blueprint);
							}
						}
					}
				}
				else
				{
					PlaceObjectInArea(Z, R, ApplyHintsToObject((customFabricator == null) ? GameObject.Create(list[i].Blueprint) : customFabricator(list[i].Blueprint), list[i].Hint, i), i, j, string.IsNullOrEmpty(list[i].Hint) ? DefaultHint : list[i].Hint, list[i].Builder, bAllowCaching: true);
				}
			}
		}
	}

	public static void encloseRectWithWall(Zone zone, Rect2D rect, string wallObject)
	{
		for (int i = rect.x1; i <= rect.x2; i++)
		{
			if (zone.GetCell(i, rect.y1) != null)
			{
				zone.GetCell(i, rect.y1).ClearWalls();
			}
			if (zone.GetCell(i, rect.y1) != null)
			{
				zone.GetCell(i, rect.y1).AddObject(wallObject);
			}
			if (zone.GetCell(i, rect.y2) != null)
			{
				zone.GetCell(i, rect.y2).ClearWalls();
			}
			if (zone.GetCell(i, rect.y2) != null)
			{
				zone.GetCell(i, rect.y2).AddObject(wallObject);
			}
		}
		for (int j = rect.y1 + 1; j <= rect.y2 - 1; j++)
		{
			if (zone.GetCell(rect.x1, j) != null)
			{
				zone.GetCell(rect.x1, j).ClearWalls();
			}
			if (zone.GetCell(rect.x1, j) != null)
			{
				zone.GetCell(rect.x1, j).AddObject(wallObject);
			}
			if (zone.GetCell(rect.x2, j) != null)
			{
				zone.GetCell(rect.x2, j).ClearWalls();
			}
			if (zone.GetCell(rect.x2, j) != null)
			{
				zone.GetCell(rect.x2, j).AddObject(wallObject);
			}
		}
	}

	public static GameObject PlaceObjectOnFloor(Zone Z, string Object, string OkFloor)
	{
		List<Cell> cellsWithFloor = Z.GetCellsWithFloor(OkFloor);
		Algorithms.RandomShuffleInPlace(cellsWithFloor, Stat.Rand);
		foreach (Cell item in cellsWithFloor)
		{
			if (item.IsEmpty() && !item.HasObject(Object))
			{
				return item.AddObject(Object);
			}
		}
		return null;
	}

	public static void PlacePopulationInRect(Zone Z, Rect2D R, string Table)
	{
		List<PopulationResult> list = PopulationManager.Generate(Table, "zonetier", Z.NewTier.ToString());
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].Number; j++)
			{
				PlaceObjectInRect(Z, R, list[i].Blueprint, list[i].Hint, i);
			}
		}
	}

	public static void PlacePopulationInRect(Zone Z, Rect2D R, string Table, Action<GameObject> BeforeObjectCreated)
	{
		List<PopulationResult> list = PopulationManager.Generate(Table, "zonetier", Z.NewTier.ToString());
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].Number; j++)
			{
				PlaceObjectInRect(Z, R, list[i].Blueprint, list[i].Hint, BeforeObjectCreated);
			}
		}
	}

	public static void PlacePopulationInRect(Zone Z, Rect2D R, Rect2D Exclude, string Table)
	{
		List<PopulationResult> list = PopulationManager.Generate(Table, "zonetier", Z.NewTier.ToString());
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].Number; j++)
			{
				PlaceObjectInRect(Z, R, Exclude, list[i].Blueprint, list[i].Hint);
			}
		}
	}

	public static void PlacePopulationInRect(Zone Z, Rect2D R, Rect2D Exclude, string Table, Action<GameObject> BeforeObjectCreated)
	{
		List<PopulationResult> list = PopulationManager.Generate(Table, "zonetier", Z.NewTier.ToString());
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].Number; j++)
			{
				PlaceObjectInRect(Z, R, Exclude, list[i].Blueprint, list[i].Hint, BeforeObjectCreated);
			}
		}
	}

	public static void PlacePopulationInRegionRect(Zone Z, ILocationArea Region, Rect2D Rect, string FrontSide, string Table, string Owner = null, List<Location2D> AvoidPoints = null, Func<string, GameObject> customFabricator = null)
	{
		List<PopulationResult> list = PopulationManager.Generate(Table, "zonetier", Z.NewTier.ToString());
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].Number; j++)
			{
				PlaceObjectInRegionRect(Z, Region, Rect, FrontSide, list[i].Blueprint, list[i].Hint, Owner, AvoidPoints, null, customFabricator);
			}
		}
	}

	public static void AddPointWithHint(List<Location2D> L, Location2D P, Rect2D R, string Hint, List<Location2D> AvoidPoints = null)
	{
		if (!R.IsPointAdjacentToDoor(P) && (AvoidPoints == null || !R.IsPointAdjacentToAvoid(P, AvoidPoints)))
		{
			L.Add(P);
		}
	}

	public static void AddPointWithHint(List<Location2D> L, Location2D P, List<Location2D> Avoid, Rect2D R, string Hint)
	{
		if (!R.IsPointAdjacentToAvoid(P, Avoid))
		{
			L.Add(P);
		}
	}

	public static bool PlaceObjectInRegionRect(Zone Z, ILocationArea Region, Rect2D R, string FrontSide, string Object, string Hint = null, string Owner = null, List<Location2D> AvoidPoints = null, GameObject precreated = null, Func<string, GameObject> customFabricator = null)
	{
		Points.Clear();
		if (string.IsNullOrEmpty(Hint))
		{
			if (precreated != null)
			{
				Hint = precreated.GetPropertyOrTag("PlacementHint", "");
			}
			else
			{
				GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(Object);
				Hint = ((blueprint != null) ? blueprint.GetTag("PlacementHint") : "");
			}
		}
		if (Hint.StartsWith("OutsideDoor:"))
		{
			int num = Convert.ToInt32(Hint.Split(':')[1]);
			Rect2D rect2D = R.ReduceBy(-2, -2);
			if (FrontSide == "N")
			{
				Points.Add(Location2D.Get(R.Door.x - num, R.Door.y - 1));
				Points.Add(Location2D.Get(R.Door.x + num, R.Door.y - 1));
			}
			if (FrontSide == "S")
			{
				Points.Add(Location2D.Get(R.Door.x - num, R.Door.y + 1));
				Points.Add(Location2D.Get(R.Door.x + num, R.Door.y + 1));
			}
			if (FrontSide == "E")
			{
				Points.Add(Location2D.Get(rect2D.Door.x + 1, rect2D.Door.y - num));
				Points.Add(Location2D.Get(rect2D.Door.x + 1, rect2D.Door.y + num));
			}
			if (FrontSide == "W")
			{
				Points.Add(Location2D.Get(rect2D.Door.x - 1, rect2D.Door.y - num));
				Points.Add(Location2D.Get(rect2D.Door.x - 1, rect2D.Door.y + num));
			}
		}
		else
		{
			switch (Hint)
			{
			case "AlongWall":
				foreach (Location2D item in Region.EnumerateLocations())
				{
					if (Z.GetCell(item).GetLocalAdjacentCells().Any((Cell c) => c.HasWall()))
					{
						AddPointWithHint(Points, item, R, Hint, AvoidPoints);
					}
				}
				break;
			case "InsideCorner":
				if (FrontSide != "N" || FrontSide != "W")
				{
					AddPointWithHint(Points, Location2D.Get(R.x1, R.y1), R, Hint, AvoidPoints);
				}
				if (FrontSide != "S" || FrontSide != "W")
				{
					AddPointWithHint(Points, Location2D.Get(R.x1, R.y2), R, Hint, AvoidPoints);
				}
				if (FrontSide != "N" || FrontSide != "E")
				{
					AddPointWithHint(Points, Location2D.Get(R.x2, R.y1), R, Hint, AvoidPoints);
				}
				if (FrontSide != "S" || FrontSide != "E")
				{
					AddPointWithHint(Points, Location2D.Get(R.x2, R.y2), R, Hint, AvoidPoints);
				}
				break;
			case "AlongOutsideFrontWall":
			{
				Rect2D r2 = R.ReduceBy(-2, -2);
				for (int num4 = r2.x1 + 1; num4 <= r2.x2 - 1; num4++)
				{
					if (FrontSide == "N")
					{
						AddPointWithHint(Points, Location2D.Get(num4, r2.y1), r2, Hint, AvoidPoints);
					}
					if (FrontSide == "S")
					{
						AddPointWithHint(Points, Location2D.Get(num4, r2.y2), r2, Hint, AvoidPoints);
					}
				}
				for (int num5 = R.y1 + 1; num5 <= R.y2 - 1; num5++)
				{
					if (FrontSide == "W")
					{
						AddPointWithHint(Points, Location2D.Get(r2.x1, num5), r2, Hint, AvoidPoints);
					}
					if (FrontSide == "E")
					{
						AddPointWithHint(Points, Location2D.Get(r2.x2, num5), r2, Hint, AvoidPoints);
					}
				}
				break;
			}
			case "AlongOutsideWall":
			{
				Rect2D r = R.ReduceBy(-2, -2);
				for (int m = r.x1 + 1; m <= r.x2 - 1; m++)
				{
					if (FrontSide != "N")
					{
						AddPointWithHint(Points, Location2D.Get(m, r.y1), r, Hint, AvoidPoints);
					}
					if (FrontSide != "S")
					{
						AddPointWithHint(Points, Location2D.Get(m, r.y2), r, Hint, AvoidPoints);
					}
				}
				for (int n = r.y1 + 1; n <= r.y2 - 1; n++)
				{
					if (FrontSide != "W")
					{
						AddPointWithHint(Points, Location2D.Get(r.x1, n), r, Hint, AvoidPoints);
					}
					if (FrontSide != "E")
					{
						AddPointWithHint(Points, Location2D.Get(r.x2, n), r, Hint, AvoidPoints);
					}
				}
				break;
			}
			case "AlongInsideWall":
			{
				for (int num2 = R.x1; num2 <= R.x2; num2++)
				{
					if (FrontSide != "N")
					{
						AddPointWithHint(Points, Location2D.Get(num2, R.y1), R, Hint, AvoidPoints);
					}
					if (FrontSide != "S")
					{
						AddPointWithHint(Points, Location2D.Get(num2, R.y2), R, Hint, AvoidPoints);
					}
				}
				for (int num3 = R.y1; num3 <= R.y2; num3++)
				{
					if (FrontSide != "W")
					{
						AddPointWithHint(Points, Location2D.Get(R.x1, num3), R, Hint, AvoidPoints);
					}
					if (FrontSide != "E")
					{
						AddPointWithHint(Points, Location2D.Get(R.x2, num3), R, Hint, AvoidPoints);
					}
				}
				break;
			}
			case "Interior":
			case "Inside":
			{
				for (int k = R.y1 + 1; k <= R.y2 - 1; k++)
				{
					for (int l = R.x1 + 1; l <= R.x2 - 1; l++)
					{
						AddPointWithHint(Points, Location2D.Get(l, k), R, Hint, AvoidPoints);
					}
				}
				break;
			}
			default:
			{
				for (int i = R.y1; i <= R.y2; i++)
				{
					for (int j = R.x1; j <= R.x2; j++)
					{
						AddPointWithHint(Points, Location2D.Get(j, i), R, Hint, AvoidPoints);
					}
				}
				break;
			}
			}
		}
		Points.RemoveAll((Location2D p) => Z.GetCell(p).HasSpawnBlocker());
		if (Points.Count == 0)
		{
			for (int num6 = R.y1; num6 <= R.y2; num6++)
			{
				for (int num7 = R.x1; num7 <= R.x2; num7++)
				{
					AddPointWithHint(Points, Location2D.Get(num7, num6), R, Hint, AvoidPoints);
				}
			}
		}
		if (Points.Count == 0)
		{
			Zone z = Z;
			ILocationArea a = Region;
			GameObject gameObject = ((precreated != null) ? precreated : ((customFabricator == null) ? GameObject.Create(Object) : customFabricator(Object)));
			return PlaceObjectInArea(z, a, gameObject, 0, 0, Hint);
		}
		Algorithms.RandomShuffleInPlace(Points, Stat.Rand);
		for (int num8 = 0; num8 < Points.Count; num8++)
		{
			if (Z.GetCell(Points[num8]).Objects.Count == 0)
			{
				if (precreated != null)
				{
					Z.GetCell(Points[num8]).AddObject(precreated).Physics.Owner = Owner;
				}
				else
				{
					Z.GetCell(Points[num8]).AddObject(Object).Physics.Owner = Owner;
				}
				return true;
			}
		}
		return false;
	}

	public static void FilterPoints(string Hint, InfluenceMapRegion R, List<Location2D> Valid)
	{
	}

	public static void PlaceStructuredPopulationInRegion(Zone Z, InfluenceMapRegion R, PopulationStructuredResult P, List<Location2D> ValidPoints = null)
	{
		List<Location2D> list = new List<Location2D>();
		if (ValidPoints == null)
		{
			list.AddRange(R.Cells);
		}
		else
		{
			list.AddRange(ValidPoints);
		}
		FilterPoints(P.Hint, R, list);
	}

	public static GameObject ApplyHintsToObject(GameObject gameObject, string hints, int group = 0)
	{
		if (hints == null || hints == "" || gameObject == null)
		{
			return gameObject;
		}
		string[] array = hints.Split(',');
		foreach (string text in array)
		{
			if (!text.StartsWith("Template"))
			{
				continue;
			}
			string text2 = text.Replace("Template(", "");
			text2 = text2.Substring(0, text2.Length - 1);
			string[] array2 = text2.Split(' ');
			string text3 = "each";
			if (array2.Length >= 3)
			{
				text3 = array2[2].ToLower();
			}
			int num = 100;
			if (array2.Length >= 2)
			{
				num = Convert.ToInt32(array2[1]);
			}
			if ((!(text3 == "each") || Stat.Roll(1, 100) > num) && (!(text3 == "all") || GetOracleIntFromString("populationgroup" + group, 1, 100) > 100))
			{
				continue;
			}
			if (!templateApplicators.ContainsKey(array2[0]))
			{
				try
				{
					MethodInfo method = ModManager.ResolveType("XRL.World.Biomes." + array2[0]).GetMethod("Apply");
					templateApplicators.Add(array2[0], method);
				}
				catch (Exception)
				{
					Debug.LogError("Unresolved template type: " + array2[0]);
				}
			}
			templateApplicators[array2[0]].Invoke(null, new object[1] { gameObject });
		}
		return gameObject;
	}

	public static bool PlaceObjectInArea(Zone Z, ILocationArea A, GameObject gameObject, int group = 0, int groupordinal = 0, string Hints = null, string Builder = null, bool bAllowCaching = false)
	{
		if (gameObject == null)
		{
			MetricsManager.LogError("placing null object");
			return true;
		}
		gameObject = ApplyHintsToObject(gameObject, Hints, group);
		if (gameObject.HasTag("PlacementHint:AlwaysPlace00"))
		{
			Z.GetCell(0, 0).AddObject(gameObject);
			return true;
		}
		foreach (List<Location2D> hintField in hintFields)
		{
			hintField.Clear();
		}
		hintFieldtotals.Clear();
		workingSet.Clear();
		if (string.IsNullOrEmpty(Hints))
		{
			Hints = gameObject.GetPropertyOrTag("PlacementHint", "");
		}
		gameObject.SetIntProperty("_populationgroup", group);
		gameObject.SetIntProperty("_populationgroupordinal", groupordinal);
		int num = 0;
		bool flag = gameObject.IsSpawnBlocker();
		string[] array = Hints.Split(',');
		foreach (string text in array)
		{
			if (text == "")
			{
				continue;
			}
			if (Points == null)
			{
				Points = new List<Location2D>();
			}
			else
			{
				Points.Clear();
			}
			bool flag2 = false;
			if (bAllowCaching && ObjectPlacementCache.ContainsKey(text))
			{
				Points = ObjectPlacementCache[text];
				flag2 = Points.Count > 0;
			}
			if (!flag2)
			{
				bool flag3 = false;
				if (text.StartsWith("OnWall"))
				{
					foreach (Location2D item in A.EnumerateLocations())
					{
						if (Z.GetCell(item).HasExternalWall())
						{
							Points.Add(item);
						}
					}
					flag3 = true;
				}
				else if (text.StartsWith("Aquatic"))
				{
					foreach (Location2D item2 in A.EnumerateLocations())
					{
						if (Z.GetCell(item2).HasOpenLiquidVolume() && !Z.GetCell(item2).HasBridge())
						{
							Points.Add(item2);
						}
					}
					if (Points.Count == 0)
					{
						foreach (Cell item3 in from c in Z.GetCells()
							where c.HasOpenLiquidVolume() && !c.HasBridge()
							select c)
						{
							if (!Points.Contains(item3.Location))
							{
								Points.Add(item3.Location);
							}
						}
					}
					flag3 = true;
				}
				else if (text.StartsWith("AdjacentToBlueprint:"))
				{
					string blueprint = text.Split(':')[1];
					foreach (Location2D item4 in A.EnumerateLocations())
					{
						if (!Z.GetCell(item4).HasObjectWithBlueprint(blueprint))
						{
							continue;
						}
						foreach (Cell localCardinalAdjacentCell in Z.GetCell(item4).GetLocalCardinalAdjacentCells())
						{
							if (!localCardinalAdjacentCell.HasObjectWithBlueprint(blueprint) && !Points.Contains(localCardinalAdjacentCell.Location))
							{
								Points.Add(localCardinalAdjacentCell.Location);
							}
						}
					}
					bAllowCaching = false;
				}
				else if (text.StartsWith("StackWithStartsWith:"))
				{
					string blueprint2 = text.Split(':')[1];
					foreach (Location2D item5 in A.EnumerateLocations())
					{
						if (Z.GetCell(item5).HasObjectWithBlueprintStartsWith(blueprint2) && !Points.Contains(item5))
						{
							Points.Add(item5);
						}
					}
					if (Points.Count == 0)
					{
						Points.AddRange(A.EnumerateLocations());
					}
					flag3 = true;
					bAllowCaching = false;
				}
				else if (text.StartsWith("StackWith:"))
				{
					string blueprint3 = text.Split(':')[1];
					foreach (Location2D item6 in A.EnumerateLocations())
					{
						if (Z.GetCell(item6).HasObjectWithBlueprint(blueprint3) && !Points.Contains(item6))
						{
							Points.Add(item6);
						}
					}
					flag3 = true;
				}
				else if (text.StartsWith("StackWithWithTag:"))
				{
					string name = text.Split(':')[1];
					foreach (Location2D item7 in A.EnumerateLocations())
					{
						if (Z.GetCell(item7).HasObjectWithPropertyOrTag(name) && !Points.Contains(item7))
						{
							Points.Add(item7);
						}
					}
					flag3 = true;
				}
				else if (text.StartsWith("StackWithSelf") || text == "Stack")
				{
					foreach (Location2D item8 in A.EnumerateLocations())
					{
						if (Z.GetCell(item8).HasObjectWithBlueprint(gameObject.Blueprint) && !Points.Contains(item8))
						{
							Points.Add(item8);
						}
					}
					if (Points.Count == 0)
					{
						Points.AddRange(A.EnumerateLocations());
					}
					flag3 = true;
				}
				else if (text.StartsWith("StackWithEndsWith:"))
				{
					string blueprint4 = text.Split(':')[1];
					foreach (Location2D item9 in A.EnumerateLocations())
					{
						if (Z.GetCell(item9).HasObjectWithBlueprintEndsWith(blueprint4) && !Points.Contains(item9))
						{
							Points.Add(item9);
						}
					}
					flag3 = true;
				}
				else if (text.StartsWith("AdjacentToStartsWith:"))
				{
					string blueprint5 = text.Split(':')[1];
					foreach (Location2D item10 in A.EnumerateLocations())
					{
						if (!Z.GetCell(item10).HasObjectWithBlueprintStartsWith(blueprint5))
						{
							continue;
						}
						foreach (Cell localCardinalAdjacentCell2 in Z.GetCell(item10).GetLocalCardinalAdjacentCells())
						{
							if (!localCardinalAdjacentCell2.HasObjectWithBlueprintStartsWith(blueprint5) && !Points.Contains(localCardinalAdjacentCell2.Location))
							{
								Points.Add(localCardinalAdjacentCell2.Location);
							}
						}
					}
					bAllowCaching = true;
				}
				else if (text.StartsWith("AdjacentToEndsWith:"))
				{
					string blueprint6 = text.Split(':')[1];
					foreach (Location2D item11 in A.EnumerateLocations())
					{
						if (!Z.GetCell(item11).HasObjectWithBlueprintEndsWith(blueprint6))
						{
							continue;
						}
						foreach (Cell localCardinalAdjacentCell3 in Z.GetCell(item11).GetLocalCardinalAdjacentCells())
						{
							if (!localCardinalAdjacentCell3.HasObjectWithBlueprintEndsWith(blueprint6) && !Points.Contains(localCardinalAdjacentCell3.Location))
							{
								Points.Add(localCardinalAdjacentCell3.Location);
							}
						}
					}
					bAllowCaching = false;
				}
				else if (text.StartsWith("AdjacentToWithTag:"))
				{
					string name2 = text.Split(':')[1];
					foreach (Location2D item12 in A.EnumerateLocations())
					{
						if (!Z.GetCell(item12).HasObjectWithPropertyOrTag(name2))
						{
							continue;
						}
						foreach (Cell localCardinalAdjacentCell4 in Z.GetCell(item12).GetLocalCardinalAdjacentCells())
						{
							if (!localCardinalAdjacentCell4.HasObjectWithPropertyOrTag(name2) && !Points.Contains(localCardinalAdjacentCell4.Location))
							{
								Points.Add(localCardinalAdjacentCell4.Location);
							}
						}
					}
					bAllowCaching = true;
				}
				else if (text.StartsWith("AdjacentGroup"))
				{
					int num2 = 100;
					if (text.Contains(":"))
					{
						num2 = int.Parse(text.Split(':')[1]);
					}
					if (Stat.Random(1, 100) <= num2)
					{
						foreach (Location2D item13 in A.EnumerateLocations())
						{
							if (Z.GetCell(item13).AnyLocalAdjacentCell((Cell c) => c.HasObject((GameObject o) => o.GetIntProperty("_populationgroup", -1) == group)) && !Points.Contains(item13))
							{
								Points.Add(item13);
							}
						}
					}
					bAllowCaching = false;
				}
				else if (text.StartsWith("Adjacent"))
				{
					int num3 = 100;
					if (text.Contains(":"))
					{
						num3 = int.Parse(text.Split(':')[1]);
					}
					if (Stat.Random(1, 100) <= num3)
					{
						foreach (Location2D item14 in A.EnumerateLocations())
						{
							if (!Z.GetCell(item14).HasObjectWithBlueprint(gameObject.Blueprint))
							{
								continue;
							}
							foreach (Cell localCardinalAdjacentCell5 in Z.GetCell(item14).GetLocalCardinalAdjacentCells())
							{
								if (!localCardinalAdjacentCell5.HasObjectWithBlueprint(gameObject.Blueprint) && !Points.Contains(localCardinalAdjacentCell5.Location))
								{
									Points.Add(localCardinalAdjacentCell5.Location);
								}
							}
						}
					}
					bAllowCaching = false;
				}
				else if (text.StartsWith("LivesOnWalls"))
				{
					flag3 = true;
					foreach (Location2D item15 in A.EnumerateLocations())
					{
						foreach (Cell localAdjacentCell in Z.GetCell(item15).GetLocalAdjacentCells())
						{
							if (localAdjacentCell.HasWall() && !localAdjacentCell.HasCombatObject() && !Points.Contains(localAdjacentCell.Location))
							{
								Points.Add(localAdjacentCell.Location);
							}
						}
					}
					if (Points.Count == 0)
					{
						foreach (Cell passableCell in Z.GetPassableCells())
						{
							Location2D location = passableCell.Location;
							foreach (Cell localAdjacentCell2 in Z.GetCell(location).GetLocalAdjacentCells())
							{
								if (localAdjacentCell2.HasWall() && !localAdjacentCell2.HasCombatObject() && !Points.Contains(localAdjacentCell2.Location))
								{
									Points.Add(localAdjacentCell2.Location);
								}
							}
						}
					}
					if (Points.Count == 0)
					{
						foreach (Cell reachableCell in Z.GetReachableCells())
						{
							Location2D location2 = reachableCell.Location;
							foreach (Cell localAdjacentCell3 in Z.GetCell(location2).GetLocalAdjacentCells())
							{
								if (localAdjacentCell3.HasWall() && !localAdjacentCell3.HasCombatObject() && !Points.Contains(localAdjacentCell3.Location))
								{
									Points.Add(localAdjacentCell3.Location);
								}
							}
						}
					}
					if (Points.Count == 0)
					{
						foreach (Cell emptyCell in Z.GetEmptyCells())
						{
							Location2D location3 = emptyCell.Location;
							foreach (Cell localAdjacentCell4 in Z.GetCell(location3).GetLocalAdjacentCells())
							{
								if (localAdjacentCell4.HasWall() && !localAdjacentCell4.HasCombatObject() && !Points.Contains(localAdjacentCell4.Location))
								{
									Points.Add(localAdjacentCell4.Location);
								}
							}
						}
					}
				}
				else if (text == "InsideCorner")
				{
					foreach (Location2D item16 in A.EnumerateLocations())
					{
						Cell cell = Z.GetCell(item16);
						if (!cell.HasWall() && ((cell.HasWallInDirection("W") && cell.HasWallInDirection("N") && cell.HasWallInDirection("NW")) || (cell.HasWallInDirection("N") && cell.HasWallInDirection("E") && cell.HasWallInDirection("NE")) || (cell.HasWallInDirection("S") && cell.HasWallInDirection("E") && cell.HasWallInDirection("SE")) || (cell.HasWallInDirection("S") && cell.HasWallInDirection("W") && cell.HasWallInDirection("SW"))))
						{
							Points.Add(item16);
						}
					}
					if (Points.Count == 0)
					{
						foreach (Location2D item17 in A.EnumerateLocations())
						{
							if (Z.GetCell(item17).GetLocalAdjacentCells().Any((Cell c) => c.HasWall()) && !Points.Contains(item17))
							{
								Points.Add(item17);
							}
						}
					}
				}
				else if (text.StartsWith("AlongWall:"))
				{
					string direction = text.Split(':')[1].ToString();
					foreach (Location2D item18 in A.EnumerateLocations())
					{
						Cell cell2 = Z.GetCell(item18);
						Cell cellFromDirection = cell2.GetCellFromDirection(direction);
						if (cellFromDirection != null && cellFromDirection.HasWall() && !Points.Contains(cell2.Location))
						{
							Points.Add(cell2.Location);
						}
					}
				}
				else if (text.StartsWith("OutsideToThe:"))
				{
					string direction2 = text.Split(':')[1].ToString();
					foreach (Location2D item19 in A.EnumerateLocations())
					{
						Cell cell3 = Z.GetCell(item19);
						Cell cellFromDirection2 = cell3.GetCellFromDirection(direction2);
						while (true)
						{
							if (cellFromDirection2 != null)
							{
								if (cellFromDirection2.HasWall())
								{
									break;
								}
								cellFromDirection2 = cellFromDirection2.GetCellFromDirection(direction2);
								continue;
							}
							if (!Points.Contains(cell3.Location))
							{
								Points.Add(cell3.Location);
							}
							break;
						}
					}
				}
				else if (text == "AlongWall")
				{
					foreach (Location2D item20 in A.EnumerateLocations())
					{
						if (Z.GetCell(item20).GetLocalAdjacentCells().Any((Cell c) => c.HasWall()) && !Points.Contains(item20))
						{
							Points.Add(item20);
						}
					}
				}
				else if (text.StartsWith("Center"))
				{
					Points.Add(A.GetCenter());
				}
				else if (text.StartsWith("RadialFromCenter:"))
				{
					int radius = Convert.ToInt32(text.Split(':')[1]);
					List<Location2D> radialPoints = A.GetCenter().GetRadialPoints(radius);
					for (int num4 = 0; num4 < radialPoints.Count; num4++)
					{
						if (A.PointIn(radialPoints[num4]) && !A.EnumerateBorderLocations().Contains(radialPoints[num4]))
						{
							Points.Add(radialPoints[num4]);
						}
					}
				}
				else if (text.StartsWith("Nonborder"))
				{
					Points.AddRange(A.EnumerateNonBorderLocations());
				}
				else if (text.StartsWith("Border"))
				{
					Points.AddRange(A.EnumerateBorderLocations());
				}
				else if (text.StartsWith("Any"))
				{
					Points.AddRange(A.EnumerateLocations());
				}
				if (!flag3)
				{
					Points.RemoveAll((Location2D l) => !Z.GetCell(l).IsEmpty() || Z.GetCell(l).HasObject(gameObject.Blueprint));
				}
			}
			if (bAllowCaching && !ObjectPlacementCache.ContainsKey(text))
			{
				ObjectPlacementCache.Add(text, Points);
			}
			foreach (Location2D point in Points)
			{
				if (hintFieldtotals.ContainsKey(point))
				{
					hintFieldtotals[point]++;
				}
				else
				{
					hintFieldtotals.Add(point, 1);
				}
				if (hintFieldtotals[point] > num)
				{
					num = hintFieldtotals[point];
					workingSet.Clear();
				}
				if (hintFieldtotals[point] >= num)
				{
					workingSet.Add(point);
				}
			}
		}
		if (workingSet.Count == 0)
		{
			workingSet.AddRange(from l in A.EnumerateLocations()
				where Z.GetCell(l).IsReachable() && Z.GetCell(l).IsEmpty()
				select l);
			if (workingSet.Count == 0)
			{
				workingSet.AddRange(from l in A.EnumerateLocations()
					where Z.GetCell(l).IsEmpty()
					select l);
			}
			bAllowCaching = false;
		}
		if (gameObject.Brain != null)
		{
			if (gameObject.Brain.LivesOnWalls)
			{
				workingSet.RemoveAll((Location2D p) => !Z.GetCell(p).HasWall());
			}
			if (gameObject.Brain.Aquatic)
			{
				workingSet.RemoveAll((Location2D p) => !Z.GetCell(p).HasOpenLiquidVolume() || Z.GetCell(p).HasBridge());
			}
		}
		if (flag)
		{
			workingSet.RemoveAll((Location2D l) => Z.GetCell(l).HasRealObject());
		}
		else
		{
			workingSet.RemoveAll((Location2D l) => Z.GetCell(l).HasSpawnBlocker());
		}
		foreach (ZoneConnection item21 in Z.EnumerateConnections())
		{
			workingSet.Remove(item21.Loc2D);
		}
		if (gameObject.IsCombatObject())
		{
			workingSet.RemoveAll((Location2D p) => Z.GetCell(p).HasCombatObject());
		}
		Location2D location2D = workingSet.GetRandomElement();
		if (location2D == null)
		{
			Cell cell4 = (from ce in Z.GetCells()
				where ce.IsEmpty()
				select ce).GetRandomElement();
			if (cell4 == null)
			{
				cell4 = Z.GetRandomCell();
			}
			if (!gameObject.IsImportant() && gameObject.HasTagOrProperty("PlacementIsSkippable") && cell4.HasObjectWithTagOrProperty("PlacementIsSkippable"))
			{
				return true;
			}
			location2D = cell4.Location;
		}
		if (gameObject.isFurniture() && Z.GetCell(location2D).HasFurniture())
		{
			Z.GetCell(location2D).getClosestPassableCell((Cell c) => c != null && !c.HasFurniture())?.AddObject(gameObject);
		}
		else if (gameObject.IsCombatObject() && Z.GetCell(location2D).HasCombatObject())
		{
			Z.GetCell(location2D).getClosestPassableCell().AddObject(gameObject);
		}
		else
		{
			Z.GetCell(location2D).AddObject(gameObject);
		}
		if (Options.GetOption("OptionDrawPopulationHintMaps") == "Yes")
		{
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			for (int num5 = 0; num5 < 80; num5++)
			{
				for (int num6 = 0; num6 < 25; num6++)
				{
					scrapBuffer.Goto(num5, num6);
					if (num5 == location2D.X && num6 == location2D.Y)
					{
						scrapBuffer.Write("&CX");
					}
					else if (hintFieldtotals.ContainsKey(Location2D.Get(num5, num6)))
					{
						if (hintFieldtotals[Location2D.Get(num5, num6)] == 1)
						{
							scrapBuffer.Write("&R1");
						}
						if (hintFieldtotals[Location2D.Get(num5, num6)] == 2)
						{
							scrapBuffer.Write("&O2");
						}
						if (hintFieldtotals[Location2D.Get(num5, num6)] == 3)
						{
							scrapBuffer.Write("&W3");
						}
						if (hintFieldtotals[Location2D.Get(num5, num6)] == 4)
						{
							scrapBuffer.Write("&G4");
						}
						if (hintFieldtotals[Location2D.Get(num5, num6)] == 5)
						{
							scrapBuffer.Write("&B5");
						}
						if (hintFieldtotals[Location2D.Get(num5, num6)] == 6)
						{
							scrapBuffer.Write("&M6");
						}
						if (hintFieldtotals[Location2D.Get(num5, num6)] >= 7)
						{
							scrapBuffer.Write("&Y7");
						}
					}
					else if (workingSet.Contains(Location2D.Get(num5, num6)))
					{
						if (Z.GetCell(num5, num6).HasWall())
						{
							scrapBuffer.Write("&g#");
						}
						else
						{
							scrapBuffer.Write("&g.");
						}
					}
					else if (Z.GetCell(num5, num6).HasWall())
					{
						scrapBuffer.Write("&K#");
					}
					else
					{
						scrapBuffer.Write("&K.");
					}
				}
			}
			scrapBuffer.Draw();
			Keyboard.getch();
		}
		if (bAllowCaching)
		{
			foreach (KeyValuePair<string, List<Location2D>> item22 in ObjectPlacementCache)
			{
				item22.Value.Remove(location2D);
			}
		}
		workingSet.Clear();
		return true;
	}

	public static bool PlaceObjectInRegion(Zone Z, ILocationArea A, string Blueprint, int p = 0, int n = 0, string Hint = null, string Builder = null, bool bAllowCaching = false, GameObject precreated = null, Func<string, GameObject> customFabricator = null)
	{
		return PlaceObjectInArea(Z, A, (precreated != null) ? precreated : ApplyHintsToObject((customFabricator == null) ? GameObject.Create(Blueprint) : customFabricator(Blueprint), Hint, p), p, n, Hint, Builder, bAllowCaching);
	}

	public static bool PlaceObjectInRect(Zone Z, Rect2D R, Rect2D Exclude, string Object, string Hint = null, int group = 0, Func<string, GameObject> customFabricator = null)
	{
		R = R.enforceVailidity(R);
		List<Location2D> list = new List<Location2D>(Math.Max(1, R.Area - Exclude.Area));
		for (int i = R.y1; i <= R.y2; i++)
		{
			for (int j = R.x1; j <= R.x2; j++)
			{
				if (!Exclude.PointIn(Location2D.Get(j, i)))
				{
					list.Add(Location2D.Get(j, i));
				}
			}
		}
		PlaceObjectInArea(Z, new LocationList(list), (customFabricator == null) ? GameObject.Create(Object) : customFabricator(Object), group, 0, Hint);
		return false;
	}

	public static bool PlaceObjectInRect(Zone Z, Rect2D R, Rect2D Exclude, string Object, string Hint, Action<GameObject> BeforeObjectCreated, Func<string, GameObject> customFabricator = null)
	{
		List<Location2D> list = new List<Location2D>(Math.Max(1, R.Area - Exclude.Area));
		for (int i = R.y1; i <= R.y2; i++)
		{
			for (int j = R.x1; j <= R.x2; j++)
			{
				if (!Exclude.PointIn(Location2D.Get(j, i)))
				{
					list.Add(Location2D.Get(j, i));
				}
			}
		}
		PlaceObjectInArea(Z, new LocationList(list), (customFabricator == null) ? GameObject.Create(Object) : customFabricator(Object), 0, 0, Hint);
		return false;
	}

	public static bool PlaceObjectInRect(Zone Z, Rect2D R, Rect2D Exclude, GameObject Object, string Hint = null)
	{
		List<Location2D> list = new List<Location2D>(R.Area - Exclude.Area);
		for (int i = R.y1; i <= R.y2; i++)
		{
			for (int j = R.x1; j <= R.x2; j++)
			{
				if (!Exclude.PointIn(Location2D.Get(j, i)))
				{
					list.Add(Location2D.Get(j, i));
				}
			}
		}
		PlaceObjectInArea(Z, new LocationList(list), Object, 0, 0, Hint);
		return false;
	}

	public static bool PlaceObjectInRect(Zone Z, Rect2D R, string Object, string Hint = null, int group = 0, Func<string, GameObject> customFabricator = null)
	{
		if (R.Area <= 0)
		{
			PlaceObject(ApplyHintsToObject((customFabricator == null) ? GameObject.Create(Object) : customFabricator(Object), Hint, group), Z);
			return false;
		}
		List<Location2D> list = new List<Location2D>(R.Area);
		for (int i = R.x1; i <= R.x2; i++)
		{
			for (int j = R.y1; j <= R.y2; j++)
			{
				list.Add(Location2D.Get(i, j));
			}
		}
		PlaceObjectInArea(Z, new LocationList(list), (customFabricator == null) ? GameObject.Create(Object) : customFabricator(Object), group, 0, Hint);
		return false;
	}

	public static bool PlaceObjectInRect(Zone Z, Rect2D R, string Object, string Hint, Action<GameObject> BeforeObjectCreated, Func<string, GameObject> customFabricator = null)
	{
		if (R.Area <= 0)
		{
			PlaceObject(ApplyHintsToObject((customFabricator == null) ? GameObject.Create(Object) : customFabricator(Object), Hint), Z);
			return false;
		}
		List<Location2D> list = new List<Location2D>(R.Area);
		for (int i = R.x1; i <= R.x2; i++)
		{
			for (int j = R.y1; j <= R.y2; j++)
			{
				list.Add(Location2D.Get(i, j));
			}
		}
		PlaceObjectInArea(Z, new LocationList(list), (customFabricator == null) ? GameObject.Create(Object) : customFabricator(Object), 0, 0, Hint);
		return false;
	}

	public static bool PlaceObjectInRect(Zone Z, Rect2D R, GameObject Object, string Hint = null)
	{
		R = R.enforceVailidity(R);
		List<Location2D> list = new List<Location2D>(R.Area);
		for (int i = R.y1; i <= R.y2; i++)
		{
			for (int j = R.x1; j <= R.x2; j++)
			{
				list.Add(Location2D.Get(j, i));
			}
		}
		PlaceObjectInArea(Z, new LocationList(list), Object, 0, 0, Hint);
		return false;
	}

	public static void PlacePrefab(Zone Z, string mapName, PlacePrefabAlign alignment, string ChunkHint = null, Action<Cell> PreAction = null, Action<Cell> PostAction = null, Func<string, Cell, bool> ShouldPlace = null, Func<string, Cell, string> Replace = null, Action<string, Cell> BeforePlacement = null, Action<string, Cell> AfterPlacement = null)
	{
		MapFile mapFile = MapFile.Resolve(mapName);
		Cell upperLeft = null;
		if (alignment == PlacePrefabAlign.NW)
		{
			upperLeft = Z.GetCell(0, 0);
		}
		if (alignment == PlacePrefabAlign.N)
		{
			upperLeft = Z.GetCell(Z.Width / 2 - mapFile.RightmostObject() / 2, 0);
		}
		if (alignment == PlacePrefabAlign.NE)
		{
			upperLeft = Z.GetCell(Z.Width - mapFile.RightmostObject(), 0);
		}
		if (alignment == PlacePrefabAlign.E)
		{
			upperLeft = Z.GetCell(Z.Width - mapFile.RightmostObject(), Z.Height / 2 - mapFile.BottommostObject() / 2);
		}
		if (alignment == PlacePrefabAlign.SE)
		{
			upperLeft = Z.GetCell(Z.Width - mapFile.RightmostObject(), Z.Height - mapFile.BottommostObject());
		}
		if (alignment == PlacePrefabAlign.S)
		{
			upperLeft = Z.GetCell(Z.Width / 2 - mapFile.RightmostObject() / 2, Z.Height - mapFile.BottommostObject());
		}
		if (alignment == PlacePrefabAlign.SW)
		{
			upperLeft = Z.GetCell(0, Z.Height - mapFile.BottommostObject());
		}
		if (alignment == PlacePrefabAlign.W)
		{
			upperLeft = Z.GetCell(0, Z.Height / 2 - mapFile.BottommostObject() / 2);
		}
		if (alignment == PlacePrefabAlign.Center)
		{
			upperLeft = Z.GetCell(Z.Width / 2 - mapFile.RightmostObject() / 2, Z.Height / 2 - mapFile.BottommostObject() / 2);
		}
		PlacePrefab(Z, mapName, upperLeft, 0, ChunkHint, PreAction, PostAction, ShouldPlace, Replace, BeforePlacement, AfterPlacement);
	}

	public static void PlacePrefab(Zone Z, string mapName, Cell upperLeft, int Padding = 0, string ChunkHint = null, Action<Cell> PreAction = null, Action<Cell> PostAction = null, Func<string, Cell, bool> ShouldPlace = null, Func<string, Cell, string> Replace = null, Action<string, Cell> BeforePlacement = null, Action<string, Cell> AfterPlacement = null)
	{
		MapFile mapFile = MapFile.Resolve(mapName);
		Point2D pos2D = upperLeft.Pos2D;
		int num = mapFile.RightmostObject() + 1;
		int num2 = mapFile.BottommostObject() + 1;
		if (pos2D.x + num + Padding >= Z.Width)
		{
			pos2D.x -= pos2D.x + num + Padding - Z.Width;
		}
		if (pos2D.y + num2 + Padding >= Z.Height)
		{
			pos2D.y -= pos2D.y + num2 + Padding - Z.Height;
		}
		if (ChunkHint != null && ChunkHint == "Center")
		{
			pos2D.x = Z.Width / 2 - num / 2;
			pos2D.y = Z.Height / 2 - num2 / 2;
		}
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Cell cell = Z.GetCell(pos2D.x + j, pos2D.y + i);
				if (cell != null)
				{
					mapFile.Cells[j, i].ApplyTo(cell, CheckEmpty: false, PreAction, PostAction, ShouldPlace, Replace, null, BeforePlacement, AfterPlacement);
				}
			}
		}
	}

	public void ClearRect(Zone Z, Rect2D R)
	{
		for (int i = R.x1; i <= R.x2; i++)
		{
			for (int j = R.y1; j <= R.y2; j++)
			{
				Z.GetCell(i, j)?.Clear();
			}
		}
	}

	public void FillRect(Zone Z, Rect2D R, string Blueprint)
	{
		for (int i = R.x1; i <= R.x2; i++)
		{
			for (int j = R.y1; j <= R.y2; j++)
			{
				Z.GetCell(i, j)?.AddObject(Blueprint);
			}
		}
	}

	public static void PlaceObjectOnRect(Zone Z, string Blueprint, Rect2D R, bool bClear = false)
	{
		PlaceObjectOnRect(Z, GameObjectFactory.Factory.CreateObject(Blueprint), R, bClear);
	}

	public static void PlaceObjectOnRect(Zone Z, GameObject objectToCopy, Rect2D R, bool bClear = false)
	{
		if (Z == null)
		{
			MetricsManager.LogError("zone missing");
			return;
		}
		if (objectToCopy == null)
		{
			MetricsManager.LogError("object missing");
			return;
		}
		int num = ((!(R.DoorDirection == "W") && !(R.DoorDirection == "E")) ? 1 : 0);
		for (int i = R.x1 + num; i <= R.x2 - num; i++)
		{
			if (R.Pinch > -1 && ((R.DoorDirection == "W" && i - R.x1 <= R.Pinch) || (R.DoorDirection == "E" && i - R.x1 >= R.Pinch)))
			{
				Z.GetCellChecked(i, R.y1 + 1)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
				Z.GetCellChecked(i, R.y2 - 1)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
				if (i - R.x1 == R.Pinch)
				{
					Z.GetCellChecked(i, R.y1)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
					Z.GetCellChecked(i, R.y2)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
				}
			}
			else
			{
				Z.GetCellChecked(i, R.y1)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
				Z.GetCellChecked(i, R.y2)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
			}
		}
		num = ((R.DoorDirection == "W" || R.DoorDirection == "E") ? 1 : 0);
		for (int j = R.y1 + num; j <= R.y2 - num; j++)
		{
			if (R.Pinch > -1 && ((R.DoorDirection == "S" && j - R.y1 <= R.Pinch) || (R.DoorDirection == "N" && j - R.y1 >= R.Pinch)))
			{
				Z.GetCellChecked(R.x1 + 1, j)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
				Z.GetCellChecked(R.x2 - 1, j)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
				if (j - R.y1 == R.Pinch)
				{
					Z.GetCellChecked(R.x1, j)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
					Z.GetCellChecked(R.x2, j)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
				}
			}
			else
			{
				Z.GetCellChecked(R.x1, j)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
				Z.GetCellChecked(R.x2, j)?.ClearAndAddObject(objectToCopy.DeepCopy(), bClear);
			}
		}
	}

	public static InfluenceMap GenerateInfluenceMapNRegions(Zone Z, List<Point> AdditionalSeeds, InfluenceMapSeedStrategy SeedStrategy, int NumberOfRegions, List<Location2D> AdditionalWalls = null, bool bDraw = false, bool bAddExtraConnectionRegions = true)
	{
		InfluenceMap influenceMap = new InfluenceMap(Z.Width, Z.Height);
		influenceMap.bDraw = bDraw;
		Z.SetInfluenceMapWalls(influenceMap.Walls);
		if (AdditionalWalls != null)
		{
			foreach (Location2D AdditionalWall in AdditionalWalls)
			{
				influenceMap.Walls[AdditionalWall.X, AdditionalWall.Y] = 1;
			}
		}
		if (AdditionalSeeds != null)
		{
			foreach (Point AdditionalSeed in AdditionalSeeds)
			{
				influenceMap.AddSeed(AdditionalSeed.X, AdditionalSeed.Y, bRecalculate: false);
			}
		}
		if (bAddExtraConnectionRegions)
		{
			foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
			{
				if (!Z.GetCell(zoneConnection.X, zoneConnection.Y).IsSolid())
				{
					influenceMap.AddSeed(zoneConnection.X, zoneConnection.Y, bRecalculate: false);
				}
			}
			foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
			{
				if (item.TargetDirection == "-" && !Z.GetCell(item.X, item.Y).IsSolid())
				{
					influenceMap.AddSeed(item.X, item.Y, bRecalculate: false);
				}
			}
		}
		if (influenceMap.Seeds.Count > 0)
		{
			influenceMap.Recalculate();
			List<Cell> cells = Z.GetCells();
			Algorithms.RandomShuffleInPlace(cells, Stat.Rand);
			foreach (Cell item2 in cells)
			{
				if (influenceMap.GetSeedAt(Location2D.Get(item2.X, item2.Y)) == -1 && item2.IsEmpty())
				{
					influenceMap.AddSeed(item2.X, item2.Y);
					influenceMap.Recalculate();
				}
			}
		}
		influenceMap.bDraw = bDraw;
		for (int i = 0; i < NumberOfRegions; i++)
		{
			if (SeedStrategy == InfluenceMapSeedStrategy.RandomPoint)
			{
				influenceMap.AddSeedAtTrueRandom(0);
			}
			if (SeedStrategy == InfluenceMapSeedStrategy.LargestRegion)
			{
				influenceMap.AddSeedAtMaximaInLargestSeed();
			}
			if (SeedStrategy == InfluenceMapSeedStrategy.FurthestPoint)
			{
				influenceMap.AddSeedAtMaxima();
			}
			if (SeedStrategy == InfluenceMapSeedStrategy.RandomPointFurtherThan4)
			{
				influenceMap.AddSeedAtRandom();
			}
		}
		influenceMap.bDraw = bDraw;
		influenceMap.Recalculate();
		return influenceMap;
	}

	public static InfluenceMap GenerateInfluenceMap(Zone Z, List<Point> AdditionalSeeds, InfluenceMapSeedStrategy SeedStrategy, int MaxRegionSize, List<Location2D> AdditionalWalls = null, bool bDraw = false, Func<Cell, int> wallGenerator = null)
	{
		InfluenceMap influenceMap = new InfluenceMap(Z.Width, Z.Height);
		influenceMap.bDraw = bDraw;
		if (wallGenerator != null)
		{
			for (int i = 0; i < Z.Width; i++)
			{
				for (int j = 0; j < Z.Height; j++)
				{
					influenceMap.Walls[i, j] = wallGenerator(Z.GetCell(i, j));
				}
			}
		}
		else
		{
			Z.SetInfluenceMapWalls(influenceMap.Walls);
		}
		if (AdditionalWalls != null)
		{
			foreach (Location2D AdditionalWall in AdditionalWalls)
			{
				influenceMap.Walls[AdditionalWall.X, AdditionalWall.Y] = 1;
			}
		}
		if (AdditionalSeeds != null)
		{
			foreach (Point AdditionalSeed in AdditionalSeeds)
			{
				influenceMap.AddSeed(AdditionalSeed.X, AdditionalSeed.Y, bRecalculate: false);
			}
		}
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			if (!Z.GetCell(zoneConnection.X, zoneConnection.Y).IsSolid())
			{
				influenceMap.AddSeed(zoneConnection.X, zoneConnection.Y, bRecalculate: false);
			}
		}
		foreach (CachedZoneConnection item in Z.ZoneConnectionCache)
		{
			if (item.TargetDirection == "-" && !Z.GetCell(item.X, item.Y).IsSolid())
			{
				influenceMap.AddSeed(item.X, item.Y, bRecalculate: false);
			}
		}
		if (influenceMap.Seeds.Count > 0)
		{
			influenceMap.Recalculate();
			List<Cell> cells = Z.GetCells();
			Algorithms.RandomShuffleInPlace(cells, Stat.Rand);
			foreach (Cell item2 in cells)
			{
				if (influenceMap.GetSeedAt(Location2D.Get(item2.X, item2.Y)) == -1 && item2.IsEmpty() && influenceMap.Walls[item2.X, item2.Y] == 0)
				{
					influenceMap.AddSeed(item2.X, item2.Y);
					influenceMap.Recalculate();
				}
			}
		}
		influenceMap.bDraw = bDraw;
		while (influenceMap.Seeds.Count == 0 || influenceMap.LargestSize() > MaxRegionSize)
		{
			if (SeedStrategy == InfluenceMapSeedStrategy.LargestRegion)
			{
				influenceMap.AddSeedAtMaximaInLargestSeed();
			}
			if (SeedStrategy == InfluenceMapSeedStrategy.FurthestPoint)
			{
				influenceMap.AddSeedAtMaxima();
			}
			if (SeedStrategy == InfluenceMapSeedStrategy.RandomPointFurtherThan4)
			{
				influenceMap.AddSeedAtRandom(5);
			}
			if (SeedStrategy == InfluenceMapSeedStrategy.RandomPointFurtherThan1)
			{
				influenceMap.AddSeedAtRandom(0);
			}
		}
		influenceMap.bDraw = bDraw;
		influenceMap.Recalculate(bDraw);
		return influenceMap;
	}

	public static GameObject CreateObject(string Blueprint)
	{
		return GameObject.Create(Blueprint);
	}

	public static int GetLevelOfObject(string Blueprint)
	{
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints[Blueprint];
		if (gameObjectBlueprint.HasStat("Level"))
		{
			return gameObjectBlueprint.GetStat("Level").Value;
		}
		return 0;
	}

	public static void PlaceParty(List<GameObject> Party, Zone Z, int Radius = -1)
	{
		if (Radius == -1)
		{
			Radius = Math.Min(4, Party.Count / 2);
		}
		for (int i = 0; i < 500; i++)
		{
			int num = Stat.Random(0, Z.Width - 1);
			int num2 = Stat.Random(0, Z.Height - 1);
			int num3 = 0;
			Rect2D rect2D = new Rect2D(num - Radius, num2 - Radius, num + Radius, num2 + Radius).Clamp(0, 0, Z.Width - 1, Z.Height - 1);
			List<Location2D> list = new List<Location2D>(rect2D.Area);
			for (int j = rect2D.y1; j <= rect2D.y2; j++)
			{
				for (int k = rect2D.x1; k <= rect2D.x2; k++)
				{
					Location2D location2D = Location2D.Get(k, j);
					if (Z.GetCell(location2D).IsEmpty())
					{
						num3++;
					}
					list.Add(location2D);
				}
			}
			if (num3 < Party.Count)
			{
				continue;
			}
			LocationList a = new LocationList(list);
			{
				foreach (GameObject item in Party)
				{
					PlaceObjectInArea(Z, a, item);
				}
				break;
			}
		}
	}

	public static bool PlaceObject(string GO, Zone Z)
	{
		return PlaceObject(GameObject.Create(GO), Z);
	}

	public static bool PlaceObject(GameObject GO, Zone Z, string Hints = null)
	{
		return PlaceObjectInArea(Z, Z.area, GO, 0, 0, Hints);
	}

	public static void PlaceEncounter(List<GameObject> GOs, Zone Z)
	{
		foreach (GameObject GO in GOs)
		{
			PlaceObject(GO, Z);
		}
	}

	public bool IsEmpty(Location2D loc)
	{
		return zone.GetCell(loc).IsEmptyForPopulation();
	}

	public bool PlaceObjectInBuilding(GameObject obj, PopulationLayout building, string Hint = null, Predicate<Location2D> test = null)
	{
		placementCells.Clear();
		if (string.IsNullOrEmpty(Hint))
		{
			Hint = obj.GetPropertyOrTag("PlacementHint", "");
		}
		if (building == null)
		{
			PlaceObject(obj, zone);
			return true;
		}
		switch (Hint)
		{
		case "":
			placementCells.AddRange(building.inside.Where(IsEmpty));
			placementCells.AddRange(building.outside.Where(IsEmpty));
			placementCells.ShuffleInPlace();
			break;
		case "InsideCorner":
			placementCells.AddRange(building.insideCorner.Where(IsEmpty));
			placementCells.ShuffleInPlace();
			placementCells.AddRange(building.insideWall.Where(IsEmpty).ToList().ShuffleInPlace());
			placementCells.AddRange(building.inside.Where(IsEmpty).ToList().ShuffleInPlace());
			placementCells.AddRange(building.outside.Where(IsEmpty));
			break;
		case "OutsideCorner":
			placementCells.AddRange(building.outsideCorner.Where(IsEmpty));
			placementCells.ShuffleInPlace();
			placementCells.AddRange(building.outsideWall.Where(IsEmpty).ToList().ShuffleInPlace());
			placementCells.AddRange(building.outside.Where(IsEmpty).ToList().ShuffleInPlace());
			break;
		case "AlongInsideWall":
			placementCells.AddRange(building.insideWall.Where(IsEmpty).ToList().ShuffleInPlace());
			placementCells.AddRange(building.inside.Where(IsEmpty).ToList().ShuffleInPlace());
			placementCells.AddRange(building.outside.Where(IsEmpty));
			break;
		case "AlongOutsideWall":
			placementCells.AddRange(building.outsideWall.Where(IsEmpty).ToList().ShuffleInPlace());
			placementCells.AddRange(building.outside.Where(IsEmpty).ToList().ShuffleInPlace());
			placementCells.AddRange(building.inside.Where(IsEmpty).ToList().ShuffleInPlace());
			break;
		case "AlongWall":
			placementCells.AddRange(building.insideWall.Where(IsEmpty).Union(building.outsideWall.Where(IsEmpty)).ToList()
				.ShuffleInPlace());
			placementCells.AddRange(building.inside.Where(IsEmpty).Union(building.inside.Where(IsEmpty)).ToList()
				.ShuffleInPlace());
			break;
		case "Outside":
		case "Exterior":
			placementCells.AddRange(building.outside.Where(IsEmpty));
			placementCells.ShuffleInPlace();
			placementCells.AddRange(building.inside.Where(IsEmpty).ToList().ShuffleInPlace());
			break;
		case "Inside":
		case "Interior":
			placementCells.AddRange(building.inside.Where(IsEmpty));
			placementCells.ShuffleInPlace();
			placementCells.AddRange(building.outside.Where(IsEmpty).ToList().ShuffleInPlace());
			break;
		}
		if (test != null)
		{
			placementCells.RemoveAll((Location2D l) => !test(l));
		}
		if (placementCells.Count > 0)
		{
			zone.GetCell(placementCells[0]).AddObject(obj);
			return true;
		}
		PlaceObject(obj, zone);
		return false;
	}

	public int PlaceObjectInBuilding(PopulationResult result, PopulationLayout building, Predicate<Location2D> test = null)
	{
		int num = 0;
		for (int i = 0; i < result.Number; i++)
		{
			placementCells.Clear();
			string text = result.Hint;
			if (string.IsNullOrEmpty(text))
			{
				GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(result.Blueprint);
				if (blueprint != null)
				{
					text = blueprint.GetTag("PlacementHint");
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				placementCells.AddRange(building.inside.Where(IsEmpty));
				placementCells.ShuffleInPlace();
				placementCells.AddRange(building.outside.Where(IsEmpty));
			}
			else
			{
				switch (text)
				{
				case "InsideCorner":
					placementCells.AddRange(building.insideCorner.Where(IsEmpty));
					placementCells.ShuffleInPlace();
					placementCells.AddRange(building.insideWall.Where(IsEmpty).ToList().ShuffleInPlace());
					placementCells.AddRange(building.inside.Where(IsEmpty).ToList().ShuffleInPlace());
					break;
				case "OutsideCorner":
					placementCells.AddRange(building.outsideCorner.Where(IsEmpty));
					placementCells.ShuffleInPlace();
					placementCells.AddRange(building.outsideWall.Where(IsEmpty).ToList().ShuffleInPlace());
					placementCells.AddRange(building.outside.Where(IsEmpty).ToList().ShuffleInPlace());
					break;
				case "AlongInsideWall":
					placementCells.AddRange(building.insideWall.Where(IsEmpty).ToList().ShuffleInPlace());
					placementCells.AddRange(building.inside.Where(IsEmpty).ToList().ShuffleInPlace());
					break;
				case "AlongOutsideWall":
					placementCells.AddRange(building.outsideWall.Where(IsEmpty).ToList().ShuffleInPlace());
					placementCells.AddRange(building.outside.Where(IsEmpty).ToList().ShuffleInPlace());
					break;
				case "AlongWall":
					placementCells.AddRange(building.insideWall.Where(IsEmpty).Union(building.outsideWall.Where(IsEmpty)).ToList()
						.ShuffleInPlace());
					placementCells.AddRange(building.inside.Where(IsEmpty).Union(building.outside.Where(IsEmpty)).ToList()
						.ShuffleInPlace());
					break;
				case "Outside":
				case "Exterior":
					placementCells.AddRange(building.outside.Where(IsEmpty));
					placementCells.ShuffleInPlace();
					placementCells.AddRange(building.inside.Where(IsEmpty));
					break;
				case "Inside":
				case "Interior":
					placementCells.AddRange(building.inside.Where(IsEmpty));
					placementCells.ShuffleInPlace();
					placementCells.AddRange(building.outside.Where(IsEmpty));
					break;
				}
			}
			if (test != null && placementCells.Count > 0)
			{
				placementCells = placementCells.Where((Location2D l) => test(l)).ToList();
			}
			if (placementCells.Count > 0)
			{
				zone.GetCell(placementCells[0]).AddObject(result.Blueprint);
				num++;
			}
			else
			{
				PlaceObject(GameObjectFactory.Factory.CreateObject(result.Blueprint), zone);
			}
		}
		return num;
	}

	public static bool PlaceObject(Zone Z, InfluenceMapRegion R, GameObject obj, string Hint = null, string Builder = null, bool bAllowCaching = false)
	{
		Points.Clear();
		bool flag = false;
		if (obj.Brain != null && obj.Brain.Aquatic)
		{
			Hint = "Aquatic";
		}
		else if (string.IsNullOrEmpty(Hint))
		{
			Hint = obj.GetPropertyOrTag("PlacementHint", "");
		}
		if (Hint.StartsWith("Aquatic"))
		{
			foreach (Location2D cell2 in R.Cells)
			{
				if (Z.GetCell(cell2).HasWadingDepthLiquid() && !Z.GetCell(cell2).HasBridge())
				{
					Points.Add(cell2);
				}
			}
		}
		else if (Hint.StartsWith("AdjacentToBlueprint:"))
		{
			string blueprint = Hint.Split(':')[1];
			foreach (Location2D nonBorderCell in R.NonBorderCells)
			{
				if (!Z.GetCell(nonBorderCell).HasObjectWithBlueprint(blueprint))
				{
					continue;
				}
				foreach (Cell localCardinalAdjacentCell in Z.GetCell(nonBorderCell).GetLocalCardinalAdjacentCells())
				{
					if (!localCardinalAdjacentCell.HasObjectWithBlueprint(blueprint) && !Points.Contains(localCardinalAdjacentCell.Location))
					{
						Points.Add(localCardinalAdjacentCell.Location);
					}
				}
			}
			if (Points.Count == 0)
			{
				Points.AddRange(R.NonBorderCells);
			}
			bAllowCaching = false;
		}
		else if (Hint.StartsWith("StackWithStartsWith:"))
		{
			string blueprint2 = Hint.Split(':')[1];
			foreach (Location2D nonBorderCell2 in R.NonBorderCells)
			{
				if (Z.GetCell(nonBorderCell2).HasObjectWithBlueprintStartsWith(blueprint2) && !Points.Contains(nonBorderCell2))
				{
					Points.Add(nonBorderCell2);
				}
			}
			if (Points.Count == 0)
			{
				Points.AddRange(R.NonBorderCells);
			}
			flag = true;
		}
		else if (Hint.StartsWith("StackWith:"))
		{
			string blueprint3 = Hint.Split(':')[1];
			foreach (Location2D nonBorderCell3 in R.NonBorderCells)
			{
				if (Z.GetCell(nonBorderCell3).HasObjectWithBlueprint(blueprint3) && !Points.Contains(nonBorderCell3))
				{
					Points.Add(nonBorderCell3);
				}
			}
			if (Points.Count == 0)
			{
				Points.AddRange(R.NonBorderCells);
			}
			flag = true;
		}
		else if (Hint.StartsWith("StackWithSelf") || Hint == "Stack")
		{
			foreach (Location2D nonBorderCell4 in R.NonBorderCells)
			{
				if (Z.GetCell(nonBorderCell4).HasObjectWithBlueprint(obj.Blueprint) && !Points.Contains(nonBorderCell4))
				{
					Points.Add(nonBorderCell4);
				}
			}
			if (Points.Count == 0)
			{
				Points.AddRange(R.NonBorderCells);
			}
			flag = true;
		}
		else if (Hint.StartsWith("StackWithEndsWith:"))
		{
			string blueprint4 = Hint.Split(':')[1];
			foreach (Location2D nonBorderCell5 in R.NonBorderCells)
			{
				if (Z.GetCell(nonBorderCell5).HasObjectWithBlueprintEndsWith(blueprint4) && !Points.Contains(nonBorderCell5))
				{
					Points.Add(nonBorderCell5);
				}
			}
			if (Points.Count == 0)
			{
				Points.AddRange(R.NonBorderCells);
			}
			flag = true;
		}
		else if (Hint.StartsWith("AdjacentToStartsWith:"))
		{
			string blueprint5 = Hint.Split(':')[1];
			foreach (Location2D nonBorderCell6 in R.NonBorderCells)
			{
				if (!Z.GetCell(nonBorderCell6).HasObjectWithBlueprintStartsWith(blueprint5))
				{
					continue;
				}
				foreach (Cell localCardinalAdjacentCell2 in Z.GetCell(nonBorderCell6).GetLocalCardinalAdjacentCells())
				{
					if (!localCardinalAdjacentCell2.HasObjectWithBlueprintStartsWith(blueprint5) && !Points.Contains(localCardinalAdjacentCell2.Location))
					{
						Points.Add(localCardinalAdjacentCell2.Location);
					}
				}
			}
			if (Points.Count == 0)
			{
				Points.AddRange(R.NonBorderCells);
			}
			bAllowCaching = false;
		}
		else if (Hint.StartsWith("AdjacentToEndsWith:"))
		{
			string blueprint6 = Hint.Split(':')[1];
			foreach (Location2D nonBorderCell7 in R.NonBorderCells)
			{
				if (!Z.GetCell(nonBorderCell7).HasObjectWithBlueprintEndsWith(blueprint6))
				{
					continue;
				}
				foreach (Cell localCardinalAdjacentCell3 in Z.GetCell(nonBorderCell7).GetLocalCardinalAdjacentCells())
				{
					if (!localCardinalAdjacentCell3.HasObjectWithBlueprintEndsWith(blueprint6) && !Points.Contains(localCardinalAdjacentCell3.Location))
					{
						Points.Add(localCardinalAdjacentCell3.Location);
					}
				}
			}
			if (Points.Count == 0)
			{
				Points.AddRange(R.NonBorderCells);
			}
			bAllowCaching = false;
		}
		else if (Hint.StartsWith("AdjacentToEndsWith:"))
		{
			string name = Hint.Split(':')[1];
			foreach (Location2D nonBorderCell8 in R.NonBorderCells)
			{
				if (!Z.GetCell(nonBorderCell8).HasObjectWithPropertyOrTag(name))
				{
					continue;
				}
				foreach (Cell localCardinalAdjacentCell4 in Z.GetCell(nonBorderCell8).GetLocalCardinalAdjacentCells())
				{
					if (!localCardinalAdjacentCell4.HasObjectWithPropertyOrTag(name) && !Points.Contains(localCardinalAdjacentCell4.Location))
					{
						Points.Add(localCardinalAdjacentCell4.Location);
					}
				}
			}
			if (Points.Count == 0)
			{
				Points.AddRange(R.NonBorderCells);
			}
			bAllowCaching = true;
		}
		else if (Hint.StartsWith("Adjacent"))
		{
			foreach (Location2D cell3 in R.Cells)
			{
				if (!Z.GetCell(cell3).HasObjectWithBlueprint(obj.Blueprint))
				{
					continue;
				}
				foreach (Cell localCardinalAdjacentCell5 in Z.GetCell(cell3).GetLocalCardinalAdjacentCells())
				{
					if (!localCardinalAdjacentCell5.HasObjectWithBlueprint(obj.Blueprint) && !Points.Contains(localCardinalAdjacentCell5.Location))
					{
						Points.Add(localCardinalAdjacentCell5.Location);
					}
				}
			}
			if (Points.Count == 0)
			{
				Points.AddRange(R.NonBorderCells);
			}
			bAllowCaching = false;
		}
		else if (Hint.StartsWith("LivesOnWalls"))
		{
			foreach (Location2D borderCell in R.BorderCells)
			{
				foreach (Cell localAdjacentCell in Z.GetCell(borderCell).GetLocalAdjacentCells())
				{
					if (localAdjacentCell.HasWall() && !Points.Contains(localAdjacentCell.Location))
					{
						Points.Add(localAdjacentCell.Location);
					}
				}
			}
		}
		else if (Hint == "AlongWall")
		{
			foreach (Location2D cell4 in R.Cells)
			{
				Cell cell = Z.GetCell(cell4);
				if (!cell.HasWall())
				{
					continue;
				}
				cell.ForeachLocalAdjacentCell((Cell AC) =>
				{
					if (!AC.HasWall() && !Points.Contains(AC.Location))
					{
						Points.Add(AC.Location);
					}
				});
			}
		}
		else if (Hint.StartsWith("Center"))
		{
			Points.Add(R.Center);
		}
		else if (Hint.StartsWith("RadialFromCenter:"))
		{
			int radius = Convert.ToInt32(Hint.Split(':')[1]);
			List<Location2D> radialPoints = R.Center.GetRadialPoints(radius);
			for (int num = 0; num < radialPoints.Count; num++)
			{
				if (R.PointIn(radialPoints[num]) && !R.BorderCells.Contains(radialPoints[num]))
				{
					Points.Add(radialPoints[num]);
				}
			}
		}
		else
		{
			Points.AddRange(R.NonBorderCells);
		}
		if (!flag)
		{
			Points.RemoveAll((Location2D c) => !Z.GetCell(c).IsEmpty());
		}
		if (Points.Count == 0)
		{
			if (!obj.IsImportant() && obj.HasTagOrProperty("DiscardInsteadOfGeneratingStacked"))
			{
				return true;
			}
			Cell[] emptyCellsShuffled = Z.GetEmptyCellsShuffled();
			if (emptyCellsShuffled.Count() > 0)
			{
				emptyCellsShuffled[0].AddObject(obj);
			}
			else
			{
				List<Cell> list = new List<Cell>(Z.GetCells());
				list.ShuffleInPlace();
				list[0].AddObject(obj);
			}
		}
		else
		{
			Points.ShuffleInPlace();
			Z.GetCell(Points[0]).AddObject(obj);
			if (bAllowCaching)
			{
				Points.Remove(Points[0]);
			}
			Points.Clear();
		}
		return true;
	}
}
