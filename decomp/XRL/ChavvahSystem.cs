using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using Qud.API;
using UnityEngine;
using XRL.EditorFormats.Map;
using XRL.Language;
using XRL.UI;
using XRL.Wish;
using XRL.World;

namespace XRL;

[Serializable]
[HasWishCommand]
public class ChavvahSystem : IGameSystem
{
	[NonSerialized]
	public const string SECRET_ID = "$chavvahcrown";

	[NonSerialized]
	public const int ROAM_DAYS = 30;

	[NonSerialized]
	public const int CROWN_LEVELS = 3;

	[NonSerialized]
	public bool Attuned;

	[NonSerialized]
	public long ActiveAt = -1L;

	[NonSerialized]
	public string TrunkID;

	[NonSerialized]
	public Dictionary<string, int> Trunk = new Dictionary<string, int>();

	[NonSerialized]
	public string DisplacedID;

	[NonSerialized]
	public Dictionary<string, int> Displaced = new Dictionary<string, int>();

	[NonSerialized]
	private static readonly string[] RoamLocation = new string[2];

	public void RestoreDisplaced()
	{
		Zone zone = The.ZoneManager.GetZone(DisplacedID);
		Dictionary<string, XRL.World.GameObject> cachedObjects = The.ZoneManager.CachedObjects;
		foreach (XRL.World.GameObject item in zone.GetObjectsWithProperty("ChavvahTrunkObject"))
		{
			Cell currentCell = item.CurrentCell;
			if (currentCell != null)
			{
				string iD = item.ID;
				Trunk[iD] = currentCell.X | (currentCell.Y << 16);
				cachedObjects[iD] = item;
				item.RemoveFromContext();
			}
		}
		foreach (KeyValuePair<string, int> item2 in Displaced)
		{
			if (cachedObjects.TryGetValue(item2.Key, out var value))
			{
				cachedObjects.Remove(item2.Key);
				zone.GetCell(item2.Value & 0xFF, item2.Value >> 16)?.AddObject(value);
			}
		}
		DisplacedID = null;
		Displaced.Clear();
		The.ZoneManager.SuspendZone(zone);
	}

	public void PlaceTrunk(Zone Z)
	{
		DisplacedID = Z.ZoneID;
		List<XRL.World.GameObject> remove = XRL.World.Event.NewGameObjectList();
		if (Trunk.IsNullOrEmpty())
		{
			GamePartBlueprint part = GameObjectFactory.Factory.Blueprints["RoamingChavvahTrunkBase"].GetPart("MapChunkPlacement");
			MapFile mapFile = MapFile.Resolve(part.GetParameterString("Map"));
			part.TryGetParameter<int>("Width", out var Value);
			part.TryGetParameter<int>("Height", out var Value2);
			for (int i = 0; i < Value2; i++)
			{
				for (int j = 0; j < Value; j++)
				{
					Cell cell = Z.GetCell((Z.Width - Value) / 2 + j, (Z.Height - Value2) / 2 + i);
					if (cell != null)
					{
						mapFile.Cells[j, i].ApplyTo(cell, CheckEmpty: true, (Cell x) =>
						{
							DisplaceSolid(x, remove);
						}, null, null, null, MarkObject);
					}
				}
			}
			return;
		}
		Dictionary<string, XRL.World.GameObject> cachedObjects = The.ZoneManager.CachedObjects;
		foreach (KeyValuePair<string, int> item in Trunk)
		{
			if (cachedObjects.TryGetValue(item.Key, out var value))
			{
				cachedObjects.Remove(item.Key);
				Cell cell2 = Z.GetCell(item.Value & 0xFF, item.Value >> 16);
				if (cell2 != null)
				{
					DisplaceSolid(cell2, remove);
					cell2.AddObject(value);
				}
			}
		}
		Trunk.Clear();
	}

	public void MarkObject(XRL.World.GameObject Object)
	{
		Object.SetIntProperty("ChavvahTrunkObject", 1);
	}

	public void DisplaceSolid(Cell Cell, List<XRL.World.GameObject> ToRemove)
	{
		foreach (XRL.World.GameObject @object in Cell.Objects)
		{
			if (@object.Physics.IsReal && @object.Render != null && @object.Render.RenderLayer > 1 && !@object.IsPlayerControlled())
			{
				ToRemove.Add(@object);
			}
		}
		if (ToRemove.Count != 0)
		{
			Dictionary<string, XRL.World.GameObject> cachedObjects = The.ZoneManager.CachedObjects;
			for (int num = ToRemove.Count - 1; num >= 0; num--)
			{
				XRL.World.GameObject gameObject = ToRemove[num];
				string iD = gameObject.ID;
				Displaced[iD] = Cell.X | (Cell.Y << 16);
				cachedObjects[iD] = gameObject;
				gameObject.RemoveFromContext();
				ToRemove.RemoveAt(num);
			}
		}
	}

