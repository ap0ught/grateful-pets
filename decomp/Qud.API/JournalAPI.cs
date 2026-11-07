using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Genkit;
using HistoryKit;
using XRL;
using XRL.Annals;
using XRL.Collections;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Skills.Cooking;

namespace Qud.API;

[HasWishCommand]
[HasGameBasedStaticCache]
public static class JournalAPI
{
	public static StringMap<IBaseJournalEntry> NotesByID = new StringMap<IBaseJournalEntry>();

	public static List<JournalAccomplishment> Accomplishments = new List<JournalAccomplishment>();

	public static List<JournalObservation> Observations = new List<JournalObservation>();

	public static List<JournalMapNote> MapNotes = new List<JournalMapNote>();

	public static List<JournalRecipeNote> RecipeNotes = new List<JournalRecipeNote>();

	public static List<JournalGeneralNote> GeneralNotes = new List<JournalGeneralNote>();

	public static List<JournalSultanNote> SultanNotes = new List<JournalSultanNote>();

	public static List<JournalVillageNote> VillageNotes = new List<JournalVillageNote>();

	public static List<string> _mapNoteCategories = null;

	private static Dictionary<string, List<JournalMapNote>> _mapNotesByZone;

	public static bool sorting = true;

	public static int Count => Enumerable.Count(Accomplishments, (JournalAccomplishment r) => r.Revealed) + Enumerable.Count(MapNotes, (JournalMapNote r) => r.Revealed) + Enumerable.Count(Observations, (JournalObservation r) => r.Revealed) + Enumerable.Count(RecipeNotes, (JournalRecipeNote r) => r.Revealed) + Enumerable.Count(SultanNotes, (JournalSultanNote r) => r.Revealed) + Enumerable.Count(GeneralNotes, (JournalGeneralNote r) => r.Revealed);

	private static Dictionary<string, List<JournalMapNote>> mapNotesByZone
	{
		get
		{
			if (_mapNotesByZone == null)
			{
				_mapNotesByZone = new Dictionary<string, List<JournalMapNote>>();
				foreach (JournalMapNote mapNote in MapNotes)
				{
					if (!_mapNotesByZone.ContainsKey(mapNote.ZoneID))
					{
						_mapNotesByZone.Add(mapNote.ZoneID, new List<JournalMapNote>());
					}
					_mapNotesByZone[mapNote.ZoneID].Add(mapNote);
				}
			}
			return _mapNotesByZone;
		}
	}

	public static bool GetCategoryMapNoteToggle(string category)
	{
		return XRLCore.Core.Game.GetIntGameState(category + "_mapnotetoggle", 1) == 1;
	}

	public static void SetCategoryMapNoteToggle(string category, bool val)
	{
		XRLCore.Core.Game.SetIntGameState(category + "_mapnotetoggle", val ? 1 : 0);
		Cell currentCell = The.Player.GetCurrentCell();
		if (currentCell != null && currentCell.ParentZone.IsWorldMap())
		{
			currentCell.ParentZone.Activated();
		}
	}

	[GameBasedCacheInit]
	public static void Reset()
	{
		NotesByID.Clear();
		Accomplishments.Clear();
		Observations.Clear();
		MapNotes.Clear();
		RecipeNotes.Clear();
		GeneralNotes.Clear();
		SultanNotes.Clear();
		VillageNotes.Clear();
	}

