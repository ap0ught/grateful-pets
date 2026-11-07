using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using ConsoleLib.Console;
using Genkit;
using HistoryKit;
using Mono.Data.Sqlite;
using Qud.API;
using UnityEngine;
using XRL.Collections;
using XRL.Core;
using XRL.Language;
using XRL.Liquids;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Biomes;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.ZoneBuilders;

namespace XRL.World;

[Serializable]
[HasGameBasedStaticCache]
public class ZoneManager
{
	private class xyPair
	{
		public int x;

		public int y;

		public GameObject GO;

		public xyPair(int _x, int _y, GameObject _GO)
		{
			x = _x;
			y = _y;
			GO = _GO;
		}
	}

	[NonSerialized]
	public XRLGame Game;

	[NonSerialized]
	public Dictionary<string, Zone> CachedZones = new Dictionary<string, Zone>();

	[NonSerialized]
	public Zone ActiveZone;

	[NonSerialized]
	public Graveyard Graveyard = new Graveyard(256);

	[NonSerialized]
	public long LastZoneTransition;

	[NonSerialized]
	private Dictionary<string, GameObject> TerrainSamples;

	[NonSerialized]
	private Dictionary<string, List<ZoneConnection>> ZoneConnections = new Dictionary<string, List<ZoneConnection>>();

	[NonSerialized]
	public Dictionary<string, GameObject> CachedObjects = new Dictionary<string, GameObject>();

	[NonSerialized]
	public List<string> CachedObjectsToRemoveAfterZoneBuild = new List<string>();

	[GameBasedStaticCache(true, false)]
	private static HashSet<string> FrozenZones = new HashSet<string>();

	[GameBasedStaticCache(true, false)]
	private static List<string> FreezingZones = new List<string>();

	[NonSerialized]
	public Dictionary<string, Dictionary<string, object>> ZoneProperties = new Dictionary<string, Dictionary<string, object>>();

	[NonSerialized]
	public Dictionary<string, ZoneBuilderCollection> ZoneBuilders = new Dictionary<string, ZoneBuilderCollection>();

	[NonSerialized]
	public Dictionary<string, ZonePartCollection> ZoneParts = new Dictionary<string, ZonePartCollection>();

	public static long Ticker = 0L;

	[NonSerialized]
	private static List<Zone> FreezableZones = new List<Zone>();

	[NonSerialized]
	private static bool CheckingCache;

	[NonSerialized]
	public Dictionary<string, long> VisitedTime = new Dictionary<string, long>();

	public List<string> PinnedZones = new List<string>();

	[NonSerialized]
	[GameBasedStaticCache(true, false)]
	public static int ZoneTransitionCount = 0;

	[NonSerialized]
	private static StringBuilder SB = new StringBuilder();

	[NonSerialized]
	public int NameUpdateTick;

	[NonSerialized]
	public static Zone ZoneGenerationContext = null;

	[NonSerialized]
	public static int zoneGenerationContextTier = 1;

	[NonSerialized]
	public static string zoneGenerationContextZoneID = "none";

	[NonSerialized]
	private static Stack<ZonePartCollection> PartCollections = new Stack<ZonePartCollection>();

	[NonSerialized]
	private static Stack<ZoneBuilderCollection> BuilderCollections = new Stack<ZoneBuilderCollection>();

	[NonSerialized]
	private static HashSet<string> ProcessingZones = new HashSet<string>();

	[NonSerialized]
	private static GameObject[,] WallSingleTrack = new GameObject[80, 25];

	[NonSerialized]
	private static List<GameObject>[,] WallMultiTrack = new List<GameObject>[80, 25];

	private static StringBuilder hb = new StringBuilder();

	[NonSerialized]
	private static GameObject[,] LiquidTrack = new GameObject[80, 25];

	public static ZoneManager instance => The.Game.ZoneManager;

	public ZoneManager()
	{
	}

	public ZoneManager(XRLGame Game)
	{
		this.Game = Game;
	}

