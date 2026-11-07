using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.World;

namespace XRL.UI;

public class JournalScreen : IScreen
{
	[Serializable]
	public class JournalEntry
	{
		public IBaseJournalEntry baseEntry;

		public int entryAPIPosition;

		public string entry;

		public int topLine;

		public int lines;
	}

	private class CursorMemory
	{
		public int Position;

		public int Cursor;
	}

	private class TabMemory : CursorMemory
	{
		public static TabMemory[] Stock;

		public string Selected;

		public Dictionary<string, CursorMemory> Categories = new Dictionary<string, CursorMemory>();

		public void Memorize(string Category, int Position, int Cursor)
		{
			CursorMemory value = this;
			if (Category != null && !Categories.TryGetValue(Category, out value))
			{
				value = (Categories[Category] = new CursorMemory());
			}
			value.Position = Position;
			value.Cursor = Cursor;
		}

		public void Recall(string Category, out int Position, out int Cursor)
		{
			CursorMemory value;
			if (Category == null)
			{
				Position = base.Position;
				Cursor = base.Cursor;
			}
			else if (Categories.TryGetValue(Category, out value))
			{
				Position = value.Position;
				Cursor = value.Cursor;
			}
			else
			{
				Position = 0;
				Cursor = 0;
			}
		}
	}

	public static readonly string STR_LOCATIONS = "Locations";

	public static readonly string STR_CHRONOLOGY = "Chronology";

	public static readonly string STR_OBSERVATIONS = "Gossip and Lore";

	public static readonly string STR_SULTANS = "Sultan Histories";

	public static readonly string STR_VILLAGES = "Village Histories";

	public static readonly string STR_GENERAL = "General Notes";

	public static readonly string STR_RECIPES = "Recipes";

	private static string SultanTabName = STR_SULTANS;

	private static string SultanTerm = "sultan";

	private List<string> categories = new List<string>();

	private List<JournalEntry> entries = new List<JournalEntry>();

	private List<string> displayLines = new List<string>();

	private List<int> entryForDisplayLine = new List<int>();

	private int cursorPosition;

	private int currentTopLine;

	public string selectedCategory;

	private static int LastHash;

	private static List<IBaseJournalEntry> LastRaw = new List<IBaseJournalEntry>();

	public static string ADD_NEW_KEYBIND => ControlManager.getCommandInputDescription("CmdInsert");

	public static string GetTabDisplayName(string Tab)
	{
		if (Tab == STR_SULTANS)
		{
			return GetSultansDisplayName();
		}
		return Tab;
	}

	public static string GetSultansDisplayName()
	{
		string sultanTerm = HistoryAPI.GetSultanTerm();
		if (sultanTerm.IsNullOrEmpty())
		{
			return STR_SULTANS;
		}
		if (sultanTerm != SultanTerm)
		{
			Span<char> span = stackalloc char[sultanTerm.Length + 10];
			sultanTerm.CopyTo(0, span, 0, sultanTerm.Length);
			span[0] = char.ToUpper(span[0]);
			STR_SULTANS.CopyTo(6, span, sultanTerm.Length, 10);
			SultanTabName = new string(span);
			SultanTerm = sultanTerm;
		}
		return SultanTabName;
	}

	public void Memorize(int Tab, bool Tabinate = false)
	{
		TabMemory[] stock = TabMemory.Stock;
		TabMemory tabMemory = stock[Tab] ?? (stock[Tab] = new TabMemory());
		tabMemory.Memorize(selectedCategory, currentTopLine, cursorPosition);
		if (Tabinate)
		{
			tabMemory.Selected = selectedCategory;
		}
	}

	public void Recall(int Tab, bool Tabinate = false)
	{
		TabMemory[] stock = TabMemory.Stock;
		TabMemory tabMemory = stock[Tab] ?? (stock[Tab] = new TabMemory());
		if (Tabinate)
		{
			selectedCategory = tabMemory.Selected;
		}
		tabMemory.Recall(selectedCategory, out currentTopLine, out cursorPosition);
	}