	public static List<JournalSultanNote> GetSultanNotes(Func<JournalSultanNote, bool> condition = null)
	{
		List<JournalSultanNote> list = new List<JournalSultanNote>();
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (condition == null || condition(sultanNote))
			{
				list.Add(sultanNote);
			}
		}
		return list;
	}

	public static void Save(SerializationWriter Writer)
	{
		WriteList(Writer, Accomplishments);
		WriteList(Writer, Observations);
		WriteList(Writer, MapNotes);
		WriteList(Writer, RecipeNotes);
		WriteList(Writer, GeneralNotes);
		WriteList(Writer, SultanNotes);
		WriteList(Writer, VillageNotes);
	}

	public static void Load(SerializationReader Reader)
	{
		NotesByID.Clear();
		ReadList(Reader, ref Accomplishments);
		ReadList(Reader, ref Observations);
		ReadList(Reader, ref MapNotes);
		ReadList(Reader, ref RecipeNotes);
		ReadList(Reader, ref GeneralNotes);
		ReadList(Reader, ref SultanNotes);
		ReadList(Reader, ref VillageNotes);
	}

	public static void WriteList<T>(SerializationWriter Writer, List<T> Value) where T : IBaseJournalEntry
	{
		if (Value == null)
		{
			Writer.WriteOptimized(0);
			return;
		}
		int count = Value.Count;
		Writer.WriteOptimized(count + 1);
		for (int i = 0; i < count; i++)
		{
			Writer.Write(Value[i]);
		}
	}

	public static void ReadList<T>(SerializationReader Reader, ref List<T> Value) where T : IBaseJournalEntry
	{
		Value.Clear();
		int num = Reader.ReadOptimizedInt32() - 1;
		if (num != -1)
		{
			for (int i = 0; i < num; i++)
			{
				T val = (T)Reader.ReadComposite();
				Value.Add(val);
				AddedNote(val);
			}
		}
	}

	public static void AddedNote(IBaseJournalEntry Entry)
	{
		if (!Entry.ID.IsNullOrEmpty() && !NotesByID.TryAdd(Entry.ID, Entry))
		{
			MetricsManager.LogError("Duplicate entry ID: '" + Entry.ID + "'");
		}
	}

	public static bool TryRevealNote(string ID, string LearnedFrom = null, bool Silent = false)
	{
		if (ID != null && NotesByID.TryGetValue(ID, out var Value))
		{
			Value.Reveal(LearnedFrom, Silent);
			return Value.Revealed;
		}
		return false;
	}

	public static bool HasNote(string ID)
	{
		if (NotesByID.TryGetValue(ID, out var Value))
		{
			return Value.Revealed;
		}
		return false;
	}

	public static bool HasUnrevealedNote(string ID)
	{
		if (NotesByID.TryGetValue(ID, out var Value))
		{
			return !Value.Revealed;
		}
		return false;
	}

	public static IEnumerable<IBaseJournalEntry> GetAllNotes()
	{
		foreach (JournalAccomplishment accomplishment in Accomplishments)
		{
			yield return accomplishment;
		}
		foreach (JournalMapNote mapNote in MapNotes)
		{
			yield return mapNote;
		}
		foreach (JournalObservation observation in Observations)
		{
			yield return observation;
		}
		foreach (JournalRecipeNote recipeNote in RecipeNotes)
		{
			yield return recipeNote;
		}
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			yield return sultanNote;
		}
		foreach (JournalGeneralNote generalNote in GeneralNotes)
		{
			yield return generalNote;
		}
		foreach (JournalVillageNote villageNote in VillageNotes)
		{
			yield return villageNote;
		}
	}

	public static IEnumerable<IBaseJournalEntry> GetKnownNotes(Predicate<IBaseJournalEntry> filter = null)
	{
		if (filter == null)
		{
			filter = (IBaseJournalEntry n) => true;
		}
		foreach (IBaseJournalEntry allNote in GetAllNotes())
		{
			if (allNote.Revealed && filter(allNote))
			{
				yield return allNote;
			}
		}
	}

	public static List<JournalSultanNote> GetKnownSultanNotes()
	{
		List<JournalSultanNote> list = new List<JournalSultanNote>();
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.Revealed)
			{
				list.Add(sultanNote);
			}
		}
		return list;
	}

	public static List<JournalSultanNote> GetKnownNotesForSultan(string sultan, Predicate<JournalSultanNote> filter = null)
	{
		List<JournalSultanNote> list = new List<JournalSultanNote>();
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.SultanID == sultan && sultanNote.Revealed && (filter == null || filter(sultanNote)))
			{
				list.Add(sultanNote);
			}
		}
		return list;
	}

	public static List<JournalSultanNote> GetKnownNotesForResheph(Predicate<JournalSultanNote> Filter = null)
	{
		return GetKnownNotesForSultan(HistoryAPI.GetResheph().id, Filter);
	}

	public static List<JournalSultanNote> GetNotesForSultan(string sultan)
	{
		List<JournalSultanNote> list = new List<JournalSultanNote>();
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.SultanID == sultan)
			{
				list.Add(sultanNote);
			}
		}
		return list;
	}

	public static List<JournalSultanNote> GetNotesForResheph()
	{
		return GetNotesForSultan(HistoryAPI.GetResheph().id);
	}

	public static JournalSultanNote RevealSultanEventBySecretID(string id, string LearnedFrom = null)
	{
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.ID == id)
			{
				sultanNote.Reveal(LearnedFrom);
				return sultanNote;
			}
		}
		return null;
	}

	public static JournalSultanNote RevealSultanEvent(long id, string LearnedFrom = null)
	{
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.EventID == id)
			{
				sultanNote.Reveal(LearnedFrom);
				return sultanNote;
			}
		}
		return null;
	}

	public static bool KnowsSultanEvent(long id)
	{
		return SultanNotes.Any((JournalSultanNote n) => n.EventID == id && n.Revealed);
	}

	public static bool HasUnrevealedSultanEvent(long id)
	{
		int i = 0;
		for (int count = SultanNotes.Count; i < count; i++)
		{
			if (SultanNotes[i].EventID == id)
			{
				return !SultanNotes[i].Revealed;
			}
		}
		return false;
	}

	public static bool HasUnrevealedSultanEvent(string id)
	{
		int i = 0;
		for (int count = SultanNotes.Count; i < count; i++)
		{
			if (SultanNotes[i].ID == id)
			{
				return !SultanNotes[i].Revealed;
			}
		}
		return false;
	}

	public static List<JournalVillageNote> GetKnownNotesForVillage(string village)
	{
		List<JournalVillageNote> list = new List<JournalVillageNote>();
		foreach (JournalVillageNote villageNote in VillageNotes)
		{
			if (villageNote.VillageID == village && villageNote.Revealed)
			{
				list.Add(villageNote);
			}
		}
		return list;
	}

	public static List<JournalVillageNote> GetNotesForVillage(string village)
	{
		List<JournalVillageNote> list = new List<JournalVillageNote>();
		foreach (JournalVillageNote villageNote in VillageNotes)
		{
			if (villageNote.VillageID == village)
			{
				list.Add(villageNote);
			}
		}
		return list;
	}

	public static bool IsMapOrVillageNoteRevealed(string secretID)
	{
		JournalMapNote journalMapNote = MapNotes.Find((JournalMapNote m) => m.ID == secretID);
		if (journalMapNote != null && journalMapNote.Revealed)
		{
			return true;
		}
		JournalVillageNote journalVillageNote = VillageNotes.Find((JournalVillageNote m) => m.ID == secretID);
		if (journalVillageNote != null && journalVillageNote.Revealed)
		{
			return true;
		}
		return false;
	}

	public static bool RevealVillageNote(string secretID, string LearnedFrom = null)
	{
		JournalVillageNote journalVillageNote = VillageNotes.Find((JournalVillageNote n) => secretID == n.ID);
		journalVillageNote?.Reveal(LearnedFrom);
		return journalVillageNote?.Revealed ?? false;
	}

	public static void InitializeGossip()
	{
		int num = 60;
		for (int i = 0; i < num; i++)
		{
			string text = Guid.NewGuid().ToString();
			if (Stat.Random(1, 100) <= 75)
			{
				Faction randomFaction = Factions.GetRandomFaction();
				AddObservation(Grammar.InitCap(Gossip.GenerateGossip_OneFaction(randomFaction.Name)), text, "Gossip", text, new string[2]
				{
					"gossip:" + randomFaction.Name,
					"gossip"
				}, revealed: false, -1L);
				continue;
			}
			Faction randomFaction2 = Factions.GetRandomFaction();
			Faction faction = null;
			do
			{
				faction = Factions.GetRandomFaction();
			}
			while (faction == randomFaction2);
			AddObservation(Grammar.InitCap(Gossip.GenerateGossip_TwoFactions(randomFaction2.Name, faction.Name)), text, "Gossip", text, new string[3]
			{
				"gossip:" + randomFaction2.Name,
				"gossip:" + faction.Name,
				"gossip"
			}, revealed: false, -1L);
		}
	}

	public static void InitializeObservations()
	{
		AddObservation("Qud was once called Salum.", "QudSalum", "general", null, new string[1] { "old" }, revealed: false, -1L, null, initCapAsFragment: true);
		AddObservation("The shomer Rainwater claims that Brightsheol is the dream of a seraph who lives atop the Spindle.", "BrightsheolDream", "Gossip", null, new string[2] { "gossip", "old" }, revealed: false, -1L);
		AddObservation("The Palladium Reef was once called Maqqom Yd, the Place outside Itself.", "MaqqomYd", "general", null, new string[1] { "old" }, revealed: false, -1L);
	}

	public static void AddVillageGospels(HistoricEntity Village)
	{
		AddVillageGospels(Village.GetCurrentSnapshot());
	}

	public static void AddVillageGospels(HistoricEntitySnapshot Snapshot)
	{
		foreach (string item in Snapshot.GetList("Gospels"))
		{
			item.Split('|', out var First, out var Second);
			long? eventID = null;
			if (!Second.IsNullOrEmpty() && long.TryParse(Second, out var result))
			{
				eventID = result;
			}
			AddVillageNote(First, Snapshot.entity.id, eventID, "village");
		}
	}

	public static void InitializeVillageEntries()
	{
		foreach (HistoricEntity village in HistoryAPI.GetVillages())
		{
			AddVillageGospels(village);
		}
		AddVillageNote("JoppaSecret1", "In the mystic air of Jeweled Dusk, a circle of farmers clasped hands and sheathed a vinereaper in the moistened soil. From then on, the villagers of Joppa were known as the people of the vine.", "Joppa", null, "village");
		AddVillageNote("JoppaSecret2", "Since the first festival of Ut yara Ux, the villagers of Joppa have feasted on warm apple matz.", "Joppa", null, "village");
		AddVillageNote("KyakukyaSecret1", "Under the Beetle Moon, folks settled here to gather goods of passage for Ape Saad Oboroqoru, and so Kyakukya was founded.", "Kyakukya", null, "village");
		AddVillageNote("KyakukyaSecret2", "In 979, Nuntu, the legendary albino ape, grew tired of bludgeoning things to death and journeyed to a root-strangled place on the River Svy. There he came upon Kyakukya and its inhabitants. The villagers of Kyakukya asked Nuntu to employ his magisterial skills and lead the village.", "Kyakukya", null, "village");
		AddVillageNote("YdFreeholdSecret1", "Centuries after the height of the Gyre, the svardym Goek, Mak, and Geeub met the galgal Many Eyes while languishing inside of a reef sponge. Together they founded the Yd Freehold to live out the rest of their lives.", "The Yd Freehold", null, "village");
		AddVillageNote("YdFreeholdSecret2", "In 901, the villagers of the Yd Freehold and its founders abolished all forms of hierarchy from their settlement. The two pillars of civic life in the Freehold thus became anarchy and authenticity.", "The Yd Freehold", null, "village");
	}

	public static JournalSultanNote AddSultanNote(string Text, string SultanID, long? EventID = null, object Attributes = null)
	{
		return AddSultanNote(Guid.NewGuid().ToString(), Text, SultanID, EventID, Attributes);
	}

	public static JournalSultanNote AddSultanNote(string ID, string Text, string SultanID, long? EventID = null, object Attributes = null)
	{
		JournalSultanNote journalSultanNote = new JournalSultanNote();
		journalSultanNote.ID = ID;
		journalSultanNote.Text = Text;
		journalSultanNote.SultanID = SultanID;
		if (EventID.HasValue)
		{
			journalSultanNote.EventID = EventID.Value;
		}
		if (Attributes is string item)
		{
			journalSultanNote.Attributes.Add(item);
		}
		else if (Attributes is IEnumerable<string> collection)
		{
			journalSultanNote.Attributes.AddRange(collection);
		}
		SultanNotes.Add(journalSultanNote);
		AddedNote(journalSultanNote);
		return journalSultanNote;
	}

	public static void InitializeSultanEntries()
	{
		List<string> list = new List<string>();
		foreach (HistoricEntity sultan in HistoryAPI.GetSultans())
		{
			List<string> sultanLikedFactions = HistoryAPI.GetSultanLikedFactions(sultan);
			List<string> sultanHatedFactions = HistoryAPI.GetSultanHatedFactions(sultan);
			foreach (HistoricEvent sultanEventsWithGospel in HistoryAPI.GetSultanEventsWithGospels(sultan.id))
			{
				list.Clear();
				foreach (string item in sultanLikedFactions)
				{
					list.Add("include:" + item);
				}
				foreach (string item2 in sultanHatedFactions)
				{
					list.Add("include:" + item2);
				}
				list.Add("sultan");
				if (sultanEventsWithGospel.GetEventProperty("rebekah") == "true")
				{
					list.Add("rebekah");
				}
				if (sultanEventsWithGospel.GetEventProperty("rebekahWasHealer") == "true")
				{
					list.Add("rebekahWasHealer");
				}
				if (sultanEventsWithGospel.GetEventProperty("gyreplagues") == "true")
				{
					list.Add("gyreplagues");
				}
				AddSultanNote(sultanEventsWithGospel.GetEventProperty("gospel"), sultan.id, sultanEventsWithGospel.id, list);
			}
			foreach (HistoricEvent sultanEventsWithTombInscription in HistoryAPI.GetSultanEventsWithTombInscriptions(sultan.id))
			{
				list.Clear();
				foreach (string item3 in sultanLikedFactions)
				{
					list.Add("include:" + item3);
				}
				foreach (string item4 in sultanHatedFactions)
				{
					list.Add("include:" + item4);
				}
				list.Add("sultanTombPropaganda");
				list.Add("onlySellIfTargetedAndInterested");
				AddSultanNote(sultanEventsWithTombInscription.GetEventProperty("tombInscription"), sultan.id, sultanEventsWithTombInscription.id, list);
			}
		}
	}

	public static void DeleteAccomplishment(JournalAccomplishment accomplishmentToDelete)
	{
		Accomplishments.Remove(accomplishmentToDelete);
	}

	public static void DeleteGeneralNote(JournalGeneralNote noteToDelete)
	{
		GeneralNotes.Remove(noteToDelete);
	}

	public static void AddAccomplishment(string text, string muralText = null, string gospelText = null, string aggregateWith = null, string category = "general", MuralCategory muralCategory = MuralCategory.Generic, MuralWeight muralWeight = MuralWeight.Medium, string secretId = null, long time = -1L, bool revealed = true)
	{
		if (text.IsNullOrEmpty())
		{
			MetricsManager.LogError("Attempting to add accomplishment without text.");
			return;
		}
		HistoricEntitySnapshot historicEntitySnapshot = QudHistoryFactory.RequirePlayerEntitySnapshot();
		JournalAccomplishment journalAccomplishment = new JournalAccomplishment();
		journalAccomplishment.Category = category;
		journalAccomplishment.MuralCategory = muralCategory;
		journalAccomplishment.MuralWeight = muralWeight;
		journalAccomplishment.MuralText = HistoricStringExpander.ExpandString(muralText ?? "").StartReplace().ForceThirdPerson()
			.ToString();
		journalAccomplishment.GospelText = HistoricStringExpander.ExpandString(gospelText ?? "", historicEntitySnapshot, historicEntitySnapshot.entity.history).StartReplace().ForceThirdPerson()
			.ToString();
		journalAccomplishment.AggregateWith = aggregateWith;
		if (muralText == null && muralWeight != MuralWeight.Nil)
		{
			string word = text.Replace("You ", The.Game.PlayerName + " ").Replace("you ", The.Game.PlayerName + " ").Replace("Your ", Grammar.MakePossessive(The.Game.PlayerName) + " ")
				.Replace("your ", Grammar.MakePossessive(The.Game.PlayerName) + " ");
			journalAccomplishment.MuralText = Grammar.InitCap(word);
			journalAccomplishment.MuralCategory = MuralCategory.DoesSomethingRad;
			journalAccomplishment.MuralWeight = MuralWeight.Medium;
		}
		if (The.Player != null)
		{
			if (The.Player.TryGetEffect<Lovesick>(out var Effect))
			{
				text = HistoricStringExpander.ExpandString("<spice.lovesick.!random>", null, null).Replace("*var*", text).Replace("*varLower*", Grammar.InitLower(text))
					.Replace("*sob*", Grammar.Stutterize(text, "{{w|*sob*}}"))
					.Replace("*varReplacePeriod*", text.Replace(".", ""))
					.Replace("*obj*", Effect.Beauty.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true))
					.Replace("*objShort*", Effect.Beauty.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true));
			}
			if (The.Player.IsAflame())
			{
				text = "While on fire, " + (text.StartsWith(The.Game.PlayerName) ? text : Grammar.InitLower(text));
			}
			else if (The.Player.IsFrozen())
			{
				text = "While frozen solid, " + (text.StartsWith(The.Game.PlayerName) ? text : Grammar.InitLower(text));
			}
			journalAccomplishment.UpdateScreenshot();
		}
		journalAccomplishment.Text = text;
		journalAccomplishment.ID = secretId;
		journalAccomplishment.Tradable = false;
		if (time == -1)
		{
			journalAccomplishment.Time = XRLCore.Core.Game.TimeTicks;
		}
		else
		{
			journalAccomplishment.Time = time;
		}
		Accomplishments.Add(journalAccomplishment);
		AddedNote(journalAccomplishment);
		if (sorting)
		{
			Accomplishments.Sort((JournalAccomplishment a, JournalAccomplishment b) => a.Time.CompareTo(b.Time));
		}
		try
		{
			Physics physics = The.Player.Physics;
			if (physics != null && physics.CurrentCell != null)
			{
				physics.CurrentCell.ParentZone.FireEvent(Event.New("AccomplishmentAdded", "Text", text, "Category", category, "SecretId", secretId));
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
		if (revealed)
		{
			journalAccomplishment.Reveal();
		}
	}

	public static void Init()
	{
		_mapNotesByZone = null;
		_mapNoteCategories = null;
	}

	public static void AddObservation(string text, string id, string category = "general", string secretId = null, string[] attributes = null, bool revealed = false, long time = -1L, string additionalRevealText = null, bool initCapAsFragment = false, bool Tradable = true)
	{
		if (GetObservation(id) != null)
		{
			return;
		}
		JournalObservation journalObservation = new JournalObservation();
		journalObservation.ID = id.Coalesce(secretId);
		journalObservation.Text = text;
		if (revealed)
		{
			journalObservation.Reveal();
		}
		else
		{
			journalObservation.Forget();
		}
		journalObservation.RevealText = additionalRevealText;
		journalObservation.Rumor = initCapAsFragment;
		journalObservation.Tradable = Tradable;
		if (attributes != null)
		{
			journalObservation.Attributes.AddRange(attributes);
		}
		if (time == -1)
		{
			journalObservation.Time = XRLCore.Core.Game.TimeTicks;
		}
		else
		{
			journalObservation.Time = time;
		}
		journalObservation.Category = category;
		Observations.Add(journalObservation);
		AddedNote(journalObservation);
		if (sorting)
		{
			Observations.Sort((JournalObservation a, JournalObservation b) => a.Time.CompareTo(b.Time));
		}
	}

	public static void RevealObservation(JournalObservation note)
	{
		note.Reveal();
		note.Updated();
	}

	public static bool IsObservationRevealed(string secretID)
	{
		JournalObservation journalObservation = Observations.Find((JournalObservation m) => m.ID == secretID);
		if (journalObservation != null && journalObservation.Revealed)
		{
			return true;
		}
		return false;
	}

	public static void RevealObservation(string id, bool onlyIfNotRevealed = false)
	{
		foreach (JournalObservation observation in Observations)
		{
			if (observation.ID == id && (!onlyIfNotRevealed || !observation.Revealed))
			{
				RevealObservation(observation);
			}
		}
	}

	public static JournalObservation GetObservation(string id)
	{
		foreach (JournalObservation observation in Observations)
		{
			if (observation.ID == id)
			{
				return observation;
			}
		}
		return null;
	}

	public static List<JournalObservation> GetObservations(Func<JournalObservation, bool> compare = null)
	{
		List<JournalObservation> list = new List<JournalObservation>();
		foreach (JournalObservation observation in Observations)
		{
			if (compare == null || compare(observation))
			{
				list.Add(observation);
			}
		}
		return list;
	}

	public static List<JournalMapNote> GetRevealedMapNotesForWorldMapCell(int x, int y)
	{
		List<JournalMapNote> list = new List<JournalMapNote>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.Revealed && mapNote.ParasangX == x && mapNote.ParasangY == y)
			{
				list.Add(mapNote);
			}
		}
		return list;
	}

	public static List<JournalMapNote> GetMapNotesForWorldMapCell(int x, int y)
	{
		List<JournalMapNote> list = new List<JournalMapNote>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.ParasangX == x && mapNote.ParasangY == y)
			{
				list.Add(mapNote);
			}
		}
		return list;
	}

	public static List<string> GetMapNoteCategories()
	{
		if (_mapNoteCategories != null)
		{
			return _mapNoteCategories;
		}
		_mapNoteCategories = new List<string>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.Revealed && !_mapNoteCategories.Contains(mapNote.Category))
			{
				_mapNoteCategories.Add(mapNote.Category);
			}
		}
		_mapNoteCategories.Sort();
		return _mapNoteCategories;
	}

	public static List<JournalMapNote> GetMapNotesForZone(string zoneId)
	{
		if (mapNotesByZone.TryGetValue(zoneId, out var value))
		{
			return value;
		}
		return new List<JournalMapNote>();
	}

	public static JournalMapNote GetMapNote(string secretId)
	{
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.ID == secretId)
			{
				return mapNote;
			}
		}
		return null;
	}

	public static bool HasMapNote(string WorldID, int WX = -1, int WY = -1, int X = -1, int Y = -1, int Z = -1)
	{
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if ((WX == -1 || mapNote.ParasangX == WX) && (WY == -1 || mapNote.ParasangY == WY) && (X == -1 || mapNote.ZoneX == X) && (Y == -1 || mapNote.ZoneY == Y) && (Z == -1 || mapNote.ZoneZ == Z) && mapNote.WorldID == WorldID)
			{
				return true;
			}
		}
		return false;
	}

	public static List<JournalMapNote> GetMapNotes(Func<JournalMapNote, bool> compare = null)
	{
		List<JournalMapNote> list = new List<JournalMapNote>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (compare == null || compare(mapNote))
			{
				list.Add(mapNote);
			}
		}
		return list;
	}

	public static List<JournalMapNote> GetMapNotesWithAllAttributes(string attributes)
	{
		return GetMapNotes((JournalMapNote note) =>
		{
			bool result = true;
			string[] array = attributes.Split(',');
			foreach (string att in array)
			{
				if (!note.Has(att))
				{
					result = false;
					break;
				}
			}
			return result;
		});
	}

	public static List<JournalMapNote> GetMapNotesInCardinalDirections(string zoneid)
	{
		ZoneID.Parse(zoneid, out var world, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY, out var _);
		int rx = ParasangX * 3 + ZoneX;
		int ry = ParasangY * 3 + ZoneY;
		Location2D location = Location2D.Get(rx, ry);
		List<JournalMapNote> mapNotes = GetMapNotes((JournalMapNote note) =>
		{
			if (string.Equals(note.Category, "Miscellaneous"))
			{
				return false;
			}
			if (note.WorldID != world)
			{
				return false;
			}
			if (rx == note.ResolvedX && ry == note.ResolvedY)
			{
				return false;
			}
			if (rx == note.ResolvedX)
			{
				return true;
			}
			return (ry == note.ResolvedY) ? true : false;
		});
		if (mapNotes.Count == 0)
		{
			return null;
		}
		mapNotes.Sort((JournalMapNote a, JournalMapNote b) => location.Distance(a.ResolvedLocation).CompareTo(location.Distance(b.ResolvedLocation)));
		return mapNotes;
	}

	public static List<JournalMapNote> GetUnrevealedMapNotesWithinWorldRadiusN(string zoneid, int min, int max)
	{
		ZoneID.Parse(zoneid, out var world, out var px, out var py, out var _, out var _, out var z);
		return GetMapNotes((JournalMapNote note) =>
		{
			if (note.Revealed)
			{
				return false;
			}
			if (note.WorldID != world)
			{
				return false;
			}
			if (Math.Abs(px - note.ParasangX) > max)
			{
				return false;
			}
			if (Math.Abs(py - note.ParasangY) > max)
			{
				return false;
			}
			if (Math.Abs(z - note.ZoneZ) > max)
			{
				return false;
			}
			if (Math.Abs(px - note.ParasangX) < min)
			{
				return false;
			}
			if (Math.Abs(py - note.ParasangY) < min)
			{
				return false;
			}
			return (Math.Abs(z - note.ZoneZ) >= min) ? true : false;
		});
	}

	public static List<JournalMapNote> GetUnrevealedMapNotesWithinZoneRadiusN(string zoneid, int min, int max, Predicate<Location2D> isValid = null)
	{
		ZoneID.Parse(zoneid, out var world, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY, out var z);
		int rx = ParasangX * 3 + ZoneX;
		int ry = ParasangY * 3 + ZoneY;
		return GetMapNotes((JournalMapNote note) =>
		{
			if (note.Revealed)
			{
				return false;
			}
			if (note.WorldID != world)
			{
				return false;
			}
			if (note.ZoneID == zoneid)
			{
				return false;
			}
			if (Math.Abs(rx - note.ResolvedX) > max)
			{
				return false;
			}
			if (Math.Abs(ry - note.ResolvedY) > max)
			{
				return false;
			}
			if (Math.Abs(z - note.ZoneZ) > 0)
			{
				return false;
			}
			if (Math.Abs(rx - note.ResolvedX) < min)
			{
				return false;
			}
			if (Math.Abs(ry - note.ResolvedY) < min)
			{
				return false;
			}
			if (Math.Abs(z - note.ZoneZ) > 0)
			{
				return false;
			}
			return isValid == null || isValid(note.ResolvedLocation);
		});
	}

	public static List<JournalMapNote> GetUnrevealedMapNotesWithinWorldRadiusN(Zone z, int min, int max, Predicate<Location2D> isValid = null)
	{
		return GetMapNotes((JournalMapNote note) =>
		{
			if (note.Revealed)
			{
				return false;
			}
			if (Math.Abs(z.wX - note.ParasangX) > max)
			{
				return false;
			}
			if (Math.Abs(z.wY - note.ParasangY) > max)
			{
				return false;
			}
			if (Math.Abs(z.Z - note.ZoneZ) > max)
			{
				return false;
			}
			if (Math.Abs(z.wX - note.ParasangX) < min)
			{
				return false;
			}
			if (Math.Abs(z.wY - note.ParasangY) < min)
			{
				return false;
			}
			if (Math.Abs(z.Z - note.ZoneZ) < min)
			{
				return false;
			}
			return isValid == null || isValid(note.ResolvedLocation);
		});
	}

	public static List<JournalMapNote> GetUnrevealedMapNotesWithinZoneRadiusN(Zone z, int min, int max, Predicate<Location2D> isValid = null)
	{
		return GetMapNotes((JournalMapNote note) =>
		{
			if (note.Revealed)
			{
				return false;
			}
			if (Math.Abs(z.ResolvedX - note.ResolvedX) > max)
			{
				return false;
			}
			if (Math.Abs(z.ResolvedY - note.ResolvedY) > max)
			{
				return false;
			}
			if (Math.Abs(z.ResolvedX - note.ResolvedX) < min)
			{
				return false;
			}
			if (Math.Abs(z.ResolvedY - note.ResolvedY) < min)
			{
				return false;
			}
			if (Math.Abs(z.Z - note.ZoneZ) > 0)
			{
				return false;
			}
			return isValid == null || isValid(note.ResolvedLocation);
		});
	}

	public static List<JournalMapNote> GetMapNotesWithinRadiusN(Zone z, int radius)
	{
		return GetMapNotes((JournalMapNote note) =>
		{
			if (string.Equals(note.Category, "Miscellaneous"))
			{
				return false;
			}
			if (Math.Abs(z.wX - note.ParasangX) > radius)
			{
				return false;
			}
			if (Math.Abs(z.wY - note.ParasangY) > radius)
			{
				return false;
			}
			return (Math.Abs(z.Z - note.ZoneZ) <= radius) ? true : false;
		});
	}

	public static JournalMapNote GetLandmarkNearestPlayer()
	{
		return GetLandmarkNearest(The.Player.CurrentZone);
	}

	public static JournalMapNote GetLandmarkNearest(Zone Zone)
	{
		JournalMapNote result = MapNotes.GetRandomElement();
		int num = int.MaxValue;
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.Revealed && !(mapNote.Category == "Miscellaneous"))
			{
				int num2 = Math.Abs(Zone.wX - mapNote.ParasangX) * 3 + Math.Abs(Zone.X - mapNote.ZoneX);
				int num3 = Math.Abs(Zone.wY - mapNote.ParasangY) * 3 + Math.Abs(Zone.Y - mapNote.ZoneY);
				int num4 = Math.Abs(Zone.Z - mapNote.ZoneZ);
				int num5 = num2 + num3 + num4;
				if (num5 < num)
				{
					num = num5;
					result = mapNote;
				}
			}
		}
		return result;
	}

	public static List<JournalRecipeNote> GetRecipes(Func<JournalRecipeNote, bool> compare = null)
	{
		List<JournalRecipeNote> list = new List<JournalRecipeNote>();
		foreach (JournalRecipeNote recipeNote in RecipeNotes)
		{
			if (compare == null || compare(recipeNote))
			{
				list.Add(recipeNote);
			}
		}
		return list;
	}

	public static bool HasObservation(string id)
	{
		foreach (JournalObservation observation in Observations)
		{
			if (observation.ID == id && observation.Revealed)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasObservationWithTag(string Tag)
	{
		return GetObservations((JournalObservation note) => note.Revealed && note.Has(Tag)).Count > 0;
	}

	public static bool HasSultanNoteWithTag(string Tag)
	{
		return GetSultanNotes((JournalSultanNote note) => note.Revealed && note.Has(Tag)).Count > 0;
	}

	public static JournalSultanNote GetFirstSultanNoteWithTag(string Tag)
	{
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.Has(Tag))
			{
				return sultanNote;
			}
		}
		return null;
	}

	public static JournalVillageNote AddVillageNote(string Text, string VillageID, long? EventID = null, object Attributes = null)
	{
		return AddVillageNote(Guid.NewGuid().ToString(), Text, VillageID, EventID, Attributes);
	}

	public static JournalVillageNote AddVillageNote(string ID, string Text, string VillageID, long? EventID = null, object Attributes = null)
	{
		JournalVillageNote journalVillageNote = new JournalVillageNote();
		journalVillageNote.ID = ID;
		journalVillageNote.Text = Text;
		journalVillageNote.VillageID = VillageID;
		if (EventID.HasValue)
		{
			journalVillageNote.EventID = EventID.Value;
		}
		if (Attributes is string item)
		{
			journalVillageNote.Attributes.Add(item);
		}
		else if (Attributes is IEnumerable<string> collection)
		{
			journalVillageNote.Attributes.AddRange(collection);
		}
		VillageNotes.Add(journalVillageNote);
		AddedNote(journalVillageNote);
		return journalVillageNote;
	}

	public static bool HasVillageNote(string ID)
	{
		foreach (JournalVillageNote villageNote in VillageNotes)
		{
			if (villageNote.ID == ID && villageNote.Revealed)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasUnrevealedVillageNote(string ID)
	{
		foreach (JournalVillageNote villageNote in VillageNotes)
		{
			if (villageNote.ID == ID && !villageNote.Revealed)
			{
				return true;
			}
		}
		return false;
	}

	public static void DeleteMapNote(JournalMapNote note)
	{
		_mapNoteCategories = null;
		MapNotes.Remove(note);
		if (note.ZoneID != null && mapNotesByZone.TryGetValue(note.ZoneID, out var value))
		{
			value.Remove(note);
		}
	}

	public static IBaseJournalEntry GetRandomRevealedNote(Predicate<IBaseJournalEntry> filter = null)
	{
		List<IBaseJournalEntry> list = new List<IBaseJournalEntry>();
		list.AddRange((from c in ((IEnumerable<JournalAccomplishment>)Accomplishments).Select((Func<JournalAccomplishment, IBaseJournalEntry>)((JournalAccomplishment c) => c))
			where c.Revealed && (filter == null || filter(c))
			select c).ToList());
		list.AddRange((from c in ((IEnumerable<JournalObservation>)Observations).Select((Func<JournalObservation, IBaseJournalEntry>)((JournalObservation c) => c))
			where c.Revealed && (filter == null || filter(c))
			select c).ToList());
		list.AddRange((from c in ((IEnumerable<JournalSultanNote>)GetKnownSultanNotes()).Select((Func<JournalSultanNote, IBaseJournalEntry>)((JournalSultanNote c) => c))
			where c.Revealed && (filter == null || filter(c))
			select c).ToList());
		list.AddRange((from c in ((IEnumerable<JournalMapNote>)MapNotes).Select((Func<JournalMapNote, IBaseJournalEntry>)((JournalMapNote c) => c))
			where c.Revealed && (filter == null || filter(c))
			select c).ToList());
		list.AddRange((from c in ((IEnumerable<JournalRecipeNote>)RecipeNotes).Select((Func<JournalRecipeNote, IBaseJournalEntry>)((JournalRecipeNote c) => c))
			where c.Revealed && (filter == null || filter(c))
			select c).ToList());
		return list.GetRandomElement();
	}

	public static IBaseJournalEntry GetRandomUnrevealedNote(Predicate<IBaseJournalEntry> Filter = null)
	{
		return GetUnrevealedNotes(Filter).GetRandomElement();
	}

	public static List<IBaseJournalEntry> GetUnrevealedNotes(Predicate<IBaseJournalEntry> Filter = null)
	{
		List<IBaseJournalEntry> list = new List<IBaseJournalEntry>();
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (!sultanNote.Revealed && (Filter == null || Filter(sultanNote)))
			{
				list.Add(sultanNote);
			}
		}
		foreach (JournalObservation observation in Observations)
		{
			if (!observation.Revealed && (Filter == null || Filter(observation)))
			{
				list.Add(observation);
			}
		}
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (!mapNote.Revealed && (Filter == null || Filter(mapNote)))
			{
				list.Add(mapNote);
			}
		}
		return list;
	}

	public static void RevealRandomSecret()
	{
		GetRandomUnrevealedNote().Reveal();
	}

	public static void RevealMapNote(JournalMapNote note, bool silent = false, string LearnedFrom = null)
	{
		note.Reveal(LearnedFrom, silent);
	}

	public static void AddMapNote(JournalMapNote newNote)
	{
		if (!MapNotes.Contains(newNote))
		{
			MapNotes.Add(newNote);
			AddedNote(newNote);
			if (!mapNotesByZone.TryGetValue(newNote.ZoneID, out var value))
			{
				value = (mapNotesByZone[newNote.ZoneID] = new List<JournalMapNote>());
			}
			if (!value.Contains(newNote))
			{
				value.Add(newNote);
			}
		}
	}

	public static void AddMapNote(string ZoneID, string text, string category = "general", string[] attributes = null, string secretId = null, bool revealed = false, bool sold = false, long time = -1L, bool silent = false)
	{
		if (!ZoneID.Contains("."))
		{
			int x = The.Player.Physics.CurrentCell.X;
			int y = The.Player.Physics.CurrentCell.Y;
			string stringGameState = XRLCore.Core.Game.GetStringGameState("LastLocationOnSurface");
			ZoneID = ZoneID + "." + x + "." + y + ".2.2.10";
			if (stringGameState.Contains("."))
			{
				string[] array = stringGameState.Split('.');
				if (Convert.ToInt16(array[1]) == x && Convert.ToInt16(array[2]) == y)
				{
					ZoneID = stringGameState.Split('@')[0];
				}
			}
		}
		_mapNoteCategories = null;
		JournalMapNote journalMapNote = new JournalMapNote();
		if (time == -1)
		{
			journalMapNote.Time = XRLCore.Core.Game.TimeTicks;
		}
		else
		{
			journalMapNote.Time = time;
		}
		journalMapNote.Tradable = !sold;
		journalMapNote.Text = text;
		journalMapNote.ZoneID = ZoneID;
		journalMapNote.Category = category;
		journalMapNote.ID = secretId;
		if (attributes != null)
		{
			journalMapNote.Attributes.AddRange(attributes);
		}
		if (!mapNotesByZone.TryGetValue(journalMapNote.ZoneID, out var value))
		{
			value = (mapNotesByZone[journalMapNote.ZoneID] = new List<JournalMapNote>());
		}
		value.Add(journalMapNote);
		MapNotes.Add(journalMapNote);
		AddedNote(journalMapNote);
		if (sorting)
		{
			journalMapNote.Forget(fast: true);
			MapNotes.Sort((JournalMapNote a, JournalMapNote b) => a.Text.CompareTo(b.Text));
			if (revealed)
			{
				RevealMapNote(journalMapNote, silent, "Exploration");
			}
		}
		else if (revealed)
		{
			journalMapNote.Reveal("Exploration", silent);
		}
		else
		{
			journalMapNote.Forget(fast: true);
		}
	}

	public static List<JournalMapNote> GetMapNotesForColumn(string world, int wx, int wy)
	{
		List<JournalMapNote> list = new List<JournalMapNote>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.WorldID == world && mapNote.ParasangX == wx && mapNote.ParasangY == wy)
			{
				list.Add(mapNote);
			}
		}
		return list;
	}

	public static JournalRecipeNote AddRecipeNote(CookingRecipe recipe, GameObject Chef = null, bool revealed = true, bool silent = false, string id = null)
	{
		if (id == null)
		{
			id = Guid.NewGuid().ToString();
		}
		JournalRecipeNote journalRecipeNote = new JournalRecipeNote();
		journalRecipeNote.Recipe = recipe;
		journalRecipeNote.Text = recipe.GetDisplayName() + "\n" + recipe.GetIngredients() + "\n\n" + recipe.GetDescription();
		journalRecipeNote.ID = id;
		journalRecipeNote.Attributes.Add("recipe");
		journalRecipeNote.Attributes.Add(id);
		if (Chef != null)
		{
			if (Chef.IsPlayer())
			{
				journalRecipeNote.Attributes.Add("chef:player");
			}
			else
			{
				journalRecipeNote.Attributes.Add("chef:" + Chef.BaseID);
			}
		}
		RecipeNotes.Add(journalRecipeNote);
		AddedNote(journalRecipeNote);
		if (sorting)
		{
			RecipeNotes.Sort((JournalRecipeNote a, JournalRecipeNote b) => a.Text.CompareTo(b.Text));
		}
		if (revealed)
		{
			journalRecipeNote.Reveal(null, silent);
		}
		return journalRecipeNote;
	}

	public static void DeleteRecipeNote(JournalRecipeNote note)
	{
		RecipeNotes.Remove(note);
	}

	public static JournalGeneralNote AddGeneralNote(string text, string secretId = null, long time = -1L, bool revealed = true)
	{
		JournalGeneralNote journalGeneralNote = new JournalGeneralNote();
		journalGeneralNote.Text = text;
		journalGeneralNote.ID = secretId;
		journalGeneralNote.Tradable = false;
		if (time == -1)
		{
			journalGeneralNote.Time = XRLCore.Core.Game.TimeTicks;
		}
		else
		{
			journalGeneralNote.Time = time;
		}
		GeneralNotes.Add(journalGeneralNote);
		AddedNote(journalGeneralNote);
		if (sorting)
		{
			GeneralNotes.Sort((JournalGeneralNote a, JournalGeneralNote b) => a.Time.CompareTo(b.Time));
		}
		if (revealed)
		{
			journalGeneralNote.Reveal();
		}
		return journalGeneralNote;
	}

	public static void SuspendSorting()
	{
		sorting = false;
	}

	public static void ResumeSorting()
	{
		sorting = true;
		MapNotes.Sort((JournalMapNote a, JournalMapNote b) => a.Text.CompareTo(b.Text));
		Accomplishments.Sort((JournalAccomplishment a, JournalAccomplishment b) => a.Time.CompareTo(b.Time));
		Observations.Sort((JournalObservation a, JournalObservation b) => a.Time.CompareTo(b.Time));
		GeneralNotes.Sort((JournalGeneralNote a, JournalGeneralNote b) => a.Time.CompareTo(b.Time));
		RecipeNotes.Sort((JournalRecipeNote a, JournalRecipeNote b) => a.Text.CompareTo(b.Text));
	}

	public static string FormatSecretID(string Text)
	{
		int length = Text.Length;
		int length2 = 1;
		Span<char> span = stackalloc char[length + 1];
		span[0] = '$';
		for (int i = 0; i < length; i++)
		{
			char c = Text[i];
			if (char.IsLetterOrDigit(c))
			{
				span[length2++] = char.ToLowerInvariant(c);
			}
		}
		return new string(span.Slice(0, length2));
	}

	[WishCommand(null, null, Regex = "^reveal\\s*settlements?$")]
	public static bool HandleRevealSettlementsWish(Match match)
	{
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (!mapNote.Revealed && mapNote.Category == "Settlements" && mapNote.Has("villages"))
			{
				RevealMapNote(mapNote);
			}
		}
		return true;
	}

	[WishCommand("gospelme", null)]
	public static void WishGospelAccomplishments()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		foreach (JournalAccomplishment accomplishment in Accomplishments)
		{
			stringBuilder.Compound("{{K|", "\n\n {{K|---}}\n").Append(accomplishment.Text).Append("}}\n");
			stringBuilder.Append(accomplishment.GospelText.IsNullOrEmpty() ? "{{W|[MISSING GOSPEL]}}" : accomplishment.GospelText);
		}
		Popup.Show(stringBuilder.StartReplace().AddObject(The.Player).ToString(), "Gospel", "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
	}
}