	public GameObject FindObjectByID(string ID)
	{
		GameObject gameObject = ActiveZone.FindObjectByID(ID);
		if (gameObject != null)
		{
			return gameObject;
		}
		foreach (Zone value in CachedZones.Values)
		{
			if (value != ActiveZone)
			{
				gameObject = value.FindObjectByID(ID);
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
		GameObject gameObject = ActiveZone.FindObjectByID(ID);
		if (gameObject != null)
		{
			return gameObject;
		}
		foreach (Zone value in CachedZones.Values)
		{
			if (value != ActiveZone)
			{
				gameObject = value.FindObjectByID(ID);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public void FindObjects(List<GameObject> Store, Predicate<GameObject> Filter = null)
	{
		if (ActiveZone == null)
		{
			return;
		}
		ActiveZone.FindObjects(Store, Filter);
		foreach (Zone value in CachedZones.Values)
		{
			if (value != ActiveZone)
			{
				value.FindObjects(Store, Filter);
			}
		}
	}

	public List<GameObject> FindObjects(Predicate<GameObject> Filter = null)
	{
		List<GameObject> list = new List<GameObject>();
		FindObjects(list, Filter);
		return list;
	}

	public List<GameObject> FindObjectsReadonly(Predicate<GameObject> Filter = null)
	{
		List<GameObject> list = Event.NewGameObjectList();
		FindObjects(list, Filter);
		return list;
	}

	public void Release()
	{
		foreach (KeyValuePair<string, Zone> cachedZone in CachedZones)
		{
			cachedZone.Value.Release();
		}
		if (!TerrainSamples.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, GameObject> terrainSample in TerrainSamples)
			{
				terrainSample.Value.Pool();
			}
			TerrainSamples.Clear();
		}
		Graveyard.ReleaseObjects();
		ActiveZone?.Release();
		CachedZones.Clear();
		ActiveZone = null;
	}

	public bool CachedZonesContains(string ZoneID)
	{
		if (CachedZones == null)
		{
			return false;
		}
		Dictionary<string, Zone>.Enumerator enumerator = CachedZones.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string key = enumerator.Current.Key;
				if (ZoneID == key)
				{
					return true;
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		return false;
	}

	public void AddCachedZone(Zone Zone)
	{
		CachedZones[Zone.ZoneID] = Zone;
		Zone.LastCached = XRLCore.CurrentTurn;
	}

	public GameObject peekCachedObject(string ID)
	{
		if (ID != null && CachedObjects.TryGetValue(ID, out var value))
		{
			return value;
		}
		return null;
	}

	public GameObject PullCachedObject(string ID, bool DeepCopy = true)
	{
		if (ID != null && CachedObjects.TryGetValue(ID, out var value))
		{
			GameObject result = (DeepCopy ? value.DeepCopy(CopyEffects: false, CopyID: true) : value);
			CachedObjects.Remove(ID);
			return result;
		}
		return null;
	}

	public void UncacheObject(string ID)
	{
		CachedObjects.Remove(ID);
	}

	public GameObject GetCachedObjects(string ID)
	{
		if (ID == null || !CachedObjects.ContainsKey(ID))
		{
			GameObject gameObject = GameObject.Create("PhysicalObject");
			gameObject.Render.DisplayName = "INVALID CACHE OBJECT: " + ID;
			gameObject.Render.ColorString = "&M^W";
			gameObject.Render.DetailColor = "W";
			return gameObject;
		}
		CachedObjectsToRemoveAfterZoneBuild.Add(ID);
		return CachedObjects[ID].DeepCopy(CopyEffects: false, CopyID: true);
	}

	public string CacheObject(GameObject GO, bool cacheTwiceOk = false, bool replaceIfAlreadyCached = false)
	{
		if (CachedObjects.ContainsKey(GO.ID))
		{
			if (replaceIfAlreadyCached)
			{
				CachedObjects[GO.ID] = GO;
				return GO.ID;
			}
			if (!cacheTwiceOk)
			{
				Debug.LogWarning("Did you mean to cache " + GO.DisplayName + " twice?");
			}
			return GO.ID;
		}
		CachedObjects.Add(GO.ID, GO);
		return GO.ID;
	}

	public static string GetObjectTypeForZone(string zoneId)
	{
		if (!ZoneID.Parse(zoneId, out var World, out var ParasangX, out var ParasangY))
		{
			return "";
		}
		Cell cell = The.Game.ZoneManager.GetZone(World).GetCell(ParasangX, ParasangY);
		if (cell == null)
		{
			return "";
		}
		GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("TerrainTravel");
		if (firstObjectWithPart == null)
		{
			return "";
		}
		return firstObjectWithPart.Blueprint;
	}

	public static string GetObjectTypeForZone(int wx, int wy, string World)
	{
		Cell cell = The.Game.ZoneManager.GetZone(World).GetCell(wx, wy);
		if (cell == null)
		{
			return "";
		}
		GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("TerrainTravel");
		if (firstObjectWithPart == null)
		{
			return "";
		}
		return firstObjectWithPart.Blueprint;
	}

	public static GameObject GetTerrainObjectForZone(int wx, int wy, string World)
	{
		return The.Game.ZoneManager.GetZone(World)?.GetCell(wx, wy)?.GetFirstObjectWithPart("TerrainTravel");
	}

	public GameObject GetTerrainSample(string Terrain)
	{
		if (TerrainSamples != null)
		{
			if (TerrainSamples.TryGetValue(Terrain, out var value))
			{
				return value;
			}
		}
		else
		{
			TerrainSamples = new Dictionary<string, GameObject>();
		}
		GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint("Terrain" + Terrain);
		if (blueprint != null)
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateSampleObject(blueprint);
			TerrainSamples[Terrain] = gameObject;
			return gameObject;
		}
		return null;
	}

	public static Location2D GetWorldMapLocationForZoneID(string zoneID)
	{
		if (zoneID == null)
		{
			return Location2D.Invalid;
		}
		ZoneID.Parse(zoneID, out var _, out var ParasangX, out var ParasangY);
		return Location2D.Get(ParasangX, ParasangY);
	}

	public static string GetRegionForZone(Zone z)
	{
		Cell cell = The.Game.ZoneManager.GetZone(z.GetZoneWorld()).GetCell(z.wX, z.wY);
		if (cell == null)
		{
			return null;
		}
		GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("TerrainTravel");
		return firstObjectWithPart?.GetTag("Terrain", firstObjectWithPart.Blueprint);
	}

	public IEnumerable<ZoneBuilderBlueprint> GetBuildersFor(Zone Z)
	{
		return GetBuildersFor(Z.ZoneID);
	}

	public IEnumerable<ZoneBuilderBlueprint> GetBuildersFor(string ZoneID)
	{
		return GetBuilderCollection(ZoneID);
	}

	public int CountPartsFor(string ZoneID, Predicate<ZonePartBlueprint> Filter = null)
	{
		int result = 0;
		if (ZoneParts.TryGetValue(ZoneID, out var value))
		{
			result = ((Filter == null) ? value.Count : Enumerable.Count(value, Filter.Invoke));
		}
		return result;
	}

	public int CountBuildersFor(string ZoneID, Predicate<ZoneBuilderBlueprint> Filter = null)
	{
		int result = 0;
		if (ZoneBuilders.TryGetValue(ZoneID, out var value))
		{
			result = ((Filter == null) ? value.Count : Enumerable.Count(value, Filter.Invoke));
		}
		return result;
	}

	public bool ZoneHasPart(string ZoneID, string Name)
	{
		if (ZoneParts.TryGetValue(ZoneID, out var value))
		{
			return value.Any((ZonePartBlueprint x) => x.Name == Name);
		}
		return false;
	}

	public bool ZoneHasBuilder(string ZoneID, string Class)
	{
		if (ZoneBuilders.TryGetValue(ZoneID, out var value))
		{
			return value.Any((ZoneBuilderBlueprint x) => x.Class == Class);
		}
		return false;
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.WriteFields(this);
		Writer.Write(CachedZones.Count);
		foreach (KeyValuePair<string, Zone> cachedZone in CachedZones)
		{
			Zone.Save(Writer, cachedZone.Value);
		}
		Writer.Write(LastZoneTransition);
		Writer.Write(FrozenZones.Count);
		foreach (string frozenZone in FrozenZones)
		{
			Writer.WriteOptimized(frozenZone);
		}
		Writer.Write(PinnedZones.Count);
		foreach (string pinnedZone in PinnedZones)
		{
			Writer.WriteOptimized(pinnedZone);
		}
		Writer.Write(VisitedTime.Count);
		foreach (KeyValuePair<string, long> item in VisitedTime)
		{
			Writer.WriteOptimized(item.Key);
			Writer.WriteOptimized(item.Value);
		}
		Writer.Write(Ticker);
		Writer.Write(ZoneConnections.Count);
		foreach (KeyValuePair<string, List<ZoneConnection>> zoneConnection in ZoneConnections)
		{
			Writer.WriteOptimized(zoneConnection.Key);
			Writer.WriteOptimized(zoneConnection.Value.Count);
			foreach (ZoneConnection item2 in zoneConnection.Value)
			{
				Writer.WriteOptimized(item2.Object);
				Writer.WriteOptimized(item2.Type);
				Writer.WriteOptimized(item2.X);
				Writer.WriteOptimized(item2.Y);
			}
		}
		Writer.Write(ZoneProperties.Count);
		foreach (KeyValuePair<string, Dictionary<string, object>> zoneProperty in ZoneProperties)
		{
			Writer.WriteOptimized(zoneProperty.Key);
			Writer.WriteOptimized(zoneProperty.Value.Count);
			foreach (KeyValuePair<string, object> item3 in zoneProperty.Value)
			{
				Writer.WriteOptimized(item3.Key);
				Writer.WriteObject(item3.Value);
			}
		}
		Writer.Write(ActiveZone.ZoneID);
		Writer.Write(CachedObjects.Count);
		foreach (KeyValuePair<string, GameObject> cachedObject in CachedObjects)
		{
			Writer.WriteOptimized(cachedObject.Key);
			if (cachedObject.Value.Physics != null)
			{
				cachedObject.Value.Physics._CurrentCell = null;
				cachedObject.Value.Physics._Equipped = null;
				cachedObject.Value.Physics._InInventory = null;
			}
			Writer.WriteGameObject(cachedObject.Value);
		}
		Hills.Save(Writer);
		BananaGrove.Save(Writer);
		Watervine.Save(Writer);
		Cave.Save(Writer);
		SaveZoneParts(Writer);
		SaveZoneBuilders(Writer);
	}

	public void SaveZoneParts(SerializationWriter Writer)
	{
		try
		{
			Writer.Write(ZoneParts.Count);
			foreach (ZonePartCollection value in ZoneParts.Values)
			{
				ZonePartCollection.Save(Writer, value);
			}
		}
		finally
		{
			ZonePartBlueprint.ClearTokens();
		}
	}

	public void SaveZoneBuilders(SerializationWriter Writer)
	{
		try
		{
			Writer.Write(ZoneBuilders.Count);
			foreach (ZoneBuilderCollection value in ZoneBuilders.Values)
			{
				ZoneBuilderCollection.Save(Writer, value);
			}
		}
		finally
		{
			ZoneBuilderBlueprint.ClearTokens();
		}
	}

	public void LoadZoneParts(SerializationReader Reader)
	{
		try
		{
			int num = Reader.ReadInt32();
			List<ZonePartBlueprint> blueprints = new List<ZonePartBlueprint>(num);
			if (ZoneParts == null)
			{
				ZoneParts = new Dictionary<string, ZonePartCollection>(num);
			}
			ZoneParts.EnsureCapacity(num);
			for (int i = 0; i < num; i++)
			{
				ZonePartCollection zonePartCollection = ZonePartCollection.Load(Reader, blueprints);
				ZoneParts[zonePartCollection.ZoneID] = zonePartCollection;
			}
		}
		finally
		{
			ZonePartBlueprint.ClearOrphaned();
			ZonePartBlueprint.ClearTokens();
		}
	}

	public void LoadZoneBuilders(SerializationReader Reader)
	{
		try
		{
			int num = Reader.ReadInt32();
			List<ZoneBuilderBlueprint> blueprints = new List<ZoneBuilderBlueprint>(num);
			if (ZoneBuilders == null)
			{
				ZoneBuilders = new Dictionary<string, ZoneBuilderCollection>(num);
			}
			ZoneBuilders.EnsureCapacity(num);
			for (int i = 0; i < num; i++)
			{
				ZoneBuilderCollection zoneBuilderCollection = ZoneBuilderCollection.Load(Reader, blueprints);
				ZoneBuilders[zoneBuilderCollection.ZoneID] = zoneBuilderCollection;
			}
		}
		finally
		{
			ZoneBuilderBlueprint.ClearOrphaned();
			ZoneBuilderBlueprint.ClearTokens();
		}
	}

	public static ZoneManager Load(XRLGame Game, SerializationReader Reader)
	{
		ZoneManager zoneManager = Reader.ReadInstanceFields<ZoneManager>();
		int num = Reader.ReadInt32();
		zoneManager.Game = Game;
		zoneManager.CachedZones.Clear();
		zoneManager.CachedZones.EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			Zone zone = Zone.Load(Reader, null);
			if (!zoneManager.CachedZones.TryAdd(zone.ZoneID, zone))
			{
				MetricsManager.LogWarning("Duplicate zone ID found on load: " + zone.ZoneID);
			}
		}
		zoneManager.LastZoneTransition = Reader.ReadInt64();
		num = Reader.ReadInt32();
		FrozenZones.Clear();
		FrozenZones.EnsureCapacity(num);
		for (int j = 0; j < num; j++)
		{
			FrozenZones.Add(Reader.ReadOptimizedString());
		}
		num = Reader.ReadInt32();
		zoneManager.PinnedZones.Clear();
		zoneManager.PinnedZones.EnsureCapacity(num);
		for (int k = 0; k < num; k++)
		{
			zoneManager.PinnedZones.Add(Reader.ReadOptimizedString());
		}
		num = Reader.ReadInt32();
		zoneManager.VisitedTime.Clear();
		zoneManager.VisitedTime.EnsureCapacity(num);
		for (int l = 0; l < num; l++)
		{
			zoneManager.VisitedTime[Reader.ReadOptimizedString()] = Reader.ReadOptimizedInt64();
		}
		Ticker = Reader.ReadInt64();
		num = Reader.ReadInt32();
		zoneManager.ZoneConnections.Clear();
		zoneManager.ZoneConnections.EnsureCapacity(num);
		for (int m = 0; m < num; m++)
		{
			string key = Reader.ReadOptimizedString();
			int num2 = Reader.ReadOptimizedInt32();
			List<ZoneConnection> list = new List<ZoneConnection>(num2);
			for (int n = 0; n < num2; n++)
			{
				ZoneConnection zoneConnection = new ZoneConnection();
				zoneConnection.Object = Reader.ReadOptimizedString();
				zoneConnection.Type = Reader.ReadOptimizedString();
				zoneConnection.X = Reader.ReadOptimizedInt32();
				zoneConnection.Y = Reader.ReadOptimizedInt32();
				list.Add(zoneConnection);
			}
			zoneManager.ZoneConnections.Add(key, list);
		}
		num = Reader.ReadInt32();
		zoneManager.ZoneProperties.Clear();
		zoneManager.ZoneProperties.EnsureCapacity(num);
		for (int num3 = 0; num3 < num; num3++)
		{
			string key2 = Reader.ReadOptimizedString();
			int num4 = Reader.ReadOptimizedInt32();
			Dictionary<string, object> dictionary = new Dictionary<string, object>(num4);
			for (int num5 = 0; num5 < num4; num5++)
			{
				dictionary[Reader.ReadOptimizedString()] = Reader.ReadObject();
			}
			zoneManager.ZoneProperties[key2] = dictionary;
		}
		zoneManager.ActiveZone = zoneManager.GetZone(Reader.ReadOptimizedString());
		num = Reader.ReadInt32();
		zoneManager.CachedObjects.Clear();
		zoneManager.CachedObjects.EnsureCapacity(num);
		for (int num6 = 0; num6 < num; num6++)
		{
			string key3 = Reader.ReadOptimizedString();
			GameObject gameObject = Reader.ReadGameObject("zone cache");
			zoneManager.CachedObjects[key3] = gameObject;
			if (gameObject.Physics != null)
			{
				gameObject.Physics._CurrentCell = null;
				gameObject.Physics._Equipped = null;
				gameObject.Physics._InInventory = null;
			}
		}
		Hills.Load(Reader);
		BananaGrove.Load(Reader);
		Watervine.Load(Reader);
		Cave.Load(Reader);
		zoneManager.LoadZoneParts(Reader);
		zoneManager.LoadZoneBuilders(Reader);
		return zoneManager;
	}

	public static long CopyTo(Stream source, Stream destination)
	{
		byte[] array = new byte[2048];
		long num = 0L;
		int num2;
		while ((num2 = source.Read(array, 0, array.Length)) > 0)
		{
			destination.Write(array, 0, num2);
			num += num2;
		}
		return num;
	}

	public static void ForceCollect()
	{
		MemoryHelper.GCCollectMax();
	}

	public void ForceTryThawZone(string ZoneID, out Zone Zone)
	{
		if (!FrozenZones.Contains(ZoneID))
		{
			FrozenZones.Add(ZoneID);
		}
		TryThawZone(ZoneID, out Zone);
	}

	public bool TryThawZone(string ZoneID, out Zone Zone)
	{
		bool flag = FrozenZones.Contains(ZoneID);
		if (!flag && !Options.DisableZoneThawRestrict)
		{
			Zone = null;
			return false;
		}
		if (!ProcessingZones.Add(ZoneID))
		{
			MetricsManager.LogError("Attempting to thaw reserved zone.");
			Zone = null;
			return false;
		}
		Loading.Status status = Loading.StartTask("Thawing zone...", "Thawing " + ZoneID);
		try
		{
			SerializationReader serializationReader = SerializationReader.Get();
			long num = -1L;
			try
			{
				using (PooledList<byte> pooledList = new PooledList<byte>(2048))
				{
					byte[] array = ArrayPool<byte>.Shared.Rent(4096);
					DataManager.CacheOperation cacheOperation = DataManager.StartCacheOperation();
					try
					{
						cacheOperation.Command.CommandText = "SELECT Data, FrozenTick FROM FrozenZone WHERE ZoneID = @ZoneID LIMIT 1";
						cacheOperation.Command.Parameters.AddWithValue("ZoneID", ZoneID);
						using SqliteDataReader sqliteDataReader = cacheOperation.Command.ExecuteReader();
						if (!sqliteDataReader.HasRows)
						{
							if (flag)
							{
								MetricsManager.LogWarning("ZoneCacheMiss: No row by ZoneID '" + ZoneID + "' in database.");
							}
							Zone = null;
							return false;
						}
						if (sqliteDataReader.Read())
						{
							if (sqliteDataReader[1].GetType() != typeof(DBNull))
							{
								num = sqliteDataReader.GetInt64(1);
							}
							int num2 = 0;
							int num3 = 0;
							do
							{
								num3 = (int)sqliteDataReader.GetBytes(0, num2, array, 0, array.Length);
								pooledList.AddRange(array.AsSpan(0, num3));
								num2 += num3;
							}
							while ((long)num3 > 0L);
						}
					}
					finally
					{
						ArrayPool<byte>.Shared.Return(array, clearArray: true);
						cacheOperation.Dispose();
					}
					int num4 = BitConverter.ToInt32(pooledList.AsSpan(0, 4));
					serializationReader.Stream.SetLength(num4);
					array = serializationReader.Stream.GetBuffer();
					try
					{
						if (!BrotliDecoder.TryDecompress(pooledList.AsSpan(4), array, out var bytesWritten) || bytesWritten != num4)
						{
							throw new InvalidDataException("Decoded brotli chunk not correct length.");
						}
					}
					catch (ArgumentOutOfRangeException source)
					{
						MetricsManager.LogError($"Invalid brotli arguments:: Length: {num4}, Buffer: {array.Length}, List: {pooledList.Count}");
						ExceptionDispatchInfo.Throw(source);
					}
				}
				serializationReader.Start();
				int num5 = serializationReader.ReadInt32();
				if (num5 != 123457)
				{
					MetricsManager.LogError($"Token mismatch, wanted 123457, got {num5}");
				}
				serializationReader.ReadString();
				Zone = Zone.Load(serializationReader, ZoneID);
				AddCachedZone(Zone);
				serializationReader.FinalizeRead();
				ForceCollect();
				if (serializationReader.Errors > 0)
				{
					Popup.DisplayLoadError(serializationReader, "zone", serializationReader.Errors);
				}
			}
			catch (Exception ex)
			{
				string text = "ZoneManager::TryThawZone::";
				text = ((!(ex is FileNotFoundException)) ? (text + "ReadError::") : (text + "CacheMiss::"));
				if (ModManager.TryGetStackMod(ex, out var Mod, out var Frame))
				{
					MethodBase method = Frame.GetMethod();
					Mod.Error(method.DeclaringType?.FullName + "::" + method.Name + "::" + ex);
				}
				else
				{
					MetricsManager.LogException(text, ex, "serialization_error");
				}
				MessageQueue.AddPlayerMessage("ThawZone exception", 'W');
				Zone = null;
				CachedZones.Remove(ZoneID);
			}
			finally
			{
				SerializationReader.Release(serializationReader);
			}
			if (Zone == null)
			{
				MetricsManager.LogError("Thaw error: " + ZoneID);
				return false;
			}
			PaintWalls(Zone);
			PaintWater(Zone);
			FrozenZones.Remove(ZoneID);
			long frozenTicks = ((num > 0) ? (The.Game.TimeTicks - num) : 0);
			Zone.Thawed(frozenTicks);
			Debug.Log("Thaw complete");
			return true;
		}
		catch (Exception x)
		{
			MessageQueue.AddPlayerMessage("ThawZone exception", 'r');
			MetricsManager.LogException("ZoneManager::TryThawZone", x, "serialization_error");
			Zone = null;
			return false;
		}
		finally
		{
			ProcessingZones.Remove(ZoneID);
			status.Dispose();
		}
	}

	public void FreezeZone(Zone Z, DataManager.CacheTransaction transaction)
	{
		Z.FireEvent("ZoneFreezing");
		Loading.Status status = Loading.StartTask("Freezing zone...", "Freezing " + Z.ZoneID);
		try
		{
			if (FreezingZones.CleanContains(Z.ZoneID))
			{
				return;
			}
			if (The.Player?.GetCurrentZone() == Z)
			{
				MetricsManager.LogError($"[{XRLCore.CurrentTurn}] Attempting to freeze player zone (LastActive: {Z.LastActive}, LastCached: {Z.LastCached}, LastPlayerPresence: {Z.LastPlayerPresence}).");
				return;
			}
			SerializationWriter serializationWriter = SerializationWriter.Get();
			try
			{
				CachedZones.Remove(Z.ZoneID);
				if (FreezingZones.CleanContains(Z.ZoneID))
				{
					return;
				}
				FreezingZones.Add(Z.ZoneID);
				serializationWriter.Start(409);
				serializationWriter.Write(123457);
				serializationWriter.Write(Assembly.GetExecutingAssembly().GetName().Version.ToString());
				Zone.Save(serializationWriter, Z);
				serializationWriter.FinalizeWrite();
				byte[] buffer = serializationWriter.Stream.GetBuffer();
				int num = (int)serializationWriter.Stream.Position;
				byte[] array = ArrayPool<byte>.Shared.Rent(num + 4);
				try
				{
					if (!BrotliEncoder.TryCompress(buffer.AsSpan(0, num), array.AsSpan(4, num), out var bytesWritten, 1, 16))
					{
						throw new InvalidDataException("No encoded data captured.");
					}
					BitConverter.TryWriteBytes(array.AsSpan(0, 4), num);
					SqliteCommand sqliteCommand = transaction.AddCommand();
					sqliteCommand.CommandText = "REPLACE INTO FrozenZone (ZoneID, Data, FrozenTick) VALUES (@ZoneID, @Data, @FrozenTick);";
					sqliteCommand.Parameters.Add(new SqliteParameter
					{
						ParameterName = "ZoneID",
						Value = Z.ZoneID
					});
					sqliteCommand.Parameters.AddWithValue("FrozenTick", The.Game.TimeTicks);
					sqliteCommand.Parameters.Add("Data", DbType.Binary).Value = array.AsSpan(0, bytesWritten + 4).ToArray();
					sqliteCommand.ExecuteNonQuery();
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(array, clearArray: true);
				}
				FrozenZones.Add(Z.ZoneID);
			}
			catch (Exception ex)
			{
				string text = "ZoneManager::FreezeZone::";
				text = ((!(ex is IOException)) ? (text + "WriteError::") : (text + "IO::"));
				if (ModManager.TryGetStackMod(ex, out var Mod, out var Frame))
				{
					MethodBase method = Frame.GetMethod();
					Mod.Error(method.DeclaringType?.FullName + "::" + method.Name + "::" + ex);
				}
				else
				{
					MetricsManager.LogException(text, ex, "serialization_error");
				}
			}
			finally
			{
				SerializationWriter.Release(serializationWriter);
			}
			FreezingZones.Remove(Z.ZoneID);
			Z.Release();
			if (Options.CollectEarly)
			{
				ForceCollect();
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("ZoneManager::FreezeZone", x, "serialization_error");
		}
		finally
		{
			status.Dispose();
			ForceCollect();
			The.Game?.Clean();
		}
	}

	public void CheckCached(bool AllowFreeze = true, bool ForceFreeze = false)
	{
		if (CheckingCache || The.Game.Transient)
		{
			return;
		}
		CheckingCache = true;
		try
		{
			int freezabilityTurns = Zone.GetFreezabilityTurns();
			int suspendabilityTurns = Zone.GetSuspendabilityTurns();
			if (AllowFreeze)
			{
				Graveyard.ReleaseObjects();
			}
			FreezableZones.Clear();
			foreach (var (_, zone2) in CachedZones)
			{
				if (ProcessingZones.Contains(zone2.ZoneID))
				{
					continue;
				}
				if (!zone2.Suspended)
				{
					if (ActiveZone == zone2 || zone2.GetSuspendability(suspendabilityTurns) != Suspendability.Suspendable)
					{
						continue;
					}
					SuspendZone(zone2);
				}
				if (AllowFreeze && zone2.Suspended && zone2.GetFreezability(freezabilityTurns) == Freezability.Freezable)
				{
					FreezableZones.Add(zone2);
				}
			}
			if (!((AllowFreeze && FreezableZones.Count >= GetPendingZoneFreezeThreshold()) | ForceFreeze))
			{
				return;
			}
			using DataManager.CacheTransaction transaction = DataManager.StartCacheTransaction();
			for (int num = FreezableZones.Count - 1; num >= 0; num--)
			{
				FreezeZone(FreezableZones[num], transaction);
			}
			transaction.CommitTransaction();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error checking zone caching", x);
		}
		finally
		{
			CheckingCache = false;
		}
	}

	private int GetPendingZoneFreezeThreshold()
	{
		if (CapabilityManager.HasCapability(CapabilityManager.CapabilityType.LimitedWriteFrequency))
		{
			return 8;
		}
		return 1;
	}

	public void Tick(bool AllowFreeze)
	{
		try
		{
			Ticker++;
			if (ActiveZone != null)
			{
				ActiveZone.MarkActive();
				ActiveZone.CheckWeather();
			}
			if (Options.DisableZoneCaching2)
			{
				if (Ticker % 100 == 0L)
				{
					MessageQueue.AddPlayerMessage("&RWARNING: You have the Disable Zone Caching option enabled, this will cause massive memory use over time.");
				}
			}
			else
			{
				CheckCached(AllowFreeze);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Freeze zone exception", x);
		}
	}

	public bool IsZoneBuilt(string ZoneID)
	{
		if (CachedZones.ContainsKey(ZoneID))
		{
			return true;
		}
		if (FrozenZones.Contains(ZoneID))
		{
			return true;
		}
		return false;
	}

	public void ClearFrozen()
	{
		foreach (string frozenZone in FrozenZones)
		{
			if (ZoneConnections.TryGetValue(frozenZone, out var value))
			{
				value.Clear();
			}
		}
		FrozenZones.Clear();
		Directory.Delete(The.Game.GetCacheDirectory("ZoneCache"), recursive: true);
	}

	public ZoneBuilderCollection GetBuilderCollection(string ZoneID)
	{
		ZoneBuilders.TryGetValue(ZoneID, out var value);
		return value;
	}

	public ZoneBuilderCollection RequireBuilderCollection(string ZoneID)
	{
		if (!ZoneBuilders.TryGetValue(ZoneID, out var value))
		{
			value = (ZoneBuilders[ZoneID] = new ZoneBuilderCollection(ZoneID));
		}
		return value;
	}

	public void AddZoneBuilder(string ZoneID, int Priority, string Class)
	{
		AddZoneBuilderInternal(ZoneID, Priority, ZoneBuilderBlueprint.Get(Class));
	}

	public void AddZoneBuilder(string ZoneID, int Priority, string Class, string Key1, object Value1)
	{
		AddZoneBuilderInternal(ZoneID, Priority, ZoneBuilderBlueprint.Get(Class, Key1, Value1));
	}

	public void AddZoneBuilder(string ZoneID, int Priority, string Class, string Key1, object Value1, string Key2, object Value2)
	{
		AddZoneBuilderInternal(ZoneID, Priority, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2));
	}

	public void AddZoneBuilder(string ZoneID, int Priority, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3)
	{
		AddZoneBuilderInternal(ZoneID, Priority, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3));
	}

	public void AddZoneBuilder(string ZoneID, int Priority, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4)
	{
		AddZoneBuilderInternal(ZoneID, Priority, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4));
	}

	public void AddZoneBuilder(string ZoneID, int Priority, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5)
	{
		AddZoneBuilderInternal(ZoneID, Priority, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4, Key5, Value5));
	}

	public void AddZoneBuilder(string ZoneID, int Priority, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5, string Key6, object Value6)
	{
		AddZoneBuilderInternal(ZoneID, Priority, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4, Key5, Value5, Key6, Value6));
	}

	private void AddZoneBuilderInternal(string ZoneID, int Priority, ZoneBuilderBlueprint Builder)
	{
		RequireBuilderCollection(ZoneID).Add(Builder, Priority);
	}

	public void AddZoneBuilderOverride(string ZoneID, string Builder)
	{
		AddZoneBuilderOverrideInternal(ZoneID, ZoneBuilderBlueprint.Get(Builder));
	}

	private void AddZoneBuilderOverrideInternal(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		RequireBuilderCollection(ZoneID).Add(Builder, -1000);
	}