	public static List<IBaseJournalEntry> GetRawEntriesFor(string Tab, string Category = null)
	{
		int num = Tab.GetHashCode() ^ JournalAPI.Count;
		if (Category != null)
		{
			num ^= Category.GetHashCode();
		}
		if (LastHash == num)
		{
			return LastRaw;
		}
		LastHash = num;
		LastRaw.Clear();
		if (Tab == STR_CHRONOLOGY)
		{
			LastRaw.AddRange(JournalAPI.Accomplishments.Where((JournalAccomplishment c) => c.Revealed));
		}
		else if (Tab == STR_OBSERVATIONS)
		{
			LastRaw.AddRange(JournalAPI.Observations.Where((JournalObservation c) => c.Revealed));
		}
		else if (Tab == STR_SULTANS)
		{
			LastRaw.AddRange(from c in JournalAPI.SultanNotes
				where c.Revealed && (c.SultanID == Category || Category == null)
				orderby c.EventID, c.Has("sultan") descending
				select c);
		}
		else if (Tab == STR_VILLAGES)
		{
			LastRaw.AddRange(JournalAPI.VillageNotes.Where((JournalVillageNote c) => c.Revealed && (c.VillageID == Category || Category == null)));
		}
		else if (Tab == STR_LOCATIONS)
		{
			LastRaw.AddRange(from x in JournalAPI.MapNotes
				where x.Revealed && (x.Category == Category || Category == null)
				orderby x.Tracked descending, x.Visited, x.Text, x.Time
				select x);
		}
		else if (Tab == STR_GENERAL)
		{
			LastRaw.AddRange(JournalAPI.GeneralNotes.Where((JournalGeneralNote x) => x.Revealed));
		}
		else if (Tab == STR_RECIPES)
		{
			LastRaw.AddRange(JournalAPI.RecipeNotes.Where((JournalRecipeNote x) => x.Revealed));
		}
		return LastRaw;
	}

	public static void ResetRawHash()
	{
		LastHash = 0;
	}