	private bool NoMoonStair(Cell Cell)
	{
		XRL.World.GameObject firstObjectThatInheritsFrom = Cell.GetFirstObjectThatInheritsFrom("Terrain");
		if (firstObjectThatInheritsFrom == null)
		{
			Debug.LogError($"No terrain in world cell at {Cell}");
			return true;
		}
		return firstObjectThatInheritsFrom.Blueprint != "TerrainMoonStair";
	}

	public List<Location2D> GetTargetParasangs()
	{
		Zone zone = The.ZoneManager.GetZone("JoppaWorld");
		List<Location2D> list = new List<Location2D>(20);
		for (int i = 0; i < zone.Height; i++)
		{
			for (int j = 0; j < zone.Width; j++)
			{
				Cell cell = zone.GetCell(j, i);
				if (!NoMoonStair(cell) && !cell.AnyLocalAdjacentCell(NoMoonStair))
				{
					list.Add(Location2D.Get(j, i));
				}
			}
		}
		return list;
	}

	public bool IsValidLocation(string ZoneID)
	{
		if (ZoneID != TrunkID && !The.ZoneManager.IsZoneLive(ZoneID) && JournalAPI.GetMapNotesForZone(ZoneID).IsNullOrEmpty())
		{
			return The.ZoneManager.CountBuildersFor(ZoneID) == 0;
		}
		return false;
	}

	public string PickLocation()
	{
		List<Location2D> targetParasangs = GetTargetParasangs();
		if (targetParasangs.Count == 0)
		{
			return null;
		}
		targetParasangs.ShuffleInPlace();
		List<Location2D> within = Location2D.GetWithin(0, 0, 2, 2);
		foreach (Location2D item in targetParasangs)
		{
			within.ShuffleInPlace();
			foreach (Location2D item2 in within)
			{
				string text = ZoneID.Assemble("JoppaWorld", item.X, item.Y, item2.X, item2.Y, 10);
				if (IsValidLocation(text))
				{
					return text;
				}
			}
		}
		return null;
	}

	public bool Reveal(bool Attune = true)
	{
		JournalMapNote mapNote = JournalAPI.GetMapNote("$chavvahcrown");
		if (mapNote == null)
		{
			if (ActiveAt == -1)
			{
				ActiveAt = Calendar.TotalTimeTicks;
			}
			JournalAPI.AddMapNote(TrunkID, "The roaming keter of Chavvah, Tree of Life", "Oddities", null, "$chavvahcrown", revealed: false, sold: true, -1L);
			mapNote = JournalAPI.GetMapNote("$chavvahcrown");
		}
		else if (Attuned)
		{
			return false;
		}
		Popup.Show(Attune ? "You touch the chiming rock. White noise carries on a distant wind." : ("You discover " + Grammar.InitLowerIfArticle(mapNote.Text) + "!"));
		mapNote.Tracked = (Attuned = Attune);
		mapNote.Reveal();
		return true;
	}

	public void Hide()
	{
		JournalMapNote mapNote = JournalAPI.GetMapNote("$chavvahcrown");
		if (mapNote != null)
		{
			if (Attuned && mapNote.Revealed)
			{
				Popup.Show("Chavvah roams out of feeling range. You are no longer attuned.");
			}
			JournalAPI.DeleteMapNote(mapNote);
			ActiveAt = -1L;
			if (The.ActiveZone.IsWorldMap())
			{
				The.ActiveZone.Activated();
			}
		}
	}

	public static bool IsChavvahBuilder(ZoneBuilderBlueprint Blueprint)
	{
		if (Blueprint.Class == "MapBuilder")
		{
			return Blueprint.GetParameter("FileName", "").StartsWith("Chavvah");
		}
		return false;
	}