	public void AddZonePreBuilder(string ZoneID, string Builder)
	{
		AddZonePreBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Builder));
	}

	public void AddZonePreBuilder(string ZoneID, string Class, string Key1, object Value1)
	{
		AddZonePreBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1));
	}

	public void AddZonePreBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2)
	{
		AddZonePreBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2));
	}

	public void AddZonePreBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3)
	{
		AddZonePreBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3));
	}

	public void AddZonePreBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4)
	{
		AddZonePreBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4));
	}

	public void AddZonePreBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5)
	{
		AddZonePreBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4, Key5, Value5));
	}

	public void AddZonePreBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5, string Key6, object Value6)
	{
		AddZonePreBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4, Key5, Value5, Key6, Value6));
	}

	private void AddZonePreBuilderInternal(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		RequireBuilderCollection(ZoneID).Add(Builder, 3000);
	}

	public void AddZoneMidBuilder(string ZoneID, string Builder)
	{
		AddZoneMidBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Builder));
	}

	public void AddZoneMidBuilder(string ZoneID, string Class, string Key1, object Value1)
	{
		AddZoneMidBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1));
	}

	public void AddZoneMidBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2)
	{
		AddZoneMidBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2));
	}

	public void AddZoneMidBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3)
	{
		AddZoneMidBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3));
	}

	public void AddZoneMidBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4)
	{
		AddZoneMidBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4));
	}

	public void AddZoneMidBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5)
	{
		AddZoneMidBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4, Key5, Value5));
	}

	public void AddZoneMidBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5, string Key6, object Value6)
	{
		AddZoneMidBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4, Key5, Value5, Key6, Value6));
	}

	private void AddZoneMidBuilderInternal(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		RequireBuilderCollection(ZoneID).Add(Builder, 4500);
	}

	public void AddZonePostBuilderIfNotAlreadyPresent(string ZoneID, string Builder)
	{
		RequireBuilderCollection(ZoneID).AddUnique(ZoneBuilderBlueprint.Get(Builder), 5000);
	}

	public void AddZonePostBuilder(string ZoneID, string Builder)
	{
		AddZonePostBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Builder));
	}

	public void AddZonePostBuilder(string ZoneID, string Class, string Key1, object Value1)
	{
		AddZonePostBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1));
	}

	public void AddZonePostBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2)
	{
		AddZonePostBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2));
	}

	public void AddZonePostBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3)
	{
		AddZonePostBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3));
	}

	public void AddZonePostBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4)
	{
		AddZonePostBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4));
	}

	public void AddZonePostBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5)
	{
		AddZonePostBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4, Key5, Value5));
	}

	public void AddZonePostBuilder(string ZoneID, string Class, string Key1, object Value1, string Key2, object Value2, string Key3, object Value3, string Key4, object Value4, string Key5, object Value5, string Key6, object Value6)
	{
		AddZonePostBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Class, Key1, Value1, Key2, Value2, Key3, Value3, Key4, Value4, Key5, Value5, Key6, Value6));
	}

	private void AddZonePostBuilderInternal(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		RequireBuilderCollection(ZoneID).Add(Builder, 5000);
	}

	public void AddZonePostBuilderAtStart(string ZoneID, string Builder)
	{
		RequireBuilderCollection(ZoneID).Add(ZoneBuilderBlueprint.Get(Builder), 4990);
	}

	public void AddZonePostBuilderAfterTerrain(string ZoneID, string Builder)
	{
		RequireBuilderCollection(ZoneID).Add(ZoneBuilderBlueprint.Get(Builder), 6010);
	}

	public void AddZonePostBuilderAtStart(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		RequireBuilderCollection(ZoneID).Add(ZoneBuilderBlueprint.Get(Builder), 4990);
	}

	[Obsolete]
	public void AddZoneBuilderOverride(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		AddZoneBuilderOverrideInternal(ZoneID, ZoneBuilderBlueprint.Get(Builder));
	}

	[Obsolete]
	public void AddZonePreBuilder(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		AddZonePreBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Builder));
	}

	[Obsolete]
	public void AddZoneMidBuilderAtStart(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		RequireBuilderCollection(ZoneID).Add(ZoneBuilderBlueprint.Get(Builder), 4490);
	}

	[Obsolete]
	public void AddZoneMidBuilder(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		AddZoneMidBuilderInternal(ZoneID, ZoneBuilderBlueprint.Get(Builder));
	}

	[Obsolete]
	public void AddZoneMidBuilderAtStart(string ZoneID, string Builder)
	{
		RequireBuilderCollection(ZoneID).Add(ZoneBuilderBlueprint.Get(Builder), 4490);
	}

	[Obsolete]
	public void AddZonePostBuilderAfterTerrain(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		RequireBuilderCollection(ZoneID).Add(ZoneBuilderBlueprint.Get(Builder), 6010);
	}

	[Obsolete]
	public void AddZonePostBuilder(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		RequireBuilderCollection(ZoneID).Add(ZoneBuilderBlueprint.Get(Builder), 5000);
	}

	public void ClearZoneBuilders(string ZoneID)
	{
		GetBuilderCollection(ZoneID)?.Clear();
	}

	public int RemoveZoneBuilders(string ZoneID, Predicate<ZoneBuilderBlueprint> Predicate)
	{
		if (ZoneBuilders.TryGetValue(ZoneID, out var value))
		{
			return value.RemoveAll(Predicate);
		}
		return 0;
	}

	public int RemoveZoneBuilders(string ZoneID, string Class, Predicate<ZoneBuilderBlueprint> Predicate = null)
	{
		if (ZoneBuilders.TryGetValue(ZoneID, out var value))
		{
			return value.RemoveAll(Class, Predicate);
		}
		return 0;
	}

	public void SetZoneColumnProperty(string ZoneID, string Name, object Value)
	{
		ZoneID = ZoneID.Substring(0, ZoneID.LastIndexOf('.'));
		SetZoneProperty(ZoneID, Name, Value);
	}

	public void SetWorldCellProperty(string ZoneID, string Name, object Value)
	{
		ZoneID = ZoneID.Substring(0, ZoneID.UpToNthIndex('.', 3));
		SetZoneProperty(ZoneID, Name, Value);
	}

	public void SetZoneProperty(string ZoneID, string Name, object Value)
	{
		if (!ZoneProperties.TryGetValue(ZoneID, out var value))
		{
			value = new Dictionary<string, object>();
			ZoneProperties.Add(ZoneID, value);
		}
		value[Name] = Value;
	}

	public bool HasZoneProperty(string ZoneID, string Name)
	{
		if (ZoneProperties.TryGetValue(ZoneID, out var value))
		{
			return value.ContainsKey(Name);
		}
		return false;
	}

	public bool RemoveZoneProperty(string ZoneID, string Name)
	{
		if (ZoneProperties.TryGetValue(ZoneID, out var value))
		{
			return value.Remove(Name);
		}
		return false;
	}

	public bool HasZoneColumnProperty(string ZoneID, string Name)
	{
		if (ZoneID != null)
		{
			ZoneID = ZoneID.Substring(0, ZoneID.LastIndexOf('.'));
			if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.ContainsKey(Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetZoneColumnProperty<T>(string ZoneID, string Name, out T Value)
	{
		ZoneID = ZoneID.Substring(0, ZoneID.LastIndexOf('.'));
		if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.TryGetValue(Name, out var value2))
		{
			Value = (T)value2;
			return true;
		}
		Value = default;
		return false;
	}

	public object GetZoneColumnProperty(string ZoneID, string Name, object Default = null)
	{
		if (ZoneID != null)
		{
			ZoneID = ZoneID.Substring(0, ZoneID.LastIndexOf('.'));
			if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.TryGetValue(Name, out var value2))
			{
				return value2;
			}
		}
		return Default;
	}

	public bool TryGetWorldCellProperty<T>(string ZoneID, string Name, out T Value)
	{
		ZoneID = ZoneID.Substring(0, ZoneID.UpToNthIndex('.', 3));
		if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.TryGetValue(Name, out var value2))
		{
			Value = (T)value2;
			return true;
		}
		Value = default;
		return false;
	}

	public object GetWorldCellProperty(string ZoneID, string Name, object Default = null)
	{
		if (ZoneID != null)
		{
			ZoneID = ZoneID.Substring(0, ZoneID.UpToNthIndex('.', 3));
			if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.TryGetValue(Name, out var value2))
			{
				return value2;
			}
		}
		return Default;
	}

	public bool TryGetZoneProperty<T>(string ZoneID, string Name, out T Value)
	{
		if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.TryGetValue(Name, out var value2))
		{
			Value = (T)value2;
			return true;
		}
		Value = default;
		return false;
	}

	public object GetZoneProperty(string zID, string Name, bool bClampToLevel30 = false, string defaultvalue = null)
	{
		if (zID != null)
		{
			if (ZoneProperties.TryGetValue(zID, out var value) && value.TryGetValue(Name, out var value2))
			{
				return value2;
			}
			if (bClampToLevel30 && zID.Contains(".") && ZoneID.Parse(zID, out var World, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY, out var _))
			{
				zID = ZoneID.Assemble(World, ParasangX, ParasangY, ZoneX, ZoneY, 29);
				return GetZoneProperty(zID, Name, bClampToLevel30: false, defaultvalue);
			}
		}
		return defaultvalue;
	}

	public bool CheckBiomesAllowed(string ZoneID)
	{
		if (!XRL.World.ZoneID.Parse(ZoneID, out var World, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY, out var ZoneZ))
		{
			return false;
		}
		return CheckBiomesAllowed(ZoneID, World, ParasangX, ParasangY, ZoneX, ZoneY, ZoneZ);
	}

	public bool CheckBiomesAllowed(Zone Z)
	{
		return CheckBiomesAllowed(Z.ZoneID, Z.ZoneWorld, Z.wX, Z.wY, Z.X, Z.Y, Z.Z);
	}

	public bool CheckBiomesAllowed(string ZoneID, string WorldID, int WX, int WY, int X, int Y, int Z)
	{
		ZoneBlueprint zoneBlueprint = GetZoneBlueprint(WorldID, WX, WY, X, Y, Z);
		if (zoneBlueprint != null && !zoneBlueprint.HasBiomes)
		{
			return false;
		}
		GameObject terrainObjectForZone = GetTerrainObjectForZone(WX, WY, WorldID);
		if (terrainObjectForZone != null && terrainObjectForZone.HasTagOrProperty("NoBiomes"))
		{
			return false;
		}
		if (TryGetZoneProperty<string>(ZoneID, "NoBiomes", out var Value) && Value == "Yes")
		{
			return false;
		}
		return true;
	}

	public List<ZoneConnection> GetZoneConnections(string ZoneID)
	{
		if (!ZoneConnections.TryGetValue(ZoneID, out var value))
		{
			return new List<ZoneConnection>();
		}
		return value;
	}

	public List<ZoneConnection> GetZoneConnectionsCopy(string ZoneID)
	{
		if (!ZoneConnections.TryGetValue(ZoneID, out var value))
		{
			return new List<ZoneConnection>();
		}
		return new List<ZoneConnection>(value);
	}

	public void AddZoneConnection(string ZoneID, string TargetDirection, int X, int Y, string Type, string ConnectionObject = null)
	{
		string zoneFromIDAndDirection = GetZoneFromIDAndDirection(ZoneID, TargetDirection);
		if (!ZoneConnections.ContainsKey(zoneFromIDAndDirection))
		{
			ZoneConnections.Add(zoneFromIDAndDirection, new List<ZoneConnection>());
		}
		ZoneConnection zoneConnection = new ZoneConnection();
		zoneConnection.X = X;
		zoneConnection.Y = Y;
		zoneConnection.Type = Type;
		zoneConnection.Object = ConnectionObject;
		ZoneConnections[zoneFromIDAndDirection].Add(zoneConnection);
	}

	public void RemoveZoneConnection(string ZoneID, string TargetDirection, int X, int Y, string Type, string ConnectionObject = null)
	{
		string zoneFromIDAndDirection = GetZoneFromIDAndDirection(ZoneID, TargetDirection);
		if (!ZoneConnections.TryGetValue(zoneFromIDAndDirection, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			ZoneConnection zoneConnection = value[num];
			if (zoneConnection.X == X && zoneConnection.Y == Y && !(zoneConnection.Type != Type) && !(zoneConnection.Object != ConnectionObject))
			{
				value.RemoveAt(num);
			}
		}
	}

	public void RemoveZoneConnection(string ZoneID, string TargetDirection, ZoneConnection Connection)
	{
		string zoneFromIDAndDirection = GetZoneFromIDAndDirection(ZoneID, TargetDirection);
		if (ZoneConnections.TryGetValue(zoneFromIDAndDirection, out var value))
		{
			value.Remove(Connection);
		}
	}

	public string GetZoneFromIDAndDirection(string ZoneID, string Direction)
	{
		if (ZoneID.IsNullOrEmpty() || !ZoneID.Contains("."))
		{
			return "";
		}
		XRL.World.ZoneID.Parse(ZoneID, out var World, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY, out var ZoneZ);
		Direction = Direction.ToLower();
		if (Direction == "u")
		{
			ZoneZ--;
		}
		if (Direction == "d")
		{
			ZoneZ++;
		}
		if (Direction == "n")
		{
			ZoneY--;
		}
		if (Direction == "s")
		{
			ZoneY++;
		}
		if (Direction == "e")
		{
			ZoneX++;
		}
		if (Direction == "w")
		{
			ZoneX--;
		}
		if (Direction == "nw")
		{
			ZoneX--;
			ZoneY--;
		}
		if (Direction == "ne")
		{
			ZoneX++;
			ZoneY--;
		}
		if (Direction == "sw")
		{
			ZoneX--;
			ZoneY++;
		}
		if (Direction == "se")
		{
			ZoneX++;
			ZoneY++;
		}
		if (ZoneX < 0)
		{
			ZoneX = Definitions.Width - 1;
			ParasangX--;
		}
		if (ZoneX >= Definitions.Width)
		{
			ZoneX = 0;
			ParasangX++;
		}
		if (ZoneY < 0)
		{
			ZoneY = Definitions.Height - 1;
			ParasangY--;
		}
		if (ZoneY >= Definitions.Width)
		{
			ZoneY = 0;
			ParasangY++;
		}
		if (ZoneZ < 0)
		{
			ZoneZ = Definitions.Layers - 1;
		}
		if (ZoneZ >= Definitions.Layers)
		{
			ZoneZ = 0;
		}
		return XRL.World.ZoneID.Assemble(World, ParasangX, ParasangY, ZoneX, ZoneY, ZoneZ);
	}

	public bool HasVisitedZone(string zoneid)
	{
		return VisitedTime.ContainsKey(zoneid);
	}

	public void DeleteZone(Zone Z)
	{
		CachedZones.Remove(Z.ZoneID);
		Z.Stale = true;
	}

	public bool SuspendZone(Zone Zone)
	{
		if (Zone == null || Zone.Suspended)
		{
			return true;
		}
		try
		{
			SuspendingEvent.Send(Zone);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error sending suspension event", x);
		}
		if (ProcessingZones.Contains(Zone.ZoneID))
		{
			MetricsManager.LogInfo("Zone by ID '" + Zone.ZoneID + "' was reserved by suspension event, aborting suspend.");
			return false;
		}
		ActionManager actionManager = The.ActionManager;
		RingDeque<GameObject> actionQueue = actionManager.ActionQueue;
		for (int num = actionQueue.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = actionQueue[num];
			if (gameObject != null && gameObject.CurrentZone == Zone)
			{
				actionManager.RemoveActiveObject(num);
			}
		}
		Zone.Suspended = true;
		return true;
	}

	public void SuspendAll()
	{
		foreach (var (_, zone2) in CachedZones)
		{
			if (zone2 != ActiveZone)
			{
				SuspendZone(zone2);
			}
		}
	}

	public static void ActivateObjects(Zone Zone)
	{
		Zone.ObjectEnumerator objectEnumerator;
		if (Zone.IsWorldMap())
		{
			objectEnumerator = Zone.IterateObjects();
			Zone.ObjectEnumerator enumerator = objectEnumerator.GetEnumerator();
			while (enumerator.MoveNext())
			{
				GameObject current = enumerator.Current;
				if (!current.HasPart<TerrainTravel>())
				{
					current.MakeLive();
					current.MakeActive();
				}
			}
		}
		else
		{
			objectEnumerator = Zone.IterateObjects();
			Zone.ObjectEnumerator enumerator = objectEnumerator.GetEnumerator();
			while (enumerator.MoveNext())
			{
				GameObject current2 = enumerator.Current;
				current2.MakeLive();
				current2.MakeActive();
			}
		}
	}

	public Zone SetActiveZone(string ZoneID)
	{
		if (ActiveZone != null && ActiveZone.ZoneID == ZoneID)
		{
			return ActiveZone;
		}
		Zone zone = GetZone(ZoneID);
		if (zone == null)
		{
			MetricsManager.LogError("Attempting to set null active zone by zone id '" + ZoneID + "'.");
			return ActiveZone;
		}
		return SetActiveZone(zone);
	}

	public void SetCachedZone(Zone Zone)
	{
		Zone.MarkActive();
		ActivateObjects(Zone);
		Zone.Suspended = false;
	}

	public Zone SetActiveZone(Zone Z)
	{
		Zone activeZone = ActiveZone;
		if (Z == activeZone)
		{
			return activeZone;
		}
		try
		{
			XRLCore.ParticleManager.Particles.Clear();
			GameManager.Instance.uiQueue.queueTask(CombatJuiceManager.finishAll);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("SetActiveZone::FinishJuice", x);
		}
		ActiveZone = Z;
		ZoneGenerationContext = Z;
		LastZoneTransition = XRLCore.Core.Game.Turns;
		string zoneID = ActiveZone.ZoneID;
		if (ActiveZone != null)
		{
			zoneGenerationContextTier = ActiveZone.NewTier;
			zoneGenerationContextZoneID = ActiveZone.ZoneID;
		}
		if (!XRLCore.Core.Game.bZoned && LastZoneTransition > 0)
		{
			The.Game.bZoned = true;
		}
		if (ActiveZone.Z >= 60)
		{
			Achievement.ARCONAUT.Unlock();
		}
		if (!VisitedTime.ContainsKey(zoneID))
		{
			if (The.Player != null && zoneID.Contains("."))
			{
				The.Player.FireEvent(Event.New("VisitingNewZone", "ID", zoneID));
			}
			string text = WorldFactory.Factory.ZoneDisplayName(ActiveZone.ZoneID);
			int num = Stat.Roll(2, 6);
			if (text.Contains("Kyakukya"))
			{
				if (XRLCore.CurrentTurn - The.Game.GetInt64GameState("LastKyakukyaVisit", 0L) > 1000)
				{
					JournalAPI.AddAccomplishment("You journeyed to Kyakukya.", "Done trekking through the root-strangled earth, =name= arrived in Kyakukya and was greeted by the village with warmth and reverence. Upon leaving, =name= was named Friend to Oboroqoru.", $"On an expedition down the River Svy, =name= was captured by bandits. {The.Player.GetPronounProvider().Subjective} languished in captivity for {num} years, eventually escaping to Kyakukya and befriending its mayor, the albino ape called Nuntu.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				}
				The.Game.SetInt64GameState("LastKyakukyaVisit", XRLCore.CurrentTurn);
			}
			if (text.Contains("Omonporch"))
			{
				if (XRLCore.CurrentTurn - The.Game.GetInt64GameState("LastOmonporchVisit", 0L) > 1000)
				{
					JournalAPI.AddAccomplishment("You journeyed to Omonporch.", "=name= journeyed to the Spindle at Omonporch, voiced a prayer for the Fossilized Saads, and observed a star fall along the planet's axis.", "In =year=, =name= appointed the corrupt administrator Asphodel as earl and minister of Omonporch. There xe mandated the practice of <spice.elements." + The.Player.GetMythicDomain() + ".practices.!random> in " + The.Player.GetPronounProvider().PossessiveAdjective + " name.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				}
				The.Game.SetInt64GameState("LastOmonporchVisit", XRLCore.CurrentTurn);
			}
			if ((text.Contains("Stiltgrounds") || text.Contains("Six Day Stilt")) && The.Game.GetIntGameState("VisitedSixDayStilt") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Six Day Stilt.", "=name= trekked through the salt pans, north and west, to the merchant bazaar and grand cathedral of the Six Day Stilt. There, the stiltfolk sang hymns in the sultan's honor.", "After trekking the vacant flats, =name= journeyed to the merchant bazaar and cathedral at the Six Day Stilt. There " + The.Player.GetPronounProvider().Subjective + " befriended the Mechanimists and threw an artifact down the Sacred Well.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedSixDayStilt", 1);
				Achievement.SIX_DAY_STILT.Unlock();
			}
			if (text.Contains("Red Rock") && The.Game.GetIntGameState("VisitedRedrock") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to Red Rock.", "=name= hiked the salted marshes and came upon the ancient gathering place called Red Rock, where a bevy of snapjaws cooked the sultan a satisfying meal.", "Near the location of Red Rock, =name= was captured by baboons. " + The.Player.GetPronounProvider().Subjective + " murdered their leader <spice.elements." + The.Player.GetMythicDomain() + ".murdermethods.!random> and from then on wore a neck ring stained with baboon blood.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedRedrock", 1);
			}
			if (text.Contains("rusted archway") && The.Game.GetIntGameState("VisitedRustedArchway") != 1)
			{
				string text2 = HistoricStringExpander.ExpandString("<spice.elements." + The.Player.GetMythicDomain() + ".nouns.!random>");
				JournalAPI.AddAccomplishment("You journeyed to a rusted archway.", "To commemorate the imperial conquests of =name=, a triumphal arch was erected across the fruiting gorges.", "At <spice.time.partsOfDay.!random> under <spice.commonPhrases.strange.!random.article> and rusted sky, the people of the desert canyon saw an image on the horizon that looked like a " + text2 + " under an archway. It was =name=, and after " + The.Player.GetPronounProvider().Subjective + " came and left, the people built a monument to =name= and thenceforth called " + The.Player.GetPronounProvider().Objective + " the Underarch " + Grammar.MakeTitleCase(text2) + ".", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedRustedArchway", 1);
			}
			if (text.Contains("Rustwell") && The.Game.GetIntGameState("VisitedRustwells") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the rust wells.", "Hillfolk dug wells deep into the earth and drew spring water to honor the sultan =name= in sacred ritual.", "While journeying across the wavegraph hills, =name= discovered the rust wells. There " + The.Player.GetPronounProvider().Subjective + " befriended water barons and sipped from the red iron spring.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedRustwells", 1);
			}
			if (text.Contains("Grit Gate") && The.Game.GetIntGameState("VisitedGritGate") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to Grit Gate.", "The scholar-sultan =name= visited Grit Gate and lectured the monastic order there on the subject of light refraction.", "While delving deep through moss-skirted steeples of chrome, =name= discovered the ancient Grit Gate. There " + The.Player.GetPronounProvider().Subjective + " befriended the Barathrumites and lectured them on light refraction.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedGritGate", 1);
			}
			if (text.Contains("Golgotha") && The.Game.GetIntGameState("VisitedGolgotha") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to Golgotha.", "In the month of " + Calendar.GetMonth() + " of " + Calendar.GetYear() + " AR, =name= ascended the trash chutes of Golgotha, victorious and bathed in slime.", "One auspicious day in the jungle, =name= descended the trash chutes of Golgotha and bathed in viscous slime. From that day forth, " + The.Player.GetPronounProvider().Subjective + " always kept some wet trash on " + The.Player.GetPronounProvider().PossessiveAdjective + " person.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedGolgotha", 1);
			}
			if (text.Contains("Bethesda Susa") && The.Game.GetIntGameState("VisitedBethesda") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to Bethesda Susa.", "=name= trekked to the frozen sauna at Bethesda Susa and soaked in the company of trolls.", "In =year=, =name= appointed a corrupt administrator to the frozen sauna at Bethesda Susa. There " + Grammar.RandomShePronoun() + " mandated association with trolls in " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " name.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedBethesda", 1);
			}
			if (text.Contains("Tomb of the Eaters") && The.Game.GetIntGameState("VisitedTomboftheEaters") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Tomb of the Eaters.", "In the month of " + Calendar.GetMonth() + " of " + Calendar.GetYear() + " AR, =name= trekked to the Tomb of the Eaters and traced a sigil across the Death Gate. O wise sultan!", "While traveling through the banana grove, =name= trekked to the Tomb of the Eaters and dreamed they emerged at the Court of Sultans and crossed the Death Gate. From that day forth, " + The.Player.GetPronounProvider().Subjective + " always kept the Mark of Death on " + The.Player.GetPronounProvider().PossessiveAdjective + " person.", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedTomboftheEaters", 1);
			}
			if (text.Contains("Asphalt Mines") && The.Game.GetIntGameState("VisitedAsphaltMines") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the asphalt mines.", "Strong =name= trekked to the asphalt mines to breathe the tarry vapor and bathe in the black blood of the earth.", "Sometime in =year=, =name= wandered over the high mounts and voyaged to the Asphalt Mines. There " + The.Player.GetPronounProvider().Subjective + " befriended no one and instead bathed in the black blood of the earth.", null, "general", MuralCategory.VisitsLocation, MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedAsphaltMines", 1);
			}
			if (text.Contains("City of Bones") && The.Game.GetIntGameState("VisitedCityofBones") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the City of Bones.", "Strong =name= trekked to the City of Bones to breathe the tarry vapor and bathe in the black blood of the earth.", "Sometime in =year=, =name= wandered over the high mounts and voyaged to the City of Bones in the Asphalt Mines. There " + The.Player.GetPronounProvider().Subjective + " befriended no one and instead bathed in the black blood of the earth.", null, "general", MuralCategory.VisitsLocation, MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedCityofBones", 1);
			}
			if (text.Contains("Great Sea") && The.Game.GetIntGameState("VisitedGreatSea") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Great Asphalt Sea.", "Strong =name= trekked to the Great Asphalt Sea to breathe the tarry vapor and bathe in the black blood of the earth.", "Sometime in =year=, =name= wandered over the high mounts and voyaged to the Great Sea in the Asphalt Mines. There " + The.Player.GetPronounProvider().Subjective + " befriended no one and instead bathed in the black blood of the earth.", null, "general", MuralCategory.VisitsLocation, MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedGreatSea", 1);
			}
			if (text.Contains("Tunnels of Ur") && The.Game.GetIntGameState("VisitedTunnelsUr") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Tunnels of Ur.", "Strong =name= trekked to the Tunnels of Ur to breathe the tarry vapor and bathe in the black blood of the earth.", "Sometime in =year=, =name= wandered over the high mounts and voyaged to the Tunnels of Ur in the Asphalt Mines. There " + The.Player.GetPronounProvider().Subjective + " befriended no one and instead bathed in the black blood of the earth.", null, "general", MuralCategory.VisitsLocation, MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedTunnelsUr", 1);
			}
			if (text.Contains("Swilling Vast") && The.Game.GetIntGameState("VisitedSwillingVast") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Swilling Vast.", "Strong =name= trekked to the Swilling Vast to breathe the tarry vapor and bathe in the black blood of the earth.", "Sometime in =year=, =name= wandered over the high mounts and voyaged to the Swilling Vast in the Asphalt Mines. There " + The.Player.GetPronounProvider().Subjective + " befriended no one and instead bathed in the black blood of the earth.", null, "general", MuralCategory.VisitsLocation, MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedSwillingVast", 1);
			}
		}
		VisitedTime[zoneID] = Calendar.TotalTimeTicks;
		activeZone?.Deactivated();
		ActiveZone.Activated();
		ActivateObjects(ActiveZone);
		ActiveZone.Suspended = false;
		The.Game.ZoneManager.CheckCached();
		if (ActiveZone.GetZoneWorld() == "JoppaWorld")
		{
			MessageQueue.AddPlayerMessage(WorldFactory.Factory.ZoneDisplayName(zoneID) + ", " + Calendar.GetTime(), 'C');
		}
		else
		{
			MessageQueue.AddPlayerMessage(WorldFactory.Factory.ZoneDisplayName(zoneID), 'C');
		}
		try
		{
			List<JournalMapNote> mapNotesForZone = JournalAPI.GetMapNotesForZone(ActiveZone.ZoneID);
			if (mapNotesForZone.Count > 0)
			{
				string text3 = "Notes: ";
				int num2 = 0;
				for (int i = 0; i < mapNotesForZone.Count; i++)
				{
					if (mapNotesForZone[i].Revealed)
					{
						if (num2 > 0)
						{
							text3 += ", ";
						}
						text3 += mapNotesForZone[i].Text;
						num2++;
					}
				}
				if (num2 > 0)
				{
					MessageQueue.AddPlayerMessage(text3);
				}
			}
			AutoAct.Interrupt();
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception during zone note retrieval: " + ex.ToString());
		}
		try
		{
			if ((activeZone != null) & (Z != null))
			{
				ZoneTransitionCount += Math.Min(activeZone.GetTransitionIntervalTo(Z), Z.GetTransitionIntervalTo(activeZone));
			}
			int autosaveInterval = Options.AutosaveInterval;
			if (autosaveInterval > 0 && ZoneTransitionCount >= autosaveInterval && The.Player?.CurrentCell != null)
			{
				ZoneTransitionCount = 0;
				AutoSaveCommand.Issue();
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("Exception during autosave: " + ex2.ToString());
		}
		if (ActiveZone != null)
		{
			PaintWalls(ActiveZone);
			PaintWater(ActiveZone);
		}
		ProcessGoToPartyLeader();
		return ActiveZone;
	}

	public void CheckEventQueue()
	{
		if (ActiveZone != null)
		{
			ActiveZone.CheckEventQueue();
		}
	}

	public bool IsZoneLive(string ZoneID)
	{
		if (ZoneID == null)
		{
			return false;
		}
		return CachedZones.ContainsKey(ZoneID);
	}

	public bool IsZoneLive(string World, int wX, int wY, int X, int Y, int Z)
	{
		return CachedZones.ContainsKey(ZoneID.Assemble(World, wX, wY, X, Y, Z));
	}

	public void RebuildActiveZone(bool Flush = false)
	{
		if (The.Player.CurrentZone != ActiveZone)
		{
			return;
		}
		Zone activeZone = ActiveZone;
		List<GameObject> objects = activeZone.GetObjects((GameObject o) => o.IsPlayerLed());
		Dictionary<GameObject, Location2D> dictionary = new Dictionary<GameObject, Location2D> { [The.Player] = The.Player.CurrentCell.Location };
		The.Player.CurrentCell.RemoveObject(The.Player);
		foreach (GameObject item in objects)
		{
			try
			{
				dictionary[item] = item.CurrentCell.Location;
				item.CurrentCell.RemoveObject(item);
			}
			catch
			{
			}
		}
		if (activeZone.IsWorldMap())
		{
			SetActiveZone(The.ZoneManager.GetZone("JoppaWorld.0.0.0.0.0"));
		}
		else
		{
			SetActiveZone(The.ZoneManager.ActiveZone.GetZoneWorld());
		}
		SuspendZone(activeZone);
		DeleteZone(activeZone);
		if (Flush)
		{
			The.ZoneManager.CachedZones.Clear();
			The.ZoneManager.ActiveZone = null;
		}
		activeZone = The.ZoneManager.GetZone(activeZone.ZoneID);
		The.ZoneManager.SetActiveZone(activeZone);
		try
		{
			activeZone.GetCell(dictionary[The.Player]).AddObject(The.Player, Forced: true, System: true);
			The.ZoneManager.ProcessGoToPartyLeader();
		}
		catch
		{
		}
		foreach (GameObject item2 in objects)
		{
			try
			{
				activeZone.GetCell(dictionary[item2]).AddObject(item2);
			}
			catch
			{
			}
		}
	}

	public Zone GetZone(string World, int wX, int wY, int X, int Y, int Z)
	{
		if (World.IsNullOrEmpty())
		{
			return null;
		}
		return GetZone(ZoneID.Assemble(World, wX, wY, X, Y, Z));
	}

	public Zone GetZone(string ZoneID)
	{
		if (ZoneID.IsNullOrEmpty())
		{
			return null;
		}
		Zone value = null;
		try
		{
			Zone value2;
			Zone Zone2;
			if (GetZoneEvent.TryGetFor(Game, ZoneID, out var Zone))
			{
				value = Zone;
			}
			else if (CachedZones.TryGetValue(ZoneID, out value2))
			{
				value = value2;
			}
			else if (TryThawZone(ZoneID, out Zone2))
			{
				value = Zone2;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error getting existing zone '" + ZoneID + "'", x);
		}
		if (value == null)
		{
			GenerateZone(ZoneID);
			if (!CachedZones.TryGetValue(ZoneID, out value))
			{
				MetricsManager.LogError("Error generating zone '" + ZoneID + "'");
			}
		}
		return value;
	}

	public bool MoveZone(string FromID, string ToID, bool Overwrite = true)
	{
		if (CachedZones.TryGetValue(FromID, out var value))
		{
			if (Overwrite)
			{
				CachedZones[ToID] = value;
			}
			else if (CachedZones.ContainsKey(ToID))
			{
				return false;
			}
			CachedZones.Remove(FromID);
			value.ZoneID = ToID;
			return true;
		}
		if (FrozenZones.Contains(FromID))
		{
			string text = The.Game.GetCacheDirectory(Path.Combine("ZoneCache", FromID + ".zone.gz"));
			if (!File.Exists(text))
			{
				text = text.Remove(text.Length - 2, 2);
			}
			if (!File.Exists(text))
			{
				return false;
			}
			string text2 = text.Replace(FromID, ToID);
			if (Overwrite)
			{
				File.Delete(text2);
			}
			else if (FrozenZones.Contains(ToID))
			{
				return false;
			}
			FrozenZones.Remove(FromID);
			FrozenZones.Add(ToID);
			File.Move(text, text2);
			return true;
		}
		return false;
	}

	public List<CellBlueprint> GetCellBlueprints(string ZoneID)
	{
		try
		{
			List<CellBlueprint> result = new List<CellBlueprint>();
			if (ZoneID.IndexOf('.') == -1)
			{
				return result;
			}
			ZoneID.AsDelimitedSpans('.', out var First, out var Second, out var Third);
			string world = new string(First);
			int parasangX = int.Parse(Second);
			int parasangY = int.Parse(Third);
			return GetCellBlueprints(world, parasangX, parasangY);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Exception on getcellblueprints for: " + ZoneID, x);
			return new List<CellBlueprint>();
		}
	}

	public List<CellBlueprint> GetCellBlueprints(string World, int ParasangX, int ParasangY)
	{
		try
		{
			List<CellBlueprint> list = new List<CellBlueprint>();
			GameObject terrainObjectForZone = GetTerrainObjectForZone(ParasangX, ParasangY, World);
			if (terrainObjectForZone == null)
			{
				return list;
			}
			WorldBlueprint world = WorldFactory.Factory.getWorld(World);
			if (world == null)
			{
				return list;
			}
			if (world.CellBlueprintsByApplication.TryGetValue(terrainObjectForZone.Blueprint, out var value))
			{
				list.Add(value);
			}
			string key = ParasangX + "." + ParasangY;
			if (world.CellBlueprintsByApplication.TryGetValue(key, out value))
			{
				list.Add(value);
			}
			return list;
		}
		catch (Exception x)
		{
			MetricsManager.LogException("GetCellBlueprints", x);
			return new List<CellBlueprint>();
		}
	}

	public Point3D GetLandingLocation(string World, int ParasangX, int ParasangY)
	{
		foreach (CellBlueprint cellBlueprint in GetCellBlueprints(World, ParasangX, ParasangY))
		{
			if (cellBlueprint.LandingZone.IsNullOrEmpty())
			{
				continue;
			}
			cellBlueprint.LandingZone.AsDelimitedSpans(',', out var First, out var Second, out var Third);
			if (int.TryParse(First, out var result) && int.TryParse(Second, out var result2))
			{
				if (!int.TryParse(Third, out var result3))
				{
					result3 = 10;
				}
				return new Point3D(result, result2, result3);
			}
		}
		return new Point3D(1, 1, 10);
	}

	public void SetZoneDisplayName(string ZoneID, string Name, bool Sync = true)
	{
		SetZoneBaseDisplayName(ZoneID, Name, Sync);
	}

	public ZoneBlueprint GetZoneBlueprint(string ZoneID)
	{
		return GetZoneBlueprint(new ZoneRequest(ZoneID));
	}

	public ZoneBlueprint GetZoneBlueprint(string ZoneID, out int ZPos)
	{
		ZoneRequest request = new ZoneRequest(ZoneID);
		ZPos = request.Z;
		return GetZoneBlueprint(request);
	}

	public ZoneBlueprint GetZoneBlueprint(ZoneRequest Request)
	{
		if (Request.IsWorldZone)
		{
			return null;
		}
		ref WorldBlueprint world = ref Request.World;
		if (world == null)
		{
			world = WorldFactory.Factory.getWorld(Request.WorldID);
		}
		return Request.World?.GetBlueprintFor(Request);
	}

	public List<ZoneBlueprint> GetZoneBlueprints(ZoneRequest Request)
	{
		if (Request.IsWorldZone)
		{
			return null;
		}
		ref WorldBlueprint world = ref Request.World;
		if (world == null)
		{
			world = WorldFactory.Factory.getWorld(Request.WorldID);
		}
		return Request.World?.GetBlueprintsFor(Request);
	}

	public ZoneBlueprint GetZoneBlueprint(string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		return GetZoneBlueprint(new ZoneRequest(WorldID, wXPos, wYPos, XPos, YPos, ZPos));
	}

	public ZoneBlueprint GetZoneBlueprint(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (ZoneID.IndexOf('.') == -1)
		{
			return null;
		}
		return GetZoneBlueprint(WorldID, wXPos, wYPos, XPos, YPos, ZPos);
	}

	public string GetZoneDisplayName(string ZoneID, int ZPos, ZoneBlueprint ZBP, bool WithIndefiniteArticle = false, bool WithDefiniteArticle = false, bool WithStratum = true, bool Mutate = true)
	{
		string zoneBaseDisplayName = GetZoneBaseDisplayName(ZoneID, ZBP, Mutate);
		if (zoneBaseDisplayName != null && ZoneID != null && ZoneID.StartsWith("ThinWorld"))
		{
			return zoneBaseDisplayName;
		}
		SB.Clear();
		if (!zoneBaseDisplayName.IsNullOrEmpty())
		{
			if (WithIndefiniteArticle)
			{
				string zoneIndefiniteArticle = GetZoneIndefiniteArticle(ZoneID, ZBP);
				if (zoneIndefiniteArticle == null || zoneIndefiniteArticle == "a")
				{
					if (!GetZoneHasProperName(ZoneID, ZBP))
					{
						SB.Append(Grammar.IndefiniteArticleShouldBeAn(zoneBaseDisplayName) ? "an " : "a ");
					}
				}
				else if (zoneIndefiniteArticle != "")
				{
					SB.Append(zoneIndefiniteArticle).Append(' ');
				}
			}
			else if (WithDefiniteArticle)
			{
				string zoneDefiniteArticle = GetZoneDefiniteArticle(ZoneID, ZBP);
				if (zoneDefiniteArticle == null)
				{
					if (!GetZoneHasProperName(ZoneID, ZBP))
					{
						SB.Append("the ");
					}
				}
				else if (zoneDefiniteArticle == "a")
				{
					if (!GetZoneHasProperName(ZoneID, ZBP))
					{
						SB.Append(Grammar.IndefiniteArticleShouldBeAn(zoneBaseDisplayName) ? "an " : "a ");
					}
				}
				else if (zoneDefiniteArticle != "")
				{
					SB.Append(zoneDefiniteArticle).Append(' ');
				}
			}
			SB.Append(zoneBaseDisplayName);
		}
		if (zoneBaseDisplayName.IsNullOrEmpty() || GetZoneIncludeContextInZoneDisplay(ZoneID, ZBP))
		{
			string zoneNameContext = GetZoneNameContext(ZoneID, ZBP);
			if (zoneNameContext != null)
			{
				if (SB.Length > 0)
				{
					SB.Append(", ");
				}
				if (!WithDefiniteArticle && (zoneNameContext.StartsWith("the ") || zoneNameContext.StartsWith("The ")))
				{
					SB.Append(zoneNameContext, 4, zoneNameContext.Length - 4);
				}
				else if (!WithIndefiniteArticle && (zoneNameContext.StartsWith("an ") || zoneNameContext.StartsWith("An ")))
				{
					SB.Append(zoneNameContext, 3, zoneNameContext.Length - 3);
				}
				else if (!WithIndefiniteArticle && (zoneNameContext.StartsWith("a ") || zoneNameContext.StartsWith("A ")))
				{
					SB.Append(zoneNameContext, 2, zoneNameContext.Length - 2);
				}
				else
				{
					SB.Append(zoneNameContext);
				}
			}
		}
		if (WithStratum && GetZoneIncludeStratumInZoneDisplay(ZoneID, ZBP))
		{
			int num = 10 - ZPos;
			if (num == 0)
			{
				SB.Compound("surface", ", ");
				if (ZBP != null && ZBP.AnyBuilder((ZoneBuilderBlueprint b) => b.Class == "FlagInside"))
				{
					SB.Append(" level");
				}
			}
			else
			{
				int num2 = Math.Abs(num);
				SB.Compound(num2, ", ").Append((num2 == 1) ? " stratum " : " strata ");
				if (num < 0)
				{
					SB.Append("deep");
				}
				else
				{
					SB.Append("high");
				}
			}
		}
		return SB.ToString();
	}

	public void SynchronizeZoneName(string ZoneID, int ZPos, ZoneBlueprint ZBP)
	{
		WorldFactory.Factory?.UpdateZoneDisplayName(ZoneID, GetZoneDisplayName(ZoneID, ZPos, ZBP));
		NameUpdateTick++;
	}

	public void SynchronizeZoneName(string ZoneID)
	{
		ZoneBlueprint zoneBlueprint = GetZoneBlueprint(ZoneID, out var ZPos);
		SynchronizeZoneName(ZoneID, ZPos, zoneBlueprint);
	}

	public string GetZoneDisplayName(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos, bool WithIndefiniteArticle = false, bool WithDefiniteArticle = false, bool WithStratum = true, bool Mutate = true)
	{
		return GetZoneDisplayName(ZoneID, ZPos, GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos), WithIndefiniteArticle, WithDefiniteArticle, WithStratum, Mutate);
	}

	public string GetZoneDisplayName(string ZoneID, bool WithIndefiniteArticle = false, bool WithDefiniteArticle = false, bool WithStratum = true, bool Mutate = true)
	{
		ZoneBlueprint zoneBlueprint = GetZoneBlueprint(ZoneID, out var ZPos);
		return GetZoneDisplayName(ZoneID, ZPos, zoneBlueprint, WithIndefiniteArticle, WithDefiniteArticle, WithStratum, Mutate);
	}

	public string GetZoneBaseDisplayName(string ZoneID, ZoneBlueprint ZBP, bool Mutate = true)
	{
		string text = The.Game.GetStringGameState("ZoneName_" + ZoneID, null);
		if (text.IsNullOrEmpty())
		{
			if (ZBP == null)
			{
				if (ZoneID.IndexOf('.') == -1)
				{
					text = WorldFactory.Factory.getWorld(ZoneID).DisplayName;
				}
			}
			else
			{
				text = ZBP.Name;
			}
		}
		if (!Mutate || (ZBP != null && !ZBP.HasBiomes))
		{
			return text;
		}
		return BiomeManager.MutateZoneName(text, ZoneID);
	}

	public string GetZoneBaseDisplayName(string ZoneID, bool Mutate = true)
	{
		return GetZoneBaseDisplayName(ZoneID, GetZoneBlueprint(ZoneID), Mutate);
	}

	public void SetZoneBaseDisplayName(string ZoneID, string Name, bool Sync = true)
	{
		The.Game.SetStringGameState("ZoneName_" + ZoneID, Name);
		if (Sync)
		{
			SynchronizeZoneName(ZoneID);
		}
		else
		{
			WorldFactory.Factory?.UpdateZoneDisplayName(ZoneID, Name);
		}
	}

	public string GetZoneReferenceDisplayName(string ZoneID, int ZPos, ZoneBlueprint ZBP)
	{
		string text = GetZoneBaseDisplayName(ZoneID, ZBP);
		if (!text.IsNullOrEmpty())
		{
			if (text.StartsWith("some "))
			{
				text = text.Substring(5);
			}
			if (text.StartsWith("the ") || text.StartsWith("The "))
			{
				text = text.Substring(4);
			}
			else if (text.StartsWith("an ") || text.StartsWith("An "))
			{
				text = text.Substring(3);
			}
			else if (text.StartsWith("a ") || text.StartsWith("A "))
			{
				text = text.Substring(2);
			}
		}
		SB.Clear();
		if (text.IsNullOrEmpty() || GetZoneIncludeContextInZoneDisplay(ZoneID, ZBP))
		{
			string zoneNameContext = GetZoneNameContext(ZoneID, ZBP);
			if (zoneNameContext != null)
			{
				if (zoneNameContext.StartsWith("the ") || zoneNameContext.StartsWith("The "))
				{
					SB.Append(zoneNameContext, 4, zoneNameContext.Length - 4);
				}
				else if (zoneNameContext.StartsWith("an ") || zoneNameContext.StartsWith("An "))
				{
					SB.Append(zoneNameContext, 3, zoneNameContext.Length - 3);
				}
				else if (zoneNameContext.StartsWith("a ") || zoneNameContext.StartsWith("A "))
				{
					SB.Append(zoneNameContext, 2, zoneNameContext.Length - 2);
				}
				else
				{
					SB.Append(zoneNameContext);
				}
			}
		}
		if (!text.IsNullOrEmpty())
		{
			SB.Compound(text);
		}
		if (GetZoneIncludeStratumInZoneDisplay(ZoneID, ZBP))
		{
			int num = 10 - ZPos;
			if (num != 0)
			{
				int num2 = Math.Abs(num);
				SB.Compound(num2).Append((num2 == 1) ? " stratum " : " strata ");
				if (num < 0)
				{
					SB.Append("deep");
				}
				else
				{
					SB.Append("high");
				}
			}
		}
		return SB.ToString();
	}

	public string GetZoneReferenceDisplayName(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		return GetZoneReferenceDisplayName(ZoneID, ZPos, GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos));
	}

	public string GetZoneReferenceDisplayName(string ZoneID)
	{
		ZoneBlueprint zoneBlueprint = GetZoneBlueprint(ZoneID, out var ZPos);
		return GetZoneReferenceDisplayName(ZoneID, ZPos, zoneBlueprint);
	}

	public void SetZoneNameContext(string ZoneID, string Value, bool Sync = true)
	{
		The.Game.SetStringGameState("ZoneNameContext_" + ZoneID, Value);
		if (Sync)
		{
			SynchronizeZoneName(ZoneID);
		}
	}

	public string GetZoneNameContext(string ZoneID, ZoneBlueprint ZBP)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneNameContext_" + ZoneID, null);
		if (stringGameState != null)
		{
			if (stringGameState == "")
			{
				return null;
			}
			return stringGameState;
		}
		return ZBP?.NameContext;
	}

	public string GetZoneNameContext(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneNameContext_" + ZoneID, null);
		if (stringGameState != null)
		{
			if (stringGameState == "")
			{
				return null;
			}
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.NameContext;
	}

	public string GetZoneNameContext(string ZoneID)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneNameContext_" + ZoneID, null);
		if (stringGameState != null)
		{
			if (stringGameState == "")
			{
				return null;
			}
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.NameContext;
	}

	public void SetZoneHasProperName(string ZoneID, bool? State)
	{
		if (!State.HasValue)
		{
			The.Game.RemoveBooleanGameState("ZoneProperName_" + ZoneID);
		}
		else
		{
			The.Game.SetBooleanGameState("ZoneProperName_" + ZoneID, State.Value);
		}
		SynchronizeZoneName(ZoneID);
	}

	public bool GetZoneHasProperName(string ZoneID, ZoneBlueprint ZBP)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneProperName_" + ZoneID, out var Result))
		{
			return Result;
		}
		if (ZBP != null)
		{
			return ZBP.ProperName;
		}
		if (ZoneID.IndexOf('.') == -1)
		{
			return true;
		}
		return false;
	}

	public bool GetZoneHasProperName(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneProperName_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.ProperName ?? false;
	}

	public bool GetZoneHasProperName(string ZoneID)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneProperName_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID)?.ProperName ?? false;
	}

	public void SetZoneIndefiniteArticle(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneIndefiniteArticle_" + ZoneID, Value);
		SynchronizeZoneName(ZoneID);
	}

	public string GetZoneIndefiniteArticle(string ZoneID, ZoneBlueprint ZBP)
	{
		string text = "ZoneIndefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return ZBP?.IndefiniteArticle;
	}

	public string GetZoneIndefiniteArticle(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string text = "ZoneIndefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.IndefiniteArticle;
	}

	public string GetZoneAmbientBed(string ZoneID)
	{
		string text = "ZoneAmbientBed_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		ZoneRequest request = new ZoneRequest(ZoneID);
		if (request.IsWorldZone)
		{
			ref WorldBlueprint world = ref request.World;
			if (world == null)
			{
				world = WorldFactory.Factory.getWorld(request.WorldID);
			}
			return request.World?.AmbientBed;
		}
		return GetZoneBlueprint(request)?.AmbientBed;
	}

	public string GetZoneAmbientSounds(string ZoneID)
	{
		string text = "ZoneAmbientSounds_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.AmbientSounds;
	}

	public int GetZoneAmbientVolume(string ZoneID)
	{
		string key = "ZoneAmbientVolume_" + ZoneID;
		if (The.Game.IntGameState.TryGetValue(key, out var value))
		{
			return value;
		}
		return GetZoneBlueprint(ZoneID)?.AmbientVolume ?? (-1);
	}

	public string GetZoneIndefiniteArticle(string ZoneID)
	{
		string text = "ZoneIndefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.IndefiniteArticle;
	}

	public void SetZoneDefiniteArticle(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneDefiniteArticle_" + ZoneID, Value);
		SynchronizeZoneName(ZoneID);
	}

	public string GetZoneDefiniteArticle(string ZoneID, ZoneBlueprint ZBP)
	{
		string text = "ZoneDefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return ZBP?.DefiniteArticle;
	}

	public string GetZoneDefiniteArticle(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string text = "ZoneDefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.DefiniteArticle;
	}

	public string GetZoneDefiniteArticle(string ZoneID)
	{
		string text = "ZoneDefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.DefiniteArticle;
	}

	public void SetZoneName(string ZoneID, string Name, string Context = null, string IndefiniteArticle = null, string DefiniteArticle = null, string Article = null, bool Proper = false, bool Sync = true)
	{
		if (Name != null)
		{
			if (Name.StartsWith("the ") || Name.StartsWith("The "))
			{
				Name = Name.Substring(4);
				Article = "the";
				Proper = true;
			}
			else if (Name.StartsWith("an ") || Name.StartsWith("An "))
			{
				Name = Name.Substring(3);
				Article = "a";
				Proper = true;
			}
			else if (Name.StartsWith("a ") || Name.StartsWith("A "))
			{
				Name = Name.Substring(2);
				Article = "a";
				Proper = true;
			}
			else if (Name.StartsWith("some "))
			{
				Name = Name.Substring(5);
				Article = "some";
				Proper = false;
			}
		}
		if (Article != null)
		{
			IndefiniteArticle = Article;
			DefiniteArticle = Article;
		}
		if (Proper)
		{
			if (IndefiniteArticle == null)
			{
				IndefiniteArticle = "";
			}
			if (DefiniteArticle == null)
			{
				DefiniteArticle = "";
			}
		}
		SetZoneDisplayName(ZoneID, Name, Sync);
		SetZoneNameContext(ZoneID, Context ?? "", Sync);
		SetZoneHasProperName(ZoneID, Proper);
		SetZoneIndefiniteArticle(ZoneID, IndefiniteArticle);
		SetZoneDefiniteArticle(ZoneID, DefiniteArticle);
	}

	public void SetZoneIncludeContextInZoneDisplay(string ZoneID, bool? State)
	{
		if (!State.HasValue)
		{
			The.Game.RemoveBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID);
		}
		else
		{
			The.Game.SetBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID, State.Value);
		}
		SynchronizeZoneName(ZoneID);
	}

	public bool GetZoneIncludeContextInZoneDisplay(string ZoneID, ZoneBlueprint ZBP)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return ZBP?.IncludeContextInZoneDisplay ?? true;
	}

	public bool GetZoneIncludeContextInZoneDisplay(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.IncludeContextInZoneDisplay ?? true;
	}

	public bool GetZoneIncludeContextInZoneDisplay(string ZoneID)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID)?.IncludeContextInZoneDisplay ?? true;
	}

	public void SetZoneIncludeStratumInZoneDisplay(string ZoneID, bool? State)
	{
		if (!State.HasValue)
		{
			The.Game.RemoveBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID);
		}
		else
		{
			The.Game.SetBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID, State.Value);
		}
		SynchronizeZoneName(ZoneID);
	}

	public bool GetZoneIncludeStratumInZoneDisplay(string ZoneID, ZoneBlueprint ZBP)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return ZBP?.IncludeStratumInZoneDisplay ?? true;
	}

	public bool GetZoneIncludeStratumInZoneDisplay(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.IncludeStratumInZoneDisplay ?? true;
	}

	public bool GetZoneIncludeStratumInZoneDisplay(string ZoneID)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID)?.IncludeContextInZoneDisplay ?? true;
	}

	public void SetZoneNamedByPlayer(string ZoneID, bool State)
	{
		if (State)
		{
			The.Game.SetBooleanGameState("ZoneNamedByPlayer_" + ZoneID, Value: true);
		}
		else
		{
			The.Game.RemoveBooleanGameState("ZoneNamedByPlayer_" + ZoneID);
		}
	}

	public bool GetZoneNamedByPlayer(string ZoneID)
	{
		return The.Game.GetBooleanGameState("ZoneNamedByPlayer_" + ZoneID);
	}

	public void SetZoneHasWeather(string ZoneID, bool? State)
	{
		if (!State.HasValue)
		{
			The.Game.RemoveBooleanGameState("ZoneHasWeather_" + ZoneID);
		}
		else
		{
			The.Game.SetBooleanGameState("ZoneHasWeather_" + ZoneID, State.Value);
		}
	}

	public bool GetZoneHasStaticRingGate(string ZoneID, ZoneBlueprint ZBP)
	{
		return ZBP?.HasWeather ?? false;
	}

	public bool GetZoneStaticRingGate(string ZoneID)
	{
		return GetZoneBlueprint(ZoneID)?.HasWeather ?? false;
	}

	public bool GetZoneHasWeather(string ZoneID, ZoneBlueprint ZBP)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneHasWeather_" + ZoneID, out var Result))
		{
			return Result;
		}
		return ZBP?.HasWeather ?? false;
	}

	public bool GetZoneHasWeather(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneHasWeather_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.HasWeather ?? false;
	}

	public bool GetZoneHasWeather(string ZoneID)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneHasWeather_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID)?.HasWeather ?? false;
	}

	public void SetZoneWindSpeed(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneWindSpeed_" + ZoneID, Value);
	}

	public string GetZoneWindSpeed(string ZoneID, ZoneBlueprint ZBP)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindSpeed_" + ZoneID);
		if (!stringGameState.IsNullOrEmpty())
		{
			return stringGameState;
		}
		return ZBP?.WindSpeed;
	}

	public string GetZoneWindSpeed(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindSpeed_" + ZoneID);
		if (!stringGameState.IsNullOrEmpty())
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.WindSpeed;
	}

	public string GetZoneWindSpeed(string ZoneID)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindSpeed_" + ZoneID);
		if (!stringGameState.IsNullOrEmpty())
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.WindSpeed;
	}

	public void SetZoneWindDirections(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneWindDirections_" + ZoneID, Value);
	}

	public string GetZoneWindDirections(string ZoneID, ZoneBlueprint ZBP)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDirections_" + ZoneID);
		if (!stringGameState.IsNullOrEmpty())
		{
			return stringGameState;
		}
		return ZBP?.WindDirections;
	}

	public string GetZoneWindDirections(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDirections_" + ZoneID);
		if (!stringGameState.IsNullOrEmpty())
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.WindDirections;
	}

	public string GetZoneWindDirections(string ZoneID)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDirections_" + ZoneID);
		if (!stringGameState.IsNullOrEmpty())
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.WindDirections;
	}

	public void SetZoneWindDuration(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneWindDuration_" + ZoneID, Value);
	}

	public string GetZoneWindDuration(string ZoneID, ZoneBlueprint ZBP)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDuration_" + ZoneID);
		if (!stringGameState.IsNullOrEmpty())
		{
			return stringGameState;
		}
		return ZBP?.WindDuration;
	}

	public string GetZoneWindDuration(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDuration_" + ZoneID);
		if (!stringGameState.IsNullOrEmpty())
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.WindDuration;
	}

	public string GetZoneWindDuration(string ZoneID)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDuration_" + ZoneID);
		if (!stringGameState.IsNullOrEmpty())
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.WindDuration;
	}

	public void SetZoneCurrentWindSpeed(string ZoneID, int Value)
	{
		The.Game.SetIntGameState("ZoneCurrentWindSpeed_" + ZoneID, Value);
	}

	public int GetZoneCurrentWindSpeed(string ZoneID, ZoneBlueprint ZBP)
	{
		return GetZoneCurrentWindSpeed(ZoneID);
	}

	public int GetZoneCurrentWindSpeed(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		return GetZoneCurrentWindSpeed(ZoneID);
	}

	public int GetZoneCurrentWindSpeed(string ZoneID)
	{
		return The.Game.GetIntGameState("ZoneCurrentWindSpeed_" + ZoneID);
	}

	public void SetZoneCurrentWindDirection(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneCurrentWindDirection_" + ZoneID, Value);
	}

	public string GetZoneCurrentWindDirection(string ZoneID, ZoneBlueprint ZBP)
	{
		return GetZoneCurrentWindDirection(ZoneID);
	}

	public string GetZoneCurrentWindDirection(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		return GetZoneCurrentWindDirection(ZoneID);
	}

	public string GetZoneCurrentWindDirection(string ZoneID)
	{
		return The.Game.GetStringGameState("ZoneCurrentWindDirection_" + ZoneID);
	}

	public void SetZoneNextWindChange(string ZoneID, long Value)
	{
		The.Game.SetInt64GameState("ZoneNextWindChange_" + ZoneID, Value);
	}

	public long GetZoneNextWindChange(string ZoneID, ZoneBlueprint ZBP)
	{
		return GetZoneCurrentWindSpeed(ZoneID);
	}

	public long GetZoneNextWindChange(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		return GetZoneCurrentWindSpeed(ZoneID);
	}

	public long GetZoneNextWindChange(string ZoneID)
	{
		return The.Game.GetInt64GameState("ZoneNextWindChange_" + ZoneID, 0L);
	}

	public GameObject GetOneCreatureFromZone(string ZoneID)
	{
		string populationName = "LairBosses" + The.Game.ZoneManager.GetZoneTier(ZoneID);
		int num = 0;
		while (++num < 100)
		{
			string blueprint = PopulationManager.RollOneFrom(populationName).Blueprint;
			GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints[blueprint];
			if (gameObjectBlueprint.HasPart("Combat") && gameObjectBlueprint.HasPart("Brain") && (!gameObjectBlueprint.Props.TryGetValue("Role", out var value) || !(value == "Minion")) && (!gameObjectBlueprint.Tags.TryGetValue("Role", out value) || !(value == "Minion")) && !gameObjectBlueprint.HasPart("GenericInventoryRestocker"))
			{
				return GameObject.Create(gameObjectBlueprint);
			}
		}
		return EncountersAPI.GetANonLegendaryCreature();
	}

	public void AdjustZoneGenerationTierTo(string ZoneID)
	{
		zoneGenerationContextZoneID = ZoneID;
		zoneGenerationContextTier = GetZoneTier(ZoneID);
	}

	public int GetZoneTier(string ZoneID)
	{
		if (string.IsNullOrEmpty(ZoneID) || !ZoneID.Contains("."))
		{
			return 1;
		}
		ZoneID.AsDelimitedSpans('.', out var First, out var Second, out var Third, out var _, out var _, out var Sixth);
		return GetZoneTier(new string(First), int.Parse(Second), int.Parse(Third), int.Parse(Sixth));
	}

	public int GetZoneTier(string world, int wXPos, int wYPos, int ZPos)
	{
		if (string.IsNullOrEmpty(world) || world.Contains("."))
		{
			return 1;
		}
		Zone zone = GetZone(world);
		int num = 1;
		foreach (GameObject @object in zone.GetCell(wXPos, wYPos).Objects)
		{
			int num2 = Convert.ToInt32(@object.GetTag("RegionTier", "1"));
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (ZPos > 15)
		{
			num = Math.Abs(ZPos - 16) / 5 + 2;
		}
		if (num < 1)
		{
			num = 1;
		}
		if (num > 8)
		{
			num = 8;
		}
		return num;
	}

	public GameObject GetZoneTerrain(string world, int wXPos, int wYPos)
	{
		return GetZone(world).GetCell(wXPos, wYPos).GetFirstObjectWithPart("TerrainTravel");
	}

	private bool GenerateFactoryZone(ZoneRequest Request, out Zone result)
	{
		result = null;
		IZoneFactory zoneFactoryInstance = Request.World.ZoneFactoryInstance;
		if (zoneFactoryInstance == null)
		{
			return false;
		}
		if (!zoneFactoryInstance.CanBuildZone(Request))
		{
			return false;
		}
		Zone zone = zoneFactoryInstance.BuildZone(Request);
		Zone zone2 = zone;
		if (zone2.ZoneID == null)
		{
			string text = (zone2.ZoneID = Request.ZoneID);
		}
		AddCachedZone(zone);
		BeforeZoneBuiltEvent.Send(zone);
		ZoneBuiltEvent.Send(zone);
		PaintWalls(zone);
		PaintWater(zone);
		AfterZoneBuiltEvent.Send(zone);
		zoneFactoryInstance.AfterBuildZone(zone, this);
		result = zone;
		return true;
	}

	public void GenerateZone(string ZoneID)
	{
		if (!ProcessingZones.Add(ZoneID))
		{
			MetricsManager.LogError("Attempting to generate reserved zone.");
			return;
		}
		Event.PinCurrentPool();
		ZonePartCollection zonePartCollection = ((PartCollections.Count > 0) ? PartCollections.Pop() : new ZonePartCollection());
		ZoneBuilderCollection zoneBuilderCollection = ((BuilderCollections.Count > 0) ? BuilderCollections.Pop() : new ZoneBuilderCollection());
		string text = "[empty]";
		Loading.Status status = Loading.StartTask("Building zone...", "Building " + ZoneID);
		try
		{
			Coach.StartSection("GenerateZone", bTrackGarbage: true);
			int num = 0;
			string text2 = "<none>";
			while (true)
			{
				IL_008d:
				CachedObjectsToRemoveAfterZoneBuild = new List<string>();
				Event.ResetToPin();
				zonePartCollection.Clear();
				zoneBuilderCollection.Clear();
				_ = text2 != "<none>";
				num++;
				bool flag = false;
				if (num >= 20 && num % 5 == 0 && Popup.ShowYesNo("This zone isn't building properly. Do you want to force it to stop and build immediately?") == DialogResult.Yes)
				{
					flag = true;
				}
				ZoneRequest request = new ZoneRequest(ZoneID);
				Zone result;
				foreach (WorldBlueprint world2 in WorldFactory.Factory.getWorlds())
				{
					if (world2.IsMatch(ZoneID))
					{
						request.World = world2;
						if (GenerateFactoryZone(request, out result))
						{
							return;
						}
						break;
					}
				}
				ref WorldBlueprint world;
				if (request.IsWorldZone)
				{
					world = ref request.World;
					if (world == null)
					{
						world = WorldFactory.Factory.getWorld(ZoneID);
					}
					if (GenerateFactoryZone(request, out result))
					{
						return;
					}
					Zone zone = (request.Zone = request.World.GenerateZone(request, 80, 25));
					zone.ZoneID = ZoneID;
					AddCachedZone(zone);
					ZoneGenerationContext = result;
					zoneGenerationContextTier = 1;
					zoneGenerationContextZoneID = ZoneID;
					if (!request.World.Map.IsNullOrEmpty())
					{
						MapBuilder mapBuilder = new MapBuilder();
						mapBuilder.ID = request.World.Map;
						mapBuilder.BuildZone(zone);
					}
					zone.GetCell(0, 0).AddObject("AmbientLight");
					zone.Built = true;
					BeforeZoneBuiltEvent.Send(zone);
					ZoneBuiltEvent.Send(zone);
					PaintWater(zone);
					AfterZoneBuiltEvent.Send(zone);
					break;
				}
				world = ref request.World;
				if (world == null)
				{
					world = WorldFactory.Factory.getWorld(request.WorldID);
				}
				if (!request.World.ZoneFactory.IsNullOrEmpty() && GenerateFactoryZone(request, out result))
				{
					return;
				}
				Zone zone2 = (request.Zone = request.World.GenerateZone(request, 80, 25));
				zone2.ZoneID = ZoneID;
				zone2.BuildTries = num;
				zone2.Tier = GetZoneTier(ZoneID);
				AddCachedZone(zone2);
				ZoneGenerationContext = zone2;
				zoneGenerationContextTier = zone2.NewTier;
				zoneGenerationContextZoneID = ZoneID;
				bool flag2 = false;
				string text3 = (request.Seed = "ZONESEED" + ZoneID);
				text = The.Game.GetWorldSeed(text3).ToString();
				if (num > 30 && !Options.DisableTryLimit)
				{
					zone2.ClearReachableMap(bValue: true);
					MessageQueue.AddPlayerMessage("Zone build failure:" + text2, 'R');
				}
				else
				{
					Stat.ReseedFrom(text + num);
					MetricsManager.rngCheckpoint(text3 + "startbuild" + num);
					if (ZoneBuilders.TryGetValue(ZoneID, out var value))
					{
						zoneBuilderCollection.AddRange(value);
					}
					if (ZoneParts.TryGetValue(ZoneID, out var value2))
					{
						zonePartCollection.AddRange(value2);
					}
					foreach (ZoneBlueprint item in HasZoneProperty(ZoneID, "SkipTerrainBuilders") ? new List<ZoneBlueprint>() : request.World.GetBlueprintsFor(request))
					{
						ApplyPropertiesToZone(item.Cell?.Properties, zone2);
						ApplyPropertiesToZone(item.Properties, zone2);
						if (!item.Parts.IsReadOnlyNullOrEmpty())
						{
							zonePartCollection.AddRange(item.Parts);
						}
						if (!item.Builders.IsReadOnlyNullOrEmpty())
						{
							zoneBuilderCollection.AddRange(item.Builders);
						}
						if (!item.GroundLiquid.IsNullOrEmpty())
						{
							zone2.GroundLiquid = item.GroundLiquid;
						}
					}
					if (!zonePartCollection.ApplyTo(zone2, text3, flag) || !zoneBuilderCollection.ApplyTo(zone2, text3, flag))
					{
						continue;
					}
					if (Options.ShowReachable)
					{
						ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
						for (int i = 0; i < 80; i++)
						{
							for (int j = 0; j < 25; j++)
							{
								scrapBuffer.Goto(i, j);
								if (zone2.IsReachable(i, j))
								{
									scrapBuffer.Write(".");
								}
								else
								{
									scrapBuffer.Write("#");
								}
							}
						}
						foreach (ZoneConnection zoneConnection in GetZoneConnections(zone2.ZoneID))
						{
							scrapBuffer.Goto(zoneConnection.X + 1, zoneConnection.Y + 1);
							scrapBuffer.Write(zoneConnection.ToString());
						}
						foreach (CachedZoneConnection item2 in zone2.ZoneConnectionCache)
						{
							scrapBuffer.Goto(item2.X + 1, item2.Y + 1);
							scrapBuffer.Write(item2.ToString());
						}
						foreach (ZoneConnection zoneConnection2 in GetZoneConnections(zone2.ZoneID))
						{
							scrapBuffer.Goto(zoneConnection2.X, zoneConnection2.Y);
							if (!zone2.IsReachable(zoneConnection2.X, zoneConnection2.Y))
							{
								scrapBuffer.Write("&RX");
							}
							else
							{
								scrapBuffer.Write("&GX");
							}
						}
						foreach (CachedZoneConnection item3 in zone2.ZoneConnectionCache)
						{
							if (item3.TargetDirection == "-")
							{
								scrapBuffer.Goto(item3.X, item3.Y);
								if (!zone2.IsReachable(item3.X, item3.Y))
								{
									scrapBuffer.Write("&RX");
								}
								else
								{
									scrapBuffer.Write("&GX");
								}
							}
						}
						Popup._TextConsole.DrawBuffer(scrapBuffer);
						Keyboard.getch();
					}
					flag2 |= zone2.GetZoneProperty("DisableForcedConnections") == "Yes";
					if (!flag2)
					{
						List<Point2D> list = new List<Point2D>();
						for (int k = 0; k < zone2.Height; k++)
						{
							for (int l = 0; l < zone2.Width; l++)
							{
								Cell cell = zone2.GetCell(l, k);
								if (cell.HasObjectWithBlueprint("StairsUp") || cell.HasObjectWithBlueprint("StairsDown"))
								{
									cell.ClearWalls();
									list.Add(cell.Pos2D);
								}
							}
						}
						new ForceConnections()._BuildZone(zone2, list);
					}
					Coach.StartSection("ConnectionChecks", bTrackGarbage: true);
					if (!flag2)
					{
						foreach (ZoneConnection zoneConnection3 in GetZoneConnections(zone2.ZoneID))
						{
							if (!zone2.IsReachable(zoneConnection3.X, zoneConnection3.Y) && !flag)
							{
								text2 = "Connection ZC:" + zoneConnection3.Type + "," + zoneConnection3.X + "," + zoneConnection3.Y;
								Coach.EndSection();
								goto IL_008d;
							}
						}
						foreach (CachedZoneConnection item4 in zone2.ZoneConnectionCache)
						{
							if (item4.TargetDirection == "-" && !zone2.IsReachable(item4.X, item4.Y) && !flag)
							{
								text2 = "Connection Cached:" + item4.Type + "," + item4.X + "," + item4.Y + " in cell: " + zone2.GetCell(item4.X, item4.Y).ToString();
								Coach.EndSection();
								goto IL_008d;
							}
						}
					}
					foreach (ZoneConnection item5 in GetZoneConnectionsCopy(zone2.ZoneID))
					{
						Cell cell2 = zone2.GetCell(item5.X, item5.Y);
						cell2.ClearWalls();
						if (!item5.Object.IsNullOrEmpty() && !cell2.HasObject(item5.Object))
						{
							cell2.AddObject(item5.Object);
						}
					}
					CachedObjectsToRemoveAfterZoneBuild.Clear();
				}
				Coach.EndSection();
				zone2.Built = true;
				zone2.WriteZoneConnectionCache();
				if (zone2.IsOutside())
				{
					zone2.GetCell(0, 0).AddObject("DaylightWidget");
				}
				BiomeManager.MutateZone(zone2);
				try
				{
					if (CachedZones != null)
					{
						List<Point2D> list2 = new List<Point2D>();
						for (int m = 0; m < zone2.Height; m++)
						{
							for (int n = 0; n < zone2.Width; n++)
							{
								Cell cell3 = zone2.GetCell(n, m);
								if (cell3.HasObjectWithBlueprint("StairsUp") || cell3.HasObjectWithBlueprint("StairsDown"))
								{
									cell3.ClearWalls();
									list2.Add(cell3.Pos2D);
								}
							}
						}
						string zoneIDFromDirection = zone2.GetZoneIDFromDirection("U");
						if (zoneIDFromDirection != null && CachedZones.TryGetValue(zoneIDFromDirection, out var value3))
						{
							for (int num2 = 0; num2 < value3.Height; num2++)
							{
								for (int num3 = 0; num3 < value3.Width; num3++)
								{
									Cell cell4 = value3.GetCell(num3, num2);
									if (cell4.HasObjectWithBlueprint("OpenShaft"))
									{
										zone2.GetCell(num3, num2).ClearWalls();
										list2.Add(zone2.GetCell(num3, num2).Pos2D);
									}
									if (cell4.HasObjectWithBlueprint("StairsDown"))
									{
										zone2.GetCell(num3, num2).ClearWalls();
										zone2.GetCell(num3, num2).AddObject("StairsUp");
										list2.Add(zone2.GetCell(num3, num2).Pos2D);
									}
								}
							}
						}
						string zoneIDFromDirection2 = zone2.GetZoneIDFromDirection("D");
						if (zoneIDFromDirection2 != null && CachedZones.TryGetValue(zoneIDFromDirection2, out var value4))
						{
							for (int num4 = 0; num4 < value4.Height; num4++)
							{
								for (int num5 = 0; num5 < value4.Width; num5++)
								{
									if (value4.GetCell(num5, num4).HasObjectWithBlueprint("StairsUp"))
									{
										zone2.GetCell(num5, num4).ClearWalls();
										zone2.GetCell(num5, num4).AddObject("StairsDown");
										list2.Add(zone2.GetCell(num5, num4).Pos2D);
									}
								}
							}
						}
						string zoneIDFromDirection3 = zone2.GetZoneIDFromDirection("N");
						if (zoneIDFromDirection3 != null && CachedZones.TryGetValue(zoneIDFromDirection3, out var value5))
						{
							for (int num6 = 0; num6 < value5.Width; num6++)
							{
								Cell cell5 = value5.GetCell(num6, value5.Height - 1);
								if (cell5 != null && !cell5.IsSolid())
								{
									zone2.GetCell(num6, 0).ClearWalls();
								}
							}
						}
						string zoneIDFromDirection4 = zone2.GetZoneIDFromDirection("S");
						if (zoneIDFromDirection4 != null && CachedZones.TryGetValue(zoneIDFromDirection4, out var value6))
						{
							for (int num7 = 0; num7 < value6.Width; num7++)
							{
								Cell cell6 = value6.GetCell(num7, 0);
								if (cell6 != null && !cell6.IsSolid())
								{
									zone2.GetCell(num7, zone2.Height - 1).ClearWalls();
								}
							}
						}
						string zoneIDFromDirection5 = zone2.GetZoneIDFromDirection("E");
						if (zoneIDFromDirection5 != null && CachedZones.TryGetValue(zoneIDFromDirection5, out var value7))
						{
							for (int num8 = 0; num8 < value7.Height; num8++)
							{
								Cell cell7 = value7.GetCell(0, num8);
								if (cell7 != null && !cell7.IsSolid())
								{
									zone2.GetCell(zone2.Width - 1, num8).ClearWalls();
								}
							}
						}
						string zoneIDFromDirection6 = zone2.GetZoneIDFromDirection("W");
						if (zoneIDFromDirection6 != null && CachedZones.TryGetValue(zoneIDFromDirection6, out var value8))
						{
							for (int num9 = 0; num9 < value8.Height; num9++)
							{
								Cell cell8 = value8.GetCell(0, num9);
								if (cell8 != null && !cell8.IsSolid())
								{
									zone2.GetCell(0, num9).ClearWalls();
								}
							}
						}
						try
						{
							if (!flag2 && zone2.GetZoneProperty("DisableForcedConnections") != "Yes")
							{
								new ForceConnections()._BuildZone(zone2, list2);
							}
						}
						catch (Exception x)
						{
							MetricsManager.LogException("Exception connecting stairs", x);
						}
					}
				}
				catch
				{
				}
				SanityCheck(zone2);
				BeforeZoneBuiltEvent.Send(zone2);
				ZoneBuiltEvent.Send(zone2);
				PaintWalls(zone2);
				PaintWater(zone2);
				AfterZoneBuiltEvent.Send(zone2);
				ForceCollect();
				break;
			}
			Coach.EndSection();
		}
		catch (Exception ex)
		{
			if (Popup.ShowYesNo("There was an issue building this zone. Automatically report it to us? " + ex.ToString()) == DialogResult.Yes)
			{
				MetricsManager.LogException("Zone build", ex);
			}
			Zone zone3 = new Zone(80, 25);
			zone3.ZoneID = ZoneID;
			zone3.BuildTries = 0;
			zone3.Tier = 0;
			AddCachedZone(zone3);
		}
		finally
		{
			status.Dispose();
			ProcessingZones.Remove(ZoneID);
			zoneBuilderCollection.Clear();
			BuilderCollections.Push(zoneBuilderCollection);
			zonePartCollection.Clear();
			PartCollections.Push(zonePartCollection);
		}
		Event.ResetToPin();
	}

	public static void SanityCheck(Zone Z)
	{
		try
		{
			for (int i = 0; i < Z.Height; i++)
			{
				for (int j = 0; j < Z.Width; j++)
				{
					Cell cell = Z.Map[j][i];
					if (cell.CountObjectsWithTag("Stairs") <= 1 || cell.CountObjectsWithTag("Pit") < 1)
					{
						continue;
					}
					foreach (GameObject item in cell.GetObjectsWithTag("Stairs"))
					{
						if (!item.HasTag("Pit"))
						{
							item.Obliterate();
						}
					}
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("SanityCheck::NoStairsOnPits", x);
		}
		try
		{
			for (int k = 0; k < Z.Height; k++)
			{
				for (int l = 0; l < Z.Width; l++)
				{
					Cell cell2 = Z.Map[l][k];
					if (cell2.CountObjectsWithTag("Stairs") <= 1)
					{
						continue;
					}
					foreach (GameObject item2 in cell2.GetObjectsWithTag("Stairs"))
					{
						if (item2.TryGetPart<XRL.World.Parts.StairsDown>(out var Part) && Part.PullDown)
						{
							item2.Obliterate();
						}
					}
				}
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("SanityCheck::NoStairsOnShafts", x2);
		}
		try
		{
			for (int m = 0; m < Z.Height; m++)
			{
				for (int n = 0; n < Z.Width; n++)
				{
					Cell cell3 = Z.Map[n][m];
					if (!cell3.HasWall() || !cell3.HasCombatObject())
					{
						continue;
					}
					foreach (GameObject item3 in cell3.GetObjectsWithPart("Combat"))
					{
						if (item3.Brain != null && !item3.Brain.LivesOnWalls && !item3.HasTagOrProperty("IgnoreWallSanityCheck"))
						{
							Cell closestPassableCellFor = cell3.getClosestPassableCellFor(item3);
							item3.DirectMoveTo(closestPassableCellFor);
						}
					}
				}
			}
		}
		catch (Exception x3)
		{
			MetricsManager.LogException("SanityCheck::NoMonstersInWalls", x3);
		}
		try
		{
			for (int num = 0; num < Z.Height; num++)
			{
				for (int num2 = 0; num2 < Z.Width; num2++)
				{
					Cell cell4 = Z.Map[num2][num];
					if (!cell4.HasObjectWithTag("Stairs") || !cell4.HasCombatObject())
					{
						continue;
					}
					foreach (GameObject item4 in cell4.GetObjectsWithTag("Combat"))
					{
						if (!item4.HasTagOrProperty("IgnoreStairSanityCheck"))
						{
							Cell closestPassableCellFor2 = cell4.getClosestPassableCellFor(item4);
							item4.DirectMoveTo(closestPassableCellFor2);
						}
					}
				}
			}
		}
		catch (Exception x4)
		{
			MetricsManager.LogException("SanityCheck::NoMonsterOnStairs", x4);
		}
		try
		{
			for (int num3 = 0; num3 < Z.Height; num3++)
			{
				for (int num4 = 0; num4 < Z.Width; num4++)
				{
					Cell cell5 = Z.Map[num4][num3];
					if (!cell5.HasStairs())
					{
						continue;
					}
					cell5.ClearWalls();
					if (!cell5.IsSolid())
					{
						continue;
					}
					foreach (GameObject solidObject in cell5.GetSolidObjects())
					{
						if (solidObject.CanClear())
						{
							solidObject.Obliterate();
						}
					}
				}
			}
		}
		catch (Exception x5)
		{
			MetricsManager.LogException("SanityCheck::NoStairsOnSolids", x5);
		}
		try
		{
			for (int num5 = 0; num5 < Z.Height; num5++)
			{
				for (int num6 = 0; num6 < Z.Width; num6++)
				{
					Cell cell6 = Z.Map[num6][num5];
					if (!cell6.HasStairs() || !cell6.HasObject((GameObject o) => o.TryGetPart<XRL.World.Parts.StairsDown>(out var Part2) && Part2.PullDown))
					{
						continue;
					}
					foreach (GameObject @object in cell6.GetObjects((GameObject o) => o.CanFall))
					{
						@object.SystemMoveTo(cell6.getClosestPassableCell((Cell c) => !c.HasObjectWithPart("StairsDown")));
					}
				}
			}
		}
		catch (Exception x6)
		{
			MetricsManager.LogException("SanityCheck::No falling objects on pulldowns", x6);
		}
		try
		{
			for (int num7 = 0; num7 < Z.Height; num7++)
			{
				for (int num8 = 0; num8 < Z.Width; num8++)
				{
					Cell cell7 = Z.Map[num8][num7];
					if (!cell7.HasStairs() || cell7.GetObjectsWithTagOrProperty("NoOverlap").Count <= 1)
					{
						continue;
					}
					List<GameObject> objects = cell7.GetObjects((GameObject o) => o.HasTagOrProperty("NoOverlap"));
					objects.RemoveAt(0);
					foreach (GameObject item5 in objects)
					{
						item5.SystemMoveTo(cell7.getClosestPassableCell((Cell c) => !c.HasObjectWithTagOrProperty("NoOverlap")));
					}
				}
			}
		}
		catch (Exception x7)
		{
			MetricsManager.LogException("SanityCheck::No falling objects on pulldowns", x7);
		}
	}

	public void PaintWalls()
	{
		PaintWalls(ActiveZone);
	}

	public static void ShatterCrystals(Zone Z)
	{
		if (!Z.Built)
		{
			return;
		}
		int i = 0;
		for (int num = Z.Height - 2; i < num; i += 2)
		{
			int j = 0;
			for (int num2 = Z.Width - 3; j < num2; j += 3)
			{
				int num3 = j / 3 % 2;
				bool flag = false;
				bool flag2 = true;
				GameObject gameObject = null;
				for (int k = 0; k < 3; k++)
				{
					for (int l = 0; l < 2; l++)
					{
						Cell cell = Z.GetCell(j + k, i + l + num3);
						if (cell != null && !cell.HasObject("CrystalWall"))
						{
							flag2 = false;
							continue;
						}
						flag = true;
						gameObject = Z.GetCell(j + k, i + l + num3)?.GetObjects("CrystalWall").FirstOrDefault();
					}
				}
				int num4 = 2;
				if (!flag || flag2)
				{
					continue;
				}
				for (int m = 0; m < 3; m++)
				{
					for (int n = 0; n < 2; n++)
					{
						GameObject gameObject2 = Z.GetCell(j + m, i + n + num3).GetObjects("CrystalWall").FirstOrDefault();
						GameObject gameObject3 = null;
						if (Stat.Random(1, 100) <= num4)
						{
							gameObject3 = Z.GetCell(j + m, i + n + num3).AddObject("SmallHexCrystal");
						}
						if (gameObject2 != null && gameObject3 != null)
						{
							gameObject3.Render.ColorString = gameObject2.Render.ColorString;
							Render render = gameObject3.Render;
							string[] array = gameObject2.Render.ColorString.Split('&');
							render.DetailColor = ((array != null) ? array[1] : null);
						}
						else if (gameObject != null && gameObject3 != null)
						{
							gameObject3.Render.ColorString = gameObject.Render.ColorString;
							Render render2 = gameObject3.Render;
							string[] array2 = gameObject.Render.ColorString.Split('&');
							render2.DetailColor = ((array2 != null) ? array2[1] : null);
						}
						gameObject2?.CurrentCell?.RemoveObject(gameObject2, Forced: true, System: true, IgnoreGravity: false, Silent: false, NoStack: false, Repaint: false);
						gameObject2?.Obliterate();
					}
				}
			}
		}
	}

	public static void PaintCrystalWalls(Zone Z, int x1 = 0, int y1 = 0, int x2 = -1, int y2 = -1)
	{
		x1 = 0;
		y1 = 0;
		x2 = 80;
		y2 = 25;
		ShatterCrystals(Z);
		Array.Clear(WallSingleTrack, 0, WallSingleTrack.Length);
		Array.Clear(WallMultiTrack, 0, WallMultiTrack.Length);
		for (int i = y1 - 1; i <= y2 + 1; i++)
		{
			for (int j = x1 - 1; j <= x2 + 1; j++)
			{
				if (j < 0 || i < 0 || j > Z.Width - 1 || i > Z.Height - 1)
				{
					continue;
				}
				Cell cell = Z.GetCell(j, i);
				if (cell != null)
				{
					if (!cell.HasObjectWithPropertyOrTag("PaintedCrystal") && 0 == 0)
					{
						WallSingleTrack[j, i] = null;
						WallMultiTrack[j, i] = null;
					}
					else
					{
						GameObject firstObjectWithPropertyOrTag = cell.GetFirstObjectWithPropertyOrTag("PaintedCrystal");
						WallSingleTrack[j, i] = firstObjectWithPropertyOrTag;
						WallMultiTrack[j, i] = null;
					}
				}
			}
		}
		for (int k = y1 - 1; k <= y2 + 1; k++)
		{
			for (int l = x1 - 1; l <= x2 + 1; l++)
			{
				if (l >= 0 && k >= 0 && l <= Z.Width - 1 && k <= Z.Height - 1 && WallSingleTrack[l, k] != null)
				{
					Z.GetCell(l, k);
					PaintCrystalWall(WallSingleTrack[l, k], l, k, WallSingleTrack);
				}
			}
		}
	}

	private static void PaintCrystalWall(GameObject obj, int x, int y, GameObject[,] SingleTrack)
	{
		int num = x % 3;
		int num2 = x / 3 % 2;
		int num3 = (y + num2) % 2;
		bool flag = check(x - 1, y - 1);
		bool flag2 = check(x, y - 1);
		bool flag3 = check(x + 1, y);
		hb.Length = 0;
		if (num == 0 && num3 == 0)
		{
			hb.Append("Assets_Content_Textures_Tiles_cryshex_nw_");
			flag = check(x - 1, y + 1);
			flag2 = check(x - 1, y);
			flag3 = check(x, y - 1);
		}
		if (num == 1 && num3 == 0)
		{
			hb.Append("Assets_Content_Textures_Tiles_cryshex_n_");
			flag = check(x - 2, y - 1);
			flag2 = check(x, y - 1);
			flag3 = check(x + 2, y - 1);
		}
		if (num == 2 && num3 == 0)
		{
			hb.Append("Assets_Content_Textures_Tiles_cryshex_ne_");
			flag = check(x, y - 1);
			flag2 = check(x + 1, y);
			flag3 = check(x + 1, y + 1);
		}
		if (num == 0 && num3 == 1)
		{
			hb.Append("Assets_Content_Textures_Tiles_cryshex_sw_");
			flag = check(x, y + 1);
			flag2 = check(x - 1, y);
			flag3 = check(x - 1, y - 1);
		}
		if (num == 1 && num3 == 1)
		{
			hb.Append("Assets_Content_Textures_Tiles_cryshex_s_");
			flag = check(x + 2, y + 1);
			flag2 = check(x, y + 1);
			flag3 = check(x - 2, y + 1);
		}
		if (num == 2 && num3 == 1)
		{
			hb.Append("Assets_Content_Textures_Tiles_cryshex_se_");
			flag = check(x + 1, y - 1);
			flag2 = check(x + 1, y);
			flag3 = check(x, y + 1);
		}
		if (!flag2)
		{
			hb.Append("0.png");
		}
		else if (flag2 && !flag3 && !flag)
		{
			hb.Append("1.png");
		}
		else if ((flag2 & flag) && !flag3)
		{
			hb.Append("2.png");
		}
		else if ((flag2 & flag3) && !flag)
		{
			hb.Append("3.png");
		}
		else
		{
			hb.Length = 0;
			hb.Append("Assets_Content_Textures_Tiles_cryshex_full.png");
		}
		obj.Render.Tile = hb.ToString();
		bool check(int num4, int num5)
		{
			if (num4 < 0)
			{
				return true;
			}
			if (num5 < 0)
			{
				return true;
			}
			if (num4 >= 80)
			{
				return true;
			}
			if (num5 >= 25)
			{
				return true;
			}
			return SingleTrack[num4, num5] != null;
		}
	}

	public static void PaintWalls(Zone Z, int x1 = 0, int y1 = 0, int x2 = -1, int y2 = -1)
	{
		if (Z == null)
		{
			return;
		}
		if (x2 == -1)
		{
			x2 = Z.Width - 1;
		}
		if (y2 == -1)
		{
			y2 = Z.Height - 1;
		}
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (x2 > Z.Width - 1)
		{
			x2 = Z.Width - 1;
		}
		if (y2 > Z.Height - 1)
		{
			y2 = Z.Height - 1;
		}
		PaintCrystalWalls(Z, x1, y1, x2, y2);
		Array.Clear(WallSingleTrack, 0, WallSingleTrack.Length);
		Array.Clear(WallMultiTrack, 0, WallMultiTrack.Length);
		string text = null;
		List<string> list = null;
		for (int i = y1 - 1; i <= y2 + 1; i++)
		{
			for (int j = x1 - 1; j <= x2 + 1; j++)
			{
				if (j < 0 || i < 0 || j > Z.Width - 1 || i > Z.Height - 1)
				{
					continue;
				}
				Cell cell = Z.GetCell(j, i);
				if (cell == null)
				{
					continue;
				}
				int objectCountWithTagsOrProperties = cell.GetObjectCountWithTagsOrProperties("PaintedWall", "PaintedFence", "PaintWith", CheckPaintabilityEvent.Check);
				if (objectCountWithTagsOrProperties == 0)
				{
					WallSingleTrack[j, i] = null;
					WallMultiTrack[j, i] = null;
					continue;
				}
				if (objectCountWithTagsOrProperties > 1)
				{
					WallSingleTrack[j, i] = null;
					List<GameObject> list2 = new List<GameObject>(objectCountWithTagsOrProperties);
					cell.GetObjectsWithTagsOrProperties("PaintedWall", "PaintedFence", "PaintWith", CheckPaintabilityEvent.Check, list2);
					WallMultiTrack[j, i] = list2;
					foreach (GameObject item in list2)
					{
						string propertyOrTag = item.GetPropertyOrTag("PaintPart");
						if (propertyOrTag != null && propertyOrTag != text)
						{
							if (list == null)
							{
								list = new List<string>(2) { text, propertyOrTag };
							}
							else if (!list.Contains(propertyOrTag))
							{
								list.Add(propertyOrTag);
							}
						}
					}
					continue;
				}
				GameObject firstObjectWithTagsOrProperties = cell.GetFirstObjectWithTagsOrProperties("PaintedWall", "PaintedFence", "PaintWith", CheckPaintabilityEvent.Check);
				WallSingleTrack[j, i] = firstObjectWithTagsOrProperties;
				WallMultiTrack[j, i] = null;
				string propertyOrTag2 = firstObjectWithTagsOrProperties.GetPropertyOrTag("PaintPart");
				if (propertyOrTag2 != null && propertyOrTag2 != text)
				{
					if (list == null)
					{
						list = new List<string>(2) { text, propertyOrTag2 };
					}
					else if (!list.Contains(propertyOrTag2))
					{
						list.Add(propertyOrTag2);
					}
				}
			}
		}
		if (list != null)
		{
			for (int k = y1 - 1; k <= y2 + 1; k++)
			{
				for (int l = x1 - 1; l <= x2 + 1; l++)
				{
					if (l < 0 || k < 0 || l > Z.Width - 1 || k > Z.Height - 1)
					{
						continue;
					}
					Cell cell2 = Z.GetCell(l, k);
					if (cell2 == null)
					{
						continue;
					}
					int objectCountWithPart = cell2.GetObjectCountWithPart(list);
					if (objectCountWithPart > 1)
					{
						List<GameObject> list3 = new List<GameObject>(objectCountWithPart);
						cell2.GetObjectsWithPart(text, list3);
						if (WallMultiTrack[l, k] != null)
						{
							WallMultiTrack[l, k].AddRange(list3);
						}
						else if (WallSingleTrack[l, k] != null)
						{
							WallMultiTrack[l, k] = new List<GameObject>(objectCountWithPart + 1) { WallSingleTrack[l, k] };
							WallMultiTrack[l, k].AddRange(list3);
							WallSingleTrack[l, k] = null;
						}
						else
						{
							WallMultiTrack[l, k] = list3;
						}
					}
					else if (objectCountWithPart == 1)
					{
						GameObject firstObjectWithPart = cell2.GetFirstObjectWithPart(list);
						if (WallMultiTrack[l, k] != null)
						{
							WallMultiTrack[l, k].Add(firstObjectWithPart);
						}
						else if (WallSingleTrack[l, k] != null)
						{
							WallMultiTrack[l, k] = new List<GameObject>(2)
							{
								WallSingleTrack[l, k],
								firstObjectWithPart
							};
							WallSingleTrack[l, k] = null;
						}
						else
						{
							WallSingleTrack[l, k] = firstObjectWithPart;
						}
					}
				}
			}
		}
		else if (text != null)
		{
			for (int m = y1 - 1; m <= y2 + 1; m++)
			{
				for (int n = x1 - 1; n <= x2 + 1; n++)
				{
					if (n < 0 || m < 0 || n > Z.Width - 1 || m > Z.Height - 1)
					{
						continue;
					}
					Cell cell3 = Z.GetCell(n, m);
					if (cell3 == null)
					{
						continue;
					}
					int objectCountWithPart2 = cell3.GetObjectCountWithPart(text);
					if (objectCountWithPart2 > 1)
					{
						List<GameObject> list4 = new List<GameObject>(objectCountWithPart2);
						cell3.GetObjectsWithPart(text, list4);
						if (WallMultiTrack[n, m] != null)
						{
							WallMultiTrack[n, m].AddRange(list4);
						}
						else if (WallSingleTrack[n, m] != null)
						{
							WallMultiTrack[n, m] = new List<GameObject>(objectCountWithPart2 + 1) { WallSingleTrack[n, m] };
							WallMultiTrack[n, m].AddRange(list4);
							WallSingleTrack[n, m] = null;
						}
						else
						{
							WallMultiTrack[n, m] = list4;
						}
					}
					else if (objectCountWithPart2 == 1)
					{
						GameObject firstObjectWithPart2 = cell3.GetFirstObjectWithPart(text);
						if (WallMultiTrack[n, m] != null)
						{
							WallMultiTrack[n, m].Add(firstObjectWithPart2);
						}
						else if (WallSingleTrack[n, m] != null)
						{
							WallMultiTrack[n, m] = new List<GameObject>(2)
							{
								WallSingleTrack[n, m],
								firstObjectWithPart2
							};
							WallSingleTrack[n, m] = null;
						}
						else
						{
							WallSingleTrack[n, m] = firstObjectWithPart2;
						}
					}
				}
			}
		}
		Event.PinCurrentPool();
		StringBuilder f = Event.NewStringBuilder();
		StringBuilder s = Event.NewStringBuilder();
		StringBuilder b = Event.NewStringBuilder();
		for (int num = y1; num <= y2; num++)
		{
			for (int num2 = x1; num2 <= x2; num2++)
			{
				GameObject gameObject = WallSingleTrack[num2, num];
				if (gameObject != null)
				{
					PaintWall(gameObject, num2, num, f, s, b, WallSingleTrack, WallMultiTrack);
				}
				List<GameObject> list5 = WallMultiTrack[num2, num];
				if (list5 == null)
				{
					continue;
				}
				foreach (GameObject item2 in list5)
				{
					PaintWall(item2, num2, num, f, s, b, WallSingleTrack, WallMultiTrack);
				}
			}
		}
		Event.ResetToPin();
	}

	private static void PaintWall(GameObject obj, int x, int y, StringBuilder f, StringBuilder s, StringBuilder b, GameObject[,] SingleTrack, List<GameObject>[,] MultiTrack)
	{
		string tagOrStringProperty = obj.GetTagOrStringProperty("PaintedWall");
		string tagOrStringProperty2 = obj.GetTagOrStringProperty("PaintedFence");
		if ((tagOrStringProperty.IsNullOrEmpty() && tagOrStringProperty2.IsNullOrEmpty()) || !CheckTileChangeEvent.Check(obj))
		{
			return;
		}
		string tagOrStringProperty3 = obj.GetTagOrStringProperty("PaintPart");
		string tagOrStringProperty4 = obj.GetTagOrStringProperty("PaintWith");
		s.Length = 0;
		s.Append('-');
		s.Append(HasWallInDirection(obj, x, y, "N", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
		s.Append(HasWallInDirection(obj, x, y, "NE", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
		s.Append(HasWallInDirection(obj, x, y, "E", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
		s.Append(HasWallInDirection(obj, x, y, "SE", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
		s.Append(HasWallInDirection(obj, x, y, "S", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
		s.Append(HasWallInDirection(obj, x, y, "SW", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
		s.Append(HasWallInDirection(obj, x, y, "W", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
		s.Append(HasWallInDirection(obj, x, y, "NW", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
		f.Length = 0;
		f.Append('_');
		if (HasWallInDirection(obj, x, y, "N", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4))
		{
			f.Append('n');
		}
		if (HasWallInDirection(obj, x, y, "S", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4))
		{
			f.Append('s');
		}
		if (HasWallInDirection(obj, x, y, "E", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4))
		{
			f.Append('e');
		}
		if (HasWallInDirection(obj, x, y, "W", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4))
		{
			f.Append('w');
		}
		bool flag = false;
		if (!tagOrStringProperty.IsNullOrEmpty())
		{
			string text = obj.GetTagOrStringProperty("paintedWallSubstring");
			if (text == null)
			{
				text = tagOrStringProperty.GetRandomSubstring(',', Trim: false, Stat.Rnd2);
			}
			obj.SetStringProperty("paintedWallSubstring", text);
			string value = null;
			if (obj.HasTag("PaintedCheckerboard"))
			{
				value = (((x + y) % 2 == 0) ? "1" : "2");
			}
			b.Length = 0;
			b.Append(obj.GetTagOrStringProperty("PaintedWallAtlas", "Assets_Content_Textures_Tiles_")).Append(text).Append(value)
				.Append(s)
				.Append(obj.GetTagOrStringProperty("PaintedWallExtension", ".bmp"));
			obj.Render.Tile = b.ToString();
			flag = true;
		}
		if (!tagOrStringProperty2.IsNullOrEmpty())
		{
			string text2 = obj.GetTagOrStringProperty("paintedFenceSubstring", tagOrStringProperty2.GetRandomSubstring(',', Trim: false, Stat.Rnd2));
			if (text2 == null)
			{
				text2 = tagOrStringProperty2.GetRandomSubstring(',', Trim: false, Stat.Rnd2);
			}
			obj.SetStringProperty("paintedFenceSubstring", text2);
			b.Length = 0;
			b.Append(obj.GetTagOrStringProperty("PaintedFenceAtlas", "Assets_Content_Textures_Tiles_")).Append(text2).Append(f)
				.Append(obj.GetTagOrStringProperty("PaintedFenceExtension", ".bmp"));
			obj.Render.Tile = b.ToString();
			flag = true;
		}
		if (flag)
		{
			RepaintedEvent.Send(obj);
		}
	}

	public static bool HasWallInDirection(GameObject obj, int x, int y, string D, GameObject[,] SingleTrack, List<GameObject>[,] MultiTrack, string PaintPartName, string PaintWith, bool bEdgeFlag = false)
	{
		if (PaintWith == "!PitVoid")
		{
			return !(obj.CurrentCell.GetCellFromDirection(D)?.HasObject((GameObject o) => o.GetStringProperty("PaintWith") == "PitVoid") ?? false);
		}
		bool flag = _HasWallInDirection(obj, x, y, D, SingleTrack, MultiTrack, PaintPartName, PaintWith, bEdgeFlag);
		if (obj.HasTag("PaintedWallInvert"))
		{
			return !flag;
		}
		return flag;
	}

	public static bool _HasWallInDirection(GameObject obj, int x, int y, string D, GameObject[,] SingleTrack, List<GameObject>[,] MultiTrack, string PaintPartName, string PaintWith, bool bEdgeFlag = false)
	{
		if (D.Contains("N"))
		{
			y--;
		}
		else if (D.Contains("S"))
		{
			y++;
		}
		if (D.Contains("E"))
		{
			x++;
		}
		else if (D.Contains("W"))
		{
			x--;
		}
		if (y < 0)
		{
			return bEdgeFlag;
		}
		if (x < 0)
		{
			return bEdgeFlag;
		}
		if (x > SingleTrack.GetUpperBound(0))
		{
			return bEdgeFlag;
		}
		if (y > SingleTrack.GetUpperBound(1))
		{
			return bEdgeFlag;
		}
		GameObject gameObject = SingleTrack[x, y];
		if (gameObject != null)
		{
			if (obj.Blueprint == gameObject.Blueprint)
			{
				return true;
			}
			if (!PaintPartName.IsNullOrEmpty() && gameObject.HasPart(PaintPartName))
			{
				return true;
			}
			string tagOrStringProperty = gameObject.GetTagOrStringProperty("PaintWith");
			if (tagOrStringProperty == "*")
			{
				return true;
			}
			if (!PaintWith.IsNullOrEmpty() && tagOrStringProperty == PaintWith)
			{
				return true;
			}
		}
		List<GameObject> list = MultiTrack[x, y];
		if (list != null)
		{
			foreach (GameObject item in list)
			{
				if (obj.Blueprint == item.Blueprint)
				{
					return true;
				}
			}
			if (!PaintPartName.IsNullOrEmpty())
			{
				foreach (GameObject item2 in list)
				{
					if (item2.HasPart(PaintPartName))
					{
						return true;
					}
				}
			}
			if (!PaintWith.IsNullOrEmpty())
			{
				foreach (GameObject item3 in list)
				{
					string tagOrStringProperty2 = item3.GetTagOrStringProperty("PaintWith");
					if (tagOrStringProperty2 == "*")
					{
						return true;
					}
					if (!PaintWith.IsNullOrEmpty() && tagOrStringProperty2 == PaintWith)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static void PaintWater(Zone Z, int x1 = 0, int y1 = 0, int x2 = -1, int y2 = -1)
	{
		if (Z == null)
		{
			return;
		}
		if (x2 == -1)
		{
			x2 = Z.Width - 1;
		}
		if (y2 == -1)
		{
			y2 = Z.Height - 1;
		}
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (x2 > Z.Width - 1)
		{
			x2 = Z.Width - 1;
		}
		if (y2 > Z.Height - 1)
		{
			y2 = Z.Height - 1;
		}
		Array.Clear(LiquidTrack, 0, LiquidTrack.Length);
		int num = 0;
		for (int i = y1 - 1; i <= y2 + 1; i++)
		{
			for (int j = x1 - 1; j <= x2 + 1; j++)
			{
				if (j < 0 || i < 0 || x2 > Z.Width - 1 || y2 > Z.Height - 1)
				{
					continue;
				}
				Cell cell = Z.GetCell(j, i);
				if (cell == null)
				{
					continue;
				}
				for (int k = 0; k < cell.Objects.Count; k++)
				{
					if (cell.Objects[k].HasTag("PaintedLiquidAtlas"))
					{
						LiquidTrack[j, i] = cell.Objects[k];
						num++;
						break;
					}
				}
			}
		}
		if (num <= 0)
		{
			return;
		}
		int num2 = 0;
		for (int l = y1; l <= y2; l++)
		{
			for (int m = x1; m <= x2; m++)
			{
				GameObject gameObject = LiquidTrack[m, l];
				if (gameObject == null || !CheckTileChangeEvent.Check(gameObject))
				{
					continue;
				}
				LiquidVolume liquidVolume = gameObject.LiquidVolume;
				BaseLiquid primaryLiquid = liquidVolume.GetPrimaryLiquid();
				if (primaryLiquid == null)
				{
					continue;
				}
				int paintGroup = primaryLiquid.GetPaintGroup(liquidVolume);
				int num3 = (LiquidInDirection(m, l, paintGroup, "N", LiquidTrack, 1) << 7) | (LiquidInDirection(m, l, paintGroup, "E", LiquidTrack, 1) << 5) | (LiquidInDirection(m, l, paintGroup, "S", LiquidTrack, 1) << 3) | (LiquidInDirection(m, l, paintGroup, "W", LiquidTrack, 1) << 1);
				if (liquidVolume.IsWadingDepth())
				{
					if (num3.HasAllBits(160))
					{
						num3 |= LiquidInDirection(m, l, paintGroup, "NE", LiquidTrack, 1) << 6;
					}
					if (num3.HasAllBits(40))
					{
						num3 |= LiquidInDirection(m, l, paintGroup, "SE", LiquidTrack, 1) << 4;
					}
					if (num3.HasAllBits(10))
					{
						num3 |= LiquidInDirection(m, l, paintGroup, "SW", LiquidTrack, 1) << 2;
					}
					if (num3.HasAllBits(130))
					{
						num3 |= LiquidInDirection(m, l, paintGroup, "NW", LiquidTrack, 1);
					}
				}
				liquidVolume.Paint(num3);
				if (++num2 >= num)
				{
					return;
				}
			}
		}
	}

	private static int LiquidInDirection(int x, int y, int group, string D, GameObject[,] Track, int EdgeValue = 0)
	{
		if (D.Contains("N"))
		{
			y--;
		}
		else if (D.Contains("S"))
		{
			y++;
		}
		if (D.Contains("E"))
		{
			x++;
		}
		else if (D.Contains("W"))
		{
			x--;
		}
		if (y < 0)
		{
			return EdgeValue;
		}
		if (x < 0)
		{
			return EdgeValue;
		}
		if (x > Track.GetUpperBound(0))
		{
			return EdgeValue;
		}
		if (y > Track.GetUpperBound(1))
		{
			return EdgeValue;
		}
		GameObject gameObject = Track[x, y];
		if (gameObject != null)
		{
			if (!gameObject.LiquidVolume.CanPaintWith(group))
			{
				return 0;
			}
			return 1;
		}
		return 0;
	}

	public void ProcessGoToPartyLeader()
	{
		JoinPartyLeaderCommand.Issue();
	}

	public static bool ApplyBuilderToZone(string Builder, Zone NewZone)
	{
		string text = "XRL.World.ZoneBuilders." + Builder;
		Type type = ModManager.ResolveType(text);
		if (type == null)
		{
			MetricsManager.LogError("Unknown builder " + text + "!");
			return true;
		}
		object obj = Activator.CreateInstance(type);
		MethodInfo method = type.GetMethod("BuildZone");
		if (method != null && !(bool)method.Invoke(obj, new object[1] { NewZone }))
		{
			return false;
		}
		return true;
	}

	private void ApplyPropertiesToZone(Dictionary<string, object> Properties, Zone Z)
	{
		if (Properties.IsNullOrEmpty())
		{
			return;
		}
		Dictionary<string, object> dictionary = ZoneProperties.GetValue(Z.ZoneID) ?? (ZoneProperties[Z.ZoneID] = new Dictionary<string, object>());
		foreach (KeyValuePair<string, object> Property in Properties)
		{
			dictionary[Property.Key] = Property.Value;
		}
	}

	public bool WantEvent(int ID, int Cascade)
	{
		if (ActiveZone != null && ActiveZone.WantEvent(ID, Cascade))
		{
			return true;
		}
		foreach (Zone value in CachedZones.Values)
		{
			if (value != ActiveZone && value.WantEvent(ID, Cascade))
			{
				return true;
			}
		}
		return false;
	}

	public bool HandleEvent<T>(T E) where T : MinEvent
	{
		if (ActiveZone != null && !ActiveZone.HandleEvent(E))
		{
			return false;
		}
		foreach (Zone value in CachedZones.Values)
		{
			if (value != ActiveZone && !value.HandleEvent(E))
			{
				return false;
			}
		}
		return true;
	}
}