	public void UpdateEntries(string selectedTab, GameObject GO)
	{
		displayLines.Clear();
		entries.Clear();
		entryForDisplayLine.Clear();
		categories.Clear();
		if (selectedCategory == null)
		{
			if (selectedTab == STR_VILLAGES)
			{
				if (JournalAPI.GetKnownNotesForVillage("Joppa").Count > 0)
				{
					displayLines.Add("Joppa");
					categories.Add("Joppa");
				}
				if (JournalAPI.GetKnownNotesForVillage("Kyakukya").Count > 0)
				{
					displayLines.Add("Kyakukya");
					categories.Add("Kyakukya");
				}
				if (JournalAPI.GetKnownNotesForVillage("The Yd Freehold").Count > 0)
				{
					displayLines.Add("The Yd Freehold");
					categories.Add("The Yd Freehold");
				}
				foreach (HistoricEntity knownVillage in HistoryAPI.GetKnownVillages())
				{
					displayLines.Add(knownVillage.GetCurrentSnapshot().GetProperty("name", knownVillage.id));
					categories.Add(knownVillage.id);
				}
				if (displayLines.Count == 0)
				{
					displayLines.Add("{{K|You have no knowledge of any villages.}}");
				}
				return;
			}
			if (selectedTab == STR_SULTANS)
			{
				foreach (HistoricEntity knownSultan in HistoryAPI.GetKnownSultans())
				{
					displayLines.Add(knownSultan.GetCurrentSnapshot().GetProperty("name", knownSultan.id));
					categories.Add(knownSultan.id);
				}
				if (displayLines.Count == 0)
				{
					displayLines.Add("{{K|You have no knowledge of the sultans.}}");
				}
				return;
			}
			if (selectedTab == STR_LOCATIONS)
			{
				foreach (string mapNoteCategory in JournalAPI.GetMapNoteCategories())
				{
					string item = (JournalAPI.GetCategoryMapNoteToggle(mapNoteCategory) ? "[{{G|X}}] " : "[ ] ") + mapNoteCategory;
					displayLines.Add(item);
					categories.Add(mapNoteCategory);
				}
				if (displayLines.Count == 0)
				{
					displayLines.Add("{{K|You have no map notes.}}");
				}
				return;
			}
		}
		int num = 0;
		foreach (IBaseJournalEntry item2 in GetRawEntriesFor(selectedTab, selectedCategory))
		{
			JournalEntry journalEntry = new JournalEntry();
			journalEntry.entry = item2.GetDisplayText();
			if (Options.DebugInternals && selectedTab == STR_CHRONOLOGY && item2 is JournalAccomplishment)
			{
				string muralText = (item2 as JournalAccomplishment).MuralText;
				journalEntry.entry = journalEntry.entry + "\n\n{{internals|Ã " + muralText.Replace("=name=", GO.BaseDisplayNameStripped) + "}}";
			}
			journalEntry.baseEntry = item2;
			journalEntry.entryAPIPosition = num;
			entries.Add(journalEntry);
			num++;
		}
		int num2 = 0;
		if (selectedTab == STR_SULTANS && selectedCategory != null)
		{
			displayLines.Add("[History of " + HistoryAPI.GetEntityName(selectedCategory) + "]");
			displayLines.Add("");
			entryForDisplayLine.Add(0);
			entryForDisplayLine.Add(0);
			num2 += 2;
		}
		if (selectedTab == STR_VILLAGES && selectedCategory != null)
		{
			displayLines.Add("[History of " + HistoryAPI.GetEntityName(selectedCategory) + "]");
			displayLines.Add("");
			entryForDisplayLine.Add(0);
			entryForDisplayLine.Add(0);
			num2 += 2;
		}
		int maxWidth = 75;
		StringBuilder stringBuilder = Event.NewStringBuilder();
		for (int i = 0; i < entries.Count; i++)
		{
			int MaxClippedWidth = 0;
			List<string> list;
			if (selectedTab == STR_CHRONOLOGY)
			{
				JournalAccomplishment journalAccomplishment = (JournalAccomplishment)entries[i].baseEntry;
				stringBuilder.Clear().Append(entries[i].entry);
				if (journalAccomplishment.Category == "player")
				{
					stringBuilder.Insert(0, "@ ");
				}
				else
				{
					stringBuilder.Insert(0, journalAccomplishment.Tradable ? "{{G|$}} " : "{{K|$}} ");
				}
				list = StringFormat.ClipTextToArray(stringBuilder.ToString(), maxWidth, out MaxClippedWidth, KeepNewlines: true);
			}
			else if (selectedTab == STR_LOCATIONS)
			{
				JournalMapNote journalMapNote = (JournalMapNote)entries[i].baseEntry;
				stringBuilder.Clear().Append(journalMapNote.Tracked ? "[{{G|X}}] " : "[ ] ").Append(Grammar.InitCapWithFormatting(entries[i].entry));
				if (journalMapNote.Category == "player")
				{
					stringBuilder.Insert(0, "@ ");
				}
				else
				{
					stringBuilder.Insert(0, journalMapNote.Visited ? "{{K|?}} " : "{{G|?}} ");
					stringBuilder.Insert(0, journalMapNote.Tradable ? "{{G|$}} " : "{{K|$}} ");
				}
				list = StringFormat.ClipTextToArray(stringBuilder.ToString(), maxWidth, out MaxClippedWidth, KeepNewlines: true);
			}
			else
			{
				stringBuilder.Clear().Append("{{").Append(entries[i].baseEntry.Tradable ? 'G' : 'K')
					.Append("|$}} ");
				bool flag = entries[i].baseEntry.Has("sultanTombPropaganda");
				if (flag)
				{
					stringBuilder.Append("{{w|[tomb engraving] ");
				}
				stringBuilder.Append(entries[i].entry);
				if (flag)
				{
					stringBuilder.Append("}}");
				}
				list = StringFormat.ClipTextToArray(stringBuilder.ToString(), maxWidth, out MaxClippedWidth, KeepNewlines: true);
			}
			entries[i].topLine = num2;
			entries[i].lines = list.Count;
			displayLines.AddRange(list);
			if (i < entries.Count - 1)
			{
				entries[i].lines++;
				displayLines.Add("");
			}
			for (int j = 0; j < list.Count + 1; j++)
			{
				entryForDisplayLine.Add(i);
			}
			num2 += entries[i].lines;
		}
		if (displayLines.Count == 0)
		{
			if (selectedTab == STR_CHRONOLOGY)
			{
				displayLines.Add("{{K|You have no history. That's pretty weird to be honest.}}");
			}
			if (selectedTab == STR_OBSERVATIONS)
			{
				displayLines.Add("{{K|You have made no observations.}}");
			}
			if (selectedTab == STR_LOCATIONS)
			{
				displayLines.Add("{{K|You have made no map notes. Hit " + ADD_NEW_KEYBIND + " to add a new one.}}");
			}
			if (selectedTab == STR_GENERAL)
			{
				displayLines.Add("{{K|You have made no general notes. Hit " + ADD_NEW_KEYBIND + " to add a new one.}}");
			}
			if (selectedTab == STR_RECIPES)
			{
				displayLines.Add("{{K|You have learned no recipes.}}");
			}
		}
	}