	public void SetCrownLocation(string ZoneID)
	{
		string text = ZoneID;
		string text2 = TrunkID;
		for (int i = 1; i <= 3; i++)
		{
			text = The.ZoneManager.GetZoneFromIDAndDirection(text, "U");
			if (text2 != null)
			{
				text2 = The.ZoneManager.GetZoneFromIDAndDirection(text2, "U");
				The.ZoneManager.RemoveZoneBuilders(text2, IsChavvahBuilder);
				The.ZoneManager.RemoveZoneProperty(text2, "ConfirmPullDown");
				The.ZoneManager.MoveZone(text2, text);
			}
			The.ZoneManager.AddZoneBuilder(text, 6000, "MapBuilder", "FileName", $"ChavvahCrownLv{i}.rpm", "ClearChasms", true);
			The.ZoneManager.SetZoneProperty(text, "ConfirmPullDown", true);
		}
		TrunkID = ZoneID;
		RoamLocation[0] = text2;
		RoamLocation[1] = ZoneID;
		GenericCommandEvent.Send(Game, "AfterChavvahLocationSet", null, RoamLocation, this);
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(ZoneActivatedEvent.ID);
		Registrar.Register(TravelSpeedEvent.ID);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		TickLocation(E.Zone);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TravelSpeedEvent E)
	{
		Zone currentZone = E.Actor.CurrentZone;
		if (currentZone != null)
		{
			TickLocation(currentZone);
		}
		return base.HandleEvent(E);
	}

	public void TickLocation(Zone Z)
	{
		if (TrunkID == null)
		{
			SetCrownLocation(PickLocation());
		}
		if (ActiveAt != -1 && Calendar.TotalTimeTicks - ActiveAt > 36000)
		{
			ZoneID.Match(TrunkID, The.ZoneManager.ActiveZone.ZoneID);
			_ = 1;
		}
		if (DisplacedID != TrunkID)
		{
			if (!DisplacedID.IsNullOrEmpty())
			{
				RestoreDisplaced();
			}
			if (TrunkID == Z.ZoneID)
			{
				PlaceTrunk(Z);
				Reveal(Attune: false);
			}
		}
	}

	public override void Read(SerializationReader Reader)
	{
		Attuned = Reader.ReadBoolean();
		ActiveAt = Reader.ReadInt64();
		TrunkID = Reader.ReadOptimizedString();
		Trunk = Reader.ReadDictionary<string, int>();
		DisplacedID = Reader.ReadOptimizedString();
		Displaced = Reader.ReadDictionary<string, int>();
	}

	public override void Write(SerializationWriter Writer)
	{
		Writer.Write(Attuned);
		Writer.Write(ActiveAt);
		Writer.Write(TrunkID);
		Writer.Write(Trunk);
		Writer.Write(DisplacedID);
		Writer.Write(Displaced);
	}

	[WishCommand("chavvah:warp", null)]
	public static void ChavvahWarp()
	{
		ChavvahSystem system = The.Game.GetSystem<ChavvahSystem>();
		if (!string.IsNullOrEmpty(system?.TrunkID))
		{
			The.Player.ZoneTeleport(system.TrunkID);
		}
	}

	[WishCommand("chavvah:roam", null)]
	public static void ChavvahRoam()
	{
		The.Game.GetSystem<ChavvahSystem>().ActiveAt = 0L;
	}

	[WishCommand("chavvah:possible", null)]
	public static void ChavvahPossibleBare()
	{
		ChavvahPossible(null);
	}

	[WishCommand("chavvah:possible", null)]
	public static void ChavvahPossible(string Parameter)
	{
		bool flag = Parameter?.StartsWith("detail") ?? false;
		ChavvahSystem system = The.Game.GetSystem<ChavvahSystem>();
		List<Location2D> targetParasangs = system.GetTargetParasangs();
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		Zone zone = The.ZoneManager.GetZone("JoppaWorld");
		zone.LightAll();
		zone.VisAll();
		zone.Render(scrapBuffer);
		List<Location2D> within = Location2D.GetWithin(0, 0, 2, 2);
		foreach (Location2D item in targetParasangs)
		{
			ConsoleChar consoleChar = scrapBuffer[item.X, item.Y];
			consoleChar.Background = Color.red;
			if (!flag)
			{
				continue;
			}
			int num = 0;
			foreach (Location2D item2 in within)
			{
				string zoneID = ZoneID.Assemble("JoppaWorld", item.X, item.Y, item2.X, item2.Y, 10);
				if (system.IsValidLocation(zoneID))
				{
					num++;
				}
			}
			consoleChar.Tile = null;
			consoleChar.Char = (char)(num + 48);
		}
		scrapBuffer.Draw();
		Keyboard.getch();
	}
}
