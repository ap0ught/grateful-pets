using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ConsoleLib.Console;
using Genkit;
using Newtonsoft.Json;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Collections;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.ZoneBuilders;
using XRL.World.ZoneBuilders.Utility;
using XRL.World.ZoneParts;

namespace XRL.World;

[Serializable]
[HasWishCommand]
public class Zone
{
	private readonly struct Segment
	{
		public readonly int Head;

		public readonly int Tail;

		public Segment(int Index)
		{
			Head = Index;
			Tail = Index;
		}

		public Segment(int Head, int Tail)
		{
			this.Head = Head;
			this.Tail = Tail;
		}
	}

	public ref struct ObjectEnumerator
	{
		private Zone Zone;

		private Rack<GameObject> Objects;

		private GameObject Object;

		private int X;

		private int Y;

		private int Z;

		public GameObject Current => Object;

		public ObjectEnumerator(Zone Zone)
		{
			this.Zone = Zone;
			Object = null;
			X = (Y = 0);
			Objects = Zone.Map[X][Y].Objects;
			Z = Objects.Count - 1;
		}

		public ObjectEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			while (Z < 0)
			{
				if (++X >= Zone.Width)
				{
					if (++Y >= Zone.Height)
					{
						Object = null;
						return false;
					}
					X = 0;
				}
				Objects = Zone.Map[X][Y].Objects;
				Z = Objects.Count - 1;
			}
			Object = Objects[Z--];
			return true;
		}
	}

	private const int FLOOD_WIDTH = 80;

	private const int FLOOD_HEIGHT = 25;

	private const int ARR_LEN = 2048;

	[NonSerialized]
	private static int FloodValue = 0;

	[NonSerialized]
	public int[,] FloodMap = new int[80, 25];

	[NonSerialized]
	private static Location2D[] FloodArray = new Location2D[4096];

	[NonSerialized]
	private static Segment[] FloodSegments = new Segment[2048];

	[NonSerialized]
	private static Rack<Location2D> FloodVisited;

	[NonSerialized]
	private static Dictionary<Cell, int> FloodNextIndex = new Dictionary<Cell, int>(64);

	[NonSerialized]
	private static List<Cell> FloodNext = new List<Cell>(64);

	[NonSerialized]
	private static List<int> FloodNextRadii = new List<int>(64);

	[NonSerialized]
	private static List<Cell> FloodCurrent = new List<Cell>(64);

	[NonSerialized]
	private static List<int> FloodCurrentRadii = new List<int>(64);

	[NonSerialized]
	private static Dictionary<Cell, int> Flooded = null;

	[NonSerialized]
	private static string InFloodMethod = null;

	public const int NAV_SMART = 1;

	public const int NAV_BURROWER = 2;

	public const int NAV_AUTOEXPLORING = 4;

	public const int NAV_FLYING = 8;

	public const int NAV_WALL_WALKER = 16;

	public const int NAV_IGNORES_WALLS = 32;

	public const int NAV_SWIMMING = 64;

	public const int NAV_SLIMEWALKING = 128;

	public const int NAV_AQUATIC = 256;

	public const int NAV_POLYPWALKING = 512;

	public const int NAV_STRUTWALKING = 1024;

	public const int NAV_JUGGERNAUT = 2048;

	public const int NAV_REEFER = 4096;

	public const int NAV_IGNORE_CREATURES = 8192;

	public const int NAV_IGNORE_GASES = 16384;

	public const int NAV_UNBREATHING = 32768;

	public const int NAV_FILTH_AFFINITY = 65536;

	public const int NAV_OUT_OF_PHASE = 131072;

	public const int NAV_OMNIPHASE = 262144;

	public const int NAV_NULLPHASE = 524288;

	public const int NAV_FLEX_PHASE = 1048576;

	public const int NAV_LAZY_INIT = 268435456;

	public const int NAV_REEFWALKING = 1536;

	public const int REEFER_NAV_WEIGHT = 25;

	public const int NAV_WEIGHT_DETER = 999;

	public const int NAV_WEIGHT_DENY = 9999;

	public const int RANGE_UNLIMITED = 9999;

	public bool Suspended;

	/// <summary>This is a stale reference to a released/frozen zone.</summary>
	[NonSerialized]
	public bool Stale;

	public long LastActive;

	/// <summary>When this zone last entered into the cache, either newly built or thawed.</summary>
	public long LastCached = -1L;

	public long LastPlayerPresence = -1L;

	public string GroundLiquid = "salt-1000";

	[NonSerialized]
	public List<IZonePart> Parts;

	[NonSerialized]
	private static LocationList _locationList;

	private Graveyard _Graveyard;

	[NonSerialized]
	private int _NewTier;

	public string _ZoneID;

	public string ZoneWorld = "";

	public int BaseTemperature = 25;

	public int Tier = 1;

	public int BuildTries;

	public int X;

	public int Y;

	public int Z;

	public int wX;

	public int wY;

	public bool Built;

	public int Width;

	public int Height;

	[NonSerialized]
	public static int[,] SoundWalls = new int[80, 25];

	[NonSerialized]
	public static InfluenceMap SoundMap = new InfluenceMap(80, 25);

	[NonSerialized]
	public static Rack<ISoundRenderer> SoundRenderers = new Rack<ISoundRenderer>();

	public static bool SoundMapDirty = true;

	private static HashSet<Location2D> visited = new HashSet<Location2D>();

	private static Queue<Location2D> horizon = new Queue<Location2D>();

	private static int LastActCount;

	[NonSerialized]
	public LightLevel[] LightMap;

	[NonSerialized]
	public bool[] ExploredMap;

	[NonSerialized]
	public bool[] VisibilityMap;

	[NonSerialized]
	public bool[,] ReachableMap;

	[NonSerialized]
	public NavigationWeight[,] NavigationMap;

	[NonSerialized]
	public List<CachedZoneConnection> ZoneConnectionCache = new List<CachedZoneConnection>();

	[NonSerialized]
	public Cell[][] Map;

	[NonSerialized]
	public MissileMapType[][] MissileMap;

	private string region;

	private static readonly List<Point> LineReturnCache = new List<Point>(16);

	private string _DisplayName;

	[NonSerialized]
	private static CleanQueue<Location2D> CellQueue = new CleanQueue<Location2D>();

	[NonSerialized]
	private static Dictionary<Location2D, bool> VisitedList = new Dictionary<Location2D, bool>(2000);

	[NonSerialized]
	public int RenderedObjects;

	[NonSerialized]
	private static RenderEvent eRender = new RenderEvent();

	[NonSerialized]
	private static RenderSoundEvent eRenderSound = new RenderSoundEvent();

	[NonSerialized]
	private static List<GameObject> WantsToPaint = new List<GameObject>();

	[NonSerialized]
	private List<Cell> CellList;

	[NonSerialized]
	private bool? _isWorldMapCache;

	[NonSerialized]
	public List<IEvent> QueuedEvents;

	[NonSerialized]
	public List<IEvent> QueuedEventsToFire;

	[NonSerialized]
	private static Type[] HandleEventMethodParameterList = new Type[1];

	[NonSerialized]
	private static object[] HandleEventMethodArgumentList1 = new object[1];

	[NonSerialized]
	private static bool HandleEventMethodArgumentList1InUse;

	[NonSerialized]
	private static object[] HandleEventMethodArgumentList2 = new object[1];

	[NonSerialized]
	private static bool HandleEventMethodArgumentList2InUse;

	[NonSerialized]
	private static object[] HandleEventMethodArgumentList3 = new object[1];

	[NonSerialized]
	private static bool HandleEventMethodArgumentList3InUse;

	[NonSerialized]
	private static Dictionary<Type, Dictionary<Type, MethodInfo>> HandleEventLookup = new Dictionary<Type, Dictionary<Type, MethodInfo>>();

	[NonSerialized]
	private static PrimePowerSystemsEvent ePrimePowerSystems = new PrimePowerSystemsEvent();

	[NonSerialized]
	private static SynchronizeExistenceEvent eSynchronizeExistence = new SynchronizeExistenceEvent();

	[NonSerialized]
	private static List<IZonePart> TurnTickParts = new List<IZonePart>();

	[NonSerialized]
	private static bool TurnTickPartsInUse;

	private byte WantTurnTickCache;

	[JsonIgnore]
	public LocationList area
	{
		get
		{
			if (_locationList == null)
			{
				List<Location2D> list = new List<Location2D>();
				for (int i = 0; i < Height; i++)
				{
					for (int j = 0; j < Width; j++)
					{
						list.Add(Location2D.Get(j, i));
					}
				}
				_locationList = new LocationList(list);
			}
			return _locationList;
		}
	}

	public Graveyard Graveyard => _Graveyard ?? (_Graveyard = Graveyard.Get());

	public int Level => NewTier * 5;

	public int NewTier
	{
		get
		{
			if (_NewTier > 0)
			{
				return _NewTier;
			}
			string text = The.ZoneManager.GetZoneProperty(ZoneID, "ZoneTierOverride") as string;
			if (!text.IsNullOrEmpty() && int.TryParse(text, out _NewTier))
			{
				return _NewTier;
			}
			ZoneBlueprint blueprint = GetBlueprint();
			if (blueprint != null && blueprint.Tier > 0)
			{
				return _NewTier = blueprint.Tier;
			}
			text = GetTerrainObject()?.GetTag("RegionTier") ?? "1";
			if (!int.TryParse(text, out _NewTier))
			{
				_NewTier = 1;
			}
			if (Z > 15)
			{
				_NewTier = Math.Abs(Z - 16) / 5 + 2;
			}
			if (_NewTier < 1)
			{
				_NewTier = 1;
			}
			if (_NewTier > 8)
			{
				_NewTier = 8;
			}
			return _NewTier;
		}
	}

	public Location2D ResolvedLocation => Location2D.Get(ResolvedX, ResolvedY);

	public int ResolvedX => wX * 3 + X;

	public int ResolvedY => wY * 3 + Y;

	public int ResolvedStartX => ResolvedX * 80;

	public int ResolvedStartY => ResolvedY * 25;

	public virtual string ZoneID
	{
		get
		{
			return _ZoneID;
		}
		set
		{
			_ZoneID = value;
			XRL.World.ZoneID.Parse(value, out ZoneWorld, out wX, out wY, out X, out Y, out Z);
		}
	}

	public string DisplayName
	{
		get
		{
			return _DisplayName ?? (_DisplayName = The.ZoneManager.GetZoneDisplayName(ZoneID));
		}
		set
		{
			_DisplayName = value;
			The.ZoneManager.SetZoneDisplayName(ZoneID, value, Sync: false);
		}
	}

	public string BaseDisplayName
	{
		get
		{
			return The.ZoneManager.GetZoneBaseDisplayName(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneBaseDisplayName(ZoneID, value);
		}
	}

	public string ReferenceDisplayName => The.ZoneManager.GetZoneReferenceDisplayName(ZoneID);

	public string NameContext
	{
		get
		{
			return The.ZoneManager.GetZoneNameContext(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneNameContext(ZoneID, value);
		}
	}

	public bool HasProperName
	{
		get
		{
			return The.ZoneManager.GetZoneHasProperName(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneHasProperName(ZoneID, value);
		}
	}

	public string IndefiniteArticle
	{
		get
		{
			return The.ZoneManager.GetZoneIndefiniteArticle(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneIndefiniteArticle(ZoneID, value);
		}
	}

	public string DefiniteArticle
	{
		get
		{
			return The.ZoneManager.GetZoneDefiniteArticle(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneDefiniteArticle(ZoneID, value);
		}
	}

	public bool IncludeContextInZoneDisplay
	{
		get
		{
			return The.ZoneManager.GetZoneIncludeContextInZoneDisplay(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneIncludeContextInZoneDisplay(ZoneID, value);
		}
	}

	public bool IncludeStratumInZoneDisplay
	{
		get
		{
			return The.ZoneManager.GetZoneIncludeStratumInZoneDisplay(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneIncludeStratumInZoneDisplay(ZoneID, value);
		}
	}

	public bool NamedByPlayer
	{
		get
		{
			return The.ZoneManager.GetZoneNamedByPlayer(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneNamedByPlayer(ZoneID, value);
		}
	}

	public string AmbientBed
	{
		get
		{
			return The.ZoneManager.GetZoneAmbientBed(ZoneID);
		}
		set
		{
			The.Game.SetStringGameState("ZoneAmbientBed_" + ZoneID, value);
		}
	}

	public string AmbientSounds => The.ZoneManager.GetZoneAmbientSounds(ZoneID);

	public int AmbientVolume
	{
		get
		{
			return The.ZoneManager.GetZoneAmbientVolume(ZoneID);
		}
		set
		{
			The.Game.SetIntGameState("ZoneAmbientVolume_" + ZoneID, value);
		}
	}

	public bool HasWeather
	{
		get
		{
			return The.ZoneManager.GetZoneHasWeather(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneHasWeather(ZoneID, value);
		}
	}

	public string WindSpeed
	{
		get
		{
			return The.ZoneManager.GetZoneWindSpeed(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneWindSpeed(ZoneID, value);
		}
	}

	public string WindDirections
	{
		get
		{
			return The.ZoneManager.GetZoneWindDirections(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneWindDirections(ZoneID, value);
		}
	}

	public string WindDuration
	{
		get
		{
			return The.ZoneManager.GetZoneWindDuration(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneWindDuration(ZoneID, value);
		}
	}

	public int CurrentWindSpeed
	{
		get
		{
			return The.ZoneManager.GetZoneCurrentWindSpeed(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneCurrentWindSpeed(ZoneID, value);
		}
	}

	public string CurrentWindDirection
	{
		get
		{
			return The.ZoneManager.GetZoneCurrentWindDirection(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneCurrentWindDirection(ZoneID, value);
		}
	}

	public long NextWindChange
	{
		get
		{
			return The.ZoneManager.GetZoneNextWindChange(ZoneID);
		}
		set
		{
			The.ZoneManager.SetZoneNextWindChange(ZoneID, value);
		}
	}

	public string DebugName => ZoneID ?? "null-ID zone";

	[Obsolete("Use Suspended")]
	public bool bSuspended
	{
		get
		{
			return Suspended;
		}
		set
		{
			Suspended = value;
		}
	}

	[Obsolete("Use ResolvedLocation")]
	public Location2D resolvedLocation => Location2D.Get(ResolvedX, ResolvedY);

	[Obsolete("Use ResolvedX")]
	public int resolvedX => wX * 3 + X;

	[Obsolete("Use ResolvedY")]
	public int resolvedY => wY * 3 + Y;

	[Obsolete("Use ResolvedStartX")]
	public int resolvedStartX => ResolvedX * 80;

	[Obsolete("Use ResolvedStartY")]
	public int resolvedStartY => ResolvedY * 25;

	public void FloodAutoexplore(Rack<Cell> Cells, Rack<GameObject> Objects, int Course = 0)
	{
		Cells.Clear();
		Objects.Clear();
		GameObject player = The.Player;
		Cell currentCell = player.CurrentCell;
		if (currentCell == null || currentCell.ParentZone != this)
		{
			return;
		}
		if (Course == 0)
		{
			Course = Courses.Ordinal.GetRandomElement();
		}
		Location2D[,] grid = Location2D.Grid;
		Cell[][] map = Map;
		bool[] exploredMap = ExploredMap;
		int[,] floodMap = FloodMap;
		int num = FloodValue + 1;
		int x = currentCell.X;
		int y = currentCell.Y;
		if (num == int.MaxValue)
		{
			num = int.MinValue;
		}
		FloodValue = num;
		floodMap[x, y] = num;
		int num2 = -1;
		int num3 = 0;
		int num4 = 2048;
		int num5 = 2049;
		Location2D[] floodArray = FloodArray;
		Location2D target = (floodArray[num4] = grid[x, y]);
		int nav = CalculateNav(player, Autoexploring: true, Juggernaut: false, IgnoreCreatures: false, IgnoreGases: false, FlexPhase: false, AllowDigging: false);
		bool autoexploreChests = Options.AutoexploreChests;
		string setting = AutoAct.Setting;
		OngoingAction action = AutoAct.Action;
		bool drawFloodExplore = Options.DrawFloodExplore;
		if (drawFloodExplore)
		{
			if (FloodVisited == null)
			{
				FloodVisited = new Rack<Location2D>(2048);
			}
			FloodVisited.Clear();
		}
		Pivot[][] toExpandPivot = Courses.ToExpandPivot;
		Segment[] floodSegments = FloodSegments;
		_ = ref floodSegments[0];
		while (true)
		{
			Location2D location2D;
			if (num2 >= 0)
			{
				location2D = floodArray[num2--];
				Course = location2D.GetCourseFrom(target);
				target = location2D;
				floodSegments[num3++] = new Segment(num4, num5);
				num4 = num5;
			}
			else if (num4 != num5)
			{
				location2D = floodArray[num4++];
			}
			else
			{
				if (num3 <= 0)
				{
					break;
				}
				Segment segment = floodSegments[--num3];
				num4 = segment.Head;
				num5 = segment.Tail;
				if (num4 == num5)
				{
					continue;
				}
				location2D = floodArray[num4++];
				Course = location2D.Ordinal;
			}
			int x2 = location2D.X;
			int y2 = location2D.Y;
			currentCell = map[x2][y2];
			if (drawFloodExplore)
			{
				FloodVisited.Add(location2D);
			}
			GameObject[] array = currentCell.Objects.GetArray();
			int i = 0;
			for (int count = currentCell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = array[i];
				if ((autoexploreChests && gameObject.ShouldAutoexploreAsChest()) || AutoexploreObjectEvent.Check(player, gameObject, setting, action))
				{
					Objects.Add(gameObject);
				}
			}
			if (currentCell.NavigationWeight(player, nav) >= 95)
			{
				continue;
			}
			int num6 = num5;
			Pivot[] array2 = toExpandPivot[Course];
			for (int j = 0; j < array2.Length; j++)
			{
				Pivot pivot = array2[j];
				int num7 = x2 - pivot.X;
				if ((uint)num7 >= 80u)
				{
					continue;
				}
				int num8 = y2 - pivot.Y;
				if ((uint)num8 < 25u && floodMap[num7, num8] != num)
				{
					floodMap[num7, num8] = num;
					if (exploredMap[num7 + num8 * 80])
					{
						floodArray[num5++] = grid[num7, num8];
						continue;
					}
					floodArray[++num2] = grid[num7, num8];
					Cells.Add(map[num7][num8]);
				}
			}
			int k = num6;
			for (int num9 = num5 - 1; k < num9; k++)
			{
				Location2D location2D2 = floodArray[k];
				floodArray[k] = floodArray[num9];
				floodArray[num9--] = location2D2;
			}
		}
		for (int y2 = 0; y2 < 25; y2++)
		{
			for (int x2 = 0; x2 < 80; x2++)
			{
				if (floodMap[x2, y2] == num)
				{
					continue;
				}
				currentCell = map[x2][y2];
				GameObject[] array3 = currentCell.Objects.GetArray();
				int l = 0;
				for (int count2 = currentCell.Objects.Count; l < count2; l++)
				{
					GameObject gameObject2 = array3[l];
					if ((autoexploreChests && gameObject2.ShouldAutoexploreAsChest()) || AutoexploreObjectEvent.Check(player, gameObject2, setting, action))
					{
						Objects.Add(gameObject2);
					}
				}
			}
		}
		if (drawFloodExplore)
		{
			DrawFloodAutoexplore(Cells);
		}
	}

	private void DrawFloodAutoexplore(Rack<Cell> Cells)
	{
		Cell[][] map = Map;
		Rack<Location2D> floodVisited = FloodVisited;
		ScreenBuffer screenBuffer = Popup._ScreenBuffer;
		Stopwatch frameTimer = XRLCore.FrameTimer;
		The.Core.RenderBaseToBuffer(screenBuffer);
		int i = 0;
		for (int count = floodVisited.Count; i < count; i++)
		{
			float num = (float)i / ((float)count * 1f);
			Location2D location2D = floodVisited[i];
			ConsoleChar consoleChar = screenBuffer[location2D];
			Cell cell = map[location2D.X][location2D.Y];
			bool flag = Cells.Contains(cell);
			consoleChar.Foreground = (flag ? The.Color.White : The.Color.Gray);
			consoleChar.Background = Color.HSVToRGB(num, 1f, 1f);
			consoleChar.Detail = Color.HSVToRGB((num + 0.5f) % 1f, 1f, 1f);
			if (cell.GetNavigationWeightFor(The.Player, Autoexploring: true) >= 95)
			{
				GameObject highestRenderLayerObject = cell.GetHighestRenderLayerObject();
				if (highestRenderLayerObject.IsValid() && highestRenderLayerObject.Render != null)
				{
					consoleChar.Tile = highestRenderLayerObject.Render.Tile;
				}
			}
			else if (flag)
			{
				consoleChar.Char = '.';
			}
			else
			{
				consoleChar.Char = ' ';
			}
			long num2 = frameTimer.ElapsedMilliseconds + 1;
			do
			{
				screenBuffer.Draw();
			}
			while (frameTimer.ElapsedMilliseconds < num2);
		}
		floodVisited.Clear();
		screenBuffer.Draw();
		Keyboard.getch();
	}

	public List<Cell> FastFloodNeighbors(int x1, int y1, int Radius)
	{
		List<Cell> list = new List<Cell>();
		FastFloodNeighborsRecurse(x1, y1, x1 - 1, y1, Radius, list);
		FastFloodNeighborsRecurse(x1, y1, x1 + 1, y1, Radius, list);
		FastFloodNeighborsRecurse(x1, y1, x1, y1 + 1, Radius, list);
		FastFloodNeighborsRecurse(x1, y1, x1, y1 - 1, Radius, list);
		return list;
	}

	public void FastFloodNeighborsRecurse(int xs, int ys, int x1, int y1, int Radius, List<Cell> Return)
	{
		if (x1 >= 0 && x1 <= Width - 1 && y1 >= 0 && y1 <= Height - 1 && Math.Abs(xs - x1) <= Radius && Math.Abs(ys - y1) <= Radius && !GetCell(x1, y1).IsOccluding() && !Return.CleanContains(GetCell(x1, y1)))
		{
			Return.Add(GetCell(x1, y1));
			FastFloodNeighborsRecurse(xs, ys, x1 - 1, y1, Radius, Return);
			FastFloodNeighborsRecurse(xs, ys, x1 + 1, y1, Radius, Return);
			FastFloodNeighborsRecurse(xs, ys, x1, y1 + 1, Radius, Return);
			FastFloodNeighborsRecurse(xs, ys, x1, y1 - 1, Radius, Return);
		}
	}

	public List<GameObject> FastFloodSearch(int x1, int y1, int Radius, string SearchPart = null, GameObject Looker = null, Predicate<GameObject> Filter = null)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		FastFloodSearchRecurse(x1, y1, x1 - 1, y1, Radius, SearchPart, Filter, list, Looker);
		FastFloodSearchRecurse(x1, y1, x1 + 1, y1, Radius, SearchPart, Filter, list, Looker);
		FastFloodSearchRecurse(x1, y1, x1, y1 + 1, Radius, SearchPart, Filter, list, Looker);
		FastFloodSearchRecurse(x1, y1, x1, y1 - 1, Radius, SearchPart, Filter, list, Looker);
		FastFloodSearchRecurse(x1, y1, x1 - 1, y1 - 1, Radius, SearchPart, Filter, list, Looker);
		FastFloodSearchRecurse(x1, y1, x1 + 1, y1 + 1, Radius, SearchPart, Filter, list, Looker);
		FastFloodSearchRecurse(x1, y1, x1 - 1, y1 + 1, Radius, SearchPart, Filter, list, Looker);
		FastFloodSearchRecurse(x1, y1, x1 + 1, y1 - 1, Radius, SearchPart, Filter, list, Looker);
		return list;
	}

	private void FastFloodSearchRecurse(int xs, int ys, int x1, int y1, int Radius, string SearchPart, Predicate<GameObject> Filter, List<GameObject> Return, GameObject Looker)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1 || FloodMap[x1, y1] == FloodValue || Math.Abs(xs - x1) > Radius || Math.Abs(ys - y1) > Radius)
		{
			return;
		}
		Cell cell = null;
		FloodMap[x1, y1] = FloodValue;
		if (cell == null)
		{
			cell = GetCell(x1, y1);
		}
		int i = 0;
		for (int count = cell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = cell.Objects[i];
			if ((SearchPart.IsNullOrEmpty() || gameObject.HasPart(SearchPart)) && (Filter == null || Filter(gameObject)))
			{
				Return.Add(gameObject);
			}
		}
		FastFloodSearchRecurse(xs, ys, x1 - 1, y1, Radius, SearchPart, Filter, Return, Looker);
		FastFloodSearchRecurse(xs, ys, x1 + 1, y1, Radius, SearchPart, Filter, Return, Looker);
		FastFloodSearchRecurse(xs, ys, x1, y1 + 1, Radius, SearchPart, Filter, Return, Looker);
		FastFloodSearchRecurse(xs, ys, x1, y1 - 1, Radius, SearchPart, Filter, Return, Looker);
		FastFloodSearchRecurse(xs, ys, x1 - 1, y1 - 1, Radius, SearchPart, Filter, Return, Looker);
		FastFloodSearchRecurse(xs, ys, x1 + 1, y1 + 1, Radius, SearchPart, Filter, Return, Looker);
		FastFloodSearchRecurse(xs, ys, x1 - 1, y1 + 1, Radius, SearchPart, Filter, Return, Looker);
		FastFloodSearchRecurse(xs, ys, x1 + 1, y1 - 1, Radius, SearchPart, Filter, Return, Looker);
	}

	public List<GameObject> FastFloodVisibility(int x1, int y1, int Radius, string SearchPart = null, GameObject Looker = null, Predicate<GameObject> ExtraVisibility = null, Predicate<GameObject> Filter = null)
	{
		Type type = ModManager.ResolveType("XRL.World.Parts." + SearchPart);
		if ((object)type == null)
		{
			throw new TypeUnloadedException(SearchPart);
		}
		return FastFloodVisibility(x1, y1, Radius, type, Looker, ExtraVisibility, Filter);
	}

	public List<GameObject> FastFloodVisibility(int x1, int y1, int Radius, Type SearchPart = null, GameObject Looker = null, Predicate<GameObject> ExtraVisibility = null, Predicate<GameObject> Filter = null)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FastFloodVisibility(x1, y1, Radius, list, SearchPart, Looker, ExtraVisibility, Filter);
		return list;
	}

	public void FastFloodVisibility(int x1, int y1, int Radius, List<GameObject> Return, Type SearchPart = null, GameObject Looker = null, Predicate<GameObject> ExtraVisibility = null, Predicate<GameObject> Filter = null)
	{
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return;
		}
		int num = FloodValue + 1;
		if (num == int.MaxValue)
		{
			num = int.MinValue;
		}
		FloodValue = num;
		Location2D[] floodArray = FloodArray;
		int num2 = 0;
		floodArray[0] = cell.Location;
		Cell[][] map = Map;
		Location2D[,] grid = Location2D.Grid;
		int[,] floodMap = FloodMap;
		LightLevel[] lightMap = LightMap;
		int width = Width;
		int num3 = Math.Max(x1 - Radius, 0);
		int num4 = Math.Min(x1 + Radius, width - 1);
		int num5 = Math.Max(y1 - Radius, 0);
		int num6 = Math.Min(y1 + Radius, Height - 1);
		bool flag = Looker != null && (Looker.HasPart<DarkVision>() || Looker.HasPart<XRL.World.Parts.Mutation.NightVision>());
		while (num2 >= 0)
		{
			Location2D obj = floodArray[num2--];
			int x2 = obj.X;
			int y2 = obj.Y;
			LightLevel lightLevel = lightMap[x2 + y2 * width];
			if (lightLevel == LightLevel.Blackout || (!flag && (int)lightLevel <= 1))
			{
				continue;
			}
			cell = map[x2][y2];
			GameObject[] array = cell.Objects.GetArray();
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = array[i];
				if ((SearchPart == null || gameObject.HasPart(SearchPart)) && (Filter == null || Filter(gameObject)))
				{
					Return.Add(gameObject);
				}
			}
			if (cell.IsOccluding())
			{
				continue;
			}
			bool flag2 = y2 > num5;
			bool flag3 = y2 < num6;
			if (x2 > num3)
			{
				if (floodMap[x2 - 1, y2] != num)
				{
					floodArray[++num2] = grid[x2 - 1, y2];
					floodMap[x2 - 1, y2] = num;
				}
				if (flag2 && floodMap[x2 - 1, y2 - 1] != num)
				{
					floodArray[++num2] = grid[x2 - 1, y2 - 1];
					floodMap[x2 - 1, y2 - 1] = num;
				}
				if (flag3 && floodMap[x2 - 1, y2 + 1] != num)
				{
					floodArray[++num2] = grid[x2 - 1, y2 + 1];
					floodMap[x2 - 1, y2 + 1] = num;
				}
			}
			if (x2 < num4)
			{
				if (floodMap[x2 + 1, y2] != num)
				{
					floodArray[++num2] = grid[x2 + 1, y2];
					floodMap[x2 + 1, y2] = num;
				}
				if (flag2 && floodMap[x2 + 1, y2 - 1] != num)
				{
					floodArray[++num2] = grid[x2 + 1, y2 - 1];
					floodMap[x2 + 1, y2 - 1] = num;
				}
				if (flag3 && floodMap[x2 + 1, y2 + 1] != num)
				{
					floodArray[++num2] = grid[x2 + 1, y2 + 1];
					floodMap[x2 + 1, y2 + 1] = num;
				}
			}
			if (flag2 && floodMap[x2, y2 - 1] != num)
			{
				floodArray[++num2] = grid[x2, y2 - 1];
				floodMap[x2, y2 - 1] = num;
			}
			if (flag3 && floodMap[x2, y2 + 1] != num)
			{
				floodArray[++num2] = grid[x2, y2 + 1];
				floodMap[x2, y2 + 1] = num;
			}
		}
		if (!Options.DrawFloodVis)
		{
			return;
		}
		for (int j = 0; j < 25; j++)
		{
			for (int k = 0; k < 80; k++)
			{
				if (FloodMap[k, j] == num)
				{
					Popup._ScreenBuffer.Goto(k, j);
					Popup._ScreenBuffer.Write("*");
				}
			}
		}
		Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
		Keyboard.getch();
	}

	public bool FastFloodVisibilityAny(int x1, int y1, int Radius, string SearchPart, GameObject Looker = null)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool darkVision = false;
		if (Looker != null && (Looker.HasPart<DarkVision>() || Looker.HasPart<XRL.World.Parts.Mutation.NightVision>()))
		{
			darkVision = true;
		}
		if (!FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1, y1 + 1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1, y1 - 1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1 - 1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1 + 1, Radius, SearchPart, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1 + 1, Radius, SearchPart, Looker, darkVision))
		{
			return FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1 - 1, Radius, SearchPart, Looker, darkVision);
		}
		return true;
	}

	public bool FastFloodVisibilityAnyRecurse(int xs, int ys, int x1, int y1, int Radius, string SearchPart, GameObject Looker, bool DarkVision)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return false;
		}
		if (FloodMap[x1, y1] == FloodValue)
		{
			return false;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return false;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return false;
		}
		LightLevel light = GetLight(x1, y1);
		if (light == LightLevel.Blackout)
		{
			return false;
		}
		if (!DarkVision && (int)light <= 1)
		{
			return false;
		}
		FloodMap[x1, y1] = FloodValue;
		if (GetCell(x1, y1).HasObjectWithPart(SearchPart))
		{
			return true;
		}
		if (GetCell(x1, y1).IsOccluding())
		{
			return false;
		}
		if (!FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1, y1 + 1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1, y1 - 1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1 - 1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1 + 1, Radius, SearchPart, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1 + 1, Radius, SearchPart, Looker, DarkVision))
		{
			return FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1 - 1, Radius, SearchPart, Looker, DarkVision);
		}
		return true;
	}

	public bool FastFloodVisibilityAny(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Looker)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool darkVision = false;
		if (Looker != null && (Looker.HasPart<DarkVision>() || Looker.HasPart<XRL.World.Parts.Mutation.NightVision>()))
		{
			darkVision = true;
		}
		if (!FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1, y1 + 1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1, y1 - 1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Filter, Looker, darkVision) && !FastFloodVisibilityAnyRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Filter, Looker, darkVision))
		{
			return FastFloodVisibilityAnyRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Filter, Looker, darkVision);
		}
		return true;
	}

	public bool FastFloodVisibilityAnyRecurse(int xs, int ys, int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Looker, bool DarkVision)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return false;
		}
		if (FloodMap[x1, y1] == FloodValue)
		{
			return false;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return false;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return false;
		}
		LightLevel light = GetLight(x1, y1);
		if (light == LightLevel.Blackout)
		{
			return false;
		}
		if (!DarkVision && (int)light <= 1)
		{
			return false;
		}
		FloodMap[x1, y1] = FloodValue;
		if (GetCell(x1, y1).HasObject(Filter))
		{
			return true;
		}
		if (GetCell(x1, y1).IsOccluding())
		{
			return false;
		}
		if (!FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1, y1 + 1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1, y1 - 1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Filter, Looker, DarkVision) && !FastFloodVisibilityAnyRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Filter, Looker, DarkVision))
		{
			return FastFloodVisibilityAnyRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Filter, Looker, DarkVision);
		}
		return true;
	}

	public bool FastFloodVisibilityAnyBlueprint(int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleToPlayerOnly = false)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool darkVision = false;
		if (Looker != null && (Looker.HasPart<DarkVision>() || Looker.HasPart<XRL.World.Parts.Mutation.NightVision>()))
		{
			darkVision = true;
		}
		if (!FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 - 1, y1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 + 1, y1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly))
		{
			return FastFloodVisibilityAnyBlueprintRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly);
		}
		return true;
	}

	public bool FastFloodVisibilityAnyBlueprintRecurse(int xs, int ys, int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool DarkVision, bool VisibleToPlayerOnly)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return false;
		}
		if (FloodMap[x1, y1] == FloodValue)
		{
			return false;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return false;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return false;
		}
		LightLevel light = GetLight(x1, y1);
		if (light == LightLevel.Blackout)
		{
			return false;
		}
		if (!DarkVision && (int)light <= 1)
		{
			return false;
		}
		FloodMap[x1, y1] = FloodValue;
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return false;
		}
		if (VisibleToPlayerOnly && !cell.IsVisible())
		{
			return false;
		}
		if ((VisibleToPlayerOnly ? cell.GetFirstObject(Blueprint) : cell.GetFirstVisibleObject(Blueprint)) != null)
		{
			return true;
		}
		if (cell.IsOccluding())
		{
			return false;
		}
		if (!FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 - 1, y1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 + 1, y1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) && !FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly))
		{
			return FastFloodVisibilityAnyBlueprintRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly);
		}
		return true;
	}

	public bool FastFloodFindAnyBlueprint(int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleOnly = false, bool ExploredOnly = false)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		if (!FastFloodFindAnyBlueprintRecurse(x1, y1, x1 - 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1 + 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly))
		{
			return FastFloodFindAnyBlueprintRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly);
		}
		return true;
	}

	public bool FastFloodFindAnyBlueprintRecurse(int xs, int ys, int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleOnly, bool ExploredOnly)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return false;
		}
		if (FloodMap[x1, y1] == FloodValue)
		{
			return false;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return false;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return false;
		}
		FloodMap[x1, y1] = FloodValue;
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return false;
		}
		if (VisibleOnly && !cell.IsVisible())
		{
			return false;
		}
		if (ExploredOnly && !cell.IsExploredFor(Looker))
		{
			return false;
		}
		if ((VisibleOnly ? cell.GetFirstVisibleObject(Blueprint) : cell.GetFirstObject(Blueprint)) != null)
		{
			return true;
		}
		if (cell.IsOccluding())
		{
			return false;
		}
		if (!FastFloodFindAnyBlueprintRecurse(xs, ys, x1 - 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1 + 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) && !FastFloodFindAnyBlueprintRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly))
		{
			return FastFloodFindAnyBlueprintRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly);
		}
		return true;
	}

	public GameObject FastFloodVisibilityFirstBlueprint(int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleToPlayerOnly = false)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool darkVision = false;
		if (Looker != null && (Looker.HasPart<DarkVision>() || Looker.HasPart<XRL.World.Parts.Mutation.NightVision>()))
		{
			darkVision = true;
		}
		return FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 - 1, y1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 + 1, y1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Blueprint, Looker, darkVision, VisibleToPlayerOnly);
	}

	public GameObject FastFloodVisibilityFirstBlueprintRecurse(int xs, int ys, int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool DarkVision, bool VisibleToPlayerOnly)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return null;
		}
		if (FloodMap[x1, y1] == FloodValue)
		{
			return null;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return null;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return null;
		}
		LightLevel light = GetLight(x1, y1);
		if (light == LightLevel.Blackout)
		{
			return null;
		}
		if (!DarkVision && (int)light <= 1)
		{
			return null;
		}
		FloodMap[x1, y1] = FloodValue;
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return null;
		}
		if (VisibleToPlayerOnly && !cell.IsVisible())
		{
			return null;
		}
		GameObject gameObject = (VisibleToPlayerOnly ? cell.GetFirstVisibleObject(Blueprint) : cell.GetFirstObject(Blueprint));
		if (gameObject != null)
		{
			return gameObject;
		}
		if (cell.IsOccluding())
		{
			return null;
		}
		return FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 - 1, y1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 + 1, y1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly) ?? FastFloodVisibilityFirstBlueprintRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Blueprint, Looker, DarkVision, VisibleToPlayerOnly);
	}

	public GameObject FastFloodFindFirstBlueprint(int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleOnly = false, bool ExploredOnly = false)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		return FastFloodFindFirstBlueprintRecurse(x1, y1, x1 - 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 + 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 - 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 + 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 - 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(x1, y1, x1 + 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly);
	}

	public GameObject FastFloodFindFirstBlueprintRecurse(int xs, int ys, int x1, int y1, int Radius, string Blueprint, GameObject Looker, bool VisibleOnly, bool ExploredOnly)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1)
		{
			return null;
		}
		if (FloodMap[x1, y1] == FloodValue)
		{
			return null;
		}
		if (Math.Abs(xs - x1) > Radius)
		{
			return null;
		}
		if (Math.Abs(ys - y1) > Radius)
		{
			return null;
		}
		FloodMap[x1, y1] = FloodValue;
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return null;
		}
		if (VisibleOnly && !cell.IsVisible())
		{
			return null;
		}
		if (ExploredOnly && !cell.IsExploredFor(Looker))
		{
			return null;
		}
		GameObject gameObject = (VisibleOnly ? cell.GetFirstVisibleObject(Blueprint) : cell.GetFirstObject(Blueprint));
		if (gameObject != null)
		{
			return gameObject;
		}
		if (cell.IsOccluding())
		{
			return null;
		}
		return FastFloodFindFirstBlueprintRecurse(xs, ys, x1 - 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 + 1, y1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 - 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 + 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 - 1, y1 + 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly) ?? FastFloodFindFirstBlueprintRecurse(xs, ys, x1 + 1, y1 - 1, Radius, Blueprint, Looker, VisibleOnly, ExploredOnly);
	}

	private void AddFloodNext(int xs, int ys, int x1, int y1, int Radius)
	{
		if (x1 < 0 || x1 > Width - 1 || y1 < 0 || y1 > Height - 1 || FloodMap[x1, y1] == FloodValue || Math.Abs(xs - x1) > Radius || Math.Abs(ys - y1) > Radius)
		{
			return;
		}
		Cell cell = GetCell(x1, y1);
		if (cell == null)
		{
			return;
		}
		if (FloodNextIndex.TryGetValue(cell, out var value))
		{
			if (Radius > FloodNextRadii[value])
			{
				FloodNextRadii[value] = Radius;
			}
		}
		else
		{
			FloodNextIndex[cell] = value;
			FloodNext.Add(cell);
			FloodNextRadii.Add(Radius);
			FloodMap[x1, y1] = FloodValue;
		}
	}

	public List<GameObject> FastFloodAudibility(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Hearer)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool heightenedHearing = false;
		if (Hearer != null)
		{
			HeightenedHearing part = Hearer.GetPart<HeightenedHearing>();
			if (part != null)
			{
				heightenedHearing = true;
				Radius += part.Level;
			}
		}
		FloodNext.Clear();
		FloodNextIndex.Clear();
		FloodNextRadii.Clear();
		AddFloodNext(x1, y1, x1, y1, Radius);
		while (FloodNext.Count > 0)
		{
			FloodCurrent.Clear();
			FloodCurrent.AddRange(FloodNext);
			FloodCurrentRadii.Clear();
			FloodCurrentRadii.AddRange(FloodNextRadii);
			FloodNext.Clear();
			FloodNextIndex.Clear();
			FloodNextRadii.Clear();
			for (int i = 0; i < FloodCurrent.Count; i++)
			{
				FastFloodAudibilityProcess(x1, y1, FloodCurrent[i], FloodCurrentRadii[i], Filter, list, Hearer, heightenedHearing);
			}
		}
		FloodCurrent.Clear();
		FloodCurrentRadii.Clear();
		if (Options.DrawFloodAud)
		{
			for (int j = 0; j < 25; j++)
			{
				for (int k = 0; k < 80; k++)
				{
					if (FloodMap[k, j] == FloodValue)
					{
						Popup._ScreenBuffer.Goto(k, j);
						Popup._ScreenBuffer.Write("*");
					}
				}
			}
			Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
			Keyboard.getch();
		}
		return list;
	}

	public void FastFloodAudibilityProcess(int xs, int ys, Cell C, int Radius, Predicate<GameObject> Filter, List<GameObject> Return, GameObject Hearer, bool HeightenedHearing)
	{
		foreach (GameObject @object in C.Objects)
		{
			if (Filter(@object))
			{
				Return.Add(@object);
			}
		}
		if (C.IsSolid())
		{
			Radius = ((!HeightenedHearing) ? (Radius - 10) : (Radius - 1));
			if (Radius <= 0)
			{
				return;
			}
		}
		AddFloodNext(xs, ys, C.X - 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y - 1, Radius);
	}

	public bool FastFloodAudibilityAny(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Hearer)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool heightenedHearing = false;
		if (Hearer != null)
		{
			HeightenedHearing part = Hearer.GetPart<HeightenedHearing>();
			if (part != null)
			{
				heightenedHearing = true;
				Radius += part.Level;
			}
		}
		FloodNext.Clear();
		FloodNextIndex.Clear();
		FloodNextRadii.Clear();
		AddFloodNext(x1, y1, x1, y1, Radius);
		try
		{
			while (FloodNext.Count > 0)
			{
				FloodCurrent.Clear();
				FloodCurrent.AddRange(FloodNext);
				FloodCurrentRadii.Clear();
				FloodCurrentRadii.AddRange(FloodNextRadii);
				FloodNext.Clear();
				FloodNextIndex.Clear();
				FloodNextRadii.Clear();
				for (int i = 0; i < FloodCurrent.Count; i++)
				{
					if (FastFloodAudibilityAnyProcess(x1, y1, FloodCurrent[i], FloodCurrentRadii[i], Filter, Hearer, heightenedHearing))
					{
						return true;
					}
				}
			}
		}
		finally
		{
			FloodCurrent.Clear();
			FloodCurrentRadii.Clear();
		}
		return false;
	}

	public bool FastFloodAudibilityAnyProcess(int xs, int ys, Cell C, int Radius, Predicate<GameObject> Filter, GameObject Hearer, bool HeightenedHearing)
	{
		if (C.HasObject(Filter))
		{
			return true;
		}
		if (C.IsSolid())
		{
			Radius = ((!HeightenedHearing) ? (Radius - 10) : (Radius - 1));
			if (Radius <= 0)
			{
				return false;
			}
		}
		AddFloodNext(xs, ys, C.X - 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y - 1, Radius);
		return false;
	}

	public List<GameObject> FastFloodOlfaction(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Smeller)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool heightenedSmell = false;
		if (Smeller != null)
		{
			HeightenedSmell part = Smeller.GetPart<HeightenedSmell>();
			if (part != null)
			{
				heightenedSmell = true;
				Radius += part.Level * 4;
			}
		}
		FloodNext.Clear();
		FloodNextIndex.Clear();
		FloodNextRadii.Clear();
		AddFloodNext(x1, y1, x1, y1, Radius);
		while (FloodNext.Count > 0)
		{
			FloodCurrent.Clear();
			FloodCurrent.AddRange(FloodNext);
			FloodCurrentRadii.Clear();
			FloodCurrentRadii.AddRange(FloodNextRadii);
			FloodNext.Clear();
			FloodNextIndex.Clear();
			FloodNextRadii.Clear();
			for (int i = 0; i < FloodCurrent.Count; i++)
			{
				FastFloodOlfactionProcess(x1, y1, FloodCurrent[i], FloodCurrentRadii[i], Filter, list, Smeller, heightenedSmell);
			}
		}
		FloodCurrent.Clear();
		FloodCurrentRadii.Clear();
		if (Options.DrawFloodOlf)
		{
			for (int j = 0; j < 25; j++)
			{
				for (int k = 0; k < 80; k++)
				{
					if (FloodMap[k, j] == FloodValue)
					{
						Popup._ScreenBuffer.Goto(k, j);
						Popup._ScreenBuffer.Write("*");
					}
				}
			}
			Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
			Keyboard.getch();
		}
		return list;
	}

	public void FastFloodOlfactionProcess(int xs, int ys, Cell C, int Radius, Predicate<GameObject> Filter, List<GameObject> Return, GameObject Smeller, bool HeightenedSmell)
	{
		foreach (GameObject @object in C.Objects)
		{
			if (Filter(@object))
			{
				Return.Add(@object);
			}
		}
		if (C.IsSolid())
		{
			if (!HeightenedSmell || !C.HasObjectWithPart("Door"))
			{
				return;
			}
			Radius = Radius / 2 - 1;
			if (Radius <= 0)
			{
				return;
			}
		}
		if (C.IsOccluding() || C.HasWadingDepthLiquid())
		{
			if (!HeightenedSmell)
			{
				Radius /= 2;
			}
			Radius--;
			if (Radius <= 0)
			{
				return;
			}
		}
		AddFloodNext(xs, ys, C.X - 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y - 1, Radius);
	}

	public bool FastFloodOlfactionAny(int x1, int y1, int Radius, Predicate<GameObject> Filter, GameObject Smeller)
	{
		if (++FloodValue == int.MaxValue)
		{
			FloodValue = int.MinValue;
		}
		bool heightenedSmell = false;
		if (Smeller != null)
		{
			HeightenedSmell part = Smeller.GetPart<HeightenedSmell>();
			if (part != null)
			{
				heightenedSmell = true;
				Radius += part.Level * 4;
			}
		}
		FloodNext.Clear();
		FloodNextIndex.Clear();
		FloodNextRadii.Clear();
		AddFloodNext(x1, y1, x1, y1, Radius);
		try
		{
			while (FloodNext.Count > 0)
			{
				FloodCurrent.Clear();
				FloodCurrent.AddRange(FloodNext);
				FloodCurrentRadii.Clear();
				FloodCurrentRadii.AddRange(FloodNextRadii);
				FloodNext.Clear();
				FloodNextIndex.Clear();
				FloodNextRadii.Clear();
				for (int i = 0; i < FloodCurrent.Count; i++)
				{
					if (FastFloodOlfactionAnyProcess(x1, y1, FloodCurrent[i], FloodCurrentRadii[i], Filter, Smeller, heightenedSmell))
					{
						return true;
					}
				}
			}
		}
		finally
		{
			FloodCurrent.Clear();
			FloodCurrentRadii.Clear();
		}
		return false;
	}

	public bool FastFloodOlfactionAnyProcess(int xs, int ys, Cell C, int Radius, Predicate<GameObject> Filter, GameObject Smeller, bool HeightenedSmell)
	{
		if (C.HasObject(Filter))
		{
			return true;
		}
		if (C.IsSolid())
		{
			if (!HeightenedSmell || !C.HasObjectWithPart("Door"))
			{
				return false;
			}
			Radius = Radius / 2 - 1;
			if (Radius <= 0)
			{
				return false;
			}
		}
		if (C.IsOccluding() || C.HasWadingDepthLiquid())
		{
			if (!HeightenedSmell)
			{
				Radius /= 2;
			}
			Radius--;
			if (Radius <= 0)
			{
				return false;
			}
		}
		AddFloodNext(xs, ys, C.X - 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y, Radius);
		AddFloodNext(xs, ys, C.X, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y - 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X - 1, C.Y + 1, Radius);
		AddFloodNext(xs, ys, C.X + 1, C.Y - 1, Radius);
		return false;
	}

	private void InitFlood(string Method, int Radius)
	{
		if (InFloodMethod == null)
		{
			InFloodMethod = Method;
		}
		else
		{
			MetricsManager.LogError("Call to " + Method + "() inside " + InFloodMethod + "(), global flood methods are mutually non-reentrant");
		}
		if (Flooded == null)
		{
			Flooded = new Dictionary<Cell, int>((Radius + 1) * (Radius + 1));
		}
		else
		{
			Flooded.Clear();
		}
	}

	private void DoneFlood()
	{
		Flooded.Clear();
		InFloodMethod = null;
	}

	public List<GameObject> GlobalFloodObjects(int x1, int y1, int Radius, string SearchPart, GameObject Looker, List<GameObject> Return, bool CheckInWalls = false, bool ForFluid = false)
	{
		Return.Clear();
		if (Looker == null)
		{
			return Return;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return Return;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return Return;
		}
		InitFlood("GlobalFloodObjects", Radius);
		GlobalFloodObjectsRecurse(cell, Radius, SearchPart, Return, ForFluid, CheckInWalls, First: true);
		DoneFlood();
		return Return;
	}

	public void GlobalFloodObjectsRecurse(Cell C, int Radius, string SearchPart, List<GameObject> Return, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null || Radius < 0 || (!First && C.IsSolid(ForFluid)))
		{
			return;
		}
		if (!Flooded.ContainsKey(C))
		{
			Flooded.Add(C, 0);
			C.ForeachObjectWithPart(SearchPart, (GameObject o) =>
			{
				Return.Add(o);
			});
		}
		if (Flooded[C] >= Radius)
		{
			return;
		}
		Flooded[C] = Radius;
		int num = Radius - 1;
		if (num > 0)
		{
			string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
			foreach (string direction in directionListCardinalFirst)
			{
				GlobalFloodObjectsRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, Return, ForFluid, CheckInWalls);
			}
			GlobalFloodObjectsRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, Return, ForFluid, CheckInWalls);
			GlobalFloodObjectsRecurse(C.GetReversibleAccessDownCell(), num, SearchPart, Return, ForFluid, CheckInWalls);
		}
	}

	public GameObject GlobalFloodFirstObject(int x1, int y1, int Radius, string SearchPart, GameObject Looker, bool ForFluid = false, bool CheckInWalls = false)
	{
		if (Looker == null)
		{
			return null;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return null;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return null;
		}
		InitFlood("GlobalFloodFirstObject", Radius);
		GameObject result = GlobalFloodFirstObjectRecurse(cell, Radius, SearchPart, ForFluid, CheckInWalls, First: true);
		DoneFlood();
		return result;
	}

	public GameObject GlobalFloodFirstObjectRecurse(Cell C, int Radius, string SearchPart, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null)
		{
			return null;
		}
		if (Radius < 0)
		{
			return null;
		}
		if (!First && C.IsSolid(ForFluid))
		{
			return null;
		}
		if (!Flooded.ContainsKey(C))
		{
			GameObject firstObjectWithPart = C.GetFirstObjectWithPart(SearchPart);
			if (firstObjectWithPart != null)
			{
				return firstObjectWithPart;
			}
			Flooded.Add(C, 0);
		}
		if (Flooded[C] < Radius)
		{
			Flooded[C] = Radius;
			int num = Radius - 1;
			if (num >= 0)
			{
				string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
				GameObject gameObject;
				foreach (string direction in directionListCardinalFirst)
				{
					gameObject = GlobalFloodFirstObjectRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, ForFluid, CheckInWalls);
					if (gameObject != null)
					{
						return gameObject;
					}
				}
				gameObject = GlobalFloodFirstObjectRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, ForFluid, CheckInWalls);
				if (gameObject != null)
				{
					return gameObject;
				}
				gameObject = GlobalFloodFirstObjectRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, ForFluid, CheckInWalls);
				if (gameObject == null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject GlobalFloodFirstObject(int x1, int y1, int Radius, string SearchPart, Predicate<GameObject> pFilter, GameObject Looker, bool ForFluid = false)
	{
		if (Looker == null)
		{
			return null;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return null;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return null;
		}
		InitFlood("GlobalFloodFirstObject", Radius);
		GameObject result = GlobalFloodFirstObjectRecurse(cell, Radius, SearchPart, pFilter, ForFluid, CheckInWalls: true);
		DoneFlood();
		return result;
	}

	public GameObject GlobalFloodFirstObjectRecurse(Cell C, int Radius, string SearchPart, Predicate<GameObject> pFilter, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null)
		{
			return null;
		}
		if (Radius < 0)
		{
			return null;
		}
		bool flag = false;
		if (!First && C.IsSolid(ForFluid))
		{
			if (!CheckInWalls)
			{
				return null;
			}
			flag = true;
		}
		if (!Flooded.ContainsKey(C))
		{
			GameObject firstObjectWithPart = C.GetFirstObjectWithPart(SearchPart, pFilter);
			if (firstObjectWithPart != null)
			{
				return firstObjectWithPart;
			}
			Flooded.Add(C, 0);
		}
		if (flag)
		{
			return null;
		}
		if (Flooded[C] < Radius)
		{
			Flooded[C] = Radius;
			int num = Radius - 1;
			if (num >= 0)
			{
				string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
				GameObject gameObject;
				foreach (string direction in directionListCardinalFirst)
				{
					gameObject = GlobalFloodFirstObjectRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, pFilter, ForFluid, CheckInWalls);
					if (gameObject != null)
					{
						return gameObject;
					}
				}
				gameObject = GlobalFloodFirstObjectRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, pFilter, ForFluid, CheckInWalls);
				if (gameObject != null)
				{
					return gameObject;
				}
				gameObject = GlobalFloodFirstObjectRecurse(C.GetReversibleAccessDownCell(), num, SearchPart, pFilter, ForFluid, CheckInWalls);
				if (gameObject == null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public bool GlobalFloodAny(int x1, int y1, int Radius, string SearchPart, GameObject Looker, bool ForFluid = false, bool CheckInWalls = false)
	{
		if (Looker == null)
		{
			return false;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return false;
		}
		InitFlood("GlobalFloodAny", Radius);
		bool result = GlobalFloodAnyRecurse(cell, Radius, SearchPart, ForFluid, CheckInWalls, First: true);
		DoneFlood();
		return result;
	}

	public bool GlobalFloodAnyRecurse(Cell C, int Radius, string SearchPart, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null)
		{
			return false;
		}
		if (Radius < 0)
		{
			return false;
		}
		bool flag = false;
		if (!First && C.IsSolid(ForFluid))
		{
			if (!CheckInWalls)
			{
				return false;
			}
			flag = true;
		}
		if (!Flooded.ContainsKey(C))
		{
			if (C.HasObjectWithPart(SearchPart))
			{
				return true;
			}
			Flooded.Add(C, 0);
		}
		if (flag)
		{
			return false;
		}
		if (Flooded[C] < Radius)
		{
			Flooded[C] = Radius;
			int num = Radius - 1;
			if (num >= 0)
			{
				string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
				foreach (string direction in directionListCardinalFirst)
				{
					if (GlobalFloodAnyRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, ForFluid, CheckInWalls))
					{
						return true;
					}
				}
				if (GlobalFloodAnyRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, ForFluid, CheckInWalls))
				{
					return true;
				}
				if (GlobalFloodAnyRecurse(C.GetReversibleAccessDownCell(), num, SearchPart, ForFluid, CheckInWalls))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool GlobalFloodAny(int x1, int y1, int Radius, string SearchPart, Predicate<GameObject> pFilter, GameObject Looker, bool ForFluid = false, bool CheckInWalls = false)
	{
		if (Looker == null)
		{
			return false;
		}
		Zone currentZone = Looker.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		Cell cell = currentZone.GetCell(x1, y1);
		if (cell == null)
		{
			return false;
		}
		InitFlood("GlobalFloodAny", Radius);
		bool result = GlobalFloodAnyRecurse(cell, Radius, SearchPart, pFilter, ForFluid, CheckInWalls, First: true);
		DoneFlood();
		return result;
	}

	public bool GlobalFloodAnyRecurse(Cell C, int Radius, string SearchPart, Predicate<GameObject> pFilter, bool ForFluid, bool CheckInWalls, bool First = false)
	{
		if (C == null)
		{
			return false;
		}
		if (Radius < 0)
		{
			return false;
		}
		bool flag = false;
		if (!First && C.IsSolid(ForFluid))
		{
			if (!CheckInWalls)
			{
				return false;
			}
			flag = true;
		}
		if (!Flooded.ContainsKey(C))
		{
			if (C.HasObjectWithPart(SearchPart, pFilter))
			{
				return true;
			}
			Flooded.Add(C, 0);
		}
		if (flag)
		{
			return false;
		}
		if (Flooded[C] < Radius)
		{
			Flooded[C] = Radius;
			int num = Radius - 1;
			if (num >= 0)
			{
				string[] directionListCardinalFirst = Cell.DirectionListCardinalFirst;
				foreach (string direction in directionListCardinalFirst)
				{
					if (GlobalFloodAnyRecurse(C.GetCellFromDirectionGlobalIfBuilt(direction), num, SearchPart, pFilter, ForFluid, CheckInWalls))
					{
						return true;
					}
				}
				if (GlobalFloodAnyRecurse(C.GetReversibleAccessUpCell(), num, SearchPart, pFilter, ForFluid, CheckInWalls))
				{
					return true;
				}
				if (GlobalFloodAnyRecurse(C.GetReversibleAccessDownCell(), num, SearchPart, pFilter, ForFluid, CheckInWalls))
				{
					return true;
				}
			}
		}
		return false;
	}

	[WishCommand("unexplore", null)]
	private static void WishClearExplore()
	{
		The.ActiveZone.ClearExploredMap();
	}

	public virtual int GetTransitionIntervalTo(Zone Z)
	{
		if (Z != this)
		{
			return 1;
		}
		return 0;
	}

	public virtual string GetCheckpointKey()
	{
		return GetCell(0, 0)?.GetFirstObject("CheckpointWidget")?.GetStringProperty("CheckpointKey");
	}

	public virtual bool IsCheckpoint()
	{
		return GetCell(0, 0)?.HasObject("CheckpointWidget") ?? false;
	}

	public void Release()
	{
		Stale = true;
		if (_Graveyard != null)
		{
			Graveyard.Return(ref _Graveyard);
		}
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Cell cell = Map[i][j];
				for (int num = cell.Objects.Count - 1; num >= 0; num--)
				{
					cell.Objects[num].Pool();
				}
				cell.Objects.Clear();
			}
		}
		Array.Clear(FloodArray, 0, FloodArray.Length);
	}

	public void MarkActive()
	{
		LastActive = XRLCore.CurrentTurn;
	}

	public void MarkActive(Zone AlongWith)
	{
		LastActive = XRLCore.CurrentTurn;
		if (AlongWith != null && AlongWith != this)
		{
			AlongWith.MarkActive();
		}
	}

	public string GetDefaultWall()
	{
		string zoneProperty = GetZoneProperty("DefaultWall");
		if (zoneProperty != "")
		{
			return zoneProperty;
		}
		string text = "Shale";
		GameObject terrainObject = GetTerrainObject();
		if (terrainObject != null && Z <= 15)
		{
			text = terrainObject.GetTag("DefaultWall", "Shale");
		}
		if ((text == "Shale" || text == "Limestone") && Z > 49)
		{
			text = "Granite";
		}
		return text;
	}

	public void loadMap(string file)
	{
		MapBuilder mapBuilder = new MapBuilder();
		mapBuilder.ID = file;
		mapBuilder.BuildZone(this);
	}

	public void SetActive()
	{
		The.ZoneManager.SetActiveZone(ZoneID);
	}

	public void SetMusic(string Track)
	{
		GetCell(0, 0).RequireObject("ZoneMusic").SetStringProperty("Track", Track);
	}

	public void PlayMusic()
	{
		GameObject firstObject = GetCell(0, 0).GetFirstObject("ZoneMusic");
		if (firstObject != null && firstObject.TryGetPart<XRL.World.Parts.Music>(out var Part))
		{
			Part.TryMusic();
		}
	}

	public void AddSemanticTag(string newTag)
	{
		string text = GetZoneProperty("SemanticTags");
		if (text != "")
		{
			text += ",";
		}
		text += newTag;
		SetZoneProperty("SemanticTags", text);
	}

	public List<string> GetSemanticTags()
	{
		return GetZoneProperty("SemanticTags", "*Default").Split(',').ToList();
	}

	public string getZoneWorld()
	{
		if (ZoneID == null)
		{
			return null;
		}
		if (!ZoneID.Contains("."))
		{
			return ZoneID;
		}
		return ZoneID.Substring(0, ZoneID.IndexOf('.'));
	}

	public void SetInfluenceMapWalls(int[,] Ret)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.HasWall() || cell.HasObjectWithTag("EnsureVoidBlocker") || cell.HasObjectWithTag("InfluenceMapBlocker"))
				{
					Ret[j, i] = 1;
				}
				else
				{
					Ret[j, i] = 0;
				}
			}
		}
	}

	public void SetInfluenceMapWallsDoors(int[,] Ret)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.HasWall() || cell.HasObjectWithPart("Door"))
				{
					Ret[j, i] = 1;
				}
				else
				{
					Ret[j, i] = 0;
				}
			}
		}
	}

	public void UpdateSoundMap()
	{
		if (!SoundMapDirty)
		{
			return;
		}
		GameObject player = The.Player;
		if (player.CurrentCell == null)
		{
			return;
		}
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = Map[j][i];
				if (cell.HasWall() && cell.IsOccludingFor(player))
				{
					SoundWalls[j, i] = 1;
				}
				else
				{
					SoundWalls[j, i] = 0;
				}
			}
		}
		SoundMap.bDraw = false;
		SoundMap.ClearSeeds();
		SoundMap.Walls = SoundWalls;
		SoundMap.AddSeed(player.CurrentCell.Location, bRecalculate: false);
		SoundMap.RecalculateCostOnly();
		SoundMapDirty = false;
	}

	public int defaultPathfinderWeightFunc(int x, int y, Cell cell)
	{
		if (cell.HasWall())
		{
			return 9999;
		}
		if (cell.IsSolid())
		{
			return 9999;
		}
		return 0;
	}

	public Pathfinder getPathfinder(Func<int, int, Cell, int> weightFunc = null)
	{
		if (weightFunc == null)
		{
			weightFunc = defaultPathfinderWeightFunc;
		}
		Pathfinder pathfinder = Pathfinder.next(Width, Height);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				pathfinder.CurrentNavigationMap[j, i] = weightFunc(j, i, GetCell(j, i));
			}
		}
		return pathfinder;
	}

	public void SetInfluenceMapAutoexploreWeightsAndWalls(int[,] Weights, int[,] Walls, bool ExploredOnly = true)
	{
		int nav = CalculateNav(The.Player, Autoexploring: true);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (ExploredOnly && !GetExplored(j, i))
				{
					Weights[j, i] = 100;
					Walls[j, i] = 1;
				}
				else
				{
					Walls[j, i] = (((Weights[j, i] = GetCell(j, i).NavigationWeight(The.Player, nav)) >= 95) ? 1 : 0);
				}
			}
		}
	}

	public void SetMouseMoveAutoexploreSeeds(InfluenceMap Map, Cell targetCell, out int NumSeeds, out int NumSeedsInBlackout)
	{
		NumSeeds = 0;
		NumSeedsInBlackout = 0;
		visited.Clear();
		horizon.Enqueue(targetCell.Location);
		Zone zone = targetCell.ParentZone;
		while (horizon.Count > 0)
		{
			Location2D location2D = horizon.Dequeue();
			visited.Add(location2D);
			if (!zone.GetCell(location2D).IsExplored())
			{
				int x = location2D.X;
				int y = location2D.Y;
				Map.AddSeed(x, y, bRecalculate: false);
				NumSeeds++;
				if (GetLight(x, y) == LightLevel.Blackout)
				{
					NumSeedsInBlackout++;
				}
				Check(Location2D.Get(x - 1, y));
				Check(Location2D.Get(x + 1, y));
				Check(Location2D.Get(x, y - 1));
				Check(Location2D.Get(x, y + 1));
			}
		}
		void Check(Location2D l)
		{
			if (!(l == null) && !visited.Contains(l) && l.X >= 0 && l.Y >= 0 && l.X < zone.Width && l.Y < zone.Height && !zone.GetCell(l).IsExplored())
			{
				visited.Add(l);
				horizon.Enqueue(l);
			}
		}
	}

	public void SetInfluenceAutoexploreSeeds(InfluenceMap Map, Rack<GameObject> Objects, ref Cell Unexplored, out int NumSeeds, out int NumSeedsInBlackout)
	{
		bool flag = false;
		bool autoexploreChests = Options.AutoexploreChests;
		if (LastActCount != AutoAct.Count)
		{
			LastActCount = AutoAct.Count;
			Objects.Clear();
			Unexplored = null;
			flag = true;
		}
		NumSeeds = 0;
		NumSeedsInBlackout = 0;
		Map.ClearSeeds();
		Cell playerCell = The.PlayerCell;
		int num = 9999999;
		bool flag2 = Unexplored != null && Unexplored.ParentZone == this && ValidCell(Unexplored);
		if (!flag2)
		{
			Unexplored = null;
		}
		if (!Objects.IsNullOrEmpty())
		{
			GameObject[] array = Objects.GetArray();
			for (int num2 = Objects.Count - 1; num2 >= 0; num2--)
			{
				GameObject gameObject = array[num2];
				if (ValidObject(gameObject, this, autoexploreChests))
				{
					Cell currentCell = gameObject.CurrentCell;
					Map.AddSeed(currentCell.X, currentCell.Y, bRecalculate: false);
				}
				else
				{
					Objects.RemoveAt(num2);
				}
			}
			if (Objects.Count == 0)
			{
				flag = true;
			}
		}
		if (!flag2 | flag)
		{
			for (int i = 0; i < Height; i++)
			{
				for (int j = 0; j < Width; j++)
				{
					Cell cell = GetCell(j, i);
					bool flag3 = false;
					if (flag)
					{
						bool flag4 = cell.IsSolidFor(The.Player);
						GameObject[] array2 = cell.Objects.GetArray();
						int k = 0;
						for (int count = cell.Objects.Count; k < count; k++)
						{
							GameObject gameObject2 = array2[k];
							if ((!flag4 || gameObject2.ConsiderSolidFor(The.Player)) && ValidObject(gameObject2, this, autoexploreChests))
							{
								Objects.Add(gameObject2);
								flag3 = true;
								break;
							}
						}
					}
					if (flag3)
					{
						Map.AddSeed(j, i, bRecalculate: false);
					}
					else if (!flag2 && ValidCell(cell))
					{
						int num3 = playerCell.DistanceTo(j, i);
						if (num3 < num)
						{
							Unexplored = cell;
							num = num3;
						}
					}
				}
			}
		}
		if (Unexplored != null)
		{
			Map.AddSeed(Unexplored.Location, bRecalculate: false);
		}
		NumSeeds = Map.Seeds.Count;
		foreach (Location2D seed in Map.Seeds)
		{
			if (GetLight(seed.X, seed.Y) == LightLevel.Blackout)
			{
				NumSeedsInBlackout++;
			}
		}
		static bool ValidCell(Cell Cell)
		{
			if (!Cell.IsExplored())
			{
				if (Cell.HasWall())
				{
					return Cell.HasAdjacentLocalNonwallCell();
				}
				return true;
			}
			return false;
		}
		static bool ValidObject(GameObject Object, Zone Zone, bool ExploreChests)
		{
			if (!Object.IsValid() || Object.CurrentZone != Zone)
			{
				return false;
			}
			if (ExploreChests && Object.ShouldAutoexploreAsChest())
			{
				return true;
			}
			return AutoexploreObjectEvent.Check(The.Player, Object, AutoAct.Setting, AutoAct.Action);
		}
	}

	public int SetInfluenceMapStairsUp(InfluenceMap Map)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				for (int k = 0; k < cell.Objects.Count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (!gameObject.HasPart<XRL.World.Parts.StairsUp>() || gameObject.HasPropertyOrTag("NoAutowalk") || !gameObject.CurrentCell.Explored)
					{
						continue;
					}
					Hidden part = gameObject.GetPart<Hidden>();
					if (part == null || part.Found)
					{
						HiddenRender part2 = gameObject.GetPart<HiddenRender>();
						if (part2 == null || part2.Found)
						{
							Map.AddSeed(j, i, bRecalculate: false);
							num++;
						}
					}
				}
			}
		}
		return num;
	}

	public int SetInfluenceMapStairsDown(InfluenceMap Map)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				for (int k = 0; k < cell.Objects.Count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (!gameObject.HasPart<XRL.World.Parts.StairsDown>() || gameObject.HasPropertyOrTag("NoAutowalk") || !gameObject.CurrentCell.Explored)
					{
						continue;
					}
					Hidden part = gameObject.GetPart<Hidden>();
					if (part == null || part.Found)
					{
						HiddenRender part2 = gameObject.GetPart<HiddenRender>();
						if (part2 == null || part2.Found)
						{
							Map.AddSeed(j, i, bRecalculate: false);
							num++;
						}
					}
				}
			}
		}
		return num;
	}

	public static string XYToID(string World, int xp, int yp, int z)
	{
		int parasangX = xp / 3;
		int parasangY = yp / 3;
		int zoneX = xp % 3;
		int zoneY = yp % 3;
		return XRL.World.ZoneID.Assemble(World, parasangX, parasangY, zoneX, zoneY, z);
	}

	public void FillRoundHollowBox(Box B, string Blueprint)
	{
		if (B.Width < 8 || B.Height < 5)
		{
			for (int i = B.x1 + 1; i <= B.x2 - 1; i++)
			{
				GetCell(i, B.y1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
				GetCell(i, B.y2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			}
			for (int j = B.y1 + 1; j < B.y2; j++)
			{
				GetCell(B.x1, j).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
				GetCell(B.x2, j).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			}
			GetCell(B.x1 + 1, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(B.x2 - 1, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(B.x1 + 1, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(B.x2 - 1, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			return;
		}
		for (int k = B.x1 + 3; k <= B.x2 - 3; k++)
		{
			GetCell(k, B.y1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(k, B.y2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		}
		for (int l = B.y1 + 2; l <= B.y2 - 2; l++)
		{
			GetCell(B.x1, l).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
			GetCell(B.x2, l).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		}
		GetCell(B.x1 + 1, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 2, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 3, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 1, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 2, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 3, B.y1 + 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 1, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 2, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 3, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 1, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 2, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 3, B.y2 - 1).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 1, B.y1 + 2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 1, B.y1 + 2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x1 + 1, B.y2 - 2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
		GetCell(B.x2 - 1, B.y2 - 2).AddObject(GameObjectFactory.Factory.CreateObject(Blueprint));
	}

	public void ClearWalkableBorders()
	{
		try
		{
			string zoneIDFromDirection = GetZoneIDFromDirection("U");
			if (zoneIDFromDirection != null && The.ZoneManager.CachedZones.TryGetValue(zoneIDFromDirection, out var value))
			{
				for (int i = 0; i < value.Height; i++)
				{
					for (int j = 0; j < value.Width; j++)
					{
						Cell cell = value.GetCell(j, i);
						if (cell.HasObjectWithBlueprint("OpenShaft"))
						{
							GetCell(j, i).ClearWalls();
						}
						if (cell.HasObjectWithBlueprint("StairsDown"))
						{
							GetCell(j, i).ClearWalls();
							GetCell(j, i).AddObject("StairsUp");
						}
					}
				}
			}
			string zoneIDFromDirection2 = GetZoneIDFromDirection("D");
			if (zoneIDFromDirection2 != null && The.ZoneManager.CachedZones.TryGetValue(zoneIDFromDirection2, out var value2))
			{
				for (int k = 0; k < value2.Height; k++)
				{
					for (int l = 0; l < value2.Width; l++)
					{
						if (value2.GetCell(l, k).HasObjectWithBlueprint("StairsUp"))
						{
							GetCell(l, k).ClearWalls();
							GetCell(l, k).AddObject("StairsDown");
						}
					}
				}
			}
			string zoneIDFromDirection3 = GetZoneIDFromDirection("N");
			if (zoneIDFromDirection3 != null && The.ZoneManager.CachedZones.TryGetValue(zoneIDFromDirection3, out var value3))
			{
				for (int m = 0; m < value3.Width; m++)
				{
					Cell cell2 = value3.GetCell(m, value3.Height - 1);
					if (cell2 != null && !cell2.IsSolid())
					{
						GetCell(m, 0).ClearWalls();
					}
				}
			}
			string zoneIDFromDirection4 = GetZoneIDFromDirection("S");
			if (zoneIDFromDirection4 != null && The.ZoneManager.CachedZones.TryGetValue(zoneIDFromDirection4, out var value4))
			{
				for (int n = 0; n < value4.Width; n++)
				{
					Cell cell3 = value4.GetCell(n, 0);
					if (cell3 != null && !cell3.IsSolid())
					{
						GetCell(n, Height - 1).ClearWalls();
					}
				}
			}
			string zoneIDFromDirection5 = GetZoneIDFromDirection("E");
			if (zoneIDFromDirection5 != null && The.ZoneManager.CachedZones.TryGetValue(zoneIDFromDirection5, out var value5))
			{
				for (int num = 0; num < value5.Height; num++)
				{
					Cell cell4 = value5.GetCell(0, num);
					if (cell4 != null && !cell4.IsSolid())
					{
						GetCell(Width - 1, num).ClearWalls();
					}
				}
			}
			string zoneIDFromDirection6 = GetZoneIDFromDirection("W");
			if (zoneIDFromDirection6 == null || !The.ZoneManager.CachedZones.TryGetValue(zoneIDFromDirection6, out var value6))
			{
				return;
			}
			for (int num2 = 0; num2 < value6.Height; num2++)
			{
				Cell cell5 = value6.GetCell(0, num2);
				if (cell5 != null && cell5.IsSolid())
				{
					GetCell(0, num2).ClearWalls();
				}
			}
		}
		catch
		{
		}
	}

	public void FillBox(Box B, string Blueprint, bool clearFirst = false)
	{
		for (int i = B.y1; i <= B.y2; i++)
		{
			for (int j = B.x1; j <= B.x2; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null)
				{
					if (clearFirst)
					{
						cell.Clear();
					}
					cell.AddObject(GameObject.Create(Blueprint));
				}
			}
		}
	}

	public void ProcessHollowBox(Box B, Action<Cell> A)
	{
		for (int i = B.x1; i <= B.x2; i++)
		{
			A(GetCell(i, B.y1));
			A(GetCell(i, B.y2));
		}
		for (int j = B.y1 + 1; j < B.y2; j++)
		{
			A(GetCell(B.x1, j));
			A(GetCell(B.x2, j));
		}
	}

	public void FillHollowBox(Box B, string Blueprint)
	{
		for (int i = B.x1; i <= B.x2; i++)
		{
			GetCell(i, B.y1).AddObject(GameObject.Create(Blueprint));
			GetCell(i, B.y2).AddObject(GameObject.Create(Blueprint));
		}
		for (int j = B.y1 + 1; j < B.y2; j++)
		{
			GetCell(B.x1, j).AddObject(GameObject.Create(Blueprint));
			GetCell(B.x2, j).AddObject(GameObject.Create(Blueprint));
		}
	}

	public void Fill(string Blueprint)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).AddObject(GameObject.Create(Blueprint));
			}
		}
	}

	public static void Save(SerializationWriter Writer, Zone Z)
	{
		Type type = Z.GetType();
		Writer.WriteTokenized(type);
		Writer.WriteTypeFields(Z, type);
		Z.Save(Writer);
	}

	public virtual void Save(SerializationWriter Writer)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Map[i][j].Save(Writer);
			}
		}
		int num = LightMap?.Length ?? 0;
		Writer.WriteOptimized(num);
		for (int k = 0; k < num; k++)
		{
			Writer.Write((byte)LightMap[k]);
		}
		Writer.Write(ExploredMap);
		Writer.Write(VisibilityMap);
		int num2 = ReachableMap?.GetLength(0) ?? 0;
		int num3 = ReachableMap?.GetLength(1) ?? 0;
		Writer.WriteOptimized(num2);
		Writer.WriteOptimized(num3);
		for (int l = 0; l < num2; l++)
		{
			for (int m = 0; m < num3; m++)
			{
				Writer.Write(ReachableMap[l, m]);
			}
		}
		num2 = NavigationMap?.GetLength(0) ?? 0;
		num3 = NavigationMap?.GetLength(1) ?? 0;
		Writer.WriteOptimized(num2);
		Writer.WriteOptimized(num3);
		for (int n = 0; n < num2; n++)
		{
			for (int num4 = 0; num4 < num3; num4++)
			{
				Writer.WriteOptimized(NavigationMap[n, num4].Weight);
			}
		}
		num = Parts?.Count ?? 0;
		Writer.WriteOptimized(num);
		for (int num5 = 0; num5 < num; num5++)
		{
			IZonePart.Save(Parts[num5], Writer);
		}
	}

	public static Zone Load(SerializationReader Reader, string ZoneID = null)
	{
		Type type = Reader.ReadTokenizedType();
		Zone zone = Activator.CreateInstance(type, nonPublic: true) as Zone;
		if (!ZoneID.IsNullOrEmpty())
		{
			zone.ZoneID = ZoneID;
		}
		Reader.ReadTypeFields(zone, type);
		zone.Built = false;
		zone.Load(Reader);
		zone.Built = true;
		zone.BroadcastEvent(Event.New("ZoneLoaded"));
		return zone;
	}

	public virtual void Load(SerializationReader Reader)
	{
		FloodMap = new int[80, 25];
		Map = new Cell[Width][];
		MissileMap = new MissileMapType[Width][];
		for (int i = 0; i < Width; i++)
		{
			Map[i] = new Cell[Height];
			MissileMap[i] = new MissileMapType[Height];
		}
		for (int j = 0; j < Width; j++)
		{
			for (int k = 0; k < Height; k++)
			{
				Map[j][k] = Cell.Load(Reader, j, k, this);
			}
		}
		int num = Reader.ReadOptimizedInt32();
		LightMap = new LightLevel[num];
		for (int l = 0; l < num; l++)
		{
			LightMap[l] = (LightLevel)Reader.ReadByte();
		}
		ExploredMap = Reader.ReadBooleanArray();
		if (Reader.FileVersion < 407)
		{
			Reader.ReadBooleanArray();
		}
		VisibilityMap = Reader.ReadBooleanArray();
		int num2 = Reader.ReadOptimizedInt32();
		int num3 = Reader.ReadOptimizedInt32();
		ReachableMap = new bool[num2, num3];
		for (int m = 0; m < num2; m++)
		{
			for (int n = 0; n < num3; n++)
			{
				ReachableMap[m, n] = Reader.ReadBoolean();
			}
		}
		num2 = Reader.ReadOptimizedInt32();
		num3 = Reader.ReadOptimizedInt32();
		NavigationMap = new NavigationWeight[num2, num3];
		for (int num4 = 0; num4 < num2; num4++)
		{
			for (int num5 = 0; num5 < num3; num5++)
			{
				NavigationMap[num4, num5] = new NavigationWeight
				{
					Weight = Reader.ReadOptimizedInt32()
				};
			}
		}
		num = Reader.ReadOptimizedInt32();
		if (num <= 0)
		{
			return;
		}
		Parts = new List<IZonePart>(num);
		for (int num6 = 0; num6 < num; num6++)
		{
			IZonePart zonePart = IZonePart.Load(this, Reader);
			if (zonePart != null)
			{
				Parts.Add(zonePart);
			}
		}
	}

	public string GetRegion()
	{
		if (region == null)
		{
			region = ZoneManager.GetRegionForZone(this);
		}
		return region;
	}

	public virtual ZoneBlueprint GetBlueprint()
	{
		return The.ZoneManager.GetZoneBlueprint(new ZoneRequest(this));
	}

	public virtual WorldBlueprint ResolveWorldBlueprint()
	{
		return WorldFactory.Factory.getWorld(ResolveZoneWorld());
	}

	public virtual Cell GetWorldCell()
	{
		return The.ZoneManager.GetZone(GetZoneWorld())?.GetCell(wX, wY);
	}

	public virtual GameObject GetWorldTerrainObject()
	{
		return GetWorldCell()?.GetFirstObjectWithPart("TerrainTravel")?.GetPart<TerrainTravel>()?.ParentObject;
	}

	public virtual WorldBlueprint GetWorldBlueprint()
	{
		return WorldFactory.Factory.getWorld(ZoneWorld);
	}

	public virtual GameObject GetTerrainObject()
	{
		return ZoneManager.GetTerrainObjectForZone(wX, wY, ZoneWorld);
	}

	public GameObject GetTerrainObjectFromDirection(string dir)
	{
		int num = wX;
		int num2 = wY;
		if (dir.Contains("n", CompareOptions.IgnoreCase))
		{
			num2--;
		}
		if (dir.Contains("s", CompareOptions.IgnoreCase))
		{
			num2++;
		}
		if (dir.Contains("e", CompareOptions.IgnoreCase))
		{
			num++;
		}
		if (dir.Contains("w", CompareOptions.IgnoreCase))
		{
			num--;
		}
		return ZoneManager.GetTerrainObjectForZone(num, num2, ZoneWorld);
	}

	public string GetTerrainNameFromDirection(string dir)
	{
		int num = wX;
		int num2 = wY;
		if (dir.Contains("n", CompareOptions.IgnoreCase))
		{
			num2--;
		}
		if (dir.Contains("s", CompareOptions.IgnoreCase))
		{
			num2++;
		}
		if (dir.Contains("e", CompareOptions.IgnoreCase))
		{
			num++;
		}
		if (dir.Contains("w", CompareOptions.IgnoreCase))
		{
			num--;
		}
		GameObject terrainObjectForZone = ZoneManager.GetTerrainObjectForZone(num, num2, ZoneWorld);
		if (terrainObjectForZone == null)
		{
			return "";
		}
		return terrainObjectForZone.Blueprint;
	}

	public string GetTerrainDisplayName(bool Stripped = true)
	{
		if (Z > 10)
		{
			return "the deep underground";
		}
		return GetTerrainObject()?.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped);
	}

	public string GetTerrainRegion()
	{
		string text = GetZoneProperty("Region");
		if (text.IsNullOrEmpty())
		{
			GameObject terrainObject = GetTerrainObject();
			if (GameObject.Validate(terrainObject))
			{
				text = terrainObject.GetTagOrStringProperty("Terrain");
			}
		}
		return text;
	}

	public void ReplaceAll(string blueprint, string with)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				List<GameObject> objects = GetCell(j, i).GetObjects(blueprint);
				if (objects == null || objects.Count <= 0)
				{
					continue;
				}
				foreach (GameObject item in objects)
				{
					item.Destroy();
				}
				GetCell(j, i).AddObject(with);
			}
		}
	}

	public GameObject FindObjectByID(string id)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject gameObject = GetCell(j, i).FindObjectByID(id);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindObjectByID(int ID)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject gameObject = GetCell(j, i).FindObjectByID(ID);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public int GetObjectCount(string Blueprint)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				num += GetCell(j, i).GetObjectCount(Blueprint);
			}
		}
		return num;
	}

	public GameObject FindFirstObject(string Blueprint)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject firstObject = GetCell(j, i).GetFirstObject(Blueprint);
				if (firstObject != null)
				{
					return firstObject;
				}
			}
		}
		return null;
	}

	public GameObject FindFirstObject(Predicate<GameObject> Filter)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject firstObject = GetCell(j, i).GetFirstObject(Filter);
				if (firstObject != null)
				{
					return firstObject;
				}
			}
		}
		return null;
	}

	public void FindObjects(List<GameObject> Store, Predicate<GameObject> Filter)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).FindObjects(Store, Filter);
			}
		}
	}

	public List<GameObject> FindObjects(Predicate<GameObject> Filter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FindObjects(list, Filter);
		return list;
	}

	public void FindObjects(List<GameObject> List, string Blueprint)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).GetObjects(Blueprint, List);
			}
		}
	}

	public List<GameObject> FindObjects(string Blueprint)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FindObjects(list, Blueprint);
		return list;
	}

	public void FindObjectsWithTagOrProperty(List<GameObject> List, string Name)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).GetObjectsWithTagOrProperty(List, Name);
			}
		}
	}

	public List<GameObject> FindObjectsWithTagOrProperty(string Name)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FindObjectsWithTagOrProperty(list, Name);
		return list;
	}

	public void FindObjectsWithPart(List<GameObject> List, string Name)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GetCell(j, i).GetObjectsWithPart(Name, List);
			}
		}
	}

	public List<GameObject> FindObjectsWithPart(string Name)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FindObjectsWithPart(list, Name);
		return list;
	}

	public bool HasObject(GameObject Object)
	{
		return Object.CurrentZone == this;
	}

	public bool HasObjectWithTag(string Tag)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (GetCell(j, i).HasObjectWithTag(Tag))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectWithTagOrProperty(string Tag)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (GetCell(j, i).HasObjectWithTagOrProperty(Tag))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObject(Predicate<GameObject> Test)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (GetCell(j, i).HasObject(Test))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObject(string Blueprint)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (GetCell(j, i).HasObject(Blueprint))
				{
					return true;
				}
			}
		}
		return false;
	}

	public GameObject FindObject(Predicate<GameObject> Test)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject gameObject = GetCell(j, i).FindObject(Test);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindObject(string Blueprint)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				GameObject gameObject = GetCell(i, j).FindObject(Blueprint);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindObjectExcludingSelf(string Blueprint, GameObject self)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject gameObject = GetCell(j, i).FindObjectExcept(Blueprint, self);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindClosestObject(GameObject Source, Predicate<GameObject> Predicate, bool ExploredOnly = false, bool VisibleOnly = true, bool IncludeSelf = true)
	{
		int num = 9999999;
		GameObject result = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (ExploredOnly && !cell.Explored)
				{
					continue;
				}
				int num2 = cell.PathDistanceTo(Source.CurrentCell);
				if (num2 >= num)
				{
					continue;
				}
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (Predicate(gameObject) && (IncludeSelf || gameObject != Source) && (!VisibleOnly || (gameObject.Render != null && gameObject.Render.Visible)) && !gameObject.HasPropertyOrTag("NoAutowalk") && gameObject.CurrentCell != null)
					{
						num = num2;
						result = gameObject;
					}
				}
			}
		}
		return result;
	}

	public GameObject FindClosestObject(GameObject Source, string Blueprint, bool ExploredOnly = false, bool VisibleOnly = true, bool IncludeSelf = true)
	{
		int num = 9999999;
		GameObject result = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (ExploredOnly && !cell.Explored)
				{
					continue;
				}
				int num2 = cell.PathDistanceTo(Source.CurrentCell);
				if (num2 >= num)
				{
					continue;
				}
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (!(gameObject.Blueprint != Blueprint) && (IncludeSelf || gameObject != Source) && (!VisibleOnly || (gameObject.Render != null && gameObject.Render.Visible)) && !gameObject.HasPropertyOrTag("NoAutowalk") && gameObject.CurrentCell != null)
					{
						num = num2;
						result = gameObject;
					}
				}
			}
		}
		return result;
	}

	public GameObject FindClosestObjectWithPart(GameObject Source, string Part, bool ExploredOnly = false, bool IncludeSelf = true)
	{
		int num = 9999;
		GameObject result = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (!gameObject.HasPart(Part) || (!IncludeSelf && gameObject == Source) || gameObject.HasPropertyOrTag("NoAutowalk") || gameObject.CurrentCell == null || gameObject.CurrentCell.PathDistanceTo(Source.CurrentCell) >= num || (ExploredOnly && !gameObject.CurrentCell.Explored))
					{
						continue;
					}
					Hidden part = gameObject.GetPart<Hidden>();
					if (part == null || part.Found)
					{
						HiddenRender part2 = gameObject.GetPart<HiddenRender>();
						if (part2 == null || part2.Found)
						{
							num = gameObject.CurrentCell.PathDistanceTo(Source.CurrentCell);
							result = gameObject;
						}
					}
				}
			}
		}
		return result;
	}

	public GameObject FindClosestObjectWithTag(GameObject Source, string Tag, bool ExploredOnly = false)
	{
		int num = 9999;
		GameObject result = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (!gameObject.HasTag(Tag) || gameObject.HasPropertyOrTag("NoAutowalk") || gameObject.CurrentCell == null || gameObject.CurrentCell.PathDistanceTo(Source.CurrentCell) >= num || (ExploredOnly && !gameObject.CurrentCell.Explored))
					{
						continue;
					}
					Hidden part = gameObject.GetPart<Hidden>();
					if (part == null || part.Found)
					{
						HiddenRender part2 = gameObject.GetPart<HiddenRender>();
						if (part2 == null || part2.Found)
						{
							num = gameObject.CurrentCell.PathDistanceTo(Source.CurrentCell);
							result = gameObject;
						}
					}
				}
			}
		}
		return result;
	}

	public List<GameObject> GetObjects()
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				num += Map[j][i].GetObjectCount();
			}
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					list.AddRange(Map[l][k].Objects);
					if (list.Count >= num)
					{
						goto end_IL_0081;
					}
				}
				continue;
				end_IL_0081:
				break;
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsReadonly()
	{
		List<GameObject> list = Event.NewGameObjectList();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].GetObjects(list);
			}
		}
		return list;
	}

	public List<GameObject> GetObjects(string Blueprint)
	{
		List<Cell> cellsWithObject = GetCellsWithObject(Blueprint);
		int num = 0;
		foreach (Cell item in cellsWithObject)
		{
			num += item.GetObjectCount(Blueprint);
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			foreach (Cell item2 in cellsWithObject)
			{
				list.AddRange(item2.GetObjects(Blueprint));
				if (list.Count >= num)
				{
					break;
				}
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsReadonly(string Blueprint)
	{
		List<GameObject> list = Event.NewGameObjectList();
		foreach (Cell cell in GetCells())
		{
			cell.GetObjects(Blueprint, list);
		}
		return list;
	}

	public IEnumerable<GameObject> YieldObjects()
	{
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				for (int i = 0; i < Map[x][y].Objects.Count; i++)
				{
					yield return Map[x][y].Objects[i];
				}
			}
		}
	}

	public ObjectEnumerator IterateObjects()
	{
		return new ObjectEnumerator(this);
	}

	public int CountObjects(Predicate<GameObject> pFilter)
	{
		int num = 0;
		foreach (Cell cell in GetCells())
		{
			foreach (GameObject @object in cell.Objects)
			{
				if (@object != null && pFilter(@object))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int CountObjects(string Blueprint)
	{
		int num = 0;
		foreach (Cell cell in GetCells())
		{
			foreach (GameObject @object in cell.Objects)
			{
				if (@object != null && @object.Blueprint == Blueprint)
				{
					num++;
				}
			}
		}
		return num;
	}

	public List<GameObject> GetObjects(Predicate<GameObject> Filter)
	{
		List<Cell> cellsWithObject = GetCellsWithObject(Filter);
		int num = 0;
		foreach (Cell item in cellsWithObject)
		{
			num += item.GetObjectCount(Filter);
		}
		List<GameObject> list = new List<GameObject>(num);
		if (num > 0)
		{
			foreach (Cell item2 in cellsWithObject)
			{
				list.AddRange(item2.GetObjects(Filter));
				if (list.Count >= num)
				{
					break;
				}
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsReadonly(Predicate<GameObject> Filter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		foreach (Cell cell in GetCells())
		{
			cell.GetObjects(Filter, list);
		}
		return list;
	}

	public static Location2D zoneIDTo240x72Location(string zoneID)
	{
		zoneID.AsDelimitedSpans('.', out var _, out var Second, out var Third, out var Fourth, out var Fifth);
		try
		{
			return Location2D.Get(int.Parse(Second) * 3 + int.Parse(Fourth), int.Parse(Third) * 3 + int.Parse(Fifth));
		}
		catch (FormatException)
		{
			MetricsManager.LogError("Error parsing spans for ZoneID '" + zoneID + "': PX = " + Second.ToString() + ", PY = " + Third.ToString() + ", ZX = " + Fourth.ToString() + ", ZY = " + Fifth.ToString());
			XRL.World.ZoneID.Parse(zoneID, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY);
			return Location2D.Get(ParasangX * 3 + ZoneX, ParasangY * 3 + ZoneY);
		}
	}

	public List<GameObject> GetObjectsThatInheritFrom(string Blueprint)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				list.AddRange(Map[i][j].GetObjectsThatInheritFrom(Blueprint));
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTagOrProperty(string Property, bool UseEventList = false)
	{
		List<GameObject> list = (UseEventList ? Event.NewGameObjectList() : new List<GameObject>());
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				list.AddRange(Map[i][j].GetObjectsWithTagOrProperty(Property));
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithProperty(string Property)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].GetObjectsWithProperty(Property, list);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithPart(string Part)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].GetObjectsWithPart(Part, list);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithPartReadonly(string Part)
	{
		List<GameObject> list = Event.NewGameObjectList();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].GetObjectsWithPart(Part, list);
			}
		}
		return list;
	}

	public GameObject GetFirstObjectWithPart(string Part)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject firstObjectWithPart = Map[j][i].GetFirstObjectWithPart(Part);
				if (firstObjectWithPart != null)
				{
					return firstObjectWithPart;
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectWithPropertyOrTag(string Name)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject firstObjectWithPropertyOrTag = Map[j][i].GetFirstObjectWithPropertyOrTag(Name);
				if (firstObjectWithPropertyOrTag != null)
				{
					return firstObjectWithPropertyOrTag;
				}
			}
		}
		return null;
	}

	public IEnumerable<GameObject> LoopObjectsWithPart(string Part)
	{
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				foreach (GameObject item in Map[x][y].LoopObjectsWithPart(Part))
				{
					yield return item;
				}
			}
		}
	}

	public void ForeachObjectWithPart(string Part, Action<GameObject> aProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].ForeachObjectWithPart(Part, aProc);
			}
		}
	}

	public void ForeachObject(Action<GameObject> aProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].ForeachObject(aProc);
			}
		}
	}

	public bool HasBuilder(string builderName)
	{
		return The.ZoneManager.ZoneHasBuilder(ZoneID, builderName);
	}

	public bool HasTryingToJoinPartyLeader()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasTryingToJoinPartyLeader())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasTryingToJoinPartyLeaderForZoneUncaching()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasTryingToJoinPartyLeaderForZoneUncaching())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasPlayerLed()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasPlayerLed())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasWasPlayer()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasWasPlayer())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasLeftBehindByPlayer()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasLeftBehindByPlayer())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForeachObjectWithTagOrProperty(string Name, Action<GameObject> aProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].ForeachObjectWithTagOrProperty(Name, aProc);
			}
		}
	}

	public GameObject GetObjectWithTag(string tag)
	{
		List<GameObject> objectsWithTag = GetObjectsWithTag(tag);
		if (objectsWithTag != null && objectsWithTag.Count > 0)
		{
			return objectsWithTag.GetRandomElement();
		}
		return null;
	}

	public List<GameObject> GetObjectsWithTag(string Tag)
	{
		List<GameObject> list = Event.NewGameObjectList();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				list.AddRange(Map[j][i].GetObjectsWithTag(Tag));
			}
		}
		return list;
	}

	public GameObject GetAnyObjectWithTag(string Tag)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				GameObject objectWithTag = Map[j][i].GetObjectWithTag(Tag);
				if (objectWithTag != null)
				{
					return objectWithTag;
				}
			}
		}
		return null;
	}

	public List<Point> LineFromAngle(int x0, int y0, int degrees)
	{
		float num = (float)degrees / 58f;
		float num2 = (float)Math.Sin(num);
		float num3 = (float)Math.Cos(num);
		List<Point> list = new List<Point>();
		char displaychar = '2';
		float num4 = x0;
		float num5 = y0;
		int num6 = -1;
		int num7 = -1;
		for (int i = 0; i < 10000; i++)
		{
			int num8 = (int)Math.Round(num4, MidpointRounding.AwayFromZero);
			int num9 = (int)Math.Round(num5, MidpointRounding.AwayFromZero);
			if (num8 < 0 || num9 < 0 || num8 >= Width || num9 >= Height)
			{
				break;
			}
			if (num8 != num6 || num9 != num7)
			{
				list.Add(new Point(num8, num9, 0, displaychar));
				if (num6 != -1 && num7 != -1)
				{
					if (num8 != num6 && num9 != num7)
					{
						displaychar = (((num8 <= num6 || num9 <= num7) && (num8 >= num6 || num9 >= num7)) ? '\\' : '/');
					}
					else
					{
						displaychar = ((num8 == num6) ? '|' : '-');
					}
				}
				num6 = num8;
				num7 = num9;
				num4 += num2;
				num5 += num3;
			}
		}
		list[0].DisplayChar = '@';
		list[list.Count - 1].DisplayChar = 'X';
		return list;
	}

	public static List<Point> Line(int x0, int y0, int x1, int y1, bool ReadOnly = false)
	{
		List<Point> list = (ReadOnly ? LineReturnCache : new List<Point>(Math.Abs(y1 - y0) + Math.Abs(x1 - x0) + 1));
		Line(x0, y0, x1, y1, list);
		return list;
	}

	public static void Line(int x0, int y0, int x1, int y1, List<Point> Return)
	{
		bool flag = false;
		Return.Clear();
		if (x0 == x1 && y0 == y1)
		{
			Return.Add(new Point(x0, y0, 0, 'X'));
		}
		else
		{
			bool flag2 = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (flag2)
			{
				int num = x0;
				x0 = y0;
				y0 = num;
				int num2 = x1;
				x1 = y1;
				y1 = num2;
			}
			if (x0 > x1)
			{
				flag = true;
				int num3 = x1;
				x1 = x0;
				x0 = num3;
				int num4 = y1;
				y1 = y0;
				y0 = num4;
			}
			int num5 = x1 - x0;
			int num6 = Math.Abs(y1 - y0);
			char displaychar = '2';
			double num7 = 0.0;
			double num8 = (double)num6 / (double)num5;
			int num9 = 0;
			int num10 = y0;
			num9 = ((y0 < y1) ? 1 : (-1));
			int num11 = 0;
			for (int i = x0; i <= x1; i++)
			{
				num11++;
				if (flag2)
				{
					Return.Add(new Point(num10, i, 0, displaychar));
				}
				else
				{
					Return.Add(new Point(i, num10, 0, displaychar));
				}
				num7 += num8;
				if (num7 >= 0.5)
				{
					num10 += num9;
					num7--;
					displaychar = ((num9 >= 0) ? '\\' : '/');
				}
				else
				{
					displaychar = ((!flag2) ? '-' : '|');
				}
			}
		}
		if (flag)
		{
			Return.Reverse();
		}
		Return[0].DisplayChar = '@';
		Return[Return.Count - 1].DisplayChar = 'X';
	}

	public string GetDisplayName(bool WithIndefiniteArticle = false, bool WithDefiniteArticle = false, bool WithStratum = true, bool Mutate = true)
	{
		return The.ZoneManager.GetZoneDisplayName(ZoneID, WithIndefiniteArticle, WithDefiniteArticle, WithStratum, Mutate);
	}

	public bool IsOutside()
	{
		return !IsInside();
	}

	public bool IsInside()
	{
		if (The.ZoneManager.TryGetZoneProperty<string>(ZoneID, "Inside", out var Value))
		{
			return Value == "1";
		}
		if (!IsWorldMap())
		{
			return Z > 10;
		}
		return false;
	}

	public void SetInside(bool? State)
	{
		if (!State.HasValue)
		{
			The.ZoneManager.RemoveZoneProperty(ZoneID, "Inside");
		}
		else
		{
			The.ZoneManager.SetZoneProperty(ZoneID, "Inside", State.Value ? "1" : "0");
		}
	}

	public string SpecialUpMessage()
	{
		return GetZoneProperty("SpecialUpMessage");
	}

	public bool HasZoneProperty(string name)
	{
		return The.ZoneManager.GetZoneProperty(ZoneID, name) != null;
	}

	public void SetZoneProperty(string Name, string Value)
	{
		The.ZoneManager.SetZoneProperty(ZoneID, Name, Value);
	}

	public string GetZoneProperty(string Name, string Default = "")
	{
		return The.ZoneManager.GetZoneProperty(ZoneID, Name, bClampToLevel30: false, Default) as string;
	}

	public void RemoveZoneProperty(string Name)
	{
		The.ZoneManager.RemoveZoneProperty(ZoneID, Name);
	}

	public void ClearZoneConnectionCache()
	{
		ZoneConnectionCache.Clear();
	}

	public void WriteZoneConnectionCache()
	{
		foreach (CachedZoneConnection item in ZoneConnectionCache)
		{
			AddZoneConnection(item.TargetDirection, item.X, item.Y, item.Type, item.Object);
		}
		ZoneConnectionCache.Clear();
	}

	public void CacheZoneConnection(string TargetZoneDirection, Location2D location, string Type, string ConnectionObject = null)
	{
		CacheZoneConnection(TargetZoneDirection, location.X, location.Y, Type, ConnectionObject);
	}

	public void CacheZoneConnection(string TargetZoneDirection, int X, int Y, string Type, string ConnectionObject)
	{
		ZoneConnectionCache.Add(new CachedZoneConnection(TargetZoneDirection, X, Y, Type, ConnectionObject));
	}

	public void CacheZoneConnection(string TargetZoneDirection, Point P, string Type, string ConnectionObject)
	{
		ZoneConnectionCache.Add(new CachedZoneConnection(TargetZoneDirection, P.X, P.Y, Type, ConnectionObject));
	}

	public void AddZoneConnection(string TargetZoneDirection, int X, int Y, string Type, string ConnectionObject)
	{
		if (!Built)
		{
			CacheZoneConnection(TargetZoneDirection, X, Y, Type, ConnectionObject);
		}
		else
		{
			The.ZoneManager.AddZoneConnection(ZoneID, TargetZoneDirection, X, Y, Type, ConnectionObject);
		}
	}

	public void RemoveZoneConnection(string TargetZoneDirection, int X, int Y, string Type, string ConnectionObject)
	{
		if (!Built)
		{
			for (int num = ZoneConnectionCache.Count - 1; num >= 0; num--)
			{
				CachedZoneConnection cachedZoneConnection = ZoneConnectionCache[num];
				if (cachedZoneConnection.X == X && cachedZoneConnection.Y == Y && !(cachedZoneConnection.TargetDirection != TargetZoneDirection) && !(cachedZoneConnection.Type != Type) && !(cachedZoneConnection.Object != ConnectionObject))
				{
					ZoneConnectionCache.RemoveAt(num);
				}
			}
		}
		else
		{
			The.ZoneManager.RemoveZoneConnection(ZoneID, TargetZoneDirection, X, Y, Type, ConnectionObject);
		}
	}

	public IEnumerable<ZoneConnection> EnumerateConnections()
	{
		foreach (ZoneConnection zoneConnection in The.ZoneManager.GetZoneConnections(ZoneID))
		{
			yield return zoneConnection;
		}
		if (ZoneConnectionCache == null)
		{
			yield break;
		}
		foreach (CachedZoneConnection item in ZoneConnectionCache)
		{
			if (item.TargetDirection == "-")
			{
				yield return item;
			}
		}
	}

	public Zone()
	{
	}

	public Zone(int _Width, int _Height)
	{
		Width = _Width;
		Height = _Height;
		Map = new Cell[Width][];
		MissileMap = new MissileMapType[Width][];
		for (int i = 0; i < Width; i++)
		{
			Map[i] = new Cell[Height];
			MissileMap[i] = new MissileMapType[Height];
			for (int j = 0; j < Height; j++)
			{
				Map[i][j] = new Cell(this);
				Map[i][j].X = i;
				Map[i][j].Y = j;
				Map[i][j].Live = true;
				MissileMap[i][j] = MissileMapType.Empty;
			}
		}
		ExploredMap = new bool[Width * Height];
		VisibilityMap = new bool[Width * Height];
		ReachableMap = new bool[Width, Height];
		LightMap = new LightLevel[Width * Height];
		NavigationMap = new NavigationWeight[Width, Height];
		for (int k = 0; k < Height; k++)
		{
			for (int l = 0; l < Width; l++)
			{
				NavigationMap[l, k] = new NavigationWeight();
			}
		}
		ClearReachableMap(bValue: true);
	}

	public void Clear(string BlueprintsToKeep = null)
	{
		ClearBox(new Box(0, 0, Width - 1, Height - 1), BlueprintsToKeep);
	}

	public void Clear(IEnumerable<Location2D> cells, string blueprint = null)
	{
		foreach (Location2D cell in cells)
		{
			GetCell(cell)?.Clear(blueprint);
		}
	}

	public void ClearBox(Box B, Action<Cell> after = null)
	{
		ClearBox(B, null, null, after);
	}

	public void ClearBox(Rect2D R)
	{
		ClearBox(new Box(R.x1, R.y1, R.x2, R.y2));
	}

	public void ClearBoxWith(Rect2D rect, string blueprint, Action<Cell> after = null)
	{
		ClearBox(new Box(rect.x1, rect.y1, rect.x2, rect.y2), null, blueprint, after);
	}

	public void ClearBox(Box B, string BlueprintsToKeep, string with = null, Action<Cell> after = null)
	{
		for (int i = B.y1; i <= B.y2; i++)
		{
			for (int j = B.x1; j <= B.x2; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell == null)
				{
					continue;
				}
				if (!cell.HasObject(The.Player))
				{
					if (BlueprintsToKeep == null)
					{
						cell.Clear();
						if (with != null)
						{
							GetCell(j, i).AddObject(with);
						}
					}
					else
					{
						List<GameObject> list = Event.NewGameObjectList();
						foreach (GameObject @object in cell.Objects)
						{
							if ((BlueprintsToKeep == null || !BlueprintsToKeep.Contains(@object.Blueprint)) && @object.CanClear())
							{
								list.Add(@object);
							}
						}
						foreach (GameObject item in list)
						{
							cell.Objects.Remove(item);
						}
						if (with != null)
						{
							cell.AddObject(with);
						}
					}
				}
				after?.Invoke(cell);
			}
		}
	}

	public static bool ColorsVisible(LightLevel Light)
	{
		switch (Light)
		{
		case LightLevel.Darkvision:
		case LightLevel.Light:
		case LightLevel.Interpolight:
		case LightLevel.LitRadar:
		case LightLevel.Omniscient:
			return true;
		default:
			return false;
		}
	}

	public void ClearLightMap()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			LightMap[i] = LightLevel.None;
		}
	}

	public void ClearVisiblityMap()
	{
		Array.Clear(VisibilityMap, 0, Width * Height);
	}

	public void ClearExploredMap()
	{
		Array.Clear(ExploredMap, 0, Width * Height);
	}

	[Obsolete("Legacy amnesia behaviour, use ClearExploredMap")]
	public void ClearFakeUnexploredMap()
	{
	}

	public void SetReachable(int x, int y, bool reachable = true)
	{
		ReachableMap[x, y] = reachable;
	}

	public void SetLight(int x, int y, LightLevel v)
	{
		LightMap[x + y * Width] = v;
	}

	public void SetVisibility(int x, int y, bool v)
	{
		VisibilityMap[x + y * Width] = v;
		if (v)
		{
			ExploredMap[x + y * Width] = true;
		}
	}

	public List<Tuple<Cell, char>> GetLine(int x0, int y0, int x1, int y1, bool IncludeSolid = false, GameObject UseTargetability = null)
	{
		List<Tuple<Cell, char>> list = new List<Tuple<Cell, char>>();
		bool flag = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
		if (flag)
		{
			int num = x0;
			x0 = y0;
			y0 = num;
			int num2 = x1;
			x1 = y1;
			y1 = num2;
		}
		int value = x1 - x0;
		int num3 = Math.Abs(y1 - y0);
		int num4 = Math.Abs(value) / 2;
		int num5 = y0;
		int num6 = ((y0 < y1) ? 1 : (-1));
		if (x0 > x1)
		{
			for (int num7 = x0; num7 >= x1; num7--)
			{
				char c = '.';
				Cell cell;
				if (flag)
				{
					if (num6 != 0)
					{
						c = ((num6 >= 0) ? '\\' : '/');
					}
					else
					{
						c = '|';
					}
					cell = Map[num5][num7];
				}
				else
				{
					if (num6 != 0)
					{
						c = ((num6 >= 0) ? '\\' : '/');
					}
					else
					{
						c = '-';
					}
					cell = Map[num7][num5];
				}
				list.Add(new Tuple<Cell, char>(cell, c));
				if (num7 != x0 && num7 != x1)
				{
					if (cell.IsOccluding())
					{
						return list;
					}
					if (IncludeSolid)
					{
						if (UseTargetability == null)
						{
							if (cell.IsSolid(ForFluid: true))
							{
								return list;
							}
						}
						else if (cell.IsSolidFor(UseTargetability))
						{
							return list;
						}
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		else
		{
			for (int i = x0; i <= x1; i++)
			{
				char c2 = '.';
				Cell cell;
				if (flag)
				{
					if (num6 != 0)
					{
						c2 = ((num6 >= 0) ? '/' : '\\');
					}
					else
					{
						c2 = '|';
					}
					cell = Map[num5][i];
				}
				else
				{
					if (num6 != 0)
					{
						c2 = ((num6 >= 0) ? '/' : '\\');
					}
					else
					{
						c2 = '-';
					}
					cell = Map[i][num5];
				}
				list.Add(new Tuple<Cell, char>(cell, c2));
				if (i != x0 && i != x1)
				{
					if (cell.IsOccluding())
					{
						return list;
					}
					if (IncludeSolid)
					{
						if (UseTargetability == null)
						{
							if (cell.IsSolid(ForFluid: true))
							{
								return list;
							}
						}
						else if (cell.IsSolidFor(UseTargetability))
						{
							return list;
						}
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		return list;
	}

	public bool CalculateLOS(int x0, int y0, int x1, int y1, bool IncludeSolid = false, bool BlackoutStops = false, GameObject UseTargetability = null, Predicate<Cell> OverrideBlocking = null, int PassBlocks = 0, bool BlockContinue = false, bool CheckOcclusion = true)
	{
		int num = y1 - y0;
		int num2 = (num ^ (num >> 31)) - (num >> 31);
		int num3 = x1 - x0;
		int num4 = (num3 ^ (num3 >> 31)) - (num3 >> 31);
		bool flag = num2 > num4;
		if (flag)
		{
			int num5 = x0;
			x0 = y0;
			y0 = num5;
			int num6 = x1;
			x1 = y1;
			y1 = num6;
		}
		int num7 = x1 - x0;
		num = y1 - y0;
		int num8 = (num ^ (num >> 31)) - (num >> 31);
		int num9 = ((num7 ^ (num7 >> 31)) - (num7 >> 31)) / 2;
		int num10 = y0;
		int num11 = ((y0 < y1) ? 1 : (-1));
		int num12 = 0;
		if (x0 > x1)
		{
			for (int num13 = x0; num13 >= x1; num13--)
			{
				Cell cell = ((!flag) ? Map[num13][num10] : Map[num10][num13]);
				if (num13 != x0 && num13 != x1)
				{
					if (BlockContinue && num12 > 0)
					{
						if (++num12 > PassBlocks)
						{
							return false;
						}
					}
					else if (OverrideBlocking != null)
					{
						if (OverrideBlocking(cell) && ++num12 > PassBlocks)
						{
							return false;
						}
					}
					else if (CheckOcclusion && (cell.OccludeCache == 1 || (cell.OccludeCache != 0 && cell.IsOccluding())))
					{
						if (++num12 > PassBlocks)
						{
							return false;
						}
					}
					else if (BlackoutStops && cell.IsBlackedOut())
					{
						if (++num12 > PassBlocks)
						{
							return false;
						}
					}
					else if (IncludeSolid)
					{
						if (UseTargetability == null)
						{
							if (cell.IsSolid(ForFluid: true) && ++num12 > PassBlocks)
							{
								return false;
							}
						}
						else if (cell.IsSolidFor(UseTargetability) && ++num12 > PassBlocks)
						{
							return false;
						}
					}
				}
				num9 -= num8;
				if (num9 < 0)
				{
					num10 += num11;
					num9 += (num7 ^ (num7 >> 31)) - (num7 >> 31);
				}
			}
		}
		else
		{
			for (int i = x0; i <= x1; i++)
			{
				Cell cell = ((!flag) ? Map[i][num10] : Map[num10][i]);
				if (i != x0 && i != x1)
				{
					if (BlockContinue && num12 > 0)
					{
						if (++num12 > PassBlocks)
						{
							return false;
						}
					}
					else if (OverrideBlocking != null)
					{
						if (OverrideBlocking(cell) && ++num12 > PassBlocks)
						{
							return false;
						}
					}
					else if (cell.OccludeCache == 1 || (cell.OccludeCache != 0 && cell.IsOccluding()))
					{
						if (++num12 > PassBlocks)
						{
							return false;
						}
					}
					else if (BlackoutStops && cell.IsBlackedOut())
					{
						if (++num12 > PassBlocks)
						{
							return false;
						}
					}
					else if (IncludeSolid && cell.IsSolidForProjectile(null, UseTargetability) && ++num12 > PassBlocks)
					{
						return false;
					}
				}
				num9 -= num8;
				if (num9 < 0)
				{
					num10 += num11;
					num9 += (num7 ^ (num7 >> 31)) - (num7 >> 31);
				}
			}
		}
		return true;
	}

	public bool CalculateLOS(Cell C, int x1, int y1, bool IncludeSolid = false, bool BlackoutStops = false, GameObject UseTargetability = null, Predicate<Cell> OverrideBlocking = null, int PassBlocks = 0, bool BlockContinue = false, bool CheckOcclusion = true)
	{
		if (C == null)
		{
			return false;
		}
		return CalculateLOS(C.X, C.Y, x1, y1, IncludeSolid, BlackoutStops, UseTargetability, OverrideBlocking, PassBlocks, BlockContinue, CheckOcclusion);
	}

	public bool CalculateLOS(Cell C, Cell TC, bool IncludeSolid = false, bool BlackoutStops = false, GameObject UseTargetability = null, Predicate<Cell> OverrideBlocking = null, int PassBlocks = 0, bool BlockContinue = false)
	{
		if (TC == null)
		{
			return false;
		}
		return CalculateLOS(C, TC.X, TC.Y, IncludeSolid, BlackoutStops, UseTargetability, OverrideBlocking, PassBlocks, BlockContinue);
	}

	public bool CalculateLOS(GameObject GO, int x1, int y1, bool IncludeSolid = false, bool BlackoutStops = false, GameObject UseTargetability = null, Predicate<Cell> OverrideBlocking = null, int PassBlocks = 0, bool BlockContinue = false)
	{
		return CalculateLOS(GO?.CurrentCell, x1, y1, IncludeSolid, BlackoutStops, UseTargetability, OverrideBlocking, PassBlocks, BlockContinue);
	}

	public bool HasUnobstructedLineTo(int x0, int y0, int x1, int y1, bool BlackoutStops = false, GameObject UseTargetability = null)
	{
		bool flag = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
		if (flag)
		{
			int num = x0;
			x0 = y0;
			y0 = num;
			int num2 = x1;
			x1 = y1;
			y1 = num2;
		}
		int value = x1 - x0;
		int num3 = Math.Abs(y1 - y0);
		int num4 = Math.Abs(value) / 2;
		int num5 = y0;
		int num6 = ((y0 < y1) ? 1 : (-1));
		if (x0 > x1)
		{
			for (int num7 = x0; num7 >= x1; num7--)
			{
				Cell cell = ((!flag) ? Map[num7][num5] : Map[num5][num7]);
				if (num7 != x0 && num7 != x1)
				{
					if (BlackoutStops && cell.IsBlackedOut())
					{
						return false;
					}
					if (UseTargetability == null)
					{
						if (cell.IsSolid(ForFluid: true))
						{
							return false;
						}
					}
					else if (cell.IsSolidFor(UseTargetability))
					{
						return false;
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		else
		{
			for (int i = x0; i <= x1; i++)
			{
				Cell cell = ((!flag) ? Map[i][num5] : Map[num5][i]);
				if (i != x0 && i != x1 && cell.IsSolid(ForFluid: true))
				{
					if (BlackoutStops && cell.IsBlackedOut())
					{
						return false;
					}
					if (UseTargetability == null)
					{
						if (cell.IsSolid(ForFluid: true))
						{
							return false;
						}
					}
					else if (cell.IsSolidFor(UseTargetability))
					{
						return false;
					}
				}
				num4 -= num3;
				if (num4 < 0)
				{
					num5 += num6;
					num4 += Math.Abs(value);
				}
			}
		}
		return true;
	}

	public bool CalculateLOSFloat(int xstart, int ystart, int xend, int yend)
	{
		int num = (int)Math.Sqrt((xstart - xend) * (xstart - xend) + (ystart - yend) * (ystart - yend));
		float num2 = (float)xstart + 0.5f;
		float num3 = (float)ystart + 0.5f;
		float num4 = ((float)xend + 0.5f - num2) / (float)num;
		float num5 = ((float)yend + 0.5f - num3) / (float)num;
		for (int i = 0; i < num - 1; i++)
		{
			num2 += num4;
			num3 += num5;
			int num6 = (int)Math.Floor(num2);
			int num7 = (int)Math.Floor(num3);
			if (Map[num6][num7].IsOccluding())
			{
				return false;
			}
		}
		return true;
	}

	public void LightAll(LightLevel Level = LightLevel.Light)
	{
		for (int i = 0; i < Width * Height; i++)
		{
			if ((int)LightMap[i] < (int)Level)
			{
				LightMap[i] = Level;
			}
		}
	}

	public void UnlightAll()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			if ((int)LightMap[i] < 210)
			{
				LightMap[i] = LightLevel.None;
			}
		}
	}

	public void BlackoutAll()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			if ((int)LightMap[i] < 210)
			{
				LightMap[i] = LightLevel.Blackout;
			}
		}
	}

	public void VisAll()
	{
		for (int i = 0; i < Width * Height; i++)
		{
			VisibilityMap[i] = true;
		}
	}

	public void RemoveLight(int x, int y, int r)
	{
		RemoveLight(x, y, r, LightLevel.None);
	}

	public void AddLight(int x, int y, int r)
	{
		AddLight(x, y, r, LightLevel.Light);
	}

	public void RemoveLight(int x, int y, int r, LightLevel Level, LightLevel RemoveUpTo = LightLevel.Interpolight)
	{
		if ((int)GetLight(x, y) < (int)RemoveUpTo)
		{
			SetLight(x, y, Level);
		}
		int i = x - r;
		if (i < 0)
		{
			i = 0;
		}
		int num = y - r;
		if (num < 0)
		{
			num = 0;
		}
		for (r *= r; i < Width && i <= x + r; i++)
		{
			for (int j = num; j < Height && j <= y + r; j++)
			{
				if ((i - x) * (i - x) + (j - y) * (j - y) <= r && (int)GetLight(i, j) < (int)RemoveUpTo && CalculateLOS(x, y, i, j))
				{
					SetLight(i, j, Level);
				}
			}
		}
	}

	public static bool BlocksRadar(Cell C)
	{
		return C.BlocksRadar();
	}

	public void MixLight(int x, int y, LightLevel Level, bool Force = false)
	{
		if (Force)
		{
			SetLight(x, y, Level);
			if ((int)Level >= 210)
			{
				SetVisibility(x, y, v: true);
			}
			return;
		}
		LightLevel light = GetLight(x, y);
		if (Level == LightLevel.Radar && (int)light > 1 && (int)light < 228)
		{
			SetLight(x, y, LightLevel.LitRadar);
			SetVisibility(x, y, v: true);
		}
		else if (light == LightLevel.Radar && (int)Level > 1 && (int)Level < 228)
		{
			if (CalculateLOS(The.Player, x, y))
			{
				SetLight(x, y, LightLevel.LitRadar);
				SetVisibility(x, y, v: true);
			}
		}
		else if ((int)Level > (int)light)
		{
			SetLight(x, y, Level);
			if ((int)Level >= 210)
			{
				SetVisibility(x, y, v: true);
			}
		}
	}

	public void MixLight(int x, int y, int xp, int yp, LightLevel Level, bool Force = false)
	{
		if (Force)
		{
			SetLight(xp, yp, Level);
			if ((int)Level >= 210)
			{
				SetVisibility(xp, yp, v: true);
			}
			return;
		}
		LightLevel light = GetLight(xp, yp);
		if (Level == LightLevel.Radar && (int)light > 1 && (int)light < 228 && CalculateLOS(x, y, xp, yp, IncludeSolid: true, BlackoutStops: false, null, BlocksRadar))
		{
			if (CalculateLOS(The.Player, xp, yp))
			{
				SetLight(xp, yp, LightLevel.LitRadar);
			}
			else
			{
				SetLight(xp, yp, LightLevel.Radar);
			}
			SetVisibility(xp, yp, v: true);
		}
		else if (light == LightLevel.Radar && (int)Level > 1 && (int)Level < 228 && CalculateLOS(The.Player, xp, yp))
		{
			SetLight(xp, yp, LightLevel.LitRadar);
			SetVisibility(xp, yp, v: true);
		}
		else
		{
			if ((int)Level <= (int)light)
			{
				return;
			}
			if (Level == LightLevel.Radar)
			{
				if (CalculateLOS(x, y, xp, yp, IncludeSolid: true, BlackoutStops: false, null, BlocksRadar))
				{
					SetLight(xp, yp, Level);
					SetVisibility(xp, yp, v: true);
				}
			}
			else if (Level == LightLevel.Interpolight)
			{
				if (CalculateLOS(x, y, xp, yp, IncludeSolid: false, BlackoutStops: false, null, null, 2, BlockContinue: true))
				{
					SetLight(xp, yp, Level);
					SetVisibility(xp, yp, v: true);
				}
			}
			else if ((int)Level >= 255 || CalculateLOS(x, y, xp, yp))
			{
				SetLight(xp, yp, Level);
				if ((int)Level >= 228)
				{
					SetVisibility(xp, yp, v: true);
				}
			}
		}
	}

	public void AddLight(int x, int y, int r, LightLevel Level, bool Force = false)
	{
		MixLight(x, y, Level, Force);
		if (r <= 0)
		{
			return;
		}
		int i = Math.Max(x - r, 0);
		int num = Math.Max(y - r, 0);
		for (r *= r; i < Width && i <= x + r; i++)
		{
			for (int j = num; j < Height && j <= y + r; j++)
			{
				if ((i - x) * (i - x) + (j - y) * (j - y) <= r && (x != i || y != j))
				{
					MixLight(x, y, i, j, Level, Force);
				}
			}
		}
	}

	public void AddLight(LightLevel Level, bool Force = false)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				MixLight(j, i, Level, Force);
			}
		}
	}

	public void AddExplored(int x, int y, int r)
	{
		try
		{
			SetVisibility(x, y, v: true);
			int num = Math.Max(y - r, 0);
			int num2 = Math.Min(y + r, Height - 1);
			int num3 = r * r;
			int i = Math.Max(x - r, 0);
			for (int num4 = Math.Min(x + r, Width - 1); i <= num4; i++)
			{
				for (int j = num; j <= num2; j++)
				{
					int num5 = (i - x) * (i - x) + (j - y) * (j - y);
					if (num5 <= num3 && !VisibilityMap[i + j * Width] && (num5 <= 1 || (num5 == 2 && Math.Abs(i - x) <= 1 && Math.Abs(j - y) <= 1) || (CalculateLOS(x, y, i, j) && (int)GetLight(i, j) > 1)))
					{
						SetVisibility(i, j, v: true);
					}
				}
			}
		}
		catch (Exception ex)
		{
			MetricsManager.LogException("Vis error", ex);
			throw ex;
		}
	}

	public void AddVisibility(int x, int y, int r)
	{
		SetVisibility(x, y, v: true);
		int num = Math.Max(y - r, 0);
		int num2 = Math.Min(y + r, Height - 1);
		int num3 = r * r;
		int i = Math.Max(x - r, 0);
		for (int num4 = Math.Min(x + r, Width - 1); i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				int num5 = (i - x) * (i - x) + (j - y) * (j - y);
				if (num5 <= num3 && !VisibilityMap[i + j * Width] && (num5 <= 1 || (num5 == 2 && Math.Abs(i - x) <= 1 && Math.Abs(j - y) <= 1) || (CalculateLOS(x, y, i, j) && (int)GetLight(i, j) > 1)))
				{
					SetVisibility(i, j, v: true);
				}
			}
		}
	}

	private void CalculateXYRadius(int x, int y, int Radius, out int x1, out int x2, out int y1, out int y2)
	{
		x1 = x - Radius;
		x2 = x + Radius;
		y1 = y - Radius;
		y2 = y + Radius;
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (x2 > 79)
		{
			x2 = 79;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (y2 > 24)
		{
			y2 = 24;
		}
	}

	public List<GameObject> FastSquareSearchNoImpassible(int x, int y, int Radius, string SearchPart, GameObject Looker)
	{
		List<GameObject> Return = Event.NewGameObjectList();
		if (IsWorldMap())
		{
			return Return;
		}
		int Nav = 268435456;
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.NavigationWeight(Looker, ref Nav) < 100)
				{
					cell.ForeachObjectWithPart(SearchPart, (GameObject o) =>
					{
						Return.Add(o);
					});
				}
			}
		}
		return Return;
	}

	public List<GameObject> FastSquareSearch(int x, int y, int Radius, string SearchPart)
	{
		if (IsWorldMap())
		{
			return Event.NewGameObjectList();
		}
		List<GameObject> list = Event.NewGameObjectList();
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				List<GameObject> objectsViaEventList = GetCell(j, i).GetObjectsViaEventList();
				for (int k = 0; k < objectsViaEventList.Count; k++)
				{
					if (objectsViaEventList[k].HasPart(SearchPart))
					{
						list.Add(objectsViaEventList[k]);
					}
				}
			}
		}
		return list;
	}

	public List<GameObject> FastSquareSearch(int x, int y, int Radius, Predicate<GameObject> filter, bool CachedOkay = false)
	{
		if (IsWorldMap())
		{
			return Event.NewGameObjectList();
		}
		List<GameObject> list = (CachedOkay ? Event.NewGameObjectList() : null);
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				foreach (GameObject @object in GetCell(j, i).Objects)
				{
					if (filter(@object))
					{
						if (list == null)
						{
							list = new List<GameObject> { @object };
						}
						else
						{
							list.Add(@object);
						}
					}
				}
			}
		}
		return list;
	}

	public bool FastSquareSearchAny(int x, int y, int Radius, Predicate<GameObject> filter)
	{
		if (IsWorldMap())
		{
			return false;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				foreach (GameObject @object in GetCell(j, i).Objects)
				{
					if (filter(@object))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public List<GameObject> FastCombatSquareVisibility(int x, int y, int Radius, GameObject Looker, Predicate<GameObject> Filter = null, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, bool IncludeLooker = true, bool NoBrainOnly = true, GameObject skip = null)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (IsWorldMap())
		{
			return list;
		}
		int Nav = 268435456;
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell == null || (VisibleToPlayerOnly && !cell.IsVisible()) || !(VisibleToPlayerOnly ? cell.IsExploredFor(Looker) : ExploredMap[j + i * Width]))
				{
					continue;
				}
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					if (cell.Objects[k].IsCombatObject() && (IncludeLooker || cell.Objects[k] != Looker) && cell.Objects[k] != skip && (Filter == null || Filter(cell.Objects[k])) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100) && (!VisibleToPlayerOnly || cell.Objects[k].IsVisible()))
					{
						list.Add(cell.Objects[k]);
					}
				}
			}
		}
		return list;
	}

	public List<GameObject> FastSquareVisibility(int x, int y, int Radius, string SearchPart, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null)
	{
		if (IsWorldMap())
		{
			return Event.NewGameObjectList();
		}
		List<GameObject> list = Event.NewGameObjectList();
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell == null || (VisibleToPlayerOnly && !cell.IsVisible()) || !cell.IsExploredFor(Looker) || (!IncludeWalls && cell.NavigationWeight(Looker, ref Nav) >= 100))
				{
					continue;
				}
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					if (cell.Objects[k] != skip && cell.Objects[k].HasPart(SearchPart) && (!VisibleToPlayerOnly || cell.Objects[k].IsVisible()))
					{
						list.Add(cell.Objects[k]);
					}
				}
			}
		}
		return list;
	}

	public List<GameObject> FastSquareVisibility(int x, int y, int Radius, GameObject Looker, Predicate<GameObject> Filter = null, bool VisibleToPlayerOnly = false, bool IncludeWalls = false)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (IsWorldMap())
		{
			return list;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell == null)
				{
					continue;
				}
				if (VisibleToPlayerOnly)
				{
					if (!cell.IsVisible() || !cell.IsExploredFor(Looker))
					{
						continue;
					}
				}
				else if (!cell.IsExplored())
				{
					continue;
				}
				if (!IncludeWalls && cell.NavigationWeight(Looker, ref Nav) >= 100)
				{
					continue;
				}
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if ((Filter == null || Filter(gameObject)) && (!VisibleToPlayerOnly || gameObject.IsVisible()))
					{
						list.Add(gameObject);
					}
				}
			}
		}
		return list;
	}

	public GameObject FastSquareVisibilityFirst(int x, int y, int Radius, string SearchPart, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null, int IgnoreFartherThan = 9999999, int IgnoreEasierThan = int.MinValue)
	{
		if (IsWorldMap())
		{
			return null;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && (IgnoreFartherThan >= 9999999 || Looker == null || Looker.DistanceTo(cell) <= IgnoreFartherThan) && cell.IsExploredFor(Looker) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100))
				{
					GameObject gameObject = ((IgnoreEasierThan > int.MinValue && Looker != null) ? cell.GetFirstObjectWithPartExcept(SearchPart, skip, IgnoreEasierThan, Looker) : cell.GetFirstObjectWithPartExcept(SearchPart, skip));
					if (gameObject != null && (!VisibleToPlayerOnly || gameObject.IsVisible()))
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public GameObject FastSquareVisibilityFirstBlueprint(int x, int y, int Radius, string blueprint, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null)
	{
		if (IsWorldMap())
		{
			return null;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && cell.IsExploredFor(Looker) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100))
				{
					GameObject gameObject = (VisibleToPlayerOnly ? cell.GetFirstVisibleObjectExcept(blueprint, skip) : cell.GetFirstObjectExcept(blueprint, skip));
					if (gameObject != null)
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public GameObject FastSquareVisibilityFirst(int x, int y, int Radius, string SearchPart, Predicate<GameObject> Filter, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null, int IgnoreFartherThan = 9999999, int IgnoreEasierThan = int.MinValue)
	{
		if (IsWorldMap())
		{
			return null;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && (IgnoreFartherThan >= 9999999 || Looker == null || Looker.DistanceTo(cell) <= IgnoreFartherThan) && cell.IsExploredFor(Looker) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100))
				{
					GameObject gameObject;
					if (VisibleToPlayerOnly)
					{
						gameObject = ((IgnoreEasierThan > int.MinValue && Looker != null) ? cell.GetFirstVisibleObjectWithPartExcept(SearchPart, Filter, skip, IgnoreEasierThan, Looker) : cell.GetFirstVisibleObjectWithPartExcept(SearchPart, Filter, skip));
					}
					else
					{
						gameObject = ((IgnoreEasierThan > int.MinValue && Looker != null) ? cell.GetFirstObjectWithPartExcept(SearchPart, Filter, skip, IgnoreEasierThan, Looker) : cell.GetFirstObjectWithPartExcept(SearchPart, Filter, skip));
					}
					if (gameObject != null)
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public bool FastSquareVisibilityAny(int x, int y, int Radius, string SearchPart, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null)
	{
		if (IsWorldMap())
		{
			return false;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && cell.IsExploredFor(Looker) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100) && (VisibleToPlayerOnly ? cell.HasVisibleObjectWithPartExcept(SearchPart, skip) : cell.HasObjectWithPartExcept(SearchPart, skip)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool FastSquareVisibilityAny(int x, int y, int Radius, string SearchPart, Predicate<GameObject> Filter, GameObject Looker, bool VisibleToPlayerOnly = false, bool IncludeWalls = false, GameObject skip = null)
	{
		if (IsWorldMap())
		{
			return false;
		}
		CalculateXYRadius(x, y, Radius, out var x2, out var x3, out var y2, out var y3);
		int Nav = 268435456;
		for (int i = y2; i <= y3; i++)
		{
			for (int j = x2; j <= x3; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell != null && (!VisibleToPlayerOnly || cell.IsVisible()) && cell.IsExploredFor(Looker) && (IncludeWalls || cell.NavigationWeight(Looker, ref Nav) < 100) && (VisibleToPlayerOnly ? cell.HasVisibleObjectWithPartExcept(SearchPart, Filter, skip) : cell.HasObjectWithPartExcept(SearchPart, Filter, skip)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ClearReachableMap(bool bValue)
	{
		if (ReachableMap == null)
		{
			ReachableMap = new bool[Width, Height];
		}
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				ReachableMap[j, i] = bValue;
			}
		}
	}

	public void ClearReachableMap()
	{
		ClearReachableMap(bValue: false);
	}

	public int BuildReachableMap(int StartX, int StartY)
	{
		return BuildReachableMap(StartX, StartY, bClearFirst: true);
	}

	public int BuildReachableMap(int StartX, int StartY, bool bClearFirst)
	{
		if (bClearFirst)
		{
			ClearReachableMap();
		}
		if (ReachableMap == null)
		{
			ClearReachableMap();
		}
		return BuildReachableMapHelper(StartX, StartY, ReachableMap);
	}

	public bool BuildReachabilityFromEdges()
	{
		if (BuildReachableMap(Width / 2, Height - 1) >= 400)
		{
			return true;
		}
		if (BuildReachableMap(Width / 2, 0) >= 400)
		{
			return true;
		}
		if (BuildReachableMap(0, Height / 2) >= 400)
		{
			return true;
		}
		if (BuildReachableMap(Width - 1, Height / 2) >= 400)
		{
			return true;
		}
		for (int i = 0; i < Height; i++)
		{
			if (BuildReachableMap(0, i) >= 400)
			{
				return true;
			}
			if (BuildReachableMap(Width - 1, i) >= 400)
			{
				return true;
			}
		}
		for (int j = 0; j < Width; j++)
		{
			if (BuildReachableMap(j, Height - 1) >= 400)
			{
				return true;
			}
			if (BuildReachableMap(j, 0) >= 400)
			{
				return true;
			}
		}
		return false;
	}

	public void BuildReachableMap()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (!cell.IsOccluding() && !cell.IsReachable())
				{
					BuildReachableMap(j, i, bClearFirst: false);
				}
			}
		}
	}

	public void RebuildReachableMap()
	{
		ClearReachableMap();
		BuildReachableMap();
	}

	public bool IsActive()
	{
		return XRLCore.Core.Game?.ZoneManager.ActiveZone == this;
	}

	public bool IsReachable(int X, int Y)
	{
		if (ReachableMap == null)
		{
			return true;
		}
		return ReachableMap[X, Y];
	}

	private int BuildReachableMapHelper(int StartX, int StartY, bool[,] Reachable)
	{
		VisitedList.Clear();
		CellQueue.Clear();
		int num = 0;
		if (StartX < 0)
		{
			return num;
		}
		if (StartY < 0)
		{
			return num;
		}
		if (StartX >= Width)
		{
			return num;
		}
		if (StartY >= Height)
		{
			return num;
		}
		if (GetCell(StartX, StartY).HasWall())
		{
			return num;
		}
		CellQueue.Enqueue(Location2D.Get(StartX, StartY));
		while (CellQueue.Count > 0)
		{
			Location2D location2D = CellQueue.Dequeue();
			if (VisitedList.ContainsKey(location2D))
			{
				continue;
			}
			VisitedList.Add(location2D, value: true);
			if (!GetCell(location2D).HasWall())
			{
				Reachable[location2D.X, location2D.Y] = true;
				num++;
				if (location2D.X - 1 >= 0)
				{
					CellQueue.Enqueue(Location2D.Get(location2D.X - 1, location2D.Y));
				}
				if (location2D.X + 1 < Width)
				{
					CellQueue.Enqueue(Location2D.Get(location2D.X + 1, location2D.Y));
				}
				if (location2D.Y - 1 >= 0)
				{
					CellQueue.Enqueue(Location2D.Get(location2D.X, location2D.Y - 1));
				}
				if (location2D.Y + 1 < Height)
				{
					CellQueue.Enqueue(Location2D.Get(location2D.X, location2D.Y + 1));
				}
				if (location2D.X - 1 >= 0 && location2D.Y - 1 >= 0)
				{
					CellQueue.Enqueue(Location2D.Get(location2D.X - 1, location2D.Y - 1));
				}
				if (location2D.X + 1 < Width && location2D.Y - 1 >= 0)
				{
					CellQueue.Enqueue(Location2D.Get(location2D.X + 1, location2D.Y - 1));
				}
				if (location2D.X - 1 >= 0 && location2D.Y + 1 < Height)
				{
					CellQueue.Enqueue(Location2D.Get(location2D.X - 1, location2D.Y + 1));
				}
				if (location2D.X + 1 < Width && location2D.Y + 1 < Height)
				{
					CellQueue.Enqueue(Location2D.Get(location2D.X + 1, location2D.Y + 1));
				}
			}
		}
		VisitedList.Clear();
		return num;
	}

	private void BuildReachableMapHelperFlood(int StartX, int StartY, bool[,] Reachable)
	{
		if (StartX >= 0 && StartY >= 0 && StartX < Width && StartY < Height && !Reachable[StartX, StartY] && GetCell(StartX, StartY).IsEmpty() && (GetCell(StartX, StartY).Objects.Count <= 0 || GetCell(StartX, StartY).IsEmpty()))
		{
			Reachable[StartX, StartY] = true;
			BuildReachableMapHelper(StartX - 1, StartY - 1, Reachable);
			BuildReachableMapHelper(StartX, StartY - 1, Reachable);
			BuildReachableMapHelper(StartX + 1, StartY - 1, Reachable);
			BuildReachableMapHelper(StartX - 1, StartY, Reachable);
			BuildReachableMapHelper(StartX + 1, StartY, Reachable);
			BuildReachableMapHelper(StartX - 1, StartY + 1, Reachable);
			BuildReachableMapHelper(StartX, StartY + 1, Reachable);
			BuildReachableMapHelper(StartX + 1, StartY + 1, Reachable);
		}
	}

	public void DebugDrawSemantics()
	{
		if (!(Options.GetOption("OptionDrawZoneSemantics") == "Yes"))
		{
			return;
		}
		ScreenBuffer newBuffer = ScreenBuffer.GetScrapBuffer1();
		List<string> colorCodes = new List<string> { "G", "B", "Y", "W", "M", "K", "C", "R" };
		Dictionary<string, string> tagColors = new Dictionary<string, string>();
		GetCells().ForEach((Cell c) =>
		{
			c.SemanticTags?.ForEach((string t) =>
			{
				if (t != null && !tagColors.ContainsKey(t))
				{
					tagColors.Add(t, "&" + colorCodes[tagColors.Count % colorCodes.Count]);
				}
			});
		});
		GetCells().ForEach((Cell c) =>
		{
			newBuffer.Goto(c.X, c.Y);
			List<string> semanticTags = GetCell(c.X, c.Y).SemanticTags;
			if (semanticTags != null && semanticTags.Count > 0)
			{
				newBuffer.Write(tagColors[GetCell(c.X, c.Y).SemanticTags[0]] + "#");
			}
		});
		newBuffer.Draw();
		Keyboard.getch();
	}

	public void Render(ScreenBuffer Buf)
	{
		if (GameManager.bDraw != 12)
		{
			Buf.Clear();
			Event.PinCurrentPool();
			Render(Buf, 0, 0, 79, 24);
			Event.ResetToPin();
		}
	}

	public void Render(ScreenBuffer Buf, int X1, int Y1, int X2, int Y2)
	{
		if (The.Core.HostileWalkObjects == null)
		{
			The.Core.HostileWalkObjects = new List<GameObject>();
		}
		RenderedObjects = 0;
		bool bAlt = Keyboard.bAlt;
		bool disableFullscreenColorEffects = Options.DisableFullscreenColorEffects;
		WantsToPaint.Clear();
		if (SoundMapDirty)
		{
			UpdateSoundMap();
		}
		RenderEvent renderEvent = eRender;
		for (int i = Y1; i <= Y2; i++)
		{
			for (int j = X1; j <= X2; j++)
			{
				int num = j + i * Width;
				ConsoleChar consoleChar = Buf.Buffer[j, i];
				Map[j][i].Render(renderEvent, VisibilityMap[num], LightMap[num], ExploredMap[num], bAlt, disableFullscreenColorEffects, WantsToPaint);
				consoleChar.Set(renderEvent);
			}
		}
		if (SoundRenderers.Count != 0)
		{
			ISoundRenderer[] array = SoundRenderers.GetArray();
			RenderSoundEvent renderSoundEvent = eRenderSound;
			renderSoundEvent.Zone = this;
			renderSoundEvent.Buffer = Buf.Buffer;
			for (int num2 = SoundRenderers.Count - 1; num2 >= 0; num2--)
			{
				renderSoundEvent.Unregister = false;
				array[num2].Render(renderSoundEvent);
				renderSoundEvent.Char = null;
				if (renderSoundEvent.Unregister)
				{
					SoundRenderers.RemoveAt(num2);
				}
			}
		}
		foreach (GameObject item in WantsToPaint)
		{
			item.Paint(Buf);
		}
		The.Core.RenderedObjects = RenderedObjects;
	}

	public void RegisterSoundRenderer(ISoundRenderer Renderer)
	{
		if (SoundRenderers == null)
		{
			SoundRenderers = new Rack<ISoundRenderer> { Renderer };
		}
		else if (!SoundRenderers.Contains(Renderer))
		{
			SoundRenderers.Add(Renderer);
		}
	}

	public void UnregisterSoundRenderer(ISoundRenderer Renderer)
	{
		SoundRenderers?.Remove(Renderer);
	}

	public void ForeachCell(Action<Cell> aProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				aProc(GetCell(j, i));
			}
		}
	}

	public bool ForeachCell(Predicate<Cell> pProc)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (!pProc(GetCell(j, i)))
				{
					return false;
				}
			}
		}
		return true;
	}

	public List<Cell> GetEmptyReachableCellsWithout(string[] Blueprint)
	{
		List<Cell> list = new List<Cell>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable() && !cell.HasObjectWithBlueprint(Blueprint))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetReachableCells()
	{
		List<Cell> list = new List<Cell>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsReachable())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetReachableCellsWithout(string[] Blueprint)
	{
		List<Cell> list = new List<Cell>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsReachable() && !cell.HasObjectWithBlueprint(Blueprint))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyReachableCells(Rect2D r)
	{
		List<Cell> list = new List<Cell>();
		for (int i = r.y1; i <= r.y2; i++)
		{
			for (int j = r.x1; j <= r.x2; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public Cell GetSpawnCell()
	{
		Cell randomElement = GetEmptyReachableCells().GetRandomElement();
		if (randomElement == null)
		{
			randomElement = GetEmptyCells().GetRandomElement();
		}
		if (randomElement == null)
		{
			randomElement = (from c in GetReachableCells()
				where !c.HasWall()
				select c).GetRandomElement();
		}
		if (randomElement == null)
		{
			randomElement = GetCells().GetRandomElement();
		}
		return randomElement;
	}

	public List<Cell> GetEmptyReachableCells()
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable())
				{
					list.Add(cell);
				}
			}
		}
		if (list.Count == 0)
		{
			return GetEmptyCells();
		}
		return list;
	}

	public List<Cell> GetEmptyReachableCells(Predicate<Cell> Filter)
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable() && Filter(cell))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyReachableCells(Event E)
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable() && cell.FireEvent(E))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public bool AnyEmptyReachableCells()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyEmptyReachableCells(Event E)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.IsReachable() && cell.FireEvent(E))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<Cell> GetEmptyCellsWithNoFurniture()
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && !cell.HasFurniture())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetPassableCells(GameObject Object = null)
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsPassable(Object))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyCells(Predicate<Cell> filter = null, List<Cell> listToUse = null)
	{
		List<Cell> list = ((listToUse != null) ? listToUse : new List<Cell>(32));
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && (filter == null || filter(cell)))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetEmptyCells(Rect2D R)
	{
		List<Cell> list = new List<Cell>();
		for (int i = R.y1; i <= R.y2; i++)
		{
			for (int j = R.x1; j <= R.x2; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public int GetEmptyCellCount()
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (GetCell(j, i).IsEmpty())
				{
					num++;
				}
			}
		}
		return num;
	}

	public List<Cell> GetEmptyCells(Event E)
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsEmpty() && cell.FireEvent(E))
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public List<Cell> GetExploredCells()
	{
		List<Cell> list = new List<Cell>(32);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				if (cell.IsExplored())
				{
					list.Add(cell);
				}
			}
		}
		return list;
	}

	public Cell[] GetExploredCellsShuffled()
	{
		return Algorithms.RandomShuffle(GetExploredCells(), Stat.Rand);
	}

	[Obsolete("Legacy amnesia behaviour, use GetExploredCells")]
	public List<Cell> GetReallyExploredCells()
	{
		return GetExploredCells();
	}

	[Obsolete("Legacy amnesia behaviour, use GetExploredCellsShuffled")]
	public Cell[] GetReallyExploredCellsShuffled()
	{
		return GetExploredCellsShuffled();
	}

	[Obsolete("Legacy amnesia behaviour, use AnyUnexplored")]
	public bool AnyReallyUnexplored()
	{
		return AnyUnexplored();
	}

	public bool AnyUnexplored()
	{
		Cell[][] map = Map;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (!map[j][i].IsExplored())
				{
					return true;
				}
			}
		}
		return false;
	}

	public Cell[] GetEmptyCellsShuffled()
	{
		return Algorithms.RandomShuffle(GetEmptyCells(), Stat.Rand);
	}

	public Cell GetCellWithEmptyBorder(int Size)
	{
		List<Cell> list = new List<Cell>();
		Cell[] emptyCellsShuffled = GetEmptyCellsShuffled();
		foreach (Cell cell in emptyCellsShuffled)
		{
			if (cell.X <= Size || cell.X >= Width - Size || cell.Y <= Size || cell.Y >= Height - Size)
			{
				continue;
			}
			list.Clear();
			cell.GetAdjacentCells(Size, list);
			using List<Cell>.Enumerator enumerator = list.GetEnumerator();
			do
			{
				if (!enumerator.MoveNext())
				{
					return cell;
				}
			}
			while (enumerator.Current.IsEmpty());
		}
		return null;
	}

	public List<Cell> GetEmptyVisibleCells()
	{
		List<Cell> list = new List<Cell>();
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (GetCell(j, i).IsEmpty() && GetCell(j, i).IsVisible())
				{
					list.Add(GetCell(j, i));
				}
			}
		}
		return list;
	}

	public IEnumerable<Cell> GetCells(Box b)
	{
		for (int y = b.y1; y <= b.y2; y++)
		{
			for (int x = b.x1; x <= b.x2; x++)
			{
				yield return GetCell(x, y);
			}
		}
	}

	public List<Cell> GetCells()
	{
		if (CellList == null)
		{
			CellList = new List<Cell>(Width * Height);
			for (int i = 0; i < Height; i++)
			{
				for (int j = 0; j < Width; j++)
				{
					CellList.Add(Map[j][i]);
				}
			}
		}
		return CellList;
	}

	public List<Cell> GetCellsViaEventList(Predicate<Cell> pFilter)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (pFilter(Map[j][i]))
				{
					num++;
				}
			}
		}
		List<Cell> list = Event.NewCellList();
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (pFilter(Map[l][k]))
					{
						list.Add(Map[l][k]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public List<Cell> GetCells(Predicate<Cell> pFilter)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (pFilter(Map[j][i]))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (pFilter(Map[l][k]))
					{
						list.Add(Map[l][k]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public List<Cell> GetCellsWithObject(string Blueprint)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasObject(Blueprint))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (Map[l][k].HasObject(Blueprint))
					{
						list.Add(Map[l][k]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public IEnumerable<Cell> LoopCells()
	{
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				yield return Map[x][y];
			}
		}
	}

	public List<Cell> GetCellsWithObject(Predicate<GameObject> pFilter)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasObject(pFilter))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (Map[l][k].HasObject(pFilter))
					{
						list.Add(Map[l][k]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public List<Cell> GetCellsWithTaggedObject(string What)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasObjectWithTag(What))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (Map[l][k].HasObjectWithTag(What))
					{
						list.Add(Map[l][k]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public List<Cell> GetCellsWithTagOrPropertyObject(string What)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].HasObjectWithTagOrProperty(What))
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Width; k++)
			{
				for (int l = 0; l < Height; l++)
				{
					if (Map[k][l].HasObjectWithTagOrProperty(What))
					{
						list.Add(Map[k][l]);
						if (list.Count >= num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public List<Cell> GetCellsWithFloor(string OkFloor)
	{
		int num = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Map[j][i].PaintTile == OkFloor)
				{
					num++;
				}
			}
		}
		List<Cell> list = new List<Cell>(num);
		if (num > 0)
		{
			for (int k = 0; k < Height; k++)
			{
				for (int l = 0; l < Width; l++)
				{
					if (Map[l][k].PaintTile == OkFloor)
					{
						list.Add(Map[l][k]);
						if (list.Count > num)
						{
							return list;
						}
					}
				}
			}
		}
		return list;
	}

	public GameObject GetFirstObject()
	{
		GameObject gameObject = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if ((gameObject = Map[j][i].GetFirstObject()) != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObject(string Blueprint)
	{
		GameObject gameObject = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if ((gameObject = Map[j][i].GetFirstObject(Blueprint)) != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObject(Predicate<GameObject> pFilter)
	{
		GameObject gameObject = null;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if ((gameObject = Map[j][i].GetFirstObject(pFilter)) != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public int[,] GetUnreachableGrid()
	{
		int[,] array = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!GetCell(i, j).IsReachable())
				{
					array[i, j] = 0;
				}
				else
				{
					array[i, j] = 1;
				}
			}
		}
		return array;
	}

	public int[,] GetWallGrid()
	{
		int[,] array = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (GetCell(i, j).HasWall())
				{
					array[i, j] = 0;
				}
				else
				{
					array[i, j] = 1;
				}
			}
		}
		return array;
	}

	public int[,] GetUnreachableOrWallGrid()
	{
		int[,] array = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (GetCell(i, j).HasWall() || !GetCell(i, j).IsReachable())
				{
					array[i, j] = 0;
				}
				else
				{
					array[i, j] = 1;
				}
			}
		}
		return array;
	}

	public Cell GetCell(Point2D p)
	{
		return GetCell(p.x, p.y);
	}

	public Cell GetCell(Location2D p)
	{
		return GetCell(p.X, p.Y);
	}

	public Cell GetCell(Point p)
	{
		return GetCell(p.X, p.Y);
	}

	public Cell GetCell(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Width || y >= Height)
		{
			return null;
		}
		return Map[x][y];
	}

	public Cell GetCellChecked(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Width || y >= Height)
		{
			MetricsManager.LogError("cell at invalid coordinates " + x + ", " + y + " requested from " + DebugName);
			return null;
		}
		Cell cell = Map[x][y];
		if (cell == null)
		{
			MetricsManager.LogError("no cell at coordinates " + x + ", " + y + " in " + DebugName);
		}
		return cell;
	}

	public Cell GetCellGlobal(int XD, int YD, bool LocalOnly = false, bool BuiltOnly = true)
	{
		if (XD >= 0 && YD >= 0 && XD < Width && YD < Height)
		{
			return Map[XD][YD];
		}
		if (LocalOnly)
		{
			return null;
		}
		int num = wX;
		int num2 = X;
		int num3 = wY;
		int num4 = Y;
		do
		{
			if (XD < 0)
			{
				if (num2 == 0)
				{
					num--;
					num2 += Definitions.Width;
				}
				num2--;
				XD += Width;
			}
			else if (XD >= Width)
			{
				if (num2 == Definitions.Width - 1)
				{
					num++;
					num2 -= Definitions.Width;
				}
				num2++;
				XD -= Width;
			}
			if (YD < 0)
			{
				if (num4 == 0)
				{
					num3--;
					num4 += Definitions.Height;
				}
				num4--;
				YD += Height;
			}
			else if (YD >= Height)
			{
				if (num4 == Definitions.Height - 1)
				{
					num3++;
					num4 -= Definitions.Height;
				}
				num4++;
				YD -= Height;
			}
		}
		while (XD < 0 || YD < 0 || XD >= Width || YD >= Height);
		if (num < 0)
		{
			return null;
		}
		if (num3 < 0)
		{
			return null;
		}
		if (num > 79)
		{
			return null;
		}
		if (num3 > 24)
		{
			return null;
		}
		string zoneID = XRL.World.ZoneID.Assemble(ZoneWorld, num, num3, num2, num4, Z);
		if (BuiltOnly && !The.ZoneManager.IsZoneLive(zoneID))
		{
			return null;
		}
		return The.ZoneManager.GetZone(zoneID)?.GetCell(XD, YD);
	}

	public Cell GetRandomCell(int withinBorder = 0)
	{
		return GetCell(Stat.Random(withinBorder, Width - 1 - withinBorder), Stat.Random(withinBorder, Height - 1 - withinBorder));
	}

	public Cell GetCenterCell()
	{
		return GetCell(Width / 2, Height / 2);
	}

	public void UnexploreAll()
	{
		for (int i = 0; i <= ExploredMap.GetUpperBound(0); i++)
		{
			ExploredMap[i] = false;
		}
	}

	public void ExploreAll()
	{
		int i = 0;
		for (int upperBound = ExploredMap.GetUpperBound(0); i <= upperBound; i++)
		{
			ExploredMap[i] = true;
		}
	}

	public bool FireEvent(string ID)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!Map[i][j].FireEvent(ID))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool FireEvent(Event E)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!Map[i][j].FireEvent(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool BroadcastEvent(Event E)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!Map[i][j].BroadcastEvent(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool BroadcastEvent(string Name)
	{
		return BroadcastEvent(Event.New(Name));
	}

	public bool FireEventDirect(Event E)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (!Map[i][j].FireEventDirect(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void CalculateMissileMap(GameObject Shooter)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = GetCell(j, i);
				bool flag = false;
				bool flag2 = false;
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					if (cell.Objects[k].IsCombatObject())
					{
						if (!cell.Objects[k].IsHostileTowards(Shooter) && !Shooter.IsHostileTowards(cell.Objects[k]))
						{
							flag2 = true;
							break;
						}
						flag = true;
					}
				}
				if (flag2)
				{
					MissileMap[j][i] = MissileMapType.Friendly;
					continue;
				}
				if (flag)
				{
					MissileMap[j][i] = MissileMapType.Hostile;
					continue;
				}
				if (cell.IsSolidFor(Shooter))
				{
					MissileMap[j][i] = MissileMapType.Wall;
					continue;
				}
				int num = 0;
				int l = 0;
				for (int count2 = cell.Objects.Count; l < count2; l++)
				{
					num += GetMissileCoverPercentageEvent.GetFor(cell.Objects[l], Shooter);
				}
				if (num > 0)
				{
					if (num >= 80)
					{
						MissileMap[j][i] = MissileMapType.VeryHeavyCover;
					}
					else if (num >= 50)
					{
						MissileMap[j][i] = MissileMapType.HeavyCover;
					}
					else if (num >= 30)
					{
						MissileMap[j][i] = MissileMapType.MediumCover;
					}
					else if (num >= 20)
					{
						MissileMap[j][i] = MissileMapType.LightCover;
					}
					else
					{
						MissileMap[j][i] = MissileMapType.VeryLightCover;
					}
				}
				else
				{
					MissileMap[j][i] = MissileMapType.Empty;
				}
			}
		}
	}

	public static int CalculateNav(GameObject Looker, bool Autoexploring = false, bool Juggernaut = false, bool IgnoreCreatures = false, bool IgnoreGases = false, bool FlexPhase = false, bool AllowDigging = true)
	{
		int num = 0;
		if (GameObject.Validate(ref Looker))
		{
			if (!Looker.HasStat("Intelligence") || Looker.Stat("Intelligence") > 6)
			{
				num |= 1;
			}
			if (AllowDigging && Looker.PathAsBurrower)
			{
				num |= 2;
			}
			if (Autoexploring)
			{
				num |= 4;
			}
			if (Looker.IsFlying || Looker.HasTagOrProperty("PathAsIfFlying"))
			{
				num |= 8;
			}
			else
			{
				Brain brain = Looker.Brain;
				if (brain != null && brain.WallWalker)
				{
					num |= 0x10;
				}
			}
			if (Looker.HasPropertyOrTag("IgnoresWalls"))
			{
				num |= 0x20;
			}
			int MoveSpeedPenalty = 50;
			GetSwimmingPerformanceEvent.GetFor(Looker, ref MoveSpeedPenalty);
			if (MoveSpeedPenalty < 50)
			{
				num |= 0x40;
			}
			if (Looker.Slimewalking)
			{
				num |= 0x80;
			}
			if (Looker.LimitToAquatic())
			{
				num |= 0x100;
			}
			if (Looker.Polypwalking)
			{
				num |= 0x200;
			}
			if (Looker.Strutwalking)
			{
				num |= 0x400;
			}
			if (Juggernaut)
			{
				num |= 0x800;
			}
			if (IgnoreCreatures)
			{
				num |= 0x2000;
			}
			if (!Looker.Respires)
			{
				num |= 0x8000;
			}
			if (IgnoreGases)
			{
				num |= 0x4000;
			}
			if (Looker.HasPropertyOrTag("FilthAffinity"))
			{
				num |= 0x10000;
			}
			if (Looker.HasEffect<Phased>())
			{
				num |= 0x20000;
			}
			if (Looker.HasEffect<Omniphase>())
			{
				num |= 0x40000;
			}
			if (Looker.HasTag("Nullphase") || Looker.GetIntProperty("Nullphase") >= 1)
			{
				num |= 0x80000;
			}
			if (FlexPhase)
			{
				num |= 0x100000;
			}
		}
		return num;
	}

	public static int CalculateNav(bool Smart, bool Burrower, bool Autoexploring, bool Flying, bool WallWalker, bool IgnoresWalls, bool Swimming, bool Slimewalking, bool Aquatic, bool Polypwalking, bool Strutwalking, bool Juggernaut, bool Reefer, bool IgnoreCreatures, bool IgnoreGases, bool Unbreathing, bool FilthAffinity, bool OutOfPhase, bool Omniphase, bool Nullphase, bool FlexPhase)
	{
		int num = 0;
		if (Smart)
		{
			num |= 1;
		}
		if (Burrower)
		{
			num |= 2;
		}
		if (Autoexploring)
		{
			num |= 4;
		}
		if (Flying)
		{
			num |= 8;
		}
		if (WallWalker)
		{
			num |= 0x10;
		}
		if (IgnoresWalls)
		{
			num |= 0x20;
		}
		if (Swimming)
		{
			num |= 0x40;
		}
		if (Slimewalking)
		{
			num |= 0x80;
		}
		if (Aquatic)
		{
			num |= 0x100;
		}
		if (Polypwalking)
		{
			num |= 0x200;
		}
		if (Strutwalking)
		{
			num |= 0x400;
		}
		if (Juggernaut)
		{
			num |= 0x800;
		}
		if (Reefer)
		{
			num |= 0x1000;
		}
		if (IgnoreCreatures)
		{
			num |= 0x2000;
		}
		if (IgnoreGases)
		{
			num |= 0x4000;
		}
		if (Unbreathing)
		{
			num |= 0x8000;
		}
		if (FilthAffinity)
		{
			num |= 0x10000;
		}
		if (OutOfPhase)
		{
			num |= 0x20000;
		}
		if (Omniphase)
		{
			num |= 0x40000;
		}
		if (Nullphase)
		{
			num |= 0x80000;
		}
		if (FlexPhase)
		{
			num |= 0x100000;
		}
		return num;
	}

	public static void UnpackNav(int Nav, out bool Smart, out bool Burrower, out bool Autoexploring, out bool Flying, out bool WallWalker, out bool IgnoresWalls, out bool Swimming, out bool Slimewalking, out bool Aquatic, out bool Polypwalking, out bool Strutwalking, out bool Juggernaut, out bool Reefer, out bool IgnoreCreatures, out bool IgnoreGases, out bool Unbreathing, out bool FilthAffinity, out bool OutOfPhase, out bool Omniphase, out bool Nullphase, out bool FlexPhase)
	{
		Smart = (Nav & 1) != 0;
		Burrower = (Nav & 2) != 0;
		Autoexploring = (Nav & 4) != 0;
		Flying = (Nav & 8) != 0;
		WallWalker = (Nav & 0x10) != 0;
		IgnoresWalls = (Nav & 0x20) != 0;
		Swimming = (Nav & 0x40) != 0;
		Slimewalking = (Nav & 0x80) != 0;
		Aquatic = (Nav & 0x100) != 0;
		Polypwalking = (Nav & 0x200) != 0;
		Strutwalking = (Nav & 0x400) != 0;
		Juggernaut = (Nav & 0x800) != 0;
		Reefer = (Nav & 0x1000) != 0;
		IgnoreCreatures = (Nav & 0x2000) != 0;
		IgnoreGases = (Nav & 0x4000) != 0;
		Unbreathing = (Nav & 0x8000) != 0;
		FilthAffinity = (Nav & 0x10000) != 0;
		OutOfPhase = (Nav & 0x20000) != 0;
		Omniphase = (Nav & 0x40000) != 0;
		Nullphase = (Nav & 0x80000) != 0;
		FlexPhase = (Nav & 0x100000) != 0;
	}

	public void CalculateNavigationMap(GameObject Looker, bool AddNoise = false, bool ExploredOnly = false, bool Autoexploring = false, bool Juggernaut = false, bool IgnoreCreatures = false, bool IgnoreGases = false, bool FlexPhase = false)
	{
		NoiseMap noiseMap = null;
		if (AddNoise)
		{
			noiseMap = new NoiseMap(80, 25, 10, 3, 3, 4, 80, 80, 6, 3, -3, 1, new List<NoiseMapNode>());
		}
		int nav = CalculateNav(Looker, Autoexploring, Juggernaut, IgnoreCreatures, IgnoreGases, FlexPhase);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (ExploredOnly && !GetExplored(j, i))
				{
					NavigationMap[j, i].Weight = 100;
					continue;
				}
				NavigationMap[j, i].Weight = Map[j][i].NavigationWeight(Looker, nav);
				if (AddNoise && noiseMap.Noise[j, i] > 1)
				{
					NavigationMap[j, i].Weight = 98;
				}
			}
		}
		if (!Options.DrawNavigationWeightMaps)
		{
			return;
		}
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		for (int k = 0; k < scrapBuffer.Width; k++)
		{
			for (int l = 0; l < scrapBuffer.Height; l++)
			{
				string text = Math.Min(NavigationMap[k, l].Weight / 6, 15).ToString("X");
				scrapBuffer.Goto(k, l);
				string text2 = "&g";
				if (NavigationMap[k, l].Weight == 0)
				{
					text2 = "&G";
				}
				if (NavigationMap[k, l].Weight >= 50)
				{
					text2 = "&W";
				}
				if (NavigationMap[k, l].Weight >= 98)
				{
					text2 = "&r";
					text = "E";
				}
				if (NavigationMap[k, l].Weight >= 100)
				{
					text2 = "&K";
					text = "-";
				}
				scrapBuffer.Write(text2 + text[0]);
			}
		}
		XRLCore._Console.DrawBuffer(scrapBuffer);
		Keyboard.getch();
	}

	public bool IsWorldMap()
	{
		if (ZoneID == null)
		{
			return false;
		}
		if (!_isWorldMapCache.HasValue)
		{
			_isWorldMapCache = ZoneID.IndexOf('.') == -1;
		}
		return _isWorldMapCache == true;
	}

	public string GetZoneIDFromDirection(string Direction, int n = 1)
	{
		int x = GetZoneX();
		int y = GetZoneY();
		int z = GetZoneZ();
		int wx = GetZonewX();
		int wy = GetZonewY();
		if (Direction == ".")
		{
			return ZoneID;
		}
		Directions.ApplyDirectionGlobal(Direction, ref x, ref y, ref z, ref wx, ref wy, n);
		if (wx < 0)
		{
			return null;
		}
		if (wx > 79)
		{
			return null;
		}
		if (wy < 0)
		{
			return null;
		}
		if (wy > 24)
		{
			return null;
		}
		return XRL.World.ZoneID.Assemble(GetZoneWorld(), wx, wy, x, y, z);
	}

	public Zone GetZoneAtLevel(int Level)
	{
		int zoneX = GetZoneX();
		int zoneY = GetZoneY();
		int zonewX = GetZonewX();
		int zonewY = GetZonewY();
		return The.ZoneManager.GetZone(GetZoneWorld(), zonewX, zonewY, zoneX, zoneY, Level);
	}

	public Zone GetZoneFromDirection(string Direction)
	{
		int x = GetZoneX();
		int y = GetZoneY();
		int z = GetZoneZ();
		int wx = GetZonewX();
		int wy = GetZonewY();
		if (Direction == ".")
		{
			return this;
		}
		Directions.ApplyDirectionGlobal(Direction, ref x, ref y, ref z, ref wx, ref wy);
		return The.ZoneManager.GetZone(GetZoneWorld(), wx, wy, x, y, z);
	}

	public Zone GetZoneFromDirection(string Direction, int n)
	{
		int x = GetZoneX();
		int y = GetZoneY();
		int z = GetZoneZ();
		int wx = GetZonewX();
		int wy = GetZonewY();
		if (Direction == ".")
		{
			return this;
		}
		Directions.ApplyDirectionGlobal(Direction, ref x, ref y, ref z, ref wx, ref wy, n);
		return The.ZoneManager.GetZone(GetZoneWorld(), wx, wy, x, y, z);
	}

	public virtual string ResolveZoneWorld()
	{
		return ZoneWorld;
	}

	public string GetZoneWorld()
	{
		return ZoneWorld;
	}

	public int GetZonewX()
	{
		return wX;
	}

	public int GetZonewY()
	{
		return wY;
	}

	public int GetZoneX()
	{
		return X;
	}

	public int GetZoneY()
	{
		return Y;
	}

	public int GetZoneZ()
	{
		return Z;
	}

	public bool GetVisibility(int x, int y)
	{
		return VisibilityMap[x + y * Width];
	}

	public LightLevel GetLight(int x, int y)
	{
		return LightMap[x + y * Width];
	}

	public bool GetExplored(int x, int y)
	{
		return ExploredMap[x + y * Width];
	}

	public void SetExplored(int x, int y, bool state)
	{
		ExploredMap[x + y * Width] = state;
	}

	[Obsolete("Legacy amnesia behaviour, use GetExplored")]
	public bool GetReallyExplored(int x, int y)
	{
		return ExploredMap[x + y * Width];
	}

	[Obsolete("Legacy amnesia behaviour, use SetExplored")]
	public void SetFakeUnexplored(int x, int y, bool state)
	{
	}

	[Obsolete("This is dangerous to use because the queued events will be overwritten. We'll probably remove or refactor this. -BB 7-16-22")]
	public void QueueEvent(Event E)
	{
		if (QueuedEvents == null)
		{
			QueuedEvents = new List<IEvent>();
		}
		QueuedEvents.Add(E);
	}

	private void _QueueEvent(Event E)
	{
		if (QueuedEvents == null)
		{
			QueuedEvents = new List<IEvent>();
		}
		QueuedEvents.Add(E);
	}

	private void QueueEvent(MinEvent E)
	{
		if (QueuedEvents == null)
		{
			QueuedEvents = new List<IEvent>();
		}
		QueuedEvents.Add(E);
	}

	public void QueueEvent(string ID)
	{
		_QueueEvent(new Event(ID));
	}

	private void QueueEventOnce(Event E)
	{
		if (QueuedEvents != null)
		{
			foreach (IEvent queuedEvent in QueuedEvents)
			{
				if (queuedEvent is Event obj && obj.ID == E.ID)
				{
					return;
				}
			}
		}
		_QueueEvent(E);
	}

	public void QueueEventOnce(string ID)
	{
		if (QueuedEvents != null)
		{
			foreach (IEvent queuedEvent in QueuedEvents)
			{
				if (queuedEvent is Event obj && obj.ID == ID)
				{
					return;
				}
			}
		}
		QueueEvent(ID);
	}

	private void QueueEventOnce(MinEvent E)
	{
		if (QueuedEvents != null)
		{
			foreach (IEvent queuedEvent in QueuedEvents)
			{
				if (queuedEvent is MinEvent minEvent && minEvent.ID == E.ID)
				{
					return;
				}
			}
		}
		QueueEvent(E);
	}

	public void CheckEventQueue()
	{
		if (QueuedEvents == null || QueuedEvents.Count <= 0)
		{
			return;
		}
		int num = 0;
		while (QueuedEvents.Count > 0)
		{
			if (++num >= 100)
			{
				MetricsManager.LogError("cyclic event queue on " + QueuedEvents[0].GetType().Name + ", clearing");
				QueuedEvents.Clear();
				break;
			}
			if (QueuedEventsToFire == null)
			{
				QueuedEventsToFire = new List<IEvent>(QueuedEvents.Count);
			}
			QueuedEventsToFire.Clear();
			QueuedEventsToFire.AddRange(QueuedEvents);
			QueuedEvents.Clear();
			foreach (IEvent item in QueuedEventsToFire)
			{
				try
				{
					if (item is Event e)
					{
						FireEvent(e);
					}
					else if (item is MinEvent minEvent && WantEvent(minEvent.GetID(), minEvent.GetCascadeLevel()))
					{
						if (minEvent is SynchronizeExistenceEvent e2)
						{
							HandleEvent(e2);
						}
						else
						{
							HandleEvent(minEvent);
						}
					}
				}
				catch (Exception ex)
				{
					XRLCore.LogError(ex.ToString(), "Queued event error");
				}
			}
		}
	}

	public bool IsAdjacentTo(Zone Z)
	{
		if (Math.Abs(Z.wX - wX) <= 1)
		{
			return Math.Abs(Z.wY - wY) <= 1;
		}
		return false;
	}

	public static int GetSuspendabilityTurns()
	{
		if (!Options.CacheEarly)
		{
			return 40;
		}
		return 5;
	}

	public Suspendability GetSuspendability(int Turns)
	{
		long currentTurn = XRLCore.CurrentTurn;
		if (LastCached == -1)
		{
			MetricsManager.LogError("Cache time uninitialized for '" + ZoneID + "', initializing now.");
			LastCached = currentTurn;
			return Suspendability.TooRecentlyActive;
		}
		if (!Built)
		{
			return Suspendability.TooRecentlyActive;
		}
		if (The.ZoneManager.ActiveZone == this)
		{
			return Suspendability.Active;
		}
		if (The.ZoneManager.PinnedZones != null)
		{
			if (The.ZoneManager.PinnedZones.Count > 3)
			{
				MetricsManager.LogException("ZonePins", new Exception("ZonePins"));
				The.ZoneManager.PinnedZones.Clear();
			}
			if (The.ZoneManager.PinnedZones.Contains(ZoneID))
			{
				return Suspendability.Pinned;
			}
		}
		if (currentTurn - LastActive <= Turns && !IsWorldMap())
		{
			return Suspendability.TooRecentlyActive;
		}
		Suspendability result = Suspendability.Suspendable;
		try
		{
			result = GetZoneSuspendabilityEvent.GetFor(this);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error getting suspendability of " + ZoneID, x);
		}
		return result;
	}

	public static int GetFreezabilityTurns()
	{
		return 0;
	}

	public Freezability GetFreezability(int Turns)
	{
		long currentTurn = XRLCore.CurrentTurn;
		if (LastCached == -1)
		{
			MetricsManager.LogError("Cache time uninitialized for '" + ZoneID + "', initializing now.");
			LastCached = currentTurn;
		}
		if (!Built || currentTurn - LastActive <= Turns || currentTurn == LastCached)
		{
			return Freezability.TooRecentlyActive;
		}
		if (IsWorldMap())
		{
			if (ZoneWorld == The.ActiveZone.ZoneWorld)
			{
				MarkActive();
				return Freezability.IsWorldMap;
			}
			if (currentTurn - LastActive <= 40)
			{
				return Freezability.TooRecentlyActive;
			}
		}
		Freezability result = Freezability.Freezable;
		try
		{
			result = GetZoneFreezabilityEvent.GetFor(this);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error getting freezability of " + ZoneID, x);
		}
		return result;
	}

	public Freezability GetFreezability()
	{
		return GetFreezability(GetFreezabilityTurns());
	}

	public bool GetFreezable(int Turns)
	{
		return GetFreezability(Turns) == Freezability.Freezable;
	}

	public bool GetFreezable()
	{
		return GetFreezable(GetFreezabilityTurns());
	}

	public virtual bool WantEvent(int ID, int cascade)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].WantEvent(ID, cascade))
				{
					return true;
				}
			}
		}
		if (!MinEvent.CascadeTo(cascade, 32))
		{
			for (int j = 0; j < Height; j++)
			{
				for (int k = 0; k < Width; k++)
				{
					if (Map[k][j].WantEvent(ID, cascade))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static MethodInfo getHandleEventMethod(Type HandlingType, Type HandledType)
	{
		if (!HandleEventLookup.TryGetValue(HandlingType, out var value))
		{
			value = new Dictionary<Type, MethodInfo>();
			HandleEventLookup.Add(HandlingType, value);
		}
		MethodInfo value2 = null;
		if (!value.TryGetValue(HandledType, out value2))
		{
			HandleEventMethodParameterList[0] = HandledType;
			value2 = HandlingType.GetMethod("HandleEvent", BindingFlags.Instance | BindingFlags.Public, null, HandleEventMethodParameterList, null);
			if (value2 != null && value2.GetParameters()[0].ParameterType.Name == "MinEvent")
			{
				value2 = null;
			}
			value.Add(HandledType, value2);
		}
		return value2;
	}

	public static bool callHandleEventMethod(object obj, MethodInfo method, MinEvent E)
	{
		if (method == null)
		{
			return true;
		}
		bool flag = false;
		object[] array;
		if (!HandleEventMethodArgumentList1InUse)
		{
			array = HandleEventMethodArgumentList1;
			HandleEventMethodArgumentList1InUse = true;
		}
		else if (!HandleEventMethodArgumentList2InUse)
		{
			array = HandleEventMethodArgumentList2;
			HandleEventMethodArgumentList2InUse = true;
		}
		else if (!HandleEventMethodArgumentList3InUse)
		{
			array = HandleEventMethodArgumentList3;
			HandleEventMethodArgumentList3InUse = true;
		}
		else
		{
			array = new object[1];
			flag = true;
		}
		array[0] = E;
		try
		{
			return (bool)method.Invoke(obj, array);
		}
		catch (ArgumentException ex)
		{
			UnityEngine.Debug.LogError("failed to call event handler " + obj.GetType().FullName + "::" + method.Name + "(" + method.GetParameters()[0].ParameterType.Name + ") with parameter " + E.GetType().FullName);
			throw ex;
		}
		finally
		{
			array[0] = null;
			if (!flag)
			{
				if (array == HandleEventMethodArgumentList1)
				{
					HandleEventMethodArgumentList1InUse = false;
				}
				else if (array == HandleEventMethodArgumentList2)
				{
					HandleEventMethodArgumentList2InUse = false;
				}
				else if (array == HandleEventMethodArgumentList3)
				{
					HandleEventMethodArgumentList3InUse = false;
				}
			}
		}
	}

	public virtual bool HandleEvent(MinEvent E)
	{
		if (Parts != null)
		{
			int iD = E.GetID();
			int cascadeLevel = E.GetCascadeLevel();
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				IZonePart zonePart = Parts[i];
				if (!zonePart.WantEvent(iD, cascadeLevel))
				{
					continue;
				}
				if (!E.Dispatch(zonePart))
				{
					return false;
				}
				if (!zonePart.HandleEvent(E))
				{
					return false;
				}
				if (count != Parts.Count)
				{
					count = Parts.Count;
					if (i < count && Parts[i] != zonePart)
					{
						i--;
					}
				}
			}
		}
		if (!E.CascadeTo(32))
		{
			for (int j = 0; j < Height; j++)
			{
				for (int k = 0; k < Width; k++)
				{
					if (!Map[k][j].HandleEvent(E))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public bool HandleEvent<T>(T E, IEvent ParentEvent) where T : MinEvent
	{
		bool result = HandleEvent(E);
		ParentEvent?.ProcessChildEvent(E);
		return result;
	}

	public void GetObjectsThatWantEvent(int ID, int Cascade, ICollection<GameObject> Result)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell.ObjectRack objects = Map[j][i].Objects;
				GameObject[] array = objects.GetArray();
				int k = 0;
				for (int count = objects.Count; k < count; k++)
				{
					if (array[k].WantEvent(ID, Cascade))
					{
						Result.Add(array[k]);
					}
				}
			}
		}
	}

	public List<GameObject> GetObjectsThatWantEvent(int ID, int Cascade)
	{
		List<GameObject> result = Event.NewGameObjectList();
		GetObjectsThatWantEvent(ID, Cascade, result);
		return result;
	}

	public bool AnyObjectsWantEvent(int ID, int cascade)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell.ObjectRack objects = Map[j][i].Objects;
				int k = 0;
				for (int count = objects.Count; k < count; k++)
				{
					if (objects[k].WantEvent(ID, cascade))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void GetWantEventHandlers(int ID, int Cascade, Rack<IEventHandler> Result)
	{
		List<IZonePart> parts = Parts;
		if (parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = parts.Count; i < count; i++)
		{
			IZonePart zonePart = parts[i];
			if (zonePart.WantEvent(ID, Cascade))
			{
				Result.Add(zonePart);
			}
		}
	}

	public void GetContentWantEventHandlers(int ID, int Cascade, ICollection<IEventHandler> Result)
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell.ObjectRack objects = Map[j][i].Objects;
				GameObject[] array = objects.GetArray();
				int k = 0;
				for (int count = objects.Count; k < count; k++)
				{
					if (array[k].WantEvent(ID, Cascade))
					{
						Result.Add(array[k]);
					}
				}
			}
		}
	}

	public void Thawed(long FrozenTicks = 0L)
	{
		ZoneThawedEvent.Send(this, FrozenTicks);
	}

	public virtual void Activated()
	{
		MarkActive();
		try
		{
			ZoneActivatedEvent.Send(this);
			ePrimePowerSystems.Zone = this;
			HandleEvent(ePrimePowerSystems);
			ePrimePowerSystems.Reset();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("ZoneActivation", x);
		}
	}

	public virtual void Deactivated()
	{
		ZoneDeactivatedEvent.Send(this);
	}

	public void WantSynchronizeExistence()
	{
		QueueEventOnce(eSynchronizeExistence);
	}

	public void CheckWeather(long TurnNumber)
	{
		if (HasWeather && TurnNumber > NextWindChange)
		{
			WindChange(TurnNumber);
		}
	}

	public void CheckWeather()
	{
		CheckWeather(The.Game.TimeTicks);
	}

	public void WindChange(long TurnNumber)
	{
		if (string.IsNullOrEmpty(WindDirections))
		{
			return;
		}
		int currentWindSpeed = CurrentWindSpeed;
		string currentWindDirection = CurrentWindDirection;
		string windSpeedDescription = GetWindSpeedDescription(currentWindSpeed);
		int num = (CurrentWindSpeed = WindSpeed.RollCached());
		int num3 = num;
		string text = (CurrentWindDirection = WindDirections.CachedCommaExpansion().GetRandomElement());
		string d = text;
		NextWindChange = TurnNumber + WindDuration.RollCached();
		if (!IsActive() || !The.Player.HasSkill("Survival"))
		{
			return;
		}
		string windSpeedDescription2 = GetWindSpeedDescription(num3);
		if (windSpeedDescription == windSpeedDescription2 && !currentWindDirection.IsNullOrEmpty())
		{
			if (currentWindDirection != CurrentWindDirection)
			{
				if (The.Player.HasSkill("Survival_Trailblazer"))
				{
					MessageQueue.AddPlayerMessage("The wind changes direction from the " + Directions.GetExpandedDirection(currentWindDirection) + " to the " + Directions.GetExpandedDirection(d) + ".");
				}
				else
				{
					MessageQueue.AddPlayerMessage("The wind changes direction.");
				}
			}
		}
		else if (The.Player.HasSkill("Survival_Trailblazer"))
		{
			if (windSpeedDescription2 == null)
			{
				MessageQueue.AddPlayerMessage("The wind becomes still.");
			}
			else if (windSpeedDescription == null)
			{
				MessageQueue.AddPlayerMessage("The wind begins blowing at " + windSpeedDescription2 + " from the " + Directions.GetExpandedDirection(d) + ".");
			}
			else if (num3 > currentWindSpeed)
			{
				MessageQueue.AddPlayerMessage("The wind intensifies to " + windSpeedDescription2 + ", blowing from the " + Directions.GetExpandedDirection(d) + ".");
			}
			else if (num3 < currentWindSpeed)
			{
				MessageQueue.AddPlayerMessage("The wind calms to " + windSpeedDescription2 + ", blowing from the " + Directions.GetExpandedDirection(d) + ".");
			}
		}
		else if (windSpeedDescription2 == null)
		{
			MessageQueue.AddPlayerMessage("The wind becomes still.");
		}
		else if (windSpeedDescription == null)
		{
			MessageQueue.AddPlayerMessage("The wind begins blowing at " + windSpeedDescription2 + ".");
		}
		else if (num3 > currentWindSpeed)
		{
			MessageQueue.AddPlayerMessage("The wind intensifies to " + windSpeedDescription2 + ".");
		}
		else if (num3 < currentWindSpeed)
		{
			MessageQueue.AddPlayerMessage("The wind calms to " + windSpeedDescription2 + ".");
		}
	}

	public void WindChange()
	{
		CheckWeather(The.Game.TimeTicks);
	}

	public static string GetWindSpeedDescription(int kph)
	{
		if (kph <= 0)
		{
			return null;
		}
		if (kph <= 10)
		{
			return "a very gentle breeze";
		}
		if (kph <= 20)
		{
			return "a gentle breeze";
		}
		if (kph <= 30)
		{
			return "a moderate breeze";
		}
		if (kph <= 40)
		{
			return "a fresh breeze";
		}
		if (kph <= 50)
		{
			return "a strong breeze";
		}
		if (kph <= 60)
		{
			return "near gale intensity";
		}
		if (kph <= 70)
		{
			return "gale intensity";
		}
		if (kph <= 90)
		{
			return "strong gale intensity";
		}
		if (kph <= 100)
		{
			return "storm intensity";
		}
		if (kph <= 120)
		{
			return "violent storm intensity";
		}
		return "hurricane intensity";
	}

	public void FlushNavigationCaches()
	{
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Map[j][i].FlushNavigationCache();
			}
		}
	}

	public string GetDirectionFromZone(Zone From)
	{
		if (From == null || From == this)
		{
			return ".";
		}
		if (Z != From.Z)
		{
			if (Z < From.Z)
			{
				return "U";
			}
			if (Z > From.Z)
			{
				return "D";
			}
			return ".";
		}
		if (ResolvedX == From.ResolvedX)
		{
			if (ResolvedY == From.ResolvedY)
			{
				return ".";
			}
			if (ResolvedY > From.ResolvedY)
			{
				return "S";
			}
			return "N";
		}
		if (ResolvedX > From.ResolvedX)
		{
			if (ResolvedY == From.ResolvedY)
			{
				return "E";
			}
			if (ResolvedY > From.ResolvedY)
			{
				return "SE";
			}
			return "NE";
		}
		if (ResolvedY == From.ResolvedY)
		{
			return "W";
		}
		if (ResolvedY > From.ResolvedY)
		{
			return "SW";
		}
		return "NW";
	}

	public void Constrain(ref int x, ref int y)
	{
		if (x < 0)
		{
			x = 0;
		}
		if (y < 0)
		{
			y = 0;
		}
		if (x >= Width)
		{
			x = Width - 1;
		}
		if (y >= Height)
		{
			y = Height - 1;
		}
	}

	public void Constrain(ref int x1, ref int y1, ref int x2, ref int y2)
	{
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (x2 < 0)
		{
			x2 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (y2 < 0)
		{
			y2 = 0;
		}
		if (x1 >= Width)
		{
			x1 = Width - 1;
		}
		if (x2 >= Width)
		{
			x2 = Width - 1;
		}
		if (y1 >= Height)
		{
			y1 = Height - 1;
		}
		if (y2 >= Height)
		{
			y2 = Height - 1;
		}
	}

	public Cell GetPullDownLocation(GameObject who)
	{
		GetTransitiveLocationEvent.GetFor(this, "PullDown,WorldMap", who, who, who.CurrentCell, out var Destination, out var _);
		if (Destination != null)
		{
			return Destination;
		}
		try
		{
			string text = GetZoneProperty("PullDownLocation", null) ?? GetZoneProperty("pulldownLocation");
			if (!text.IsNullOrEmpty() && text.IndexOf(',') != -1)
			{
				text.AsDelimitedSpans(',', out var First, out var Second);
				Cell cell = GetCell(int.Parse(First), int.Parse(Second));
				if (cell != null && cell.IsPassable(who))
				{
					return cell;
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception processing pulldown location", x);
		}
		for (int num = Height - 1; num >= 0; num--)
		{
			for (int i = Width / 2; i < Width - 1; i++)
			{
				Cell cell = GetCell(i, num);
				if (cell != null && cell.IsReachable() && cell.IsPassable(who) && !cell.HasSwimmingDepthLiquid() && !cell.HasSpawnBlocker())
				{
					return cell;
				}
				cell = GetCell(Width - 1 - i, num);
				if (cell != null && cell.IsReachable() && cell.IsPassable(who) && !cell.HasSwimmingDepthLiquid() && !cell.HasSpawnBlocker())
				{
					return cell;
				}
			}
		}
		for (int num2 = Height - 1; num2 >= 0; num2--)
		{
			for (int j = Width / 2; j < Width - 1; j++)
			{
				Cell cell = GetCell(j, num2);
				if (cell != null && cell.IsReachable() && cell.IsPassable(who))
				{
					return cell;
				}
				cell = GetCell(Width - 1 - j, num2);
				if (cell != null && cell.IsReachable() && cell.IsPassable(who))
				{
					return cell;
				}
			}
		}
		for (int num3 = Height - 1; num3 >= 0; num3--)
		{
			for (int k = Width / 2; k < Width - 1; k++)
			{
				Cell cell = GetCell(k, num3);
				if (cell != null && cell.IsPassable(who))
				{
					return cell;
				}
				cell = GetCell(Width - 1 - k, num3);
				if (cell != null && cell.IsPassable(who))
				{
					return cell;
				}
			}
		}
		return GetCell(Width / 2, Height - 1);
	}

	public float GetCoverAt(int X, int Y)
	{
		if (X >= 0 && Y >= 0 && X < Width && Y <= Height)
		{
			switch (MissileMap[X][Y])
			{
			case MissileMapType.Wall:
				return 1f;
			case MissileMapType.VeryHeavyCover:
				return 0.8f;
			case MissileMapType.HeavyCover:
				return 0.5f;
			case MissileMapType.MediumCover:
				return 0.3f;
			case MissileMapType.LightCover:
				return 0.1f;
			case MissileMapType.VeryLightCover:
				return 0.05f;
			}
		}
		return 0f;
	}

	public T GetPart<T>() where T : IZonePart
	{
		if (Parts != null)
		{
			string name = typeof(T).Name;
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Name == name)
				{
					return Parts[i] as T;
				}
			}
		}
		return null;
	}

	public IZonePart GetPart(string Name)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Name == Name)
				{
					return Parts[i];
				}
			}
		}
		return null;
	}

	public IZonePart GetPartExcept(string Name, IZonePart Skip)
	{
		if (Parts != null)
		{
			if (Skip == null)
			{
				return GetPart(Name);
			}
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Name == Name && Parts[i] != Skip)
				{
					return Parts[i];
				}
			}
		}
		return null;
	}

	public bool TryGetPart<T>(out T Part) where T : IZonePart
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if ((object)Parts[i].GetType() == typeof(T))
				{
					Part = Parts[i] as T;
					return true;
				}
			}
		}
		Part = null;
		return false;
	}

	protected void AddPartInternals(IZonePart Part, bool Initial = false, bool Creation = false)
	{
		FlushWantTurnTickCache();
		if (Part != null)
		{
			if (Parts == null)
			{
				Parts = new List<IZonePart>();
			}
			Parts.Add(Part);
			Part.ParentZone = this;
			if (Initial)
			{
				Part.Initialize();
			}
			Part.Attach();
			if (!Creation)
			{
				Part.AddedAfterCreation();
			}
		}
	}

	public IZonePart AddPart(IZonePart Part, bool Creation = false)
	{
		AddPartInternals(Part, Initial: true, Creation);
		return Part;
	}

	public T AddPart<T>(T P, bool Creation = false) where T : IZonePart
	{
		return AddPart((IZonePart)P, Creation) as T;
	}

	public T AddPart<T>(bool Creation = false) where T : IZonePart, new()
	{
		return AddPart((IZonePart)new T(), Creation) as T;
	}

	public T RequirePart<T>(bool Creation = false) where T : IZonePart, new()
	{
		T part = GetPart<T>();
		if (part != null)
		{
			return part;
		}
		return AddPart<T>(Creation);
	}

	public IEnumerable<IZonePart> LoopParts()
	{
		if (Parts == null)
		{
			yield break;
		}
		foreach (IZonePart part in Parts)
		{
			yield return part;
		}
	}

	public void ForeachPart(Action<IZonePart> Proc)
	{
		if (Proc != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Proc(Parts[i]);
			}
		}
	}

	public bool ForeachPart(Predicate<IZonePart> Proc)
	{
		if (Proc != null)
		{
			foreach (IZonePart part in Parts)
			{
				if (!Proc(part))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ForeachPartDescendedFrom<T>(Action<T> Proc) where T : IZonePart
	{
		if (Parts == null)
		{
			return;
		}
		foreach (IZonePart part in Parts)
		{
			if (part is T obj)
			{
				Proc(obj);
			}
		}
	}

	public bool ForeachPartDescendedFrom<T>(Predicate<T> Proc) where T : IPart
	{
		if (Parts != null)
		{
			foreach (IZonePart part in Parts)
			{
				if (part is T obj && !Proc(obj))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool RemovePart(Type Part)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].GetType() == Part && RemovePart(Parts[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool RemovePart(string Part)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Name == Part && RemovePart(Parts[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool RemovePart<T>() where T : IZonePart
	{
		IZonePart part = GetPart<T>();
		if (part != null)
		{
			return RemovePart(part);
		}
		return false;
	}

	public bool RemovePart(IZonePart Part)
	{
		FlushWantTurnTickCache();
		if (Part == null)
		{
			return false;
		}
		if (Parts == null)
		{
			return false;
		}
		if (!Parts.Remove(Part))
		{
			return false;
		}
		Part.Remove();
		Part.ParentZone = null;
		return true;
	}

	public static bool Validate(Zone Zone)
	{
		if (Zone != null)
		{
			return !Zone.Stale;
		}
		return false;
	}

	public static bool Validate(ref Zone Zone)
	{
		if (Zone != null)
		{
			if (!Zone.Stale)
			{
				return true;
			}
			Zone = null;
		}
		return false;
	}

	public void FlushWantTurnTickCache()
	{
		WantTurnTickCache = 0;
	}

	public bool WantTurnTick()
	{
		if (Parts != null)
		{
			if ((WantTurnTickCache & 1) != 0)
			{
				return true;
			}
			if ((WantTurnTickCache & 2) != 0)
			{
				return false;
			}
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].WantTurnTick())
				{
					WantTurnTickCache |= 1;
					return true;
				}
			}
			WantTurnTickCache |= 2;
		}
		return false;
	}

	public void TurnTick(long TimeTick, int Amount)
	{
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			IZonePart zonePart = Parts[i];
			zonePart.TurnTick(TimeTick, Amount);
			if (count != Parts.Count)
			{
				count = Parts.Count;
				if (i < count && Parts[i] != zonePart)
				{
					i--;
				}
			}
		}
	}

	[Obsolete("Use FindObjectById")]
	public GameObject findObjectById(string id)
	{
		return FindObjectByID(id);
	}
}