	public static bool HandleInsert(string selectedTab, GameObject GO)
	{
		if (selectedTab == STR_CHRONOLOGY || selectedTab == STR_GENERAL || selectedTab == STR_LOCATIONS)
		{
			string text = Popup.AskString("Entry text", "", "Sounds/UI/ui_notification", null, null, 2147483646);
			if (!string.IsNullOrEmpty(text))
			{
				if (selectedTab == STR_CHRONOLOGY)
				{
					JournalAPI.AddAccomplishment(text, null, null, null, "player", MuralCategory.Generic, MuralWeight.Nil, null, -1L);
					return true;
				}
				if (selectedTab == STR_GENERAL)
				{
					JournalAPI.AddGeneralNote(text, null, -1L);
					return true;
				}
				if (selectedTab == STR_LOCATIONS)
				{
					JournalAPI.AddMapNote(GO.CurrentZone.ZoneID, text, "Miscellaneous", null, null, revealed: true, sold: true, -1L);
					return true;
				}
			}
		}
		return false;
	}

	public static bool HandleDelete(string selectedTab, IBaseJournalEntry entry, GameObject GO)
	{
		if (selectedTab == STR_CHRONOLOGY)
		{
			if ((entry as JournalAccomplishment).Category == "player")
			{
				if (Popup.ShowYesNo("Are you sure you want to delete this entry?") == DialogResult.Yes)
				{
					JournalAPI.DeleteAccomplishment(entry as JournalAccomplishment);
					return true;
				}
			}
			else
			{
				Popup.Show("You can't delete automatically recorded chronology entries.");
			}
		}
		if (selectedTab == STR_GENERAL && Popup.ShowYesNo("Are you sure you want to delete this entry?") == DialogResult.Yes)
		{
			JournalAPI.DeleteGeneralNote(entry as JournalGeneralNote);
			return true;
		}
		if (selectedTab == STR_LOCATIONS && Popup.ShowYesNo("Are you sure you want to delete this entry?") == DialogResult.Yes)
		{
			JournalAPI.DeleteMapNote(entry as JournalMapNote);
			return true;
		}
		if (selectedTab == STR_RECIPES)
		{
			JournalRecipeNote journalRecipeNote = entry as JournalRecipeNote;
			if (Popup.ShowYesNo("Are you sure you want to delete {{y|" + journalRecipeNote.Recipe.DisplayName + "}}?") == DialogResult.Yes)
			{
				JournalAPI.DeleteRecipeNote(entry as JournalRecipeNote);
				return true;
			}
		}
		return false;
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Journal");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		Keys keys = Keys.None;
		bool flag = false;
		string[] array = new string[7] { STR_LOCATIONS, STR_OBSERVATIONS, STR_SULTANS, STR_VILLAGES, STR_CHRONOLOGY, STR_GENERAL, STR_RECIPES };
		TabMemory.Stock = new TabMemory[array.Length];
		int num = 0;
		cursorPosition = 0;
		currentTopLine = 0;
		selectedCategory = null;
		UpdateEntries(array[0], GO);
		string text = "< {{W|7}} Quests | Tinkering {{W|9}} >";
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			text = "< {{W|" + ControlManager.getCommandInputDescription("Page Left", mapGlyphs: false) + "}} Quests | Tinkering {{W|" + ControlManager.getCommandInputDescription("Page Right", mapGlyphs: false) + "}} >";
		}
		while (!flag)
		{
			Event.ResetPool(resetMinEventPools: false);
			scrapBuffer.Clear();
			scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			scrapBuffer.Goto(35, 0);
			scrapBuffer.Write("[ {{W|Journal}} ]");
			scrapBuffer.Goto(1, 2);
			for (int i = Math.Max(num - 4, 0); i < array.Length && i < Math.Max(num - 4, 0) + 5; i++)
			{
				string tabDisplayName = GetTabDisplayName(array[i]);
				if (num == i)
				{
					scrapBuffer.Write("  ");
					scrapBuffer.Write("{{W|" + tabDisplayName + "}}");
				}
				else
				{
					scrapBuffer.Write("  ");
					scrapBuffer.Write("{{K|" + tabDisplayName + "}}");
				}
			}
			if (num > 4)
			{
				scrapBuffer.Goto(1, 2);
				scrapBuffer.Write("{{G|<<}}");
			}
			if (num <= 4)
			{
				scrapBuffer.Goto(76, 2);
				scrapBuffer.Write("{{G|>>}}");
			}
			scrapBuffer.Goto(60, 0);
			scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Cancel", mapGlyphs: false) + "}} to exit ");
			scrapBuffer.Goto(79 - ColorUtility.StripFormatting(text).Length, 24);
			scrapBuffer.Write(text);
			if (array[num] == STR_RECIPES)
			{
				scrapBuffer.Goto(2, 24);
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("CmdDelete", mapGlyphs: false) + "}} - Delete ");
				}
				else
				{
					scrapBuffer.Write(" {{W|del}} - Delete ");
				}
			}
			if (array[num] == STR_CHRONOLOGY || array[num] == STR_GENERAL || array[num] == STR_LOCATIONS)
			{
				scrapBuffer.Goto(2, 24);
				scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("CmdInsert", mapGlyphs: false) + "}} Add {{W|" + ControlManager.getCommandInputDescription("CmdDelete", mapGlyphs: false) + "}} - Delete ");
				if (array[num] == STR_LOCATIONS)
				{
					scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("V Positive", mapGlyphs: false) + "}} - Rename Location");
				}
			}
			int num2 = 4;
			int num3 = 23;
			int num4 = num3 - num2 + 1;
			int num5 = currentTopLine;
			int num6 = 0;
			for (int j = num2; j <= num3; j++)
			{
				if (num5 >= displayLines.Count)
				{
					break;
				}
				if (j - num2 == cursorPosition)
				{
					scrapBuffer.Goto(2, j);
					scrapBuffer.Write("{{Y|>}}");
				}
				scrapBuffer.Goto(3, j);
				scrapBuffer.Write(displayLines[num5]);
				num5++;
				num6++;
			}
			if (displayLines.Count > num4)
			{
				int num7 = (int)((float)num4 / (float)displayLines.Count * 23f);
				int num8 = (int)((float)currentTopLine / (float)displayLines.Count * 23f);
				scrapBuffer.Fill(79, 1, 79, 23, 177, ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Black), TextColor.Black));
				scrapBuffer.Fill(79, 1 + num8, 79, 1 + num8 + num7, 177, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && Keyboard.RawCode != Keys.Prior && Keyboard.RawCode != Keys.Next))
			{
				flag = true;
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5)
			{
				if ((array[num] == STR_SULTANS || array[num] == STR_VILLAGES || array[num] == STR_LOCATIONS) && selectedCategory != null)
				{
					Memorize(num);
					selectedCategory = null;
					Recall(num);
					UpdateEntries(array[num], GO);
				}
				else
				{
					flag = true;
				}
			}
			if ((keys == Keys.N || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Positive")) && array[num] == STR_LOCATIONS)
			{
				Zone parentZone = GO.CurrentCell.ParentZone;
				if (parentZone.IsWorldMap())
				{
					Popup.Show("You cannot do that on the world map.");
				}
				else if (parentZone.HasProperName && !parentZone.NamedByPlayer)
				{
					Popup.Show("This place already has a name.");
				}
				else
				{
					_ = parentZone.DisplayName;
					string baseDisplayName = parentZone.BaseDisplayName;
					bool namedByPlayer = parentZone.NamedByPlayer;
					string text2 = Popup.AskString(namedByPlayer ? ("Enter a new name for " + baseDisplayName + ".") : "Enter a name for this location.", "", "Sounds/UI/ui_notification", null, null, 40);
					if (!string.IsNullOrEmpty(text2))
					{
						if (namedByPlayer)
						{
							Popup.Show("You stop calling this location '" + baseDisplayName + "' and start calling it '" + text2 + "'.");
							JournalAPI.AddAccomplishment("You stopped calling a location '" + baseDisplayName + "' and start calling it '" + text2 + "'.", "In " + Calendar.GetMonth() + " of " + Calendar.GetYear() + ", =name= commanded " + GO.GetPronounProvider().PossessiveAdjective + " cartographers to change the name of " + Grammar.GetProsaicZoneName(parentZone) + " in the world atlas to " + text2 + ".", "In =year=, =name= won a decisive victory against the combined forces of " + JournalAPI.GetLandmarkNearest(parentZone).Text + " at the bloody Battle of " + baseDisplayName + ". As a result of the battle, " + baseDisplayName + " was so <spice.elements." + The.Player.GetMythicDomain() + ".ruinReason> that it was renamed " + text2 + ".", null, "general", MuralCategory.DoesBureaucracy, MuralWeight.Low, null, -1L);
						}
						else
						{
							Popup.Show("You start calling this location '" + text2 + "'.");
							JournalAPI.AddAccomplishment("You started calling a location '" + text2 + "'.", "In " + Calendar.GetMonth() + " of " + Calendar.GetYear() + ", =name= commanded " + GO.GetPronounProvider().PossessiveAdjective + " cartographers to change the name of " + Grammar.GetProsaicZoneName(parentZone) + " in the world atlas to " + text2 + ".", "In =year=, =name= won a decisive victory against the combined forces of " + JournalAPI.GetLandmarkNearest(parentZone).Text + " at a bloody battle near " + Grammar.GetProsaicZoneName(parentZone) + ". As a result of the battle, " + Grammar.GetProsaicZoneName(parentZone) + " was so <spice.elements." + The.Player.GetMythicDomain() + ".ruinReason> that it was renamed " + text2 + ".", null, "general", MuralCategory.DoesBureaucracy, MuralWeight.Low, null, -1L);
						}
						parentZone.IncludeContextInZoneDisplay = true;
						parentZone.IncludeStratumInZoneDisplay = false;
						parentZone.NamedByPlayer = true;
						parentZone.HasProperName = true;
						parentZone.BaseDisplayName = text2;
						JournalAPI.AddMapNote(parentZone.ZoneID, text2, "Named Locations", null, null, revealed: true, sold: true, -1L);
						UpdateEntries(array[num], GO);
						int num9 = Math.Max(0, displayLines.Count - num4);
						currentTopLine = num9 + 100;
					}
				}
			}
			int num10 = currentTopLine + cursorPosition;
			if (keys == Keys.Tab)
			{
				if (array[num] == STR_LOCATIONS && selectedCategory == null && num10 < categories.Count)
				{
					JournalAPI.SetCategoryMapNoteToggle(categories[num10], !JournalAPI.GetCategoryMapNoteToggle(categories[num10]));
					UpdateEntries(array[num], GO);
				}
				else if (array[num] == STR_LOCATIONS && selectedCategory != null && num10 < entryForDisplayLine.Count)
				{
					(entries[entryForDisplayLine[num10]].baseEntry as JournalMapNote).Tracked = !(entries[entryForDisplayLine[num10]].baseEntry as JournalMapNote).Tracked;
					UpdateEntries(array[num], GO);
				}
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdDelete" && HandleDelete(array[num], entries[entryForDisplayLine[num10]].baseEntry, GO))
			{
				UpdateEntries(array[num], GO);
			}
			if ((keys == Keys.Oemplus || keys == Keys.Add || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdInsert")) && HandleInsert(array[num], GO))
			{
				UpdateEntries(array[num], GO);
				int num11 = Math.Max(0, displayLines.Count - num4);
				currentTopLine = num11 + 100;
			}
			if (keys == Keys.Space || keys == Keys.Enter)
			{
				if ((array[num] == STR_SULTANS || array[num] == STR_VILLAGES || array[num] == STR_LOCATIONS) && selectedCategory == null)
				{
					if (categories.Count > num10)
					{
						Memorize(num);
						selectedCategory = categories[num10];
						Recall(num);
						UpdateEntries(array[num], GO);
					}
				}
				else if (selectedCategory != null && array[num] == STR_LOCATIONS)
				{
					(entries[entryForDisplayLine[num10]].baseEntry as JournalMapNote).Tracked = !(entries[entryForDisplayLine[num10]].baseEntry as JournalMapNote).Tracked;
					UpdateEntries(array[num], GO);
				}
			}
			if (keys == Keys.NumPad8)
			{
				if (cursorPosition == 0)
				{
					if (currentTopLine > 0)
					{
						currentTopLine--;
					}
				}
				else
				{
					cursorPosition--;
				}
			}
			if (keys == Keys.NumPad2)
			{
				if (cursorPosition >= num3 - num2)
				{
					currentTopLine++;
				}
				else
				{
					cursorPosition++;
				}
			}
			switch (keys)
			{
			case Keys.Next:
				currentTopLine += num4;
				cursorPosition += num4;
				break;
			case Keys.Prior:
				currentTopLine -= num4;
				cursorPosition -= num4;
				break;
			case Keys.Home:
				currentTopLine = (cursorPosition = 0);
				break;
			case Keys.End:
				currentTopLine = (cursorPosition = int.MaxValue);
				break;
			}
			int num12 = Math.Max(0, displayLines.Count - num4);
			if (currentTopLine < 0)
			{
				currentTopLine = 0;
			}
			if (currentTopLine > num12)
			{
				currentTopLine = num12;
			}
			if (cursorPosition >= num6)
			{
				cursorPosition = num6 - 1;
			}
			if (cursorPosition < 0)
			{
				cursorPosition = 0;
			}
			if (cursorPosition + currentTopLine > displayLines.Count)
			{
				cursorPosition = displayLines.Count - currentTopLine;
			}
			switch (keys)
			{
			case Keys.NumPad4:
				Memorize(num, Tabinate: true);
				num--;
				if (num < 0)
				{
					num = array.Length - 1;
				}
				selectedCategory = null;
				Recall(num, Tabinate: true);
				UpdateEntries(array[num], GO);
				break;
			case Keys.NumPad6:
				Memorize(num, Tabinate: true);
				num++;
				if (num >= array.Length)
				{
					num = 0;
				}
				selectedCategory = null;
				Recall(num, Tabinate: true);
				UpdateEntries(array[num], GO);
				break;
			}
		}
		TabMemory.Stock = null;
		GameManager.Instance.PopGameView();
		if (keys == Keys.NumPad7 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Left"))
		{
			return ScreenReturn.Previous;
		}
		if (keys == Keys.NumPad9 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Right"))
		{
			return ScreenReturn.Next;
		}
		if (GO.OnWorldMap())
		{
			GO.CurrentZone.Activated();
		}
		return ScreenReturn.Exit;
	}
}
